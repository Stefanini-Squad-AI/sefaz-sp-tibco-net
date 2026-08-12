#requires -version 5
<#
  Generates artifacts/process-model.json : a normalized, engine-agnostic
  representation of the TIBCO XPDL package, with iProcess/AMX-BPM specific
  semantics made explicit so a .NET workflow implementation can be derived.
#>
param(
    [string]$XpdlPath = "$PSScriptRoot\..\input\Arquivos Poc Camunda\POC_Camunda\POC_Epat\Process Packages\POC_Epat.xpdl",
    [string]$OutPath = "$PSScriptRoot\..\artifacts\process-model.json"
)

$ErrorActionPreference = 'Stop'
[xml]$x = Get-Content -LiteralPath $XpdlPath -Raw
$ns = New-Object System.Xml.XmlNamespaceManager($x.NameTable)
$ns.AddNamespace('x', 'http://www.wfmc.org/2008/XPDL2.1')
$ns.AddNamespace('e', 'http://www.tibco.com/XPD/xpdExtension1.0.0')
$ns.AddNamespace('ip', 'http://www.tibco.com/XPD/iProcessExt1.0.0')
$ns.AddNamespace('em', 'http://www.tibco.com/XPD/email1.0.0')
$XPDEXT = 'http://www.tibco.com/XPD/xpdExtension1.0.0'
$IPEXT = 'http://www.tibco.com/XPD/iProcessExt1.0.0'

function Txt([string]$s) { if ($null -eq $s) { return $null }; $t = ($s -replace "`r", '').Trim(); if ($t -eq '') { $null } else { $t } }

# ---------------------------------------------------------------- lookups
$laneName = @{}; $lanePool = @{}
foreach ($pool in $x.SelectNodes('//x:Pool', $ns)) {
    foreach ($l in $pool.SelectNodes('x:Lanes/x:Lane', $ns)) {
        $laneName[$l.GetAttribute('Id')] = $l.GetAttribute('Name')
        $lanePool[$l.GetAttribute('Id')] = $pool.GetAttribute('Name')
    }
}
$participant = @{}
foreach ($p in $x.SelectNodes('//x:Participant', $ns)) {
    $t = $p.SelectSingleNode('x:ParticipantType', $ns)
    $participant[$p.GetAttribute('Id')] = [ordered]@{
        id   = $p.GetAttribute('Id')
        name = $p.GetAttribute('Name')
        type = if ($t) { $t.GetAttribute('Type') } else { $null }
    }
}
$typeDecl = @{}
foreach ($t in $x.SelectNodes('//x:TypeDeclaration', $ns)) {
    $bt = $t.SelectSingleNode('x:BasicType', $ns)
    $typeDecl[$t.GetAttribute('Id')] = [ordered]@{
        id          = $t.GetAttribute('Id')
        name        = $t.GetAttribute('Name')
        basicType   = if ($bt) { $bt.GetAttribute('Type') } else { $null }
        precision   = if ($bt) { Txt $bt.SelectSingleNode('x:Precision', $ns).InnerText } else { $null }
        length      = if ($bt) { Txt $bt.SelectSingleNode('x:Length', $ns).InnerText } else { $null }
        description = Txt $t.SelectSingleNode('x:Description', $ns).InnerText
    }
}
$processById = @{}
foreach ($p in $x.SelectNodes('//x:WorkflowProcess', $ns)) {
    $processById[$p.GetAttribute('Id')] = [ordered]@{
        id   = $p.GetAttribute('Id')
        name = $p.GetAttribute('Name')
        displayName = $p.GetAttribute('DisplayName', $XPDEXT)
    }
}
# A chamada dinamica aponta para uma ProcessInterface, nao para um processo. Sem esta
# tabela o alvo fica por resolver e o grafo parte-se na fronteira do processo.
$processInterface = @{}
foreach ($pi in $x.SelectNodes('//e:ProcessInterface', $ns)) {
    $processInterface[$pi.GetAttribute('Id')] = [ordered]@{
        id             = $pi.GetAttribute('Id')
        name           = $pi.GetAttribute('Name')
        displayName    = $pi.GetAttribute('DisplayName', $XPDEXT)
        implementedBy  = @()
    }
}
foreach ($p in $x.SelectNodes('//x:WorkflowProcess', $ns)) {
    foreach ($ii in $p.SelectNodes('.//e:ImplementedInterface', $ns)) {
        $iid = $ii.GetAttribute('ProcessInterfaceId', $XPDEXT)
        if ($processInterface.ContainsKey($iid)) {
            $processInterface[$iid].implementedBy = @($processInterface[$iid].implementedBy) + @($p.GetAttribute('Name'))
        }
    }
}
$externalPackage = @{}
foreach ($ep in $x.SelectNodes('//x:ExternalPackage', $ns)) {
    $externalPackage[$ep.GetAttribute('Id')] = $ep.GetAttribute('href')
}

# ---------------------------------------------------------------- helpers
function Get-DataType($node) {
    $dt = $node.SelectSingleNode('x:DataType', $ns)
    if (-not $dt) { return $null }
    $b = $dt.SelectSingleNode('x:BasicType', $ns)
    if ($b) {
        return [ordered]@{
            kind      = 'basic'
            type      = $b.GetAttribute('Type')
            length    = Txt $b.SelectSingleNode('x:Length', $ns).InnerText
            precision = Txt $b.SelectSingleNode('x:Precision', $ns).InnerText
            scale     = Txt $b.SelectSingleNode('x:Scale', $ns).InnerText
        }
    }
    $d = $dt.SelectSingleNode('x:DeclaredType', $ns)
    if ($d) {
        $ref = $typeDecl[$d.GetAttribute('Id')]
        return [ordered]@{
            kind      = 'declared'
            typeRefId = $d.GetAttribute('Id')
            typeName  = if ($ref) { $ref.name } else { $null }
            type      = if ($ref) { $ref.basicType } else { $null }
            precision = if ($ref) { $ref.precision } else { $null }
            length    = if ($ref) { $ref.length } else { $null }
        }
    }
    return [ordered]@{ kind = 'unknown'; raw = ($dt.InnerXml -replace '\s+', ' ') }
}

function Get-Mappings($container) {
    $r = @()
    foreach ($m in $container.SelectNodes('.//x:DataMapping', $ns)) {
        $act = $m.SelectSingleNode('x:Actual', $ns)
        $r += [ordered]@{
            direction = $m.GetAttribute('Direction')
            formal    = $m.GetAttribute('Formal')
            actual    = if ($act) { Txt $act.InnerText } else { $null }
            grammar   = if ($act) { $act.GetAttribute('ScriptGrammar') } else { $null }
        }
    }
    , $r
}

function Get-NodeModel($a, $processId) {
    $id = $a.GetAttribute('Id')
    $node = [ordered]@{
        id          = $id
        name        = $a.GetAttribute('Name')
        displayName = (Txt $a.GetAttribute('DisplayName', $XPDEXT))
        kind        = $null
        lane        = $null
        pool        = $null
        description = Txt $a.SelectSingleNode('x:Description', $ns).InnerText
        stepIndex   = $a.GetAttribute('StepIndex', $IPEXT)
    }
    $g = $a.SelectSingleNode('x:NodeGraphicsInfos/x:NodeGraphicsInfo', $ns)
    if ($g) {
        $lid = $g.GetAttribute('LaneId')
        $node.lane = $laneName[$lid]
        $node.pool = $lanePool[$lid]
    }

    # ---- events
    $se = $a.SelectSingleNode('x:Event/x:StartEvent', $ns)
    if ($se) {
        $node.kind = 'startEvent'
        $node.trigger = $(if ($se.GetAttribute('Trigger')) { $se.GetAttribute('Trigger') } else { 'None' })
    }
    $ee = $a.SelectSingleNode('x:Event/x:EndEvent', $ns)
    if ($ee) {
        $node.kind = 'endEvent'
        $node.result = $(if ($ee.GetAttribute('Result')) { $ee.GetAttribute('Result') } else { 'None' })
    }
    $ie = $a.SelectSingleNode('x:Event/x:IntermediateEvent', $ns)
    if ($ie) {
        $trigger = $ie.GetAttribute('Trigger')
        $target = Txt $ie.GetAttribute('Target')
        $lnk = $ie.SelectSingleNode('x:TriggerResultLink', $ns)
        $sig = $ie.SelectSingleNode('x:TriggerResultSignal', $ns)
        $tim = $ie.SelectSingleNode('x:TriggerTimer', $ns)
        if ($lnk) {
            $node.kind = $(if ($lnk.GetAttribute('CatchThrow') -eq 'THROW') { 'linkThrow' } else { 'linkCatch' })
            $node.linkRef = $lnk.GetAttribute('Name')   # THROW: id of the catch node
        }
        elseif ($sig) {
            $node.kind = $(if ($sig.GetAttribute('CatchThrow') -eq 'THROW') { 'signalThrow' } else { 'signalCatch' })
            $node.signalName = $sig.GetAttribute('Name')
        }
        elseif ($trigger -eq 'Timer') {
            $node.kind = 'timerEvent'
        }
        else {
            $node.kind = "intermediateEvent:$trigger"
        }
        if ($target) {
            $node.boundary = $true
            $node.attachedTo = $target
            # ContinueOnTimeout=true => the host task keeps running (non-interrupting)
            $cot = if ($tim) { $tim.GetAttribute('ContinueOnTimeout', $XPDEXT) } else { $null }
            $node.interrupting = ($cot -ne 'true')
        }
        $dl = $a.SelectSingleNode('x:Deadline/x:DeadlineDuration', $ns)
        if ($dl) {
            $cp = $dl.SelectSingleNode('e:ConstantPeriod', $ns)
            $node.deadline = [ordered]@{
                grammar    = $dl.GetAttribute('ScriptGrammar')
                expression = Txt $dl.InnerText
                days       = if ($cp) { $cp.GetAttribute('Days') } else { $null }
                hours      = if ($cp) { $cp.GetAttribute('Hours') } else { $null }
                minutes    = if ($cp) { $cp.GetAttribute('Minutes') } else { $null }
            }
        }
    }

    # ---- gateway
    $r = $a.SelectSingleNode('x:Route', $ns)
    if ($r) {
        $node.kind = 'gateway'
        $gt = $r.GetAttribute('GatewayType')
        $node.gatewayType = $(if ($gt) { $gt } else { 'Exclusive' })
    }

    # ---- tasks
    $tu = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskUser', $ns)
    if ($tu) {
        $node.kind = 'userTask'
        $node.performers = @($a.SelectNodes('x:Performers/x:Performer', $ns) | ForEach-Object {
                $partId = Txt $_.InnerText
                if ($participant[$partId]) { $participant[$partId] } else { [ordered]@{ id = $partId; name = $null; type = 'UNRESOLVED' } }
            })
        $fi = $a.SelectSingleNode('.//e:FormImplementation', $ns)
        if ($fi) {
            $node.form = [ordered]@{
                formType = $fi.GetAttribute('FormType')
                uri      = $fi.GetAttribute('FormURI')
                external = ($fi.GetAttribute('FormType') -eq 'UserDefined')
            }
        }
        $inf = $a.SelectSingleNode('.//e:Information', $ns)
        if ($inf) { $node.assignmentScript = [ordered]@{ grammar = $inf.GetAttribute('ScriptGrammar'); expression = Txt $inf.InnerText } }
    }

    $tsv = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskService', $ns)
    if ($tsv) {
        $implType = $tsv.GetAttribute('ImplementationType', $XPDEXT)
        if ($implType -eq 'E-Mail') {
            $node.kind = 'emailTask'
            $mail = $tsv.SelectSingleNode('em:Email', $ns)
            if ($mail) {
                $node.email = [ordered]@{
                    to      = Txt $mail.SelectSingleNode('em:Definition/em:To', $ns).InnerText
                    cc      = Txt $mail.SelectSingleNode('em:Definition/em:Cc', $ns).InnerText
                    bcc     = Txt $mail.SelectSingleNode('em:Definition/em:Bcc', $ns).InnerText
                    subject = Txt $mail.SelectSingleNode('em:Definition/em:Subject', $ns).InnerText
                    body    = Txt $mail.SelectSingleNode('em:Body', $ns).InnerText
                    # %FIELD% placeholders are iProcess case-field substitutions
                    tokens  = @([regex]::Matches($mail.OuterXml, '%([A-Z0-9_]+)%') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
                }
            }
        }
        else {
            $node.kind = 'serviceTask'
            $wso = $a.SelectSingleNode('.//x:WebServiceOperation', $ns)
            if ($wso) {
                $svc = $wso.SelectSingleNode('x:Service', $ns)
                $ext = $wso.SelectSingleNode('x:Service/x:EndPoint/x:ExternalReference', $ns)
                $node.operation = [ordered]@{
                    operationName = $wso.GetAttribute('OperationName')
                    transport     = $wso.GetAttribute('Transport', $XPDEXT)
                    serviceName   = if ($svc) { $svc.GetAttribute('ServiceName') } else { $null }
                    portName      = if ($svc) { $svc.GetAttribute('PortName') } else { $null }
                    wsdl          = if ($ext) { $ext.GetAttribute('location') } else { $null }
                }
            }
            $node.inputMappings = Get-Mappings $a.SelectSingleNode('.//x:MessageIn', $ns)
            $node.outputMappings = Get-Mappings $a.SelectSingleNode('.//x:MessageOut', $ns)
        }
        $pids = @($a.SelectNodes('x:Performers/x:Performer', $ns) | ForEach-Object { Txt $_.InnerText })
        $node.systemParticipants = @($pids | ForEach-Object { if ($participant[$_]) { $participant[$_] } })
    }

    $tsc = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskScript/x:Script', $ns)
    if ($tsc) {
        $node.kind = 'scriptTask'
        $node.script = [ordered]@{
            grammar = $tsc.GetAttribute('ScriptGrammar')
            body    = Txt $tsc.InnerText
        }
    }

    $trc = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskReceive', $ns)
    if ($trc) {
        $node.kind = 'receiveTask'
        $node.instantiate = ($trc.GetAttribute('Instantiate') -eq 'true')
        $node.note = 'iProcess event/deferred step: resumes when an external party posts to this step.'
    }

    $sf = $a.SelectSingleNode('x:Implementation/x:SubFlow', $ns)
    if ($sf) {
        $node.kind = 'callActivity'
        $tid = $sf.GetAttribute('Id')
        $dyn = $sf.GetAttribute('ProcessIdentifierField', $XPDEXT)
        $iface = $processInterface[$tid]
        # Alvo dinamico resolve-se pela ProcessInterface que o processo declara implementar.
        $viaIface = @(if ($iface) { $iface.implementedBy })
        $node.call = [ordered]@{
            targetId              = $tid
            targetName            = if ($processById[$tid]) { $processById[$tid].name } elseif ($viaIface.Count -eq 1) { $viaIface[0] } else { $null }
            targetDisplayName     = if ($processById[$tid]) { $processById[$tid].displayName } else { $null }
            resolved              = [bool]($processById[$tid] -or $viaIface.Count -eq 1)
            resolvedVia           = if ($processById[$tid]) { 'process' } elseif ($viaIface.Count -ge 1) { 'processInterface' } else { $null }
            interfaceName         = if ($iface) { $iface.name } else { $null }
            interfaceImplementedBy = $viaIface
            processIdentifierField = Txt $dyn
            dynamic               = [bool](Txt $dyn)
        }
        $node.mappings = Get-Mappings $sf
        $dsp = $a.SelectSingleNode('.//ip:DynamicSubProcessTask', $ns)
        if ($dsp) {
            $node.call.isGraftStep = ($dsp.GetAttribute('IsGraftStep') -eq 'true')
            $node.call.haltOnBadSubProcess = ($dsp.GetAttribute('HaltOnBadSubProcess') -eq 'true')
        }
    }

    $ba = $a.SelectSingleNode('x:BlockActivity', $ns)
    if ($ba) {
        $node.kind = 'subProcessScope'
        $node.activitySetId = $ba.GetAttribute('ActivitySetId')
    }

    if (-not $node.kind) { $node.kind = 'unknown' }
    return $node
}

function Get-EdgeModel($t) {
    $c = $t.SelectSingleNode('x:Condition', $ns)
    $type = if ($c) { $c.GetAttribute('Type') } else { 'UNCONDITIONAL' }
    if (-not $type) { $type = 'UNCONDITIONAL' }
    [ordered]@{
        id         = $t.GetAttribute('Id')
        label      = Txt $t.GetAttribute('Name')
        from       = $t.GetAttribute('From')
        to         = $t.GetAttribute('To')
        conditionType = $type
        condition  = if ($c) { Txt $c.InnerText } else { $null }
        isDefault  = ($type -eq 'OTHERWISE')
    }
}

function Get-Container($container, $processId, $scopeName, $scopeId) {
    $nodes = @()
    foreach ($a in $container.SelectNodes('x:Activities/x:Activity', $ns)) { $nodes += Get-NodeModel $a $processId }
    $edges = @()
    foreach ($t in $container.SelectNodes('x:Transitions/x:Transition', $ns)) { $edges += Get-EdgeModel $t }
    [ordered]@{ scope = $scopeName; scopeId = $scopeId; nodes = $nodes; edges = $edges }
}

# ---------------------------------------------------------------- build
$processes = @()
foreach ($p in $x.SelectNodes('//x:WorkflowProcess', $ns)) {
    $procId = $p.GetAttribute('Id')

    $formals = @()
    foreach ($fp in $p.SelectNodes('x:FormalParameters/x:FormalParameter', $ns)) {
        $formals += [ordered]@{
            name = $fp.GetAttribute('Name')
            mode = $fp.GetAttribute('Mode')
            dataType = Get-DataType $fp
        }
    }
    $fields = @()
    foreach ($df in $p.SelectNodes('x:DataFields/x:DataField', $ns)) {
        $fields += [ordered]@{
            name = $df.GetAttribute('Name')
            isArray = ($df.GetAttribute('IsArray') -eq 'true')
            dataType = Get-DataType $df
        }
    }

    $scopes = @(Get-Container $p $procId 'MAIN' $null)
    foreach ($as in $p.SelectNodes('x:ActivitySets/x:ActivitySet', $ns)) {
        $scopes += Get-Container $as $procId ($(if ($as.GetAttribute('Name')) { $as.GetAttribute('Name') } else { 'ActivitySet' })) $as.GetAttribute('Id')
    }

    $processes += [ordered]@{
        id              = $procId
        name            = $p.GetAttribute('Name')
        displayName     = $p.GetAttribute('DisplayName', $XPDEXT)
        formalParameters = $formals
        dataFields      = $fields
        scopes          = $scopes
    }
}

# ------------------------------------------------- derived: link/signal wiring
$allNodes = @{}
foreach ($p in $processes) { foreach ($s in $p.scopes) { foreach ($n in $s.nodes) { $allNodes[$n.id] = @{ node = $n; process = $p.name; scope = $s.scope } } } }

$linkEdges = @()
foreach ($kv in $allNodes.GetEnumerator()) {
    $n = $kv.Value.node
    if ($n.kind -eq 'linkThrow' -and $n.linkRef -and $allNodes[$n.linkRef]) {
        $linkEdges += [ordered]@{
            kind = 'link'; name = $allNodes[$n.linkRef].node.displayName
            from = $n.id; fromLabel = $n.displayName
            to = $n.linkRef; toLabel = $allNodes[$n.linkRef].node.displayName
            process = $kv.Value.process
            note = 'Implicit GOTO. Must become an explicit transition in the .NET flow.'
        }
    }
}
$signalEdges = @()
$throws = $allNodes.Values | Where-Object { $_.node.kind -eq 'signalThrow' }
$catches = $allNodes.Values | Where-Object { $_.node.kind -eq 'signalCatch' }
foreach ($th in $throws) {
    foreach ($ca in $catches | Where-Object { $_.node.signalName -eq $th.node.signalName }) {
        $signalEdges += [ordered]@{
            kind = 'signal'; name = $th.node.signalName
            from = $th.node.id; fromLabel = $th.node.displayName
            to = $ca.node.id; toLabel = $ca.node.displayName
            catchIsBoundary = [bool]$ca.node.boundary
            attachedTo = $ca.node.attachedTo
            process = $th.process
            note = 'Broadcast cancellation/completion between parallel branches.'
        }
    }
}

# --------------------------------------------------- derived: invocation wiring
# O grafo de cada escopo termina no no de chamada. Sem estas arestas, o processo
# chamado parece independente e nenhuma jornada atravessa a fronteira.
$callEdges = @()
foreach ($p in $processes) {
    $activitySetScope = @{}
    foreach ($s in $p.scopes) { if ($s.scopeId) { $activitySetScope[$s.scopeId] = $s.scope } }
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) {
            if ($n.kind -eq 'callActivity') {
                $callEdges += [ordered]@{
                    kind = 'call'
                    fromProcess = $p.name; fromScope = $s.scope; fromNode = $n.id; fromLabel = $n.displayName
                    toProcess = $n.call.targetName; toScope = 'MAIN'
                    dynamic = $n.call.dynamic
                    resolved = $n.call.resolved
                    resolvedVia = $n.call.resolvedVia
                    isGraftStep = $n.call.isGraftStep
                    note = if ($n.call.resolved) { 'Chamada de processo: a jornada continua no processo alvo e regressa ao sucessor deste no.' } else { 'Alvo fora do pacote entregue - a jornada corta aqui e continua num duble.' }
                }
            }
            if ($n.kind -eq 'subProcessScope' -and $n.activitySetId) {
                $callEdges += [ordered]@{
                    kind = 'activitySet'
                    fromProcess = $p.name; fromScope = $s.scope; fromNode = $n.id; fromLabel = $n.displayName
                    toProcess = $p.name; toScope = $activitySetScope[$n.activitySetId]
                    dynamic = $false
                    resolved = [bool]$activitySetScope[$n.activitySetId]
                    resolvedVia = 'activitySet'
                    isGraftStep = $false
                    note = 'Escopo embutido: a jornada desce para o ActivitySet e regressa ao sucessor deste no.'
                }
            }
        }
    }
}

# ------------------------------------------------- derived: migration hazards
$hazards = @()
foreach ($kv in $allNodes.GetEnumerator()) {
    $n = $kv.Value.node
    $proc = $kv.Value.process
    if ($n.kind -eq 'callActivity' -and $n.call.dynamic) {
        $hazards += [ordered]@{ severity = 'high'; category = 'dynamic-subprocess'; node = $n.displayName; nodeId = $n.id; process = $proc
            detail = "Callee resolved at runtime from case field '$($n.call.processIdentifierField)', so it cannot be resolved statically - this is the iProcess graft step / dynamic procedure. The target may well exist in this package; what is unknown is which one is chosen. Requires a process-name registry + late binding in .NET." }
    }
    # Only a STATIC call can be said to be missing; a dynamic one is unresolved by design.
    if ($n.kind -eq 'callActivity' -and -not $n.call.dynamic -and -not $n.call.resolved) {
        $hazards += [ordered]@{ severity = 'high'; category = 'graft-step'; node = $n.displayName; nodeId = $n.id; process = $proc
            detail = 'Static call whose target process is not in this package (external package never delivered). Parent waits for children that attach themselves later - correlation join with runtime cardinality.' }
    }
    if ($n.kind -in 'linkThrow', 'linkCatch') {
        $hazards += [ordered]@{ severity = 'medium'; category = 'link-goto'; node = $n.displayName; nodeId = $n.id; process = $proc
            detail = 'XPDL Link event used as cross-lane GOTO; flatten to explicit edge.' }
    }
    if ($n.boundary -and $n.interrupting -eq $false) {
        $hazards += [ordered]@{ severity = 'medium'; category = 'non-interrupting-boundary'; node = $n.displayName; nodeId = $n.id; process = $proc
            detail = "Non-interrupting boundary event on '$($n.attachedTo)' - host task keeps running while a side branch fires." }
    }
    if ($n.kind -eq 'receiveTask') {
        $hazards += [ordered]@{ severity = 'medium'; category = 'external-event'; node = $n.displayName; nodeId = $n.id; process = $proc
            detail = 'iProcess deferred/event step. Needs an explicit correlation key + inbound API in .NET.' }
    }
    if ($n.kind -eq 'scriptTask' -and $n.script.body -match 'IPESystemValues|IPEStringUtil|IPEDateTimeUtil') {
        $fns = @([regex]::Matches($n.script.body, '(IPESystemValues\.[A-Z_]+|IPEStringUtil\.[A-Z]+|IPEDateTimeUtil\.[A-Z]+)') | ForEach-Object { $_.Value } | Sort-Object -Unique)
        $hazards += [ordered]@{ severity = 'high'; category = 'iprocess-builtin'; node = $n.displayName; nodeId = $n.id; process = $proc
            detail = "Uses iProcess runtime built-ins: $($fns -join ', '). Requires a compatibility shim."; symbols = $fns }
    }
    if ($n.deadline -and $n.deadline.grammar -eq 'JavaScript') {
        $hazards += [ordered]@{ severity = 'medium'; category = 'expression-deadline'; node = $n.displayName; nodeId = $n.id; process = $proc
            detail = "Deadline is a script expression ('$($n.deadline.expression)') combining a DATE and a TIME field, not a duration." }
    }
}

$model = [ordered]@{
    '$schema'   = 'sefaz-sp/tibco-intermediate/process-model/v1'
    source      = [ordered]@{
        package     = $x.DocumentElement.GetAttribute('Name')
        xpdlVersion = Txt $x.SelectSingleNode('//x:XPDLVersion', $ns).InnerText
        vendor      = Txt $x.SelectSingleNode('//x:Vendor', $ns).InnerText
        created     = Txt $x.SelectSingleNode('//x:PackageHeader/x:Created', $ns).InnerText
        language    = $x.SelectSingleNode('//x:PackageHeader', $ns).GetAttribute('Language', $XPDEXT)
        file        = (Split-Path $XpdlPath -Leaf)
    }
    externalPackages = $externalPackage
    participants = @($participant.Values)
    typeDeclarations = @($typeDecl.Values)
    processInterfaces = @($processInterface.Values)
    processes   = $processes
    derived     = [ordered]@{
        linkEdges     = $linkEdges
        signalEdges   = $signalEdges
        callEdges     = $callEdges
        migrationHazards = $hazards
    }
    statistics  = [ordered]@{
        processCount = $processes.Count
        nodeCount    = $allNodes.Count
        hazardCount  = $hazards.Count
    }
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutPath) | Out-Null
$model | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath $OutPath -Encoding UTF8
Write-Host "Wrote $OutPath  ($($processes.Count) processes, $($allNodes.Count) nodes, $($hazards.Count) hazards)"
