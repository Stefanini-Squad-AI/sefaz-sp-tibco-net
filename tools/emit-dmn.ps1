<#
.SYNOPSIS
    S4 - emits DMN 1.3 from the Corticon rulesheet, for analyst review.

.DESCRIPTION
    The hard problem here is semantic, not syntactic.

    Corticon does NOT do first-match-wins. Every matching column fires, in column
    order, and a later column that writes the same response attribute overrides the
    earlier one. DMN's RULE ORDER hit policy preserves ordering but returns a LIST -
    it does not express the override fold. An analyst reading a RULE ORDER table with
    a first-match mental model approves the wrong behaviour, silently.

    This emitter solves that instead of annotating around it. Given that
      (a) conditions read only request.* attributes,
      (b) actions write only response.* attributes,
      (c) so no action can affect any condition (verified at runtime, not assumed),
    the fold collapses per attribute: the value of response.X is whatever the LAST
    matching column that writes X set it to. Reverse the column order and "last match
    wins" becomes "first match wins", which is DMN hit policy FIRST - standard,
    unambiguous, and impossible to misread.

    So the primary output is one single-output decision per response attribute,
    hit policy FIRST, containing only the columns that write that attribute, in
    reversed Corticon order, each rule annotated with its original column number.

    A second file mirrors the untransformed 49x21 sheet with RULE ORDER, purely for
    1:1 traceability against the original .ers. It is explicitly marked as NOT the
    reviewable representation.

    If precondition (c) ever fails, the decomposition is unsound and this script
    refuses to emit it rather than producing a plausible-looking wrong answer.
#>
[CmdletBinding()]
param(
    [string]$DecisionsPath = "$PSScriptRoot/../artifacts/POC_Epat/decision-tables.json",
    [string]$GlossaryPath  = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$OutDir        = "$PSScriptRoot/../artifacts/POC_Epat/dmn",
    [string]$Package       = ''
)

$ErrorActionPreference = 'Stop'

$DmnNs = 'xmlns="https://www.omg.org/spec/DMN/20191111/MODEL/" ' +
         'xmlns:dmndi="https://www.omg.org/spec/DMN/20191111/DMNDI/" ' +
         'xmlns:dc="http://www.omg.org/spec/DMN/20180521/DC/" ' +
         'xmlns:di="http://www.omg.org/spec/DMN/20180521/DI/"'

# ------------------------------------------------------------------- load ----

if (-not (Test-Path $DecisionsPath)) { throw "decision-tables.json not found: $DecisionsPath" }
$dt = Get-Content $DecisionsPath -Raw -Encoding UTF8 | ConvertFrom-Json

if (-not $Package) { $Package = Split-Path (Split-Path $DecisionsPath -Parent) -Leaf }
$sheet = [IO.Path]::GetFileNameWithoutExtension([string]$dt.source.file)
if (-not $sheet) { $sheet = 'rulesheet' }
$sheetId = [regex]::Replace($sheet, '[^A-Za-z0-9_]', '_')
$baseName = $sheet.ToLowerInvariant()
$dmnNamespace = "http://sefaz.sp.gov.br/$($Package.ToLowerInvariant())/dmn"

$glossary = @{}
if (Test-Path $GlossaryPath) {
    $section = ''; $entry = ''
    foreach ($line in (Get-Content $GlossaryPath -Encoding UTF8)) {
        if ($line -match '^([A-Za-z_][A-Za-z0-9_]*):\s*$') { $section = $Matches[1]; $entry = ''; continue }
        if ($line -match '^  ([^\s#][^:]*):\s*$')          { $entry = $Matches[1].Trim('"'); continue }
        if ($line -match '^    ([A-Za-z_]+):\s*(.+?)\s*$' -and $section -and $entry) {
            $val = $Matches[2].Trim('"').Trim("'")
            if ($val) { $glossary["$section|$entry|$($Matches[1])"] = $val }
        }
    }
}

# ---------------------------------------------------------------- helpers ----

function ConvertTo-XmlText {
    param([string]$Text)
    if ($null -eq $Text) { return '' }
    return $Text.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;').Replace('"', '&quot;')
}

function Get-DmnId {
    param([string]$Raw)
    $clean = [regex]::Replace([string]$Raw, '[^A-Za-z0-9_.-]', '_')
    if ($clean -notmatch '^[A-Za-z_]') { $clean = "d_$clean" }
    return $clean
}

function Get-ShortAttr {
    param([string]$Lhs)
    $parts = $Lhs -split '\.'
    return $parts[$parts.Length - 1]
}

# Corticon writes 'NA', '110' as quoted literals even for numeric-looking codes.
# FEEL needs double quotes for strings and bare digits for numbers, and getting
# this wrong changes whether a rule matches at all.
function ConvertTo-FeelLiteral {
    param([string]$Value, [string]$Datatype)
    $v = [string]$Value
    if ($v.StartsWith("'") -and $v.EndsWith("'") -and $v.Length -ge 2) { $v = $v.Substring(1, $v.Length - 2) }
    if ($Datatype -in @('Integer', 'Decimal', 'Number') -and $v -match '^-?\d+(\.\d+)?$') { return $v }
    return '"' + $v.Replace('\', '\\').Replace('"', '\"') + '"'
}

function Get-TypeRef {
    param([string]$Datatype)
    switch ($Datatype) {
        'Integer' { 'number' }
        'Decimal' { 'number' }
        'Boolean' { 'boolean' }
        default   { 'string' }
    }
}

# The Corticon vocabulary is not the ePAT vocabulary. Recovering the case field a
# column really refers to is what lets a business reader recognise the table.
$attrToCaseField = @{}
foreach ($m in $dt.caseFieldMapping) {
    if (-not $m.soapPath) { continue }
    $leaf = ($m.soapPath -split '/')[-1]
    if ($leaf -and -not $attrToCaseField.ContainsKey($leaf)) { $attrToCaseField[$leaf] = $m.caseField }
}

function Get-ColumnLabel {
    param([string]$Lhs)
    $attr = Get-ShortAttr $Lhs
    $caseField = $attrToCaseField[$attr]
    if (-not $caseField) { return $attr }
    $term = $glossary["fields|$caseField|term"]
    if ($term) { return "$term ($attr)" }
    return "$caseField ($attr)"
}

# ------------------------------------------------- soundness precondition ----

$condLhs = @($dt.conditionRows.lhs | Sort-Object -Unique)
$actLhs  = @($dt.actionRows.lhs   | Sort-Object -Unique)
$chained = @($condLhs | Where-Object { $_ -in $actLhs })

if ($chained.Count -gt 0) {
    Write-Host ''
    Write-Host 'ABORTADO: a decomposicao por atributo nao e valida para esta tabela.' -ForegroundColor Red
    Write-Host 'Existem atributos lidos por condicoes E escritos por acoes, ou seja, o Corticon' -ForegroundColor Red
    Write-Host 'encadeia: uma acao pode mudar o resultado de uma condicao avaliada depois.' -ForegroundColor Red
    Write-Host 'Reordenar as colunas deixaria de ser equivalente. Atributos envolvidos:' -ForegroundColor Red
    foreach ($c in $chained) { Write-Host "    $c" -ForegroundColor Red }
    exit 1
}

# ------------------------------------------------------------- generation ----

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
Get-ChildItem -Path $OutDir -Filter '*.dmn' -ErrorAction SilentlyContinue | Remove-Item -Force

$condRowByIndex = @{}
foreach ($cr in $dt.conditionRows) { $condRowByIndex[[int]$cr.row] = $cr }
$actRowByLhs = @{}
foreach ($ar in $dt.actionRows) { $actRowByLhs[$ar.lhs] = $ar }

# Builds one <input> plus the per-rule <inputEntry>. A row the rule does not
# constrain becomes '-', which in DMN means "don't care" - never "false".
function Get-InputEntryText {
    param($Rule, [int]$Row)
    $cond = $Rule.conditions | Where-Object { [int]$_.row -eq $Row } | Select-Object -First 1
    if (-not $cond) { return '-' }
    $dtype = $condRowByIndex[$Row].datatype
    $vals = @($cond.values | ForEach-Object { ConvertTo-FeelLiteral -Value $_ -Datatype $dtype })
    if ($vals.Count -eq 0) { return '-' }
    return ($vals -join ',')   # a FEEL list in an input entry is a disjunction
}

$decisions   = [System.Collections.Generic.List[object]]::new()
$xmlBody     = [System.Collections.Generic.List[string]]::new()
$totalRules  = 0

foreach ($ar in $dt.actionRows) {
    $attr    = Get-ShortAttr $ar.lhs
    $decId   = Get-DmnId "Decision_$attr"
    $tabId   = Get-DmnId "Table_$attr"
    $typeRef = Get-TypeRef $ar.datatype

    # Only the columns that actually write this attribute, last-wins reversed to first-wins.
    $writers = @($dt.rules | Where-Object { $_.actions.lhs -contains $ar.lhs })
    [array]::Reverse($writers)
    if ($writers.Count -eq 0) { continue }

    # Only the condition rows those columns actually constrain: 21 columns shrinks
    # to the handful that matter, which is most of what makes the table readable.
    $usedRows = [System.Collections.Generic.List[int]]::new()
    foreach ($r in $writers) { foreach ($c in $r.conditions) { if (-not $usedRows.Contains([int]$c.row)) { $usedRows.Add([int]$c.row) } } }
    $usedRows = @($usedRows | Sort-Object)

    $desc = "Valor de $attr. " +
            "ATENCAO: esta tabela NAO reproduz a ordem original do Corticon. No Corticon todas as colunas que casam disparam, " +
            "em ordem, e uma escrita posterior sobrepoe a anterior - portanto vale a ULTIMA coluna que escreve $attr. " +
            "Aqui as colunas foram INVERTIDAS e a politica e FIRST, o que e equivalente e nao admite leitura ambigua. " +
            "A anotacao de cada linha traz o numero da coluna original no Corticon. " +
            "Se nenhuma linha casar, $attr fica sem valor, o que no iProcess aparece como SW_NA."

    $xmlBody.Add("  <decision id=`"$decId`" name=`"$(ConvertTo-XmlText $attr)`">")
    $xmlBody.Add("    <description>$(ConvertTo-XmlText $desc)</description>")
    $xmlBody.Add("    <question>$(ConvertTo-XmlText "Qual o valor de $attr?")</question>")
    $xmlBody.Add("    <variable id=`"$(Get-DmnId "Var_$attr")`" name=`"$(ConvertTo-XmlText $attr)`" typeRef=`"$typeRef`" />")
    $xmlBody.Add("    <decisionTable id=`"$tabId`" hitPolicy=`"FIRST`">")

    foreach ($row in $usedRows) {
        $cr = $condRowByIndex[$row]
        $xmlBody.Add("      <input id=`"$(Get-DmnId "In_${attr}_$row")`" label=`"$(ConvertTo-XmlText (Get-ColumnLabel $cr.lhs))`">")
        $xmlBody.Add("        <inputExpression id=`"$(Get-DmnId "InExpr_${attr}_$row")`" typeRef=`"$(Get-TypeRef $cr.datatype)`">")
        $xmlBody.Add("          <text>$(ConvertTo-XmlText $cr.lhs)</text>")
        $xmlBody.Add('        </inputExpression>')
        $xmlBody.Add('      </input>')
    }
    $xmlBody.Add("      <output id=`"$(Get-DmnId "Out_$attr")`" label=`"$(ConvertTo-XmlText (Get-ColumnLabel $ar.lhs))`" name=`"$(ConvertTo-XmlText $attr)`" typeRef=`"$typeRef`" />")
    $xmlBody.Add("      <annotation name=`"coluna Corticon`" />")

    foreach ($r in $writers) {
        $ruleId = Get-DmnId "Rule_${attr}_$($r.column)"
        $xmlBody.Add("      <rule id=`"$ruleId`">")
        $xmlBody.Add("        <description>$(ConvertTo-XmlText "coluna $($r.column) - $($r.name)")</description>")
        foreach ($row in $usedRows) {
            $txt = Get-InputEntryText -Rule $r -Row $row
            $xmlBody.Add("        <inputEntry id=`"$(Get-DmnId "InE_${attr}_$($r.column)_$row")`"><text>$(ConvertTo-XmlText $txt)</text></inputEntry>")
        }
        $act = $r.actions | Where-Object { $_.lhs -eq $ar.lhs } | Select-Object -First 1
        $lit = ConvertTo-FeelLiteral -Value $act.rhs -Datatype $ar.datatype
        $xmlBody.Add("        <outputEntry id=`"$(Get-DmnId "OutE_${attr}_$($r.column)")`"><text>$(ConvertTo-XmlText $lit)</text></outputEntry>")
        $xmlBody.Add("        <annotationEntry><text>$(ConvertTo-XmlText "col. $($r.column)")</text></annotationEntry>")
        $xmlBody.Add('      </rule>')
        $totalRules++
    }
    $xmlBody.Add('    </decisionTable>')
    $xmlBody.Add('  </decision>')

    $decisions.Add([ordered]@{
        id = $decId; attribute = $attr; typeRef = $typeRef
        rules = $writers.Count; inputs = $usedRows.Count
    })
}

# DRD layout: a plain grid. The decisions are independent, so nothing is lost.
$di = [System.Collections.Generic.List[string]]::new()
$di.Add('  <dmndi:DMNDI>')
$di.Add("    <dmndi:DMNDiagram id=`"DRD_$sheetId`">")
$col = 0; $rowN = 0
foreach ($d in $decisions) {
    $x = 60 + ($col * 220); $y = 60 + ($rowN * 130)
    $di.Add("      <dmndi:DMNShape id=`"Shape_$($d.id)`" dmnElementRef=`"$($d.id)`">")
    $di.Add("        <dc:Bounds height=`"80`" width=`"180`" x=`"$x`" y=`"$y`" />")
    $di.Add('      </dmndi:DMNShape>')
    $col++; if ($col -ge 4) { $col = 0; $rowN++ }
}
$di.Add('    </dmndi:DMNDiagram>')
$di.Add('  </dmndi:DMNDI>')

$out = [System.Collections.Generic.List[string]]::new()
$out.Add('<?xml version="1.0" encoding="UTF-8"?>')
$out.Add("<definitions $DmnNs id=`"Defs_$sheetId`" name=`"$(ConvertTo-XmlText $sheet)`" namespace=`"$dmnNamespace`">")
$out.AddRange($xmlBody)
$out.AddRange($di)
$out.Add('</definitions>')
($out -join "`r`n") | Set-Content -LiteralPath (Join-Path $OutDir "$baseName.dmn") -Encoding UTF8

# ------------------------------------------------- untransformed mirror ----

$mirror = [System.Collections.Generic.List[string]]::new()
$mirror.Add('<?xml version="1.0" encoding="UTF-8"?>')
$mirror.Add("<definitions $DmnNs id=`"Defs_${sheetId}_Espelho`" name=`"$(ConvertTo-XmlText "$sheet - espelho do rulesheet")`" namespace=`"$dmnNamespace`">")
$mirror.Add('  <decision id="Decision_Espelho" name="Espelho do rulesheet Corticon">')
$mirror.Add("    <description>$(ConvertTo-XmlText (
    'ESPELHO 1:1 do rulesheet Corticon (' + @($dt.rules).Count + ' colunas x ' + @($dt.conditionRows).Count + ' condicoes), para rastreabilidade contra o .ers original. ' +
    'NAO USE ESTA TABELA PARA REVISAR COMPORTAMENTO. A politica RULE ORDER devolve a LISTA das linhas que casam, ' +
    'na ordem, e nao expressa a sobreposicao: no Corticon uma escrita posterior sobrepoe a anterior. ' +
    'Alem disso a celula vazia (null) aqui significa "esta coluna nao escreve este atributo", e NAO "define como nulo". ' +
    "Para revisar comportamento use $baseName.dmn, onde cada atributo vira uma decisao FIRST equivalente."))</description>")
$mirror.Add('    <decisionTable id="Table_Espelho" hitPolicy="RULE ORDER">')

$allRows = @($dt.conditionRows | Sort-Object { [int]$_.row })
foreach ($cr in $allRows) {
    $mirror.Add("      <input id=`"$(Get-DmnId "MIn_$($cr.row)")`" label=`"$(ConvertTo-XmlText (Get-ColumnLabel $cr.lhs))`">")
    $mirror.Add("        <inputExpression id=`"$(Get-DmnId "MInExpr_$($cr.row)")`" typeRef=`"$(Get-TypeRef $cr.datatype)`">")
    $mirror.Add("          <text>$(ConvertTo-XmlText $cr.lhs)</text>")
    $mirror.Add('        </inputExpression>')
    $mirror.Add('      </input>')
}
foreach ($ar in $dt.actionRows) {
    $attr = Get-ShortAttr $ar.lhs
    $mirror.Add("      <output id=`"$(Get-DmnId "MOut_$attr")`" label=`"$(ConvertTo-XmlText (Get-ColumnLabel $ar.lhs))`" name=`"$(ConvertTo-XmlText $attr)`" typeRef=`"$(Get-TypeRef $ar.datatype)`" />")
}
$mirror.Add('      <annotation name="coluna Corticon" />')

foreach ($r in $dt.rules) {
    $mirror.Add("      <rule id=`"$(Get-DmnId "MRule_$($r.column)")`">")
    $mirror.Add("        <description>$(ConvertTo-XmlText "coluna $($r.column) - $($r.name)")</description>")
    foreach ($cr in $allRows) {
        $txt = Get-InputEntryText -Rule $r -Row ([int]$cr.row)
        $mirror.Add("        <inputEntry id=`"$(Get-DmnId "MInE_$($r.column)_$($cr.row)")`"><text>$(ConvertTo-XmlText $txt)</text></inputEntry>")
    }
    foreach ($ar in $dt.actionRows) {
        $act = $r.actions | Where-Object { $_.lhs -eq $ar.lhs } | Select-Object -First 1
        $txt = if ($act) { ConvertTo-FeelLiteral -Value $act.rhs -Datatype $ar.datatype } else { 'null' }
        $mirror.Add("        <outputEntry id=`"$(Get-DmnId "MOutE_$($r.column)_$(Get-ShortAttr $ar.lhs)")`"><text>$(ConvertTo-XmlText $txt)</text></outputEntry>")
    }
    $mirror.Add("        <annotationEntry><text>$(ConvertTo-XmlText "col. $($r.column)")</text></annotationEntry>")
    $mirror.Add('      </rule>')
}
$mirror.Add('    </decisionTable>')
$mirror.Add('  </decision>')
$mirror.Add('  <dmndi:DMNDI>')
$mirror.Add("    <dmndi:DMNDiagram id=`"DRD_${sheetId}_Espelho`">")
$mirror.Add('      <dmndi:DMNShape id="Shape_Decision_Espelho" dmnElementRef="Decision_Espelho">')
$mirror.Add('        <dc:Bounds height="80" width="240" x="60" y="60" />')
$mirror.Add('      </dmndi:DMNShape>')
$mirror.Add('    </dmndi:DMNDiagram>')
$mirror.Add('  </dmndi:DMNDI>')
$mirror.Add('</definitions>')
($mirror -join "`r`n") | Set-Content -LiteralPath (Join-Path $OutDir "$baseName-espelho.dmn") -Encoding UTF8

# ------------------------------------------------------------------ index ----

$indexDoc = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/dmn-index/v1'
    package   = $Package
    rulesheet = $dt.source.file
    primary   = "$baseName.dmn"
    mirror    = "$baseName-espelho.dmn"
    note      = "DMN 1.3. $baseName.dmn e a representacao REVISAVEL (uma decisao FIRST por atributo, colunas invertidas, equivalente ao fold do Corticon). $baseName-espelho.dmn e apenas rastreabilidade 1:1 com o .ers e NAO deve ser usado para revisar comportamento."
    soundness = [ordered]@{
        conditionsReadOnlyRequest = $true
        actionsWriteOnlyResponse  = $true
        chainedAttributes         = @($chained)
        justification = 'Nenhum atributo lido por condicao e escrito por acao, portanto nenhuma acao altera a avaliacao de outra coluna e a ordem pode ser invertida sem mudar o resultado.'
    }
    totals = [ordered]@{
        sourceRules       = @($dt.rules).Count
        sourceConditions  = @($dt.conditionRows).Count
        sourceActionCells = [int]$dt.statistics.actionCellCount
        decisions         = $decisions.Count
        emittedRules      = $totalRules
    }
    decisions = @($decisions)
}
$indexDoc | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutDir 'index.json') -Encoding UTF8

# ------------------------------------------------------------ self-check ----

# DMN demands exact arity: a rule with the wrong number of entries opens as a
# corrupt table in Camunda Modeler instead of failing loudly. Check it here.
$faults = [System.Collections.Generic.List[string]]::new()

foreach ($file in (Get-ChildItem -Path $OutDir -Filter '*.dmn')) {
    $doc = New-Object System.Xml.XmlDocument
    try { $doc.Load($file.FullName) }
    catch { $faults.Add("$($file.Name): XML mal formado - $($_.Exception.Message)"); continue }

    $mgr = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
    $mgr.AddNamespace('d', 'https://www.omg.org/spec/DMN/20191111/MODEL/')

    foreach ($tbl in $doc.SelectNodes('//d:decisionTable', $mgr)) {
        $nIn  = $tbl.SelectNodes('d:input',      $mgr).Count
        $nOut = $tbl.SelectNodes('d:output',     $mgr).Count
        $nAnn = $tbl.SelectNodes('d:annotation', $mgr).Count
        foreach ($rule in $tbl.SelectNodes('d:rule', $mgr)) {
            $rIn  = $rule.SelectNodes('d:inputEntry',      $mgr).Count
            $rOut = $rule.SelectNodes('d:outputEntry',     $mgr).Count
            $rAnn = $rule.SelectNodes('d:annotationEntry', $mgr).Count
            if ($rIn -ne $nIn)   { $faults.Add("$($file.Name): regra $($rule.GetAttribute('id')) tem $rIn inputEntry para $nIn input") }
            if ($rOut -ne $nOut) { $faults.Add("$($file.Name): regra $($rule.GetAttribute('id')) tem $rOut outputEntry para $nOut output") }
            if ($rAnn -ne $nAnn) { $faults.Add("$($file.Name): regra $($rule.GetAttribute('id')) tem $rAnn annotationEntry para $nAnn annotation") }
        }
    }
    foreach ($shape in $doc.SelectNodes('//*[local-name()="DMNShape"]')) {
        $ref = $shape.GetAttribute('dmnElementRef')
        if (-not $doc.SelectSingleNode("//d:decision[@id='$ref']", $mgr)) { $faults.Add("$($file.Name): DMNShape -> decisao '$ref' inexistente") }
    }
}

# Every action cell in the source must appear exactly once in the decomposition,
# otherwise a rule was silently dropped.
if ($totalRules -ne [int]$dt.statistics.actionCellCount) {
    $faults.Add("regras emitidas ($totalRules) != celulas de acao na origem ($([int]$dt.statistics.actionCellCount))")
}

if ($faults.Count -gt 0) {
    Write-Host ''
    Write-Host "FALHA: $($faults.Count) problema(s) no DMN gerado" -ForegroundColor Red
    foreach ($f in ($faults | Select-Object -First 20)) { Write-Host "    $f" -ForegroundColor Red }
    exit 1
}

Write-Host ("Wrote {0}  ({1} decisoes FIRST, {2} regras; espelho RULE ORDER com {3} colunas x {4} condicoes)" -f `
    $OutDir, $decisions.Count, $totalRules, @($dt.rules).Count, @($dt.conditionRows).Count)
Write-Host ("    auto-verificacao: aridade das regras OK, {0} celulas de acao preservadas, sem encadeamento" -f $totalRules) -ForegroundColor DarkGray
