<#
.SYNOPSIS
    S2.1 - projecta os artefactos num backlog de cards validos contra o schema.

.DESCRIPTION
    O card e a unidade que atravessa a comporta entre este kit deterministico e a
    fase 2 agentica. O agente que o recebe NAO conhece TIBCO e NAO le os artefactos:
    o que nao vier no card nao existe.

    A UNIDADE DE TRABALHO E O SEGMENTO DA JORNADA, nao o no. Um card por no daria
    61 cards de gateway que nao produzem codigo e obrigaria a inventar 53 oraculos
    a mao; um card por segmento tem oraculo pronto - o troco do cenario - e uma
    etapa exacta, porque o segmento E o troco da etapa. O detalhe fino nao se perde:
    desce para content.checklist, onde fica conferivel em vez de implicito.

    NADA AQUI E CRIATIVO. Card, ordem, dependencia, bloqueador e cobertura sao
    projeccoes de factos ja extraidos. O julgamento entra por uma porta so, e
    rotulada: analysis/backlog-review.json, produzido por um agente, entra como
    PARECER anexo e nunca altera uma contagem, um escopo ou uma etapa.

    A saida e validada contra config/schemas/backlog-card.schema.json. Card que nao
    valida nao e escrito: e reportado como falha do gerador.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$SchemaPath   = "$PSScriptRoot/../config/schemas/backlog-card.schema.json",
    [string]$ConceptsPath = "$PSScriptRoot/../config/poc-concepts.json",
    [string]$MapPath      = "$PSScriptRoot/../config/dotnet-architecture.json",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$ReviewPath   = "$PSScriptRoot/../analysis/backlog-review.json",
    [string]$OutDir       = "$PSScriptRoot/../artifacts/POC_Epat/backlog"
)

$ErrorActionPreference = 'Stop'

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) { if ($Optional) { return $null }; throw "artifact not found: $p" }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }
function Slug { param([string]$s) return (($s -replace '[^A-Za-z0-9]', '').ToUpperInvariant()) }

# Ficou decidido manter o segmento como unidade de entrega mesmo quando atravessa
# camadas. A condicao para isso ser honesto e o tamanho estar declarado - com
# fronteiras fixas, para que a classe nao dependa de quem gera nem de quando.
function Get-Peso {
    param([int]$Camadas, [int]$AEscrever, [int]$Nos)
    if ($Camadas -ge 3 -or $AEscrever -ge 5) { return 'atravessa-o-sistema' }
    if ($Camadas -ge 2 -or $AEscrever -ge 3 -or $Nos -gt 12) { return 'grande' }
    if ($AEscrever -ge 2 -or $Nos -gt 4) { return 'medio' }
    return 'pequeno'
}

$model    = Read-Artifact 'process-model.json'
$scope    = Read-Artifact 'scope.json'
$conf     = Read-Artifact 'conformance.json'
$dossier  = Read-Artifact 'review-dossier.json'
$rules    = Read-Artifact 'rule-catalogue.json'  -Optional
$fields   = Read-Artifact 'case-field-dictionary.json' -Optional
$services = Read-Artifact 'service-contracts.json' -Optional
$decisions = Read-Artifact 'decision-tables.json' -Optional
$index    = Read-Artifact 'scenarios/index.json'
$spec     = Get-Content $ConceptsPath -Raw -Encoding UTF8 | ConvertFrom-Json
$map      = Get-Content $MapPath      -Raw -Encoding UTF8 | ConvertFrom-Json

# Os gateways sem rotulo ja foram nomeados por um humano, na seccao decisions do
# glossario. Isso e resposta ratificada, nao julgamento: entra por leitura directa
# e nao pelo parecer. Sem esta ligacao, 42 passos chegavam a fase 2 como
# 'gateway _zJIuclqiEfG5K7mY0I3I6w' com a resposta a dormir noutro ficheiro.
$decisaoRatificada = @{}
if (Test-Path $GlossaryPath) {
    $linhas = Get-Content $GlossaryPath -Encoding UTF8
    $dentro = $false
    $chave = $null
    foreach ($l in $linhas) {
        if ($l -match '^decisions:') { $dentro = $true; continue }
        if ($dentro -and $l -match '^[a-z]' ) { break }
        if (-not $dentro) { continue }
        if ($l -match '^\s+"([^"]+)":\s*$') { $chave = $Matches[1]; continue }
        if ($chave -and $l -match '^\s+question:\s*"(.+)"\s*$') { $decisaoRatificada[$chave] = $Matches[1] }
    }
}

# Onde cada construto aterra, do mapa autorado. Sem isto todos os cards de build
# apontavam para o mesmo ficheiro, e o manifesto do agente nao teria de onde
# derivar permissao de escrita.
$projOfLayer = @{}
foreach ($l in $map.layers) { $projOfLayer[$l.name] = $l.project }
$landingOfKind = @{}
foreach ($e in (Arr $map.landing.byNodeKind)) { $landingOfKind[$e.kind] = $e }
function Get-Landings {
    param($Entry, [bool]$RegraDeNegocio)
    $out = @()
    if (-not $Entry) { return $out }
    $principal = $Entry
    if ($RegraDeNegocio -and $Entry.quandoRegraDeNegocio) { $principal = $Entry.quandoRegraDeNegocio }
    $out += [pscustomobject]@{ Layer = $principal.layer; Folder = $principal.folder; Status = $principal.status; Porque = $Entry.porque }
    foreach ($t in (Arr $Entry.tambem)) {
        $out += [pscustomobject]@{ Layer = $t.layer; Folder = $t.folder; Status = $t.status; Porque = $Entry.porque }
    }
    return $out
}

$manifestPath = Join-Path $ArtifactsDir 'manifest.json'
$manifest     = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

# O manifesto tem generatedAt, durationMs e host, que mudam a cada execucao sem
# que nada de substancia mude. Hashear o ficheiro inteiro faria os 77 cards mudar
# a cada corrida e treinava toda a gente a ignorar diferencas - que e exactamente
# como uma diferenca real passa despercebida. Hasheia-se a projeccao estavel.
function Get-Sha256 {
    param([string]$Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return (($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($Text)) | ForEach-Object { $_.ToString('x2') }) -join '') }
    finally { $sha.Dispose() }
}
$canonico = @()
foreach ($s in (Arr $manifest.sources | Sort-Object file)) { $canonico += "source|$($s.file)|$($s.sha256)" }
foreach ($a in (Arr $manifest.artifacts | Sort-Object name)) { $canonico += "artifact|$($a.name)|$($a.sha256)" }
foreach ($p in @($manifest.counts.PSObject.Properties | Sort-Object Name)) { $canonico += "count|$($p.Name)|$($p.Value)" }
$manifestSha = Get-Sha256 ($canonico -join "`n")

$xpdlFile = 'POC_Epat.xpdl'
# O carimbo do card NAO e o relogio da maquina: e a data de exportacao do pacote,
# lida do cabecalho do XPDL. E o que o campo serve para dizer - 'a fonte de onde
# isto saiu' - e e a unica leitura que torna duas corridas identicas.
$exportado = $(if ($model.source.created) { $model.source.created } else { '1970-01-01' })
# Lido como UTC, nao como local: converter o fuso da maquina reintroduzia a
# dependencia da maquina que este carimbo existe para eliminar.
$geradoEm  = ([datetime]::ParseExact($exportado.Substring(0, 10), 'yyyy-MM-dd',
    [Globalization.CultureInfo]::InvariantCulture,
    [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal)
    ).ToString('yyyy-MM-ddTHH:mm:ssZ', [Globalization.CultureInfo]::InvariantCulture)

# Parecer agentico: entra como camada anexa e rotulada, nunca como facto. E uma
# ENTRADA do bundle - se mudar, os cards mudam -, por isso e fixado por sha256
# exactamente como o XPDL e o glossario. Julgamento pode entrar; sem rasto, nao.
$review = $null
$reviewSha = $null
if (Test-Path $ReviewPath) {
    $review = Get-Content $ReviewPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $reviewSha = (Get-FileHash -LiteralPath $ReviewPath -Algorithm SHA256).Hash.ToLowerInvariant()
}
$parecerDe = @{}
foreach ($r in (Arr $review.pareceres)) { $parecerDe[$r.alvo] = $r }

# ------------------------------------------------------------------ index ----

$nodeOf = @{}; $procOf = @{}
foreach ($p in $model.processes) {
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) { $nodeOf[$n.id] = $n; $procOf[$n.id] = $p.name }
    }
}

$scopeReasonOf = @{}
$inScopeNode = @{}
foreach ($e in (Arr $scope.elements)) {
    if ($e.kind -eq 'process') { $scopeReasonOf[$e.id] = $e.reason }
    if ($e.kind -eq 'node' -and $e.inScope) { $inScopeNode[$e.id] = $true }
}

# nodeKind -> conceitos que esse construto prova
$conceptOfKind = @{}
foreach ($c in (Arr $spec.concepts)) {
    foreach ($k in (Arr $c.detect.nodeKinds)) {
        if (-not $conceptOfKind.ContainsKey($k)) { $conceptOfKind[$k] = [System.Collections.Generic.List[string]]::new() }
        if ($c.id -notin $conceptOfKind[$k]) { $conceptOfKind[$k].Add($c.id) }
    }
}
$resultOfConcept = @{}
foreach ($er in (Arr $conf.expectedResults)) {
    foreach ($cid in (Arr $er.concepts)) {
        if (-not $resultOfConcept.ContainsKey($cid)) { $resultOfConcept[$cid] = [System.Collections.Generic.List[string]]::new() }
        if ($er.id -notin $resultOfConcept[$cid]) { $resultOfConcept[$cid].Add($er.id) }
    }
}
# O documento nomeia conceitos por rotulo ('Service Task'); os artefactos por id
# ('service-tasks'). Sem esta traducao os cards declaravam rotulos e a cobertura
# de resultados esperados dava sempre por descoberta.
# O acento tem de ser DECOMPOSTO e nao apagado: 'Variaveis' contra 'Variáveis'
# davam VARIAVEIS contra VARIVEIS, e o conceito deixava de casar em silencio.
function ConvertTo-Fold {
    param([string]$s)
    if (-not $s) { return '' }
    $d = $s.Normalize([Text.NormalizationForm]::FormD).ToCharArray()
    $keep = @($d | Where-Object { [Globalization.CharUnicodeInfo]::GetUnicodeCategory($_) -ne 'NonSpacingMark' })
    return ((-join $keep) -replace '[^A-Za-z0-9]', '').ToUpperInvariant()
}
$conceptIdByLabel = @{}
foreach ($c in (Arr $conf.concepts)) { $conceptIdByLabel[(ConvertTo-Fold $c.name)] = $c.id }
function Resolve-Concepts {
    param([string[]]$Labels)
    $out = [System.Collections.Generic.List[string]]::new()
    foreach ($l in (Arr $Labels)) {
        $id = $conceptIdByLabel[(ConvertTo-Fold $l)]
        if (-not $id -and $conceptIdByLabel.ContainsValue($l)) { $id = $l }
        if ($id -and $id -notin $out) { $out.Add($id) }
    }
    return @($out)
}
function Resolve-Results {
    param([string[]]$ConceptIds)
    $out = [System.Collections.Generic.List[string]]::new()
    foreach ($cid in (Arr $ConceptIds)) {
        foreach ($rid in (Arr $resultOfConcept[$cid])) { if ($rid -notin $out) { $out.Add($rid) } }
    }
    return @($out)
}

# Itens sem equivalente .NET, indexados por no: e assim que o bloqueador cai no card certo.
$noEqOfNode = @{}
foreach ($it in @($dossier.items | Where-Object { $_.category -eq 'no-net-equivalent' })) {
    foreach ($oc in (Arr $it.occurrences)) {
        if (-not $oc.nodeId) { continue }
        if (-not $noEqOfNode.ContainsKey($oc.nodeId)) { $noEqOfNode[$oc.nodeId] = [System.Collections.Generic.List[object]]::new() }
        $noEqOfNode[$oc.nodeId].Add($it)
    }
}

# Regras ligam-se por processo + rotulo do passo; o catalogo nao carrega nodeId.
$rulesOfStep = @{}
foreach ($r in (Arr $rules.rules)) {
    if (-not $r.processo -or -not $r.onde.passo) { continue }
    $k = "$($r.processo)|$($r.onde.passo)"
    if (-not $rulesOfStep.ContainsKey($k)) { $rulesOfStep[$k] = [System.Collections.Generic.List[object]]::new() }
    $rulesOfStep[$k].Add($r)
}

# ------------------------------------------------------- ler os cenarios -----

$scenDir = Join-Path $ArtifactsDir 'scenarios'
$cenarios = @(foreach ($s in (Arr $index.scenarios)) {
    Get-Content (Join-Path $scenDir "$($s.id).json") -Raw -Encoding UTF8 | ConvertFrom-Json
})

# ------------------------------------------------- agrupar por segmento ------

# Duas ocorrencias do mesmo troco em cenarios diferentes sao O MESMO trabalho.
# A chave e etapas + no que abre + no que fecha; o representante e a ocorrencia
# mais rica, porque uma versao truncada por corte de laco esconderia passos.
$grupos = @{}
foreach ($c in $cenarios) {
    foreach ($seg in (Arr $c.segmentos)) {
        $k = "$(@($seg.etapas) -join ',')|$($seg.abrePor)|$($seg.fechaEm)"
        if (-not $grupos.ContainsKey($k)) { $grupos[$k] = [ordered]@{ chave = $k; ocorrencias = [System.Collections.Generic.List[object]]::new() } }
        $grupos[$k].ocorrencias.Add([pscustomobject]@{ Cenario = $c; Seg = $seg })
    }
}

$build = [System.Collections.Generic.List[object]]::new()
$idSeq = 0
$cardDoGrupo = @{}

foreach ($k in ($grupos.Keys | Sort-Object)) {
    $g = $grupos[$k]
    $melhor = @($g.ocorrencias | Sort-Object { @($_.Seg.nos).Count } -Descending)[0]
    $seg = $melhor.Seg
    $cen = $melhor.Cenario
    $nos = @(Arr $seg.nos)

    $emEscopo = @($nos | Where-Object { $inScopeNode.ContainsKey($_.id) })
    if ($emEscopo.Count -eq 0) { continue }

    $idSeq++
    $cardId = 'BUILD-{0}-seg{1:D3}' -f (Slug $cen.process), $idSeq
    $cardDoGrupo[$k] = $cardId

    # Como se entra em cada no: tudo o que nao for 'fluxo' nao existe como
    # transicao no XPDL e e a omissao mais facil de cometer no .NET.
    $entrouPor = @{}
    for ($i = 0; $i -lt @($cen.path).Count; $i++) { $entrouPor[$cen.path[$i].id] = $cen.path[$i].entrouPor }

    $checklist = @()
    $ordem = 0
    foreach ($n in $nos) {
        $ordem++
        $item = [ordered]@{ ordem = $ordem; nodeId = $n.id; nome = $n.nome; kind = $n.tipo }
        # O id curto e o que o glossario usa como chave.
        $q = $decisaoRatificada["$($procOf[$n.id])/$($n.id.Substring(0, [Math]::Min(10, $n.id.Length)))"]
        if ($q) { $item.decideQue = $q }
        $ep = $entrouPor[$n.id]
        if ($ep) { $item.entrouPor = $ep }
        $rk = "$($procOf[$n.id])|$($n.nome)"
        if ($rulesOfStep.ContainsKey($rk)) {
            $reg = @($rulesOfStep[$rk] | Where-Object { $_.classification.eRegraDeNegocio })
            if ($reg.Count -gt 0) { $item.nota = "$($reg.Count) regra(s) de negocio neste passo: $((@($reg | ForEach-Object { $_.id }) | Select-Object -First 4) -join ', ')" }
        }
        $checklist += $item
    }

    # O representante e uma passagem so; outras passagens pelo mesmo troco visitam
    # nos que essa nao visita. Sem a uniao, esses nos ficavam sem card nenhum.
    $naChecklist = @{}
    foreach ($n in $nos) { $naChecklist[$n.id] = $true }
    foreach ($oc in $g.ocorrencias) {
        foreach ($n in (Arr $oc.Seg.nos)) {
            if ($naChecklist.ContainsKey($n.id)) { continue }
            $naChecklist[$n.id] = $true
            $ordem++
            $checklist += [ordered]@{
                ordem = $ordem; nodeId = $n.id; nome = $n.nome; kind = $n.tipo
                nota = "Visitado noutra passagem por este mesmo troco ($($oc.Cenario.id)); nao aparece no percurso de referencia."
            }
        }
    }
    $todosOsNos = @($checklist | ForEach-Object { $_.nodeId })

    $foraDeFluxo = @($checklist | Where-Object { $_.entrouPor -and $_.entrouPor -ne 'fluxo' })
    $etapas = @(@($seg.etapas) | ForEach-Object { [int]$_ } | Sort-Object -Unique)
    $nomesEtapa = @(foreach ($e in $etapas) { @($conf.etapas | Where-Object { [int]$_.n -eq $e })[0].name })

    # Extremo do troco sem rotulo proprio: usa-se a pergunta que o humano ja
    # respondeu, senao o titulo chega a fase 2 como 'gateway _zJIuclqiE...'.
    function Get-Extremo {
        param([string]$Rotulo, $Passos)
        if ($Rotulo -notmatch '^(gateway|startEvent|endEvent|timerEvent) _') { return $Rotulo }
        $achado = @($Passos | Where-Object { $_.nome -eq $Rotulo -and $_.decideQue })
        if ($achado.Count -gt 0) { return $achado[0].decideQue }
        return $Rotulo
    }
    $abre  = Get-Extremo -Rotulo $seg.abrePor -Passos $checklist
    $fecha = Get-Extremo -Rotulo $seg.fechaEm -Passos $checklist

    $conceitos = [System.Collections.Generic.List[string]]::new()
    foreach ($nid in $todosOsNos) {
        $kind = $(if ($nodeOf[$nid]) { $nodeOf[$nid].kind } else { $null })
        foreach ($cid in (Arr $conceptOfKind[$kind])) { if ($cid -notin $conceitos) { $conceitos.Add($cid) } }
    }
    $resultados = @(Resolve-Results -ConceptIds @($conceitos))

    $gaps = @()
    $refs = [System.Collections.Generic.List[string]]::new()
    foreach ($nid in $todosOsNos) {
        foreach ($it in (Arr $noEqOfNode[$nid])) {
            if ($it.id -in $refs) { continue }
            $refs.Add($it.id)
            $quantos = @($todosOsNos | Where-Object { @(Arr $noEqOfNode[$_] | ForEach-Object { $_.id }) -contains $it.id }).Count
            $gaps += [ordered]@{
                construct = ($it.id -replace '^NOEQ-', '')
                detail = "$quantos no(s) deste segmento, de $(@($it.occurrences).Count) no pacote inteiro. Decisao ratificada em $($it.id)."
                status = 'decided'
                decisionRef = $it.id
            }
        }
    }

    $temJulgamento = @($nos | Where-Object { $_.tipo -in @('scriptTask','gateway','callActivity','serviceTask') }).Count -gt 0
    $tipoTraducao = $(if ($temJulgamento) { 'lossy' } else { 'lossless' })
    $nivel = $(if ($gaps.Count -gt 0) { 'medium' } elseif ($temJulgamento) { 'medium' } else { 'high' })

    $resumo = "Troco de $($nos.Count) passo(s) da jornada do caso, de '$($seg.abrePor)' ate '$($seg.fechaEm)', no processo $($cen.process)."
    if ($foraDeFluxo.Count -gt 0) {
        $resumo += " ATENCAO: $($foraDeFluxo.Count) passo(s) sao alcancados por ligacao que NAO existe como transicao no XPDL ($((@($foraDeFluxo | ForEach-Object { $_.entrouPor }) | Sort-Object -Unique) -join ', ')) e tem de ser escrita explicitamente no fluxo .NET."
    }
    if ($seg.herdadoDe) { $resumo += " O troco corre dentro de um processo chamado a partir de '$($seg.herdadoDe)', e e dai que herda a etapa." }

    $sourceRef = @(foreach ($n in ($nos | Select-Object -First 20)) {
        [ordered]@{ tibcoFile = $xpdlFile; elementId = $n.id; xpath = "//xpdl2:Activity[@Id='$($n.id)']" }
    })

    # Um card aterra em tantos sitios quantos os construtos que carrega. Se
    # atravessar camadas, isso passa a estar a vista em vez de implicito.
    # O mesmo caminho pode vir por dois construtos com exigencia diferente - o
    # endEvent nao tem corpo, o gateway tem - e fica com a mais exigente das duas.
    $exigencia = @{ 'final' = 0; 'scaffold' = 1; 'draft' = 2 }
    $aterragens = @{}
    foreach ($i in $checklist) {
        $entry = $landingOfKind[$i.kind]
        if (-not $entry) { continue }
        $rk = "$($procOf[$i.nodeId])|$($i.nome)"
        $eRegra = $false
        if ($rulesOfStep.ContainsKey($rk)) { $eRegra = @($rulesOfStep[$rk] | Where-Object { $_.classification.eRegraDeNegocio }).Count -gt 0 }
        foreach ($l in (Get-Landings -Entry $entry -RegraDeNegocio $eRegra)) {
            $path = "src/$($projOfLayer[$l.Layer])/$($l.Folder)"
            if ($aterragens.ContainsKey($path) -and $exigencia[$aterragens[$path].status] -ge $exigencia[$l.Status]) { continue }
            $aterragens[$path] = [ordered]@{ path = $path; status = $l.Status; note = $l.Porque }
        }
    }
    $scaffold = @($aterragens.Keys | Sort-Object | ForEach-Object { $aterragens[$_] })
    if ($scaffold.Count -eq 0) {
        $scaffold = @([ordered]@{ path = "src/$($projOfLayer['Application'])/Workflows"; status = 'scaffold'; note = 'Topologia derivada do XPDL.' })
    }

    $camadas = @($scaffold | ForEach-Object { ($_.path -split '/')[1] } | Sort-Object -Unique)
    $aEscrever = @($scaffold | Where-Object { $_.status -ne 'final' })
    $regrasNoTroco = 0
    foreach ($i in $checklist) {
        $rk = "$($procOf[$i.nodeId])|$($i.nome)"
        if ($rulesOfStep.ContainsKey($rk)) { $regrasNoTroco += @($rulesOfStep[$rk] | Where-Object { $_.classification.eRegraDeNegocio }).Count }
    }
    $peso = Get-Peso -Camadas $camadas.Count -AEscrever $aEscrever.Count -Nos @($checklist).Count
    $dimensao = [ordered]@{
        nos = @($checklist).Count
        camadas = $camadas.Count
        pastas = @($scaffold).Count
        aEscrever = $aEscrever.Count
        bloqueadores = $gaps.Count
        regrasDeNegocio = $regrasNoTroco
        peso = $peso
    }
    if ($peso -eq 'atravessa-o-sistema') {
        $dimensao.nota = "Toca $($camadas.Count) projecto(s) e $($aEscrever.Count) pasta(s) por escrever. Continua a ser um card valido - e o troco que cumpre a etapa -, mas nao deve ser atribuido a uma pessoa so nem estimado como os outros."
    }

    $card = [ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/backlog-card/v1'
        id = $cardId
        cardType = 'build'
        title = "$($nomesEtapa -join ' + '): de '$abre' ate '$fecha'"
        epic = "etapa-$($etapas -join '-')"
        content = [ordered]@{
            intent = "Construir o troco da jornada que vai de '$($seg.abrePor)' a '$($seg.fechaEm)', reproduzindo o comportamento observado no legado."
            injectedContext = [ordered]@{
                summary = $resumo
                hypotheses = @()
            }
            scaffold = @($scaffold)
            checklist = @($checklist)
        }
        irRef = [ordered]@{
            artifact = 'scenarios/index.json'
            pointer = "scenarios[$($cen.id)].segmentos[$($seg.ordemNaJornada)]"
            kind = 'segment'
            nodeIds = @($todosOsNos)
        }
        sourceRef = @($sourceRef)
        confidence = [ordered]@{
            level = $nivel
            basis = "Topologia e ordem sao transcricao; $(if ($temJulgamento) { 'os corpos de script, condicao e chamada exigem interpretacao' } else { 'o troco nao contem corpo a interpretar' })."
            verified = $false
            translation = $tipoTraducao
        }
        provenance = [ordered]@{
            generatedAt = $geradoEm
            manifestSha256 = $manifestSha
            package = $Package
        }
        acceptance = [ordered]@{
            oracle = [ordered]@{
                kind = 'scenario-path'
                fixture = "artifacts/$Package/scenarios/$($cen.id).json"
                immutable = $true
                caseCount = @($g.ocorrencias).Count
            }
            criteria = @(
                'Build limpo, sem avisos',
                "O percurso do passo $($seg.doPasso) ao $($seg.aoPasso) do cenario $($cen.id) corre de ponta a ponta"
            )
        }
        scope = [ordered]@{
            process = $cen.process
            inPocScope = $true
            scopeReason = $(if ($scopeReasonOf[$cen.process]) { $scopeReasonOf[$cen.process] } else { 'processo em escopo segundo scope.json' })
        }
        fulfills = [ordered]@{
            etapas = @($etapas)
            segmento = [ordered]@{
                cenarioReferencia = $cen.id
                ordemNaJornada = [int]$seg.ordemNaJornada
                doPasso = [int]$seg.doPasso
                aoPasso = [int]$seg.aoPasso
                abrePor = $seg.abrePor
                fechaEm = $seg.fechaEm
            }
            concepts = @($conceitos)
            expectedResults = @($resultados)
            evidenceKind = 'instance-trace'
        }
        dimensao = $dimensao
    }
    if ($seg.herdadoDe) { $card.fulfills.segmento.herdadoDe = $seg.herdadoDe }
    if ($seg.prologo) { $card.fulfills.segmento.prologo = $true }
    if ($gaps.Count -gt 0) { $card.gaps = @($gaps); $card.reviewRefs = @($refs) }

    # O parecer do agente entra aqui, e so aqui. O card que ele tocar passa a
    # declarar o sha do parecer: mudou o julgamento, o card tem de ser regerado.
    $par = $parecerDe[$cardId]
    if (-not $par) { $par = $parecerDe[$k] }
    if ($par) {
        $card.content.injectedContext.hypotheses = @(Arr $par.hipoteses)
        if ($par.tituloSugerido) { $card.title = $par.tituloSugerido }
        $card.provenance.reviewSha256 = $reviewSha
    }

    $build.Add([pscustomobject]@{ Card = $card; Cenario = $cen; Seg = $seg; Chave = $k })
}

# --------------------------------------------------------------- dependsOn ---

# A ordem sai da jornada: o segmento #7 depende do #6 do mesmo cenario. Nao ha
# aqui juizo nenhum, e uma leitura da posicao.
$porCenarioOrdem = @{}
foreach ($b in $build) { $porCenarioOrdem["$($b.Cenario.id)|$($b.Seg.ordemNaJornada)"] = $b.Card.id }
foreach ($b in $build) {
    $ant = $porCenarioOrdem["$($b.Cenario.id)|$([int]$b.Seg.ordemNaJornada - 1)"]
    if ($ant) { $b.Card.dependsOn = @($ant) }
}

# ------------------------------------------------------- cards de validacao --

$valid = [System.Collections.Generic.List[object]]::new()
foreach ($fp in (Arr $index.etapas.footprint)) {
    if ([int]$fp.cenarios -eq 0) { continue }
    $etapaN = [int]$fp.n
    $doProcesso = @($cenarios | Where-Object { $etapaN -in @($_.etapas) -and $_.destino.como -eq 'fim-do-caso' })
    if ($doProcesso.Count -eq 0) { $doProcesso = @($cenarios | Where-Object { $etapaN -in @($_.etapas) }) }
    $ref = @($doProcesso | Sort-Object { $_.lengthNodes } -Descending)[0]
    $conceitos = @(Resolve-Concepts -Labels @(Arr ($conf.etapas | Where-Object { [int]$_.n -eq $etapaN }).conceptsInDocument))

    $valid.Add([ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/backlog-card/v1'
        id = 'VALID-ETAPA{0}-percurso' -f $etapaN
        cardType = 'validation'
        title = "Provar a etapa $etapaN - $($fp.name) - por rasto de instancia"
        epic = "etapa-$etapaN"
        content = [ordered]@{
            intent = "Montar o arnes que corre os percursos da etapa $etapaN e compara o rasto com o cenario, para que a etapa deixe de estar extraida e passe a provada."
            injectedContext = [ordered]@{
                summary = "A etapa toca $($fp.nos) no(s) e e exercitada por $($fp.cenarios) cenario(s). O percurso de referencia e $($ref.id), com $($ref.lengthNodes) passos, de '$($ref.origem.nome)' a '$($ref.destino.nome)'. Os valores esperados sao do kit e nao se editam."
                hypotheses = @()
            }
            scaffold = @([ordered]@{ path = 'tests/SefazSp.Epat.Oracles.Tests'; status = 'scaffold'; note = 'O agente liga o arnes a fixture e nunca escreve o valor esperado.' })
        }
        irRef = [ordered]@{ artifact = 'scenarios/index.json'; pointer = "etapas.footprint[$etapaN]"; kind = 'segment'; nodeIds = @() }
        sourceRef = @(@(Arr $conf.etapas | Where-Object { [int]$_.n -eq $etapaN }).anchorNodes | Select-Object -First 5 | ForEach-Object {
            [ordered]@{ tibcoFile = $xpdlFile; elementId = $_.nodeId; xpath = "//xpdl2:Activity[@Id='$($_.nodeId)']" }
        })
        confidence = [ordered]@{ level = 'high'; basis = 'O oraculo e o cenario derivado do grafo; nao ha interpretacao no arnes.'; verified = $false; translation = 'lossless' }
        provenance = [ordered]@{ generatedAt = $geradoEm; manifestSha256 = $manifestSha; package = $Package }
        acceptance = [ordered]@{
            oracle = [ordered]@{ kind = 'scenario-path'; fixture = "artifacts/$Package/scenarios/$($ref.id).json"; immutable = $true; caseCount = [int]$fp.cenarios }
            criteria = @('O rasto de execucao bate passo a passo com o percurso do cenario')
        }
        scope = [ordered]@{ process = $ref.process; inPocScope = $true; scopeReason = 'etapa declarada na seccao 1 do plano de cumprimento' }
        fulfills = [ordered]@{
            etapas = @($etapaN)
            segmento = [ordered]@{ cenarioReferencia = $ref.id; ordemNaJornada = 1; doPasso = 1; aoPasso = [int]$ref.lengthNodes; abrePor = $ref.origem.nome; fechaEm = $ref.destino.nome }
            concepts = @($conceitos)
            expectedResults = @(Resolve-Results -ConceptIds $conceitos)
            evidenceKind = 'instance-trace'
        }
        dimensao = [ordered]@{
            nos = [int]$fp.nos; camadas = 1; pastas = 1; aEscrever = 1
            bloqueadores = @(Arr $fp.bloqueadores).Count; regrasDeNegocio = 0
            peso = (Get-Peso -Camadas 1 -AEscrever 1 -Nos ([int]$fp.nos))
        }
    })
}

# O nome da operacao no WSDL traz o caminho de pasta codificado. O cartao mostra a
# forma legivel; o elementId continua a ser o nome cru, que e a ligacao a fonte.
$caminhoDaOperacao = @{}
foreach ($svc in (Arr $services.services)) {
    foreach ($op in (Arr $svc.operations)) { $caminhoDaOperacao[$op.name] = $op.logicalPath }
}
function Get-NomeDaOperacao {
    param([string]$OpName)
    $p = $caminhoDaOperacao[$OpName]
    if (-not $p) { $p = $OpName }
    return (($p -split '/')[-1])
}

foreach ($opName in (Arr $services.invokedOperations)) {
    $nomeCurto = Get-NomeDaOperacao $opName
    $valid.Add([ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/backlog-card/v1'
        id = 'VALID-{0}-contrato' -f (Slug $nomeCurto)
        cardType = 'validation'
        title = "Provar o contrato da operacao $nomeCurto"
        epic = 'fundacao'
        content = [ordered]@{
            intent = "Verificar que a porta .NET desta operacao respeita a forma declarada no WSDL, em ambos os sentidos."
            injectedContext = [ordered]@{ summary = "Operacao realmente invocada pelo processo, em $(if ($caminhoDaOperacao[$opName]) { $caminhoDaOperacao[$opName] } else { $opName }). O contrato e o WSDL entregue; o teste compara pedido e resposta contra ele."; hypotheses = @() }
            scaffold = @([ordered]@{ path = 'tests/SefazSp.Epat.Oracles.Tests'; status = 'scaffold'; note = 'Conformidade de contrato contra o WSDL fixado.' })
        }
        irRef = [ordered]@{ artifact = 'service-contracts.json'; pointer = "operations[$opName]"; kind = 'operation'; nodeIds = @() }
        sourceRef = @(@([ordered]@{ tibcoFile = 'EPAT.wsdl'; elementId = $opName }))
        confidence = [ordered]@{ level = 'high'; basis = 'O contrato e transcricao do WSDL.'; verified = $false; translation = 'lossless' }
        provenance = [ordered]@{ generatedAt = $geradoEm; manifestSha256 = $manifestSha; package = $Package }
        acceptance = [ordered]@{
            oracle = [ordered]@{ kind = 'contract'; fixture = "artifacts/$Package/service-contracts.json"; immutable = $true; caseCount = 1 }
            criteria = @('Pedido e resposta validam contra o esquema do WSDL')
        }
        scope = [ordered]@{ process = 'fundacao'; inPocScope = $true; scopeReason = 'operacao invocada por um passo em escopo' }
        fulfills = [ordered]@{
            etapas = @(3)
            segmento = [ordered]@{ cenarioReferencia = 'contrato'; ordemNaJornada = 1; doPasso = 1; aoPasso = 1; abrePor = $opName; fechaEm = $opName }
            concepts = @('service-tasks')
            expectedResults = @(Resolve-Results -ConceptIds @('service-tasks'))
            evidenceKind = 'contract-test'
        }
        dimensao = [ordered]@{ nos = 1; camadas = 1; pastas = 1; aEscrever = 1; bloqueadores = 0; regrasDeNegocio = 0; peso = 'pequeno' }
    })
}

if ($decisions) {
    $linhas = @(Arr $decisions.rulesheets | ForEach-Object { @(Arr $_.rules).Count } | Measure-Object -Sum).Sum
    $valid.Add([ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/backlog-card/v1'
        id = 'VALID-CORTICON-tabela'
        cardType = 'validation'
        title = 'Provar a equivalencia entre o DMN gerado e as planilhas Corticon'
        epic = 'fundacao'
        content = [ordered]@{
            intent = 'Correr cada coluna da planilha como caso e comparar o resultado do motor .NET com o do Corticon.'
            injectedContext = [ordered]@{ summary = "As planilhas trazem $linhas coluna(s) de regra. A ordem das colunas importa: uma posterior sobrescreve a anterior."; hypotheses = @() }
            scaffold = @([ordered]@{ path = 'tests/SefazSp.Epat.Oracles.Tests'; status = 'scaffold'; note = 'Equivalencia DMN x Corticon, ja provada em 3000 casos; o card fixa-a como teste permanente.' })
        }
        irRef = [ordered]@{ artifact = 'decision-tables.json'; pointer = 'rulesheets[*]'; kind = 'rule'; nodeIds = @() }
        sourceRef = @(@([ordered]@{ tibcoFile = 'intimacoes_Parametros.ers'; elementId = 'rulesheets' }))
        confidence = [ordered]@{ level = 'high'; basis = 'Equivalencia ja verificada pelo kit; o card so a fixa como teste permanente.'; verified = $false; translation = 'lossless' }
        provenance = [ordered]@{ generatedAt = $geradoEm; manifestSha256 = $manifestSha; package = $Package }
        acceptance = [ordered]@{
            oracle = [ordered]@{ kind = 'decision-table'; fixture = "artifacts/$Package/dmn"; immutable = $true; caseCount = [Math]::Max(1, [int]$linhas) }
            criteria = @('Zero divergencias entre DMN e Corticon')
        }
        scope = [ordered]@{ process = 'fundacao'; inPocScope = $true; scopeReason = 'regras de negocio do motor de decisao, em escopo pela etapa 3' }
        fulfills = [ordered]@{
            etapas = @(3)
            segmento = [ordered]@{ cenarioReferencia = 'corticon'; ordemNaJornada = 1; doPasso = 1; aoPasso = 1; abrePor = 'planilha'; fechaEm = 'planilha' }
            concepts = @('decisions')
            expectedResults = @(Resolve-Results -ConceptIds @('decisions'))
            evidenceKind = 'branch-test'
        }
        dimensao = [ordered]@{ nos = [int]$linhas; camadas = 1; pastas = 1; aEscrever = 1; bloqueadores = 0; regrasDeNegocio = [int]$linhas; peso = 'medio' }
    })
}

# ------------------------------------------------------------ cards de duble -

$doubles = [System.Collections.Generic.List[object]]::new()
foreach ($pi in (Arr $model.processInterfaces)) {
    $impl = @(Arr $pi.implementedBy)
    $doubles.Add([ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/backlog-card/v1'
        id = 'DOUBLE-{0}-interface' -f (Slug $pi.name)
        cardType = 'double'
        title = "Duble para a interface de processo $($pi.name)"
        epic = 'fundacao'
        content = [ordered]@{
            intent = "Construir um duble fiel ao contrato de $($pi.name), conduzido por cenario, para que o destino em falta falhe de forma visivel em vez de silenciosa."
            injectedContext = [ordered]@{
                summary = "A chamada e resolvida em execucao. Implementacoes entregues no pacote: $(if ($impl.Count) { $impl -join ', ' } else { 'nenhuma' }). O conjunto de destinos NAO e fechado, por isso o registo e validado no arranque: destino sem duble quebra o teste de registo, nao a producao."
                hypotheses = @()
            }
            scaffold = @([ordered]@{ path = "src/$($projOfLayer['Infrastructure'])/Integration.Doubles"; status = 'scaffold'; note = 'Duble tipado a partir da ProcessInterface, conduzido por cenario.' })
        }
        irRef = [ordered]@{ artifact = 'process-model.json'; pointer = "processInterfaces[$($pi.name)]"; kind = 'operation'; nodeIds = @() }
        sourceRef = @(@([ordered]@{ tibcoFile = $xpdlFile; elementId = $pi.id; xpath = "//xpdExt:ProcessInterface[@Id='$($pi.id)']" }))
        confidence = [ordered]@{ level = 'high'; basis = 'A interface e transcricao do xpdExt:ProcessInterface.'; verified = $false; translation = 'lossless' }
        provenance = [ordered]@{ generatedAt = $geradoEm; manifestSha256 = $manifestSha; package = $Package }
        acceptance = [ordered]@{
            oracle = [ordered]@{ kind = 'contract'; fixture = "artifacts/$Package/scaffold/src/SefazSp.Epat.Application/Abstractions/Processes/I$($pi.name).cs"; immutable = $true; caseCount = 1 }
            criteria = @('O registo de destinos e validado no arranque', 'Destino sem implementacao falha de forma visivel')
        }
        scope = [ordered]@{ process = 'fundacao'; inPocScope = $true; scopeReason = 'interface usada por chamada dinamica de um passo em escopo' }
        fulfills = [ordered]@{
            etapas = @(2)
            segmento = [ordered]@{ cenarioReferencia = 'interface'; ordemNaJornada = 1; doPasso = 1; aoPasso = 1; abrePor = $pi.name; fechaEm = $pi.name }
            concepts = @('procedures-dinamicas')
            expectedResults = @(Resolve-Results -ConceptIds @('procedures-dinamicas'))
            evidenceKind = 'contract-test'
        }
        dimensao = [ordered]@{ nos = $impl.Count; camadas = 1; pastas = 1; aEscrever = 1; bloqueadores = 0; regrasDeNegocio = 0; peso = 'pequeno' }
    })
}

# ------------------------------------ cards para conceito sem quem o prove ---

# Nem todo o conceito se prova por um no. 'Variaveis de Processo' detecta-se por
# caseFields, 'Regras de Decisao' por decisionRules, 'Graft Step' pelo NOME do
# passo - e um card de segmento, que nasce de tipos de no, nunca os pode reclamar.
#
# O fecho e por CONCEITO e nao por resultado esperado, e a diferenca importa:
# 'Criacao dinamica de etapas' depende de graft-step E de procedures-dinamicas, e
# se bastasse um deles o resultado dava-se por coberto provando metade. Um
# conceito por provar e um resultado por provar, mesmo que outro conceito do
# mesmo resultado ja tenha card.
$conceptSpec = @{}
foreach ($c in (Arr $spec.concepts)) { $conceptSpec[$c.id] = $c }
$etapasDoConceito = @{}
foreach ($et in (Arr $conf.etapas)) {
    foreach ($cid in (Resolve-Concepts -Labels @(Arr $et.conceptsInDocument))) {
        if (-not $etapasDoConceito.ContainsKey($cid)) { $etapasDoConceito[$cid] = [System.Collections.Generic.List[int]]::new() }
        if ([int]$et.n -notin $etapasDoConceito[$cid]) { $etapasDoConceito[$cid].Add([int]$et.n) }
    }
}

$conceitoReclamado = @{}
foreach ($c in @(@($build | ForEach-Object { $_.Card }) + @($valid) + @($doubles))) {
    foreach ($x in (Arr $c.fulfills.concepts)) { $conceitoReclamado[$x] = $true }
}

# Que etapa toca cada no, lido dos segmentos ja calculados. E por aqui que um
# conceito que o documento nao atribui a etapa nenhuma - 'Timers e Deadlines',
# 'Graft Step' - descobre onde vive, em vez de cair num valor por omissao.
$etapasDoNo = @{}
foreach ($cen in $cenarios) {
    foreach ($seg in (Arr $cen.segmentos)) {
        foreach ($no in (Arr $seg.nos)) {
            if (-not $etapasDoNo.ContainsKey($no.id)) { $etapasDoNo[$no.id] = @{} }
            foreach ($e in (Arr $seg.etapas)) { $etapasDoNo[$no.id][[int]$e] = $true }
        }
    }
}
# O BPMN emitido e o unico sitio onde direccao de gateway e definicao de evento se
# distinguem; aqui traduz-se de volta para o tipo de no do modelo.
$kindDoElementoBpmn = @{
    'exclusiveGateway'     = @{ kind = 'gateway';    gatewayType = 'Exclusive' }
    'parallelGateway'      = @{ kind = 'gateway';    gatewayType = 'Parallel' }
    'timerEventDefinition' = @{ kind = 'timerEvent'; gatewayType = $null }
}
function Get-ConceptNodes {
    param($Detect)
    $ids = [System.Collections.Generic.List[string]]::new()
    foreach ($p in $model.processes) {
        foreach ($s in $p.scopes) {
            foreach ($n in $s.nodes) {
                $bate = $false
                if ((Arr $Detect.nodeKinds) -contains $n.kind) { $bate = $true }
                if ($Detect.nodeNamePattern -and $n.displayName -match [regex]::Escape($Detect.nodeNamePattern)) { $bate = $true }
                foreach ($be in (Arr $Detect.bpmnElements)) {
                    $t = $kindDoElementoBpmn[$be]
                    if (-not $t) { continue }
                    if ($n.kind -ne $t.kind) { continue }
                    if ($t.gatewayType -and $n.gatewayType -ne $t.gatewayType) { continue }
                    $bate = $true
                }
                if ($bate -and $n.id -notin $ids) { $ids.Add($n.id) }
            }
        }
    }
    return @($ids)
}

foreach ($cc in (Arr $conf.concepts)) {
    if ($conceitoReclamado.ContainsKey($cc.id)) { continue }
    $det = $conceptSpec[$cc.id].detect

    # O oraculo sai do DETECTOR: prova-se pelo mesmo artefacto por onde se conta,
    # senao o card provava outra coisa diferente da que reclama.
    $oracle = 'scenario-path'; $fixture = "artifacts/$Package/scenarios/index.json"
    $evidencia = 'instance-trace'; $casos = @($cenarios).Count; $porOnde = 'percurso'
    if ($det.caseFields) {
        $oracle = 'schema-conformance'; $fixture = "artifacts/$Package/case-field-dictionary.json"
        $evidencia = 'state-snapshot'; $casos = @(Arr $fields.fields).Count; $porOnde = 'dicionario de campos'
    }
    elseif ($det.decisionRules) {
        $oracle = 'decision-table'; $fixture = "artifacts/$Package/dmn"
        $evidencia = 'branch-test'; $casos = [Math]::Max(1, @(Arr $decisions.rules).Count); $porOnde = 'planilhas de decisao'
    }
    elseif ($det.nodeNamePattern) {
        $evidencia = 'parent-child-trace'; $porOnde = "nome do passo ('$($det.nodeNamePattern)')"
    }
    elseif ($det.bpmnElements) {
        $evidencia = 'concurrent-timeline'; $porOnde = "elemento BPMN ($(@($det.bpmnElements) -join ', '))"
    }

    # A etapa vem do documento quando ele a declara; senao, dos nos onde o
    # conceito de facto ocorre. Nunca de um valor por omissao.
    $etapas = @(Arr $etapasDoConceito[$cc.id] | Sort-Object -Unique)
    $comoSoubeDaEtapa = 'declarada no documento'
    if ($etapas.Count -eq 0) {
        $doGrafo = @{}
        foreach ($nid in (Get-ConceptNodes -Detect $det)) {
            foreach ($e in @($etapasDoNo[$nid].Keys)) { $doGrafo[[int]$e] = $true }
        }
        $etapas = @($doGrafo.Keys | Sort-Object)
        $comoSoubeDaEtapa = 'derivada dos nos onde o conceito ocorre - o documento nao o atribui a etapa nenhuma'
    }
    if ($etapas.Count -eq 0) {
        $etapas = @(1..7)
        $comoSoubeDaEtapa = 'transversal: nao se prende a no nenhum nem o documento o atribui'
    }
    $resultados = @(Resolve-Results -ConceptIds @($cc.id))

    $valid.Add([ordered]@{
        '$schema' = 'sefaz-sp/tibco-intermediate/backlog-card/v1'
        id = 'VALID-{0}-conceito' -f (Slug $cc.id)
        cardType = 'validation'
        title = "Provar o conceito $($cc.name)"
        epic = 'fundacao'
        content = [ordered]@{
            intent = "Montar o arnes que prova o conceito '$($cc.name)': $($cc.objective) Nenhum passo do processo o demonstra sozinho, por isso nao cabe em card de segmento."
            injectedContext = [ordered]@{
                summary = "Conceito da seccao 2 do documento, detectado por $porOnde e nao por tipo de no - e por isso invisivel para os cards de segmento. Ocorre em $($cc.occurrences) ponto(s). Etapa $comoSoubeDaEtapa. Sustenta o(s) resultado(s) esperado(s) $(@($resultados) -join ', '). Os valores esperados sao do kit e nao se editam."
                hypotheses = @()
            }
            scaffold = @([ordered]@{ path = 'tests/SefazSp.Epat.Oracles.Tests'; status = 'scaffold'; note = 'O agente liga o arnes a fixture e nunca escreve o valor esperado.' })
        }
        irRef = [ordered]@{ artifact = 'process-model.json'; pointer = "concepts[$($cc.id)]"; kind = 'field'; nodeIds = @() }
        sourceRef = @(@([ordered]@{ tibcoFile = $xpdlFile; elementId = $cc.id }))
        confidence = [ordered]@{ level = 'high'; basis = 'O oraculo e o proprio artefacto extraido; nao ha interpretacao no arnes.'; verified = $false; translation = 'lossless' }
        provenance = [ordered]@{ generatedAt = $geradoEm; manifestSha256 = $manifestSha; package = $Package }
        acceptance = [ordered]@{
            oracle = [ordered]@{ kind = $oracle; fixture = $fixture; immutable = $true; caseCount = [int]$casos }
            criteria = @("O conceito e observavel em execucao nos $($cc.occurrences) ponto(s) onde o pacote o usa")
        }
        scope = [ordered]@{ process = 'fundacao'; inPocScope = $true; scopeReason = 'conceito declarado na seccao 2 do documento da POC' }
        fulfills = [ordered]@{
            etapas = @($etapas)
            segmento = [ordered]@{ cenarioReferencia = $cc.id; ordemNaJornada = 1; doPasso = 1; aoPasso = 1; abrePor = $cc.name; fechaEm = $cc.name }
            concepts = @($cc.id)
            expectedResults = @($resultados)
            evidenceKind = $evidencia
        }
        dimensao = [ordered]@{ nos = [int]$cc.occurrences; camadas = 1; pastas = 1; aEscrever = 1; bloqueadores = @(Arr $cc.blockers).Count; regrasDeNegocio = 0; peso = 'medio' }
    })
}

# ------------------------------------------------------------------ write ----

$todos = @(@($build | ForEach-Object { $_.Card }) + @($valid) + @($doubles))

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
Get-ChildItem $OutDir -Filter '*.json' -File -ErrorAction SilentlyContinue | Remove-Item -Force

# Card que nao valida contra o schema NAO e escrito: seria um card partido a
# atravessar a comporta, que e a unica coisa que este kit existe para impedir.
$schemaRaw = Get-Content $SchemaPath -Raw -Encoding UTF8
$invalidos = [System.Collections.Generic.List[object]]::new()
foreach ($c in $todos) {
    $json = $c | ConvertTo-Json -Depth 20
    $erro = $null
    $ok = $false
    try { $ok = Test-Json -Json $json -Schema $schemaRaw -ErrorAction Stop } catch { $erro = $_.Exception.Message }
    if ($ok) { $json | Set-Content -LiteralPath (Join-Path $OutDir "$($c.id).json") -Encoding UTF8 }
    else { $invalidos.Add([ordered]@{ id = $c.id; erro = $erro }) }
}

# ------------------------------------------------------------- cobertura -----

$cobertos = @{}
foreach ($c in $todos) { foreach ($nid in (Arr $c.irRef.nodeIds)) { $cobertos[$nid] = $true } }
$emEscopo = @($inScopeNode.Keys)
$semCard = @($emEscopo | Where-Object { -not $cobertos.ContainsKey($_) } | ForEach-Object {
    [ordered]@{ nodeId = $_; processo = $procOf[$_]; nome = $(if ($nodeOf[$_].displayName) { $nodeOf[$_].displayName } else { $nodeOf[$_].name }); kind = $nodeOf[$_].kind }
})

$porEtapa = [ordered]@{}
foreach ($n in 1..7) {
    $cs = @($todos | Where-Object { $n -in @($_.fulfills.etapas) })
    $porEtapa["etapa $n"] = [ordered]@{
        nome = @($conf.etapas | Where-Object { [int]$_.n -eq $n })[0].name
        build = @($cs | Where-Object { $_.cardType -eq 'build' }).Count
        validation = @($cs | Where-Object { $_.cardType -eq 'validation' }).Count
        double = @($cs | Where-Object { $_.cardType -eq 'double' }).Count
    }
}

$conceitosCobertos = @{}
foreach ($c in $todos) { foreach ($x in (Arr $c.fulfills.concepts)) { $conceitosCobertos[$x] = $true } }
$conceitosSemCard = @(Arr $conf.concepts | Where-Object { -not $conceitosCobertos.ContainsKey($_.id) } | ForEach-Object {
    [ordered]@{ id = $_.id; nome = $_.name; ocorrencias = $_.occurrences }
})

# Um resultado esperado so esta coberto quando TODOS os seus conceitos tem card.
# Bastar um deles dava por provada 'Criacao dinamica de etapas' com metade da
# prova - o duble da interface feito, o graft step por fazer.
$resultadosCobertos = @{}
foreach ($c in $todos) { foreach ($r in (Arr $c.fulfills.expectedResults)) { $resultadosCobertos[$r] = $true } }
$resultadosSemCard = @(Arr $conf.expectedResults | Where-Object {
        $emFalta = @(Arr $_.concepts | Where-Object { -not $conceitosCobertos.ContainsKey($_) })
        (-not $resultadosCobertos.ContainsKey($_.id)) -or $emFalta.Count -gt 0
    } | ForEach-Object {
    [ordered]@{
        id = $_.id; texto = $_.text; conceitos = @(Arr $_.concepts)
        conceitosSemCard = @(Arr $_.concepts | Where-Object { -not $conceitosCobertos.ContainsKey($_) })
    }
})

$idx = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/backlog/v1'
    package = $Package
    note = 'Backlog projectado dos artefactos. A unidade de trabalho e o SEGMENTO da jornada: tem oraculo pronto (o troco do cenario) e etapa exacta, e o detalhe fino desce para content.checklist. Nada aqui e criativo - card, ordem, dependencia, bloqueador e cobertura sao leituras de factos ja extraidos. O julgamento entra apenas por analysis/backlog-review.json, como parecer anexo, e nunca altera uma contagem.'
    generatedAt = $geradoEm
    manifestSha256 = $manifestSha
    parecerAgentico = [ordered]@{
        ficheiro = 'analysis/backlog-review.json'
        presente = [bool]$review
        sha256 = $reviewSha
        pareceres = @(Arr $review.pareceres).Count
        aplicados = @($todos | Where-Object { $_.provenance.reviewSha256 }).Count
        nota = 'Entrada do bundle, nao saida: pode alterar titulo e hipoteses, logo e fixada por sha256 como o XPDL e o glossario. Mesmas fontes + mesmo glossario + mesmo parecer = mesmo backlog.'
    }
    summary = [ordered]@{
        total = $todos.Count
        build = $build.Count
        validation = $valid.Count
        double = $doubles.Count
        invalidos = $invalidos.Count
        nosEmEscopo = $emEscopo.Count
        nosComCard = @($cobertos.Keys | Where-Object { $inScopeNode.ContainsKey($_) }).Count
        nosSemCard = $semCard.Count
        conceitosTocados = "$($conceitosCobertos.Count)/$(@($conf.concepts).Count)"
        conceitosSemCard = $conceitosSemCard.Count
        resultadosSemCard = $resultadosSemCard.Count
    }
    porPeso = [ordered]@{}
    atravessamOSistema = @($todos | Where-Object { $_.dimensao.peso -eq 'atravessa-o-sistema' } | ForEach-Object {
        [ordered]@{ id = $_.id; camadas = $_.dimensao.camadas; pastasPorEscrever = $_.dimensao.aEscrever; nos = $_.dimensao.nos; title = $_.title }
    })
    porEtapa = $porEtapa
    conceitosSemCard = @($conceitosSemCard)
    resultadosSemCard = @($resultadosSemCard)
    nosSemCard = @($semCard)
    invalidos = @($invalidos)
    cards = @($todos | ForEach-Object {
        [ordered]@{
            id = $_.id; cardType = $_.cardType; epic = $_.epic; title = $_.title
            etapas = @($_.fulfills.etapas); nos = @(Arr $_.irRef.nodeIds).Count
            peso = $_.dimensao.peso; camadas = $_.dimensao.camadas; pastasPorEscrever = $_.dimensao.aEscrever
            oraculo = $_.acceptance.oracle.kind; casos = $_.acceptance.oracle.caseCount
            bloqueadores = @(Arr $_.gaps | ForEach-Object { $_.construct })
            dependsOn = @(Arr $_.dependsOn)
        }
    })
}
foreach ($g in ($todos | Group-Object { $_.dimensao.peso } | Sort-Object Name)) { $idx.porPeso[$g.Name] = $g.Count }
$idx | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $OutDir 'index.json') -Encoding UTF8

Write-Host ("Wrote {0}  ({1} cards: {2} build, {3} validation, {4} double; {5}/{6} nos em escopo com card; {7} resultado(s) esperado(s) sem card)" -f `
    $OutDir, $idx.summary.total, $idx.summary.build, $idx.summary.validation, $idx.summary.double, `
    $idx.summary.nosComCard, $idx.summary.nosEmEscopo, $idx.summary.resultadosSemCard)
if ($invalidos.Count -gt 0) {
    Write-Host ("    {0} card(s) NAO validaram contra o schema e nao foram escritos - ver index.json > invalidos" -f $invalidos.Count) -ForegroundColor Red
}
if ($semCard.Count -gt 0) {
    Write-Host ("    {0} no(s) em escopo sem card - ver index.json > nosSemCard" -f $semCard.Count) -ForegroundColor Yellow
}
$grandes = @($todos | Where-Object { $_.dimensao.peso -eq 'atravessa-o-sistema' })
if ($grandes.Count -gt 0) {
    Write-Host ("    {0} card(s) atravessam o sistema (3+ projectos) - validos, mas nao sao trabalho para uma pessoa so" -f $grandes.Count) -ForegroundColor Yellow
}
