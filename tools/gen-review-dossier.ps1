<#
.SYNOPSIS
    S3 - builds the human review dossier and seeds the glossary.

.DESCRIPTION
    Every open question this migration cannot answer mechanically is collected here,
    each one carrying the graph evidence an analyst needs in order to decide:
    what ran immediately before the decision, which alternative branches compete,
    and where each branch leads afterwards.

    Four categories are raised:
      unresolved-identifier  an identifier steers a branch but is declared nowhere
      clone-divergence       structurally identical processes whose logic differs
      unlabeled-decision     a gateway with no name at all
      sentinel-branch        a branch that tests the three-valued SW_NA sentinel

    The dossier is regenerable and carries no authored content beyond two declared
    maps ($EnvelopeAlias, $EngineValues) covering the only two facts no artifact
    states: the iProcess 15-character name truncation, and the engine-supplied
    values. Everything else about the envelope is read from the WSDL. Human answers
    go into config/glossary.yaml, which this script SEEDS but never overwrites: any
    non-empty value already present there is preserved verbatim.
#>
[CmdletBinding()]
param(
    [string]$ModelPath    = "$PSScriptRoot/../artifacts/POC_Epat/process-model.json",
    [string]$FieldsPath   = "$PSScriptRoot/../artifacts/POC_Epat/case-field-dictionary.json",
    [string]$ServicesPath = "$PSScriptRoot/../artifacts/POC_Epat/service-contracts.json",
    [string]$ScreensPath  = "$PSScriptRoot/../artifacts/POC_Epat/screen-catalogue.json",
    [string]$BuiltinsPath = "$PSScriptRoot/../artifacts/POC_Epat/builtin-contract.json",
    [string]$CatalogPath  = "$PSScriptRoot/../config/net-equivalence-catalog.json",
    [string]$OutPath      = "$PSScriptRoot/../artifacts/POC_Epat/review-dossier.json",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$IntentPath   = "$PSScriptRoot/../artifacts/POC_Epat/intent-map.json",
    [string]$AnalysisPath = "$PSScriptRoot/../config/analysis-notes.json",
    [int]   $TraceDepth   = 6
)

$ErrorActionPreference = 'Stop'

# Identifiers that are language surface, not data.
$Builtins = @(
    'true', 'false', 'null', 'undefined', 'var', 'if', 'else', 'return',
    'IPESystemValues', 'IPEStringUtil', 'IPEDateTimeUtil', 'IPEMathUtil', 'Math'
)

# Finding F1: DataMapping targets that belong to the technical envelope, not the
# 209-field domain model. The envelope itself is DERIVED from the WSDL below; the
# only authored parts are the two things no artifact states.

# The iProcess truncates identifiers to 15 characters, so the XPDL name and the
# WSDL element name diverge. Nothing in either artifact records the pairing.
$EnvelopeAlias = @{
    'STERRORCODE' = 'ERROR_CODE'
    'STERRORDESC' = 'ERROR_DESCRIPTION'
    'DUMP'        = 'DUMP_ANALYSIS'
}

# Engine values: supplied by the iProcess motor, present in no WSDL.
$EngineValues = @{
    'SW_MAINCASE'   = 'IPESystemValues - identificador do caso principal, fornecido pelo motor'
    'SW_MAINPROC'   = 'IPESystemValues - identificador do processo principal, fornecido pelo motor'
    'SW_PARENTCASE' = 'IPESystemValues - caso pai; chave de correlacao usada pelo graft step'
}

# ------------------------------------------------------------------- load ----

if (-not (Test-Path $ModelPath))  { throw "process-model.json not found: $ModelPath" }
if (-not (Test-Path $FieldsPath)) { throw "case-field-dictionary.json not found: $FieldsPath" }

$model  = Get-Content $ModelPath  -Raw -Encoding UTF8 | ConvertFrom-Json
$fields = Get-Content $FieldsPath -Raw -Encoding UTF8 | ConvertFrom-Json

# The envelope is read out of the WSDL rather than authored here, so a change in
# the contract cannot leave a stale copy behind in this script.
$screens = $null
if ($ScreensPath -and (Test-Path $ScreensPath)) {
    $screens = Get-Content $ScreensPath -Raw -Encoding UTF8 | ConvertFrom-Json
}
# Not $builtins: that collides case-insensitively with the $Builtins keyword filter.
$builtinContract = $null
if ($BuiltinsPath -and (Test-Path $BuiltinsPath)) {
    $builtinContract = Get-Content $BuiltinsPath -Raw -Encoding UTF8 | ConvertFrom-Json
}
$Envelope       = @{}
$EnvelopeOrigin = @{}
if (Test-Path $ServicesPath) {
    $services = Get-Content $ServicesPath -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($svc in @($services.services)) {
        if (-not $svc.technicalEnvelope) { continue }
        foreach ($block in $svc.technicalEnvelope.PSObject.Properties) {
            foreach ($el in @($block.Value)) {
                if (-not $el.name) { continue }
                $Envelope[$el.name]       = "$($block.Name)/$($el.name) - elemento do envelope tecnico declarado em $($svc.file)"
                $EnvelopeOrigin[$el.name] = 'wsdl-derived'
            }
        }
    }
    # XPDL uses the truncated name; point it at the element it actually carries.
    foreach ($short in $EnvelopeAlias.Keys) {
        $full = $EnvelopeAlias[$short]
        if ($Envelope.ContainsKey($full)) {
            $Envelope[$short]       = "$($Envelope[$full]) (nome truncado pelo iProcess: '$short' = '$full')"
            $EnvelopeOrigin[$short] = 'wsdl-derived-alias'
        }
    }
}
foreach ($k in $EngineValues.Keys) {
    $Envelope[$k]       = $EngineValues[$k]
    $EnvelopeOrigin[$k] = 'engine-value'
}

$fieldByName = @{}
foreach ($f in $fields.fields) { $fieldByName[$f.name] = $f }

# ------------------------------------------------------------------ index ----

# One flat index keyed by node id, plus adjacency restricted to the owning scope.
$nodeById = @{}
$outOf    = @{}   # nodeId -> edge[]
$intoOf   = @{}   # nodeId -> edge[]
$scopes   = [System.Collections.Generic.List[object]]::new()

foreach ($proc in $model.processes) {
    foreach ($scope in $proc.scopes) {
        $scopes.Add([pscustomobject]@{
            Process = $proc.name
            Scope   = $scope.scope
            Nodes   = $scope.nodes
            Edges   = $scope.edges
        })
        foreach ($n in $scope.nodes) {
            $nodeById[$n.id] = [pscustomobject]@{
                Node = $n; Process = $proc.name; Scope = $scope.scope
            }
        }
        foreach ($e in $scope.edges) {
            if (-not $outOf.ContainsKey($e.from))  { $outOf[$e.from]  = [System.Collections.Generic.List[object]]::new() }
            if (-not $intoOf.ContainsKey($e.to))   { $intoOf[$e.to]   = [System.Collections.Generic.List[object]]::new() }
            $outOf[$e.from].Add($e)
            $intoOf[$e.to].Add($e)
        }
    }
}

# ---------------------------------------------------------------- helpers ----

function Get-NodeLabel {
    param($NodeEntry)
    if ($null -eq $NodeEntry) { return '(desconhecido)' }
    $n = $NodeEntry.Node
    $label = $n.displayName
    if ([string]::IsNullOrWhiteSpace($label)) { $label = $n.name }
    if ([string]::IsNullOrWhiteSpace($label)) { return "$($n.kind) «sem rotulo»" }
    return "$($n.kind) '$label'"
}

function Get-ShortId {
    param([string]$Id)
    if ([string]::IsNullOrEmpty($Id)) { return '' }
    if ($Id.Length -le 10) { return $Id }
    return $Id.Substring(0, 10)
}

# Walks backwards until it reaches a node that carries a real label, so the analyst
# is told what actually happened before the decision rather than "gateway -> gateway".
function Get-ArrivesFrom {
    param([string]$NodeId, [int]$MaxHops = 4)

    $result  = [System.Collections.Generic.List[string]]::new()
    $visited = @{ $NodeId = $true }
    $frontier = @($NodeId)

    for ($hop = 0; $hop -lt $MaxHops -and $frontier.Count -gt 0; $hop++) {
        $next = [System.Collections.Generic.List[string]]::new()
        foreach ($id in $frontier) {
            if (-not $intoOf.ContainsKey($id)) { continue }
            foreach ($e in $intoOf[$id]) {
                if ($visited.ContainsKey($e.from)) { continue }
                $visited[$e.from] = $true
                $src = $nodeById[$e.from]
                if ($null -eq $src) { continue }
                $hasLabel = -not ([string]::IsNullOrWhiteSpace($src.Node.displayName) -and
                                  [string]::IsNullOrWhiteSpace($src.Node.name))
                if ($hasLabel) { $result.Add((Get-NodeLabel $src)) }
                else           { $next.Add($e.from) }
            }
        }
        $frontier = $next
    }
    return @($result | Sort-Object -Unique)
}

# Follows one branch forward and renders the remaining path as a readable trace.
# Stops at an end event, a re-visit, a fan-out, or the depth cap.
function Get-ForwardTrace {
    param([string]$FromNodeId, [int]$Depth)

    $trace   = [System.Collections.Generic.List[string]]::new()
    $visited = @{}
    $cur     = $FromNodeId

    for ($i = 0; $i -lt $Depth; $i++) {
        if ([string]::IsNullOrEmpty($cur) -or $visited.ContainsKey($cur)) {
            $trace.Add('... (retorna a um ponto ja percorrido)'); break
        }
        $visited[$cur] = $true
        $entry = $nodeById[$cur]
        if ($null -eq $entry) { $trace.Add('... (fora do escopo)'); break }

        $desc = Get-NodeLabel $entry
        if ($entry.Node.kind -eq 'callActivity' -and $entry.Node.call) {
            $desc += " -> chama '$($entry.Node.call.targetName)'"
        }
        $trace.Add($desc)

        if ($entry.Node.kind -eq 'endEvent') { break }
        if (-not $outOf.ContainsKey($cur))   { break }

        $outs = @($outOf[$cur])
        if ($outs.Count -gt 1) {
            $trace.Add("... (ramifica em $($outs.Count) caminhos)"); break
        }
        $cur = $outs[0].to
    }
    return @($trace)
}

# Strips literals and system values, then returns the bare identifiers.
function Get-ConditionIdentifiers {
    param([string]$Expression)
    if ([string]::IsNullOrWhiteSpace($Expression)) { return @() }

    $clean = $Expression
    $clean = [regex]::Replace($clean, "'[^']*'", ' ')
    $clean = [regex]::Replace($clean, '"[^"]*"', ' ')
    $clean = [regex]::Replace($clean, '\b(IPESystemValues|IPEStringUtil|IPEDateTimeUtil|IPEMathUtil|Math)\.\w+', ' ')

    $found = [System.Collections.Generic.List[string]]::new()
    foreach ($m in [regex]::Matches($clean, '[A-Za-z_][A-Za-z0-9_]*')) {
        $tok = $m.Value
        if ($tok -in $Builtins) { continue }
        if ($tok -match '^\d') { continue }
        $found.Add($tok)
    }
    return @($found | Sort-Object -Unique)
}

# "OUTCOME=='OK'" -> "== 'OK'". Gives the analyst the value domain without guessing.
function Get-ComparedValues {
    param([string]$Identifier, [string]$Expression)
    $pattern = "\b$([regex]::Escape($Identifier))\b\s*(==|!=|<=|>=|<|>)\s*('[^']*'|""[^""]*""|[A-Za-z_][A-Za-z0-9_.]*|-?\d+(?:\.\d+)?)"
    $vals = [System.Collections.Generic.List[string]]::new()
    foreach ($m in [regex]::Matches($Expression, $pattern)) {
        $vals.Add("$($m.Groups[1].Value) $($m.Groups[2].Value)")
    }
    return @($vals)
}

# Every conditional edge in the package, with its decision point resolved.
$conditionUsages = [System.Collections.Generic.List[object]]::new()
foreach ($sc in $scopes) {
    foreach ($e in $sc.Edges) {
        if ([string]::IsNullOrWhiteSpace($e.condition)) { continue }
        $conditionUsages.Add([pscustomobject]@{
            Process = $sc.Process
            Scope   = $sc.Scope
            Edge    = $e
            Gateway = $nodeById[$e.from]
            Target  = $nodeById[$e.to]
        })
    }
}

# Renders the competing branches of one decision point, so a condition is never
# read in isolation from the alternative that fires when it is false.
function Get-Branches {
    param([string]$GatewayId, [string]$HighlightEdgeId)

    $branches = [System.Collections.Generic.List[object]]::new()
    if (-not $outOf.ContainsKey($GatewayId)) { return @($branches) }

    foreach ($e in ($outOf[$GatewayId] | Sort-Object { $_.id })) {
        $tgt = $nodeById[$e.to]
        $branches.Add([ordered]@{
            isThisOne     = ($e.id -eq $HighlightEdgeId)
            label         = $e.label
            conditionType = $e.conditionType
            condition     = $e.condition
            leadsTo       = (Get-NodeLabel $tgt)
            thenPath      = (Get-ForwardTrace -FromNodeId $e.to -Depth $TraceDepth)
        })
    }
    return @($branches)
}

# ------------------------------------------------- item 1: unresolved ids ----

$identityUsage = @{}
foreach ($u in $conditionUsages) {
    foreach ($id in (Get-ConditionIdentifiers $u.Edge.condition)) {
        if (-not $identityUsage.ContainsKey($id)) {
            $identityUsage[$id] = [System.Collections.Generic.List[object]]::new()
        }
        $identityUsage[$id].Add($u)
    }
}

# --------------------------------------------------- prior human answers ----

# Loaded before the items are built so every item can report whether its question
# was already answered. Key is "section|entry|property".
$existing = @{}
if (Test-Path $GlossaryPath) {
    $section = ''; $entry = ''
    foreach ($line in (Get-Content $GlossaryPath -Encoding UTF8)) {
        if ($line -match '^([A-Za-z_][A-Za-z0-9_]*):\s*$')      { $section = $Matches[1]; $entry = ''; continue }
        if ($line -match '^  ([^\s#][^:]*):\s*$')               { $entry = $Matches[1].Trim('"'); continue }
        if ($line -match '^    ([A-Za-z_]+):\s*(.+?)\s*$' -and $section -and $entry) {
            $val = $Matches[2]
            if ($val -ne '""' -and $val -ne "''") { $existing["$section|$entry|$($Matches[1])"] = $val }
        }
    }
}

function Get-Kept {
    param([string]$Section, [string]$Entry, [string]$Prop)
    $k = "$Section|$Entry|$Prop"
    if ($existing.ContainsKey($k)) { return $existing[$k] }
    return '""'
}

# ------------------------------------------------- structured handoff ----

# Every item separates content (briefing, evidence) from metadata (below), so a
# downstream consumer - human or agent - can weigh, locate and close the finding
# without re-reading the XPDL.

# How far this finding can be trusted without a human ruling.
function New-Confidence {
    param([ValidateSet('high', 'medium', 'low')][string]$Level, [string]$Basis)
    return [ordered]@{ level = $Level; basis = $Basis; verified = $false }
}

# Where the finding came from, addressable in the raw XPDL - the short ids used
# for display cannot be resolved back to a source element, these can.
function New-SourceRef {
    param([string]$Process, [string]$Scope, [string]$ElementId, [string]$ElementType = 'Activity')
    return [ordered]@{
        tibcoFile = $model.source.file
        elementId = $ElementId
        xpath     = "//xpdl2:$ElementType[@Id='$ElementId']"
        artifact  = 'process-model.json'
        pointer   = "processes[$Process].scopes[$Scope]"
    }
}

# The single place a human answer may live, and whether it is already there. The
# dossier stays evidence-only: it records the address, never the answer.
function New-Resolution {
    param([string]$Section, [string]$Key, [string[]]$Props = @())
    if ([string]::IsNullOrEmpty($Section)) {
        return [ordered]@{
            status   = 'unresolved'
            answered = $false
            answerIn = $null
            key      = $null
            note     = 'Sem slot no glossario: exige decisao escrita registrada fora deste artefato.'
        }
    }
    $answered = $false
    foreach ($p in $Props) { if ($existing.ContainsKey("$Section|$Key|$p")) { $answered = $true } }
    return [ordered]@{
        status   = $(if ($answered) { 'answered' } else { 'unresolved' })
        answered = $answered
        answerIn = "config/glossary/$(Split-Path -Leaf $GlossaryPath)"
        key      = "$Section.$Key"
        note     = $null
    }
}

# Ordering for the analyst worklist. A construct with no .NET equivalent outranks
# everything else: until it is ruled on, any implementation of the nodes it touches
# is a guess, and the guess fails silently.
function Get-Priority {
    param([string]$Category, [string]$Severity, [bool]$NoEquivalent)
    if ($NoEquivalent -and $Severity -eq 'high') { return 'P1' }
    if ($NoEquivalent)                           { return 'P2' }
    if ($Severity -eq 'blocker')                 { return 'P2' }
    if ($Category -in @('sentinel-branch', 'unresolved-identifier')) { return 'P3' }
    return 'P4'
}

$PriorityRank = @{ P1 = 1; P2 = 2; P3 = 3; P4 = 4 }

$items = [System.Collections.Generic.List[object]]::new()

# The POC document says what each stage exists to prove. Whoever answers an open
# question needs that, and it is the one thing the XPDL cannot supply.
$intentByNode = @{}
$intentByProcess = @{}
if ($IntentPath -and (Test-Path $IntentPath)) {
    $intent = Get-Content $IntentPath -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($st in $intent.stages) {
        foreach ($el in $st.matchedElements) {
            $rec = [ordered]@{
                stage = $st.stage; title = $st.title
                concepts = @($st.concepts); matchedOn = $el.matchedOn
            }
            if ($el.kind -eq 'process') {
                if (-not $intentByProcess.ContainsKey($el.name)) { $intentByProcess[$el.name] = $rec }
            }
            elseif (-not $intentByNode.ContainsKey($el.id)) { $intentByNode[$el.id] = $rec }
        }
    }
}

# A grouped finding can span several stages, so all of them are returned - picking
# the first would be arbitrary and would hide the others.
function Get-Intent {
    param([string[]]$Processes, [string[]]$NodeIds)
    $hits = [System.Collections.Generic.List[object]]::new()
    $seen = @{}
    foreach ($id in @($NodeIds)) {
        if ($id -and $intentByNode.ContainsKey($id) -and -not $seen.ContainsKey($intentByNode[$id].stage)) {
            $seen[$intentByNode[$id].stage] = $true; $hits.Add($intentByNode[$id])
        }
    }
    foreach ($p in @($Processes)) {
        if ($p -and $intentByProcess.ContainsKey($p) -and -not $seen.ContainsKey($intentByProcess[$p].stage)) {
            $seen[$intentByProcess[$p].stage] = $true; $hits.Add($intentByProcess[$p])
        }
    }
    return @($hits | Sort-Object stage)
}

$techByName = @{}
foreach ($t in @($fields.technicalFields)) { if ($t) { $techByName[$t.name] = $t } }

foreach ($id in ($identityUsage.Keys | Sort-Object)) {
    $isField    = $fieldByName.ContainsKey($id)
    $isEnvelope = $Envelope.ContainsKey($id)
    $tech       = $techByName[$id]
    if ($isField) { continue }   # declared fields are handled by the glossary section

    $usages = $identityUsage[$id]
    $values = [System.Collections.Generic.List[string]]::new()
    foreach ($u in $usages) { foreach ($v in (Get-ComparedValues $id $u.Edge.condition)) { $values.Add($v) } }
    $values = @($values | Sort-Object -Unique)

    $procs = @($usages | ForEach-Object { $_.Process } | Sort-Object -Unique)

    if ($tech) {
        # The .form declaration downgrades this from "unknown" to "known but outside the domain".
        $status   = 'declared-in-form'
        $severity = 'review'
        $confLevel = 'medium'
        $confBasis = 'declaracao encontrada no formulario TIBCO; semantica dos valores nao confirmada'
        $origin   = $(if ($isEnvelope) { " Origem no envelope: $($Envelope[$id])." } else { '' })
        $mut      = $(if ($tech.inout -eq 'IN') { 'so e lido pelo processo (IN)' } else { "e alterado pelo processo ou pelo usuario ($($tech.inout))" })
        $briefing = "'$id' nao e um dos 209 campos de negocio, mas TEM declaracao: o formulario TIBCO " +
                    "$($tech.declaredIn -join ', ') o declara como $($tech.declaredType)" +
                    $(if ($tech.maxLength) { " (tamanho $($tech.maxLength))" } else { '' }) +
                    ", e $mut.$origin " +
                    "Ele decide o fluxo em $($usages.Count) ponto(s), nos processos $($procs -join ', '), " +
                    "portanto o modelo .NET precisa expor esse valor - fora do modelo de dominio."
        $questions = @(
            "Confirmar o dominio de valores de '$id' e qual deles significa sucesso.",
            "Definir onde '$id' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?)."
        )
    }
    elseif ($isEnvelope) {
        $status   = 'technical-envelope'
        $severity = 'review'
        $confLevel = 'medium'
        $confBasis = "origem $($EnvelopeOrigin[$id]); semantica dos valores nao confirmada"
        $briefing = "'$id' nao e um dos 209 campos de negocio: pertence ao envelope tecnico ($($Envelope[$id])). " +
                    "Mesmo assim ele decide o fluxo em $($usages.Count) ponto(s), nos processos $($procs -join ', '). " +
                    "O modelo .NET precisa expor esse valor explicitamente, senao a ramificacao nao tem como ser reproduzida."
        $questions = @(
            "Confirmar o dominio de valores de '$id' e qual deles significa sucesso.",
            "Definir onde '$id' passa a morar no modelo .NET (contexto de execucao? resultado da chamada?)."
        )
    }
    else {
        $status   = 'undeclared'
        $severity = 'blocker'
        $confLevel = 'low'
        $confBasis = 'declaracao ausente nos artefatos entregues; pacotes externos referenciados nao foram fornecidos'
        $briefing = "'$id' decide o fluxo em $($usages.Count) ponto(s), nos processos $($procs -join ', '), " +
                    "mas nao e DataField, nao e FormalParameter e nao pertence ao envelope tecnico conhecido. " +
                    "O XPDL entregue referencia $($model.externalPackages.PSObject.Properties.Count) pacotes externos cujos arquivos NAO foram fornecidos, " +
                    "portanto a declaracao provavelmente esta em um deles. Sem essa definicao o comportamento tem de ser autorizado por escrito."
        $questions = @(
            "De onde vem '$id'? Campo herdado de pacote externo, campo predefinido do iProcess, ou variavel de passo?",
            "Qual o dominio de valores e o valor inicial de '$id'?",
            "Quem escreve '$id', e em que momento do fluxo?"
        )
    }

    $usageDetail = [System.Collections.Generic.List[object]]::new()
    $sourceRefs  = [System.Collections.Generic.List[object]]::new()
    foreach ($u in $usages) {
        $usageDetail.Add([ordered]@{
            process      = $u.Process
            scope        = $u.Scope
            decisionId   = (Get-ShortId $u.Edge.from)
            decisionElementId = $u.Edge.from
            decision     = (Get-NodeLabel $u.Gateway)
            arrivesFrom  = (Get-ArrivesFrom -NodeId $u.Edge.from)
            condition    = $u.Edge.condition
            branches     = (Get-Branches -GatewayId $u.Edge.from -HighlightEdgeId $u.Edge.id)
        })
        $sourceRefs.Add((New-SourceRef -Process $u.Process -Scope $u.Scope -ElementId $u.Edge.id -ElementType 'Transition'))
    }

    $items.Add([ordered]@{
        id                 = "IDENT-$id"
        category           = 'unresolved-identifier'
        severity           = $severity
        subject            = $id
        declarationStatus  = $status
        briefing           = $briefing
        comparedAgainst    = $values
        usedInProcesses    = $procs
        usages             = @($usageDetail)
        questionsForAnalyst = $questions
        confidence         = (New-Confidence -Level $confLevel -Basis $confBasis)
        sourceRef          = @($sourceRefs)
        resolution         = (New-Resolution -Section 'unresolved' -Key $id -Props @('origin', 'term', 'description', 'values'))
    })
}

# --------------------------------------------- item 2: clone divergences ----

# Processes with the same structural signature are copies of one template. Any
# difference in their conditions is either a deliberate variation or a copy-paste
# defect - both need a human ruling, and neither should be guessed by a generator.
$signature = @{}
foreach ($proc in $model.processes) {
    $kinds = [System.Collections.Generic.List[string]]::new()
    $conds = [System.Collections.Generic.List[string]]::new()
    foreach ($scope in $proc.scopes) {
        foreach ($n in $scope.nodes) { $kinds.Add($n.kind) }
        foreach ($e in $scope.edges) { if ($e.condition) { $conds.Add($e.condition.Trim()) } }
    }
    $sig = (($kinds | Group-Object | Sort-Object Name | ForEach-Object { "$($_.Name):$($_.Count)" }) -join ',')
    if (-not $signature.ContainsKey($sig)) { $signature[$sig] = [System.Collections.Generic.List[object]]::new() }
    $signature[$sig].Add([pscustomobject]@{
        Name       = $proc.name
        Conditions = @($conds | Sort-Object)
    })
}

foreach ($sig in ($signature.Keys | Sort-Object)) {
    $group = $signature[$sig]
    if ($group.Count -lt 2) { continue }

    $variants = $group | Group-Object { $_.Conditions -join ' || ' }
    if ($variants.Count -lt 2) { continue }

    $majority = $variants | Sort-Object Count -Descending | Select-Object -First 1
    $majorSet = $majority.Group[0].Conditions

    foreach ($variant in $variants) {
        if ($variant.Name -eq $majority.Name) { continue }
        $odd     = $variant.Group[0]
        $only    = @($odd.Conditions | Where-Object { $_ -notin $majorSet })
        $missing = @($majorSet        | Where-Object { $_ -notin $odd.Conditions })
        $others  = @($majority.Group  | ForEach-Object { $_.Name } | Sort-Object)

        $items.Add([ordered]@{
            id       = "CLONE-$($odd.Name)"
            category = 'clone-divergence'
            severity = 'blocker'
            subject  = $odd.Name
            briefing = "$($odd.Name) tem estrutura identica a $($others -join ', '), ou seja, sao copias do mesmo template. " +
                       "As condicoes, porem, divergem. Como esse template concentra o tratamento de erro e retentativa de TODAS as chamadas de servico, " +
                       "uma divergencia aqui muda a politica de erro de uma integracao especifica. " +
                       "E preciso decidir se e variacao intencional ou defeito de copia no original."
            divergence = [ordered]@{
                onlyInThisProcess = $only
                presentInSiblings = $missing
                siblings          = $others
            }
            questionsForAnalyst = @(
                "A diferenca em $($odd.Name) e intencional ou e um defeito herdado do TIBCO?",
                'Se for defeito: a migracao deve reproduzi-lo fielmente ou corrigi-lo? (decisao que precisa ficar registrada)'
            )
            confidence = (New-Confidence -Level 'high' -Basis 'assinatura estrutural comparada de forma deterministica entre processos irmaos')
            sourceRef  = @([ordered]@{
                tibcoFile = $model.source.file
                elementId = $odd.Name
                xpath     = "//xpdl2:WorkflowProcess[@Name='$($odd.Name)']"
                artifact  = 'process-model.json'
                pointer   = "processes[$($odd.Name)]"
            })
            resolution = (New-Resolution -Section 'rulings' -Key "CLONE-$($odd.Name)" -Props @('decisao', 'justificativa'))
        })
    }
}

# ------------------------------------------ item 3: unlabeled decisions ----

foreach ($u in ($conditionUsages | Sort-Object { $_.Process }, { $_.Edge.from }, { $_.Edge.id })) {
    $gw = $u.Gateway
    if ($null -eq $gw) { continue }
    $hasLabel = -not ([string]::IsNullOrWhiteSpace($gw.Node.displayName) -and
                      [string]::IsNullOrWhiteSpace($gw.Node.name))
    if ($hasLabel) { continue }
    if ($items | Where-Object { $_.id -eq "DECISION-$($u.Process)-$(Get-ShortId $u.Edge.from)" }) { continue }

    $items.Add([ordered]@{
        id          = "DECISION-$($u.Process)-$(Get-ShortId $u.Edge.from)"
        category    = 'unlabeled-decision'
        severity    = 'review'
        subject     = "$($u.Process) / $(Get-ShortId $u.Edge.from)"
        briefing    = "Este ponto de decisao nao tem nome nenhum no XPDL - nem name, nem xpdExt:DisplayName. " +
                      "No diagrama BPMN ele aparece como um losango vazio, e nenhum revisor consegue aprovar o que nao consegue ler. " +
                      "O contexto abaixo (o que acontece antes e para onde cada ramo leva) existe para que se possa nomear a pergunta que este gateway faz."
        process     = $u.Process
        scope       = $u.Scope
        arrivesFrom = (Get-ArrivesFrom -NodeId $u.Edge.from)
        branches    = (Get-Branches -GatewayId $u.Edge.from -HighlightEdgeId $null)
        questionsForAnalyst = @(
            'Qual pergunta de negocio este gateway faz? (vira o rotulo do losango no BPMN)',
            'Cada ramo esta rotulado com a resposta correspondente?'
        )
        confidence = (New-Confidence -Level 'high' -Basis 'ausencia de name e de xpdExt:DisplayName no XPDL - fato verificavel')
        sourceRef  = @((New-SourceRef -Process $u.Process -Scope $u.Scope -ElementId $u.Edge.from -ElementType 'Activity'))
        resolution = (New-Resolution -Section 'decisions' -Key "$($u.Process)/$(Get-ShortId $u.Edge.from)" -Props @('question', 'branches'))
    })
}

# --------------------------------------------- item 4: sentinel branches ----

foreach ($u in ($conditionUsages | Sort-Object { $_.Process }, { $_.Edge.id })) {
    if ($u.Edge.condition -notmatch 'SW_NA') { continue }

    $idsInCond = @(Get-ConditionIdentifiers $u.Edge.condition)
    $items.Add([ordered]@{
        id       = "SENTINEL-$($u.Process)-$(Get-ShortId $u.Edge.id)"
        category = 'sentinel-branch'
        severity = 'review'
        subject  = ($idsInCond -join ', ')
        briefing = "Esta condicao testa o sentinela SW_NA do iProcess, que e um terceiro estado distinto: nao e null e nao e string vazia. " +
                   "O campo tem tres caminhos possiveis (valor definido / SW_NA / demais valores). " +
                   "Traduzir SW_NA para null em C# muda silenciosamente qual ramo dispara."
        process    = $u.Process
        scope      = $u.Scope
        decision   = (Get-NodeLabel $u.Gateway)
        condition  = $u.Edge.condition
        arrivesFrom = (Get-ArrivesFrom -NodeId $u.Edge.from)
        branches   = (Get-Branches -GatewayId $u.Edge.from -HighlightEdgeId $u.Edge.id)
        questionsForAnalyst = @(
            'O que significa, no negocio, este campo estar "nao preenchido" neste ponto?',
            'O ramo de SW_NA deve seguir junto com algum dos outros ou merece tratamento proprio?'
        )
        confidence = (New-Confidence -Level 'high' -Basis 'ocorrencia textual do sentinela SW_NA na condicao da transicao')
        sourceRef  = @((New-SourceRef -Process $u.Process -Scope $u.Scope -ElementId $u.Edge.id -ElementType 'Transition'))
        resolution = (New-Resolution -Section 'rulings' -Key "SENTINEL-$($u.Process)-$(Get-ShortId $u.Edge.id)" -Props @('decisao', 'justificativa'))
    })
}

# ------------------------------------- item 5: constructs with no .NET peer ----

# One ruling per CONSTRUCT, not per occurrence: the analyst decides once and the
# decision applies to every node that uses it. Options come from the declared
# catalogue - this script enumerates them and never picks one.
$catalog = $null
if (Test-Path $CatalogPath) { $catalog = Get-Content $CatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json }

$hazards = @($model.derived.migrationHazards)
if ($catalog -and $hazards.Count -gt 0) {
    foreach ($grp in ($hazards | Group-Object category | Sort-Object Name)) {
        $cat = $catalog.categories.PSObject.Properties[$grp.Name]
        if (-not $cat) { continue }
        $spec = $cat.Value

        $worst   = $(if (@($grp.Group | Where-Object { $_.severity -eq 'high' }).Count -gt 0) { 'high' } else { 'medium' })
        $procs   = @($grp.Group | ForEach-Object { $_.process } | Sort-Object -Unique)
        $symbols = @($grp.Group | ForEach-Object { $_.symbols } | Where-Object { $_ } | Sort-Object -Unique)

        $occurrences = foreach ($h in ($grp.Group | Sort-Object process, node)) {
            [ordered]@{
                process = $h.process
                node    = $h.node
                nodeId  = $h.nodeId
                detail  = $h.detail
            }
        }

        $options = foreach ($o in @($spec.options)) {
            [ordered]@{
                id          = $o.id
                approach    = $o.approach
                consequence = $o.consequence
                suggested   = [bool]$o.recomendada
            }
        }

        $items.Add([ordered]@{
            id       = "NOEQ-$($grp.Name)"
            category = 'no-net-equivalent'
            severity = $worst
            priority = (Get-Priority -Category 'no-net-equivalent' -Severity $worst -NoEquivalent $true)
            subject  = $grp.Name
            briefing = "$($spec.construct). NAO HA equivalente direto em .NET: $($spec.whyNoEquivalent) " +
                       "Ocorre em $($grp.Count) ponto(s), nos processos $($procs -join ', '). " +
                       "Risco de ignorar: $($spec.riskIfIgnored) " +
                       "As opcoes abaixo sao as alternativas conhecidas - a escolha e do gate humano e vale para todas as ocorrencias."
            occurrenceCount = $grp.Count
            usedInProcesses = $procs
            symbols         = $symbols
            occurrences     = @($occurrences)
            suggestedOptions = @($options)
            questionsForAnalyst = @(
                "Qual opcao adotar para '$($grp.Name)'? (indicar o id da opcao)",
                'A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?'
            )
            confidence = (New-Confidence -Level 'high' -Basis 'construcao detectada deterministicamente no XPDL; ausencia de equivalente e fato declarado no catalogo')
            sourceRef  = @(foreach ($h in ($grp.Group | Sort-Object process, node)) {
                New-SourceRef -Process $h.process -Scope 'MAIN' -ElementId $h.nodeId -ElementType 'Activity'
            })
            resolution = (New-Resolution -Section 'gaps' -Key $grp.Name -Props @('opcaoEscolhida', 'justificativa'))
        })
    }

    # Nem toda a construcao sem equivalente produz hazard. O graft step deixou de
    # produzir quando as ProcessInterface passaram a resolver as chamadas dinamicas
    # - e continua la, declarado no nome que o autor deu aos passos. O catalogo diz
    # por que palavra procurar; sem isto, o conceito de destaque da etapa 2 da POC
    # desaparecia do dossie sem ninguem dar por isso.
    $comHazard = @($hazards | ForEach-Object { $_.category } | Sort-Object -Unique)
    foreach ($prop in $catalog.categories.PSObject.Properties) {
        $nome = $prop.Name
        $spec = $prop.Value
        if ($nome -in $comHazard) { continue }
        if (-not $spec.detectByNodeName) { continue }

        $achados = @(foreach ($p in $model.processes) {
            foreach ($s in $p.scopes) {
                foreach ($n in $s.nodes) {
                    if ($n.displayName -notmatch [regex]::Escape($spec.detectByNodeName)) { continue }
                    [pscustomobject]@{ Process = $p.name; Node = $n.displayName; NodeId = $n.id; Kind = $n.kind }
                }
            }
        })
        if ($achados.Count -eq 0) { continue }

        $procs = @($achados | ForEach-Object { $_.Process } | Sort-Object -Unique)
        $items.Add([ordered]@{
            id       = "NOEQ-$nome"
            category = 'no-net-equivalent'
            severity = 'high'
            priority = (Get-Priority -Category 'no-net-equivalent' -Severity 'high' -NoEquivalent $true)
            subject  = $nome
            briefing = "$($spec.construct). NAO HA equivalente direto em .NET: $($spec.whyNoEquivalent) " +
                       "Detectado pelo NOME dos passos - a palavra '$($spec.detectByNodeName)' - e nao por hazard: " +
                       "as chamadas do pacote resolvem-se todas, logo nenhum detector estrutural dispara aqui. " +
                       "Ocorre em $($achados.Count) ponto(s), nos processos $($procs -join ', '). " +
                       "Risco de ignorar: $($spec.riskIfIgnored)"
            occurrenceCount = $achados.Count
            usedInProcesses = $procs
            symbols         = @()
            occurrences     = @(foreach ($a in ($achados | Sort-Object Process, Node)) {
                [ordered]@{ process = $a.Process; node = $a.Node; nodeId = $a.NodeId
                    detail = "Passo do tipo $($a.Kind) cujo nome declara a construcao." }
            })
            suggestedOptions = @(foreach ($o in @($spec.options)) {
                [ordered]@{ id = $o.id; approach = $o.approach; consequence = $o.consequence; suggested = [bool]$o.recomendada }
            })
            questionsForAnalyst = @(
                "Qual opcao adotar para '$nome'? (indicar o id da opcao)",
                'A opcao escolhida vale para todas as ocorrencias ou alguma exige tratamento proprio?'
            )
            confidence = (New-Confidence -Level 'medium' -Basis "deteccao por nome de passo, nao por estrutura: o pacote nomeia a construcao mas a flag exportada nao a declara")
            sourceRef  = @(foreach ($a in ($achados | Sort-Object Process, Node)) {
                New-SourceRef -Process $a.Process -Scope 'MAIN' -ElementId $a.NodeId -ElementType 'Activity'
            })
            resolution = (New-Resolution -Section 'gaps' -Key $nome -Props @('opcaoEscolhida', 'justificativa'))
        })
    }
}

# ----------------------------- item 6: data conflicts and delivery gaps ----

# These were detectable but lived only inside other artifacts, so nobody was ever
# asked about them. Each family is raised as ONE grouped question: 14 type clashes
# are a single ruling, not fourteen.

function Add-GroupedFinding {
    param(
        [string]$Id, [string]$Category, [string]$Severity, [string]$Subject,
        [string]$Briefing, [array]$Occurrences, [string[]]$Questions,
        [string]$ConfidenceBasis, [string]$RulingKey, [array]$SourceRef = @()
    )
    if ($Occurrences.Count -eq 0) { return }
    $items.Add([ordered]@{
        id       = $Id
        category = $Category
        severity = $Severity
        subject  = $Subject
        briefing = $Briefing
        occurrenceCount = $Occurrences.Count
        occurrences     = @($Occurrences)
        questionsForAnalyst = @($Questions)
        confidence = (New-Confidence -Level 'high' -Basis $ConfidenceBasis)
        sourceRef  = @($SourceRef)
        resolution = (New-Resolution -Section 'rulings' -Key $RulingKey -Props @('decisao', 'justificativa'))
    })
}

$typeClashes = @($fields.typeDisagreements)
Add-GroupedFinding -Id 'TYPE-XPDL-VS-FORM' -Category 'type-conflict' -Severity 'blocker' `
    -Subject 'tipo divergente entre XPDL e formulario' -RulingKey 'TYPE-XPDL-VS-FORM' `
    -Occurrences @($typeClashes | ForEach-Object {
        [ordered]@{ field = $_.field; fromXpdl = $_.fromXpdl; fromForm = $_.fromForm; xpdlPrecision = $_.xpdlPrecision; form = $_.form }
    }) `
    -Briefing ("A precisao declarada no XPDL implica um tipo mais largo do que o formulario TIBCO declara " +
               "(tipicamente long contra Integer de 32 bits). Estreitar pode estourar em numero de AIIM; alargar pode quebrar " +
               "o contrato com a tela. E UMA decisao de padrao, valida para todos os campos abaixo, nao uma decisao por campo.") `
    -Questions @(
        'Qual fonte prevalece quando XPDL e formulario discordam: a precisao do XPDL ou o tipo do formulario?',
        'Ha algum campo da lista que exija excecao a essa regra?'
    ) `
    -ConfidenceBasis 'comparacao deterministica entre a precisao declarada no XPDL e o BomPrimitiveType do formulario'

$labelConflicts = @($fields.fields | Where-Object { $_.labelConflictsWith })
Add-GroupedFinding -Id 'LABEL-CONFLICT' -Category 'label-conflict' -Severity 'blocker' `
    -Subject 'rotulo aponta para outro campo' -RulingKey 'LABEL-CONFLICT' `
    -Occurrences @($labelConflicts | ForEach-Object {
        [ordered]@{ field = $_.name; label = $_.labelSuggestion; collidesWith = $_.labelConflictsWith }
    }) `
    -Briefing ("O rotulo que o formulario da a estes campos e, literalmente, o NOME DE OUTRO campo existente. " +
               "Aceitar o rotulo renomearia o campo errado no modelo .NET, e o erro so apareceria em producao. " +
               "Provavel defeito de copia no formulario TIBCO.") `
    -Questions @(
        'Para cada campo: qual e o nome de negocio correto?',
        'O rotulo errado deve ser reportado a SEFAZ para correcao na origem?'
    ) `
    -ConfidenceBasis 'o rotulo normalizado coincide exatamente com o nome de outro campo declarado'

$labelSuggestions = @($fields.fields | Where-Object { $_.labelSuggestion -and -not $_.labelConflictsWith })
Add-GroupedFinding -Id 'LABEL-SUGGESTION' -Category 'label-suggestion' -Severity 'review' `
    -Subject 'rotulos de formulario nao verificados' -RulingKey 'LABEL-SUGGESTION' `
    -Occurrences @($labelSuggestions | ForEach-Object {
        [ordered]@{ field = $_.name; suggestion = $_.labelSuggestion }
    }) `
    -Briefing ("O formulario TIBCO da a estes campos um rotulo de negocio que NAO deriva do nome. " +
               "Sao afirmacoes de terceiros nunca verificadas - ha erro de digitacao conhecido ('Contorle') e ao menos um rotulo truncado. " +
               "Estao propostos como comentario no glossario, campo a campo, para aceitar ou recusar.") `
    -Questions @(
        'Aceitar em bloco os rotulos do formulario como termo de negocio, ou revisar caso a caso?',
        'Havendo divergencia com o vocabulario oficial da SEFAZ, qual prevalece?'
    ) `
    -ConfidenceBasis 'rotulo declarado no .form e diferente do nome do campo apos normalizacao'

$extPkgs = @($model.externalPackages.PSObject.Properties | ForEach-Object { $_.Name })
Add-GroupedFinding -Id 'MISSING-EXTERNAL-PACKAGES' -Category 'source-not-delivered' -Severity 'blocker' `
    -Subject 'pacotes externos referenciados e nao entregues' -RulingKey 'MISSING-EXTERNAL-PACKAGES' `
    -Occurrences @($extPkgs | ForEach-Object { [ordered]@{ package = $_ } }) `
    -Briefing ("O XPDL referencia estes pacotes, mas os arquivos nunca foram entregues. " +
               "E a raiz de varios outros achados deste dossie: identificadores sem declaracao, o campo que nomeia o subprocesso " +
               "grafado e nunca escrito, e campos de tela fora do dicionario. " +
               "Nao e lacuna de analise - e lacuna de entrega, e nenhuma analise adicional a resolve.") `
    -Questions @(
        'A SEFAZ pode entregar estes pacotes? Quais sao viaveis e em que prazo?',
        'Para os que nao vierem: a semantica sera autorizada por escrito ou o escopo sera reduzido?'
    ) `
    -ConfidenceBasis 'declarados em xpdl2:ExternalPackages e ausentes da entrega - fato verificavel'

if ($screens) {
    $undeclared = @($screens.undeclaredFields)
    Add-GroupedFinding -Id 'SCREEN-UNDECLARED-FIELD' -Category 'source-not-delivered' -Severity 'review' `
        -Subject 'campo lido pela tela e ausente do dicionario' -RulingKey 'SCREEN-UNDECLARED-FIELD' `
        -Occurrences @($undeclared | ForEach-Object { [ordered]@{ field = $_ } }) `
        -Briefing ("A tela ASP.NET trava o work item pedindo estes campos, mas eles nao estao entre os campos de caso do pacote. " +
                   "Vem provavelmente de um dos pacotes externos nao entregues. Sem eles a tarefa humana nao pode ser reproduzida por completo.") `
        -Questions @(
            'De qual pacote vem cada campo, e qual o seu tipo e dominio?'
        ) `
        -ConfidenceBasis 'nome extraido de WorkItemLockField no code-behind e ausente do dicionario de campos'

    $missingCtl = @($screens.missingControls)
    Add-GroupedFinding -Id 'SCREEN-MISSING-CONTROLS' -Category 'source-not-delivered' -Severity 'review' `
        -Subject 'controles .ascx referenciados e nao entregues' -RulingKey 'SCREEN-MISSING-CONTROLS' `
        -Occurrences @($missingCtl | ForEach-Object { [ordered]@{ control = $_ } }) `
        -Briefing ("As telas registram estes controles de usuario, mas os arquivos nao vieram. " +
                   "A maior parte da interface das duas tarefas humanas esta dentro deles, entao o que o operador ve " +
                   "permanece desconhecido - o catalogo de telas so consegue descrever o contrato de work item.") `
        -Questions @(
            'Os controles serao entregues ou a interface sera redesenhada do zero em .NET?'
        ) `
        -ConfidenceBasis 'diretiva Register no .aspx apontando para arquivo inexistente na entrega'
}

if ($builtinContract) {
    $unconfirmed = @($builtinContract.builtins | Where-Object { $_.kind -eq 'function' -and $_.semanticsStatus -eq 'unconfirmed' })
    Add-GroupedFinding -Id 'BUILTIN-SEMANTICS' -Category 'builtin-semantics' -Severity 'blocker' `
        -Subject 'semantica dos builtins iProcess nao confirmada' -RulingKey 'BUILTIN-SEMANTICS' `
        -Occurrences @($unconfirmed | ForEach-Object {
            [ordered]@{ builtin = $_.name; callCount = $_.callCount; observedArity = @($_.observedArity) }
        }) `
        -Briefing ("Estas funcoes nao tem equivalente direto em .NET e sua semantica NAO e derivavel da entrega: " +
                   "nao ha TIBCO em execucao nem documentacao do fornecedor. O risco concreto e SUBSTR/SEARCH: " +
                   "se forem base 1 no iProcess e forem portadas como base 0, o recorte perde um caractere e um id de " +
                   "documento chega truncado, sem excecao nenhuma. " +
                   "Ha um vetor de teste comportamental em builtin-contract.json que qualquer implementacao candidata precisa satisfazer.") `
        -Questions @(
            'SUBSTR e SEARCH sao base 1 ou base 0? (confirmar na documentacao do iProcess)',
            'O terceiro argumento de SUBSTR e COMPRIMENTO ou POSICAO FINAL? Os dados atuais nao distinguem os dois casos.',
            'O que SEARCH retorna quando o separador nao existe, e o que SUBSTR faz com comprimento negativo?'
        ) `
        -ConfidenceBasis 'chamadas e aridade extraidas dos scriptTasks; a semantica permanece nao verificada por ausencia de fonte'

    $hardcoded = @($builtinContract.scriptHazards | Where-Object { $_.kind -like 'hardcoded*' })
    Add-GroupedFinding -Id 'SCRIPT-HARDCODED' -Category 'script-scaffolding' -Severity 'blocker' `
        -Subject 'valores fixos embutidos em scriptTask' -RulingKey 'SCRIPT-HARDCODED' `
        -Occurrences @($hardcoded | ForEach-Object {
            [ordered]@{ kind = $_.kind; process = $_.process; node = $_.node; variable = $_.variable; value = $_.value }
        }) `
        -Briefing ("Scripts atribuem valores literais a campos de caso. Entre eles ha uma lista de ids de teste que " +
                   "SOBRESCREVE a entrada real depois do processamento, e enderecos de e-mail nominais fixos no codigo. " +
                   "Portar isso literalmente carrega dado de teste e destinatario pessoal para producao.") `
        -Questions @(
            'Cada valor fixo e andaime de POC (remover) ou parametro legitimo (externalizar em configuracao)?',
            'Os destinatarios de e-mail devem vir de configuracao ou de cadastro?'
        ) `
        -ConfidenceBasis 'atribuicao de literal detectada no corpo do script - fato textual'

    $dead = @($builtinContract.scriptHazards | Where-Object { $_.kind -eq 'commented-out-logic' })
    Add-GroupedFinding -Id 'SCRIPT-COMMENTED-LOGIC' -Category 'script-scaffolding' -Severity 'review' `
        -Subject 'regra comentada divergente da ativa' -RulingKey 'SCRIPT-COMMENTED-LOGIC' `
        -Occurrences @($dead | ForEach-Object {
            [ordered]@{ process = $_.process; node = $_.node; code = $_.value }
        }) `
        -Briefing ("Ha logica comentada nos scripts. Em ao menos um caso a regra desativada testava codigos especificos " +
                   "e foi substituida por uma comparacao generica contra SW_NA - ou seja, o comportamento ATIVO e mais " +
                   "permissivo que o comentado. Precisa-se saber qual dos dois e a regra de negocio correta.") `
        -Questions @(
            'A regra comentada foi desativada de proposito ou e residuo de teste?',
            'A migracao deve reproduzir o comportamento ATIVO (mais permissivo) ou o COMENTADO (restrito)?'
        ) `
        -ConfidenceBasis 'bloco /* */ presente no corpo do script - fato textual'
}

# Priority for the categories raised above, now that all items exist.
foreach ($it in $items) {
    if (-not $it.Contains('priority')) {
        $it['priority'] = (Get-Priority -Category $it.category -Severity $it.severity -NoEquivalent $false)
    }
}

# ------------------------------------------------------------- write json ----

# Analise agentica: entra como HIPOTESE a confirmar, nunca como facto, e fica
# rotulada como tal em cada item. Transforma uma pergunta aberta - 'o que
# significa isto?' - numa fechada - 'confirma que e isto?' -, que custa ao
# analista uma leitura em vez de uma investigacao. O pipeline corre igual sem ela.
$analysis = $null
if (Test-Path $AnalysisPath) { $analysis = Get-Content $AnalysisPath -Raw -Encoding UTF8 | ConvertFrom-Json }
$comHipotese = 0
if ($analysis) {
    foreach ($item in $items) {
        $nota = $analysis.items.PSObject.Properties[$item.id]
        if (-not $nota) { continue }
        $n = $nota.Value
        $item.analise = [ordered]@{
            origem = 'analise agentica sobre os artefactos - NAO verificada com quem opera o processo'
            hipotese = $n.hipotese
            raciocinio = $n.raciocinio
            oQueConfirmaria = $n.oQueConfirmaria
            riscoSeErrada = $n.riscoSeErrada
            confianca = $n.confianca
        }
        # A pergunta deixa de ser aberta: passa a pedir confirmacao de uma hipotese.
        $item.questionsForAnalyst = @(@("CONFIRMA a hipotese? $($n.oQueConfirmaria)") + @($item.questionsForAnalyst))
        $comHipotese++
    }
}

# Attach the design intent last, so every category benefits without each one having
# to know the document exists.
$withIntent = 0
foreach ($item in $items) {
    $nodeIds = @($item.sourceRef | ForEach-Object { $_.elementId })
    $procs = @($item.usedInProcesses) + @($item.process) + @($item.sourceRef | ForEach-Object { $_.process })
    $found = @(Get-Intent -Processes @($procs | Where-Object { $_ }) -NodeIds @($nodeIds | Where-Object { $_ }))
    if ($found.Count -eq 0) { continue }
    $item.intent = [ordered]@{
        note = "Intencao DECLARADA no documento de POC, nao evidencia do XPDL - use como contexto da decisao, nao como prova."
        stages = @(foreach ($f in $found) {
            [ordered]@{ stage = $f.stage; title = $f.title; concepts = @($f.concepts); matchedOn = $f.matchedOn }
        })
    }
    $withIntent++
}

$byCategory = $items | Group-Object { $_.category } | Sort-Object Name
$ordered = @($items | Sort-Object @{ e = { $PriorityRank[$_.priority] } }, @{ e = { $_.category } }, @{ e = { $_.id } })
$dossier = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/review-dossier/v3'
    package   = $model.source.package
    note      = 'Questoes que a migracao nao pode responder mecanicamente, em ordem de prioridade. Cada item traz a evidencia de grafo necessaria para a decisao humana. P1/P2 sao construcoes SEM equivalente em .NET: ate serem decididas, qualquer implementacao dos nos afetados e um palpite. Respostas vao para config/glossary.yaml, nunca neste arquivo.'
    summary   = [ordered]@{
        total        = $items.Count
        blockers     = @($items | Where-Object { $_.severity -eq 'blocker' }).Count
        noNetEquivalent = @($items | Where-Object { $_.category -eq 'no-net-equivalent' }).Count
        answered     = @($items | Where-Object { $_.resolution.answered }).Count
        open         = @($items | Where-Object { -not $_.resolution.answered }).Count
        comHipoteseAgentica = $comHipotese
        byPriority   = [ordered]@{}
        byCategory   = [ordered]@{}
        byConfidence = [ordered]@{}
    }
    items = $ordered
}
foreach ($p in @('P1', 'P2', 'P3', 'P4')) {
    $dossier.summary.byPriority[$p] = @($items | Where-Object { $_.priority -eq $p }).Count
}
foreach ($g in $byCategory) { $dossier.summary.byCategory[$g.Name] = $g.Count }
foreach ($lvl in @('high', 'medium', 'low')) {
    $dossier.summary.byConfidence[$lvl] = @($items | Where-Object { $_.confidence.level -eq $lvl }).Count
}

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$dossier | ConvertTo-Json -Depth 12 | Set-Content -Path $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} itens: {2} P1, {3} P2, {4} sem equivalente .NET; {5} com hipotese; {6} ja respondido(s))" -f `
    $OutPath, $dossier.summary.total, $dossier.summary.byPriority.P1, $dossier.summary.byPriority.P2,
    $dossier.summary.noNetEquivalent, $dossier.summary.comHipoteseAgentica, $dossier.summary.answered)

# --------------------------------------------------------- seed glossary ----

function ConvertTo-YamlComment {
    param([string]$Text, [int]$Indent = 4)
    $pad = ' ' * $Indent
    return "$pad# $($Text -replace '\r?\n', ' ')"
}

$yaml = [System.Collections.Generic.List[string]]::new()
$yaml.Add('# Glossario de negocio - SEFAZ-SP ePAT')
$yaml.Add('#')
$yaml.Add('# ESTE ARQUIVO E PREENCHIDO POR HUMANOS. O gerador apenas SEMEIA as entradas e')
$yaml.Add('# atualiza os comentarios de evidencia; qualquer valor ja preenchido e preservado.')
$yaml.Add('#')
$yaml.Add('# term        nome de negocio, curto. Vira o rotulo no BPMN e o nome da propriedade C#.')
$yaml.Add('# description o que o campo significa e por que ele existe.')
$yaml.Add('# values      significado de cada valor do dominio.')
$yaml.Add('#')
$yaml.Add('# Os comentarios "evidencia:" sao gerados a partir dos artefatos - nao edite,')
$yaml.Add('# eles sao reescritos a cada execucao de gen-review-dossier.ps1.')
$yaml.Add('')
$yaml.Add('version: 1')
$yaml.Add("package: $($model.source.package)")
$yaml.Add('')

# --- fields ---
# Fields that steer a branch, plus any whose form label needs a human ruling.
$yaml.Add('fields:')
$usedFieldNames = @($identityUsage.Keys | Where-Object { $fieldByName.ContainsKey($_) })
$labelled = @($fields.fields | Where-Object { $_.labelSuggestion -or $_.fullName } | ForEach-Object { $_.name })
$usedFieldNames = @(($usedFieldNames + $labelled) | Sort-Object -Unique)
foreach ($name in $usedFieldNames) {
    $f = $fieldByName[$name]
    $usages = if ($identityUsage.ContainsKey($name)) { @($identityUsage[$name]) } else { @() }
    $vals = [System.Collections.Generic.List[string]]::new()
    foreach ($u in $usages) { foreach ($v in (Get-ComparedValues $name $u.Edge.condition)) { $vals.Add($v) } }
    $vals = @($vals | Sort-Object -Unique)

    $typeDesc = $f.clrType
    if ($f.maxLength) { $typeDesc += "($($f.maxLength))" }

    $yaml.Add("  ${name}:")
    $branchNote = if ($usages.Count -gt 0) { "decide $($usages.Count) ramificacao(oes)" } else { 'nao decide ramificacao' }
    $yaml.Add((ConvertTo-YamlComment "evidencia: tipo $typeDesc; $branchNote"))
    if ($vals.Count -gt 0)          { $yaml.Add((ConvertTo-YamlComment "evidencia: comparado com $($vals -join ' , ')")) }
    if ($f.usesSwNaSentinel)        { $yaml.Add((ConvertTo-YamlComment 'evidencia: usa o sentinela SW_NA - campo de TRES estados, nao booleano')) }
    if ($f.fullName) {
        $why = if ($f.nameTruncated) { 'o iProcess corta o nome em 15 caracteres' } else { 'nome abreviado no XPDL' }
        $yaml.Add((ConvertTo-YamlComment "evidencia: nome completo '$($f.fullName)' ($why)"))
    }
    if ($f.labelSuggestion) {
        if ($f.labelConflictsWith) {
            $yaml.Add((ConvertTo-YamlComment "ATENCAO: o formulario rotula este campo como '$($f.labelSuggestion)', que e o nome de OUTRO campo ($($f.labelConflictsWith)). Provavel defeito no formulario - nao aceite sem conferir."))
        }
        else {
            $yaml.Add((ConvertTo-YamlComment "sugestao (rotulo do formulario, NAO verificado): '$($f.labelSuggestion)' - copie para term: se estiver correto"))
        }
    }
    if ($f.usedInForm.Count -gt 0)  { $yaml.Add((ConvertTo-YamlComment "evidencia: preenchido no formulario $(($f.usedInForm | ForEach-Object { $_.form }) -join ', ')")) }
    if ($f.boundToService.Count -gt 0) { $yaml.Add((ConvertTo-YamlComment "evidencia: trafega em $($f.boundToService.Count) chamada(s) de servico")) }
    $yaml.Add("    term: $(Get-Kept 'fields' $name 'term')")
    $yaml.Add("    description: $(Get-Kept 'fields' $name 'description')")
    $yaml.Add("    values: $(Get-Kept 'fields' $name 'values')")
    $yaml.Add('')
}

# --- gaps: construcoes sem equivalente em .NET (prioridade maxima) ---
$yaml.Add('# gaps: construcoes do TIBCO sem equivalente direto em .NET.')
$yaml.Add('# Escolha UMA opcao pelo id e justifique. A decisao vale para todas as ocorrencias.')
$yaml.Add('gaps:')
foreach ($item in ($ordered | Where-Object { $_.category -eq 'no-net-equivalent' })) {
    $key = $item.subject
    $yaml.Add("  ${key}:")
    $yaml.Add((ConvertTo-YamlComment "prioridade: $($item.priority); $($item.occurrenceCount) ocorrencia(s) em $($item.usedInProcesses -join ', ')"))
    foreach ($o in $item.suggestedOptions) {
        $mark = if ($o.suggested) { ' [SUGERIDA - precisa de ratificacao]' } else { '' }
        $yaml.Add((ConvertTo-YamlComment "opcao '$($o.id)'$mark : $($o.approach)"))
        $yaml.Add((ConvertTo-YamlComment "    consequencia: $($o.consequence)"))
    }
    $yaml.Add("    opcaoEscolhida: $(Get-Kept 'gaps' $key 'opcaoEscolhida')")
    $yaml.Add("    justificativa: $(Get-Kept 'gaps' $key 'justificativa')")
    $yaml.Add('')
}

# --- rulings: decisoes que nao sao vocabulario, e sim politica ---
$yaml.Add('# rulings: decisoes de comportamento e de padrao. Nao sao termos de negocio:')
$yaml.Add('# respondem "o que fazer", nao "o que significa". Preencha decisao e justificativa.')
$yaml.Add('rulings:')
foreach ($item in ($ordered | Where-Object { $_.resolution.key -like 'rulings.*' })) {
    $key = $item.resolution.key -replace '^rulings\.', ''
    $yaml.Add("  ${key}:")
    $yaml.Add((ConvertTo-YamlComment "prioridade: $($item.priority); categoria: $($item.category); assunto: $($item.subject)"))
    if ($item.Contains('occurrenceCount')) {
        $yaml.Add((ConvertTo-YamlComment "ocorrencias: $($item.occurrenceCount)"))
    }
    if ($item.Contains('condition') -and $item.condition) {
        $yaml.Add((ConvertTo-YamlComment "condicao: $($item.condition)"))
    }
    if ($item.Contains('divergence') -and $item.divergence) {
        foreach ($c in @($item.divergence.onlyInThisProcess)) { $yaml.Add((ConvertTo-YamlComment "so neste processo: $c")) }
        foreach ($c in @($item.divergence.presentInSiblings))  { $yaml.Add((ConvertTo-YamlComment "presente nos irmaos: $c")) }
    }
    foreach ($q in $item.questionsForAnalyst) { $yaml.Add((ConvertTo-YamlComment "pergunta: $q")) }
    $yaml.Add("    decisao: $(Get-Kept 'rulings' $key 'decisao')")
    $yaml.Add("    justificativa: $(Get-Kept 'rulings' $key 'justificativa')")
    $yaml.Add('')
}

# --- decisions ---
$yaml.Add('decisions:')
foreach ($item in ($items | Where-Object { $_.category -eq 'unlabeled-decision' })) {
    $key = $item.subject.Replace(' / ', '/')
    $yaml.Add("  ""${key}"":")
    foreach ($a in $item.arrivesFrom) { $yaml.Add((ConvertTo-YamlComment "evidencia: chega de $a")) }
    foreach ($b in $item.branches) {
        $cond = if ($b.condition) { $b.condition } else { "[$($b.conditionType)]" }
        $yaml.Add((ConvertTo-YamlComment "evidencia: ramo $cond -> $($b.leadsTo)"))
    }
    $yaml.Add("    question: $(Get-Kept 'decisions' $key 'question')")
    $yaml.Add("    branches: $(Get-Kept 'decisions' $key 'branches')")
    $yaml.Add('')
}

# --- unresolved ---
$yaml.Add('unresolved:')
foreach ($item in ($items | Where-Object { $_.category -eq 'unresolved-identifier' })) {
    $key = $item.subject
    $yaml.Add("  ${key}:")
    $yaml.Add((ConvertTo-YamlComment "evidencia: $($item.declarationStatus); usado em $($item.usedInProcesses -join ', ')"))
    if ($item.comparedAgainst.Count -gt 0) { $yaml.Add((ConvertTo-YamlComment "evidencia: comparado com $($item.comparedAgainst -join ' , ')")) }
    foreach ($q in $item.questionsForAnalyst) { $yaml.Add((ConvertTo-YamlComment "pergunta: $q")) }
    $yaml.Add("    origin: $(Get-Kept 'unresolved' $key 'origin')")
    $yaml.Add("    term: $(Get-Kept 'unresolved' $key 'term')")
    $yaml.Add("    description: $(Get-Kept 'unresolved' $key 'description')")
    $yaml.Add("    values: $(Get-Kept 'unresolved' $key 'values')")
    $yaml.Add('')
}

$glossaryDir = Split-Path -Parent $GlossaryPath
if (-not (Test-Path $glossaryDir)) { New-Item -ItemType Directory -Path $glossaryDir -Force | Out-Null }
($yaml -join "`r`n") | Set-Content -Path $GlossaryPath -Encoding UTF8

$kept = $existing.Count
Write-Host ("Wrote {0}  ({1} campos, {2} decisoes, {3} nao resolvidos; {4} valor(es) humano(s) preservado(s))" -f `
    $GlossaryPath, $usedFieldNames.Count,
    @($items | Where-Object { $_.category -eq 'unlabeled-decision' }).Count,
    @($items | Where-Object { $_.category -eq 'unresolved-identifier' }).Count, $kept)
