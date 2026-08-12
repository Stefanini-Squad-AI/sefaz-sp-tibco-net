#requires -version 5
<#
  Generates artifacts/decision-tables.json

  Exports the Progress Corticon rulesheet 'intimacoes_Parametros.ers' as a plain
  decision table: 21 condition rows x 49 rule columns plus the action cells, the
  vocabulary it is written against, and the mapping back to iProcess case fields
  through the PrepararIntimacao service call.

  Corticon layout inside the .ers:
    ruleset/rule[0]      -> the definition column (no cells)
    ruleset/rule[1..49]  -> one column per rule; 23 positional <condition> cells
                            (empty element = "don't care") + a list of <action>s
    ruleset/ruleStatement -> the human readable name of each rule column
#>
param(
    [string]$ErsPath = "$PSScriptRoot\..\input\Arquivos Poc Camunda\intimacoes_Parametros.ers",
    [string]$ContractsPath = "$PSScriptRoot\..\artifacts\service-contracts.json",
    [string]$OutPath = "$PSScriptRoot\..\artifacts\decision-tables.json"
)
$ErrorActionPreference = 'Stop'
[xml]$ers = Get-Content -LiteralPath $ErsPath -Raw
$rs = $ers.DocumentElement.SelectSingleNode('ruleset')

# A Corticon cell is either a single literal ('1') or a set literal ({'1', '8'}),
# which means "value IN set". Normalise both into matchType + values[].
function Get-Match([string]$rhs) {
    if ($null -eq $rhs) { return $null }
    $t = $rhs.Trim()
    if ($t -match '^\{(.*)\}$') {
        $vals = @($Matches[1] -split ',' | ForEach-Object { $_.Trim().Trim("'") } | Where-Object { $_ -ne '' })
        return [ordered]@{ matchType = 'inSet'; values = $vals }
    }
    [ordered]@{ matchType = 'equals'; values = @($t.Trim("'")) }
}

function Get-Expr($cell) {
    $oe = $cell.SelectSingleNode('opaqueExpression')
    if (-not $oe) { return $null }
    $ve = $cell.SelectSingleNode('viewExpressions')
    [ordered]@{
        expression = $oe.GetAttribute('expression')
        lhs        = if ($ve) { $ve.GetAttribute('lhs') } else { $null }
        rhs        = if ($ve) { $ve.GetAttribute('rhs') } else { $null }
        datatype   = $oe.SelectSingleNode('parserOutput').GetAttribute('datatype')
    }
}

# ------------------------------------------------------------- vocabulary
$vocab = @{}
foreach ($t in $ers.SelectNodes('//terms')) {
    if ($t.GetAttribute('termtype') -ne 'ATTRIBUTE') { continue }
    $full = $t.GetAttribute('fulltext')
    if (-not $vocab.ContainsKey($full)) {
        $parent = $t.SelectSingleNode('parentTerm')
        $vocab[$full] = [ordered]@{
            path      = $full
            attribute = $t.GetAttribute('text')
            entity    = if ($parent) { $parent.GetAttribute('datatype') } else { $null }
            datatype  = $t.GetAttribute('datatype')
            role      = $(if ($full -match '\.response\.') { 'output' } elseif ($full -match '\.request\.|^Request\.') { 'input' } else { 'input' })
            clrType   = $(switch ($t.GetAttribute('datatype')) {
                    'String' { 'string' } 'Integer' { 'int' } 'Decimal' { 'decimal' }
                    'Boolean' { 'bool' } 'DateTime' { 'DateTime' } 'Date' { 'DateOnly' }
                    default { 'string' }
                })
        }
    }
}

# ------------------------------------------------------------- rule statements
$statementFor = @{}
foreach ($st in $rs.SelectNodes('ruleStatement')) {
    $ref = $st.GetAttribute('ruleModelElements')   # e.g. #//@ruleset/@rules.1
    if ($ref -match '@rules\.(\d+)') {
        $txt = $st.SelectSingleNode('text')
        $statementFor[[int]$Matches[1]] = [ordered]@{
            post = $st.GetAttribute('post')
            text = if ($txt) { $txt.GetAttribute('expression') } else { $null }
        }
    }
}

# ------------------------------------------------------------- rules
$ruleNodes = @($rs.SelectNodes('rule'))
$rowLhs = @{}          # rowIndex -> lhs
$rules = @()
for ($i = 0; $i -lt $ruleNodes.Count; $i++) {
    $rn = $ruleNodes[$i]
    $condCells = @($rn.SelectNodes('condition'))
    if ($condCells.Count -eq 0) { continue }   # column 0 / definition column

    $conds = @()
    for ($r = 0; $r -lt $condCells.Count; $r++) {
        $e = Get-Expr $condCells[$r]
        if (-not $e) { continue }
        if (-not $rowLhs.ContainsKey($r)) { $rowLhs[$r] = $e.lhs }
        $mt = Get-Match $e.rhs
        $conds += [ordered]@{ row = $r; lhs = $e.lhs; rhs = $e.rhs; matchType = $mt.matchType; values = $mt.values; expression = $e.expression }
    }
    $acts = @()
    foreach ($ac in $rn.SelectNodes('action')) {
        $e = Get-Expr $ac
        if (-not $e) { continue }
        $lhs = $null; $rhs = $null
        if ($e.expression -match '^\s*(.+?)\s*=\s*(.+?)\s*$') { $lhs = $Matches[1]; $rhs = $Matches[2] }
        $acts += [ordered]@{ lhs = $lhs; rhs = $rhs; expression = $e.expression }
    }
    $st = $statementFor[$i]
    $rules += [ordered]@{
        column     = $i
        name       = if ($st) { $st.text } else { $null }
        post       = if ($st) { $st.post } else { $null }
        conditions = $conds
        actions    = $acts
    }
}

# ------------------------------------------------------------- rectangular table
$rowOrder = @($rowLhs.Keys | Sort-Object)
$conditionRows = @()
foreach ($r in $rowOrder) {
    $vals = @($rules | ForEach-Object { ($_.conditions | Where-Object row -eq $r).values } | Where-Object { $_ } | Sort-Object -Unique)
    $v = $vocab[$rowLhs[$r]]
    $conditionRows += [ordered]@{
        row = $r; lhs = $rowLhs[$r]
        datatype = if ($v) { $v.datatype } else { $null }
        clrType = if ($v) { $v.clrType } else { 'string' }
        distinctValues = $vals
        usedByRuleCount = @($rules | Where-Object { ($_.conditions | Where-Object row -eq $r) }).Count
    }
}
$actionLhs = @($rules | ForEach-Object { $_.actions.lhs } | Where-Object { $_ } | Sort-Object -Unique)
$actionRows = @()
foreach ($a in $actionLhs) {
    $v = $vocab[$a]
    $actionRows += [ordered]@{
        lhs = $a
        datatype = if ($v) { $v.datatype } else { $null }
        clrType = if ($v) { $v.clrType } else { 'string' }
        distinctValues = @($rules | ForEach-Object { ($_.actions | Where-Object lhs -eq $a).rhs } | Where-Object { $_ } | Sort-Object -Unique)
        setByRuleCount = @($rules | Where-Object { ($_.actions | Where-Object lhs -eq $a) }).Count
    }
}

# grid: one entry per rule column, '-' means don't care
$grid = @()
foreach ($rule in $rules) {
    $cells = [ordered]@{}
    foreach ($r in $rowOrder) {
        $c = $rule.conditions | Where-Object row -eq $r
        $cells[[string]$rowLhs[$r]] = $(if ($c) { $c.rhs } else { '-' })
    }
    $sets = [ordered]@{}
    foreach ($a in $rule.actions) { if ($a.lhs) { $sets[[string]$a.lhs] = $a.rhs } }
    $grid += [ordered]@{ column = $rule.column; name = $rule.name; when = $cells; then = $sets }
}

# ------------------------------------------------------------- case-field mapping

# Which service call carries this rulesheet cannot be found by WSDL filename - that
# name is package-specific. It is found by evidence instead: the binding whose SOAP
# path leaves match this rulesheet's own vocabulary attributes. If nothing matches,
# the call site is left unknown rather than asserted.
$caseFieldMapping = @()
$decisionBinding = $null
if (Test-Path -LiteralPath $ContractsPath) {
    $sc = Get-Content -LiteralPath $ContractsPath -Raw | ConvertFrom-Json

    $vocabLeaves = [System.Collections.Generic.HashSet[string]]::new(
        [string[]]@(@($conditionRows.lhs) + @($actionRows.lhs) | Where-Object { $_ } | ForEach-Object { ($_ -split '\.')[-1] }),
        [StringComparer]::OrdinalIgnoreCase)

    $best = $null; $bestScore = 0
    foreach ($b in $sc.processBindings) {
        $leaves = @(@($b.inputs) + @($b.outputs) | Where-Object { $_.soapPath } |
                    ForEach-Object { ($_.soapPath -split '/')[-1] })
        if ($leaves.Count -eq 0) { continue }
        $hits = @($leaves | Where-Object { $vocabLeaves.Contains($_) }).Count
        if ($hits -gt $bestScore) { $bestScore = $hits; $best = $b }
    }

    # A couple of coincidental name collisions should not be read as the decision call.
    if ($best -and $bestScore -ge 3) {
        $decisionBinding = $best
        foreach ($m in $best.inputs) { $caseFieldMapping += [ordered]@{ direction = 'IN'; caseField = $m.caseField; soapPath = $m.soapPath } }
        foreach ($m in $best.outputs) { $caseFieldMapping += [ordered]@{ direction = 'OUT'; caseField = $m.caseField; soapPath = $m.soapPath } }
    }
}

$invokedVia = if ($decisionBinding) {
    "$($decisionBinding.wsdl) :: $($decisionBinding.operationName), called from XPDL activity $($decisionBinding.node) inside process $($decisionBinding.process)."
}
else {
    $null
}

$doc = [ordered]@{
    '$schema'   = 'sefaz-sp/tibco-intermediate/decision-tables/v1'
    source      = [ordered]@{
        file       = (Split-Path $ErsPath -Leaf)
        engine     = 'Progress Corticon'
        version    = "$($ers.DocumentElement.GetAttribute('majorVersion')).$($ers.DocumentElement.GetAttribute('minorVersion')).$($ers.DocumentElement.GetAttribute('majorService')).$($ers.DocumentElement.GetAttribute('minorService')) build $($ers.DocumentElement.GetAttribute('buildNumber'))"
        vocabulary = $ers.DocumentElement.GetAttribute('vocabulary')
        language   = $ers.DocumentElement.GetAttribute('languageCode')
        invokedVia = $invokedVia
    }
    notes       = @(
        'Corticon evaluates ALL matching rule columns, in dependency order, not first-match-wins. Several columns write the same response attribute, so a naive if/else chain will NOT reproduce the behaviour - preserve the column order and let later writes override.',
        "A '-' cell means don't care, not false.",
        "Condition and action values are string literals ('110', '2', ...) even where they look numeric; they are ePAT domain codes (pecas, textos, prazos).",
        'The rulesheet has no explicit default column; if no rule fires, the response attributes stay unset, which surfaces in iProcess as SW_NA.'
    )
    statistics  = [ordered]@{
        ruleCount        = $rules.Count
        conditionRowCount = $conditionRows.Count
        actionRowCount   = $actionRows.Count
        actionCellCount  = ($rules | ForEach-Object { $_.actions.Count } | Measure-Object -Sum).Sum
        vocabularyTermCount = $vocab.Count
    }
    vocabulary  = @($vocab.Values | Sort-Object { $_.path })
    conditionRows = $conditionRows
    actionRows  = $actionRows
    caseFieldMapping = $caseFieldMapping
    decisionTable = $grid
    rules       = $rules
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutPath) | Out-Null
$doc | ConvertTo-Json -Depth 25 | Set-Content -LiteralPath $OutPath -Encoding UTF8
Write-Host "Wrote $OutPath  ($($rules.Count) rules x $($conditionRows.Count) conditions, $($doc.statistics.actionCellCount) action cells)"
