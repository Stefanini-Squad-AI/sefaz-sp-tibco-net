<#
.SYNOPSIS
    S1.11 - projecta a IR num esqueleto .NET em Clean Architecture.

.DESCRIPTION
    O mapa de camadas e AUTORADO em config/dotnet-architecture.json. Este gerador
    nao decide arquitectura: le o mapa e projecta os artefactos nele.

    A regra da dependencia nao fica escrita num documento - fica gravada nas
    ProjectReference dos .csproj. Domain nao referencia ninguem, Application
    referencia Domain, e por ai fora. Uma violacao deixa de compilar, que e a
    unica forma de uma regra de arquitectura sobreviver a um prazo apertado.

    O que sai daqui e LOSSLESS: entidades, tipos, portas e enumeracoes derivadas
    da IR. Nenhum corpo de metodo e escrito - isso e trabalho da fase 2, e cada
    corpo tem um oraculo que o julga.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$MapPath      = "$PSScriptRoot/../config/dotnet-architecture.json",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$OutDir       = "$PSScriptRoot/../artifacts/POC_Epat/scaffold"
)

$ErrorActionPreference = 'Stop'

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) { if ($Optional) { return $null }; throw "artifact not found: $p" }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

$map      = Get-Content $MapPath -Raw -Encoding UTF8 | ConvertFrom-Json
$fields   = Read-Artifact 'case-field-dictionary.json'
$model    = Read-Artifact 'process-model.json'
$services = Read-Artifact 'service-contracts.json'
$rules    = Read-Artifact 'rule-inventory.json'  -Optional
$screens  = Read-Artifact 'screen-rules.json'    -Optional
$scope    = Read-Artifact 'scope.json'           -Optional
$manifest = Read-Artifact 'manifest.json'

# Campo fora do cenario NAO e removido: e estado que o legado carrega, e cortar dados
# do caso e diferente de cortar trabalho. Fica marcado, para se ver de onde vem o corte.
$outOfScopeField = @{}
foreach ($e in (Arr $scope.elements)) {
    if ($e.kind -eq 'field' -and -not $e.inScope) { $outOfScopeField[$e.id] = $e.reason }
}

# ------------------------------------------------------------------ glossary ----

# term e description humanos viram comentario XML; o nome da propriedade nunca muda.
$term = @{}; $desc = @{}; $vals = @{}
if (Test-Path $GlossaryPath) {
    $section = ''; $entry = ''
    foreach ($line in (Get-Content $GlossaryPath -Encoding UTF8)) {
        if ($line -match '^([a-z]+):\s*$')        { $section = $Matches[1]; continue }
        if ($line -match '^\s{2}"?([^":]+)"?:\s*$') { $entry = $Matches[1]; continue }
        if ($section -notin 'fields', 'unresolved') { continue }
        if ($line -match '^\s{4}term:\s*"(.+)"\s*$')        { $term[$entry] = $Matches[1] }
        elseif ($line -match '^\s{4}description:\s*"(.+)"\s*$') { $desc[$entry] = $Matches[1] }
        elseif ($line -match '^\s{4}values:\s*"(.+)"\s*$')      { $vals[$entry] = $Matches[1] }
    }
}

# ------------------------------------------------------------------ helpers ----

$outRoot = $OutDir
$emitted = [System.Collections.Generic.List[string]]::new()

function Write-File {
    param([string]$RelPath, [string[]]$Lines)
    $full = Join-Path $outRoot $RelPath
    $dir = Split-Path -Parent $full
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    ($Lines -join "`r`n") | Set-Content -LiteralPath $full -Encoding UTF8
    $emitted.Add($RelPath)
}

function Esc-Xml { param([string]$T) return (([string]$T) -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;') }

# Comentario XML a partir do que o humano respondeu; nada e inventado aqui.
function Doc-Comment {
    param([string]$Name, [string]$Fallback = '', [int]$Indent = 4)
    $pad = ' ' * $Indent
    $t = $term[$Name]; $d = $desc[$Name]; $v = $vals[$Name]
    $body = @()
    if ($t) { $body += "$pad/// $(Esc-Xml $t)" }
    elseif ($Fallback) { $body += "$pad/// $(Esc-Xml $Fallback)" }
    else { $body += "$pad/// Identificador do TIBCO. Termo de negocio ainda nao respondido no glossario." }
    if ($d) { $body += "$pad/// $(Esc-Xml $d)" }
    if ($v) { $body += "$pad/// Dominio: $(Esc-Xml $v)" }
    return @("$pad/// <summary>") + $body + @("$pad/// </summary>")
}

function To-Pascal {
    param([string]$Name)
    $parts = ($Name -split '[^A-Za-z0-9]+') | Where-Object { $_ }
    return -join ($parts | ForEach-Object { $_.Substring(0, 1).ToUpperInvariant() + $_.Substring(1).ToLowerInvariant() })
}

# Tipos de referencia sob Nullable=enable precisam de inicializador, senao CS8618
# transforma-se em erro por causa de TreatWarningsAsErrors. O esqueleto tem de compilar.
$RefTypes = @('string')
function Get-Initialiser {
    param([string]$ClrType, [bool]$IsArray, [bool]$IsSentinel, [bool]$IsNullable)
    if ($IsArray) { return ' = [];' }
    if ($IsSentinel) { return '' }
    if ($IsNullable) { return '' }
    if ($ClrType -in $RefTypes) { return ' = default!;' }
    return ''
}

# CS8669: um ficheiro marcado como auto-generated fica FORA do contexto de nullable
# do projecto e exige a directiva explicita. Sem ela o esqueleto nao compila.
$header = @(
    '// <auto-generated>',
    "//   Gerado por tools/gen-scaffold.ps1 a partir do pacote $Package.",
    "//   Fonte fixada por sha256 em artifacts/$Package/manifest.json.",
    '//   NAO EDITAR: uma nova extraccao reescreve este ficheiro.',
    '// </auto-generated>',
    '#nullable enable',
    ''
)

# ------------------------------------------------------------ csproj + sln ----

$tfm = 'net8.0'
$projByName = @{}
foreach ($l in $map.layers) { $projByName[$l.name] = $l.project }

foreach ($l in $map.layers) {
    $refs = @(foreach ($d in (Arr $l.dependsOn)) {
        "    <ProjectReference Include=`"..\$($projByName[$d])\$($projByName[$d]).csproj`" />"
    })
    $body = @(
        '<Project Sdk="Microsoft.NET.Sdk">',
        '',
        '  <PropertyGroup>',
        "    <TargetFramework>$tfm</TargetFramework>",
        '    <Nullable>enable</Nullable>',
        '    <ImplicitUsings>enable</ImplicitUsings>',
        '    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>',
        "    <RootNamespace>$($l.project)</RootNamespace>",
        '  </PropertyGroup>',
        ''
    )
    if ($refs.Count -gt 0) {
        # A regra da dependencia vive aqui: o que nao esta declarado nao compila.
        $body += @('  <!-- Regra da dependencia de Clean Architecture. Acrescentar referencia', "       fora do mapa de config/dotnet-architecture.json e uma violacao. -->", '  <ItemGroup>') + $refs + @('  </ItemGroup>', '')
    }
    else {
        $body += @('  <!-- Domain nao referencia projecto nenhum, por desenho. -->', '')
    }
    $body += @('</Project>')
    Write-File "src/$($l.project)/$($l.project).csproj" $body
}

foreach ($t in $map.tests) {
    $refs = @(foreach ($d in (Arr $t.dependsOn)) {
        "    <ProjectReference Include=`"..\..\src\$($projByName[$d])\$($projByName[$d]).csproj`" />"
    })
    Write-File "tests/$($t.project)/$($t.project).csproj" @(
        '<Project Sdk="Microsoft.NET.Sdk">',
        '',
        '  <PropertyGroup>',
        "    <TargetFramework>$tfm</TargetFramework>",
        '    <Nullable>enable</Nullable>',
        '    <IsPackable>false</IsPackable>',
        '  </PropertyGroup>',
        '',
        '  <ItemGroup>'
    ) + $refs + @('  </ItemGroup>', '', '</Project>')
}

$slnLines = @('Microsoft Visual Studio Solution File, Format Version 12.00', '# Visual Studio Version 17')
foreach ($l in $map.layers) { $slnLines += "# src/$($l.project)" }
foreach ($t in $map.tests)  { $slnLines += "# tests/$($t.project)" }
$slnLines += @('', "# Gerado por tools/gen-scaffold.ps1. Use 'dotnet sln add' para materializar.", '')
Write-File "$($map.solution).sln.txt" $slnLines

# --------------------------------------------------------- Domain: campos ----

$technical = @{}
foreach ($t in (Arr $fields.technicalFields)) { $technical[$t.name] = $t }

$domainFields = @(Arr $fields.fields | Where-Object { -not $technical.ContainsKey($_.name) })

$dom = $projByName['Domain']
$body = $header + @(
    "using $dom.Abstractions;",
    "using $dom.ValueObjects;",
    '',
    "namespace $dom.Cases;",
    '',
    '/// <summary>',
    '/// O caso do AIIM: o estado de negocio que o processo carrega.',
    "/// $($domainFields.Count) campos de negocio, dos quais $($domainFields.Count - $outOfScopeField.Count) sao tocados pelo cenario da POC.",
    "/// Os outros $($outOfScopeField.Count) ficam, marcados com [OutOfPocScope]: cortar dados do caso e",
    '/// diferente de cortar trabalho, e o legado carrega-os na mesma.',
    "/// Os $($technical.Count) do envelope tecnico ficam de fora por decisao registada no glossario -",
    "/// vivem em $($projByName['Application']).Execution.",
    '/// </summary>',
    'public sealed class AiimCase',
    '{'
)
foreach ($f in ($domainFields | Sort-Object name)) {
    $t = $f.clrType
    if ($f.isArray) { $t = "IReadOnlyList<$t>" }
    elseif ($f.usesSwNaSentinel) { $t = "FieldValue<$t>" }
    elseif ($f.clrNullable) { $t += '?' }
    $init = Get-Initialiser -ClrType $f.clrType -IsArray ([bool]$f.isArray) -IsSentinel ([bool]$f.usesSwNaSentinel) -IsNullable ([bool]$f.clrNullable)
    $body += (Doc-Comment -Name $f.name)
    if ($f.maxLength) { $body += "    // XPDL: comprimento maximo $($f.maxLength)" }
    if ($outOfScopeField.ContainsKey($f.name)) {
        $body += "    [OutOfPocScope(`"$(Esc-Xml $outOfScopeField[$f.name])`")]"
    }
    $body += "    public $t $($f.name) { get; set; }$init"
    $body += ''
}
$body += '}'
Write-File "src/$dom/Cases/AiimCase.cs" $body

# ------------------------------------------------- Domain: tri-estado SW_NA ----

Write-File "src/$dom/ValueObjects/FieldValue.cs" ($header + @(
    "namespace $dom.ValueObjects;",
    '',
    '/// <summary>',
    '/// O sentinela SW_NA do iProcess e um TERCEIRO estado: nao e nulo e nao e vazio.',
    "/// $(@($fields.fields | Where-Object { $_.usesSwNaSentinel }).Count) campos do pacote comparam-se com ele.",
    '/// Colapsar SW_NA em null troca o ramo que dispara, sem erro de compilacao e sem teste vermelho.',
    '/// Decidido em config/glossary: gaps.iprocess-builtin = shim-tri-state.',
    '/// </summary>',
    'public readonly record struct FieldValue<T>',
    '{',
    '    private readonly T? _value;',
    '    private readonly FieldState _state;',
    '',
    '    private FieldValue(T? value, FieldState state) { _value = value; _state = state; }',
    '',
    '    public static FieldValue<T> Of(T value) => new(value, FieldState.HasValue);',
    '',
    '    /// <summary>SW_NA: o motor diz explicitamente que o valor nao esta disponivel.</summary>',
    '    public static FieldValue<T> NotAvailable => new(default, FieldState.IsNotAvailable);',
    '',
    '    /// <summary>Declarado mas nunca preenchido. Distinto de SW_NA.</summary>',
    '    public static FieldValue<T> Empty => new(default, FieldState.Empty);',
    '',
    '    public bool HasValue      => _state == FieldState.HasValue;',
    '    public bool IsNotAvailable => _state == FieldState.IsNotAvailable;',
    '    public bool IsEmpty       => _state == FieldState.Empty;',
    '',
    '    /// <summary>',
    '    /// Forca o chamador a decidir os tres casos. O switch exaustivo do C# e',
    '    /// o que impede que um ramo desapareca em silencio.',
    '    /// </summary>',
    '    public TResult Match<TResult>(',
    '        Func<T, TResult> hasValue,',
    '        Func<TResult> notAvailable,',
    '        Func<TResult> empty) => _state switch',
    '        {',
    '            FieldState.HasValue      => hasValue(_value!),',
    '            FieldState.IsNotAvailable => notAvailable(),',
    '            _                         => empty()',
    '        };',
    '}',
    '',
    'public enum FieldState { Empty = 0, HasValue = 1, IsNotAvailable = 2 }'
))

# O campo continua a existir; o atributo diz que o cenario da POC nao passa por ele.
Write-File "src/$dom/Abstractions/OutOfPocScopeAttribute.cs" ($header + @(
    "namespace $dom.Abstractions;",
    '',
    '/// <summary>',
    '/// Marca estado que o legado carrega mas que o cenario da POC nao exercita.',
    '/// Nao altera comportamento: existe para que a fronteira do escopo seja visivel no codigo',
    '/// e consultavel por ferramenta, em vez de viver so num relatorio.',
    '/// O veredicto e o motivo vem de artifacts/&lt;pacote&gt;/scope.json.',
    '/// </summary>',
    '[AttributeUsage(AttributeTargets.Property)]',
    'public sealed class OutOfPocScopeAttribute(string reason) : Attribute',
    '{',
    '    public string Reason { get; } = reason;',
    '}'
))

Write-File "src/$dom/Abstractions/IClock.cs" ($header + @(
    "namespace $dom.Abstractions;",
    '',
    '/// <summary>',
    '/// O prazo por expressao precisa de tempo, e tempo e dependencia.',
    '/// Nunca DateTime.Now: sem relogio injectado nao ha teste de prazo reproduzivel,',
    '/// e a etapa 4 do plano de cumprimento exige exactamente isso.',
    '/// </summary>',
    'public interface IClock',
    '{',
    '    DateTimeOffset Now { get; }',
    '    TimeZoneInfo TimeZone { get; }',
    '}'
))

# ------------------------------------------------------ Domain: enumeracoes ----

# So o que o pacote observou. Um dominio nao observado nao vira enumeracao.
$enums = [System.Collections.Generic.List[object]]::new()
foreach ($f in (Arr $fields.fields)) {
    $observed = @(Arr $f.usedInConditions | ForEach-Object {
        foreach ($m in [regex]::Matches([string]$_.expression, "$([regex]::Escape($f.name))\s*[=!]=\s*'([^']*)'")) { $m.Groups[1].Value }
    }) | Sort-Object -Unique
    if ($observed.Count -lt 2) { continue }
    if ($f.clrType -ne 'string') { continue }
    $enums.Add([pscustomobject]@{ Field = $f.name; Values = $observed })
}
foreach ($e in $enums) {
    $members = @(foreach ($v in $e.Values) {
        $id = ($v -replace '[^A-Za-z0-9_]', '')
        if ($id -match '^\d') { $id = "V$id" }
        @("    /// <summary>Literal no XPDL: '$(Esc-Xml $v)'</summary>", "    $id,")
    })
    # O identificador do TIBCO e preservado tal e qual: o toolkit transcreve, nao baptiza.
    Write-File "src/$dom/Enums/$($e.Field).cs" ($header + @(
        "namespace $dom.Enums;",
        ''
    ) + (Doc-Comment -Name $e.Field -Fallback "Dominio fechado observado no pacote para $($e.Field)." -Indent 0) + @(
        "public enum $($e.Field)",
        '{'
    ) + $members + @('}'))
}

# ------------------------------------ Application: envelope tecnico + portas ----

$app = $projByName['Application']

$body = $header + @(
    "namespace $app.Execution;",
    '',
    '/// <summary>',
    "/// Os $($technical.Count) campos que NAO sao dominio: envelope de servico, contadores de",
    '/// retentativa e decisao do operador. Ficaram aqui por decisao registada no glossario,',
    '/// seccao unresolved. Nao sao persistidos como dados do caso.',
    '/// </summary>',
    'public sealed class ProcessExecutionContext',
    '{'
)
foreach ($t in ((Arr $fields.technicalFields) | Sort-Object name)) {
    $ct = $t.clrType
    if ($ct -eq 'string') { $ct = 'string?' }
    $body += (Doc-Comment -Name $t.name -Fallback $t.label)
    if ($t.note) { $body += "    // $(Esc-Xml $t.note)" }
    $body += "    public $ct $($t.name) { get; set; }"
    $body += ''
}
$body += '}'
Write-File "src/$app/Execution/ProcessExecutionContext.cs" $body

# Contratos partilhados pelas portas. Ficam na Application porque sao vocabulario
# de fronteira, nao estado de negocio.
Write-File "src/$app/Abstractions/Contracts.cs" ($header + @(
    "namespace $app.Abstractions;",
    '',
    '/// <summary>',
    '/// Identidade do caso atravessando uma fronteira. PROCESS_ID e a chave de correlacao',
    "/// que o legado ja constroi nos scripts, no formato 'idAiim-<n>idProc-<n>' - nao precisa",
    '/// de ser inventada, so transcrita.',
    '/// </summary>',
    'public readonly record struct AiimCaseRef(long IdAiim, string ProcessId);',
    '',
    '/// <summary>Desfecho de uma chamada a subprocesso resolvida em runtime.</summary>',
    'public readonly record struct ProcessCallResult(bool Started, string? ChildInstanceId, string? Failure);',
    '',
    '/// <summary>',
    '/// O envelope tecnico devolvido pelo BusinessWorks, declarado no EPAT.wsdl.',
    "/// STATUS_CODE = '0' e sucesso, confirmado no glossario. Os valores daqui sao copiados",
    '/// para o ProcessExecutionContext num passo explicito de mapeamento.',
    '/// </summary>',
    'public readonly record struct ServiceEnvelope(string? STATUS_CODE, string? STERRORCODE, string? STERRORDESC);'
))

# Uma interface por xpdExt:ProcessInterface, lida do modelo. Ler o XPDL aqui daria
# uma segunda leitura do mesmo facto, com tratamento proprio de cardinalidade e de
# falha - e duas leituras independentes divergem sem ninguem dar por isso.
$ifaces = @(foreach ($pi in (Arr $model.processInterfaces)) {
    [pscustomobject]@{ Name = $pi.name; Id = $pi.id; Impl = @(Arr $pi.implementedBy) }
})
foreach ($i in $ifaces) {
    $implText = $(if ($i.Impl.Count) { $i.Impl -join ', ' } else { 'NENHUMA entregue - ver rulings.MISSING-EXTERNAL-PACKAGES' })
    Write-File "src/$app/Abstractions/Processes/I$($i.Name).cs" ($header + @(
        "using $app.Abstractions;",
        '',
        "namespace $app.Abstractions.Processes;",
        '',
        '/// <summary>',
        "/// Traducao directa do xpdExt:ProcessInterface '$($i.Name)' do XPDL.",
        "/// Implementacoes entregues no pacote: $(Esc-Xml $implText).",
        '/// O destino da chamada dinamica e resolvido em runtime, mas o CONJUNTO de destinos',
        '/// e derivado do XPDL e validado no arranque - gaps.dynamic-subprocess = interface-registry-validated.',
        '/// Um destino sem implementacao quebra o teste de registo, nao a producao.',
        '/// </summary>',
        "public interface I$($i.Name)",
        '{',
        '    Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct);',
        '}'
    ))
}

# Uma porta por operacao realmente invocada pelo processo.
$invoked = @(Arr $services.invokedOperations)
$opIndex = @{}
foreach ($svc in (Arr $services.services)) {
    foreach ($op in (Arr $svc.operations)) { $opIndex[$op.name] = [pscustomobject]@{ Op = $op; File = $svc.file } }
}
$portLines = $header + @(
    "using $app.Abstractions;",
    '',
    "namespace $app.Abstractions.Services;",
    '',
    '/// <summary>',
    "/// As $($invoked.Count) operacoes que o processo realmente invoca, de $(@($services.services.operations).Count) catalogadas.",
    '/// Sao PORTAS: a implementacao SOAP e os dubles vivem na infraestrutura.',
    '/// </summary>',
    'public interface IEpatServices',
    '{'
)
foreach ($name in $invoked) {
    $meta = $opIndex[$name]
    $short = To-Pascal (($name -split '_sol_')[-1] -replace '\.\d+$', '')
    $portLines += @(
        '    /// <summary>',
        "    /// Operacao TIBCO: $(Esc-Xml $name)",
        "    /// Declarada em $(if ($meta) { Esc-Xml $meta.File } else { 'WSDL nao resolvido' }).",
        '    /// </summary>',
        "    Task<ServiceEnvelope> ${short}Async(AiimCaseRef caseRef, CancellationToken ct);",
        ''
    )
}
$portLines += '}'
Write-File "src/$app/Abstractions/Services/IEpatServices.cs" $portLines

Write-File "src/$app/Abstractions/Runtime/IGraftJoin.cs" ($header + @(
    "namespace $app.Abstractions.Runtime;",
    '',
    '/// <summary>',
    '/// Graft step: o pai NAO inicia os filhos - espera que se anexem, possivelmente em',
    '/// momentos diferentes, e so prossegue quando todos terminarem. Uma instancia por solidario.',
    '/// gaps.graft-step = correlation-join: o contrato fica do lado do pai e o filho apenas sinaliza,',
    '/// para nao obrigar processos de pacotes externos a registarem-se.',
    '///',
    '/// POR DEFINIR antes de implementar, registado no glossario: a chave de correlacao formal,',
    '/// o criterio de encerramento e o timeout para um filho que nunca termina. No iProcess',
    '/// os tres estao implicitos na identidade do caso.',
    '/// </summary>',
    'public interface IGraftJoin',
    '{',
    '    Task<GraftToken> ParkAsync(string correlationKey, CancellationToken ct);',
    '    Task AttachAsync(string correlationKey, string childInstanceId, CancellationToken ct);',
    '    Task SignalCompletedAsync(string correlationKey, string childInstanceId, CancellationToken ct);',
    '}',
    '',
    'public readonly record struct GraftToken(string CorrelationKey, int AttachedCount);'
))

# ------------------------------------------------- Infrastructure: legado ----

$inf = $projByName['Infrastructure']
$builtins = Read-Artifact 'builtin-contract.json' -Optional
$bl = @()
foreach ($b in (Arr $builtins.builtins | Where-Object { $_.kind -eq 'function' })) {
    $bl += "    // $($b.name) - $($b.callCount) chamada(s), aridade observada $((Arr $b.observedArity) -join '/')"
}
Write-File "src/$inf/Legacy/IProcessBuiltins.cs" ($header + @(
    "namespace $inf.Legacy;",
    '',
    '/// <summary>',
    '/// CAMADA ANTICORRUPCAO. Unico sitio do codigo onde a palavra iProcess aparece.',
    '///',
    '/// BLOQUEADO: rulings.BUILTIN-SEMANTICS decidiu consultar a documentacao TIBCO antes de',
    '/// implementar. SUBSTR e SEARCH sao base 1 ou base 0? O terceiro argumento de SUBSTR e',
    '/// comprimento ou posicao final? Um desvio de uma posicao nao falha - devolve valor errado.',
    '/// O vector de comportamento de builtin-contract.json e o oraculo que qualquer implementacao',
    '/// candidata tem de satisfazer.',
    '/// </summary>',
    'public interface IProcessBuiltins',
    '{'
) + $bl + @('}'))

# ------------------------------------------------------------------ readme ----

$tick = [char]0x60
$layerRows = @(foreach ($l in $map.layers) {
    $deps = @(Arr $l.dependsOn | ForEach-Object { "$tick$_$tick" })
    $depText = $(if ($deps.Count) { $deps -join ', ' } else { 'nenhuma' })
    "| $tick$($l.project)$tick | $depText | $($l.regra) |"
})
Write-File 'README.md' @(
    "# $($map.solution) - esqueleto gerado",
    '',
    "Gerado por ``tools/gen-scaffold.ps1`` a partir de ``artifacts/$Package``.",
    "Fonte fixada por sha256 em ``manifest.json``. Regenerar sobrepoe tudo.",
    '',
    '## Regra da dependencia',
    '',
    '| Projecto | Referencia | Regra |',
    '|---|---|---|'
) + $layerRows + @(
    '',
    $map.porqueEncaixa,
    '',
    '## O que este esqueleto ja traz',
    '',
    '| Ficheiro | Derivado de |',
    '|---|---|',
    "| ``Cases/AiimCase.cs`` | $($domainFields.Count) campos de negocio do dicionario |",
    "| ``ValueObjects/FieldValue.cs`` | os $(@($fields.fields | Where-Object { $_.usesSwNaSentinel }).Count) campos com sentinela SW_NA |",
    "| ``Enums/*.cs`` | $($enums.Count) dominio(s) fechado(s) observado(s) nas condicoes |",
    "| ``Execution/ProcessExecutionContext.cs`` | os $($technical.Count) campos do envelope tecnico |",
    "| ``Abstractions/Processes/*.cs`` | $($ifaces.Count) xpdExt:ProcessInterface do XPDL |",
    "| ``Abstractions/Services/IEpatServices.cs`` | as $($invoked.Count) operacoes invocadas |",
    '',
    '## O que NAO traz, de proposito',
    '',
    'Nenhum corpo de metodo. O que e transcricao sai daqui pronto; o que exige julgamento',
    'sai como assinatura vazia, e cada corpo tem um oraculo que o julga na fase 2.',
    '',
    '## Nomes',
    '',
    $map.namingRule,
    ''
)

# ------------------------------------------------------------------- done ----

Write-Host ("Wrote {0}  ({1} ficheiros; {2} campos de dominio, {3} tecnicos, {4} enum(s), {5} interface(s) de processo, {6} porta(s) de servico)" -f `
    $outRoot, $emitted.Count, $domainFields.Count, $technical.Count, $enums.Count, $ifaces.Count, $invoked.Count)
