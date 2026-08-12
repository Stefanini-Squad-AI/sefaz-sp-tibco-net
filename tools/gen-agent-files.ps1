<#
.SYNOPSIS
    S2.4 - projecta os manifestos de papel em ficheiros de customizacao do GitHub.

.DESCRIPTION
    O manifesto JSON diz o que cada papel pode fazer; isto traduz-o para o formato
    que o Copilot no repositorio consome - .agent.md, .instructions.md, SKILL.md e
    AGENTS.md. Continua a nao haver raciocinio aqui: e a mesma informacao noutra
    forma, e a permissao continua a sair do mapa de camadas.

    A REPARTICAO SEGUE O QUE CADA PRIMITIVA E:
      agents/*.agent.md         - o papel: persona, ferramentas minimas, fronteiras
      instructions/*.instructions.md - a regra que se aplica a ficheiros, por applyTo
      skills/<nome>/SKILL.md    - o conhecimento que varios papeis precisam
      copilot-instructions.md   - o que vale sempre, para toda a gente
      AGENTS.md                 - a porta de entrada do repositorio

    O que NAO vai para aqui: o oraculo. Ele fica nos ficheiros de fixture, imutavel,
    e o agente liga-se a ele - nunca o transcreve para dentro de uma instrucao, senao
    passava a poder edita-lo.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$MapPath      = "$PSScriptRoot/../config/dotnet-architecture.json",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$CatalogPath  = "$PSScriptRoot/../config/net-equivalence-catalog.json",
    [string]$OutDir       = "$PSScriptRoot/../artifacts/POC_Epat/github",
    # A pasta muda com o repositorio de destino; os ficheiros e o conteudo nao.
    [string]$Base         = '.github'
)

$ErrorActionPreference = 'Stop'

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }
function Esc { param([string]$T) return (($T -replace '"', "'") -replace '\r?\n', ' ') }

$agents  = Get-Content (Join-Path $ArtifactsDir 'agents/index.json')  -Raw -Encoding UTF8 | ConvertFrom-Json
$backlog = Get-Content (Join-Path $ArtifactsDir 'backlog/index.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$map     = Get-Content $MapPath -Raw -Encoding UTF8 | ConvertFrom-Json
$catalog = Get-Content $CatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$papeis  = @(Get-ChildItem (Join-Path $ArtifactsDir 'agents') -Filter '*.json' -File |
    Where-Object { $_.Name -ne 'index.json' } |
    ForEach-Object { Get-Content $_.FullName -Raw -Encoding UTF8 | ConvertFrom-Json })

# As decisoes ratificadas dos construtos sem equivalente .NET viram SKILL: e o
# conhecimento que sete papeis precisam e nenhum deles deve redescobrir.
$gapDecidido = @{}
if (Test-Path $GlossaryPath) {
    $dentro = $false; $chave = $null
    foreach ($l in (Get-Content $GlossaryPath -Encoding UTF8)) {
        if ($l -match '^gaps:') { $dentro = $true; continue }
        if ($dentro -and $l -match '^[a-z]') { break }
        if (-not $dentro) { continue }
        if ($l -match '^\s{2}([a-z-]+):\s*$') { $chave = $Matches[1]; $gapDecidido[$chave] = [ordered]@{ opcao = ''; justificativa = '' }; continue }
        if ($chave -and $l -match '^\s+opcaoEscolhida:\s*"(.*)"\s*$') { $gapDecidido[$chave].opcao = $Matches[1] }
        if ($chave -and $l -match '^\s+justificativa:\s*"(.*)"\s*$') { $gapDecidido[$chave].justificativa = $Matches[1] }
    }
}

# Ferramentas minimas por tipo de papel. Excesso de ferramentas dilui o foco, e
# um revisor com permissao de escrita deixa de ser revisor.
$ferramentas = @{
    'implementador'   = @('read', 'search', 'edit', 'execute', 'todo')
    'fundacao'        = @('read', 'search', 'edit', 'execute', 'todo')
    'coordenacao'     = @('read', 'search', 'todo')
    'autor-de-testes' = @('read', 'search', 'edit', 'execute')
    'revisor'         = @('read', 'search')
}

if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
$Base = $Base.Trim('/', '\')
foreach ($d in @("$Base/agents", "$Base/instructions", "$Base/skills")) {
    New-Item -ItemType Directory -Path (Join-Path $OutDir $d) -Force | Out-Null
}

$emitidos = [System.Collections.Generic.List[object]]::new()
function Write-Doc {
    param([string]$Rel, [string[]]$Linhas)
    $p = Join-Path $OutDir $Rel
    $dir = Split-Path -Parent $p
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    ($Linhas -join "`n") | Set-Content -LiteralPath $p -Encoding UTF8
    $emitidos.Add([ordered]@{ ficheiro = $Rel; linhas = $Linhas.Count })
}

# ------------------------------------------------------------ agents ---------

foreach ($p in $papeis) {
    $tools = $ferramentas[$p.tipo]
    if (-not $tools) { $tools = @('read', 'search') }
    $seus = @(Arr $p.cards)
    $gatilhos = @(Arr $p.escreveEm | ForEach-Object { ($_ -replace '/\*\*$', '') -replace '^src/|^tests/', '' })

    $desc = "Use when implementing cards assigned to $($p.papel)"
    if ($gatilhos.Count -gt 0) { $desc += ", or when the work touches $($gatilhos -join ', ')" }
    $desc += ". Migration of the SEFAZ-SP ePAT process from TIBCO iProcess to .NET."

    $body = @(
        '---'
        "description: `"$(Esc $desc)`""
        "name: `"$(Esc $p.papel)`""
        "tools: [$($tools -join ', ')]"
        'user-invocable: true'
        '---'
        ''
        "You implement cards of the ePAT migration backlog assigned to the role **$($p.papel)**."
        ''
        $p.porqueExiste
        ''
        '## What you may write'
        ''
    )
    if (@(Arr $p.escreveEm).Count -gt 0) {
        foreach ($e in (Arr $p.escreveEm)) { $body += "- ``$e``" }
    } else {
        $body += '- Nothing. This role coordinates and reviews; it does not write code.'
    }
    $body += @(
        ''
        '## Constraints'
        ''
        '- DO NOT write outside the paths listed above. The Clean Architecture dependency rule is recorded in the `.csproj` ProjectReference: a violation stops compiling, it does not merely fail review.'
        '- DO NOT edit any oracle fixture. You wire the harness to the fixture and never author or edit an expected value - that would make the test mark its own homework.'
        '- DO NOT modify files whose card marks them `final`: those are transcription of the WSDL or the XPDL, and rewriting them breaks the contract.'
        '- DO NOT rename identifiers. `EXISTENOTIFICAC` stays `EXISTENOTIFICAC`; the business term goes in an XML comment. The toolkit transcribes, it does not baptise.'
        '- DO NOT resolve an unresolved gap on your own. You may propose; deciding is the human gate.'
    )
    if (@(Arr $p.gapsPorResolver).Count -gt 0) {
        $body += "- This role is blocked by: $((Arr $p.gapsPorResolver) -join ', '). Do not start until it is settled."
    }
    $body += @(
        ''
        '## Approach'
        ''
        '1. Read the card in `backlog/`. Everything you need is in it: you cannot see the TIBCO artifacts and you are not expected to.'
        '2. Work through `content.checklist` in order. A step whose `entrouPor` is not `fluxo` does NOT exist as a transition in the source and must be written explicitly.'
        '3. Treat `content.injectedContext.hypotheses` as questions to confirm, never as established fact.'
        '4. Run the oracle named in `acceptance.oracle`. It is the authority on correctness - not the card text, and not you.'
        ''
        '## Output'
        ''
        'Code in the paths above, plus a short note of any hypothesis you could not confirm and any gap you had to escalate.'
    )
    if (@($p.skills).Count -gt 0) {
        $body += ''
        $body += "## Knowledge this role needs"
        $body += ''
        foreach ($s in (Arr $p.skills)) { $body += "- See skill ``$s``" }
    }
    if ($p.parecer -and $p.parecer.aviso) {
        $body += @('', '## Advisory (agentic review, not fact)', '', "> $($p.parecer.aviso)")
    }
    Write-Doc "$Base/agents/$($p.id).agent.md" $body
}

# ------------------------------------------------------ instructions ---------

foreach ($l in $map.layers) {
    $body = @(
        '---'
        "description: `"Use when writing or changing code in the $($l.name) layer of the ePAT migration. Covers what belongs there and what it may depend on.`""
        "applyTo: `"src/$($l.project)/**`""
        '---'
        "# $($l.name) - $($l.project)"
        ''
        $l.regra
        ''
        '## What lives here'
        ''
    )
    foreach ($f in $l.contains) { $body += "- **$($f.folder)** - $($f.what)" }
    $body += @('', '## Dependency rule', '')
    if (@($l.dependsOn).Count -eq 0) { $body += '- Depends on nothing. No project reference, no infrastructure package.' }
    else { $body += "- May reference only: $(@($l.dependsOn) -join ', ')." }
    $body += @(
        '- Each rule above is a ProjectReference. Breaking it does not compile.'
        ''
        '## Naming'
        ''
        "- $($map.namingRule)"
    )
    Write-Doc "$Base/instructions/layer-$($l.name.ToLowerInvariant()).instructions.md" $body
}

Write-Doc "$Base/instructions/oracles.instructions.md" @(
    '---'
    'description: "Use when writing or changing any test in the ePAT migration. Covers which values may be authored and which are fixed by the toolkit."'
    'applyTo: "tests/**"'
    '---'
    '# Oracles are immutable'
    ''
    'Every card names an oracle in `acceptance.oracle`, with `immutable: true`. The expected values come from the toolkit and are derived from the TIBCO source.'
    ''
    '- Wire the harness to the fixture. Never author or edit an expected value.'
    '- If a test only passes when you change the expected value, stop and escalate: either the implementation is wrong, or the fixture does not cover the case the card describes - and the second is a defect of the toolkit, not of the test.'
    '- `scenario-path` fixtures live in `oracles/scenarios/`, `decision-table` in `oracles/dmn/`, `contract` in `context/service-contracts.json`.'
)

# ------------------------------------------------------------- skills --------

foreach ($prop in $catalog.categories.PSObject.Properties) {
    $nome = $prop.Name
    $spec = $prop.Value
    $dec = $gapDecidido[$nome]
    if (-not $dec -or -not $dec.opcao) { continue }
    $body = @(
        '---'
        "name: $nome"
        "description: `"Use when implementing any ePAT card that carries the $nome blocker: $(Esc $spec.construct). Explains the ratified .NET approach and why the alternatives were refused.`""
        '---'
        "# $nome"
        ''
        '## The construct'
        ''
        $spec.construct
        ''
        '## Why .NET has no direct equivalent'
        ''
        $spec.whyNoEquivalent
        ''
        '## What was decided'
        ''
        "**$($dec.opcao)**"
        ''
        $dec.justificativa
        ''
        '## Risk if ignored'
        ''
        $spec.riskIfIgnored
        ''
        '## Alternatives that were refused'
        ''
    )
    foreach ($o in (Arr $spec.options)) {
        if ($o.id -eq $dec.opcao) { continue }
        $body += "- **$($o.id)** - $($o.approach)"
        $body += "  - Consequence: $($o.consequence)"
    }
    Write-Doc "$Base/skills/$nome/SKILL.md" $body
}

# ------------------------------------------- always-on + porta de entrada ----

$sempre = @(
    '# ePAT migration - always-on rules'
    ''
    "This repository implements the SEFAZ-SP ePAT process, migrated from TIBCO iProcess to .NET. The backlog, the oracles and the architecture were produced deterministically from the TIBCO export, pinned by sha256 in `manifest.json`."
    ''
    '## The three rules that never bend'
    ''
    '1. **The oracle decides.** Not the card text, not the reviewer, not you. Expected values are toolkit-owned and immutable.'
    '2. **Identifiers are transcribed, never renamed.** The business term belongs in an XML comment. A renamed field is a defect that compiles.'
    '3. **A gap is escalated, never resolved in code.** You may propose an option; the decision is human and is recorded in the glossary.'
    ''
    '## How to read a card'
    ''
    '- `content.checklist` is the work, in order. `entrouPor` other than `fluxo` means the link does NOT exist in the source and must be written explicitly - it is the easiest omission to make.'
    '- `fulfills.segmento` gives the reference journey and the exact steps: that is what the oracle replays.'
    '- `dimensao.peso = atravessa-o-sistema` means the card touches three or more projects. It is a valid card, but it is not one person''s work.'
    '- `content.injectedContext.hypotheses` are questions to confirm, never facts.'
    ''
    '## Scope'
    ''
    "This is a proof of concept, not the full migration. $($backlog.summary.nosComCard) of $($backlog.summary.nosEmEscopo) in-scope nodes have a card; what was left out is recorded with a reason in `context/scope.json`. If something looks missing, check there before assuming it was forgotten."
)
Write-Doc "$Base/copilot-instructions.md" $sempre

$porta = @(
    '# AGENTS.md'
    ''
    "Migration of the SEFAZ-SP ePAT process from TIBCO iProcess to .NET, phase 2."
    ''
    '## What is here'
    ''
    '| Folder | What it is |'
    '|---|---|'
    '| `context/` | The intermediate representation and the diagrams. Read-only reference corpus. |'
    '| `oracles/` | Immutable fixtures. Wire to them; never edit them. |'
    '| `backlog/` | The work, as cards validated against a schema. |'
    '| `agents/` | Role manifests: who may write where, and when to stop. |'
    '| `scaffold/` | Lossless .NET only - entities, ports, skeletons. No bodies. |'
    '| `glossary/` | Human decisions already ratified. |'
    '| `review/` | What is still undecided, including blockers. |'
    ''
    '## The roles'
    ''
    '| Agent | Writes in | Cards |'
    '|---|---|---:|'
)
foreach ($p in ($papeis | Sort-Object { $_.tipo }, { $_.id })) {
    $onde = $(if (@(Arr $p.escreveEm).Count -gt 0) { '`' + ((Arr $p.escreveEm) -join '`, `') + '`' } else { '-' })
    $porta += "| ``$($p.id)`` | $onde | $(@(Arr $p.cards).Count) |"
}
$porta += @(
    ''
    '## Order of attack'
    ''
    'Read from risk, not from topology. It says where to start so that a mistake costs little.'
    ''
    '| Step | Role | New cards | Unblocks |'
    '|---:|---|---:|---:|'
)
foreach ($s in (Arr $agents.ordemDeAtaque.sequencia)) {
    $porta += "| $($s.passo) | ``$($s.quem)`` | $($s.cardsNestaVolta) | $($s.cardsQueDesbloqueia) |"
}
$porta += @(
    ''
    '## Before you start'
    ''
    'Re-validate `provenance.manifestSha256` on the card against `manifest.json`. A mismatch means the card must be regenerated, not implemented.'
)
Write-Doc 'AGENTS.md' $porta

# ------------------------------------------------------------------ index ----

$idx = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/github-customization/v1'
    package = $Package
    nota = 'Ficheiros de customizacao do Copilot, projectados dos manifestos de papel. Copiar para a raiz do repositorio da fase 2: .github/ e AGENTS.md. Nao ha raciocinio nesta traducao - o que cada papel pode escrever continua a sair do mapa de camadas, e o oraculo continua fora de qualquer instrucao para nao poder ser editado.'
    generatedAt = $agents.generatedAt
    manifestSha256 = $agents.manifestSha256
    summary = [ordered]@{
        agentes = @($emitidos | Where-Object { $_.ficheiro -like '*.agent.md' }).Count
        instrucoes = @($emitidos | Where-Object { $_.ficheiro -like '*.instructions.md' }).Count
        skills = @($emitidos | Where-Object { $_.ficheiro -like '*SKILL.md' }).Count
        ficheiros = $emitidos.Count
    }
    ficheiros = @($emitidos)
}
$idx | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $OutDir 'index.json') -Encoding UTF8

Write-Host ("Wrote {0}  ({1} agentes, {2} instrucoes, {3} skills; {4} ficheiro(s) prontos a copiar para a raiz do repositorio)" -f `
    $OutDir, $idx.summary.agentes, $idx.summary.instrucoes, $idx.summary.skills, $idx.summary.ficheiros)
