#Requires -Version 7.0
<#
.SYNOPSIS
    Pipeline stage S2 - validates the intermediate artifacts.

.DESCRIPTION
    Checks semantic INVARIANTS, not JSON schemas. A conformant-but-wrong artifact
    is the failure mode that matters: a silently dropped branch still parses.

    Two classes of check:

      * Referential integrity  - every id/name that points somewhere must resolve.
      * Source coverage        - the artifact must account for EVERY construct in
                                 the source XPDL. This is the unknown-construct
                                 detector: it is what makes the generators
                                 trustworthy on packages they were not written
                                 against. Requires -XpdlPath.

    Exit code 0 = all checks passed. 1 = at least one FAIL (or WARN with
    -WarningsAsErrors). Intended as a CI gate.

.EXAMPLE
    ./tools/validate-artifacts.ps1
    ./tools/validate-artifacts.ps1 -XpdlPath 'input/.../POC_Epat.xpdl' -WarningsAsErrors
#>
[CmdletBinding()]
param(
    [string]$ArtifactsDir = (Join-Path $PSScriptRoot '..' 'artifacts'),
    [string]$XpdlPath     = '',
    [switch]$WarningsAsErrors
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------- results ----

$script:Results = [System.Collections.Generic.List[object]]::new()

function Add-Check {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][ValidateSet('PASS', 'FAIL', 'WARN', 'SKIP')][string]$Status,
        [string]$Detail = '',
        [string[]]$Offenders = @()
    )
    $script:Results.Add([pscustomobject]@{
            Id        = $Id
            Name      = $Name
            Status    = $Status
            Detail    = $Detail
            Offenders = $Offenders
        })
}

# Assert helper: PASS when $Offenders is empty, otherwise $FailStatus.
function Assert-Empty {
    param(
        [string]$Id,
        [string]$Name,
        [object[]]$Offenders,
        [string]$OkDetail = '',
        [string]$BadDetail = '',
        [ValidateSet('FAIL', 'WARN')][string]$FailStatus = 'FAIL'
    )
    $list = @($Offenders | Where-Object { $_ })
    if ($list.Count -eq 0) {
        Add-Check -Id $Id -Name $Name -Status 'PASS' -Detail $OkDetail
    }
    else {
        Add-Check -Id $Id -Name $Name -Status $FailStatus `
            -Detail ("{0} ({1})" -f $BadDetail, $list.Count) `
            -Offenders ($list | Select-Object -First 12 | ForEach-Object { [string]$_ })
    }
}

function Read-Artifact {
    param([string]$FileName)
    $path = Join-Path $ArtifactsDir $FileName
    if (-not (Test-Path -LiteralPath $path)) { return $null }
    Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
}

function Test-HasProp {
    param($Object, [string]$Name)
    if ($null -eq $Object) { return $false }
    [bool]$Object.PSObject.Properties[$Name]
}

function Get-Prop {
    param($Object, [string]$Name, $Default = $null)
    if (-not (Test-HasProp $Object $Name)) { return $Default }
    $value = $Object.$Name
    # A present-but-null property must still fall back, otherwise callers that
    # pass a default silently receive $null.
    if ($null -eq $value) { return $Default }
    $value
}

# ------------------------------------------------------------- known sets ----

# Any node kind emitted by gen-process-model.ps1. A kind outside this set means
# the generator invented something undocumented, OR a hand-edit crept in.
$KnownKinds = @(
    'startEvent', 'endEvent', 'gateway', 'userTask', 'serviceTask', 'emailTask',
    'scriptTask', 'receiveTask', 'callActivity', 'subProcessScope',
    'linkThrow', 'linkCatch', 'signalThrow', 'signalCatch', 'timerEvent'
)

$KnownClrTypes = @(
    'string', 'int', 'long', 'bool', 'decimal', 'double',
    'DateOnly', 'TimeOnly', 'DateTime', 'TimeSpan'
)

$KnownConditionTypes = @('CONDITION', 'OTHERWISE', 'UNCONDITIONAL', 'DEFAULTEXCEPTION', 'EXCEPTION')

# Identifiers that appear in condition expressions but are NOT case fields.
$ExpressionBuiltins = @(
    'true', 'false', 'null', 'undefined', 'var', 'if', 'else', 'return',
    'IPESystemValues', 'IPEStringUtil', 'IPEDateTimeUtil', 'IPEMathUtil', 'Math'
)

# ---------------------------------------------------------------- loading ----

Write-Host ''
Write-Host "Validating artifacts in: $ArtifactsDir" -ForegroundColor Cyan

$model    = Read-Artifact 'process-model.json'
$fields   = Read-Artifact 'case-field-dictionary.json'
$services = Read-Artifact 'service-contracts.json'
$rules    = Read-Artifact 'decision-tables.json'

if ($null -eq $model) {
    Write-Host "FATAL: process-model.json not found in $ArtifactsDir" -ForegroundColor Red
    exit 2
}

# Flatten the model once.
$nodes = [System.Collections.Generic.List[object]]::new()
$edges = [System.Collections.Generic.List[object]]::new()
$scopeList = [System.Collections.Generic.List[object]]::new()
$scopeKeys = @{}

foreach ($proc in $model.processes) {
    foreach ($scope in $proc.scopes) {
        $scopeName = Get-Prop $scope 'scope' 'MAIN'
        $scopeKey = '{0}::{1}' -f $proc.id, (Get-Prop $scope 'scopeId' $scopeName)
        $scopeKeys[$scopeKey] = $true
        $scopeList.Add([pscustomobject]@{
                Key = $scopeKey; Name = $scopeName; ProcessId = $proc.id
            })
        foreach ($n in $scope.nodes) {
            $nodes.Add([pscustomobject]@{
                    Node = $n; Process = $proc.displayName; ProcessId = $proc.id
                    Scope = $scopeName; ScopeKey = $scopeKey
                })
        }
        foreach ($e in $scope.edges) {
            $edges.Add([pscustomobject]@{
                    Edge = $e; Process = $proc.displayName; ScopeKey = $scopeKey
                })
        }
    }
}

$nodeById = @{}
foreach ($entry in $nodes) {
    $nodeById[$entry.Node.id] = $entry
}

$fieldNames = @{}
$technicalNames = @{}
if ($fields) {
    foreach ($f in $fields.fields) { $fieldNames[$f.name] = $f }
    foreach ($t in (Get-Prop $fields 'technicalFields')) { $technicalNames[$t.name] = $t }
}

# =============================================================== PM checks ====

# PM-001 - duplicate node ids would silently merge nodes downstream.
$dupIds = $nodes | Group-Object { $_.Node.id } | Where-Object Count -gt 1 |
    ForEach-Object { '{0} (x{1})' -f $_.Name, $_.Count }
Assert-Empty 'PM-001' 'Node ids are globally unique' $dupIds `
    -OkDetail "$($nodes.Count) nodes" -BadDetail 'duplicate node ids'

# PM-002 - UNKNOWN-CONSTRUCT DETECTOR. An unrecognised kind means the extractor
# met something it was never taught, and downstream emitters will drop it.
$badKinds = $nodes |
    Where-Object { $_.Node.kind -notin $KnownKinds -and $_.Node.kind -notlike 'intermediateEvent:*' } |
    ForEach-Object { '{0} [{1}] in {2}' -f $_.Node.kind, $_.Node.id, $_.Process }
Assert-Empty 'PM-002' 'All node kinds are recognised' $badKinds `
    -OkDetail "$(($nodes | Group-Object { $_.Node.kind }).Count) distinct kinds" `
    -BadDetail 'unrecognised node kinds'

# PM-003 - a dangling edge endpoint is a severed branch.
$danglers = foreach ($entry in $edges) {
    $e = $entry.Edge
    foreach ($end in 'from', 'to') {
        $target = $e.$end
        if (-not $target) {
            '{0}.{1} is empty ({2})' -f $e.id, $end, $entry.Process
        }
        elseif (-not $nodeById.ContainsKey($target)) {
            '{0}.{1} -> {2} not found ({3})' -f $e.id, $end, $target, $entry.Process
        }
    }
}
Assert-Empty 'PM-003' 'Every edge endpoint resolves to a node' $danglers `
    -OkDetail "$($edges.Count) edges" -BadDetail 'dangling edge endpoints'

# PM-004 - conditionType drives gateway translation; an unknown value is silent misrouting.
$badCondTypes = $edges |
    Where-Object { (Get-Prop $_.Edge 'conditionType') -and $_.Edge.conditionType -notin $KnownConditionTypes } |
    ForEach-Object { '{0} on {1}' -f $_.Edge.conditionType, $_.Edge.id }
Assert-Empty 'PM-004' 'All edge conditionTypes are recognised' $badCondTypes `
    -BadDetail 'unrecognised conditionType'

# PM-005 - activitySetId is the FK from a subProcessScope node to its scope.
$badScopeRefs = foreach ($entry in $nodes) {
    $n = $entry.Node
    if ($n.kind -eq 'subProcessScope') {
        $asid = Get-Prop $n 'activitySetId'
        if (-not $asid) {
            'no activitySetId on {0} [{1}]' -f $n.displayName, $n.id
        }
        else {
            $key = '{0}::{1}' -f $entry.ProcessId, $asid
            if (-not $scopeKeys.ContainsKey($key)) {
                '{0} -> scope {1} missing ({2})' -f $n.id, $asid, $entry.Process
            }
        }
    }
}
Assert-Empty 'PM-005' 'Every subProcessScope resolves to a scope' $badScopeRefs `
    -OkDetail "$(($nodes | Where-Object { $_.Node.kind -eq 'subProcessScope' }).Count) embedded sub-processes" `
    -BadDetail 'unresolved activitySetId'

# PM-006 - a boundary event whose host vanished would attach to nothing.
$badBoundary = foreach ($entry in $nodes) {
    $n = $entry.Node
    if ((Get-Prop $n 'boundary') -eq $true) {
        $host_ = Get-Prop $n 'attachedTo'
        if (-not $host_) { 'boundary {0} has no attachedTo' -f $n.id }
        elseif (-not $nodeById.ContainsKey($host_)) { 'boundary {0} -> host {1} missing' -f $n.id, $host_ }
    }
}
Assert-Empty 'PM-006' 'Every boundary event resolves to its host' $badBoundary `
    -OkDetail "$(($nodes | Where-Object { (Get-Prop $_.Node 'boundary') -eq $true }).Count) boundary events" `
    -BadDetail 'orphan boundary events'

# PM-007 - link throw/catch are cross-lane GOTOs; an unpaired throw is a dead end.
$throwCount = ($nodes | Where-Object { $_.Node.kind -eq 'linkThrow' }).Count
$catchCount = ($nodes | Where-Object { $_.Node.kind -eq 'linkCatch' }).Count
$linkEdges  = @(Get-Prop (Get-Prop $model 'derived') 'linkEdges' @())
$unpairedLinks = @()
if ($throwCount -ne $linkEdges.Count) {
    $unpairedLinks += 'linkThrow nodes = {0} but derived.linkEdges = {1}' -f $throwCount, $linkEdges.Count
}
foreach ($le in $linkEdges) {
    foreach ($end in 'from', 'to') {
        if (-not $nodeById.ContainsKey($le.$end)) { $unpairedLinks += 'linkEdge {0} -> missing node' -f $le.$end }
    }
}
Assert-Empty 'PM-007' 'Every link GOTO is resolved throw->catch' $unpairedLinks `
    -OkDetail "$throwCount throw / $catchCount catch / $($linkEdges.Count) resolved" `
    -BadDetail 'unresolved link GOTOs'

# PM-008 - a signal with no catch cancels nothing; mutual cancellation would break.
$sigThrow = ($nodes | Where-Object { $_.Node.kind -eq 'signalThrow' }).Count
$sigCatch = ($nodes | Where-Object { $_.Node.kind -eq 'signalCatch' }).Count
$sigEdges = @(Get-Prop (Get-Prop $model 'derived') 'signalEdges' @())
$badSignals = @()
if ($sigThrow -ne $sigEdges.Count) {
    $badSignals += 'signalThrow nodes = {0} but derived.signalEdges = {1}' -f $sigThrow, $sigEdges.Count
}
Assert-Empty 'PM-008' 'Every broadcast signal has a catch' $badSignals `
    -OkDetail "$sigThrow throw / $sigCatch catch" -BadDetail 'unmatched signals'

# PM-009 - unresolved call targets are acceptable ONLY when dynamic (callee name
# is read from a case field at runtime, so it cannot resolve statically).
$unresolvedCalls = foreach ($entry in $nodes) {
    $n = $entry.Node
    if ($n.kind -eq 'callActivity') {
        $call = Get-Prop $n 'call'
        if ($call -and (Get-Prop $call 'resolved') -ne $true -and (Get-Prop $call 'dynamic') -ne $true) {
            '{0} [{1}] -> {2}' -f $n.displayName, $n.id, (Get-Prop $call 'targetId' '?')
        }
    }
}
Assert-Empty 'PM-009' 'Static call activities all resolve' $unresolvedCalls `
    -OkDetail "$(($nodes | Where-Object { $_.Node.kind -eq 'callActivity' }).Count) call activities" `
    -BadDetail 'unresolved non-dynamic calls'

# PM-010 - a branching gateway with conditions but no default silently deadlocks
# when nothing matches. TIBCO tolerates this; most target engines do not.
$noDefault = foreach ($entry in $nodes) {
    $n = $entry.Node
    if ($n.kind -eq 'gateway') {
        $out = @($edges | Where-Object { $_.Edge.from -eq $n.id })
        $conditional = @($out | Where-Object { $_.Edge.conditionType -eq 'CONDITION' })
        $otherwise   = @($out | Where-Object { $_.Edge.conditionType -eq 'OTHERWISE' -or (Get-Prop $_.Edge 'isDefault') -eq $true })
        if ($conditional.Count -gt 0 -and $otherwise.Count -eq 0) {
            '{0} [{1}] in {2}' -f ($n.displayName ?? $n.name ?? '(unnamed)'), $n.id, $entry.Process
        }
    }
}
Assert-Empty 'PM-010' 'Conditional gateways have a default branch' $noDefault `
    -BadDetail 'gateways that can deadlock' -FailStatus 'WARN'

# PM-011 - every scope needs an entry point.
$noStart = foreach ($proc in $model.processes) {
    foreach ($scope in $proc.scopes) {
        $starts = @($scope.nodes | Where-Object { $_.kind -eq 'startEvent' })
        if ($starts.Count -eq 0) { '{0} / {1}' -f $proc.displayName, $scope.scope }
    }
}
Assert-Empty 'PM-011' 'Every scope has a start event' $noStart `
    -OkDetail "$($scopeKeys.Count) scopes" -BadDetail 'scopes with no entry point' -FailStatus 'WARN'

# =============================================================== CF checks ====

if ($null -eq $fields) {
    Add-Check -Id 'CF-*' -Name 'case-field-dictionary.json' -Status 'SKIP' -Detail 'artifact not present'
}
else {
    # CF-001 - a condition referencing an undeclared field cannot be compiled.
    $unknownRefs = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($entry in $edges) {
        $expr = Get-Prop $entry.Edge 'condition'
        if (-not $expr) { continue }
        # Strip string literals, then member access on known builtin classes.
        $clean = $expr -replace "'[^']*'", ' ' -replace '"[^"]*"', ' '
        $clean = $clean -replace 'IPE\w+\s*\.\s*\w+', ' '
        foreach ($m in [regex]::Matches($clean, '\b[A-Z][A-Z0-9_]{1,}\b')) {
            $tok = $m.Value
            if ($tok -in $ExpressionBuiltins) { continue }
            if ($technicalNames.ContainsKey($tok)) { continue }
            if (-not $fieldNames.ContainsKey($tok)) { [void]$unknownRefs.Add($tok) }
        }
    }
    Assert-Empty 'CF-001' 'Condition expressions reference declared fields' @($unknownRefs) `
        -OkDetail "$($fieldNames.Count) case fields + $($technicalNames.Count) envelope fields declared" `
        -BadDetail 'identifiers used in conditions but declared nowhere in the package' -FailStatus 'WARN'

    # CF-005 - the same field typed differently by the XPDL and by the form it appears on.
    # Picking the narrower one silently can overflow, so a human decides.
    $disagreements = @(Get-Prop $fields 'typeDisagreements')
    Assert-Empty 'CF-005' 'XPDL and form agree on field types' `
        @($disagreements | ForEach-Object { "$($_.field): xpdl=$($_.fromXpdl) form=$($_.fromForm)" }) `
        -OkDetail 'no conflicting declarations' `
        -BadDetail 'fields whose XPDL precision and form type disagree' -FailStatus 'WARN'

    # CF-002 - an unmapped CLR type means the emitter has no type to write.
    $badTypes = $fields.fields |
        Where-Object { $_.clrType -and $_.clrType -notin $KnownClrTypes } |
        ForEach-Object { '{0} : {1}' -f $_.name, $_.clrType }
    Assert-Empty 'CF-002' 'All CLR types are in the known mapping' $badTypes `
        -BadDetail 'unmapped CLR types'

    # CF-003 - SW_NA is a third value. If the field is not nullable the sentinel
    # gets collapsed to "" or 0 and 18 branches change meaning.
    $swNaNotNullable = $fields.fields |
        Where-Object { (Get-Prop $_ 'usesSwNaSentinel') -eq $true -and (Get-Prop $_ 'clrNullable') -ne $true } |
        ForEach-Object { $_.name }
    Assert-Empty 'CF-003' 'SW_NA sentinel fields are nullable' $swNaNotNullable `
        -OkDetail "$(($fields.fields | Where-Object { (Get-Prop $_ 'usesSwNaSentinel') -eq $true }).Count) sentinel fields" `
        -BadDetail 'sentinel fields that would lose their third state'

    # CF-004 - a field with no name is unusable downstream.
    $namelessFields = $fields.fields | Where-Object { -not $_.name } | ForEach-Object { 'field at index' }
    Assert-Empty 'CF-004' 'Every field has a name' $namelessFields -BadDetail 'nameless fields'
}

# =============================================================== SC checks ====

if ($null -eq $services) {
    Add-Check -Id 'SC-*' -Name 'service-contracts.json' -Status 'SKIP' -Detail 'artifact not present'
}
else {
    $opNames = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($svc in $services.services) {
        foreach ($op in $svc.operations) { [void]$opNames.Add($op.name) }
    }

    # SC-001 - a service task bound to a non-existent operation cannot be generated.
    $missingOps = foreach ($entry in $nodes) {
        $n = $entry.Node
        if ($n.kind -eq 'serviceTask') {
            $op = Get-Prop $n 'operation'
            $opName = Get-Prop $op 'operationName'
            if (-not $opName) { 'serviceTask {0} has no operationName' -f $n.id }
            elseif (-not $opNames.Contains($opName)) { '{0} -> {1}' -f $n.displayName, $opName }
        }
    }
    Assert-Empty 'SC-001' 'Every service task binds to a catalogued operation' $missingOps `
        -OkDetail "$($opNames.Count) operations catalogued" -BadDetail 'unbound service tasks'

    # SC-002 - the invoked set must agree with what the process actually calls.
    $serviceTaskCount = ($nodes | Where-Object { $_.Node.kind -eq 'serviceTask' }).Count
    $invoked = @(Get-Prop $services 'invokedOperations' @())
    $mismatch = @()
    if ($invoked.Count -ne $serviceTaskCount) {
        $mismatch += 'invokedOperations = {0} but serviceTask nodes = {1}' -f $invoked.Count, $serviceTaskCount
    }
    Assert-Empty 'SC-002' 'invokedOperations matches the service tasks' $mismatch `
        -OkDetail "$serviceTaskCount invoked" -BadDetail 'invocation count mismatch' -FailStatus 'WARN'

    # SC-003 - bindings are the join between payload paths and case fields.
    # ONLY body-bound mappings must resolve to a declared case field. Paths under
    # HEADER/ RESULT/ ERROR are the technical envelope (transaction id, status
    # code, error dump) - plumbing that belongs to a separate technical model,
    # not to the 209-field domain state.
    $badBindings = @()
    $envelopeBindings = [System.Collections.Generic.HashSet[string]]::new()
    if ($fields) {
        foreach ($b in @(Get-Prop $services 'processBindings' @())) {
            foreach ($side in 'inputs', 'outputs') {
                foreach ($m in @(Get-Prop $b $side @())) {
                    $cf = Get-Prop $m 'caseField'
                    if (-not $cf -or $fieldNames.ContainsKey($cf)) { continue }
                    $path = [string](Get-Prop $m 'soapPath' '')
                    # SW_* are IPESystemValues supplied by the engine at runtime
                    # (case number, parent case, host, date). They are never
                    # declared, and may legitimately feed a BODY element - e.g.
                    # SW_PARENTCASE is the correlation key for the graft step.
                    if ($cf -like 'SW_*' -or $path -match '/(HEADER|RESULT|ERROR)/') {
                        [void]$envelopeBindings.Add($cf)
                    }
                    else {
                        $badBindings += '{0}.{1} -> {2}' -f (Get-Prop $b 'operationName' '?'), $side, $cf
                    }
                }
            }
        }
    }
    Assert-Empty 'SC-003' 'Body bindings reference declared case fields' $badBindings `
        -BadDetail 'bindings to undeclared fields'

    # SC-004 - not a defect, but the generator MUST model these separately.
    Assert-Empty 'SC-004' 'Technical envelope fields are outside the domain model' @($envelopeBindings) `
        -OkDetail 'none' `
        -BadDetail 'envelope-bound identifiers needing a technical model' -FailStatus 'WARN'
}

# =============================================================== DT checks ====

if ($null -eq $rules) {
    Add-Check -Id 'DT-*' -Name 'decision-tables.json' -Status 'SKIP' -Detail 'artifact not present'
}
else {
    $vocab = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($v in $rules.vocabulary) {
        if (Get-Prop $v 'path')      { [void]$vocab.Add($v.path) }
        if (Get-Prop $v 'attribute') { [void]$vocab.Add($v.attribute) }
    }

    # DT-001/002 - an LHS outside the vocabulary means the rule reads a term the
    # generated evaluator has no binding for.
    $badLhs = [System.Collections.Generic.HashSet[string]]::new()
    $badActionLhs = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($r in $rules.rules) {
        foreach ($c in @(Get-Prop $r 'conditions' @())) {
            $lhs = Get-Prop $c 'lhs'
            if ($lhs -and -not $vocab.Contains($lhs)) { [void]$badLhs.Add($lhs) }
        }
        foreach ($a in @(Get-Prop $r 'actions' @())) {
            $lhs = Get-Prop $a 'lhs'
            if ($lhs -and -not $vocab.Contains($lhs)) { [void]$badActionLhs.Add($lhs) }
        }
    }
    Assert-Empty 'DT-001' 'Rule conditions use vocabulary terms' @($badLhs) `
        -OkDetail "$($vocab.Count) vocabulary terms" -BadDetail 'conditions outside the vocabulary'
    Assert-Empty 'DT-002' 'Rule actions use vocabulary terms' @($badActionLhs) `
        -BadDetail 'actions outside the vocabulary'

    # DT-003 - rules count must agree between the two projections of the sheet.
    $ruleCount  = @(Get-Prop $rules 'rules' @()).Count
    $tableCount = @(Get-Prop $rules 'decisionTable' @()).Count
    $countMismatch = @()
    if ($ruleCount -ne $tableCount) {
        $countMismatch += 'rules = {0} but decisionTable = {1}' -f $ruleCount, $tableCount
    }
    Assert-Empty 'DT-003' 'rules[] and decisionTable[] agree' $countMismatch `
        -OkDetail "$ruleCount rule columns" -BadDetail 'projection mismatch'

    # DT-004 - column order IS the semantics here (later writes override earlier).
    $cols = @(Get-Prop $rules 'rules' @() | ForEach-Object { Get-Prop $_ 'column' })
    $orderBroken = @()
    for ($i = 1; $i -lt $cols.Count; $i++) {
        if ($null -ne $cols[$i] -and $null -ne $cols[$i - 1] -and [int]$cols[$i] -le [int]$cols[$i - 1]) {
            $orderBroken += 'column {0} follows {1}' -f $cols[$i], $cols[$i - 1]
        }
    }
    Assert-Empty 'DT-004' 'Rule columns are strictly ordered (override semantics)' $orderBroken `
        -OkDetail 'ordering preserved' -BadDetail 'column order corrupted'

    # DT-005 - the Corticon <-> case-field join.
    $badMap = @()
    if ($fields) {
        foreach ($m in @(Get-Prop $rules 'caseFieldMapping' @())) {
            $cf = Get-Prop $m 'caseField'
            if ($cf -and -not $fieldNames.ContainsKey($cf)) { $badMap += $cf }
        }
    }
    Assert-Empty 'DT-005' 'Decision mappings reference declared case fields' $badMap `
        -BadDetail 'mappings to undeclared fields'
}

# ========================================================= source coverage ====

if (-not $XpdlPath) {
    Add-Check -Id 'CV-*' -Name 'Source coverage (pass -XpdlPath to enable)' -Status 'SKIP' `
        -Detail 'strongly recommended for any package this extractor has not seen'
}
elseif (-not (Test-Path -LiteralPath $XpdlPath)) {
    Add-Check -Id 'CV-*' -Name 'Source coverage' -Status 'FAIL' -Detail "XPDL not found: $XpdlPath"
}
else {
    $xpdlDoc = [xml](Get-Content -LiteralPath $XpdlPath -Raw)
    $nsMgr = [System.Xml.XmlNamespaceManager]::new($xpdlDoc.NameTable)
    $nsMgr.AddNamespace('x', 'http://www.wfmc.org/2008/XPDL2.1')

    # Counting is deliberately element-name agnostic: it catches constructs the
    # extractor has never been taught WITHOUT needing to know what they are.
    $srcActivities  = $xpdlDoc.SelectNodes('//x:Activity', $nsMgr).Count
    $srcTransitions = $xpdlDoc.SelectNodes('//x:Transition', $nsMgr).Count
    $srcActivitySet = $xpdlDoc.SelectNodes('//x:ActivitySet', $nsMgr).Count
    $srcProcesses   = $xpdlDoc.SelectNodes('//x:WorkflowProcess', $nsMgr).Count

    $modelScopes = @($scopeList | Where-Object { $_.Name -ne 'MAIN' }).Count

    $cov = @()
    if ($srcActivities -ne $nodes.Count) {
        $cov += 'XPDL Activity = {0} but model nodes = {1} ({2} unaccounted)' -f `
            $srcActivities, $nodes.Count, ($srcActivities - $nodes.Count)
    }
    Assert-Empty 'CV-001' 'Every XPDL Activity produced a node' $cov `
        -OkDetail "$srcActivities activities" -BadDetail 'ACTIVITIES SILENTLY DROPPED'

    $cov2 = @()
    if ($srcTransitions -ne $edges.Count) {
        $cov2 += 'XPDL Transition = {0} but model edges = {1} ({2} unaccounted)' -f `
            $srcTransitions, $edges.Count, ($srcTransitions - $edges.Count)
    }
    Assert-Empty 'CV-002' 'Every XPDL Transition produced an edge' $cov2 `
        -OkDetail "$srcTransitions transitions" -BadDetail 'TRANSITIONS SILENTLY DROPPED'

    $cov3 = @()
    if ($srcActivitySet -ne $modelScopes) {
        $cov3 += 'XPDL ActivitySet = {0} but non-MAIN scopes = {1}' -f $srcActivitySet, $modelScopes
    }
    Assert-Empty 'CV-003' 'Every ActivitySet produced a scope' $cov3 `
        -OkDetail "$srcActivitySet activity sets" -BadDetail 'SCOPES SILENTLY DROPPED'

    $cov4 = @()
    if ($srcProcesses -ne @($model.processes).Count) {
        $cov4 += 'XPDL WorkflowProcess = {0} but model processes = {1}' -f $srcProcesses, @($model.processes).Count
    }
    Assert-Empty 'CV-004' 'Every WorkflowProcess produced a process' $cov4 `
        -OkDetail "$srcProcesses processes" -BadDetail 'PROCESSES SILENTLY DROPPED'
}

# ----------------------------------------------------------------- report ----

Write-Host ''
foreach ($r in $script:Results) {
    $colour = switch ($r.Status) {
        'PASS' { 'Green' }; 'FAIL' { 'Red' }; 'WARN' { 'Yellow' }; default { 'DarkGray' }
    }
    $line = '{0,-5} {1,-7} {2}' -f $r.Id, $r.Status, $r.Name
    if ($r.Detail) { $line += "  -  $($r.Detail)" }
    Write-Host $line -ForegroundColor $colour
    foreach ($o in $r.Offenders) { Write-Host "            . $o" -ForegroundColor DarkGray }
}

$failed  = @($script:Results | Where-Object Status -eq 'FAIL')
$warned  = @($script:Results | Where-Object Status -eq 'WARN')
$passed  = @($script:Results | Where-Object Status -eq 'PASS')
$skipped = @($script:Results | Where-Object Status -eq 'SKIP')

Write-Host ''
Write-Host ('{0} passed, {1} failed, {2} warnings, {3} skipped' -f `
        $passed.Count, $failed.Count, $warned.Count, $skipped.Count) -ForegroundColor Cyan

if ($failed.Count -gt 0) { exit 1 }
if ($WarningsAsErrors -and $warned.Count -gt 0) { exit 1 }
exit 0
