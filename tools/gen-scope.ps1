<#
.SYNOPSIS
    S1.12 - marca cada elemento da IR como dentro ou fora do escopo da POC.

.DESCRIPTION
    A POC nao migra o ePAT completo: valida que a plataforma alvo executa o cenario
    representativo. Sem uma fronteira mecanica, o backlog encomendaria a migracao
    inteira - 127 operacoes de servico em vez das 5 que o cenario invoca.

    A fronteira e AUTORADA em config/poc-scope.json, transcrita da seccao 1 do
    POC_FULFILLMENT_PLAN. Este gerador nao decide escopo: aplica a fronteira e
    devolve, para cada elemento, se entra e PORQUE. O porque importa tanto como
    o veredicto - um elemento excluido sem motivo rastreavel e escopo perdido,
    nao escopo cortado.

    Este artefacto e a porta do backlog: elemento fora nao gera card de
    implementacao. O que fica de fora por FALTA DE FONTE, mas que o cenario
    invoca, sai marcado para card de duble - que e trabalho diferente.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$ScopePath    = "$PSScriptRoot/../config/poc-scope.json",
    [string]$OutPath      = "$PSScriptRoot/../artifacts/POC_Epat/scope.json"
)

$ErrorActionPreference = 'Stop'

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) { if ($Optional) { return $null }; throw "artifact not found: $p" }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

$scope    = Get-Content $ScopePath -Raw -Encoding UTF8 | ConvertFrom-Json
$model    = Read-Artifact 'process-model.json'
$fields   = Read-Artifact 'case-field-dictionary.json'
$services = Read-Artifact 'service-contracts.json'
$catalog  = Read-Artifact 'rule-catalogue.json'  -Optional
$screens  = Read-Artifact 'screen-rules.json'    -Optional
$conf     = Read-Artifact 'conformance.json'     -Optional

$included = @($scope.incluido.processes | ForEach-Object { $_.name })
$papelOf  = @{}
foreach ($p in $scope.incluido.processes) { $papelOf[$p.name] = $p.papel }

$items = [System.Collections.Generic.List[object]]::new()
function Add-Item {
    param([string]$Kind, [string]$Id, [string]$Name, [bool]$In, [string]$Reason, [string]$ExclusionId = '', [string]$Owner = '', [string]$Work = 'implementacao')
    $items.Add([ordered]@{
        kind = $Kind; id = $Id; name = $Name; process = $Owner
        inScope = $In; reason = $Reason
        exclusionId = $(if ($ExclusionId) { $ExclusionId } else { $null })
        workKind = $(if ($In) { $Work } else { $null })
    })
}

# ------------------------------------------------------------- processos ----

foreach ($p in $model.processes) {
    $in = $p.name -in $included
    Add-Item -Kind 'process' -Id $p.name -Name $p.name -In $in -Owner $p.name `
        -Reason $(if ($in) { "listado na seccao 1 do plano: $($papelOf[$p.name])" } else { 'nao consta da lista de processos incluidos' }) `
        -ExclusionId $(if ($in) { '' } else { 'atividade-anterior-ao-cenario' })
}

# ------------------------------------------------------------------ nos ----

# O no herda o escopo do processo: se o processo esta dentro, o cenario passa por ele.
foreach ($p in $model.processes) {
    $in = $p.name -in $included
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) {
            $label = $(if ($n.displayName) { $n.displayName } else { $n.name })
            if (-not $label) { $label = "$($n.kind) $($n.id)" }
            Add-Item -Kind 'node' -Id $n.id -Name $label -In $in -Owner $p.name `
                -Reason $(if ($in) { "pertence a $($p.name), incluido no cenario" } else { "pertence a $($p.name), fora do cenario" }) `
                -ExclusionId $(if ($in) { '' } else { 'atividade-anterior-ao-cenario' })
        }
    }
}

# ------------------------------------------------------------- operacoes ----

# A maior reducao do escopo esta aqui: o WSDL descreve o ePAT inteiro.
$invoked = @{}
foreach ($o in (Arr $services.invokedOperations)) { $invoked[$o] = $true }

foreach ($svc in (Arr $services.services)) {
    foreach ($op in (Arr $svc.operations)) {
        $in = $invoked.ContainsKey($op.name)
        Add-Item -Kind 'operation' -Id $op.name -Name $op.operationName -In $in -Owner $svc.file `
            -Reason $(if ($in) { 'invocada por um serviceTask de um processo incluido' } else { 'catalogada no WSDL mas nunca invocada pelo cenario' }) `
            -ExclusionId $(if ($in) { '' } else { 'operacoes-nao-invocadas' })
    }
}

# --------------------------------------------------------------- campos ----

foreach ($f in (Arr $fields.fields)) {
    $uses = @(Arr $f.readBy).Count + @(Arr $f.writtenBy).Count + @(Arr $f.usedInConditions).Count +
            @(Arr $f.boundToService).Count + @(Arr $f.boundToSubProcess).Count +
            @(Arr $f.usedInForm).Count + @(Arr $f.usedInEmail).Count
    $in = $uses -gt 0
    Add-Item -Kind 'field' -Id $f.name -Name $f.name -In $in `
        -Reason $(if ($in) { "referenciado em $uses ponto(s) do cenario" } else { 'declarado no XPDL mas nunca lido, escrito, testado nem mapeado' }) `
        -ExclusionId $(if ($in) { '' } else { 'campos-nao-referenciados' })
}

foreach ($t in (Arr $fields.technicalFields)) {
    Add-Item -Kind 'technicalField' -Id $t.name -Name $t.name -In $true `
        -Reason 'envelope tecnico exigido pelo cenario, fora do modelo de dominio'
}

# --------------------------------------------------------------- regras ----

foreach ($r in (Arr $catalog.rules)) {
    $proc = $r.processo
    $in = $true; $why = ''; $exc = ''
    if ($r.classification.portador -eq 'corticon') {
        $why = 'planilha do servico de decisao, exercitada na etapa 3'
    }
    elseif ($r.classification.portador -eq 'screenCode') {
        if ($r.classification.eRegraDeNegocio) { $why = 'decisao do code-behind com peso de negocio: determina o desfecho do processo' }
        else { $in = $false; $why = 'mecanica de pagina sem peso de negocio'; $exc = 'decisao-de-tela-tecnica' }
    }
    elseif ($proc -and $proc -notin $included) {
        $in = $false; $why = "vive em $proc, fora do cenario"; $exc = 'atividade-anterior-ao-cenario'
    }
    else {
        $why = "vive em $proc, incluido no cenario"
    }
    Add-Item -Kind 'rule' -Id $r.id -Name $r.classification.efeito -In $in -Owner $proc -Reason $why -ExclusionId $exc
}

# ------------------------------------------------- pacotes nao entregues ----

# Nao desaparecem: o cenario invoca-os, logo viram card de DUBLE e nao de implementacao.
foreach ($prop in $model.externalPackages.PSObject.Properties) {
    Add-Item -Kind 'externalPackage' -Id $prop.Name -Name $prop.Value -In $true `
        -Reason 'referenciado pelo XPDL e nunca entregue; o cenario precisa dele' `
        -Work 'duble'
}

# ------------------------------------------------------------------ write ----

$byKind = [ordered]@{}
foreach ($g in ($items | Group-Object { $_.kind } | Sort-Object Name)) {
    $inside = @($g.Group | Where-Object { $_.inScope }).Count
    $byKind[$g.Name] = [ordered]@{
        total = $g.Count
        dentro = $inside
        fora = $g.Count - $inside
        reducao = "$([Math]::Round(100 * ($g.Count - $inside) / [Math]::Max(1, $g.Count)))%"
    }
}

$byExclusion = [ordered]@{}
foreach ($g in ($items | Where-Object { $_.exclusionId } | Group-Object { $_.exclusionId } | Sort-Object Count -Descending)) {
    $byExclusion[$g.Name] = $g.Count
}

$out = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/scope/v1'
    package   = $Package
    note      = 'Fronteira do escopo da POC aplicada a cada elemento da IR. A fronteira e autorada em config/poc-scope.json, transcrita da seccao 1 do POC_FULFILLMENT_PLAN. Cada elemento traz o motivo do veredicto: um elemento excluido sem motivo rastreavel e escopo perdido, nao escopo cortado. Este artefacto e a porta do backlog.'
    cenario   = $scope.cenario
    conflitoResolvido = $scope.conflitoResolvido
    processosIncluidos = $included
    pocFlowProcessesDetectados = @($conf.etapas | ForEach-Object { $_.processes } | Where-Object { $_ } | Sort-Object -Unique)
    summary = [ordered]@{
        total  = $items.Count
        dentro = @($items | Where-Object { $_.inScope }).Count
        fora   = @($items | Where-Object { -not $_.inScope }).Count
        cardsDeImplementacao = @($items | Where-Object { $_.workKind -eq 'implementacao' }).Count
        cardsDeDuble         = @($items | Where-Object { $_.workKind -eq 'duble' }).Count
        byKind = $byKind
        byExclusion = $byExclusion
    }
    exclusionRules = @($scope.excluido)
    elements = @($items)
}

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$out | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} elementos: {2} dentro, {3} fora do escopo da POC)" -f `
    $OutPath, $out.summary.total, $out.summary.dentro, $out.summary.fora)
foreach ($k in $byKind.Keys) {
    Write-Host ("    {0,-16} {1,4} dentro / {2,4} total   ({3} cortado)" -f $k, $byKind[$k].dentro, $byKind[$k].total, $byKind[$k].reducao) -ForegroundColor DarkGray
}
