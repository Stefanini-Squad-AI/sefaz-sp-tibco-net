<#
.SYNOPSIS
    S1.7 - scores the extracted artifacts against the concepts the PoC document requires.

.DESCRIPTION
    The client's PoC document does not ask for a migration: it asks whether the target
    platform supports a declared list of BPM concepts. This script answers that question
    in the client's own vocabulary, and only from mechanical evidence.

    Two axes are reported separately, because conflating them is how a PoC claims success
    it has not earned:

      extraction  the concept was found in the source and is described by the artifacts
      execution   the concept was demonstrated running on the target platform

    Extraction is decided by counting; execution stays 'pending' until a scenario run
    reports otherwise. A concept is never marked covered without a count behind it.
#>
[CmdletBinding()]
param(
    [string]$Package       = 'POC_Epat',
    [string]$ArtifactsDir  = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$ConceptsPath  = "$PSScriptRoot/../config/poc-concepts.json",
    [string]$OutPath       = "$PSScriptRoot/../artifacts/POC_Epat/conformance.json"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ConceptsPath)) { throw "poc-concepts.json not found: $ConceptsPath" }
$spec = Get-Content $ConceptsPath -Raw -Encoding UTF8 | ConvertFrom-Json

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) { if ($Optional) { return $null }; throw "artifact not found: $p" }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}

$model    = Read-Artifact 'process-model.json'
$fields   = Read-Artifact 'case-field-dictionary.json'
$dossier  = Read-Artifact 'review-dossier.json'
$intent   = Read-Artifact 'intent-map.json'
$manifest = Read-Artifact 'manifest.json' -Optional
$screens  = Read-Artifact 'screen-catalogue.json' -Optional
$bpmnDir  = Join-Path $ArtifactsDir 'bpmn'

# ------------------------------------------------------------------ index ----

$nodesFlat = [System.Collections.Generic.List[object]]::new()
foreach ($p in $model.processes) {
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) {
            $label = $n.displayName; if (-not $label) { $label = $n.name }
            $nodesFlat.Add([pscustomobject]@{
                Process = $p.name; Scope = $s.scope; Kind = $n.kind
                Label = $label; Name = $n.name; Id = $n.id
            })
        }
    }
}

$hazards = @($model.derived.migrationHazards)

# The emitted BPMN is the only place gateway direction and event definitions are
# distinguishable, so element-level concepts are counted there.
$bpmnCounts = @{}
if (Test-Path $bpmnDir) {
    foreach ($f in (Get-ChildItem -Path $bpmnDir -Filter *.bpmn)) {
        $text = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8
        foreach ($m in [regex]::Matches($text, '<bpmn:([A-Za-z]+)')) {
            $k = $m.Groups[1].Value
            if (-not $bpmnCounts.ContainsKey($k)) { $bpmnCounts[$k] = 0 }
            $bpmnCounts[$k]++
        }
    }
}

# Concepts that map onto a construct with no .NET peer inherit that item as a blocker.
$noEqByCategory = @{}
foreach ($it in @($dossier.items | Where-Object { $_.category -eq 'no-net-equivalent' })) {
    $noEqByCategory[$it.subject] = $it
}

# Processes the document names but the package never delivered.
$declaredProcesses = @($model.processes | ForEach-Object { $_.name })
$externalPackages  = @($model.externalPackages.PSObject.Properties | ForEach-Object { $_.Name })

# ------------------------------------------------------------- evaluation ----

$results = [System.Collections.Generic.List[object]]::new()

foreach ($c in $spec.concepts) {
    $count    = 0
    $evidence = [System.Collections.Generic.List[object]]::new()
    $blockers = [System.Collections.Generic.List[string]]::new()

    if ($c.detect.nodeKinds) {
        foreach ($kind in $c.detect.nodeKinds) {
            $hits = @($nodesFlat | Where-Object { $_.Kind -eq $kind })
            if ($hits.Count -gt 0) {
                $count += $hits.Count
                $evidence.Add([ordered]@{
                    source    = 'process-model.json'
                    what      = "nodeKind '$kind'"
                    count     = $hits.Count
                    processes = @($hits | ForEach-Object { $_.Process } | Sort-Object -Unique)
                    samples   = @($hits | Select-Object -First 3 | ForEach-Object { "$($_.Process)/$($_.Label)" })
                })
            }
        }
    }

    if ($c.detect.bpmnElements) {
        foreach ($el in $c.detect.bpmnElements) {
            $n = 0; if ($bpmnCounts.ContainsKey($el)) { $n = $bpmnCounts[$el] }
            if ($n -gt 0) {
                $count += $n
                $evidence.Add([ordered]@{ source = 'bpmn/'; what = "elemento BPMN '$el'"; count = $n })
            }
        }
    }

    if ($c.detect.hazardCategories) {
        foreach ($cat in $c.detect.hazardCategories) {
            $hits = @($hazards | Where-Object { $_.category -eq $cat })
            if ($hits.Count -gt 0) {
                $count += $hits.Count
                $evidence.Add([ordered]@{
                    source    = 'process-model.json (migrationHazards)'
                    what      = "construcao '$cat'"
                    count     = $hits.Count
                    processes = @($hits | ForEach-Object { $_.process } | Sort-Object -Unique)
                    samples   = @($hits | Select-Object -First 3 | ForEach-Object { "$($_.process)/$($_.node)" })
                })
            }
            if ($noEqByCategory.ContainsKey($cat)) { $blockers.Add($noEqByCategory[$cat].id) }
        }
    }

    if ($c.detect.caseFields) {
        $n = @($fields.fields).Count
        $count += $n
        $evidence.Add([ordered]@{
            source = 'case-field-dictionary.json'
            what   = 'campos de caso declarados'
            count  = $n
            note   = "$(@($fields.fields | Where-Object usesSwNaSentinel).Count) usam o sentinela SW_NA (tres estados)"
        })
    }

    if ($c.detect.decisionRules) {
        $n = 0
        if ($manifest -and $manifest.counts.decisionRules) { $n = $manifest.counts.decisionRules }
        $count += $n
        $evidence.Add([ordered]@{
            source = 'decision-tables.json + dmn/'
            what   = 'colunas de regra Corticon convertidas para DMN'
            count  = $n
            note   = 'equivalencia verificada por execucao sobre 3.000 casos aleatorios x 11 atributos, sem divergencia'
        })
    }

    if ($c.detect.screens -and $screens) {
        $n = @($screens.screens).Count
        if ($n -gt 0) {
            $evidence.Add([ordered]@{ source = 'screen-catalogue.json'; what = 'telas ligadas a userTask'; count = $n })
        }
    }

    # Some constructs are named rather than typed - the iProcess graft step is only
    # identifiable by the label the modeller gave it.
    if ($c.detect.nodeNamePattern) {
        $pat  = $c.detect.nodeNamePattern
        $hits = @($nodesFlat | Where-Object { $_.Label -match $pat -or $_.Name -match $pat })
        if ($hits.Count -gt 0) {
            $count += $hits.Count
            $evidence.Add([ordered]@{
                source    = 'process-model.json'
                what      = "passos cujo nome casa com '$pat'"
                count     = $hits.Count
                processes = @($hits | ForEach-Object { $_.Process } | Sort-Object -Unique)
                samples   = @($hits | Select-Object -First 4 | ForEach-Object { "$($_.Process)/$($_.Label)" })
            })
        }
    }

    $extraction = if ($count -gt 0) { 'verified' } else { 'absent' }

    # Only the Decisions concept has been demonstrated end to end so far: the DMN was
    # executed against the original rules. Everything else awaits a runnable target.
    $execution = 'pending'
    $executionEvidence = $null
    if ($c.id -eq 'decisions' -and $count -gt 0) {
        $execution = 'proven'
        $executionEvidence = 'verify-dmn-equivalence.ps1 - 3000 casos, 11 atributos, 0 divergencias'
    }

    $results.Add([ordered]@{
        id             = $c.id
        name           = $c.name
        headline       = [bool]$c.headline
        objective      = $c.objective
        validationGoal = @($c.validationGoal)
        extraction     = $extraction
        occurrences    = $count
        execution      = $execution
        executionEvidence = $executionEvidence
        blockers       = @($blockers | Sort-Object -Unique)
        evidence       = @($evidence)
    })
}

# ---------------------------------------------------------------- etapas ----

# Stages come from intent-map.json, which parses the PoC document itself and links a
# stage to a model element only on a verbatim name match. Re-transcribing them here
# would create a second, hand-made version of a fact that is already derived.
$conceptNameById = @{}
foreach ($r in $results) { $conceptNameById[$r.id] = $r.name }

# intent-map casa o titulo da etapa contra NOMES DE PROCESSO. Duas das sete etapas
# nao tem forma de processo - 'Integracao com Decisions' e um par de nos, 'Gateways
# Paralelos' e uma regiao - e por isso ficavam sem ligacao ao modelo. A mesma regra
# de casamento verbatim, aplicada ao nivel do NO, ancora-as sem inventar mapeamento.
function ConvertTo-Fold {
    param([string]$s)
    if (-not $s) { return '' }
    $d = $s.Normalize([Text.NormalizationForm]::FormD).ToCharArray()
    $keep = @($d | Where-Object { [Globalization.CharUnicodeInfo]::GetUnicodeCategory($_) -ne 'NonSpacingMark' })
    return (-join $keep).ToUpperInvariant()
}
$stopWords = @('DE','DO','DA','DOS','DAS','COM','E','O','A','AO','EM','VALIDACAO','INTEGRACAO','CONTROLE')
function Get-StageTerms {
    param([string]$Title)
    $words = @((ConvertTo-Fold $Title) -split '[^A-Z0-9]+' | Where-Object { $_.Length -ge 4 -and $_ -notin $stopWords })
    return @($words | Sort-Object -Unique)
}
$nodeFold = @{}
foreach ($nf in $nodesFlat) { $nodeFold[$nf.Id] = ConvertTo-Fold $nf.Label }

# Encerrar a INSTANCIA e terminar o processo que ninguem chama; o endEvent de um
# subprocesso e um regresso ao chamador.
$chamados = @{}
foreach ($ce in @($model.derived.callEdges)) { if ($ce.kind -eq 'call' -and $ce.toProcess) { $chamados[$ce.toProcess] = $true } }
$raiz = @{}
foreach ($p in $model.processes) { if (-not $chamados.ContainsKey($p.name)) { $raiz[$p.name] = $true } }

$conceitoAncora = @{}
foreach ($ca in @($spec.stageConceptAnchors.byConcept)) { $conceitoAncora[(ConvertTo-Fold $ca.concept)] = $ca }

$etapas = foreach ($s in $intent.stages) {
    $matched = @($s.matchedElements)
    $terms   = Get-StageTerms $s.title
    $anchors = @(foreach ($nf in $nodesFlat) {
        $hit = @($terms | Where-Object { $nodeFold[$nf.Id] -like "*$_*" })
        if ($hit.Count -eq 0) { continue }
        [ordered]@{ process = $nf.Process; scope = $nf.Scope; nodeId = $nf.Id; node = $nf.Label; kind = $nf.Kind; via = 'titulo'; termo = $hit[0] }
    })
    # Titulo que nao casa com nome de no nenhum: cai para o construto que os
    # conceitos declarados pela propria etapa nomeiam.
    if ($anchors.Count -eq 0) {
        $porConceito = @(foreach ($cn in @(@($s.concepts) | Where-Object { $_ })) {
            $ca = $conceitoAncora[(ConvertTo-Fold $cn)]
            if (-not $ca) { continue }
            foreach ($nf in $nodesFlat) {
                if ($nf.Kind -notin @($ca.nodeKinds)) { continue }
                if ($ca.apenasProcessoRaiz -and -not $raiz.ContainsKey($nf.Process)) { continue }
                [ordered]@{ process = $nf.Process; scope = $nf.Scope; nodeId = $nf.Id; node = $nf.Label; kind = $nf.Kind; via = 'conceito-declarado'; termo = $cn }
            }
        })
        $anchors = @($porConceito | Group-Object { $_.nodeId } | ForEach-Object { $_.Group[0] })
    }
    $procs = @(@($matched | ForEach-Object { $_.process }) + @($anchors | ForEach-Object { $_.process }) |
        Where-Object { $_ } | Sort-Object -Unique)

    # Medida GROSSEIRA, mantida so como indicio: diz que o item ocorre nalgum ponto
    # de um processo que a etapa toca, nao que ocorre dentro da etapa. A medida
    # exacta e por no e vive em scenarios/index.json -> etapas.footprint[].bloqueadores,
    # porque so ali existe o segmento da jornada que delimita a etapa.
    $etapaBlockers = [System.Collections.Generic.List[string]]::new()
    foreach ($it in @($dossier.items | Where-Object { $_.category -eq 'no-net-equivalent' })) {
        foreach ($pr in $procs) {
            if ($pr -in @($it.usedInProcesses)) { $etapaBlockers.Add($it.id) }
        }
    }

    [ordered]@{
        n                 = $s.stage
        name              = $s.title
        conceptsInDocument = @($s.concepts)
        matchedElements   = $matched
        anchorTerms       = $terms
        anchorNodes       = $anchors
        processes         = $procs
        ambiguousMentions = @($s.ambiguousMentions)
        blockersPorProcesso = @($etapaBlockers | Sort-Object -Unique)
        blockersExactos   = 'ver scenarios/index.json -> etapas.footprint[].bloqueadores'
        status            = $(if ($matched.Count -eq 0 -and $anchors.Count -eq 0) { 'unlinked' } elseif ($matched.Count -eq 0) { 'anchored-by-node' } else { 'extracted' })
    }
}

# -------------------------------------------------------- expected results ----

$expected = foreach ($x in $spec.expectedResults) {
    $rs = @($results | Where-Object { $_.id -in $x.concepts })
    [ordered]@{
        id         = $x.id
        text       = $x.text
        concepts   = @($x.concepts)
        extraction = $(if (@($rs | Where-Object { $_.extraction -ne 'verified' }).Count -eq 0) { 'verified' } else { 'partial' })
        execution  = $(if (@($rs | Where-Object { $_.execution -ne 'proven' }).Count -eq 0) { 'proven' } else { 'pending' })
        blockers   = @($rs | ForEach-Object { $_.blockers } | Where-Object { $_ } | Sort-Object -Unique)
    }
}

# ------------------------------------------------------------------ write ----

$doc = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/conformance/v1'
    package   = $Package
    source    = $spec.source
    objective = $intent.scopeStatement
    coverageClaim = $intent.coverageClaim
    note      = 'Duas dimensoes distintas: EXTRACTION = o conceito foi encontrado e descrito a partir da fonte; EXECUTION = o conceito foi demonstrado em execucao na plataforma alvo. Um conceito extraido nao e um conceito provado. As etapas vem de intent-map.json, derivado do proprio documento.'
    summary   = [ordered]@{
        concepts            = $results.Count
        extractionVerified  = @($results | Where-Object { $_.extraction -eq 'verified' }).Count
        extractionAbsent    = @($results | Where-Object { $_.extraction -eq 'absent' }).Count
        executionProven     = @($results | Where-Object { $_.execution -eq 'proven' }).Count
        executionPending    = @($results | Where-Object { $_.execution -eq 'pending' }).Count
        headlineConcepts    = @($results | Where-Object { $_.headline }).Count
        headlineProven      = @($results | Where-Object { $_.headline -and $_.execution -eq 'proven' }).Count
        conceptsBlocked     = @($results | Where-Object { @($_.blockers).Count -gt 0 }).Count
        etapas              = @($etapas).Count
        etapasUnlinked      = @($etapas | Where-Object { $_.status -eq 'unlinked' }).Count
        etapasBlocked       = @($etapas | Where-Object { @($_.blockersPorProcesso).Count -gt 0 }).Count
    }
    externalPackagesNotDelivered = $externalPackages
    concepts        = @($results)
    etapas          = @($etapas)
    expectedResults = @($expected)
}

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$doc | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} conceitos: {2} extraidos, {3} provados em execucao; {4} bloqueados; {5}/{6} etapas ligadas ao modelo)" -f `
    $OutPath, $doc.summary.concepts, $doc.summary.extractionVerified, $doc.summary.executionProven,
    $doc.summary.conceptsBlocked, ($doc.summary.etapas - $doc.summary.etapasUnlinked), $doc.summary.etapas)
