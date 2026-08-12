<#
.SYNOPSIS
    S1.8 - inventario das regras de negocio espalhadas pelo XPDL.

.DESCRIPTION
    A planilha Corticon nao e a unica fonte de regra do pacote. O XPDL guarda logica
    de decisao em cinco lugares diferentes, e ate agora so os scriptTasks tinham sido
    catalogados:

      script       corpo dos scriptTasks
      transition   condicao das transicoes (o desvio dos gateways)
      deadline     prazo por expressao, e tambem os SLA fixos escritos no diagrama
      dataMapping  expressao dentro de um mapeamento de parametro
      formScript   script de submissao do formulario

    Tudo o que este gerador extrai e DERIVADO do XPDL. As explicacoes vem de
    config/script-rules-notes.json, sao AUTORADAS e vao marcadas como nao verificadas.

    Cada regra e marcada com inPocFlow, indicando se esta na trilha narrada pelo
    documento da POC (etapas 1-7) ou num subprocesso que a trilha apenas invoca.
#>
[CmdletBinding()]
param(
    [string]$ModelPath       = "$PSScriptRoot/../artifacts/POC_Epat/process-model.json",
    [string]$FieldsPath      = "$PSScriptRoot/../artifacts/POC_Epat/case-field-dictionary.json",
    [string]$ConformancePath = "$PSScriptRoot/../artifacts/POC_Epat/conformance.json",
    [string]$NotesPath       = "$PSScriptRoot/../config/script-rules-notes.json",
    [string]$XpdlPath        = "$PSScriptRoot/../input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl",
    [string]$OutPath         = "$PSScriptRoot/../artifacts/POC_Epat/rule-inventory.json"
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/lib-reading.ps1"
. "$PSScriptRoot/lib-classification.ps1"

$model  = Get-Content $ModelPath  -Raw -Encoding UTF8 | ConvertFrom-Json
$fields = Get-Content $FieldsPath -Raw -Encoding UTF8 | ConvertFrom-Json
$notes  = $(if (Test-Path $NotesPath) { Get-Content $NotesPath -Raw -Encoding UTF8 | ConvertFrom-Json } else { $null })
$fieldIndex = New-FieldIndex $fields

# A trilha narrada pelo documento da POC; fora dela ficam os subprocessos de servico.
$flowProcesses = @()
if (Test-Path $ConformancePath) {
    $conf = Get-Content $ConformancePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $flowProcesses = @($conf.etapas | ForEach-Object { $_.processes } | Where-Object { $_ } | Sort-Object -Unique)
}

$businessFields = @{}
foreach ($f in @($fields.fields)) { $businessFields[$f.name] = $f }

$NotData = @(
    'IPESystemValues', 'IPEStringUtil', 'IPEDateTimeUtil', 'IPEMathUtil', 'IPEConversionUtil',
    'IPEStarterUtil', 'IPEProcessNameUtil',
    'SW_NA', 'SW_DATE', 'SW_TIME', 'SW_CASENUM', 'SW_PRONAME', 'SW_HOSTNAME',
    'STR', 'NUM', 'SEARCH', 'SUBSTR', 'STRLEN', 'CALCTIME', 'DATESTR', 'GETATTRIBUTE', 'GETPROCESSNAME'
)

# ------------------------------------------------------------------ xpdl ----

[xml]$doc = Get-Content $XpdlPath -Raw -Encoding UTF8
$ns = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
$ns.AddNamespace('xpdl2', 'http://www.wfmc.org/2008/XPDL2.1')
$ns.AddNamespace('xpdExt', 'http://www.tibco.com/XPD/xpdExtension1.0.0')

$lineOfId = @{}
$n = 0
foreach ($line in [System.IO.File]::ReadLines($XpdlPath)) {
    $n++
    foreach ($m in [regex]::Matches($line, 'Id="([^"]+)"')) {
        if (-not $lineOfId.ContainsKey($m.Groups[1].Value)) { $lineOfId[$m.Groups[1].Value] = $n }
    }
}

function Get-Line { param([string]$Id) if ($Id -and $lineOfId.ContainsKey($Id)) { return $lineOfId[$Id] } return $null }

# Sobe na arvore ate ao processo dono, para saber onde a regra vive.
function Get-OwnerProcess {
    param($Node)
    $cur = $Node
    while ($cur -and $cur.NodeType -eq 'Element') {
        if ($cur.LocalName -eq 'WorkflowProcess') { return $cur.GetAttribute('Name') }
        $cur = $cur.ParentNode
    }
    return ''
}

function Get-OwnerActivity {
    param($Node)
    $cur = $Node
    while ($cur -and $cur.NodeType -eq 'Element') {
        if ($cur.LocalName -eq 'Activity') {
            $label = $cur.GetAttribute('DisplayName', 'http://www.tibco.com/XPD/xpdExtension1.0.0')
            if (-not $label) { $label = $cur.GetAttribute('Name') }
            return [pscustomobject]@{ Id = $cur.GetAttribute('Id'); Label = $label }
        }
        $cur = $cur.ParentNode
    }
    return $null
}

function Get-Note {
    param([string]$Process, [string]$Node)
    if (-not $notes) { return $null }
    foreach ($key in @("$Process/$Node", "*/$Node")) {
        $p = $notes.rules.PSObject.Properties[$key]
        if ($p) {
            return [ordered]@{
                summary = $p.Value.summary; detail = $p.Value.detail
                findings = @($p.Value.findings); migration = $p.Value.migration
                source = 'autorado - NAO verificado com o cliente'
            }
        }
    }
    return $null
}

function Get-Idents {
    param([string]$Text)
    if (-not $Text) { return @() }
    return @(foreach ($m in [regex]::Matches($Text, '\b([A-Z_][A-Z0-9_]{2,})\b')) { $m.Groups[1].Value }) |
        Sort-Object -Unique | Where-Object { $_ -notin $NotData -and $businessFields.ContainsKey($_) }
}

function New-Rule {
    param(
        [string]$Source, [string]$Process, [string]$Node, [string]$NodeId,
        [string]$Expression, [string[]]$Conditions, $Classification, [string]$Consequence, $Extra
    )
    if ([string]::IsNullOrWhiteSpace($Node)) { $Node = '(passo sem rotulo)' }
    $texto = $(if ($Expression) { $Expression } else { ($Conditions | Sort-Object -Unique) -join ' E ' }).Trim()
    $reads = Get-Idents $texto
    $item = [ordered]@{
        id           = "RI-$Source-$($Process)-$(($Node -replace '[^A-Za-z0-9]', ''))"
        source       = $Source
        process      = $Process
        node         = $Node
        nodeId       = $NodeId
        xpdlLine     = (Get-Line $NodeId)
        inPocFlow    = ($Process -in $flowProcesses)
        classification = $Classification
        expression   = $Expression
        conditions   = @($Conditions)
        readsBusinessFields = @($reads)
        leitura      = $(if ($texto) { New-Reading -Expression $texto -Index $fieldIndex -Fields $reads -Consequence $Consequence } else { $null })
        explanation  = (Get-Note -Process $Process -Node $Node)
    }
    if ($Extra) { foreach ($k in $Extra.Keys) { $item[$k] = $Extra[$k] } }
    return $item
}

$all = [System.Collections.Generic.List[object]]::new()

# --------------------------------------------------------------- scripts ----

foreach ($proc in $model.processes) {
    foreach ($scope in $proc.scopes) {
        foreach ($node in $scope.nodes) {
            if ($node.kind -ne 'scriptTask' -or -not $node.script) { continue }
            $body = [string]$node.script.body
            if ([string]::IsNullOrWhiteSpace($body)) { continue }
            $label = $(if ($node.displayName) { $node.displayName } else { $node.name })

            $conds = @(foreach ($m in [regex]::Matches($body, '\bif\s*\(([^{;]*)\)')) {
                ($m.Groups[1].Value -replace '\s+', ' ').Trim()
            })
            $writes = @(foreach ($m in [regex]::Matches($body, '(?m)^\s*([A-Z_][A-Z0-9_]*)\s*(?:\[[^\]]*\])?\s*=[^=]')) { $m.Groups[1].Value }) | Sort-Object -Unique
            $literals = @(foreach ($m in [regex]::Matches($body, '([A-Z_][A-Z0-9_]*)\s*(?:\[[^\]]*\])?\s*=\s*''([^'']*)''|([A-Z_][A-Z0-9_]*)\s*(?:\[[^\]]*\])?\s*=\s*"([^"]*)"')) {
                $fn = $(if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $m.Groups[3].Value })
                $vl = $(if ($m.Groups[2].Success) { $m.Groups[2].Value } else { $m.Groups[4].Value })
                [ordered]@{ field = $fn; value = $vl }
            })
            $comments = @(foreach ($m in [regex]::Matches($body, '(?m)^\s*//(.*)$')) { $m.Groups[1].Value.Trim() }) | Where-Object { $_ }
            $reads = Get-Idents ($conds -join ' ')
            $escreve = @($writes | Where-Object { $businessFields.ContainsKey($_) })
            $cls = New-Classification -Portador 'script' -LeCamposDoCaso ($reads.Count -gt 0) `
                -TemCondicao ($conds.Count -gt 0) -EscreveCampoDoCaso ($escreve.Count -gt 0)

            $all.Add((New-Rule -Source 'script' -Process $proc.name -Node $label -NodeId $node.id `
                -Expression '' -Conditions $conds -Classification $cls `
                -Consequence $(if (@($writes).Count) { 'Escreve ' + (@($writes) -join ', ') + '.' } else { '' }) `
                -Extra ([ordered]@{
                    metrics = [ordered]@{ conditions = $conds.Count; assignments = @($writes).Count; lines = ($body -split "`n").Count }
                    writesBusinessFields = $escreve
                    valueDomain = @($literals)
                    authorComments = @($comments)
                })))
        }
    }
}

# ----------------------------------------------------------- transicoes ----

foreach ($proc in $model.processes) {
    foreach ($scope in $proc.scopes) {
        $byId = @{}
        foreach ($nd in $scope.nodes) { $byId[$nd.id] = $nd }
        foreach ($edge in $scope.edges) {
            if (-not $edge.condition) { continue }
            $from = $byId[$edge.from]
            $label = $(if ($from) { $(if ($from.displayName) { $from.displayName } else { $from.name }) } else { $edge.from })
            if ([string]::IsNullOrWhiteSpace($label)) { $label = "gateway $($edge.from)" }
            $reads = Get-Idents $edge.condition
            $cls = New-Classification -Portador 'transition' -LeCamposDoCaso ($reads.Count -gt 0) -TemCondicao $true
            $destino = $(if ($byId[$edge.to]) { $(if ($byId[$edge.to].displayName) { $byId[$edge.to].displayName } else { $byId[$edge.to].name }) } else { $edge.to })
            if ([string]::IsNullOrWhiteSpace($destino)) {
                $nd = $byId[$edge.to]
                $destino = $(if ($nd) { "$($nd.kind) sem rotulo ($($edge.to))" } else { $edge.to })
            }
            $all.Add((New-Rule -Source 'transition' -Process $proc.name -Node $label -NodeId $edge.from `
                -Expression $edge.condition -Conditions @($edge.condition) -Classification $cls `
                -Consequence "Quando verdadeiro, o fluxo segue por $(if ($edge.label) { "'$($edge.label)'" } else { 'este ramo' }) para $destino." `
                -Extra ([ordered]@{
                    branchLabel = $edge.label
                    leadsTo = $destino
                })))
        }
    }
}

# ------------------------------------------------------------- deadlines ----

foreach ($dl in $doc.SelectNodes('//xpdl2:Deadline', $ns)) {
    $act = Get-OwnerActivity $dl
    $proc = Get-OwnerProcess $dl
    $dur = $dl.SelectSingleNode('xpdl2:DeadlineDuration', $ns)
    $expr = ''; $tipo = 'constante'
    if ($dur) {
        $const = $dur.SelectSingleNode('xpdExt:ConstantPeriod', $ns)
        if ($const) {
            $partes = @()
            foreach ($a in $const.Attributes) { $partes += "$($a.Name)=$($a.Value)" }
            $expr = ($partes -join ' ')
            $tipo = 'constante'
        }
        else {
            $expr = (($dur.InnerText -replace '\s+', ' ')).Trim()
            $tipo = 'expressao'
        }
    }
    if (-not $expr) { continue }
    $nomePasso = $(if ($act) { $act.Label } else { '(sem passo)' })
    # O passo chamado Pause e o intervalo entre tentativas do laco de retry, nao um prazo de negocio.
    $retry = ($tipo -eq 'constante' -and $nomePasso -match '^\s*Pause\s*$')
    $cls = New-Classification -Portador 'deadline' -LeCamposDoCaso ((Get-Idents $expr).Count -gt 0) -EsperaDeRetentativa $retry
    $all.Add((New-Rule -Source 'deadline' -Process $proc -Node $nomePasso `
        -NodeId $(if ($act) { $act.Id } else { '' }) -Expression $expr -Conditions @() -Classification $cls `
        -Consequence 'Esgotado o prazo, o evento de fronteira dispara e desvia o fluxo.' `
        -Extra ([ordered]@{
            deadlineKind = $tipo
        })))
}

# ---------------------------------------------------------- data mapping ----

foreach ($actual in $doc.SelectNodes('//xpdl2:Actual', $ns)) {
    $v = (($actual.InnerText -replace '\s+', ' ')).Trim()
    if ([string]::IsNullOrWhiteSpace($v)) { continue }
    if ($v -match '^[A-Z_][A-Z0-9_]*(\[[^\]]*\])?$') { continue }   # passagem simples de campo
    $act = Get-OwnerActivity $actual
    $proc = Get-OwnerProcess $actual
    $reads = Get-Idents $v
    $temLogica = ($v -match '\bif\s*\(' -or $v -match '[<>=!]=' -or $v -match '\|\||&&')
    $cls = New-Classification -Portador 'dataMapping' -LeCamposDoCaso ($reads.Count -gt 0) `
        -TemCondicao $temLogica -EscreveCampoDoCaso (-not $temLogica)
    $all.Add((New-Rule -Source 'dataMapping' -Process $proc -Node $(if ($act) { $act.Label } else { '(sem passo)' }) `
        -NodeId $(if ($act) { $act.Id } else { '' }) -Expression $v -Conditions @() -Classification $cls `
        -Consequence "O resultado desta expressao e o valor entregue ao parametro $($actual.ParentNode.GetAttribute('Formal'))." `
        -Extra ([ordered]@{
            formalParam = $actual.ParentNode.GetAttribute('Formal')
            direction   = $actual.ParentNode.GetAttribute('Direction')
        })))
}

# ---------------------------------------------------------- form script ----

foreach ($fs in $doc.SelectNodes('//xpdExt:SubmitScript', $ns)) {
    $act = Get-OwnerActivity $fs
    $proc = Get-OwnerProcess $fs
    $v = (($fs.InnerText -replace '\s+', ' ')).Trim()
    if (-not $v) { continue }
    $cls = New-Classification -Portador 'formScript' -LeCamposDoCaso ((Get-Idents $v).Count -gt 0) `
        -EscreveCampoDoCaso $true
    $all.Add((New-Rule -Source 'formScript' -Process $proc -Node $(if ($act) { $act.Label } else { '(formulario)' }) `
        -NodeId $(if ($act) { $act.Id } else { '' }) -Expression $v -Conditions @() -Classification $cls `
        -Consequence 'Corre no momento em que o utilizador submete o formulario.' -Extra $null))
}

# ------------------------------------------------------------------ write ----

$bySource = [ordered]@{}
# Group-Object por NOME de propriedade nao enxerga chaves de [ordered]; usar scriptblock.
foreach ($g in ($all | Group-Object { $_.source } | Sort-Object Name)) {
    $bySource[$g.Name] = [ordered]@{
        total          = $g.Count
        regraDeNegocio = @($g.Group | Where-Object { $_.classification.eRegraDeNegocio }).Count
        naTrilhaPoc    = @($g.Group | Where-Object { $_.inPocFlow }).Count
    }
}

$byEfeito = [ordered]@{}
foreach ($g in ($all | Group-Object { $_.classification.efeito } | Sort-Object Name)) { $byEfeito[$g.Name] = $g.Count }

$negocio = @($all | Where-Object { $_.classification.eRegraDeNegocio })

$out = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/rule-inventory/v2'
    package   = $model.source.package
    note      = 'Inventario das regras de negocio espalhadas pelo XPDL, em cinco portadores: script, transition, deadline, dataMapping e formScript. Expressoes e campos sao DERIVADOS. As explicacoes sao AUTORADAS e nao verificadas. A classificacao usa o eixo unico de tools/lib-classification.ps1, partilhado com as telas e com a planilha Corticon.'
    pocFlowProcesses = @($flowProcesses)
    summary   = [ordered]@{
        total            = $all.Count
        regraDeNegocio   = $negocio.Count
        tecnico          = @($all | Where-Object { -not $_.classification.eRegraDeNegocio }).Count
        naTrilhaPoc      = @($negocio | Where-Object { $_.inPocFlow }).Count
        foraDaTrilhaPoc  = @($negocio | Where-Object { -not $_.inPocFlow }).Count
        comExplicacao    = @($all | Where-Object { $null -ne $_.explanation }).Count
        byEfeito         = $byEfeito
        bySource         = $bySource
    }
    rules = @($all | Sort-Object @{ e = { -[int]$_.inPocFlow } }, source, process, node)
}

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$out | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} portadores: {2} de negocio, {3} tecnicos; {4} na trilha da POC)" -f `
    $OutPath, $out.summary.total, $out.summary.regraDeNegocio, $out.summary.tecnico, $out.summary.naTrilhaPoc)
