<#
.SYNOPSIS
    S2.3 - empacota o entregavel para a fase 2 e sela-o por sha256.

.DESCRIPTION
    O bundle e o que se entrega ao SAI APP 3.0. A sua producao e DETERMINISTICA:
    nenhum agente corre antes da entrega. Tudo o que vai dentro e projeccao da IR
    ou resposta humana ja capturada.

    A PROPRIEDADE QUE JUSTIFICA TUDO:

        mesmas fontes + mesmo glossario + mesmo parecer = mesmo bundle

    Sao TRES entradas, nao duas. O glossario e resposta humana; o parecer agentico
    e julgamento de modelo - e ambos alteram os cards. Uma entrada que altera a
    saida e uma entrada, independentemente de quem a escreveu, e tem de ser fixada
    por hash. Deixar o parecer de fora seria por um LLM no caminho do bundle sem o
    declarar, que e a unica coisa que este kit nao pode fazer.

    O carimbo tambem nao vem do relogio: vem da data de exportacao do pacote, no
    cabecalho do XPDL. Duas corridas sobre a mesma fonte dao o mesmo bundle, byte
    a byte - e isso e verificavel, nao prometido.
#>
[CmdletBinding()]
param(
    [string]$Package      = 'POC_Epat',
    [string]$RepoRoot     = "$PSScriptRoot/..",
    [string]$ArtifactsDir = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$ReviewPath   = "$PSScriptRoot/../analysis/backlog-review.json",
    [string]$OutDir       = "$PSScriptRoot/../artifacts/POC_Epat/bundle"
)

$ErrorActionPreference = 'Stop'

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }
function Get-Sha256File { param([string]$P) return (Get-FileHash -LiteralPath $P -Algorithm SHA256).Hash.ToLowerInvariant() }
function Get-Sha256Text {
    param([string]$T)
    $s = [System.Security.Cryptography.SHA256]::Create()
    try { return (($s.ComputeHash([Text.Encoding]::UTF8.GetBytes($T)) | ForEach-Object { $_.ToString('x2') }) -join '') }
    finally { $s.Dispose() }
}

$manifest = Get-Content (Join-Path $ArtifactsDir 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$model    = Get-Content (Join-Path $ArtifactsDir 'process-model.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$backlog  = Get-Content (Join-Path $ArtifactsDir 'backlog/index.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$agents   = Get-Content (Join-Path $ArtifactsDir 'agents/index.json')  -Raw -Encoding UTF8 | ConvertFrom-Json
$dossier  = Get-Content (Join-Path $ArtifactsDir 'review-dossier.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$conf     = Get-Content (Join-Path $ArtifactsDir 'conformance.json')   -Raw -Encoding UTF8 | ConvertFrom-Json

$exportado = $(if ($model.source.created) { $model.source.created } else { '1970-01-01' })
$selado = ([datetime]::ParseExact($exportado.Substring(0, 10), 'yyyy-MM-dd',
    [Globalization.CultureInfo]::InvariantCulture,
    [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal)
    ).ToString('yyyy-MM-ddTHH:mm:ssZ', [Globalization.CultureInfo]::InvariantCulture)

# ------------------------------------------------------------- as pastas -----

# Cada pasta responde a uma pergunta da fase 2. context/ e o corpus de leitura,
# oracles/ e quem julga, backlog/ e o trabalho, agents/ e a coleira, scaffold/ e
# o que ja esta feito e nao se refaz, glossary/ e o que um humano ja decidiu, e
# review/ e o que continua por decidir - que vai a vista e nao escondido.
$plano = @(
    [ordered]@{ pasta = 'context';  de = @('process-model.json','case-field-dictionary.json','service-contracts.json','decision-tables.json','screen-catalogue.json','builtin-contract.json','intent-map.json','conformance.json','rule-inventory.json','screen-rules.json','rule-catalogue.json','scope.json','bpmn','dmn')
        porque = 'A IR e os diagramas: o corpus so de leitura dos agentes.' }
    [ordered]@{ pasta = 'oracles';  de = @('scenarios','dmn')
        porque = 'As fixtures imutaveis. O agente liga o arnes; nunca escreve o valor esperado.' }
    [ordered]@{ pasta = 'backlog';  de = @('backlog')
        porque = 'O trabalho, em cards validados contra o schema.' }
    [ordered]@{ pasta = 'agents';   de = @('agents')
        porque = 'Quem pode escrever onde, e quando tem de parar.' }
    [ordered]@{ pasta = 'github';   de = @('github')
        porque = 'Os mesmos papeis na forma que o Copilot consome: .github/agents, .github/instructions, .github/skills e AGENTS.md. Copiar para a raiz do repositorio da fase 2.' }
    [ordered]@{ pasta = 'jira';     de = @('jira')
        porque = 'A carga para o board. A chave de idempotencia viaja na label card:<id>; o envio faz-se a mao com push-jira.ps1, que nao corre no pipeline por escrever num sistema externo.' }
    [ordered]@{ pasta = 'scaffold'; de = @('scaffold')
        porque = 'So o lossless: entidades, portas, esqueletos. Sem corpos.' }
    [ordered]@{ pasta = 'review';   de = @('dossie-validacao.md','questionario.md','review-dossier.json')
        porque = 'O que continua por decidir, incluindo os bloqueadores. Vai a vista.' }
)

if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutDir -Force | Out-Null

$conteudo = [System.Collections.Generic.List[object]]::new()
foreach ($p in $plano) {
    $destino = Join-Path $OutDir $p.pasta
    New-Item -ItemType Directory -Path $destino -Force | Out-Null
    foreach ($nome in $p.de) {
        $origem = Join-Path $ArtifactsDir $nome
        if (-not (Test-Path $origem)) { continue }
        if (Test-Path $origem -PathType Container) {
            # Quando a pasta de origem tem o mesmo nome do destino, copia-se o
            # CONTEUDO - senao dava bundle/agents/agents/. Quando e uma de varias
            # dentro do destino (bpmn, dmn, scenarios), mantem-se o nome.
            if ($nome -eq $p.pasta) { Copy-Item (Join-Path $origem '*') -Destination $destino -Recurse -Force }
            else { Copy-Item $origem -Destination (Join-Path $destino $nome) -Recurse -Force }
        } else {
            Copy-Item $origem -Destination $destino -Force
        }
    }
}
$destGloss = Join-Path $OutDir 'glossary'
New-Item -ItemType Directory -Path $destGloss -Force | Out-Null
Copy-Item $GlossaryPath -Destination $destGloss -Force

# --------------------------------------------------------------- o selo ------

foreach ($f in (Get-ChildItem $OutDir -Recurse -File | Sort-Object FullName)) {
    $rel = $f.FullName.Substring($OutDir.Length).TrimStart('\', '/') -replace '\\', '/'
    $conteudo.Add([ordered]@{ ficheiro = $rel; bytes = $f.Length; sha256 = (Get-Sha256File $f.FullName) })
}
$selo = Get-Sha256Text ((@($conteudo | ForEach-Object { "$($_.ficheiro)|$($_.sha256)" })) -join "`n")

# AS TRES ENTRADAS. Uma entrada e o que altera a saida - venha de quem vier.
$entradas = @()
foreach ($s in (Arr $manifest.sources)) {
    $entradas += [ordered]@{ tipo = 'fonte'; ficheiro = $s.file; sha256 = $s.sha256
        porque = 'Artefacto TIBCO exportado. Se mudar, tudo tem de ser reextraido.' }
}
$entradas += [ordered]@{ tipo = 'glossario'; ficheiro = 'config/glossary/POC_Epat.yaml'; sha256 = (Get-Sha256File $GlossaryPath)
    porque = 'Respostas humanas ratificadas. Alteram o conteudo dos cards, logo sao entrada.' }
if (Test-Path $ReviewPath) {
    $entradas += [ordered]@{ tipo = 'parecer-agentico'; ficheiro = 'analysis/backlog-review.json'; sha256 = (Get-Sha256File $ReviewPath)
        porque = 'Julgamento de modelo. Pode alterar titulo e hipoteses dos cards, logo E entrada e tem de ser fixado como as outras - senao havia um LLM no caminho do bundle sem estar declarado.' }
}

# ------------------------------------------------------- o que fica por fechar

$porFechar = @()
foreach ($it in @($dossier.items | Where-Object { -not $_.resolution.answered })) {
    $porFechar += [ordered]@{ id = $it.id; assunto = $it.subject; prioridade = $it.priority
        porque = 'Pergunta do dossie ainda sem resposta humana.' }
}
foreach ($e in (Arr $backlog.resultadosSemCard)) {
    $porFechar += [ordered]@{ id = $e.id; assunto = $e.texto; prioridade = 'P1'
        porque = 'Resultado esperado do plano sem card que o prove.' }
}
foreach ($c in (Arr $backlog.conceitosSemCard)) {
    $porFechar += [ordered]@{ id = $c.id; assunto = $c.nome; prioridade = 'P1'
        porque = 'Conceito do documento sem card que o prove.' }
}
foreach ($c in (Arr $agents.cardsSemDono)) {
    $porFechar += [ordered]@{ id = $c.id; assunto = 'card sem papel atribuido'; prioridade = 'P2'
        porque = 'Nenhum papel do elenco escreve onde este card manda escrever.' }
}

$doc = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/bundle-manifest/v1'
    package = $Package
    nota = 'Entregavel para a fase 2. Producao DETERMINISTICA: nenhum agente corre antes da entrega. A propriedade e verificavel, nao prometida - mesmas fontes + mesmo glossario + mesmo parecer dao este mesmo selo. O carimbo vem da data de exportacao do pacote e nao do relogio da maquina, senao duas corridas nunca coincidiriam.'
    seladoEm = $selado
    seloDoConteudo = $selo
    entradasFixadas = @($entradas)
    pastas = @($plano | ForEach-Object { [ordered]@{ pasta = $_.pasta; porque = $_.porque } })
    summary = [ordered]@{
        ficheiros = $conteudo.Count
        bytes = (@($conteudo | ForEach-Object { [long]$_.bytes } | Measure-Object -Sum).Sum)
        cards = $backlog.summary.total
        papeis = $agents.summary.papeis
        nosComCard = "$($backlog.summary.nosComCard)/$($backlog.summary.nosEmEscopo)"
        conceitosComCard = $backlog.summary.conceitosTocados
        etapasLigadas = "$(@($conf.etapas | Where-Object { $_.status -ne 'unlinked' }).Count)/$(@($conf.etapas).Count)"
        decisoesRespondidas = "$($dossier.summary.answered)/$($dossier.summary.total)"
        porFechar = $porFechar.Count
    }
    porFechar = @($porFechar)
    conteudo = @($conteudo)
}
$doc | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $OutDir 'manifest.json') -Encoding UTF8

Write-Host ("Wrote {0}  ({1} ficheiros, {2:n0} KB; selo {3}; {4} entrada(s) fixada(s); {5} ponto(s) por fechar)" -f `
    $OutDir, $doc.summary.ficheiros, ($doc.summary.bytes / 1KB), $selo.Substring(0, 12),
    @($entradas).Count, $doc.summary.porFechar)
if ($porFechar.Count -gt 0) {
    Write-Host ("    o bundle vai com {0} ponto(s) por fechar - ver manifest.json > porFechar" -f $porFechar.Count) -ForegroundColor Yellow
}
