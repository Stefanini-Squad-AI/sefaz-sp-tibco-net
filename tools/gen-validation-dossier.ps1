<#
.SYNOPSIS
    S6.2 - dossie de validacao para quem opera o TIBCO no dia a dia.

.DESCRIPTION
    O questionario pede respostas. Este documento pede CONFERENCIA: apresenta as
    respostas ja dadas a quem conhece o sistema, para dizer se batem com a realidade.

    O publico nao e o arquitecto .NET - e o programador que mexe no XPDL todos os
    dias. Por isso o documento nao fala de camadas nem de padroes: fala de passos,
    de campos e de linhas do ficheiro, e aponta sempre onde ver no TIBCO.

    Cada item separa tres coisas que nunca devem ser confundidas:
      PROVA      o que o pacote diz, derivado mecanicamente - so precisa de olhada
      DECISAO    o que ficou decidido, autorado - e isto que precisa de conferencia
      RISCO      o que parte se a decisao estiver errada - define a ordem de leitura

    A ordem nao e por categoria: e por risco. Quem tem meia hora le os primeiros
    itens e ja cobriu o que faz o projecto descarrilar.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$XpdlPath     = "$PSScriptRoot/../input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl",
    [string]$OutPath      = "$PSScriptRoot/../artifacts/POC_Epat/dossie-validacao.md"
)

$ErrorActionPreference = 'Stop'

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) { if ($Optional) { return $null }; throw "artifact not found: $p" }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

$dossier = Read-Artifact 'review-dossier.json'
$rules   = Read-Artifact 'rule-inventory.json' -Optional
$screens = Read-Artifact 'screen-rules.json'   -Optional
$scope   = Read-Artifact 'scope.json'          -Optional

# ------------------------------------------------------------ respostas ----

# O dossie sabe que a pergunta foi respondida; o TEXTO da resposta vive no glossario.
$answer = @{}
$section = ''; $entry = ''
foreach ($line in (Get-Content $GlossaryPath -Encoding UTF8)) {
    if ($line -match '^([a-z]+):\s*$')          { $section = $Matches[1]; continue }
    if ($line -match '^\s{2}"?([^":]+)"?:\s*$') { $entry = $Matches[1]; continue }
    if ($line -match '^\s{4}(\w+):\s*"(.*)"\s*$') {
        $k = "$section|$entry"
        if (-not $answer.ContainsKey($k)) { $answer[$k] = [ordered]@{} }
        if ($Matches[2]) { $answer[$k][$Matches[1]] = $Matches[2] }
    }
}
# A chave do dossie vem como 'gaps.dynamic-subprocess'; o indice e por seccao e entrada.
function Get-Answer {
    param([string]$Key)
    if (-not $Key) { return $null }
    $i = $Key.IndexOf('.')
    if ($i -lt 0) { return $null }
    $k = $Key.Substring(0, $i) + '|' + $Key.Substring($i + 1)
    if ($answer.ContainsKey($k)) { return $answer[$k] }
    return $null
}

# --------------------------------------------------------- linhas do xpdl ----

$lineOfId = @{}; $lineOfName = @{}
if (Test-Path $XpdlPath) {
    $n = 0
    foreach ($line in [System.IO.File]::ReadLines($XpdlPath)) {
        $n++
        foreach ($m in [regex]::Matches($line, 'Id="([^"]+)"'))   { if (-not $lineOfId.ContainsKey($m.Groups[1].Value))   { $lineOfId[$m.Groups[1].Value] = $n } }
        foreach ($m in [regex]::Matches($line, 'Name="([^"]+)"')) { if (-not $lineOfName.ContainsKey($m.Groups[1].Value)) { $lineOfName[$m.Groups[1].Value] = $n } }
    }
}

function Get-Where {
    param($Item)
    $rows = [System.Collections.Generic.List[string]]::new()
    $seen = @{}
    foreach ($o in (Arr $Item.occurrences)) {
        $p = $o.process; $nd = $o.node
        if (-not $nd) { $nd = $o.field; if (-not $nd) { $nd = $o.builtin } }
        if (-not $nd) { continue }
        $k = "$p/$nd"
        if ($seen.ContainsKey($k)) { continue }
        $seen[$k] = $true
        $ln = $(if ($o.nodeId -and $lineOfId.ContainsKey($o.nodeId)) { "linha $($lineOfId[$o.nodeId])" } else { '' })
        $rows.Add("$(if ($p) { "$p / " })$nd$(if ($ln) { " ($ln)" })")
    }
    foreach ($u in (Arr $Item.usages)) {
        $k = "$($u.process)/$($u.decision)"
        if ($seen.ContainsKey($k)) { continue }
        $seen[$k] = $true
        $ln = $(if ($u.decisionElementId -and $lineOfId.ContainsKey($u.decisionElementId)) { "linha $($lineOfId[$u.decisionElementId])" } else { '' })
        $rows.Add("$($u.process) / $($u.decision)$(if ($ln) { " ($ln)" })")
    }
    if ($rows.Count -eq 0 -and $Item.process) {
        $rows.Add("$($Item.process)$(if ($Item.decision) { " / $($Item.decision)" })")
    }
    if ($rows.Count -eq 0 -and $lineOfName.ContainsKey([string]$Item.subject)) {
        $rows.Add("declarado na linha $($lineOfName[[string]$Item.subject])")
    }
    return @($rows)
}

# ------------------------------------------------------------ prioridade ----

# Quem tem meia hora tem de ler primeiro o que faz o projecto descarrilar.
$pesoRisco = @{ blocker = 0; high = 1; medium = 2; review = 3 }
function Get-Peso {
    param($Item)
    $p = $pesoRisco[[string]$Item.severity]
    if ($null -eq $p) { $p = 4 }
    # Uma decisao autorada com risco declarado sobe: e onde a conferencia rende mais.
    if ($Item.analise -and $Item.analise.confianca -eq 'media') { $p -= 1 }
    if ($Item.category -eq 'no-net-equivalent') { $p -= 1 }
    return $p
}

$BT = [char]0x60
function Tick { param($T) if ($null -eq $T -or "$T" -eq '') { return '' }; return $BT + ((([string]$T) -replace '\r?\n', ' ')) + $BT }
function Esc  { param($T) if ($null -eq $T) { return '' }; return (([string]$T) -replace '\r?\n', ' ') -replace '\|', '\|' }

# ------------------------------------------------------------------ corpo ----

$L = [System.Collections.Generic.List[string]]::new()
$total = @($dossier.items).Count
$respondidas = @($dossier.items | Where-Object { $_.resolution.answered }).Count

$L.Add("# Dossie de validacao - $Package")
$L.Add('')
$L.Add('Para os programadores que mantem o ePAT no TIBCO.')
$L.Add('')
$L.Add('## O que se pede')
$L.Add('')
$L.Add("Analisamos o pacote exportado e tomamos $respondidas decisoes sobre como o comportamento actual")
$L.Add('deve ser reproduzido. Nao conhecemos o sistema em producao; voces conhecem.')
$L.Add('')
$L.Add('**Nao e preciso rever tudo.** Cada item esta separado em tres partes:')
$L.Add('')
$L.Add('| Parte | O que e | Precisa da vossa atencao? |')
$L.Add('|---|---|---|')
$L.Add('| **Prova** | o que o proprio pacote diz, extraido mecanicamente | so uma olhada; se estiver errado e porque lemos mal o ficheiro |')
$L.Add('| **Decisao** | o que ficou decidido a partir dessa prova | **sim - e isto que precisa de conferencia** |')
$L.Add('| **Risco** | o que parte se a decisao estiver errada | define a ordem: os primeiros itens sao os que doem |')
$L.Add('')
$L.Add('Marque cada item com **confere** ou **nao confere**. Onde nao conferir, uma frase a dizer o que e')
$L.Add('na realidade chega - nos tratamos do resto.')
$L.Add('')
$L.Add('A ordem NAO e por assunto: e por risco. Quem tiver meia hora, le do principio e ja cobriu o essencial.')
$L.Add('')

# --- 1. o que precisa de resposta, nao so de conferencia ---

$pendentes = [System.Collections.Generic.List[object]]::new()
foreach ($it in $dossier.items) {
    $a = Get-Answer $it.resolution.key
    if (-not $a) { continue }
    foreach ($campo in @('justificativa', 'decisao', 'description', 'origin', 'values')) {
        $txt = [string]$a[$campo]
        if (-not $txt) { continue }
        foreach ($m in [regex]::Matches($txt, '(POR CONFIRMAR|POR DEFINIR|PENDENTE DE RATIFICACAO)([^.]*\.)')) {
            $pendentes.Add([pscustomobject]@{ Item = $it; Marca = $m.Groups[1].Value; Texto = ($m.Groups[1].Value + $m.Groups[2].Value) })
        }
    }
}

if ($pendentes.Count -gt 0) {
    $L.Add('---')
    $L.Add('')
    $L.Add('## Parte 1 - o que so voces conseguem responder')
    $L.Add('')
    $L.Add("Estes $($pendentes.Count) pontos ficaram por fechar porque a resposta nao esta no pacote exportado.")
    $L.Add('Nao sao conferencias: sao perguntas.')
    $L.Add('')
    $i = 0
    foreach ($p in $pendentes) {
        $i++
        $L.Add("### 1.$i  $(Esc $p.Item.subject)")
        $L.Add('')
        $L.Add((Esc $p.Texto))
        $L.Add('')
        foreach ($w in (Get-Where $p.Item | Select-Object -First 4)) { $L.Add("- No TIBCO: $(Esc $w)") }
        $L.Add('')
        $L.Add('**Resposta:** ')
        $L.Add('')
    }
}

# --- 2. decisoes a conferir, por risco ---

$L.Add('---')
$L.Add('')
$L.Add('## Parte 2 - decisoes a conferir')
$L.Add('')

$ordenadas = @($dossier.items | Sort-Object @{ e = { Get-Peso $_ } }, @{ e = { $_.subject } })
$n = 0
foreach ($it in $ordenadas) {
    $a = Get-Answer $it.resolution.key
    if (-not $a) { continue }
    $decisao = [string]$a['decisao']
    if (-not $decisao) { $decisao = [string]$a['opcaoEscolhida'] }
    if (-not $decisao) { $decisao = [string]$a['question'] }
    if (-not $decisao) { $decisao = [string]$a['term'] }
    if (-not $decisao) { continue }

    $n++
    $L.Add("### 2.$n  $(Esc $it.subject)")
    $L.Add('')

    # Prova: derivada, mecanica. So precisa de olhada.
    $prova = [System.Collections.Generic.List[string]]::new()
    $der = $it.evidenciaDerivada
    $blocos = @(if ($der -and $der.dominioObservado) { , [pscustomobject]@{ C = $it.subject; E = $der } }
                else { foreach ($b in (Arr $der)) { if ($b.evidencia) { [pscustomobject]@{ C = $b.campo; E = $b.evidencia } } } })
    foreach ($b in $blocos) {
        $dom = @($b.E.dominioObservado)
        if ($dom.Count) { $prova.Add("O pacote so usa $((($dom | ForEach-Object { Tick $_ }) -join ', ')) para $(Tick $b.C).") }
        if ($b.E.valorPorOmissao) { $prova.Add("Valor por omissao $(Tick $b.E.valorPorOmissao.valor), escrito em $(Esc $b.E.valorPorOmissao.onde).") }
    }
    if ($it.condition) { $prova.Add("Condicao no XPDL: $(Tick $it.condition)") }
    if ($it.divergence) {
        foreach ($o in (Arr $it.divergence.onlyInThisProcess)) { $prova.Add("So em $(Esc $it.subject): $(Tick $o)") }
        foreach ($o in (Arr $it.divergence.presentInSiblings)) { $prova.Add("Nos processos irmaos: $(Tick $o)") }
    }
    if ($it.occurrenceCount) { $prova.Add("Ocorre em $($it.occurrenceCount) ponto(s).") }

    if ($prova.Count -gt 0) {
        $L.Add('**Prova** _(extraido do pacote, so confirmar que lemos bem)_')
        $L.Add('')
        foreach ($p in $prova) { $L.Add("- $p") }
        $L.Add('')
    }

    $L.Add('**Decisao** _(e isto que precisa de conferencia)_')
    $L.Add('')
    $L.Add("> $(Esc $decisao)")
    $L.Add('')
    $just = [string]$a['justificativa']
    if (-not $just) { $just = [string]$a['description'] }
    if ($just) { $L.Add((Esc $just)); $L.Add('') }

    if ($it.analise -and $it.analise.riscoSeErrada) {
        $L.Add("**Se estiver errado:** $(Esc $it.analise.riscoSeErrada)")
        $L.Add('')
    }

    $onde = Get-Where $it
    if ($onde.Count -gt 0) {
        $L.Add('**Onde ver no TIBCO**')
        $L.Add('')
        foreach ($w in ($onde | Select-Object -First 6)) { $L.Add("- $(Esc $w)") }
        if ($onde.Count -gt 6) { $L.Add("- _(+ $($onde.Count - 6) outro(s))_") }
        $L.Add('')
    }

    $L.Add('- [ ] Confere')
    $L.Add('- [ ] Nao confere. Na realidade: ')
    $L.Add('')
}

# --- 3. defeitos encontrados ---

$L.Add('---')
$L.Add('')
$L.Add('## Parte 3 - defeitos que encontramos no pacote')
$L.Add('')
$L.Add('Isto nao e critica ao trabalho de ninguem: e o que aparece quando se le 765 KB de XPDL a maquina.')
$L.Add('Vale a pena confirmar se ja sao conhecidos, e se algum ja foi corrigido em producao depois desta exportacao.')
$L.Add('')

$defeitos = [System.Collections.Generic.List[object]]::new()
foreach ($r in (Arr $rules.rules)) {
    foreach ($f in (Arr $r.explanation.findings)) {
        $defeitos.Add([pscustomobject]@{ Onde = "$($r.process) / $($r.node)"; Linha = $r.xpdlLine; O = $f })
    }
}
foreach ($c in (Arr $screens.condicoesQueNaoDecidem)) {
    foreach ($f in (Arr $c.findings)) {
        $defeitos.Add([pscustomobject]@{ Onde = "$([IO.Path]::GetFileName($c.codeBehind)) / $($c.method)"; Linha = $c.line; O = "$($f.tipo): $($f.detalhe)" })
    }
}
foreach ($m in (Arr $screens.metodosClonados | Where-Object { $_.divergente })) {
    $so = @($m.condicoesExclusivas | ForEach-Object { "$([IO.Path]::GetFileName($_.codeBehind)): $($_.condition)" })
    $defeitos.Add([pscustomobject]@{
        Onde = "metodo $($m.method), clonado nas duas telas"; Linha = $null
        O = "as duas copias divergiram. So de um lado: $(($so | Select-Object -First 3) -join ' ; ')"
    })
}

if ($defeitos.Count -gt 0) {
    $L.Add('| # | Onde | O que encontramos | Ja conhecido? |')
    $L.Add('|---:|---|---|:---:|')
    $i = 0
    foreach ($d in ($defeitos | Select-Object -First 30)) {
        $i++
        $onde = $d.Onde + $(if ($d.Linha) { " (linha $($d.Linha))" } else { '' })
        $L.Add("| $i | $(Esc $onde) | $(Esc $d.O) | [ ] sim  [ ] nao |")
    }
    $L.Add('')
}

# --- 4. o que fica de fora ---

if ($scope) {
    $L.Add('---')
    $L.Add('')
    $L.Add('## Parte 4 - o que deixamos de fora, e porque')
    $L.Add('')
    $L.Add('A prova de conceito nao migra o ePAT inteiro: valida um cenario representativo.')
    $L.Add('Confirmem que nada aqui e essencial ao cenario que vao ver demonstrado.')
    $L.Add('')
    $L.Add('| Tipo | Dentro | Total | O que ficou de fora |')
    $L.Add('|---|---:|---:|---|')
    $motivoDe = @{}
    foreach ($r in (Arr $scope.exclusionRules)) { $motivoDe[$r.id] = $r.porque }
    foreach ($k in $scope.summary.byKind.PSObject.Properties) {
        if ($k.Value.fora -eq 0) { continue }
        $exc = @($scope.elements | Where-Object { $_.kind -eq $k.Name -and $_.exclusionId } |
                 Group-Object { $_.exclusionId } | Sort-Object Count -Descending | Select-Object -First 1)
        $porque = $(if ($exc) { $motivoDe[$exc[0].Name] } else { '' })
        $L.Add("| $(Esc $k.Name) | $($k.Value.dentro) | $($k.Value.total) | $(Esc $porque) |")
    }
    $L.Add('')
    $L.Add('- [ ] Confere')
    $L.Add('- [ ] Falta alguma coisa essencial: ')
    $L.Add('')
}

$L.Add('---')
$L.Add('')
$L.Add('## Quem reviu')
$L.Add('')
$L.Add('| Nome | Papel | Data |')
$L.Add('|---|---|---|')
$L.Add('|  |  |  |')
$L.Add('')

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
($L -join "`r`n") | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} decisoes a conferir, {2} perguntas so para eles, {3} defeitos listados)" -f `
    $OutPath, $n, $pendentes.Count, $defeitos.Count)
