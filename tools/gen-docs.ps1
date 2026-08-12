<#
.SYNOPSIS
    S5 - renders the intermediate artifacts as a Docusaurus documentation site.

.DESCRIPTION
    The legacy system arrived without documentation. Everything needed to describe it
    is already in the artifacts, so this script does not author anything: it projects
    the IR into MDX pages. The site is therefore regenerable, and a re-extraction can
    never leave stale prose behind.

    Pages produced:
      intro                overview, pinned sources, counts
      processes/<name>     one page per process: flow diagram, steps, decisions, hazards
      data-model           the 209 case fields and how each one is used
      integrations         the service surface
      regras/              the three places business rules live: Corticon, XPDL, screens
      open-questions       what a human still has to answer, in priority order
      migration-gaps       constructs with no .NET equivalent and the options for each

    A page that stops being produced is deleted at the end of the run, so a rename
    can never leave a stale entry behind in the menu.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$CatalogPath  = "$PSScriptRoot/../config/net-equivalence-catalog.json",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$OutDir       = "$PSScriptRoot/../docs-site/docs"
)

$ErrorActionPreference = 'Stop'

# ------------------------------------------------------------------- load ----

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) {
        if ($Optional) { return $null }
        throw "artifact not found: $p"
    }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}

$manifest  = Read-Artifact 'manifest.json'
$model     = Read-Artifact 'process-model.json'
$fields    = Read-Artifact 'case-field-dictionary.json'
$services  = Read-Artifact 'service-contracts.json'
$decisions = Read-Artifact 'decision-tables.json'
$screens   = Read-Artifact 'screen-catalogue.json' -Optional
$dossier   = Read-Artifact 'review-dossier.json'
$conformance = Read-Artifact 'conformance.json' -Optional
$scriptRules = Read-Artifact 'rule-inventory.json' -Optional
$screenRules = Read-Artifact 'screen-rules.json' -Optional
$ruleCatalogue = Read-Artifact 'rule-catalogue.json' -Optional
$catalog   = $(if (Test-Path $CatalogPath) { Get-Content $CatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json } else { $null })

# ---------------------------------------------------------------- helpers ----

# MDX treats < and { as syntax. Wrapping every value in a code span neutralises both,
# and the escaped pipe keeps table cells intact.
function Code {
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return '' }
    $t = ($Text -replace '\r?\n', ' ').Trim()
    $t = $t -replace '\|', '\|'
    $t = $t -replace '`', "'"
    return "``$t``"
}

function Cell {
    param($Text)
    if ($null -eq $Text) { return '' }
    $t = (([string]$Text) -replace '\r?\n', ' ') -replace '\|', '\|'
    # MDX parses < as JSX and { as an expression, even inside a table cell.
    $t = $t -replace '<', '&lt;'
    $t = $t -replace '>', '&gt;'
    $t = $t -replace '\{', '&#123;'
    $t = $t -replace '\}', '&#125;'
    return $t
}

function Slug {
    param([string]$Text)
    return ($Text -replace '[^A-Za-z0-9]+', '-').Trim('-').ToLowerInvariant()
}

# @($null).Count vale 1; sem isto uma lista ausente rende uma linha em branco.
function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

# A leitura derivada de uma regra: a expressao por extenso, o que se sabe de cada
# termo, e - com o mesmo destaque - o que o pacote nao diz.
function Format-Reading {
    param($Leitura)
    if (-not $Leitura -or -not $Leitura.frase) { return @() }
    $o = @("**Leitura:** $(Cell $Leitura.frase)", '')
    if ($Leitura.consequencia) { $o += @($(Cell $Leitura.consequencia), '') }

    $termos = Arr $Leitura.termos
    if ($termos.Count -gt 0) {
        $o += @('| Campo | Significa | Tipo | Valores vistos no pacote |', '|---|---|---|---|')
        foreach ($t in $termos) {
            $rot = $(if ($t.rotulo) { Cell $t.rotulo } else { '_nao declarado_' })
            $dom = (Arr $t.valoresObservadosNoPacote | ForEach-Object { Code $_ }) -join ', '
            if (-not $dom) { $dom = '_nenhum literal no pacote_' }
            $o += "| $(Code $t.campo) | $rot | $(Cell $t.tipo) | $dom |"
        }
        $o += ''
    }

    $lacunas = Arr $Leitura.naoSabemos
    if ($lacunas.Count -gt 0) {
        $o += @(':::info O que a fonte nao diz', '')
        foreach ($n in $lacunas) { $o += "- $(Cell $n)" }
        $o += @('', ':::', '')
    }
    return $o
}

# O que o pacote ja responde, e a hipotese autorada - sempre separadas uma da outra.
function Format-Hypothesis {
    param($Item)
    $o = @()
    $der = $Item.evidenciaDerivada
    $blocos = @(if ($der -and $der.dominioObservado) { , [pscustomobject]@{ Campo = $Item.subject; E = $der } }
                else { foreach ($b in (Arr $der)) { [pscustomobject]@{ Campo = $b.campo; E = $b.evidencia } } })
    foreach ($b in ($blocos | Where-Object { $_.E })) {
        $dom = Arr $b.E.dominioObservado
        if ($dom.Count -eq 0 -and -not $b.E.valorPorOmissao) { continue }
        $o += @(':::info Ja respondido pelo pacote' + $(if ($b.Campo -ne $Item.subject) { " - $($b.Campo)" } else { '' }), '')
        if ($dom.Count -gt 0) { $o += "- Dominio observado: $(($dom | ForEach-Object { Code $_ }) -join ', ')" }
        if ($b.E.valorPorOmissao) { $o += "- Valor por omissao: $(Code $b.E.valorPorOmissao.valor) em $(Cell $b.E.valorPorOmissao.onde)" }
        foreach ($d in (Arr $b.E.divergenciaEntreClones)) {
            $o += "- Divergencia: $(Cell $d.processo) usa $(((Arr $d.valores) | ForEach-Object { Code $_ }) -join ', ')"
        }
        $o += @('', ':::', '')
    }
    if ($Item.analise) {
        $o += @(
            ":::caution Hipotese de trabalho - analise agentica NAO verificada (confianca $(Cell $Item.analise.confianca))",
            '',
            '**' + (Cell $Item.analise.hipotese) + '**',
            ''
        )
        if ($Item.analise.raciocinio) { $o += @((Cell $Item.analise.raciocinio), '') }
        if ($Item.analise.oQueConfirmaria) { $o += "- Para fechar: $(Cell $Item.analise.oQueConfirmaria)" }
        if ($Item.analise.riscoSeErrada) { $o += "- Se errada: $(Cell $Item.analise.riscoSeErrada)" }
        $o += @('', ':::', '')
    }
    return $o
}

function MermaidId {
    param([string]$Id)
    return 'n' + ($Id -replace '[^A-Za-z0-9]', '')
}

$SuggestedGatewayLabels = @{
    '_CtQ7BVqPEfG5K7mY0I3I6w' = 'Execucao paralela'
    '_Faq_RFqTEfG5K7mY0I3I6w' = 'Execucao paralela'
    '_lrer_VqhEfG5K7mY0I3I6w' = 'Deve aguardar o prazo de defesa?'
    '_zJIuclqiEfG5K7mY0I3I6w' = 'Calculo do prazo retornou erro?'
    '_qIDu4l6BEfGBBLgT-R5iuw' = 'Busca retornou erro?'
    '_KEwDVl6EEfGBBLgT-R5iuw' = 'Captura de parametros retornou erro?'
    '_RNdKGl6PEfGBBLgT-R5iuw' = 'Atualizacao da intimacao retornou erro?'
    '_EvOwVF6eEfGJqLUhfbpFcQ' = 'Prazo de recebimento deve ser atualizado?'
    '_NcJxLl9KEfGqPfX31TKC3w' = 'Criacao da notificacao retornou erro?'
}

# A resposta do analista, quando existe, substitui a sugestao do gerador.
$AnsweredDecisions = @{}
if (Test-Path $GlossaryPath) {
    $section = ''; $entry = ''
    foreach ($line in (Get-Content $GlossaryPath -Encoding UTF8)) {
        if ($line -match '^([a-z]+):\s*$') { $section = $Matches[1]; continue }
        if ($line -match '^\s{2}"?([^":]+)"?:\s*$') { $entry = $Matches[1]; continue }
        if ($section -eq 'decisions' -and $line -match '^\s{4}question:\s*"(.+)"\s*$') {
            $AnsweredDecisions[$entry] = $Matches[1]
        }
    }
}

function Get-AnsweredLabel {
    param($Node, [string]$ProcessName)
    $short = if ($Node.id.Length -gt 10) { $Node.id.Substring(0, 10) } else { $Node.id }
    $k = "$ProcessName/$short"
    if ($AnsweredDecisions.ContainsKey($k)) { return $AnsweredDecisions[$k] }
    return $null
}

function Test-NeedsHumanReview {
    param($Node, [string]$ProcessName = '')
    if ($ProcessName -and (Get-AnsweredLabel -Node $Node -ProcessName $ProcessName)) { return $false }
    return $Node.kind -eq 'gateway' -and $SuggestedGatewayLabels.ContainsKey($Node.id) -and
        $SuggestedGatewayLabels[$Node.id] -ne 'Execucao paralela'
}

function Get-TimerLabel {
    param($Node)
    if ($Node.deadline.expression) {
        $expression = ([string]$Node.deadline.expression -replace '\s+', ' ').Trim().TrimEnd(';')
        return "Timer: $expression"
    }
    $parts = @()
    if ($Node.deadline.days)    { $parts += "$($Node.deadline.days)d" }
    if ($Node.deadline.hours)   { $parts += "$($Node.deadline.hours)h" }
    if ($Node.deadline.minutes) { $parts += "$($Node.deadline.minutes)min" }
    if ($parts.Count -gt 0) { return "Timer $($parts -join ' ')" }
    return 'Timer'
}

function MermaidLabel {
    param($Node, $Edges, [string]$ProcessName = '')
    $l = $Node.displayName
    if ([string]::IsNullOrWhiteSpace($l)) { $l = $Node.name }
    if ([string]::IsNullOrWhiteSpace($l)) {
        switch ($Node.kind) {
            'startEvent' { $l = 'Inicio' }
            'endEvent'   { $l = 'Fim' }
            'timerEvent' { $l = Get-TimerLabel $Node }
            'gateway' {
                $answered = $(if ($ProcessName) { Get-AnsweredLabel -Node $Node -ProcessName $ProcessName } else { $null })
                if ($answered) { $l = $answered }
                elseif ($SuggestedGatewayLabels.ContainsKey($Node.id)) { $l = $SuggestedGatewayLabels[$Node.id] }
                else {
                    $incoming = @($Edges | Where-Object { $_.to -eq $Node.id }).Count
                    $outgoing = @($Edges | Where-Object { $_.from -eq $Node.id }).Count
                    $l = if ($incoming -gt 1 -and $outgoing -le 1) { 'Convergencia' } else { '(sem rotulo)' }
                }
            }
            default { $l = '(sem rotulo)' }
        }
    }
    return ($l -replace '"', "'") -replace '\r?\n', ' '
}

function MermaidShape {
    param($Node, $Edges, [string]$ProcessName = '')
    $id = MermaidId $Node.id
    $lb = MermaidLabel $Node $Edges $ProcessName
    switch -Regex ($Node.kind) {
        '^(start|end)Event$'      { return "$id((`"$lb`"))" }
        'Event$'                  { return "$id(((`"$lb`")))" }
        '^gateway$'               { return "$id{`"$lb`"}" }
        '^subProcessScope$'       { return "$id[[`"$lb`"]]" }
        '^callActivity$'          { return "$id[[`"$lb`"]]" }
        default                   { return "$id[`"$lb`"]" }
    }
}

$pages = [System.Collections.Generic.List[string]]::new()
function Write-Page {
    param([string]$RelPath, [string[]]$Lines)
    $full = Join-Path $OutDir $RelPath
    $dir  = Split-Path -Parent $full
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $text = $Lines -join "`r`n"
    # O servidor de desenvolvimento do Docusaurus observa esta pasta e tranca cada
    # ficheiro enquanto o recarrega; com muitas paginas a espera precisa de folga.
    for ($try = 1; $try -le 12; $try++) {
        try { $text | Set-Content -LiteralPath $full -Encoding UTF8; break }
        catch { if ($try -eq 12) { throw }; Start-Sleep -Milliseconds (100 * $try) }
    }
    $pages.Add($RelPath)
}

function Frontmatter {
    param([string]$Title, [int]$Position, [string]$Description = '', [string]$Slug = '')
    $fm = @('---', "title: $Title", "sidebar_position: $Position")
    if ($Description) { $fm += "description: $Description" }
    if ($Slug) { $fm += "slug: $Slug" }
    $fm += @('---', '')
    return $fm
}

# --------------------------------------------------------------- indexes ----

$hazardsByProcess = @{}
foreach ($h in @($model.derived.migrationHazards)) {
    if (-not $hazardsByProcess.ContainsKey($h.process)) { $hazardsByProcess[$h.process] = [System.Collections.Generic.List[object]]::new() }
    $hazardsByProcess[$h.process].Add($h)
}

$fieldByName = @{}
foreach ($f in @($fields.fields)) { $fieldByName[$f.name] = $f }

# ------------------------------------------------------------------ intro ----

$L = Frontmatter -Title 'Visao geral' -Position 1 -Description "Documentacao gerada do pacote TIBCO $Package" -Slug '/'
$L += @(
    "# $Package - sistema legado TIBCO",
    '',
    ':::info Documentacao gerada',
    'Todo o conteudo deste site e projetado a partir dos artefatos em `artifacts/' + $Package + '/`.',
    'Nada aqui e escrito a mao: uma nova extracao regenera o site inteiro, entao a documentacao',
    'nunca fica defasada em relacao ao codigo-fonte legado.',
    ':::',
    '',
    '## O que e este sistema',
    '',
    'ePAT - processo administrativo tributario da SEFAZ-SP, implementado em TIBCO ActiveMatrix BPM',
    '(iProcess). O pacote analisado contem o fluxo do AIIM (Auto de Infracao e Imposicao de Multa),',
    'suas notificacoes/intimacoes e o servico de decisao em Corticon.',
    '',
    '## Numeros',
    '',
    '| Dimensao | Quantidade |',
    '|---|---|'
)
foreach ($c in $manifest.counts.PSObject.Properties) {
    $L += "| $($c.Name) | $($c.Value) |"
}
$L += @(
    '',
    '## Fontes analisadas',
    '',
    'Cada arquivo e fixado por hash na extracao; qualquer alteracao na origem e detectada.',
    '',
    '| Arquivo | Bytes | sha256 |',
    '|---|---:|---|'
)
foreach ($s in $manifest.sources) {
    $L += "| $(Cell (Split-Path -Leaf $s.file)) | $($s.bytes) | $(Code $s.sha256.Substring(0,16)) |"
}
$L += @(
    '',
    "Extracao gerada em $(Code $manifest.generatedAt) em $($manifest.durationMs) ms.",
    '',
    '## Pacotes externos referenciados e NAO entregues',
    '',
    'O XPDL referencia pacotes cujos arquivos nao foram fornecidos. Toda chamada para eles',
    'aparece na documentacao como destino nao resolvido.',
    ''
)
$ext = @($model.externalPackages.PSObject.Properties | ForEach-Object { $_.Name })
if ($ext.Count -gt 0) {
    $L += ($ext | ForEach-Object { "- ``$_``" })
}
else { $L += '_(nenhum)_' }
Write-Page 'intro.mdx' $L

# ------------------------------------------------------------ conformance ----

if ($conformance) {
    $CF = Frontmatter -Title 'Conformidade com a POC' -Position 2
    $sum = $conformance.summary
    $CF += @(
        '# Conformidade com os conceitos exigidos pela POC',
        '',
        ':::info Duas dimensoes, nao uma',
        '**Extracao** = o conceito foi encontrado na fonte e esta descrito pelos artefatos.',
        '**Execucao** = o conceito foi demonstrado rodando na plataforma alvo.',
        'Um conceito extraido nao e um conceito provado - por isso as duas colunas sao separadas.',
        ':::',
        '',
        "> $($conformance.objective)",
        '',
        "_$($conformance.coverageClaim)_",
        '',
        '## Placar',
        '',
        '| Dimensao | Resultado |',
        '|---|---:|',
        "| Conceitos exigidos | $($sum.concepts) |",
        "| Extraidos e evidenciados | **$($sum.extractionVerified)** |",
        "| Sem evidencia na fonte | $($sum.extractionAbsent) |",
        "| Provados em execucao | **$($sum.executionProven)** |",
        "| Aguardando execucao | $($sum.executionPending) |",
        "| Conceitos com construcao sem equivalente .NET | $($sum.conceptsBlocked) |",
        "| Etapas ligadas ao modelo | $($sum.etapas - $sum.etapasUnlinked) de $($sum.etapas) |",
        '',
        '## Conceitos',
        '',
        '| Conceito | Ocorrencias | Extracao | Execucao | Bloqueio |',
        '|---|---:|---|---|---|'
    )
    foreach ($cpt in $conformance.concepts) {
        $nm = if ($cpt.headline) { "**$(Cell $cpt.name)**" } else { Cell $cpt.name }
        $ex = if ($cpt.extraction -eq 'verified') { 'ok' } else { 'ausente' }
        $ev = if ($cpt.execution -eq 'proven') { '**provado**' } else { 'pendente' }
        $bl = if (@($cpt.blockers).Count -gt 0) { (@($cpt.blockers) | ForEach-Object { Code $_ }) -join ' ' } else { '' }
        $CF += "| $nm | $($cpt.occurrences) | $ex | $ev | $bl |"
    }
    $CF += ''

    $headline = @($conformance.concepts | Where-Object { $_.headline })
    if ($headline.Count -gt 0) {
        $CF += @(
            '## Os tres conceitos que o documento destaca',
            '',
            'Sao os unicos com secao propria de "Importancia da Validacao" no documento do cliente.',
            ''
        )
        foreach ($cpt in $headline) {
            $CF += @("### $(Cell $cpt.name)", '', (Cell $cpt.objective), '')
            if ($cpt.execution -eq 'proven') {
                $CF += @(':::tip Provado', (Cell $cpt.executionEvidence), ':::', '')
            }
            elseif (@($cpt.blockers).Count -gt 0) {
                $CF += @(':::danger Bloqueado',
                    "Construcao sem equivalente em .NET: $(Cell (@($cpt.blockers) -join ', ')). Ver [lacunas de migracao](/migration-gaps).",
                    ':::', '')
            }
            else {
                $CF += @(':::caution Extraido, ainda nao executado',
                    "$($cpt.occurrences) ocorrencia(s) localizada(s). Falta demonstrar em execucao na plataforma alvo.",
                    ':::', '')
            }
            foreach ($evi in @($cpt.evidence)) {
                $line = "- $(Cell $evi.what): **$($evi.count)**"
                if ($evi.processes) { $line += " em $(Cell ($evi.processes -join ', '))" }
                $CF += $line
                if ($evi.note) { $CF += "  - $(Cell $evi.note)" }
            }
            $CF += ''
        }
    }

    $CF += @(
        '## Etapas do fluxo',
        '',
        'As etapas e seus elementos vem de `intent-map.json`, derivado do proprio documento:',
        'um elemento so e ligado a uma etapa quando o nome aparece literalmente no texto.',
        '',
        '| # | Etapa | Processos | Situacao |',
        '|---:|---|---|---|'
    )
    foreach ($et in $conformance.etapas) {
        $CF += "| $($et.n) | $(Cell $et.name) | $(Cell ($et.processes -join ', ')) | $(Cell $et.status) |"
    }
    $CF += @(
        '',
        '## Resultados esperados (secao 5 do documento)',
        '',
        '| Resultado | Extracao | Execucao |',
        '|---|---|---|'
    )
    foreach ($xr in $conformance.expectedResults) {
        $CF += "| $(Cell $xr.text) | $(Cell $xr.extraction) | $(Cell $xr.execution) |"
    }
    $CF += ''
    Write-Page 'conformance.mdx' $CF
}

# -------------------------------------------------------------- processes ----

$catL = Frontmatter -Title 'Processos' -Position 1
$catL += @(
    '# Processos',
    '',
    "O pacote contem $(@($model.processes).Count) processos. Cada pagina traz o fluxo, os passos,",
    'as decisoes com suas condicoes reais e as construcoes que nao tem equivalente em .NET.',
    '',
    '| Processo | Escopos | Passos | Fluxos | Riscos de migracao |',
    '|---|---:|---:|---:|---:|'
)

foreach ($proc in $model.processes) {
    $nodeCount = 0; $edgeCount = 0
    foreach ($s in $proc.scopes) { $nodeCount += @($s.nodes).Count; $edgeCount += @($s.edges).Count }
    $hz = @($hazardsByProcess[$proc.name]).Count
    $slug = Slug $proc.name
    $catL += "| [$(Cell $proc.name)](./$slug) | $(@($proc.scopes).Count) | $nodeCount | $edgeCount | $hz |"
}
Write-Page 'processes/index.mdx' $catL

$pos = 2
foreach ($proc in $model.processes) {
    $slug = Slug $proc.name
    $P = Frontmatter -Title $proc.name -Position $pos
    $pos++
    $P += @("# Processo $($proc.name)", '')
    if ($proc.description) { $P += @($proc.description, '') }

    foreach ($scope in $proc.scopes) {
        $nodes = @($scope.nodes)
        $edges = @($scope.edges)
        $nodeById = @{}
        foreach ($n in $nodes) { $nodeById[$n.id] = $n }

        $P += @("## Escopo ``$($scope.scope)``", '', "$($nodes.Count) passos, $($edges.Count) fluxos.", '')

        # flow diagram
        $P += @('```mermaid', 'flowchart TD')
        foreach ($n in $nodes) { $P += '    ' + (MermaidShape $n $edges $proc.name) }
        foreach ($e in $edges) {
            if (-not $nodeById.ContainsKey($e.from) -or -not $nodeById.ContainsKey($e.to)) { continue }
            $a = MermaidId $e.from
            $b = MermaidId $e.to
            $lab = $e.label
            if ([string]::IsNullOrWhiteSpace($lab) -and $e.conditionType -eq 'OTHERWISE') { $lab = 'senao' }
            if ([string]::IsNullOrWhiteSpace($lab)) { $P += "    $a --> $b" }
            else {
                $lab = ($lab -replace '"', "'") -replace '\r?\n', ' '
                $P += "    $a -- `"$lab`" --> $b"
            }
        }
        $P += @('```', '')

        # steps
        $P += @('### Passos', '', '| # | Passo | Tipo | Raia | Revisao humana |', '|---:|---|---|---|:---:|')
        foreach ($n in ($nodes | Sort-Object { [int]($_.stepIndex ?? 999) })) {
            $label = MermaidLabel $n $edges $proc.name
            $review = if (Test-NeedsHumanReview -Node $n -ProcessName $proc.name) { 'Sim' } else { 'Nao' }
            $P += "| $(Cell $n.stepIndex) | $(Cell $label) | $(Cell $n.kind) | $(Cell $n.lane) | $review |"
        }
        $P += ''

        # decisions
        $conds = @($edges | Where-Object { $_.condition })
        if ($conds.Count -gt 0) {
            $P += @('### Decisoes e condicoes', '', '| Decisao | Ramo | Condicao | Leva a | Revisao humana |', '|---|---|---|---|:---:|')
            foreach ($e in $conds) {
                $from = if ($nodeById.ContainsKey($e.from)) { MermaidLabel $nodeById[$e.from] $edges $proc.name } else { $e.from }
                $to   = if ($nodeById.ContainsKey($e.to))   { MermaidLabel $nodeById[$e.to] $edges $proc.name }   else { $e.to }
                $review = if ($nodeById.ContainsKey($e.from) -and (Test-NeedsHumanReview -Node $nodeById[$e.from] -ProcessName $proc.name)) { 'Sim' } else { 'Nao' }
                $P += "| $(Cell $from) | $(Cell $e.label) | $(Code $e.condition) | $(Cell $to) | $review |"
            }
            $P += ''
        }
    }

    $hz = @($hazardsByProcess[$proc.name])
    if ($hz.Count -gt 0) {
        $P += @(
            '## Construcoes sem equivalente em .NET',
            '',
            ':::warning Exige decisao humana',
            "Este processo usa $($hz.Count) construcao(oes) do iProcess que nao tem traducao direta.",
            'Ver [lacunas de migracao](/migration-gaps).',
            ':::',
            '',
            '| Passo | Categoria | Severidade | Detalhe |',
            '|---|---|---|---|'
        )
        foreach ($h in ($hz | Sort-Object category, node)) {
            $P += "| $(Cell $h.node) | $(Cell $h.category) | $(Cell $h.severity) | $(Cell $h.detail) |"
        }
        $P += ''
    }

    Write-Page "processes/$slug.mdx" $P
}

# ------------------------------------------------------------- data model ----

$D = Frontmatter -Title 'Modelo de dados' -Position 3
$D += @(
    '# Modelo de dados',
    '',
    "$(@($fields.fields).Count) campos de caso. O identificador original do TIBCO e preservado:",
    'nomes de negocio, quando existirem, vem do glossario e nao substituem o identificador.',
    '',
    '## Resumo',
    '',
    '| Dimensao | Quantidade |',
    '|---|---:|',
    "| Campos de caso | $(@($fields.fields).Count) |",
    "| Usam o sentinela SW_NA (tres estados) | $(@($fields.fields | Where-Object usesSwNaSentinel).Count) |",
    "| Nome truncado pelo iProcess | $(@($fields.fields | Where-Object nameTruncated).Count) |",
    "| Com rotulo sugerido pelo formulario | $(@($fields.fields | Where-Object labelSuggestion).Count) |",
    "| Nao referenciados por nenhum passo | $(@($fields.fields | Where-Object { -not $_.usedInForm -and -not $_.boundToService }).Count) |",
    "| Campos tecnicos (fora do dominio) | $(@($fields.technicalFields).Count) |",
    '',
    ':::caution O sentinela SW_NA',
    'O iProcess distingue tres estados: valor definido, `SW_NA` (nao disponivel) e vazio.',
    'C# nao possui esse terceiro estado. Traduzir `SW_NA` para `null` funde dois estados',
    'distintos e muda silenciosamente qual ramo do fluxo dispara.',
    ':::',
    '',
    '## Campos',
    '',
    '| Campo | Tipo | Tam. | SW_NA | Nome completo | Rotulo sugerido (nao verificado) |',
    '|---|---|---:|:---:|---|---|'
)
foreach ($f in ($fields.fields | Sort-Object name)) {
    $sw = if ($f.usesSwNaSentinel) { 'sim' } else { '' }
    $D += "| $(Code $f.name) | $(Cell $f.clrType) | $(Cell $f.maxLength) | $sw | $(Cell $f.fullName) | $(Cell $f.labelSuggestion) |"
}
if (@($fields.technicalFields).Count -gt 0) {
    $D += @(
        '',
        '## Campos tecnicos',
        '',
        'Declarados nos formularios TIBCO, porem fora do modelo de dominio: pertencem ao envelope',
        'de servico ou ao motor. Ainda assim decidem ramificacoes, entao o modelo .NET precisa expo-los.',
        '',
        '| Campo | Tipo | Direcao | Declarado em |',
        '|---|---|---|---|'
    )
    foreach ($t in ($fields.technicalFields | Sort-Object name)) {
        $D += "| $(Code $t.name) | $(Cell $t.declaredType) | $(Cell $t.inout) | $(Cell ($t.declaredIn -join ', ')) |"
    }
}
Write-Page 'data-model.mdx' $D

# ----------------------------------------------------------- integrations ----

$I = Frontmatter -Title 'Integracoes' -Position 4
$I += @(
    '# Integracoes',
    '',
    '| Dimensao | Quantidade |',
    '|---|---:|',
    "| Operacoes catalogadas | $($manifest.counts.operations) |",
    "| Operacoes efetivamente chamadas pelo processo | $($manifest.counts.invokedOperations) |",
    '',
    ':::note',
    'Apenas as operacoes chamadas precisam de implementacao; as demais formam o contrato',
    'disponivel. Nesta PoC as chamadas de integracao sao substituidas por mocks tipados,',
    'derivados do proprio XSD de resposta.',
    ':::',
    ''
)
$invoked = @($services.processBindings)
if ($invoked.Count -gt 0) {
    $I += @('## Operacoes chamadas pelo processo', '', '| Operacao | Processo | Escopo | Passo |', '|---|---|---|---|')
    foreach ($o in $invoked) {
        $I += "| $(Code $o.operationName) | $(Cell $o.process) | $(Cell $o.scope) | $(Cell $o.node) |"
    }
    $I += ''
}
foreach ($svc in @($services.services)) {
    $I += @("## $(Cell $svc.file)", '')
    foreach ($ep in @($svc.endpoints)) {
        $I += "- servico ``$($ep.service)`` / porta ``$($ep.port)`` - transporte: $(Cell $ep.transport)"
    }
    $I += ''
    if ($svc.technicalEnvelope) {
        $I += @('### Envelope tecnico', '', '| Bloco | Elementos |', '|---|---|')
        foreach ($b in $svc.technicalEnvelope.PSObject.Properties) {
            $names = (@($b.Value) | ForEach-Object { $_.name }) -join ', '
            $I += "| $(Cell $b.Name) | $(Code $names) |"
        }
        $I += ''
    }
}
Write-Page 'integrations.mdx' $I

# --------------------------------------------------------- business rules ----

# As tres fontes de regra vivem numa seccao unica: quem procura uma regra nao sabe,
# a partida, em qual dos tres sitios o legado a foi guardar.
Write-Page 'regras/_category_.json' @(
    '{',
    '  "label": "Regras de negocio",',
    '  "position": 5,',
    '  "link": { "type": "doc", "id": "regras/index" }',
    '}'
)

$totalCorticon = [int]$manifest.counts.decisionRules
$totalXpdl     = $(if ($scriptRules) { [int]$scriptRules.summary.regraDeNegocio } else { 0 })
$totalTelas    = $(if ($screenRules) { [int]$screenRules.summary.regraDeNegocio } else { 0 })
$totalRegras   = $(if ($ruleCatalogue) { [int]$ruleCatalogue.summary.regraDeNegocio } else { $totalCorticon + $totalXpdl + $totalTelas })

$RX = Frontmatter -Title 'Regras de negocio' -Position 5 -Slug '/regras'
$RX += @(
    '# Onde estao as regras de negocio',
    '',
    'A regra de negocio deste sistema nao vive num sitio. Vive em tres, e nenhum deles',
    'sabe da existencia dos outros. Esta seccao junta os tres num so lugar, com o mesmo',
    'criterio de classificacao, para que as contagens possam ser somadas.',
    '',
    '## Os dois eixos',
    '',
    'Cada regra e descrita por duas dimensoes independentes. O **efeito** diz o que ela',
    'faz ao caso. O **portador** diz onde ela esta escrita, e portanto o que e preciso',
    'fazer para a mudar - que e a dimensao que interessa a migracao.',
    ''
)

if ($ruleCatalogue) {
    $RX += @(
        '### Por efeito',
        '',
        '| Efeito | Regras | O que quer dizer |',
        '|---|---:|---|'
    )
    foreach ($e in $ruleCatalogue.summary.porEfeito.PSObject.Properties) {
        $RX += "| $(Code $e.Name) | $($e.Value.total) | $(Cell $e.Value.descricao) |"
    }

    $RX += @(
        '',
        '### Por portador',
        '',
        'A ultima coluna e a que conta: separa o que um analista consegue alterar do que',
        'obriga a reimplantar o processo ou a recompilar a aplicacao.',
        '',
        '| Portador | Pontos | Regras de negocio | Para mudar e preciso |',
        '|---|---:|---:|---|'
    )
    foreach ($p in $ruleCatalogue.summary.porPortador.PSObject.Properties) {
        $RX += "| $(Code $p.Name) | $($p.Value.total) | $($p.Value.regraDeNegocio) | $(Code $p.Value.alteracaoRequer) |"
    }

    $RX += @('', '### Efeito por portador', '', 'O mesmo efeito escrito em sitios diferentes: e isto que impede alguem de mudar', 'uma regra num sitio so.', '')
    $portadores = @($ruleCatalogue.summary.porPortador.PSObject.Properties.Name)
    $RX += ('| Efeito | ' + (($portadores | ForEach-Object { Code $_ }) -join ' | ') + ' | Total |')
    $RX += ('|---|' + (($portadores | ForEach-Object { '---:' }) -join '|') + '|---:|')
    foreach ($e in $ruleCatalogue.summary.porEfeito.PSObject.Properties) {
        $celulas = foreach ($pn in $portadores) {
            $v = $e.Value.porPortador.PSObject.Properties[$pn]
            $(if ($v) { $v.Value } else { '' })
        }
        $RX += ('| ' + (Code $e.Name) + ' | ' + ($celulas -join ' | ') + " | **$($e.Value.total)** |")
    }

    $RX += @(
        '',
        '## O que isto significa para a migracao',
        '',
        '| Para mudar a regra e preciso | Regras |',
        '|---|---:|'
    )
    foreach ($a in $ruleCatalogue.summary.porAlteracao.PSObject.Properties) {
        $RX += "| $(Code $a.Name) | $($a.Value) |"
    }
    $pctPlanilha = [Math]::Round(100 * [int]$ruleCatalogue.summary.porAlteracao.'publicar-planilha' / [Math]::Max(1, $totalRegras))
    $RX += @(
        '',
        ':::warning A planilha e a minoria',
        "Das $totalRegras regras de negocio, so $pctPlanilha% podem ser alteradas publicando a planilha.",
        'As restantes estao soldadas ao fluxo ou ao ecra. O documento da POC pede a separacao',
        'entre fluxo operacional e regra de negocio; hoje ela nao existe.',
        ':::',
        ''
    )
}

$RX += @(
    '## As tres fontes',
    '',
    '| Onde | Regras | O que la esta | Pagina |',
    '|---|---:|---|---|',
    "| Planilha Corticon | $totalCorticon | colunas de regra do servico de decisao, ja convertidas para DMN e provadas por execucao | [Corticon](/regras/corticon) |",
    "| Diagrama XPDL | $totalXpdl | script, condicao de transicao, prazo, mapeamento de dados e script de formulario | [XPDL](/regras/xpdl) |",
    "| Code-behind das telas | $totalTelas | decisao que escreve no motor, validacao e leitura de campo do caso | [Telas](/regras/telas) |",
    '',
    '## Como se comunicam',
    '',
    'Nenhuma regra chama outra. Todas escrevem e leem o mesmo sitio: o campo do caso.',
    'E um quadro-negro, nao uma interface.',
    '',
    '```mermaid',
    'flowchart LR',
    '    C["Corticon"] -->|11 saidas| F[("Campo do caso")]',
    '    T["Tela .aspx.cs"] -->|WorkItemReleaseField| F',
    '    F --> X["transition / script"]',
    '    X --> F',
    '    F --> T',
    '    F --> C',
    '```',
    '',
    'Como ninguem sabe quem escreveu o valor que esta a ler, uma regra pode desfazer outra',
    'em silencio. E desfaz: `INDRESPPRM` e calculado pelo Corticon e reescrito logo a seguir',
    'por um mapeamento de dados no passo Prepara Intimacao.',
    '',
    '## Como ler cada regra',
    '',
    'Cada regra aparece com a expressao original e com uma **leitura derivada**: a mesma expressao',
    'escrita por extenso, trocando o identificador pelo rotulo que o pacote declara e o operador',
    'pela palavra correspondente. A leitura nao acrescenta significado nenhum.',
    '',
    'Quando o pacote nao diz o que um valor quer dizer, a leitura diz isso mesmo, em vez de adivinhar.',
    'Essas lacunas estao listadas em [questoes em aberto](/open-questions).',
    ''
)
Write-Page 'regras/index.mdx' $RX

$R = Frontmatter -Title 'Corticon' -Position 1
$R += @(
    '# Regras de negocio (Corticon)',
    '',
    'O servico de decisao das intimacoes e uma planilha de regras Corticon. Ela foi convertida',
    'para DMN e a equivalencia foi verificada por execucao, nao por inspecao.',
    '',
    '| Dimensao | Quantidade |',
    '|---|---:|',
    "| Colunas de regra | $($manifest.counts.decisionRules) |",
    "| Termos de vocabulario | $(@($decisions.vocabulary).Count) |",
    "| Linhas de condicao | $(@($decisions.conditionRows).Count) |",
    "| Linhas de acao | $(@($decisions.actionRows).Count) |",
    '',
    ':::tip Equivalencia comprovada',
    'O DMN emitido e as regras Corticon originais foram executados sobre 3.000 casos aleatorios',
    'cobrindo 11 atributos de saida, sem nenhuma divergencia. As regras nao foram apenas',
    'traduzidas: a traducao foi demonstrada.',
    ':::',
    '',
    ':::warning Esta nao e a unica fonte de regra',
    'Ha regra de negocio tambem em scripts, condicoes de transicao, prazos, mapeamentos de dados',
    'e no script de submissao do formulario. Ver [regras no XPDL](/regras/xpdl) e',
    '[regras dentro das telas](/regras/telas).',
    ':::',
    '',
    ':::caution Ordem das regras importa',
    'A Corticon resolve conflitos por sobreposicao (uma regra sobrescreve outra). A ordem das',
    '49 colunas e preservada no espelho DMN em modo RULE ORDER. Reordenar as regras muda o resultado.',
    ':::',
    ''
)
if (@($decisions.conditionRows).Count -gt 0) {
    $R += @('## Condicoes avaliadas', '', '| # | Expressao |', '|---:|---|')
    $i = 1
    foreach ($c in @($decisions.conditionRows)) {
        $expr = $c.expression; if (-not $expr) { $expr = $c.lhs }
        $R += "| $i | $(Code $expr) |"
        $i++
    }
    $R += ''
}
if (@($decisions.actionRows).Count -gt 0) {
    $R += @('## Atributos definidos pelas regras', '', '| # | Atribuicao |', '|---:|---|')
    $i = 1
    foreach ($a in @($decisions.actionRows)) {
        $expr = $a.expression; if (-not $expr) { $expr = $a.lhs }
        $R += "| $i | $(Code $expr) |"
        $i++
    }
    $R += ''
}
Write-Page 'regras/corticon.mdx' $R

# ------------------------------------------------- regras dentro de scripts ----

if ($scriptRules) {
    $SR = Frontmatter -Title 'XPDL' -Position 2
    $sm = $scriptRules.summary
    $SR += @(
        '# Regras de negocio espalhadas pelo XPDL',
        '',
        ':::warning O Corticon nao e a unica fonte',
        "O XPDL guarda logica de decisao em cinco portadores diferentes. Ao todo sao $($sm.total) pontos,",
        "dos quais **$($sm.regraDeNegocio) sao regra de negocio** e $($sm.tecnico) sao tecnicos - retry, nulo e envelope de erro.",
        ':::',
        '',
        'O documento da POC pede "separacao entre fluxo operacional e regras de negocio".',
        'Estas regras estao do lado errado dessa fronteira: alterar qualquer uma delas exige mexer no fluxo.',
        '',
        '## Por efeito',
        '',
        '| Efeito | Pontos |',
        '|---|---:|'
    )
    foreach ($e in $sm.byEfeito.PSObject.Properties) {
        $SR += "| $(Code $e.Name) | $($e.Value) |"
    }
    $SR += @(
        '',
        '## Por portador',
        '',
        '| Portador | Total | Regra de negocio | Na trilha da POC |',
        '|---|---:|---:|---:|'
    )
    foreach ($p in $sm.bySource.PSObject.Properties) {
        $SR += "| $(Code $p.Name) | $($p.Value.total) | $($p.Value.regraDeNegocio) | $($p.Value.naTrilhaPoc) |"
    }
    $SR += @(
        '',
        '## Por escopo',
        '',
        '| Escopo | Regras |',
        '|---|---:|',
        "| Na trilha narrada pela POC (etapas 1-7) | **$($sm.naTrilhaPoc)** |",
        "| Nos subprocessos de servico que a trilha invoca | $($sm.foraDaTrilhaPoc) |",
        '',
        "Trilha: $((@($scriptRules.pocFlowProcesses) | ForEach-Object { Code $_ }) -join ', ')",
        '',
        ':::note Derivado e autorado',
        'Expressoes, campos e literais sao **derivados** do XPDL. As explicacoes sao **autoradas**',
        "e ainda **nao foram verificadas** com quem opera o processo hoje ($($sm.comExplicacao) explicadas).",
        ':::',
        ''
    )

    foreach ($escopo in @($true, $false)) {
        $lista = @($scriptRules.rules | Where-Object {
            $_.classification.eRegraDeNegocio -and $_.inPocFlow -eq $escopo
        })
        if ($lista.Count -eq 0) { continue }
        $titulo = $(if ($escopo) { 'Na trilha da POC' } else { 'Fora da trilha (subprocessos de servico)' })
        $SR += @("## $titulo", '', "$($lista.Count) regra(s).", '')

        foreach ($rule in $lista) {
            $SR += @(
                "### $(Cell $rule.process) / $(Cell $rule.node)",
                '',
                "$(Code $rule.source) &middot; $(Code $rule.classification.efeito) &middot; XPDL linha $($rule.xpdlLine)",
                ''
            )
            if ($rule.expression) { $SR += @('```javascript', ($rule.expression), '```', '') }
            elseif ((Arr $rule.conditions).Count -gt 0) {
                $SR += @('```javascript')
                foreach ($c in (Arr $rule.conditions)) { $SR += $c }
                $SR += @('```', '')
            }
            $SR += Format-Reading $rule.leitura
            if ($rule.explanation) {
                $SR += @('**' + (Cell $rule.explanation.summary) + '**', '', (Cell $rule.explanation.detail), '')
                foreach ($f in (Arr $rule.explanation.findings)) { $SR += "- $(Cell $f)" }
                if ((Arr $rule.explanation.findings).Count -gt 0) { $SR += '' }
                if ($rule.explanation.migration) {
                    $SR += @(':::tip Na migracao', (Cell $rule.explanation.migration), ':::', '')
                }
                $SR += @("_$(Cell $rule.explanation.source)_", '')
            }
            if ((Arr $rule.valueDomain).Count -gt 0) {
                foreach ($g in (Arr $rule.valueDomain | Group-Object { $_.field })) {
                    $vals = (@($g.Group | ForEach-Object { $_.value } | Sort-Object -Unique) | ForEach-Object { Code $_ }) -join ', '
                    $SR += "- Valores atribuidos a $(Code $g.Name): $vals"
                }
                $SR += ''
            }
            $comentarios = @(Arr $rule.authorComments | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
            if ($comentarios.Count -gt 0) {
                $SR += @('<details>', '<summary>Comentarios do autor</summary>', '')
                foreach ($c in $comentarios) { $SR += "- $(Code $c)" }
                $SR += @('', '</details>', '')
            }
        }
    }
    Write-Page 'regras/xpdl.mdx' $SR
}

# --------------------------------------------- regras dentro das telas .cs ----

if ($screenRules) {
    $KR = Frontmatter -Title 'Telas' -Position 3
    $ks = $screenRules.summary
    $totalLinhas = (@($screenRules.screens | ForEach-Object { [int]$_.metrics.lines }) | Measure-Object -Sum).Sum
    $KR += @(
        '# Regras de negocio no code-behind das telas',
        '',
        ':::warning A maior fonte de regra nao esta no diagrama',
        "As $($ks.screens) telas entregues carregam **$totalLinhas linhas de C#** com **$($ks.decisions) decisoes**,",
        "das quais **$($ks.regraDeNegocio) tem peso de negocio**. Nenhuma aparece no XPDL nem na planilha Corticon.",
        ':::',
        '',
        '## Por efeito',
        '',
        '| Efeito | Decisoes |',
        '|---|---:|'
    )
    foreach ($e in $ks.byClassification.PSObject.Properties) {
        $KR += "| $(Code $e.Name) | $($e.Value) |"
    }
    $KR += @(
        '',
        '## Por tela',
        '',
        '| Tela | Linhas | Metodos | Decisoes | Com peso de negocio | Processo | Na trilha |',
        '|---|---:|---:|---:|---:|---|---|'
    )
    foreach ($s in $screenRules.screens) {
        $KR += "| $(Cell $s.codeBehind) | $($s.metrics.lines) | $($s.metrics.methods) | $($s.metrics.decisions) | $($s.metrics.regraDeNegocio) | $((@($s.processes) | ForEach-Object { Code $_ }) -join ', ') | $(if ($s.inPocFlow) { 'sim' } else { 'nao' }) |"
    }

    $writes = @($screenRules.screens | ForEach-Object { $sc = $_; @($_.engineWrites) | Where-Object { $_ } | ForEach-Object { $_ | Add-Member -NotePropertyName tela -NotePropertyValue $sc.codeBehind -PassThru -Force } })
    if ($writes.Count -gt 0) {
        $KR += @(
            '',
            '## Contrato tela para processo',
            '',
            'Este e o ponto que o XPDL sozinho nao mostra: o valor que faz o processo seguir por um ramo',
            'ou por outro nao nasce no diagrama, nasce aqui, no clique do utilizador.',
            '',
            '| Campo | Valor | Tipo | Condicao que o produz | Tela | Linha | Declarado no XPDL |',
            '|---|---|---|---|---|---:|---|'
        )
        foreach ($w in $writes) {
            $KR += "| $(Code $w.field) | $(Code $w.value) | $(Code $w.swType) | $(Cell $w.guardedBy) | $(Cell ([IO.Path]::GetFileName($w.tela))) | $($w.line) | $(if ($w.declaredInXpdl) { 'sim' } else { '**nao**' }) |"
        }
    }

    if (@($screenRules.condicoesQueNaoDecidem).Count -gt 0) {
        $KR += @('', '## Condicoes que nao decidem nada', '')
        foreach ($c in @($screenRules.condicoesQueNaoDecidem)) {
            $KR += @("### $(Cell ([IO.Path]::GetFileName($c.codeBehind))) &middot; $(Cell $c.method) &middot; linha $($c.line)", '', '```csharp', $c.condition, '```', '')
            foreach ($f in @($c.findings)) { $KR += @(":::danger $(Cell $f.tipo)", (Cell $f.detalhe), ':::', '') }
        }
    }

    $div = @($screenRules.metodosClonados | Where-Object { $_.divergente })
    if ($div.Count -gt 0) {
        $KR += @(
            '',
            '## Metodos clonados que ja divergiram',
            '',
            "$($div.Count) metodo(s) existem com o mesmo nome nas duas telas, mas com decisoes diferentes.",
            'A copia foi feita e depois so um dos lados foi corrigido.',
            ''
        )
        foreach ($m in $div) {
            $KR += @(
                "### $(Cell $m.method)",
                '',
                '| Tela | Decisoes |',
                '|---|---:|'
            )
            foreach ($p in @($m.screens)) { $KR += "| $(Cell ([IO.Path]::GetFileName($p.codeBehind))) | $($p.decisoes) |" }
            $KR += @('', 'Condicoes que existem so de um lado:', '')
            foreach ($e in @($m.condicoesExclusivas)) {
                $KR += @("**$(Cell ([IO.Path]::GetFileName($e.codeBehind)))**", '', '```csharp', $e.condition, '```', '')
            }
        }
    }

    $calls = @($screenRules.screens | ForEach-Object { $sc = $_; @($_.backendCalls) | Where-Object { $_ } | ForEach-Object { $_ | Add-Member -NotePropertyName tela -NotePropertyValue $sc.codeBehind -PassThru -Force } })
    if ($calls.Count -gt 0) {
        $KR += @(
            '',
            '## Chamadas de backend a simular',
            '',
            'Cada uma destas e um mock a construir, e a condicao ao lado diz quando ele e chamado.',
            '',
            '| Chamada | Condicao | Tela | Linha |',
            '|---|---|---|---:|'
        )
        foreach ($c in ($calls | Sort-Object { $_.call })) {
            $KR += "| $(Code $c.call) | $(Cell $c.guardedBy) | $(Cell ([IO.Path]::GetFileName($c.tela))) | $($c.line) |"
        }
    }

    foreach ($s in $screenRules.screens) {
        $lista = @($s.decisions | Where-Object { $_.classification.eRegraDeNegocio })
        if ($lista.Count -eq 0) { continue }
        $KR += @('', "## $(Cell $s.codeBehind)", '', "$($lista.Count) decisoes com peso de negocio.", '')
        foreach ($d in $lista) {
            $KR += @(
                "### linha $($d.line) &middot; $(Cell $d.method)",
                '',
                "$(Code $d.classification.efeito) &middot; $(Code $d.kind)",
                '',
                '```csharp',
                $d.condition,
                '```',
                ''
            )
            $KR += Format-Reading $d.leitura
            if (@($d.readsCaseFields).Count -gt 0)  { $KR += @("Le do caso: $((@($d.readsCaseFields) | ForEach-Object { Code $_ }) -join ', ')", '') }
            if (@($d.readsEntityProps).Count -gt 0) { $KR += @("Le do AIIM: $((@($d.readsEntityProps) | ForEach-Object { Code $_ }) -join ', ')", '') }
            $ef = $d.effects
            if ($ef.engineWrites) { $KR += @("Escreve no motor: $((@($ef.engineWrites) | ForEach-Object { Code ($_.field + '=' + $_.value) }) -join ', ')", '') }
            if ($ef.engineApi)    { $KR += @("Chama o motor: $((@($ef.engineApi) | ForEach-Object { Code $_ }) -join ', ')", '') }
            if ($ef.backendCalls) { $KR += @("Chama o backend: $((@($ef.backendCalls) | ForEach-Object { Code $_ }) -join ', ')", '') }
            if ($ef.uiState)      { $KR += @("Muda o ecra: $((@($ef.uiState) | ForEach-Object { Code $_ }) -join ', ')", '') }
            if ($ef.messages) {
                $KR += @('Mostra ao utilizador:', '')
                foreach ($msg in @($ef.messages)) { $KR += "- $(Cell $msg)" }
                $KR += ''
            }
        }
    }

    Write-Page 'regras/telas.mdx' $KR
}

# -------------------------------------------------------- migration gaps ----

$G = Frontmatter -Title 'Lacunas e decisoes de migracao' -Position 6
$G += @(
    '# Lacunas e decisoes de migracao',
    '',
    'Estas construcoes **nao possuem traducao direta, um-para-um, para .NET**.',
    'Isso nao significa que sejam impossiveis de implementar: todas possuem opcoes tecnicas',
    'catalogadas abaixo. Continuam abertas porque a opcao sugerida ainda requer aprovacao humana',
    'e registro em `config/glossary/POC_Epat.yaml`.',
    '',
    ':::danger Prioridade maxima',
    'Enquanto estes pontos nao forem decididos, qualquer implementacao dos passos afetados e um',
    'palpite - e um palpite que falha em silencio: nao gera erro de compilacao nem teste vermelho.',
    ':::',
    '',
    "$(@($model.derived.migrationHazards).Count) ocorrencias, agrupadas em",
    "$(@($dossier.items | Where-Object { $_.category -eq 'no-net-equivalent' }).Count) construcoes.",
    'A decisao e tomada uma vez por construcao e vale para todas as ocorrencias.',
    ''
)
foreach ($item in @($dossier.items | Where-Object { $_.category -eq 'no-net-equivalent' })) {
    $G += @(
        "## $(Cell $item.subject)",
        '',
        "**Prioridade $($item.priority)** - $($item.occurrenceCount) ocorrencia(s) em $(Cell ($item.usedInProcesses -join ', '))",
        '',
        (Cell $item.briefing),
        ''
    )
    if (@($item.symbols).Count -gt 0) {
        $G += @("Simbolos: $((@($item.symbols) | ForEach-Object { Code $_ }) -join ', ')", '')
    }

    # What inspecting the XPDL revealed - the part that cannot be reconstructed
    # from the summary, and the reason several options changed.
    foreach ($f in @($item.findings | Where-Object { $_ })) {
        $G += @("### Descoberta: $(Cell $f.id)", '', (Cell $f.text), '')
        foreach ($e in @($f.evidence | Where-Object { $_ })) { $G += "- Evidencia: $(Code $e)" }
        foreach ($h in @($f.hypotheses | Where-Object { $_ })) { $G += "- Hipotese: $(Cell $h)" }
        if ($f.consequence) { $G += @('', ":::note Consequencia", (Cell $f.consequence), ':::') }
        $G += ''
    }

    if ($item.architectureNote) {
        $G += @(':::tip Nota de arquitetura', (Cell $item.architectureNote), ':::', '')
    }

    $G += Format-Hypothesis $item

    $G += @('### Opcoes', '', '| Opcao | Padrao de projeto | Abordagem | Consequencia | Sugerida |', '|---|---|---|---|:---:|')
    foreach ($o in @($item.suggestedOptions)) {
        $s = if ($o.suggested) { 'sim' } else { '' }
        $G += "| $(Code $o.id) | $(Cell $o.pattern) | $(Cell $o.approach) | $(Cell $o.consequence) | $s |"
    }
    $G += @(
        '',
        "Decisao registrada em ``$($item.resolution.answerIn)``, chave ``$($item.resolution.key)``.",
        ''
    )
    if (@($item.occurrences).Count -gt 0) {
        $G += @('<details>', '<summary>Ocorrencias</summary>', '', '| Processo | Passo | Detalhe |', '|---|---|---|')
        foreach ($oc in @($item.occurrences)) {
            $G += "| $(Cell $oc.process) | $(Cell $oc.node) | $(Cell $oc.detail) |"
        }
        $G += @('', '</details>', '')
    }
}
Write-Page 'migration-gaps.mdx' $G

# -------------------------------------------------------- open questions ----

$Q = Frontmatter -Title 'Questoes em aberto' -Position 7
$Q += @(
    '# Questoes que a migracao nao pode responder sozinha',
    '',
    "$($dossier.summary.total) itens, em ordem de prioridade.",
    "Respondidos: $($dossier.summary.answered). Em aberto: $($dossier.summary.open).",
    "**Bloqueadores: $($dossier.summary.blockers)** - ate serem decididos, os nos afetados nao devem ser gerados.",
    '',
    '| Prioridade | Itens |',
    '|---|---:|'
)
foreach ($p in $dossier.summary.byPriority.PSObject.Properties) {
    $Q += "| $($p.Name) | $($p.Value) |"
}
$Q += @(
    '',
    "$($dossier.summary.comEvidenciaDerivada) destas perguntas ja tem parte da resposta dentro do proprio pacote, e",
    "$($dossier.summary.comHipoteseAgentica) trazem uma hipotese de trabalho. A hipotese e **analise agentica nao verificada**:",
    'serve para transformar "o que significa isto?" em "confirma que e isto?", nunca para dispensar a resposta.',
    '',
    'As respostas vao para o glossario, nunca para os artefatos gerados.',
    '',
    '| Prio | Severidade | Categoria | Assunto | Confianca | Pergunta principal |',
    '|---|---|---|---|---|---|'
)
foreach ($it in $dossier.items) {
    # Not $q: PowerShell variables are case-insensitive, so it would overwrite the page.
    $question = @($it.questionsForAnalyst)[0]
    # Priority alone does not separate blockers: all six sit in P2 alongside four others.
    $sev = if ($it.severity -eq 'blocker') { '**bloqueador**' } else { Cell $it.severity }
    $Q += "| $(Cell $it.priority) | $sev | $(Cell $it.category) | $(Cell $it.subject) | $(Cell $it.confidence.level) | $(Cell $question) |"
}

$comHipotese = @($dossier.items | Where-Object { $_.analise -or ($_.evidenciaDerivada -and $_.evidenciaDerivada.dominioObservado) })
if ($comHipotese.Count -gt 0) {
    $Q += @('', '## Perguntas com hipotese de trabalho', '')
    foreach ($it in $comHipotese) {
        $Q += @("### $(Cell $it.subject)", '', "$(Code $it.id) &middot; $(Code $it.category)", '')
        $Q += Format-Hypothesis $it
    }
}
Write-Page 'open-questions.mdx' $Q

# ------------------------------------------------------------------ done ----

# O site e regeneravel, e isso so e verdade se uma pagina que deixou de ser
# produzida tambem desaparecer. Sem esta varredura, renomear uma pagina deixa a
# antiga viva no menu, a apontar para conteudo que ja ninguem actualiza.
$mantidos = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($p in $pages) { [void]$mantidos.Add([IO.Path]::GetFullPath((Join-Path $OutDir $p))) }

$removidos = @()
foreach ($f in Get-ChildItem -LiteralPath $OutDir -Recurse -File | Where-Object { $_.Extension -in '.mdx', '.md' -or $_.Name -eq '_category_.json' }) {
    if ($mantidos.Contains($f.FullName)) { continue }
    Remove-Item -LiteralPath $f.FullName -Force
    $removidos += $f.FullName.Substring($([IO.Path]::GetFullPath($OutDir)).Length).TrimStart('\', '/')
}
foreach ($d in (Get-ChildItem -LiteralPath $OutDir -Recurse -Directory | Sort-Object { $_.FullName.Length } -Descending)) {
    if (-not (Get-ChildItem -LiteralPath $d.FullName -Force)) { Remove-Item -LiteralPath $d.FullName -Force }
}

Write-Host ("Wrote {0}  ({1} paginas)" -f $OutDir, $pages.Count)
foreach ($p in $pages) { Write-Host "    $p" }
if ($removidos.Count -gt 0) {
    Write-Host ("Removidas {0} pagina(s) obsoleta(s)" -f $removidos.Count) -ForegroundColor Yellow
    foreach ($r in $removidos) { Write-Host "    - $r" }
}
