<#
.SYNOPSIS
    Extracts the iProcess builtin contract from the script tasks, with test vectors.

.DESCRIPTION
    The 40 script tasks call iProcess builtins that have no .NET equivalent. Porting
    them wrong is the quiet kind of failure: SUBSTR and SEARCH are widely documented as
    1-based, .NET is 0-based, and an off-by-one there silently truncates a document id
    instead of throwing.

    There is no running TIBCO and no vendor documentation in this delivery, so the
    semantics of each builtin CANNOT be established from the sources. Inventing them
    would be worse than leaving them open. What this script does instead:

      1. records every call site with its real arguments, so the surface is exact;
      2. derives the observed arity, which is a fact;
      3. builds BEHAVIOURAL test vectors from the literal data the scripts carry.

    The vectors are what make this useful without an oracle. The tokenising loop in
    prepSub parses a pipe-delimited id list, and the surrounding data flow says how many
    tokens must come out. So the requirement on any .NET shim is not "SUBSTR must be
    1-based" - it is "given this input, the loop must yield these tokens". A candidate
    implementation either satisfies that or it does not, and no knowledge of TIBCO's
    internals is needed to decide.

    Semantics that no vector pins down are emitted as open questions rather than guesses.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ModelPath,
    [Parameter(Mandatory)][string]$OutPath
)

$ErrorActionPreference = 'Stop'
$model = Get-Content -LiteralPath $ModelPath -Raw -Encoding UTF8 | ConvertFrom-Json

$FunctionNs = 'IPEStringUtil|IPEDateTimeUtil|IPEMathUtil|IPEConversionUtil'

# --------------------------------------------------------------- collect ----

$scripts = [System.Collections.Generic.List[object]]::new()
foreach ($p in $model.processes) {
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) {
            if (-not $n.script -or -not $n.script.body) { continue }
            $scripts.Add([pscustomobject]@{
                Process = $p.name; Scope = $s.scope
                Node = $(if ($n.displayName) { $n.displayName } else { $n.name })
                NodeId = $n.id; Body = [string]$n.script.body
            })
        }
    }
}

function Split-Args {
    param([string]$Text)
    $out = @(); $depth = 0; $cur = ''; $quote = $null
    foreach ($ch in $Text.ToCharArray()) {
        if ($quote) { $cur += $ch; if ($ch -eq $quote) { $quote = $null }; continue }
        if ($ch -eq '"' -or $ch -eq "'") { $quote = $ch; $cur += $ch; continue }
        if ($ch -eq '(') { $depth++ }
        elseif ($ch -eq ')') { $depth-- }
        if ($ch -eq ',' -and $depth -eq 0) { $out += $cur.Trim(); $cur = '' } else { $cur += $ch }
    }
    if ($cur.Trim()) { $out += $cur.Trim() }
    return , @($out)
}

$calls = [System.Collections.Generic.List[object]]::new()
$constants = @{}

foreach ($sc in $scripts) {
    foreach ($m in [regex]::Matches($sc.Body, "($FunctionNs)\.(\w+)\s*\(((?:[^()]|\([^()]*\))*)\)")) {
        $argList = Split-Args $m.Groups[3].Value
        $calls.Add([ordered]@{
            builtin = "$($m.Groups[1].Value).$($m.Groups[2].Value)"
            arity   = $argList.Count
            arguments = @($argList)
            process = $sc.Process; node = $sc.Node; nodeId = $sc.NodeId
            expression = $m.Value
        })
    }
    foreach ($m in [regex]::Matches($sc.Body, 'IPESystemValues\.(\w+)')) {
        $k = $m.Groups[1].Value
        $constants[$k] = 1 + [int]$constants[$k]
    }
}

# ------------------------------------------------------------- builtins ----

$builtins = [System.Collections.Generic.List[object]]::new()
# Script block, not a property name: Group-Object cannot read a key off an ordered hashtable.
foreach ($grp in ($calls | Group-Object { $_.builtin } | Sort-Object Name)) {
    $arities = @($grp.Group | ForEach-Object { $_.arity } | Sort-Object -Unique)
    $builtins.Add([ordered]@{
        name = $grp.Name
        kind = 'function'
        callCount = $grp.Count
        observedArity = $arities
        semanticsStatus = 'unconfirmed'
        callSites = @($grp.Group | ForEach-Object {
            [ordered]@{ process = $_.process; node = $_.node; nodeId = $_.nodeId; expression = $_.expression; arguments = $_.arguments }
        })
    })
}
foreach ($k in ($constants.Keys | Sort-Object)) {
    $builtins.Add([ordered]@{
        name = "IPESystemValues.$k"
        kind = 'systemValue'
        callCount = $constants[$k]
        observedArity = @()
        semanticsStatus = $(if ($k -eq 'SW_NA') { 'documented-in-artifacts' } else { 'unconfirmed' })
        callSites = @()
    })
}

# -------------------------------------------------------- test vectors ----

# The tokenising loop is the one place a wrong index base changes data instead of
# throwing, and the scripts carry a literal input for it - so it can be pinned down
# behaviourally, without knowing how TIBCO implements SUBSTR or SEARCH.
$vectors = [System.Collections.Generic.List[object]]::new()

$tokeniser = $scripts | Where-Object { $_.Body -match 'IPEStringUtil\.SEARCH' } | Select-Object -First 1
if ($tokeniser) {
    $literal = [regex]::Match($tokeniser.Body, "(\w+)\s*=\s*'([^']*\|[^']*)'\s*;")
    $sample = $(if ($literal.Success) { $literal.Groups[2].Value } else { $null })
    $expected = $(if ($sample) { @($sample -split '\|' | Where-Object { $_ -ne '' }) } else { @() })

    $vectors.Add([ordered]@{
        id = 'VEC-TOKENISE-PIPE-LIST'
        routine = "$($tokeniser.Process)/$($tokeniser.Node)"
        nodeId = $tokeniser.NodeId
        builtinsExercised = @('IPEStringUtil.SEARCH', 'IPEStringUtil.SUBSTR', 'IPEStringUtil.STRLEN')
        input = [ordered]@{
            source = $(if ($literal.Success) { $literal.Groups[1].Value } else { 'IDSINTIMADOS' })
            value  = $sample
            note   = 'Literal presente no proprio script - ver scriptHazards, e dado de teste embutido.'
        }
        expectedTokens = $expected
        assertion = 'Ao final do laco, o vetor de saida deve conter exatamente estes tokens, na ordem. Qualquer implementacao de SUBSTR/SEARCH que nao produza isso esta errada.'
        whyThisIsAnOracle = 'A expectativa vem do formato do dado (lista separada por |), nao da semantica do TIBCO. Vale para qualquer implementacao candidata sem precisar da documentacao do fornecedor.'
        indexBaseProbe = [ordered]@{
            question = 'SEARCH/SUBSTR sao base 1 ou base 0?'
            workedExample = "Com valor '$sample': em base 1, SEARCH('|') retorna a posicao do primeiro separador e SUBSTR(x,1,pos-1) recorta o primeiro token inteiro. Em base 0 o mesmo calculo perde o ultimo caractere do token."
            conclusion = 'A aritmetica do script so fecha em base 1 - PRESSUPONDO que o script original esteja correto. Confirmar contra a documentacao do iProcess antes de codificar.'
        }
    })
}

# --------------------------------------------------------------- hazards ----

$scriptHazards = [System.Collections.Generic.List[object]]::new()
foreach ($sc in $scripts) {
    foreach ($m in [regex]::Matches($sc.Body, "(?m)^\s*(\w+)\s*=\s*'([^']{6,})'\s*;")) {
        $kind = if ($m.Groups[2].Value -match '@') { 'hardcoded-recipient' } else { 'hardcoded-literal' }
        $scriptHazards.Add([ordered]@{
            kind = $kind; process = $sc.Process; node = $sc.Node; nodeId = $sc.NodeId
            variable = $m.Groups[1].Value; value = $m.Groups[2].Value
        })
    }
    foreach ($m in [regex]::Matches($sc.Body, '/\*([\s\S]*?)\*/')) {
        $body = $m.Groups[1].Value.Trim()
        if ($body.Length -lt 12) { continue }
        $scriptHazards.Add([ordered]@{
            kind = 'commented-out-logic'; process = $sc.Process; node = $sc.Node; nodeId = $sc.NodeId
            variable = $null; value = $body
        })
    }
}

$doc = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/builtin-contract/v1'
    note = 'Superficie de builtins iProcess usada pelos scriptTasks. A SEMANTICA DE CADA BUILTIN NAO E DERIVAVEL destas fontes (sem TIBCO em execucao e sem documentacao do fornecedor na entrega) e por isso vem marcada como unconfirmed. Os vetores de teste sao comportamentais: fixam o RESULTADO exigido pelo dado, nao a implementacao.'
    statistics = [ordered]@{
        scriptTaskCount = $scripts.Count
        functionCount   = @($builtins | Where-Object { $_.kind -eq 'function' }).Count
        systemValueCount = @($builtins | Where-Object { $_.kind -eq 'systemValue' }).Count
        callSiteCount   = $calls.Count
        testVectorCount = $vectors.Count
        scriptHazardCount = $scriptHazards.Count
    }
    builtins = @($builtins)
    testVectors = @($vectors)
    scriptHazards = @($scriptHazards)
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutPath) | Out-Null
$doc | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} builtins em {2} chamadas, {3} vetor(es), {4} risco(s) de script)" -f `
    $OutPath, $builtins.Count, $calls.Count, $vectors.Count, $scriptHazards.Count)
