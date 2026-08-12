<#
.SYNOPSIS
    S6 - turns the review dossier into a questionnaire a developer can act on.

.DESCRIPTION
    The dossier is machine-readable and complete; it is not something you hand to a
    developer on a Monday morning. This script renders the same items as a worklist
    in priority order, and adds the one thing the dossier does not carry: the exact
    line in the source file where each question can be seen with your own eyes.

    Line numbers are resolved by scanning the source once and indexing every
    Id="..." occurrence, so they are recomputed on every run and cannot go stale.

    Nothing here invents an answer. Suggestions come from the declared catalogues
    (net-equivalence-catalog.json for gaps, questionnaire-templates.json otherwise),
    and every question keeps the glossary key where its answer must be written.
#>
[CmdletBinding()]
param(
    [string]$Package       = 'POC_Epat',
    [string]$ArtifactsDir  = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$SourceRoot    = "$PSScriptRoot/../input/Arquivos Poc Camunda",
    [string]$XpdlPath      = "$PSScriptRoot/../input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl",
    [string]$TemplatesPath = "$PSScriptRoot/../config/questionnaire-templates.json",
    [string]$OutPath       = "$PSScriptRoot/../artifacts/POC_Epat/questionario.md",
    [ValidateSet('all', 'behavioural')][string]$Scope = 'all'
)

$ErrorActionPreference = 'Stop'

$dossierPath = Join-Path $ArtifactsDir 'review-dossier.json'
if (-not (Test-Path $dossierPath)) { throw "review-dossier.json not found: $dossierPath" }
$dossier   = Get-Content $dossierPath -Raw -Encoding UTF8 | ConvertFrom-Json
$templates = Get-Content $TemplatesPath -Raw -Encoding UTF8 | ConvertFrom-Json

# --------------------------------------------------------- source indexing ----

# elementId -> line number. One pass over the XPDL; every Id attribute is indexed.
$lineOfId   = @{}
$lineOfName = @{}
if (Test-Path $XpdlPath) {
    $lineNo = 0
    foreach ($line in [System.IO.File]::ReadLines($XpdlPath)) {
        $lineNo++
        foreach ($m in [regex]::Matches($line, 'Id="([^"]+)"')) {
            $id = $m.Groups[1].Value
            if (-not $lineOfId.ContainsKey($id)) { $lineOfId[$id] = $lineNo }
        }
        foreach ($m in [regex]::Matches($line, 'Name="([^"]+)"')) {
            $nm = $m.Groups[1].Value
            if (-not $lineOfName.ContainsKey($nm)) { $lineOfName[$nm] = $lineNo }
        }
    }
}
$xpdlRel = 'input/Arquivos Poc Camunda/POC_Camunda/POC_Epat/Process Packages/POC_Epat.xpdl'

function Get-Line {
    param([string]$ElementId)
    if ($ElementId -and $lineOfId.ContainsKey($ElementId)) { return $lineOfId[$ElementId] }
    return $null
}

# ------------------------------------------------------------- locations ----

# Every way an item can point back at the source, collapsed into one table.
function Get-Locations {
    param($Item)
    $rows = [System.Collections.Generic.List[object]]::new()
    $seen = @{}

    foreach ($sr in (Arr $Item.sourceRef)) {
        if (-not $sr) { continue }
        $ln  = Get-Line $sr.elementId
        $key = "$($sr.elementId)"
        if ($seen.ContainsKey($key)) { continue }
        $seen[$key] = $true
        $proc = ''
        if ($sr.pointer -match 'processes\[([^\]]+)\]') { $proc = $Matches[1] }
        $rows.Add([pscustomobject]@{
            File = $xpdlRel; Line = $ln; Element = $sr.elementId; Process = $proc; What = 'elemento'
        })
    }

    foreach ($u in (Arr $Item.usages)) {
        if (-not $u) { continue }
        $eid = $u.decisionElementId
        if (-not $eid) { continue }
        if ($seen.ContainsKey($eid)) { continue }
        $seen[$eid] = $true
        $rows.Add([pscustomobject]@{
            File = $xpdlRel; Line = (Get-Line $eid); Element = $eid; Process = $u.process; What = 'decisao'
        })
    }

    # Field-shaped items carry no element id; the declaration is found by name.
    if ($rows.Count -eq 0 -and $Item.subject -and $lineOfName.ContainsKey($Item.subject)) {
        $rows.Add([pscustomobject]@{
            File = $xpdlRel; Line = $lineOfName[$Item.subject]; Element = $Item.subject
            Process = ''; What = 'declaracao'
        })
    }

    return $rows
}

# ------------------------------------------------------------- rendering ----

function Esc { param($T) if ($null -eq $T) { return '' }; return (([string]$T) -replace '\r?\n', ' ') -replace '\|', '\|' }

# Nesting a quoted string inside a subexpression inside a quoted string is where
# PowerShell string parsing gives up; build code spans by concatenation instead.
$BT = [string][char]96
# Backslash escapes are not interpreted inside a code span, so pipes stay literal here.
function Tick { param($T) if ($null -eq $T) { return '' }; return $BT + ((([string]$T) -replace '\r?\n', ' ')) + $BT }

# @($null) has Count 1, so every optional field needs the nulls stripped before use.
function Arr { param($X) return @($X | Where-Object { $null -ne $_ }) }

$items = @($dossier.items)
if ($Scope -eq 'behavioural') {
    $items = @($items | Where-Object { $_.priority -in @('P1', 'P2', 'P3') })
}

$rank = @{ P1 = 1; P2 = 2; P3 = 3; P4 = 4 }
$items = @($items | Sort-Object @{ e = { $rank[$_.priority] } }, @{ e = { $_.category } }, @{ e = { $_.id } })

$L = [System.Collections.Generic.List[string]]::new()
$L.Add("# Questionario de migracao - $Package")
$L.Add('')
$L.Add("Gerado de ``review-dossier.json`` em $(Get-Date -Format 'yyyy-MM-dd HH:mm'). Regenerado a cada extracao: nao editar este arquivo.")
$L.Add('')
$L.Add('As respostas vao para `config/glossary/' + $Package + '.yaml`, na chave indicada em cada item - nunca neste documento.')
$L.Add('')
$L.Add('## Como usar')
$L.Add('')
$L.Add('| Prioridade | Significado | Efeito de nao responder |')
$L.Add('|---|---|---|')
$L.Add('| **P1** | Construcao sem equivalente em .NET, severidade alta | A implementacao dos passos afetados e um palpite que falha em silencio |')
$L.Add('| **P2** | Sem equivalente (media) ou bloqueador | Politica de erro, prazo ou correlacao fica indefinida |')
$L.Add('| **P3** | Comportamental | O ramo errado dispara em producao, sem erro de compilacao |')
$L.Add('| **P4** | Cosmetico | Apenas nomenclatura; nao bloqueia implementacao |')
$L.Add('')

$byPrio = $items | Group-Object priority | Sort-Object Name
$L.Add('| Prioridade | Perguntas |')
$L.Add('|---|---:|')
foreach ($g in $byPrio) { $L.Add("| $($g.Name) | $($g.Count) |") }
$L.Add("| **Total** | **$($items.Count)** |")
$L.Add('')
$L.Add('---')
$L.Add('')

$n = 0
foreach ($it in $items) {
    $n++
    $tpl = $templates.categories.PSObject.Properties[$it.category]
    $t   = if ($tpl) { $tpl.Value } else { $null }

    $L.Add("## $n. [$($it.priority)] $(Esc $it.subject)")
    $L.Add('')
    $L.Add((Tick $it.id) + ' &middot; categoria: ' + (Tick $it.category) + ' &middot; confianca da deteccao: **' + $it.confidence.level + '**')
    $L.Add('')

    $L.Add('### Pergunta')
    $L.Add('')
    $qi = 0
    foreach ($q in (Arr $it.questionsForAnalyst)) { $qi++; $L.Add("$qi. $(Esc $q)") }
    $L.Add('')

    $L.Add('### Por que isso importa')
    $L.Add('')
    if ($t -and $t.whyItMatters) { $L.Add((Esc $t.whyItMatters)); $L.Add('') }
    $L.Add((Esc $it.briefing))
    $L.Add('')

    # Findings carry what the XPDL itself revealed on inspection - the part a
    # developer cannot reconstruct from the summary alone.
    foreach ($f in (Arr $it.findings)) {
        $L.Add('#### Descoberta: ' + (Esc $f.id))
        $L.Add('')
        $L.Add((Esc $f.text))
        $L.Add('')
        foreach ($e in (Arr $f.evidence)) { $L.Add('- Evidencia: ' + (Tick $e)) }
        foreach ($h in (Arr $f.hypotheses)) { $L.Add('- Hipotese: ' + (Esc $h)) }
        if ($f.consequence) { $L.Add(''); $L.Add('> ' + (Esc $f.consequence)) }
        $L.Add('')
    }

    if ($it.architectureNote) {
        $L.Add('#### Nota de arquitetura')
        $L.Add('')
        $L.Add((Esc $it.architectureNote))
        $L.Add('')
    }

    $locs = Get-Locations $it
    if ($locs.Count -gt 0) {
        $L.Add('### Onde olhar')
        $L.Add('')
        $L.Add('| Arquivo | Linha | Elemento | Processo |')
        $L.Add('|---|---:|---|---|')
        foreach ($r in ($locs | Select-Object -First 12)) {
            $ln = if ($r.Line) { $r.Line } else { '?' }
            $L.Add('| ' + (Tick $r.File) + ' | ' + $ln + ' | ' + (Tick $r.Element) + ' | ' + (Esc $r.Process) + ' |')
        }
        if ($locs.Count -gt 12) { $L.Add('') ; $L.Add('_(+ ' + ($locs.Count - 12) + ' outra(s) ocorrencia(s) - ver review-dossier.json)_') }
        $L.Add('')
    }

    # Evidence, in whatever shape this category carries.
    $ev = [System.Collections.Generic.List[string]]::new()
    if ($it.condition)       { $ev.Add('- Condicao: ' + (Tick $it.condition)) }
    if ($it.comparedAgainst) { $ev.Add('- Comparado com: ' + (((Arr $it.comparedAgainst) | ForEach-Object { Tick $_ }) -join ' , ')) }
    if ($it.symbols)         { $ev.Add('- Simbolos: ' + (((Arr $it.symbols) | ForEach-Object { Tick $_ }) -join ' , ')) }
    if ($it.arrivesFrom)     { $ev.Add('- Chega de: ' + (((Arr $it.arrivesFrom) | ForEach-Object { Esc $_ }) -join ' ; ')) }
    foreach ($b in (Arr $it.branches)) {
        $cond = if ($b.condition) { Tick $b.condition } else { '[' + (Esc $b.conditionType) + ']' }
        $ev.Add('- Ramo ' + $cond + ' -> ' + (Esc $b.leadsTo))
    }
    if ($it.divergence) {
        foreach ($o in (Arr $it.divergence.onlyInThisProcess)) { $ev.Add('- So neste processo: ' + (Tick $o)) }
        foreach ($o in (Arr $it.divergence.presentInSiblings)) { $ev.Add('- Presente nos irmaos e ausente aqui: ' + (Tick $o)) }
    }
    foreach ($st in (Arr $it.intent.stages)) {
        $ev.Add('- Intencao no documento da POC: etapa ' + $st.stage + ' "' + (Esc $st.title) + '" (casou por "' + (Esc $st.matchedOn) + '")')
    }
    if ($ev.Count -gt 0) {
        $L.Add('### Evidencia')
        $L.Add('')
        foreach ($e in $ev) { $L.Add($e) }
        $L.Add('')
    }

    # O que o proprio pacote ja responde. Transforma a pergunta aberta numa
    # confirmacao, que custa uma leitura em vez de uma investigacao.
    $der = $it.evidenciaDerivada
    $blocos = @(if ($der -and $der.dominioObservado) { ,[pscustomobject]@{ Campo = $it.subject; E = $der } }
                else { foreach ($b in (Arr $der)) { [pscustomobject]@{ Campo = $b.campo; E = $b.evidencia } } })
    foreach ($b in ($blocos | Where-Object { $_.E })) {
        $L.Add('### Ja respondido pelo pacote' + $(if ($b.Campo -ne $it.subject) { ' - ' + (Tick $b.Campo) } else { '' }))
        $L.Add('')
        $dom = Arr $b.E.dominioObservado
        if ($dom.Count -gt 0) { $L.Add('- Dominio observado: ' + (($dom | ForEach-Object { Tick $_ }) -join ' , ')) }
        if ($b.E.valorPorOmissao) {
            $L.Add('- Valor por omissao: ' + (Tick $b.E.valorPorOmissao.valor) + ' em ' + (Esc $b.E.valorPorOmissao.onde))
        }
        foreach ($d in (Arr $b.E.divergenciaEntreClones)) {
            $L.Add('- Divergencia: ' + (Esc $d.processo) + ' usa ' + (((Arr $d.valores) | ForEach-Object { Tick $_ }) -join ' , '))
        }
        foreach ($w in (Arr $b.E.escritoPelaTela)) {
            $L.Add('- Escrito pela tela: ' + (Tick $w.valor) + ' em ' + (Esc $w.onde) + ' quando ' + (Tick $w.quando))
        }
        $L.Add('')
    }

    # Autorada por modelo de linguagem. Vai marcada e separada da evidencia, para
    # que ninguem a leia como resposta.
    if ($it.analise) {
        $L.Add('### Hipotese de trabalho')
        $L.Add('')
        $L.Add(':warning: **Analise agentica, NAO verificada.** Confianca: **' + (Esc $it.analise.confianca) + '**. ' +
               'Serve para acelerar a confirmacao, nunca para dispensar a resposta.')
        $L.Add('')
        $L.Add('**' + (Esc $it.analise.hipotese) + '**')
        $L.Add('')
        if ($it.analise.raciocinio)      { $L.Add((Esc $it.analise.raciocinio)); $L.Add('') }
        if ($it.analise.oQueConfirmaria) { $L.Add('- Para fechar a questao: ' + (Esc $it.analise.oQueConfirmaria)) }
        if ($it.analise.riscoSeErrada)   { $L.Add('- Se a hipotese estiver errada: ' + (Esc $it.analise.riscoSeErrada)) }
        $L.Add('')
    }

    $L.Add('### Sugestao')
    $L.Add('')
    $opts = Arr $it.suggestedOptions
    if ($opts.Count -gt 0) {
        $L.Add('| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |')
        $L.Add('|---|---|---|---|:---:|')
        foreach ($o in $opts) {
            $s = if ($o.suggested) { '**sim**' } else { '' }
            $L.Add('| ' + (Tick $o.id) + ' | ' + (Esc $o.pattern) + ' | ' + (Esc $o.approach) + ' | ' + (Esc $o.consequence) + ' | ' + $s + ' |')
        }
        $L.Add('')
        $L.Add('_A opcao marcada como sugerida precisa de ratificacao explicita; ela nao e uma decisao._')
    }
    elseif ($t -and $t.suggestion) { $L.Add((Esc $t.suggestion)) }
    else { $L.Add('_Sem sugestao declarada para esta categoria._') }
    $L.Add('')

    $L.Add('### Resposta')
    $L.Add('')
    if ($t -and $t.howToAnswer) { $L.Add("_$(Esc $t.howToAnswer)_"); $L.Add('') }
    $where = if ($it.resolution.answerIn) { (Tick $it.resolution.answerIn) + ' -> ' + (Tick $it.resolution.key) } else { '_sem slot no glossario: registrar por escrito na documentacao da POC_' }
    $L.Add('Onde registrar: ' + $where)
    $L.Add('')
    $fields = if ($t -and $t.answerFields) { @($t.answerFields) } else { @('resposta') }
    foreach ($f in $fields) { $L.Add("- [ ] **$f**: ") }
    $L.Add('- [ ] Respondido por / data: ')
    $L.Add('')
    $L.Add('---')
    $L.Add('')
}

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
($L -join "`r`n") | Set-Content -LiteralPath $OutPath -Encoding UTF8

$resolved = 0; $total = 0
foreach ($it in $items) { foreach ($r in (Get-Locations $it)) { $total++; if ($r.Line) { $resolved++ } } }
Write-Host ("Wrote {0}  ({1} perguntas; {2}/{3} localizacoes resolvidas para linha no XPDL)" -f `
    $OutPath, $items.Count, $resolved, $total)
