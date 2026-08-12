<#
.SYNOPSIS
    Proves that the emitted DMN reproduces Corticon's fold, by differential testing.

.DESCRIPTION
    emit-dmn.ps1 rewrites a 49-column Corticon rulesheet into 11 single-output DMN
    decisions with reversed rule order and hit policy FIRST. The argument for that
    being equivalent is sound, but an argument is not evidence.

    This script builds two independent evaluators and diffs them over random input
    vectors:

      reference   the Corticon fold, read from decision-tables.json: every matching
                  column fires in column order, later writes override earlier ones.
      candidate   the emitted .dmn, PARSED BACK FROM THE FILE - not from
                  the generator's intent - so it tests the artifact that will
                  actually be reviewed and ported.

    Any divergence is printed with the offending input vector, which is what makes
    a failure actionable instead of merely alarming.

    This is the Tier 1 oracle for the decision layer (decision D3).
#>
[CmdletBinding()]
param(
    [string]$DecisionsPath = "$PSScriptRoot/../artifacts/POC_Epat/decision-tables.json",
    [string]$DmnPath       = '',
    [int]   $Cases         = 3000,
    [int]   $Seed          = 20260730
)

$ErrorActionPreference = 'Stop'

$dt = Get-Content $DecisionsPath -Raw -Encoding UTF8 | ConvertFrom-Json

if (-not $DmnPath) {
    $dmnDir = Join-Path (Split-Path $DecisionsPath -Parent) 'dmn'
    $idx = Get-Content -LiteralPath (Join-Path $dmnDir 'index.json') -Raw | ConvertFrom-Json
    $DmnPath = Join-Path $dmnDir $idx.primary
}

# ------------------------------------------------------ reference evaluator ----

# The .ers stores domain codes as quoted literals ('6'), the DMN as FEEL strings
# ("6"). Both denote the same code, so strip the quoting before comparing or the
# diff reports differences that do not exist.
function ConvertFrom-CorticonLiteral {
    param([string]$Text)
    $t = [string]$Text
    if ($t.Length -ge 2 -and $t.StartsWith("'") -and $t.EndsWith("'")) { return $t.Substring(1, $t.Length - 2) }
    return $t
}

# Corticon: all matching columns fire, in order; later writes win.
# Also reports how many columns wrote the most-written attribute: anything above 1
# means the override actually happened, which is the behaviour under test. It is
# counted in this same pass because a second walk over 49 rules per case doubles
# the cost of the whole run for nothing.
function Invoke-CorticonFold {
    param([hashtable]$Vector)
    $state = @{}
    $writes = @{}
    foreach ($rule in $dt.rules) {
        $matches = $true
        foreach ($c in $rule.conditions) {
            $v = $Vector[$c.lhs]
            if ($null -eq $v -or $v -notin $c.values) { $matches = $false; break }
        }
        if (-not $matches) { continue }
        foreach ($a in $rule.actions) {
            $state[$a.lhs] = ConvertFrom-CorticonLiteral $a.rhs   # override
            $writes[$a.lhs] = 1 + [int]$writes[$a.lhs]
        }
    }
    $depth = 0
    if ($writes.Count -gt 0) { $depth = ($writes.Values | Measure-Object -Maximum).Maximum }
    return [pscustomobject]@{ State = $state; MaxDepth = $depth }
}

# Counts how many columns write the same attribute for this input. Anything above 1
# means the override actually happened, which is the behaviour under test.
# ------------------------------------------------------ candidate evaluator ----

$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path $DmnPath))
$mgr = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
$mgr.AddNamespace('d', 'https://www.omg.org/spec/DMN/20191111/MODEL/')

# Unquote a FEEL literal back to the raw domain code so both sides compare like for like.
function ConvertFrom-Feel {
    param([string]$Text)
    $t = $Text.Trim()
    if ($t.Length -ge 2 -and $t.StartsWith('"') -and $t.EndsWith('"')) { return $t.Substring(1, $t.Length - 2) }
    return $t
}

$tables = [System.Collections.Generic.List[object]]::new()
foreach ($dec in $doc.SelectNodes('//d:decision', $mgr)) {
    $tbl = $dec.SelectSingleNode('d:decisionTable', $mgr)
    if (-not $tbl) { continue }
    if ($tbl.GetAttribute('hitPolicy') -ne 'FIRST') { throw "Decisao $($dec.GetAttribute('name')) nao usa FIRST" }

    $inputs = @($tbl.SelectNodes('d:input', $mgr) | ForEach-Object { $_.SelectSingleNode('d:inputExpression/d:text', $mgr).InnerText })
    $rules  = [System.Collections.Generic.List[object]]::new()
    foreach ($r in $tbl.SelectNodes('d:rule', $mgr)) {
        $entries = @($r.SelectNodes('d:inputEntry/d:text', $mgr) | ForEach-Object { $_.InnerText })
        $tests = [System.Collections.Generic.List[object]]::new()
        for ($i = 0; $i -lt $inputs.Count; $i++) {
            $txt = $entries[$i]
            if ($txt -eq '-') { $tests.Add($null); continue }   # don't care
            $tests.Add(@($txt -split ',' | ForEach-Object { ConvertFrom-Feel $_ }))
        }
        $rules.Add([pscustomobject]@{
            Tests  = $tests
            Output = (ConvertFrom-Feel $r.SelectSingleNode('d:outputEntry/d:text', $mgr).InnerText)
        })
    }
    $tables.Add([pscustomobject]@{
        Name   = $dec.GetAttribute('name')
        Lhs    = ($dt.actionRows | Where-Object { $_.lhs -like "*.$($dec.GetAttribute('name'))" } | Select-Object -First 1).lhs
        Inputs = $inputs
        Rules  = $rules
    })
}

function Invoke-DmnFirst {
    param([hashtable]$Vector)
    $state = @{}
    foreach ($t in $tables) {
        foreach ($rule in $t.Rules) {
            $matches = $true
            for ($i = 0; $i -lt $t.Inputs.Count; $i++) {
                $allowed = $rule.Tests[$i]
                if ($null -eq $allowed) { continue }
                $v = $Vector[$t.Inputs[$i]]
                if ($null -eq $v -or $v -notin $allowed) { $matches = $false; break }
            }
            if ($matches) { $state[$t.Lhs] = $rule.Output; break }   # FIRST wins
        }
    }
    return $state
}

# --------------------------------------------------------- input generation ----

$domains = @{}
foreach ($cr in $dt.conditionRows) {
    $vals = [System.Collections.Generic.List[string]]::new()
    foreach ($v in $cr.distinctValues) { $vals.Add([string]$v) }
    $vals.Add('__FORA_DO_DOMINIO__')
    $domains[$cr.lhs] = @($vals)
}
$lhsList = @($dt.conditionRows.lhs)

$rng = [System.Random]::new($Seed)
$allActionLhs = @($dt.actionRows.lhs)

# Sampling all 21 columns independently is useless here: the rules constrain up to
# 16 columns at once, so uniform vectors almost never fire anything and the test
# passes vacuously. Instead each case is SEEDED FROM A RULE - its own columns get
# satisfying values, the rest are random - which guarantees a hit and, because
# other columns then often match too, actually exercises the override behaviour
# that this whole transformation is about. Every rule is used as a seed in turn so
# no column is left untested. A slice of purely random vectors is kept to cover
# the no-match path, where both sides must agree the attribute stays unset.
$mismatches = [System.Collections.Generic.List[string]]::new()
$hits = 0
$overrideCases = 0
$maxDepth = 0
$ruleCount = @($dt.rules).Count

for ($n = 0; $n -lt $Cases; $n++) {
    $vector = @{}
    foreach ($lhs in $lhsList) {
        $d = $domains[$lhs]
        $vector[$lhs] = $d[$rng.Next(0, $d.Count)]
    }
    if ($n % 10 -ne 0) {
        $seedRule = $dt.rules[$n % $ruleCount]
        foreach ($c in $seedRule.conditions) {
            $vals = @($c.values)
            $vector[$c.lhs] = [string]$vals[$rng.Next(0, $vals.Count)]
        }
    }

    $fold = Invoke-CorticonFold -Vector $vector
    $ref  = $fold.State
    $cand = Invoke-DmnFirst -Vector $vector
    if ($ref.Count -gt 0) { $hits++ }
    if ($fold.MaxDepth -gt 1) { $overrideCases++ }
    if ($fold.MaxDepth -gt $maxDepth) { $maxDepth = $fold.MaxDepth }

    foreach ($attr in $allActionLhs) {
        $a = $ref[$attr]; $b = $cand[$attr]
        if ([string]$a -ne [string]$b) {
            $shown = ($lhsList | Where-Object { $vector[$_] -ne '__FORA_DO_DOMINIO__' } |
                      ForEach-Object { "$(($_ -split '\.')[-1])=$($vector[$_])" }) -join ' '
            $mismatches.Add("$attr : Corticon='$a' DMN='$b'  |  entrada: $shown")
            if ($mismatches.Count -ge 10) { break }
        }
    }
    if ($mismatches.Count -ge 10) { break }
}

Write-Host ''
Write-Host "Equivalencia DMN x Corticon" -ForegroundColor Cyan
Write-Host "    casos testados       : $Cases (seed $Seed)"
Write-Host "    casos com regra ativa: $hits"
Write-Host "    casos com sobreposicao (>1 escrita no mesmo atributo): $overrideCases  (profundidade maxima $maxDepth)"
Write-Host "    decisoes comparadas  : $($tables.Count)"

if ($hits -eq 0 -or $overrideCases -eq 0) {
    Write-Host ''
    Write-Host 'FALHA: o teste passou por vacuidade - nenhum caso exercitou a sobreposicao.' -ForegroundColor Red
    Write-Host 'Sem isso a equivalencia nao foi verificada, apenas nao contrariada.' -ForegroundColor Red
    exit 1
}

if ($mismatches.Count -gt 0) {
    Write-Host ''
    Write-Host "DIVERGENCIA: o DMN gerado NAO reproduz o fold do Corticon" -ForegroundColor Red
    foreach ($m in $mismatches) { Write-Host "    $m" -ForegroundColor Red }
    exit 1
}

Write-Host ''
Write-Host "OK  nenhuma divergencia em $Cases casos x $($allActionLhs.Count) atributos" -ForegroundColor Green
exit 0
