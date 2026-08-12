<#
.SYNOPSIS
    S1.9 - regras de negocio escondidas no code-behind das telas ASP.NET.

.DESCRIPTION
    O XPDL nao e a unica fonte de regra: as duas telas entregues carregam 2750 linhas
    de C# com decisoes que o diagrama nao mostra. Este gerador le o code-behind e
    extrai, de forma deterministica:

      decisions     cada if / else if / case, com a condicao literal e o efeito
      engineWrites  campos que a tela escreve de volta no motor (o contrato tela->processo)
      backendCalls  cada chamada a uma Facade, com a condicao que a protege
      messages      mensagens literais mostradas ao utilizador

    O ficheiro e mascarado antes da analise: comentarios e literais de texto sao
    substituidos por espacos do mesmo comprimento, para que a contagem de chavetas e
    parenteses seja fiavel sem perder os numeros de linha. O texto devolvido em cada
    campo vem sempre do ficheiro original, nao da mascara.

    Tudo aqui e DERIVADO. Nada e interpretado nem batizado.
#>
[CmdletBinding()]
param(
    [string]$ScreensPath    = "$PSScriptRoot/../artifacts/POC_Epat/screen-catalogue.json",
    [string]$FieldsPath     = "$PSScriptRoot/../artifacts/POC_Epat/case-field-dictionary.json",
    [string]$ConformancePath= "$PSScriptRoot/../artifacts/POC_Epat/conformance.json",
    [string]$TelasRoot      = "$PSScriptRoot/../input/Arquivos Poc Camunda/Telas",
    [string]$OutPath        = "$PSScriptRoot/../artifacts/POC_Epat/screen-rules.json"
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/lib-reading.ps1"
. "$PSScriptRoot/lib-classification.ps1"

$screens = Get-Content $ScreensPath -Raw -Encoding UTF8 | ConvertFrom-Json
$fields  = Get-Content $FieldsPath  -Raw -Encoding UTF8 | ConvertFrom-Json
$fieldIndex = New-FieldIndex $fields

$flowProcesses = @()
if (Test-Path $ConformancePath) {
    $conf = Get-Content $ConformancePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $flowProcesses = @($conf.etapas | ForEach-Object { $_.processes } | Where-Object { $_ } | Sort-Object -Unique)
}

$caseFields = @{}
foreach ($f in @($fields.fields)) { $caseFields[$f.name.ToUpperInvariant()] = $f }

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

# ------------------------------------------------------------------ mask ----
# Apaga comentarios e literais preservando comprimento e quebras de linha.
function New-Mask {
    param([string]$Text)
    $sb = [System.Text.StringBuilder]::new($Text.Length)
    $i = 0; $n = $Text.Length
    while ($i -lt $n) {
        $c = $Text[$i]
        if ($c -eq '/' -and $i + 1 -lt $n -and $Text[$i + 1] -eq '/') {
            while ($i -lt $n -and $Text[$i] -ne "`n") { [void]$sb.Append(' '); $i++ }
            continue
        }
        if ($c -eq '/' -and $i + 1 -lt $n -and $Text[$i + 1] -eq '*') {
            [void]$sb.Append('  '); $i += 2
            while ($i -lt $n -and -not ($Text[$i] -eq '*' -and $i + 1 -lt $n -and $Text[$i + 1] -eq '/')) {
                [void]$sb.Append($(if ($Text[$i] -eq "`n") { "`n" } else { ' ' })); $i++
            }
            if ($i -lt $n) { [void]$sb.Append('  '); $i += 2 }
            continue
        }
        if ($c -eq '@' -and $i + 1 -lt $n -and $Text[$i + 1] -eq '"') {
            [void]$sb.Append('  '); $i += 2
            while ($i -lt $n) {
                if ($Text[$i] -eq '"') {
                    if ($i + 1 -lt $n -and $Text[$i + 1] -eq '"') { [void]$sb.Append('  '); $i += 2; continue }
                    [void]$sb.Append(' '); $i++; break
                }
                [void]$sb.Append($(if ($Text[$i] -eq "`n") { "`n" } else { ' ' })); $i++
            }
            continue
        }
        if ($c -eq '"' -or $c -eq "'") {
            $q = $c
            [void]$sb.Append(' '); $i++
            while ($i -lt $n -and $Text[$i] -ne $q) {
                if ($Text[$i] -eq '\' -and $i + 1 -lt $n) { [void]$sb.Append('  '); $i += 2; continue }
                [void]$sb.Append($(if ($Text[$i] -eq "`n") { "`n" } else { ' ' })); $i++
            }
            if ($i -lt $n) { [void]$sb.Append(' '); $i++ }
            continue
        }
        [void]$sb.Append($c); $i++
    }
    return $sb.ToString()
}

function Get-Match {
    param([string]$Mask, [int]$Start, [char]$Open, [char]$Close)
    if ($Start -lt 0 -or $Start -ge $Mask.Length -or $Mask[$Start] -ne $Open) { return -1 }
    $depth = 0
    for ($i = $Start; $i -lt $Mask.Length; $i++) {
        if ($Mask[$i] -eq $Open) { $depth++ }
        elseif ($Mask[$i] -eq $Close) { $depth--; if ($depth -eq 0) { return $i } }
    }
    return -1
}

function Skip-Space { param([string]$Mask, [int]$From)
    $i = $From
    while ($i -lt $Mask.Length -and [char]::IsWhiteSpace($Mask[$i])) { $i++ }
    return $i
}

# ------------------------------------------------------------- descricao ----

function Get-Reads {
    param([string]$Text)
    $cf = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($m in [regex]::Matches($Text, 'fieldsIProcess\s*\[\s*"([^"]+)"\s*\]')) { [void]$cf.Add($m.Groups[1].Value) }
    foreach ($m in [regex]::Matches($Text, 'campoIProcess\.(\w+)'))                 { [void]$cf.Add($m.Groups[1].Value) }
    foreach ($m in [regex]::Matches($Text, 'WorkItem(?:Release|Keep|Lock)Field\s*\(\s*"([^"]+)"')) { [void]$cf.Add($m.Groups[1].Value) }
    foreach ($m in [regex]::Matches($Text, '\b([A-Z][A-Z0-9_]{3,})\b')) {
        if ($caseFields.ContainsKey($m.Groups[1].Value)) { [void]$cf.Add($m.Groups[1].Value) }
    }
    $ent = @(foreach ($m in [regex]::Matches($Text, '\b(?:parametros\.)?aiim\.(\w+)')) { $m.Groups[1].Value }) | Sort-Object -Unique
    return [pscustomobject]@{
        caseFields = @($cf | Sort-Object)
        entityProps = @($ent)
    }
}

function Get-Effects {
    param([string]$Body)
    $e = [ordered]@{}
    $writes = @(foreach ($m in [regex]::Matches($Body, 'new\s+WorkItem(Release|Keep|Lock)Field\s*\(\s*"([^"]+)"\s*(?:,\s*"([^"]*)"\s*)?(?:,\s*([^\)]*))?\)')) {
        [ordered]@{
            field = $m.Groups[2].Value
            kind  = $m.Groups[1].Value.ToLowerInvariant()
            swType = $m.Groups[3].Value
            value  = (($m.Groups[4].Value -replace '\s+', ' ').Trim(' ', '"'))
        }
    })
    if ($writes.Count) { $e.engineWrites = @($writes) }

    $api = @(foreach ($m in [regex]::Matches($Body, '\b(releaseWorkItem|keepWorkItem|undoWorkItem|lockWorkItem|getFFPWorkItem|getWorkItemByFFP|getSessionStatus)\s*\(')) { $m.Groups[1].Value }) | Sort-Object -Unique
    if ($api.Count) { $e.engineApi = @($api) }

    $facades = @(foreach ($m in [regex]::Matches($Body, '\b(\w*(?:Facade|_Facade))\s*(?:\(\s*\))?\s*\.\s*(\w+)\s*\(')) { $m.Groups[1].Value + '.' + $m.Groups[2].Value }) | Sort-Object -Unique
    if ($facades.Count) { $e.backendCalls = @($facades) }

    $msgs = @(foreach ($m in [regex]::Matches($Body, '(?:lblErro|mostraErro|MensagemErro|Message|mensagem|FecharComMensagem|Text)\s*[=\(][^"\r\n]{0,40}"([^"]{6,})"')) { $m.Groups[1].Value.Trim() }) | Sort-Object -Unique
    if ($msgs.Count) { $e.messages = @($msgs) }

    $ui = @(foreach ($m in [regex]::Matches($Body, '\b(\w+)\s*\.\s*(Enabled|Visible|ReadOnly)\s*=\s*(true|false)')) { "$($m.Groups[1].Value).$($m.Groups[2].Value)=$($m.Groups[3].Value)" }) | Sort-Object -Unique
    if ($ui.Count) { $e.uiState = @($ui) }

    if ($Body -match '\bthrow\b')                  { $e.throws = $true }
    if ($Body -match 'Response\.Redirect|window\.open|RegisterStartupScript') { $e.navigates = $true }
    if ($Body -match '\breturn\b')                 { $e.returns = $true }
    return $e
}

# Deteta condicoes que nao decidem nada: sempre verdadeiras ou sempre falsas.
function Get-Findings {
    param([string]$Condition)
    $f = @()
    foreach ($m in [regex]::Matches($Condition, '([\w\.\[\]"]+)\s*!=\s*([^\s|&\)]+)\s*\|\|\s*\1\s*!=\s*([^\s|&\)]+)')) {
        if ($m.Groups[2].Value -ne $m.Groups[3].Value) {
            $f += [ordered]@{ tipo = 'condicao-sempre-verdadeira'; detalhe = "$($m.Groups[1].Value) nao pode ser igual a $($m.Groups[2].Value) e a $($m.Groups[3].Value) ao mesmo tempo, logo o OU e sempre verdadeiro; o ramo senao e inalcancavel" }
        }
    }
    foreach ($m in [regex]::Matches($Condition, '([\w\.\[\]"]+)\s*==\s*([^\s|&\)]+)\s*&&\s*\1\s*==\s*([^\s|&\)]+)')) {
        if ($m.Groups[2].Value -ne $m.Groups[3].Value) {
            $f += [ordered]@{ tipo = 'condicao-sempre-falsa'; detalhe = "$($m.Groups[1].Value) nao pode valer $($m.Groups[2].Value) e $($m.Groups[3].Value) ao mesmo tempo; o ramo e inalcancavel" }
        }
    }
    return $f
}

# Diz, em palavras, o que acontece quando a condicao e verdadeira.
function Get-Consequence {
    param($Effects)
    $p = @()
    foreach ($w in (Arr $Effects.engineWrites)) { $p += "escreve $($w.field)=$($w.value) no motor" }
    foreach ($a in (Arr $Effects.engineApi))    { $p += "chama $a no motor" }
    foreach ($b in (Arr $Effects.backendCalls)) { $p += "chama $b no backend" }
    if (Arr $Effects.messages) { $p += 'mostra mensagem ao utilizador' }
    if (Arr $Effects.uiState)  { $p += 'muda o estado do ecra' }
    if ($Effects.throws)       { $p += 'lanca excepcao' }
    if ($Effects.navigates)    { $p += 'navega ou fecha a janela' }
    if ($Effects.returns)      { $p += 'termina o metodo' }
    if ($p.Count -eq 0) { return 'Nao produz nenhum efeito que este extractor saiba reconhecer.' }
    return 'Quando verdadeiro, ' + ($p -join ', ') + '.'
}

function Get-Class {
    param($Effects, $Reads, [string]$Condition)
    return New-Classification -Portador 'screenCode' `
        -LeCamposDoCaso (($Reads.caseFields.Count + $Reads.entityProps.Count) -gt 0) `
        -TemCondicao $true `
        -EscreveCampoDoCaso ([bool](Arr $Effects.backendCalls)) `
        -EscreveNoMotor ([bool]((Arr $Effects.engineWrites) -or (Arr $Effects.engineApi))) `
        -MostraMensagem ([bool]((Arr $Effects.messages) -or $Effects.throws)) `
        -MudaEcra ([bool](Arr $Effects.uiState))
}

# ------------------------------------------------------------------ scan ----

$all = [System.Collections.Generic.List[object]]::new()
$perScreen = [System.Collections.Generic.List[object]]::new()

foreach ($scr in $screens.screens) {
    if (-not $scr.codeBehind) { continue }
    $path = Join-Path $TelasRoot $scr.codeBehind
    if (-not (Test-Path $path)) { Write-Warning "code-behind ausente: $($scr.codeBehind)"; continue }

    $text = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    $mask = New-Mask $text
    if ($mask.Length -ne $text.Length) { throw "mascara desalinhada em $($scr.codeBehind)" }

    $lineStart = [System.Collections.Generic.List[int]]::new()
    $lineStart.Add(0)
    for ($i = 0; $i -lt $text.Length; $i++) { if ($text[$i] -eq "`n") { $lineStart.Add($i + 1) } }
    function Get-LineNo { param([int]$Idx)
        $lo = 0; $hi = $lineStart.Count - 1
        while ($lo -lt $hi) { $mid = [int](($lo + $hi + 1) / 2); if ($lineStart[$mid] -le $Idx) { $lo = $mid } else { $hi = $mid - 1 } }
        return $lo + 1
    }

    # metodos: assinatura -> corpo entre chavetas
    $methods = [System.Collections.Generic.List[object]]::new()
    foreach ($m in [regex]::Matches($mask, '(?m)^[ \t]*(?:public|private|protected|internal)(?:\s+(?:static|override|virtual|partial|async|new))*\s+[\w<>,\[\]\.\?]+\s+(\w+)\s*\(')) {
        $open = $mask.IndexOf('(', $m.Index)
        $close = Get-Match $mask $open '(' ')'
        if ($close -lt 0) { continue }
        $b = Skip-Space $mask ($close + 1)
        if ($b -ge $mask.Length -or $mask[$b] -ne '{') { continue }
        $end = Get-Match $mask $b '{' '}'
        if ($end -lt 0) { continue }
        $methods.Add([pscustomobject]@{ Name = $m.Groups[1].Value; Start = $m.Index; BodyStart = $b; End = $end })
    }
    function Get-Method { param([int]$Idx)
        $best = $null
        foreach ($mm in $methods) { if ($Idx -ge $mm.BodyStart -and $Idx -le $mm.End) { if (-not $best -or $mm.BodyStart -gt $best.BodyStart) { $best = $mm } } }
        return $(if ($best) { $best.Name } else { '(fora de metodo)' })
    }

    $items = [System.Collections.Generic.List[object]]::new()
    $seq = 0

    # --- if / else if
    foreach ($m in [regex]::Matches($mask, '\bif\s*\(')) {
        $open = $mask.IndexOf('(', $m.Index)
        $close = Get-Match $mask $open '(' ')'
        if ($close -lt 0) { continue }
        $cond = ($text.Substring($open + 1, $close - $open - 1) -replace '\s+', ' ').Trim()
        if (-not $cond) { continue }

        $bs = Skip-Space $mask ($close + 1)
        $bodyEnd = $bs
        if ($bs -lt $mask.Length -and $mask[$bs] -eq '{') { $bodyEnd = Get-Match $mask $bs '{' '}' }
        else { $bodyEnd = $mask.IndexOf(';', $bs) }
        if ($bodyEnd -lt 0) { $bodyEnd = $bs }
        $body = $text.Substring($bs, [Math]::Min($bodyEnd - $bs + 1, $text.Length - $bs))

        # ramo senao, quando nao for outro if
        $elseBody = ''
        $after = Skip-Space $mask ($bodyEnd + 1)
        if ($after -lt $mask.Length -and $mask.Substring($after, [Math]::Min(4, $mask.Length - $after)) -match '^else') {
            $eb = Skip-Space $mask ($after + 4)
            if ($eb -lt $mask.Length -and $mask[$eb] -eq '{') {
                $ee = Get-Match $mask $eb '{' '}'
                if ($ee -gt 0) { $elseBody = $text.Substring($eb, $ee - $eb + 1) }
            }
            elseif ($mask.Substring($eb, [Math]::Min(3, $mask.Length - $eb)) -notmatch '^if\b') {
                $ee = $mask.IndexOf(';', $eb)
                if ($ee -gt 0) { $elseBody = $text.Substring($eb, $ee - $eb + 1) }
            }
        }

        $isElseIf = ($text.Substring([Math]::Max(0, $m.Index - 6), [Math]::Min(6, $m.Index)) -match 'else\s*$')
        $reads = Get-Reads ($cond + ' ' + $body)
        $eff = Get-Effects $body
        $effElse = $(if ($elseBody) { Get-Effects $elseBody } else { $null })
        $seq++
        $items.Add([ordered]@{
            id = "SR-$($scr.codeBehind -replace '.*/|\.aspx\.cs$', '')-$('{0:D3}' -f $seq)"
            kind = $(if ($isElseIf) { 'else-if' } else { 'if' })
            method = (Get-Method $m.Index)
            line = (Get-LineNo $m.Index)
            classification = (Get-Class $eff $reads $cond)
            condition = $cond
            readsCaseFields = @($reads.caseFields)
            readsEntityProps = @($reads.entityProps)
            leitura = (New-Reading -Expression $cond -Index $fieldIndex -Fields @($reads.caseFields) -Consequence (Get-Consequence $eff))
            effects = $eff
            elseEffects = $effElse
            findings = @(Get-Findings $cond)
        })
    }

    # --- switch / case
    foreach ($m in [regex]::Matches($mask, '\bswitch\s*\(')) {
        $open = $mask.IndexOf('(', $m.Index)
        $close = Get-Match $mask $open '(' ')'
        if ($close -lt 0) { continue }
        $subject = ($text.Substring($open + 1, $close - $open - 1) -replace '\s+', ' ').Trim()
        $bs = Skip-Space $mask ($close + 1)
        if ($bs -ge $mask.Length -or $mask[$bs] -ne '{') { continue }
        $be = Get-Match $mask $bs '{' '}'
        if ($be -lt 0) { continue }
        $inner = $mask.Substring($bs, $be - $bs + 1)

        $labels = [regex]::Matches($inner, '(?m)^\s*(?:case\s+([^:\r\n]+)|(default))\s*:')
        for ($k = 0; $k -lt $labels.Count; $k++) {
            $absStart = $bs + $labels[$k].Index
            $absEnd = $(if ($k + 1 -lt $labels.Count) { $bs + $labels[$k + 1].Index } else { $be })
            $body = $text.Substring($absStart, $absEnd - $absStart)
            $label = $(if ($labels[$k].Groups[2].Success) { 'default' } else { ($text.Substring($absStart, $labels[$k].Length) -replace '(?s)^\s*case\s+|\s*:\s*$', '').Trim() })
            $reads = Get-Reads ($subject + ' ' + $body)
            $eff = Get-Effects $body
            $seq++
            $items.Add([ordered]@{
                id = "SR-$($scr.codeBehind -replace '.*/|\.aspx\.cs$', '')-$('{0:D3}' -f $seq)"
                kind = 'case'
                method = (Get-Method $absStart)
                line = (Get-LineNo $absStart)
                classification = (Get-Class $eff $reads $subject)
                condition = "$subject == $label"
                switchSubject = $subject
                caseLabel = $label
                readsCaseFields = @($reads.caseFields)
                readsEntityProps = @($reads.entityProps)
                leitura = (New-Reading -Expression "$subject == $label" -Index $fieldIndex -Fields @($reads.caseFields) -Consequence (Get-Consequence $eff))
                effects = $eff
                elseEffects = $null
                findings = @()
            })
        }
    }

    $procs = @(Arr $scr.linkedFrom | ForEach-Object { $_.process } | Sort-Object -Unique)
    # Sort-Object por NOME de propriedade nao enxerga chaves de [ordered]; usar scriptblock.
    $sorted = @($items | Sort-Object { [int]$_.line })
    foreach ($it in $sorted) { $all.Add([pscustomobject]@{ Screen = $scr.codeBehind; Item = $it }) }

    $engineWrites = @(foreach ($it in $sorted) {
        foreach ($w in (Arr $it.effects.engineWrites)) {
            [ordered]@{
                field = $w.field; kind = $w.kind; swType = $w.swType; value = $w.value
                line = $it.line; method = $it.method
                guardedBy = $it.condition
                declaredInXpdl = $caseFields.ContainsKey($w.field.ToUpperInvariant())
            }
        }
    })

    $backend = @(foreach ($it in $sorted) {
        foreach ($c in (Arr $it.effects.backendCalls)) {
            [ordered]@{ call = $c; line = $it.line; method = $it.method; guardedBy = $it.condition }
        }
    })

    $perScreen.Add([ordered]@{
        file = $scr.file
        codeBehind = $scr.codeBehind
        processes = $procs
        inPocFlow = (@($procs | Where-Object { $_ -in $flowProcesses }).Count -gt 0)
        metrics = [ordered]@{
            lines = $scr.codeBehindLines
            methods = $methods.Count
            decisions = $sorted.Count
            regraDeNegocio = @($sorted | Where-Object { $_.classification.eRegraDeNegocio }).Count
            tecnico = @($sorted | Where-Object { -not $_.classification.eRegraDeNegocio }).Count
        }
        engineWrites = $engineWrites
        backendCalls = $backend
        caseFieldsTouched = @($sorted | ForEach-Object { $_.readsCaseFields } | Where-Object { $_ } | Sort-Object -Unique)
        decisions = $sorted
    })
}

# ----------------------------------------------------------------- write ----

$byClass = [ordered]@{}
foreach ($g in ($all | Group-Object { $_.Item.classification.efeito } | Sort-Object Name)) { $byClass[$g.Name] = $g.Count }

$undeclared = @($perScreen | ForEach-Object { $_.engineWrites } | Where-Object { $_ -and -not $_.declaredInXpdl } |
    ForEach-Object { $_.field } | Sort-Object -Unique)

# Metodos com o mesmo nome nas duas telas: copia-e-cola que pode ter evoluido em separado.
# O que interessa nao e a condicao repetida, e a condicao que existe so num dos lados.
function Get-Norm { param([string]$C) return (($C -replace 'parametros\.|this\.', '') -replace '\s+', ' ').Trim() }

$clones = @(foreach ($g in ($all | Group-Object { $_.Item.method } | Sort-Object Name)) {
    $telas = @($g.Group | ForEach-Object { $_.Screen } | Sort-Object -Unique)
    if ($telas.Count -lt 2) { continue }
    $porTela = @(foreach ($t in $telas) {
        $ds = @($g.Group | Where-Object { $_.Screen -eq $t })
        [ordered]@{
            codeBehind = $t
            decisoes = $ds.Count
            conditions = @($ds | ForEach-Object { Get-Norm $_.Item.condition } | Sort-Object -Unique)
        }
    })
    $comum = @($porTela[0].conditions | Where-Object { $c = $_; @($porTela | Where-Object { $_.conditions -contains $c }).Count -eq $telas.Count })
    $exclusivas = @(foreach ($p in $porTela) {
        foreach ($c in ($p.conditions | Where-Object { $_ -notin $comum })) {
            [ordered]@{ codeBehind = $p.codeBehind; condition = $c }
        }
    })
    [ordered]@{
        method = $g.Name
        screens = @($porTela)
        condicoesComuns = $comum
        condicoesExclusivas = $exclusivas
        divergente = ($exclusivas.Count -gt 0)
    }
})

$comFindings = @($all | Where-Object { @($_.Item.findings).Count -gt 0 } | ForEach-Object {
    [ordered]@{ codeBehind = $_.Screen; method = $_.Item.method; line = $_.Item.line; condition = $_.Item.condition; findings = @($_.Item.findings) }
})

$out = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/screen-rules/v2'
    package   = $screens.package
    note      = 'Decisoes extraidas do code-behind ASP.NET das telas entregues. Condicoes, campos e efeitos sao DERIVADOS do ficheiro fonte; nada e interpretado. A classificacao usa o eixo unico de tools/lib-classification.ps1, partilhado com o XPDL e com a planilha Corticon. O efeito decide-fluxo com escrita no motor marca o contrato tela->processo que o XPDL sozinho nao mostra.'
    pocFlowProcesses = @($flowProcesses)
    summary = [ordered]@{
        screens = $perScreen.Count
        decisions = $all.Count
        regraDeNegocio = @($all | Where-Object { $_.Item.classification.eRegraDeNegocio }).Count
        byClassification = $byClass
        engineWrites = @($perScreen | ForEach-Object { $_.engineWrites } | Where-Object { $_ }).Count
        backendCalls = @($perScreen | ForEach-Object { $_.backendCalls } | Where-Object { $_ }).Count
        condicoesQueNaoDecidem = $comFindings.Count
        metodosClonados = $clones.Count
        metodosClonadosDivergentes = @($clones | Where-Object { $_.divergente }).Count
        camposEscritosNaoDeclaradosNoXpdl = $undeclared
    }
    condicoesQueNaoDecidem = @($comFindings)
    metodosClonados = @($clones)
    screens = @($perScreen)
}

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$out | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} decisoes em {2} telas; {3} escritas no motor, {4} chamadas de backend, {5} condicoes que nao decidem, {6} metodos clonados dos quais {7} divergentes)" -f `
    $OutPath, $out.summary.decisions, $out.summary.screens, $out.summary.engineWrites, $out.summary.backendCalls, `
    $out.summary.condicoesQueNaoDecidem, $out.summary.metodosClonados, $out.summary.metodosClonadosDivergentes)
