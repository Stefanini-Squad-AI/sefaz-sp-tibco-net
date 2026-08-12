#Requires -Version 7.0
<#
.SYNOPSIS
    Runs the full extraction pipeline (S0 -> S1 -> S2 -> S3 -> S4) for one TIBCO package.

.DESCRIPTION
    Reads a package manifest from config/packages/<name>.json and drives every
    generator with explicit paths, writing to artifacts/<package>/. This is what
    makes the toolchain multi-package: nothing is hardcoded to POC_Epat, and two
    packages can no longer overwrite each other's output.

    Stages:
      S0  Pin       - SHA-256 every source file, so drift is detectable later.
      S1  Extract   - the four generators, in dependency order.
      S2  Validate  - validate-artifacts.ps1, including source-coverage checks.
      S3  BPMN      - emit-bpmn.ps1, specification only.
      S4  DMN       - emit-dmn.ps1 plus a differential proof that it reproduces
                      Corticon's override semantics.

    Artifacts themselves carry NO timestamp. Everything volatile (when it ran,
    which inputs, what hashes) lives in the sidecar artifacts/<package>/manifest.json,
    so regenerating from unchanged sources produces byte-identical artifacts and a
    git diff means a real semantic change.

.EXAMPLE
    ./tools/run-extraction.ps1 -Package POC_Epat
    ./tools/run-extraction.ps1 -Package POC_Epat -SkipValidation
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Package,
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$SkipValidation,
    [switch]$SkipBpmn,
    [switch]$SkipDmn,
    [switch]$SkipDocs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$manifestPath = Join-Path $RepoRoot 'config' 'packages' "$Package.json"
if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Package manifest not found: $manifestPath"
}
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json

$sourceRoot = Join-Path $RepoRoot $manifest.root
$outDir     = Join-Path $RepoRoot 'artifacts' $manifest.package
$null = New-Item -ItemType Directory -Path $outDir -Force

function Resolve-Source {
    param([Parameter(Mandatory)][string]$Relative)
    $full = Join-Path $sourceRoot $Relative
    if (-not (Test-Path -LiteralPath $full)) {
        throw "Source declared in the manifest does not exist: $Relative`n  expected at: $full"
    }
    (Resolve-Path -LiteralPath $full).Path
}

Write-Host ''
Write-Host "=== Package: $($manifest.package) ===" -ForegroundColor Cyan
Write-Host "    source : $sourceRoot"
Write-Host "    output : $outDir"

# ------------------------------------------------------- S0  pin the sources --

Write-Host ''
Write-Host 'S0  Pinning sources' -ForegroundColor Cyan

$xpdlPath  = Resolve-Source $manifest.sources.xpdl
$ersPath   = Resolve-Source $manifest.sources.ers
$formsRoot = Resolve-Source $manifest.sources.forms
$wsdlPaths = @($manifest.sources.wsdl | ForEach-Object { Resolve-Source $_ })

# Optional: a package may have no external screens.
$telasRoot = $null
if ($manifest.sources.PSObject.Properties.Name -contains 'telas') {
    $telasRoot = Resolve-Source $manifest.sources.telas
}
$docPath = $null
if ($manifest.sources.PSObject.Properties.Name -contains 'doc') {
    $docPath = Resolve-Source $manifest.sources.doc
}

# Sources are read-only inputs a human may well have open (Word holds the .docx),
# and Get-FileHash refuses to share. Hash through a stream that tolerates it.
function Get-SourceHash {
    param([Parameter(Mandatory)][string]$Path)
    $stream = [IO.File]::Open($Path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
    try {
        $sha = [Security.Cryptography.SHA256]::Create()
        try { return [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '') }
        finally { $sha.Dispose() }
    }
    finally { $stream.Dispose() }
}

$pinned = [System.Collections.Generic.List[object]]::new()
foreach ($src in (@($xpdlPath, $ersPath) + $wsdlPaths + @($docPath | Where-Object { $_ }))) {
    $hash = Get-SourceHash -Path $src
    $pinned.Add([ordered]@{
            file   = ([IO.Path]::GetRelativePath($RepoRoot, $src).Replace('\', '/'))
            bytes  = (Get-Item -LiteralPath $src).Length
            sha256 = $hash.ToLowerInvariant()
        })
    Write-Host ('    {0}  {1}' -f $hash.Substring(0, 12).ToLowerInvariant(), (Split-Path $src -Leaf))
}

# ---------------------------------------------------------- S1  extraction ----

$paths = [ordered]@{
    processModel  = Join-Path $outDir 'process-model.json'
    fields        = Join-Path $outDir 'case-field-dictionary.json'
    services      = Join-Path $outDir 'service-contracts.json'
    decisions     = Join-Path $outDir 'decision-tables.json'
    screens       = Join-Path $outDir 'screen-catalogue.json'
    builtins      = Join-Path $outDir 'builtin-contract.json'
    intent        = Join-Path $outDir 'intent-map.json'
    dossier       = Join-Path $outDir 'review-dossier.json'
    conformance   = Join-Path $outDir 'conformance.json'
    scriptRules   = Join-Path $outDir 'rule-inventory.json'
    screenRules   = Join-Path $outDir 'screen-rules.json'
    ruleCatalogue = Join-Path $outDir 'rule-catalogue.json'
    scope         = Join-Path $outDir 'scope.json'
}

# Human answers live outside artifacts/ because they are authored, not generated.
$glossaryPath = Join-Path $RepoRoot "config/glossary/$Package.yaml"

Write-Host ''
Write-Host 'S1  Extracting' -ForegroundColor Cyan

$sw = [System.Diagnostics.Stopwatch]::StartNew()

# The generators deliberately rely on $null propagation for OPTIONAL XPath lookups
# (an absent <XPDLVersion> must yield '' rather than throw). This script runs under
# Set-StrictMode -Latest, which would turn every one of those into a hard error, so
# each generator is invoked in a child scope with strict mode switched off.
function Invoke-Generator {
    param(
        [Parameter(Mandatory)][string]$Script,
        [Parameter(Mandatory)][hashtable]$Arguments,
        [Parameter(Mandatory)][string]$Produces
    )
    $scriptPath = Join-Path $PSScriptRoot $Script
    & {
        Set-StrictMode -Off
        & $scriptPath @Arguments
    } | Out-Null
    Write-Host "    $Produces"
}

# Order is a hard dependency chain: 1 -> 2, 1 -> 3 -> 4.
Invoke-Generator -Script 'gen-process-model.ps1' -Produces 'process-model.json' -Arguments @{
    XpdlPath = $xpdlPath; OutPath = $paths.processModel
}
Invoke-Generator -Script 'gen-field-dictionary.ps1' -Produces 'case-field-dictionary.json' -Arguments @{
    ModelPath = $paths.processModel; FormsRoot = $formsRoot; OutPath = $paths.fields
}
Invoke-Generator -Script 'gen-service-catalogue.ps1' -Produces 'service-contracts.json' -Arguments @{
    WsdlPaths = $wsdlPaths; ModelPath = $paths.processModel; OutPath = $paths.services
}
Invoke-Generator -Script 'gen-decision-table.ps1' -Produces 'decision-tables.json' -Arguments @{
    ErsPath = $ersPath; ContractsPath = $paths.services; OutPath = $paths.decisions
}
# The two UserDefined user tasks hand off to ePAT ASP.NET pages; their work item
# contract is the only record of what those steps read and write.
if ($telasRoot) {
    Invoke-Generator -Script 'gen-screen-catalogue.ps1' -Produces 'screen-catalogue.json' -Arguments @{
        TelasRoot = $telasRoot; ModelPath = $paths.processModel
        FieldsPath = $paths.fields; OutPath = $paths.screens
    }
}
# The scriptTasks call iProcess builtins with no .NET equivalent; this records the
# exact surface plus behavioural vectors, since their semantics cannot be derived.
Invoke-Generator -Script 'gen-builtin-contract.ps1' -Produces 'builtin-contract.json' -Arguments @{
    ModelPath = $paths.processModel; OutPath = $paths.builtins
}
# The POC document is the only source that states what each stage is FOR; the dossier
# attaches that as context to the questions it raises.
if ($docPath) {
    Invoke-Generator -Script 'gen-intent-map.ps1' -Produces 'intent-map.json' -Arguments @{
        DocPath = $docPath; ModelPath = $paths.processModel; OutPath = $paths.intent
    }
}
# Reads the artifacts back and collects everything a human still has to decide.
# Also seeds config/glossary/<package>.yaml, preserving any answer already written.
Invoke-Generator -Script 'gen-review-dossier.ps1' -Produces 'review-dossier.json' -Arguments @{
    ModelPath = $paths.processModel; FieldsPath = $paths.fields
    ServicesPath = $paths.services; ScreensPath = $paths.screens
    BuiltinsPath = $paths.builtins
    OutPath = $paths.dossier; GlossaryPath = $glossaryPath
    IntentPath = $paths.intent
}
# Scores the artifacts against the concepts the PoC document requires. Needs the
# intent map, so it only runs when the document was supplied.
if ($docPath) {
    Invoke-Generator -Script 'gen-conformance.ps1' -Produces 'conformance.json' -Arguments @{
        Package = $Package; ArtifactsDir = $outDir; OutPath = $paths.conformance
    }
}
# As regras que vivem no XPDL: condicao de transicao, corpo de script, prazo por
# expressao, mapeamento de dados. Precisa da conformance para saber o que esta na
# trilha da POC.
Invoke-Generator -Script 'gen-rule-inventory.ps1' -Produces 'rule-inventory.json' -Arguments @{
    ModelPath = $paths.processModel; FieldsPath = $paths.fields
    ConformancePath = $paths.conformance; XpdlPath = $xpdlPath
    OutPath = $paths.scriptRules
}
# As regras que vivem no code-behind das telas. So corre se as telas vieram.
if ($telasRoot) {
    Invoke-Generator -Script 'gen-screen-rules.ps1' -Produces 'screen-rules.json' -Arguments @{
        ScreensPath = $paths.screens; FieldsPath = $paths.fields
        ConformancePath = $paths.conformance; TelasRoot = $telasRoot
        OutPath = $paths.screenRules
    }
}
# Corticon, XPDL e telas no mesmo eixo: sem isto, tres inventarios que ninguem
# consegue somar.
Invoke-Generator -Script 'gen-rule-catalogue.ps1' -Produces 'rule-catalogue.json' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; OutPath = $paths.ruleCatalogue
}
# A fronteira da POC aplicada aos artefactos. O escopo corta o BACKLOG, nunca o
# modelo: o que fica de fora continua extraido, so nao gera trabalho.
Invoke-Generator -Script 'gen-scope.ps1' -Produces 'scope.json' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; OutPath = $paths.scope
}
# O schema do card exige um oraculo. scenario-path era o unico dos quatro tipos que
# faltava, e e ele que julga o corpo escrito na fase 2.
Invoke-Generator -Script 'gen-scenarios.ps1' -Produces 'scenarios/' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; OutDir = (Join-Path $outDir 'scenarios')
}
# A regra da dependencia de Clean Architecture fica gravada nas ProjectReference:
# uma violacao deixa de compilar, em vez de passar despercebida numa revisao.
Invoke-Generator -Script 'gen-scaffold.ps1' -Produces 'scaffold/' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir
    GlossaryPath = $glossaryPath; OutDir = (Join-Path $outDir 'scaffold')
}
# The same open questions, rendered as a worklist a developer can act on, with the
# line in the source where each one can be seen.
Invoke-Generator -Script 'gen-questionnaire.ps1' -Produces 'questionario.md' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; XpdlPath = $xpdlPath
    OutPath = (Join-Path $outDir 'questionario.md')
}
# O mesmo material virado ao contrario: nao pede resposta, pede conferencia. Vai
# para quem mantem o ePAT no TIBCO dizer se as decisoes batem com a realidade.
Invoke-Generator -Script 'gen-validation-dossier.ps1' -Produces 'dossie-validacao.md' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; XpdlPath = $xpdlPath
    GlossaryPath = $glossaryPath; OutPath = (Join-Path $outDir 'dossie-validacao.md')
}
$sw.Stop()

# ------------------------------------------------- counts + drift tripwire ----

$model     = Get-Content -LiteralPath $paths.processModel -Raw | ConvertFrom-Json
$fieldsDoc = Get-Content -LiteralPath $paths.fields       -Raw | ConvertFrom-Json
$svcDoc    = Get-Content -LiteralPath $paths.services     -Raw | ConvertFrom-Json
$ruleDoc   = Get-Content -LiteralPath $paths.decisions    -Raw | ConvertFrom-Json

$nodeCount = 0; $edgeCount = 0; $setCount = 0
foreach ($proc in $model.processes) {
    foreach ($scope in $proc.scopes) {
        $nodeCount += @($scope.nodes).Count
        $edgeCount += @($scope.edges).Count
        if ($scope.scope -ne 'MAIN') { $setCount++ }
    }
}
$opCount = 0
foreach ($svc in $svcDoc.services) { $opCount += @($svc.operations).Count }

$actual = [ordered]@{
    processes         = @($model.processes).Count
    activities        = $nodeCount
    transitions       = $edgeCount
    activitySets      = $setCount
    caseFields        = @($fieldsDoc.fields).Count
    operations        = $opCount
    invokedOperations = @($svcDoc.invokedOperations).Count
    decisionRules     = @($ruleDoc.rules).Count
}

Write-Host ''
Write-Host 'Counts' -ForegroundColor Cyan
$drift = [System.Collections.Generic.List[string]]::new()
foreach ($key in $actual.Keys) {
    $expected = if ($manifest.expected.PSObject.Properties[$key]) { $manifest.expected.$key } else { $null }
    $got = $actual[$key]
    if ($null -eq $expected) {
        Write-Host ('    {0,-18} {1,6}' -f $key, $got) -ForegroundColor DarkGray
    }
    elseif ($expected -eq $got) {
        Write-Host ('    {0,-18} {1,6}' -f $key, $got) -ForegroundColor Green
    }
    else {
        Write-Host ('    {0,-18} {1,6}   expected {2}' -f $key, $got, $expected) -ForegroundColor Red
        $drift.Add(('{0}: expected {1}, got {2}' -f $key, $expected, $got))
    }
}

# ------------------------------------------------------- sidecar manifest ----

$dossierDoc = Get-Content -LiteralPath $paths.dossier -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Host ''
Write-Host 'Open questions' -ForegroundColor Cyan
foreach ($cat in $dossierDoc.summary.byCategory.PSObject.Properties) {
    Write-Host ('    {0,-24} {1,4}' -f $cat.Name, $cat.Value) -ForegroundColor DarkGray
}
$blockerColour = if ($dossierDoc.summary.blockers -gt 0) { 'Yellow' } else { 'Green' }
Write-Host ('    {0,-24} {1,4}   -> config/glossary/{2}.yaml' -f 'BLOCKERS', $dossierDoc.summary.blockers, $Package) -ForegroundColor $blockerColour

$sidecar = [ordered]@{
    '$schema'   = 'sefaz-sp/tibco-intermediate/run-manifest/v1'
    package     = $manifest.package
    generatedAt = (Get-Date).ToString('o')
    durationMs  = $sw.ElapsedMilliseconds
    host        = [ordered]@{
        machine    = [Environment]::MachineName
        powershell = $PSVersionTable.PSVersion.ToString()
    }
    sources     = $pinned
    counts      = $actual
    drift       = @($drift)
    artifacts   = @($paths.Keys | ForEach-Object {
            $p = $paths[$_]
            [ordered]@{
                name   = Split-Path $p -Leaf
                bytes  = (Get-Item -LiteralPath $p).Length
                sha256 = (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.ToLowerInvariant()
            }
        })
}
$sidecarPath = Join-Path $outDir 'manifest.json'
$sidecar | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $sidecarPath -Encoding UTF8
Write-Host ''
Write-Host "    manifest.json  (sources pinned, artifacts hashed)" -ForegroundColor DarkGray

# O backlog corre DEPOIS do manifesto porque cada card carrega o sha256 dele: um
# card cujo manifesto ja nao bate tem de ser regerado, nao implementado.
Invoke-Generator -Script 'gen-backlog.ps1' -Produces 'backlog/' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; OutDir = (Join-Path $outDir 'backlog')
}
# O manifesto do agente nao contem raciocinio: delimita-o. Um papel e um sitio
# onde se pode escrever, e isso ja vem calculado do mapa de camadas.
Invoke-Generator -Script 'gen-agents.ps1' -Produces 'agents/' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; OutDir = (Join-Path $outDir 'agents')
}
# O manifesto JSON diz o que cada papel pode fazer; estes sao os mesmos factos na
# forma que o Copilot no repositorio consome.
Invoke-Generator -Script 'gen-agent-files.ps1' -Produces 'github/' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; GlossaryPath = $glossaryPath
    OutDir = (Join-Path $outDir 'github')
}
# O card ja tem id estavel; isto so o veste na forma que o Jira aceita. O envio
# NAO corre aqui: escreve num sistema externo e faz-se a mao com push-jira.ps1.
Invoke-Generator -Script 'gen-jira.ps1' -Produces 'jira/' -Arguments @{
    Package = $Package; ArtifactsDir = $outDir; OutDir = (Join-Path $outDir 'jira')
}
# O selo do bundle e verificavel: mesmas fontes + mesmo glossario + mesmo parecer
# dao o mesmo selo. Sao TRES entradas, porque as tres alteram a saida.
Invoke-Generator -Script 'gen-bundle.ps1' -Produces 'bundle/' -Arguments @{
    Package = $Package; RepoRoot = $RepoRoot; ArtifactsDir = $outDir
    GlossaryPath = $glossaryPath; OutDir = (Join-Path $outDir 'bundle')
}

# ---------------------------------------------------------- S2  validation ----

$exitCode = 0

if ($drift.Count -gt 0) {
    Write-Host ''
    Write-Host 'DRIFT: source no longer matches the manifest baseline.' -ForegroundColor Red
    Write-Host 'Either the input changed, or a generator regressed. Investigate before' -ForegroundColor Red
    Write-Host 'updating config/packages/*.json - the baseline exists to catch exactly this.' -ForegroundColor Red
    $exitCode = 1
}

if (-not $SkipValidation) {
    Write-Host ''
    Write-Host 'S2  Validating' -ForegroundColor Cyan
    & (Join-Path $PSScriptRoot 'validate-artifacts.ps1') -ArtifactsDir $outDir -XpdlPath $xpdlPath
    if ($LASTEXITCODE -ne 0) { $exitCode = $LASTEXITCODE }
}

# ---------------------------------------------------------- S3  BPMN emit ----

# Specification only (isExecutable="false"). Emitted last because it consumes both
# the artifacts and any answers the analyst has already written into the glossary.
if (-not $SkipBpmn) {
    Write-Host ''
    Write-Host 'S3  Emitting BPMN' -ForegroundColor Cyan
    & {
        Set-StrictMode -Off
        & (Join-Path $PSScriptRoot 'emit-bpmn.ps1') `
            -ModelPath $paths.processModel -GlossaryPath $glossaryPath `
            -OutDir (Join-Path $outDir 'bpmn')
    }
    if ($LASTEXITCODE -ne 0) { $exitCode = $LASTEXITCODE }
}

# ----------------------------------------------------------- S4  DMN emit ----

# Specification only, like the BPMN. The equivalence check is not optional theatre:
# the emitter rewrites Corticon's override fold into per-attribute FIRST tables, and
# that rewrite is only safe while conditions and actions stay disjoint. If the
# rulesheet ever changes so they overlap, this fails the build instead of shipping
# a DMN that quietly means something else.
if (-not $SkipDmn) {
    Write-Host ''
    Write-Host 'S4  Emitting DMN' -ForegroundColor Cyan
    $dmnDir = Join-Path $outDir 'dmn'
    & {
        Set-StrictMode -Off
        & (Join-Path $PSScriptRoot 'emit-dmn.ps1') `
            -DecisionsPath $paths.decisions -GlossaryPath $glossaryPath `
            -OutDir $dmnDir -Package $manifest.package
    }
    if ($LASTEXITCODE -ne 0) { $exitCode = $LASTEXITCODE }
    else {
        # The emitter names its files after the rulesheet, so ask it rather than guess.
        $dmnIndex = Get-Content -LiteralPath (Join-Path $dmnDir 'index.json') -Raw | ConvertFrom-Json
        & {
            Set-StrictMode -Off
            & (Join-Path $PSScriptRoot 'verify-dmn-equivalence.ps1') `
                -DecisionsPath $paths.decisions `
                -DmnPath (Join-Path $dmnDir $dmnIndex.primary)
        }
        if ($LASTEXITCODE -ne 0) { $exitCode = $LASTEXITCODE }
    }
}

# ------------------------------------------------------------- S5  docs ----

# The site is a projection of the artifacts, so it is regenerated on every run and
# can never describe a state the artifacts no longer have.
if (-not $SkipDocs) {
    Write-Host ''
    Write-Host 'S5  Rendering documentation' -ForegroundColor Cyan
    & {
        Set-StrictMode -Off
        & (Join-Path $PSScriptRoot 'gen-docs.ps1') `
            -Package $manifest.package -ArtifactsDir $outDir `
            -OutDir (Join-Path $PSScriptRoot '../docs-site/docs')
    } | Select-Object -First 1 | ForEach-Object { Write-Host "    $_" }
    if ($LASTEXITCODE -ne 0) { $exitCode = $LASTEXITCODE }
}

Write-Host ''
if ($exitCode -eq 0) {
    Write-Host ("OK  $($manifest.package) extracted and validated in {0:n1}s" -f ($sw.ElapsedMilliseconds / 1000)) -ForegroundColor Green
}
else {
    Write-Host "FAILED  $($manifest.package)" -ForegroundColor Red
}
exit $exitCode
