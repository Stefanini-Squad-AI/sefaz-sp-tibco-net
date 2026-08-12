<#
.SYNOPSIS
    S4 - emits BPMN 2.0 for analyst review in Camunda Modeler.

.DESCRIPTION
    One .bpmn file per scope. These are SPECIFICATION artifacts: isExecutable="false",
    never deployed, never executed. Elsa is the runtime (decision D2); both outputs are
    generated from the same process-model.json, so they cannot drift apart.

    Design rules that make the output reviewable rather than merely valid:

    * Branch conditions are rendered on the SEQUENCE FLOW, not on the gateway. A diamond
      labelled with an expression tells the reader nothing about which way is which.
    * Expressions are humanised through config/glossary/<pkg>.yaml when a term exists,
      and left verbatim when it does not - never paraphrased on a guess.
    * Every element carries a bpmn:documentation block with its original TIBCO id and
      kind, so any diagram element can be traced back to the XPDL.
    * Processes sharing a control-flow skeleton are detected and cross-referenced in
      their documentation, so a repeated template is reviewed once instead of five times.
      They are NOT merged: the five service templates each wrap a different operation,
      and merging would hide exactly what distinguishes them.

    Layout is generated (layered left-to-right). It is deliberately plain - the analyst
    is expected to rearrange in the Modeler; the point is that the file opens and reads.
#>
[CmdletBinding()]
param(
    [string]$ModelPath    = "$PSScriptRoot/../artifacts/POC_Epat/process-model.json",
    [string]$GlossaryPath = "$PSScriptRoot/../config/glossary/POC_Epat.yaml",
    [string]$OutDir       = "$PSScriptRoot/../artifacts/POC_Epat/bpmn"
)

$ErrorActionPreference = 'Stop'

$NS = @'
xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
'@.Trim()

# Layout constants. Column pitch must exceed task width or edges overlap shapes.
$ColPitch = 200; $RowPitch = 130; $OriginX = 60; $OriginY = 60

$SizeOf = @{
    gateway = @{ w = 50;  h = 50  }
    task    = @{ w = 110; h = 80  }
    event   = @{ w = 36;  h = 36  }
}
$TaskKinds  = @('userTask', 'serviceTask', 'emailTask', 'scriptTask', 'receiveTask', 'callActivity', 'subProcessScope')

# ------------------------------------------------------------------- load ----

if (-not (Test-Path $ModelPath)) { throw "process-model.json not found: $ModelPath" }
$model = Get-Content $ModelPath -Raw -Encoding UTF8 | ConvertFrom-Json

# Glossary is optional: an unseeded run must still produce openable diagrams.
$glossary = @{}
if (Test-Path $GlossaryPath) {
    $section = ''; $entry = ''
    foreach ($line in (Get-Content $GlossaryPath -Encoding UTF8)) {
        if ($line -match '^([A-Za-z_][A-Za-z0-9_]*):\s*$') { $section = $Matches[1]; $entry = ''; continue }
        if ($line -match '^  ([^\s#][^:]*):\s*$')          { $entry = $Matches[1].Trim('"'); continue }
        if ($line -match '^    ([A-Za-z_]+):\s*(.+?)\s*$' -and $section -and $entry) {
            $val = $Matches[2].Trim('"').Trim("'")
            if ($val) { $glossary["$section|$entry|$($Matches[1])"] = $val }
        }
    }
}
function Get-Glossary {
    param([string]$Section, [string]$Entry, [string]$Prop)
    $k = "$Section|$Entry|$Prop"
    if ($glossary.ContainsKey($k)) { return $glossary[$k] }
    return $null
}

# ---------------------------------------------------------------- helpers ----

function ConvertTo-XmlText {
    param([string]$Text)
    if ($null -eq $Text) { return '' }
    return $Text.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;').Replace('"', '&quot;')
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

# BPMN ids are NCName: letter or underscore first, then letters/digits/._-
function Get-BpmnId {
    param([string]$Raw)
    if ([string]::IsNullOrWhiteSpace($Raw)) { return 'id_' + [guid]::NewGuid().ToString('N').Substring(0, 8) }
    $clean = [regex]::Replace($Raw, '[^A-Za-z0-9_.-]', '_')
    if ($clean -notmatch '^[A-Za-z_]') { $clean = "n_$clean" }
    return $clean
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

# Um rotulo respondido pelo analista nao pode continuar a avisar que e sugestao.
function Test-AnsweredDecision {
    param($Node, [string]$ProcessName)
    $short = if ($Node.id.Length -gt 10) { $Node.id.Substring(0, 10) } else { $Node.id }
    return [bool](Get-Glossary 'decisions' "$ProcessName/$short" 'question')
}

function Get-NodeLabel {
    param($Node, [string]$ProcessName, $Edges)
    $label = $Node.displayName
    if ([string]::IsNullOrWhiteSpace($label)) { $label = $Node.name }
    if (-not [string]::IsNullOrWhiteSpace($label)) { return $label }

    # Unlabelled decision points may have been answered by the analyst in the glossary.
    $short = if ($Node.id.Length -gt 10) { $Node.id.Substring(0, 10) } else { $Node.id }
    $q = Get-Glossary 'decisions' "$ProcessName/$short" 'question'
    if ($q) { return $q }
    if ($SuggestedGatewayLabels.ContainsKey($Node.id)) { return $SuggestedGatewayLabels[$Node.id] }

    switch ($Node.kind) {
        'startEvent' { return 'Inicio' }
        'endEvent'   { return 'Fim' }
        'timerEvent' { return Get-TimerLabel $Node }
        'gateway' {
            $incoming = @($Edges | Where-Object { $_.to -eq $Node.id }).Count
            $outgoing = @($Edges | Where-Object { $_.from -eq $Node.id }).Count
            if ($incoming -gt 1 -and $outgoing -le 1) { return 'Convergencia' }
            return '(sem rotulo)'
        }
    }
    return ''
}

function Test-UnresolvedDecision {
    param($Node, $Edges)
    if ($Node.kind -ne 'gateway') { return $false }
    if (-not [string]::IsNullOrWhiteSpace($Node.displayName) -or -not [string]::IsNullOrWhiteSpace($Node.name)) { return $false }
    return @($Edges | Where-Object { $_.from -eq $Node.id }).Count -gt 1 -and
        $Node.gatewayType -ne 'Parallel'
}

# Renders an iProcess expression as something a business reader can check.
# Substitutions are applied only where the glossary supplies a term; anything
# unmapped survives verbatim, so the reader can tell evidence from translation.
function Format-Condition {
    param([string]$Expression)
    if ([string]::IsNullOrWhiteSpace($Expression)) { return '' }

    $t = $Expression.Trim().TrimEnd(';').Trim()
    $t = $t.Replace('IPESystemValues.SW_NA', "«nao informado»")
    $t = $t.Replace('IPESystemValues.SW_QRETRYCOUNT', "«tentativas do motor»")

    # Longest names first so PRAZORECEBIMENT is not clipped by PRAZO.
    foreach ($key in ($glossary.Keys | Where-Object { $_ -like 'fields|*|term' } |
                      Sort-Object { $_.Split('|')[1].Length } -Descending)) {
        $fieldName = $key.Split('|')[1]
        $term      = $glossary[$key]
        $t = [regex]::Replace($t, "\b$([regex]::Escape($fieldName))\b", $term)
    }

    $t = $t -replace '\s*==\s*', ' = '
    $t = $t -replace '\s*!=\s*', ' <> '
    $t = $t -replace '\s*&&\s*', ' e '
    $t = $t -replace '\s*\|\|\s*', ' ou '
    return $t.Trim()
}

# --------------------------------------------- control-flow clone detection ----

# Signature ignores which service is called and looks only at shape + conditions,
# which is what makes it useful: it finds templates that were copy-pasted.
$sigOf = @{}
foreach ($proc in $model.processes) {
    $kinds = [System.Collections.Generic.List[string]]::new()
    $conds = [System.Collections.Generic.List[string]]::new()
    foreach ($scope in $proc.scopes) {
        foreach ($n in $scope.nodes) { $kinds.Add($n.kind) }
        foreach ($e in $scope.edges) { if ($e.condition) { $conds.Add($e.condition.Trim()) } }
    }
    $shape = (($kinds | Group-Object | Sort-Object Name | ForEach-Object { "$($_.Name):$($_.Count)" }) -join ',')
    $sigOf[$proc.name] = "$shape || " + (($conds | Sort-Object) -join ' ; ')
}
$cluster = @{}
foreach ($name in $sigOf.Keys) {
    $sig = $sigOf[$name]
    if (-not $cluster.ContainsKey($sig)) { $cluster[$sig] = [System.Collections.Generic.List[string]]::new() }
    $cluster[$sig].Add($name)
}
$siblingsOf = @{}
foreach ($sig in $cluster.Keys) {
    $members = @($cluster[$sig] | Sort-Object)
    if ($members.Count -lt 2) { continue }
    foreach ($m in $members) { $siblingsOf[$m] = @($members | Where-Object { $_ -ne $m }) }
}

# Stable process ids so callActivity/calledElement resolves across files.
$procIdOf = @{}
foreach ($proc in $model.processes) { $procIdOf[$proc.name] = Get-BpmnId "P_$($proc.name)_MAIN" }
$procNameById = @{}
foreach ($proc in $model.processes) { $procNameById[$proc.id] = $proc.name }

# --------------------------------------------------------------- emission ----

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
Get-ChildItem -Path $OutDir -Filter '*.bpmn' -ErrorAction SilentlyContinue | Remove-Item -Force

$index    = [System.Collections.Generic.List[object]]::new()
$signals  = @{}

foreach ($proc in $model.processes) {
    foreach ($scope in $proc.scopes) {

        $isMain    = ($scope.scope -eq 'MAIN')
        $processId = if ($isMain) { $procIdOf[$proc.name] } else { Get-BpmnId "P_$($proc.name)_$($scope.scopeId)" }
        $nodes     = @($scope.nodes)
        $edges     = @($scope.edges)
        if ($nodes.Count -eq 0) { continue }

        $nodeById = @{}
        foreach ($n in $nodes) { $nodeById[$n.id] = $n }

        # Boundary events hang off a host and must carry no incoming flow.
        $isBoundary = @{}
        foreach ($n in $nodes) {
            if ($n.boundary -and $n.attachedTo -and $nodeById.ContainsKey($n.attachedTo)) { $isBoundary[$n.id] = $true }
        }
        $flowEdges = @($edges | Where-Object { -not $isBoundary.ContainsKey($_.to) })

        # ---- layered layout -------------------------------------------------
        $layer = @{}
        foreach ($n in $nodes) { $layer[$n.id] = 0 }
        for ($pass = 0; $pass -lt $nodes.Count; $pass++) {
            $changed = $false
            foreach ($e in $flowEdges) {
                if (-not $layer.ContainsKey($e.from) -or -not $layer.ContainsKey($e.to)) { continue }
                if ($layer[$e.to] -lt $layer[$e.from] + 1) { $layer[$e.to] = $layer[$e.from] + 1; $changed = $true }
            }
            if (-not $changed) { break }
        }

        $geo = @{}
        $placed = @($nodes | Where-Object { -not $isBoundary.ContainsKey($_.id) })
        foreach ($grp in ($placed | Group-Object { $layer[$_.id] } | Sort-Object { [int]$_.Name })) {
            $row = 0
            foreach ($n in ($grp.Group | Sort-Object { [int]($_.stepIndex ?? 0) }, { $_.id })) {
                $size = if ($n.kind -eq 'gateway') { $SizeOf.gateway }
                        elseif ($n.kind -in $TaskKinds) { $SizeOf.task }
                        else { $SizeOf.event }
                $cx = $OriginX + ([int]$grp.Name * $ColPitch) + ($SizeOf.task.w / 2)
                $cy = $OriginY + ($row * $RowPitch) + ($SizeOf.task.h / 2)
                $geo[$n.id] = @{
                    x = [int]($cx - $size.w / 2); y = [int]($cy - $size.h / 2)
                    w = $size.w; h = $size.h; cx = [int]$cx; cy = [int]$cy
                }
                $row++
            }
        }
        # Boundary events sit on the lower-right corner of their host.
        foreach ($n in $nodes) {
            if (-not $isBoundary.ContainsKey($n.id)) { continue }
            $hostGeo = $geo[$n.attachedTo]
            $s = $SizeOf.event
            $geo[$n.id] = @{
                x = $hostGeo.x + $hostGeo.w - 40; y = $hostGeo.y + $hostGeo.h - ($s.h / 2)
                w = $s.w; h = $s.h
                cx = $hostGeo.x + $hostGeo.w - 40 + ($s.w / 2); cy = $hostGeo.y + $hostGeo.h
            }
        }

        # ---- flows ----------------------------------------------------------
        $outOf = @{}; $inOf = @{}
        $flows = [System.Collections.Generic.List[object]]::new()
        foreach ($e in $flowEdges) {
            if (-not $nodeById.ContainsKey($e.from) -or -not $nodeById.ContainsKey($e.to)) { continue }
            $fid = Get-BpmnId "F_$($e.id)"
            $name = $e.label
            if ([string]::IsNullOrWhiteSpace($name)) { $name = Format-Condition $e.condition }
            if ([string]::IsNullOrWhiteSpace($name) -and $e.conditionType -eq 'OTHERWISE') { $name = 'caso contrario' }
            $flows.Add([pscustomobject]@{
                Id = $fid; From = $e.from; To = $e.to; Name = $name
                Condition = $e.condition; Type = $e.conditionType; IsDefault = ($e.conditionType -eq 'OTHERWISE')
            })
            if (-not $outOf.ContainsKey($e.from)) { $outOf[$e.from] = [System.Collections.Generic.List[string]]::new() }
            if (-not $inOf.ContainsKey($e.to))    { $inOf[$e.to]    = [System.Collections.Generic.List[string]]::new() }
            $outOf[$e.from].Add($fid); $inOf[$e.to].Add($fid)
        }
        $defaultOf = @{}
        foreach ($f in $flows) { if ($f.IsDefault -and -not $defaultOf.ContainsKey($f.From)) { $defaultOf[$f.From] = $f.Id } }

        # ---- elements -------------------------------------------------------
        $xml = [System.Collections.Generic.List[string]]::new()

        foreach ($n in ($nodes | Sort-Object { [int]($_.stepIndex ?? 0) }, { $_.id })) {
            $eid   = Get-BpmnId $n.id
            $label = ConvertTo-XmlText (Get-NodeLabel -Node $n -ProcessName $proc.name -Edges $edges)
            $attr  = "id=`"$eid`" name=`"$label`""

            $doc = [System.Collections.Generic.List[string]]::new()
            $doc.Add("TIBCO id: $($n.id)")
            $doc.Add("kind XPDL: $($n.kind)")
            if ($n.lane)   { $doc.Add("faixa: $($n.lane)") }
            if ($n.script) { $doc.Add("script: $($n.script)") }
            if ($n.operation)  { $doc.Add("operacao: $($n.operation.operationName)") }
            if ($n.deadline)   { $doc.Add("prazo: $($n.deadline | ConvertTo-Json -Compress -Depth 3)") }
            if ($n.description) { $doc.Add("descricao: $($n.description)") }
            $respondido = (Test-UnresolvedDecision -Node $n -Edges $edges) -and (Test-AnsweredDecision -Node $n -ProcessName $proc.name)
            if ($respondido) {
                $doc.Add('SEM ROTULO NO XPDL - rotulo RESPONDIDO pelo analista em config/glossary/<pacote>.yaml')
            }
            else {
                if ($SuggestedGatewayLabels.ContainsKey($n.id) -and $n.gatewayType -ne 'Parallel') {
                    $doc.Add('ROTULO SUGERIDO AUTOMATICAMENTE - requer revisao humana')
                }
                if (Test-UnresolvedDecision -Node $n -Edges $edges) {
                    $doc.Add('SEM ROTULO NO XPDL - sugestao pode ser substituida em config/glossary/<pacote>.yaml')
                }
            }
            $docXml = "      <bpmn:documentation>$(ConvertTo-XmlText ($doc -join ' | '))</bpmn:documentation>"

            $io = [System.Collections.Generic.List[string]]::new()
            if ($inOf.ContainsKey($n.id))  { foreach ($f in $inOf[$n.id])  { $io.Add("      <bpmn:incoming>$f</bpmn:incoming>") } }
            if ($outOf.ContainsKey($n.id)) { foreach ($f in $outOf[$n.id]) { $io.Add("      <bpmn:outgoing>$f</bpmn:outgoing>") } }
            $body = (@($docXml) + $io) -join "`r`n"

            if ($isBoundary.ContainsKey($n.id)) {
                $cancel = if ($n.interrupting -eq $false) { 'false' } else { 'true' }
                $defn = if ($n.kind -eq 'timerEvent') { '      <bpmn:timerEventDefinition />' }
                        else {
                            $sig = Get-BpmnId "Signal_$($n.signalName)"
                            $signals[$sig] = $n.signalName
                            "      <bpmn:signalEventDefinition signalRef=`"$sig`" />"
                        }
                $xml.Add("    <bpmn:boundaryEvent $attr attachedToRef=`"$(Get-BpmnId $n.attachedTo)`" cancelActivity=`"$cancel`">")
                $xml.Add($body); $xml.Add($defn); $xml.Add('    </bpmn:boundaryEvent>')
                continue
            }

            switch ($n.kind) {
                'startEvent' { $xml.Add("    <bpmn:startEvent $attr>"); $xml.Add($body); $xml.Add('    </bpmn:startEvent>') }
                'endEvent'   { $xml.Add("    <bpmn:endEvent $attr>");   $xml.Add($body); $xml.Add('    </bpmn:endEvent>') }
                'gateway' {
                    $tag = if ($n.gatewayType -eq 'Parallel') { 'bpmn:parallelGateway' } else { 'bpmn:exclusiveGateway' }
                    $def = if ($defaultOf.ContainsKey($n.id) -and $tag -eq 'bpmn:exclusiveGateway') { " default=`"$($defaultOf[$n.id])`"" } else { '' }
                    $xml.Add("    <$tag $attr$def>"); $xml.Add($body); $xml.Add("    </$tag>")
                }
                'userTask'    { $xml.Add("    <bpmn:userTask $attr>");    $xml.Add($body); $xml.Add('    </bpmn:userTask>') }
                'serviceTask' { $xml.Add("    <bpmn:serviceTask $attr>"); $xml.Add($body); $xml.Add('    </bpmn:serviceTask>') }
                'scriptTask'  { $xml.Add("    <bpmn:scriptTask $attr>");  $xml.Add($body); $xml.Add('    </bpmn:scriptTask>') }
                'emailTask'   { $xml.Add("    <bpmn:sendTask $attr>");    $xml.Add($body); $xml.Add('    </bpmn:sendTask>') }
                'receiveTask' { $xml.Add("    <bpmn:receiveTask $attr>"); $xml.Add($body); $xml.Add('    </bpmn:receiveTask>') }
                'callActivity' {
                    $target = $null
                    if ($n.call.targetName -and $procIdOf.ContainsKey($n.call.targetName)) { $target = $procIdOf[$n.call.targetName] }
                    $called = if ($target) { " calledElement=`"$target`"" } else { '' }
                    $xml.Add("    <bpmn:callActivity $attr$called>"); $xml.Add($body); $xml.Add('    </bpmn:callActivity>')
                }
                'subProcessScope' {
                    $sub = $proc.scopes | Where-Object { $_.scopeId -eq $n.activitySetId } | Select-Object -First 1
                    $called = if ($sub) { " calledElement=`"$(Get-BpmnId "P_$($proc.name)_$($sub.scopeId)")`"" } else { '' }
                    $xml.Add("    <bpmn:callActivity $attr$called>"); $xml.Add($body); $xml.Add('    </bpmn:callActivity>')
                }
                'timerEvent' {
                    $xml.Add("    <bpmn:intermediateCatchEvent $attr>"); $xml.Add($body)
                    $xml.Add('      <bpmn:timerEventDefinition />'); $xml.Add('    </bpmn:intermediateCatchEvent>')
                }
                'linkThrow' {
                    $xml.Add("    <bpmn:intermediateThrowEvent $attr>"); $xml.Add($body)
                    $xml.Add("      <bpmn:linkEventDefinition name=`"$(ConvertTo-XmlText $n.linkRef)`" />"); $xml.Add('    </bpmn:intermediateThrowEvent>')
                }
                'linkCatch' {
                    $xml.Add("    <bpmn:intermediateCatchEvent $attr>"); $xml.Add($body)
                    $xml.Add("      <bpmn:linkEventDefinition name=`"$(ConvertTo-XmlText $n.linkRef)`" />"); $xml.Add('    </bpmn:intermediateCatchEvent>')
                }
                'signalThrow' {
                    $sig = Get-BpmnId "Signal_$($n.signalName)"; $signals[$sig] = $n.signalName
                    $xml.Add("    <bpmn:intermediateThrowEvent $attr>"); $xml.Add($body)
                    $xml.Add("      <bpmn:signalEventDefinition signalRef=`"$sig`" />"); $xml.Add('    </bpmn:intermediateThrowEvent>')
                }
                'signalCatch' {
                    $sig = Get-BpmnId "Signal_$($n.signalName)"; $signals[$sig] = $n.signalName
                    $xml.Add("    <bpmn:intermediateCatchEvent $attr>"); $xml.Add($body)
                    $xml.Add("      <bpmn:signalEventDefinition signalRef=`"$sig`" />"); $xml.Add('    </bpmn:intermediateCatchEvent>')
                }
                default { $xml.Add("    <bpmn:task $attr>"); $xml.Add($body); $xml.Add('    </bpmn:task>') }
            }
        }

        foreach ($f in $flows) {
            $attr = "id=`"$($f.Id)`" name=`"$(ConvertTo-XmlText $f.Name)`" sourceRef=`"$(Get-BpmnId $f.From)`" targetRef=`"$(Get-BpmnId $f.To)`""
            if ($f.Condition -and -not $f.IsDefault) {
                $xml.Add("    <bpmn:sequenceFlow $attr>")
                $xml.Add("      <bpmn:conditionExpression xsi:type=`"bpmn:tFormalExpression`">$(ConvertTo-XmlText $f.Condition)</bpmn:conditionExpression>")
                $xml.Add('    </bpmn:sequenceFlow>')
            }
            else { $xml.Add("    <bpmn:sequenceFlow $attr />") }
        }

        # ---- diagram interchange -------------------------------------------
        $di = [System.Collections.Generic.List[string]]::new()
        foreach ($n in $nodes) {
            $g = $geo[$n.id]; if (-not $g) { continue }
            $eid = Get-BpmnId $n.id
            $di.Add("      <bpmndi:BPMNShape id=`"S_$eid`" bpmnElement=`"$eid`">")
            $di.Add("        <dc:Bounds x=`"$($g.x)`" y=`"$($g.y)`" width=`"$($g.w)`" height=`"$($g.h)`" />")
            $di.Add('      </bpmndi:BPMNShape>')
        }
        foreach ($f in $flows) {
            $a = $geo[$f.From]; $b = $geo[$f.To]
            if (-not $a -or -not $b) { continue }
            $x1 = $a.x + $a.w; $y1 = $a.cy; $x2 = $b.x; $y2 = $b.cy
            $di.Add("      <bpmndi:BPMNEdge id=`"E_$($f.Id)`" bpmnElement=`"$($f.Id)`">")
            $di.Add("        <di:waypoint x=`"$x1`" y=`"$y1`" />")
            if ($y1 -ne $y2) {
                $mid = [int](($x1 + $x2) / 2)
                if ($x2 -le $x1) { $mid = $x1 + 30 }   # backward edge: route outside the shapes
                $di.Add("        <di:waypoint x=`"$mid`" y=`"$y1`" />")
                $di.Add("        <di:waypoint x=`"$mid`" y=`"$y2`" />")
            }
            $di.Add("        <di:waypoint x=`"$x2`" y=`"$y2`" />")
            $di.Add('      </bpmndi:BPMNEdge>')
        }

        # ---- header ---------------------------------------------------------
        $header = [System.Collections.Generic.List[string]]::new()
        $header.Add("Processo TIBCO: $($proc.displayName ?? $proc.name)  |  escopo: $($scope.scope)")
        $header.Add("Gerado a partir de process-model.json. ARTEFATO DE ESPECIFICACAO - nao e executavel.")
        if ($siblingsOf.ContainsKey($proc.name)) {
            $header.Add("ESQUELETO COMPARTILHADO com: $($siblingsOf[$proc.name] -join ', '). " +
                        "Mesma estrutura de controle e mesmas condicoes; o que muda e a operacao invocada. " +
                        "Revise o esqueleto uma vez e depois apenas as diferencas.")
        }

        $procLabel = ConvertTo-XmlText ($proc.displayName ?? $proc.name)
        if ($scope.scope -ne 'MAIN') { $procLabel += " ($($scope.scope))" }

        $out = [System.Collections.Generic.List[string]]::new()
        $out.Add('<?xml version="1.0" encoding="UTF-8"?>')
        $out.Add("<bpmn:definitions $NS id=`"Defs_$processId`" targetNamespace=`"http://sefaz.sp.gov.br/epat`">")
        foreach ($sid in ($signals.Keys | Sort-Object)) {
            $out.Add("  <bpmn:signal id=`"$sid`" name=`"$(ConvertTo-XmlText $signals[$sid])`" />")
        }
        $out.Add("  <bpmn:process id=`"$processId`" name=`"$procLabel`" isExecutable=`"false`">")
        $out.Add("    <bpmn:documentation>$(ConvertTo-XmlText ($header -join ' '))</bpmn:documentation>")
        $out.AddRange($xml)
        $out.Add('  </bpmn:process>')
        $out.Add("  <bpmndi:BPMNDiagram id=`"D_$processId`">")
        $out.Add("    <bpmndi:BPMNPlane id=`"Plane_$processId`" bpmnElement=`"$processId`">")
        $out.AddRange($di)
        $out.Add('    </bpmndi:BPMNPlane>')
        $out.Add('  </bpmndi:BPMNDiagram>')
        $out.Add('</bpmn:definitions>')

        $fileName = "$(Get-BpmnId "$($proc.name)__$($scope.scope)").bpmn"
        $filePath = Join-Path $OutDir $fileName
        ($out -join "`r`n") | Set-Content -LiteralPath $filePath -Encoding UTF8

        $index.Add([ordered]@{
            file      = $fileName
            process   = $proc.name
            scope     = $scope.scope
            processId = $processId
            nodes     = $nodes.Count
            flows     = $flows.Count
            unlabelled = @($nodes | Where-Object { (Test-UnresolvedDecision -Node $_ -Edges $edges) -and -not (Test-AnsweredDecision -Node $_ -ProcessName $proc.name) }).Count
            answeredByAnalyst = @($nodes | Where-Object { (Test-UnresolvedDecision -Node $_ -Edges $edges) -and (Test-AnsweredDecision -Node $_ -ProcessName $proc.name) }).Count
            sharesSkeletonWith = @($siblingsOf[$proc.name])
        })
    }
}

# Signals are collected while walking the scopes, so the first files were written
# before the set was complete. Re-inject the full list into every file.
$signalBlock = ($signals.Keys | Sort-Object | ForEach-Object {
    "  <bpmn:signal id=`"$_`" name=`"$(ConvertTo-XmlText $signals[$_])`" />"
}) -join "`r`n"
foreach ($f in (Get-ChildItem -Path $OutDir -Filter '*.bpmn')) {
    $text = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8
    $text = [regex]::Replace($text, '(?s)(<bpmn:definitions[^>]*>\r?\n)(  <bpmn:signal[^\n]*\r?\n)*', "`$1$signalBlock`r`n")
    $text | Set-Content -LiteralPath $f.FullName -Encoding UTF8
}

$sumNodes = 0; $sumFlows = 0; $sumUnlabelled = 0; $sumAnswered = 0
foreach ($d in $index) { $sumNodes += $d.nodes; $sumFlows += $d.flows; $sumUnlabelled += $d.unlabelled; $sumAnswered += $d.answeredByAnalyst }

$indexDoc = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/bpmn-index/v1'
    package   = $model.source.package
    note      = 'BPMN 2.0 para revisao humana no Camunda Modeler. isExecutable=false: especificacao, nunca implantado. Um arquivo por escopo.'
    totals    = [ordered]@{
        files      = $index.Count
        nodes      = $sumNodes
        flows      = $sumFlows
        unlabelled = $sumUnlabelled
        answeredByAnalyst = $sumAnswered
    }
    diagrams = @($index)
}
$indexDoc | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $OutDir 'index.json') -Encoding UTF8

# ------------------------------------------------------------ self-check ----

# A BPMN file that is merely well-formed can still fail to open. Camunda Modeler
# rejects dangling sourceRef/targetRef/attachedToRef and silently drops shapes whose
# bpmnElement is missing, so the emitter verifies its own output before claiming success.
$emitted = Get-ChildItem -Path $OutDir -Filter '*.bpmn'
$knownProcessIds = @{}
$faults = [System.Collections.Generic.List[string]]::new()
$docs = @{}

foreach ($file in $emitted) {
    $doc = New-Object System.Xml.XmlDocument
    try { $doc.Load($file.FullName) }
    catch { $faults.Add("$($file.Name): XML mal formado - $($_.Exception.Message)"); continue }
    $docs[$file.Name] = $doc
    $mgr = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
    $mgr.AddNamespace('b', 'http://www.omg.org/spec/BPMN/20100524/MODEL')
    foreach ($p in $doc.SelectNodes('//b:process', $mgr)) { $knownProcessIds[$p.GetAttribute('id')] = $file.Name }
}

foreach ($name in ($docs.Keys | Sort-Object)) {
    $doc = $docs[$name]
    $mgr = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
    $mgr.AddNamespace('b',  'http://www.omg.org/spec/BPMN/20100524/MODEL')
    $mgr.AddNamespace('di', 'http://www.omg.org/spec/BPMN/20100524/DI')

    $ids = @{}
    foreach ($el in $doc.SelectNodes('//b:process/*', $mgr)) {
        $elId = $el.GetAttribute('id')
        if ($elId) { $ids[$elId] = $el.LocalName }
    }
    foreach ($fl in $doc.SelectNodes('//b:sequenceFlow', $mgr)) {
        foreach ($ref in 'sourceRef', 'targetRef') {
            $v = $fl.GetAttribute($ref)
            if (-not $ids.ContainsKey($v)) { $faults.Add("${name}: sequenceFlow $($fl.GetAttribute('id')) $ref -> '$v' inexistente") }
        }
    }
    foreach ($be in $doc.SelectNodes('//b:boundaryEvent', $mgr)) {
        $v = $be.GetAttribute('attachedToRef')
        if (-not $ids.ContainsKey($v)) { $faults.Add("${name}: boundaryEvent $($be.GetAttribute('id')) attachedToRef -> '$v' inexistente") }
    }
    foreach ($sh in $doc.SelectNodes('//di:BPMNShape | //di:BPMNEdge', $mgr)) {
        $v = $sh.GetAttribute('bpmnElement')
        if (-not $ids.ContainsKey($v)) { $faults.Add("${name}: DI $($sh.GetAttribute('id')) -> '$v' inexistente") }
    }
    foreach ($sd in $doc.SelectNodes('//b:signalEventDefinition', $mgr)) {
        $v = $sd.GetAttribute('signalRef')
        if (-not $doc.SelectSingleNode("//b:signal[@id='$v']", $mgr)) { $faults.Add("${name}: signalRef '$v' nao declarado") }
    }
    foreach ($ca in $doc.SelectNodes('//b:callActivity[@calledElement]', $mgr)) {
        $v = $ca.GetAttribute('calledElement')
        if (-not $knownProcessIds.ContainsKey($v)) { $faults.Add("${name}: callActivity -> processo '$v' inexistente") }
    }
}

if ($faults.Count -gt 0) {
    Write-Host ''
    Write-Host "FALHA: $($faults.Count) problema(s) de integridade no BPMN gerado" -ForegroundColor Red
    foreach ($f in ($faults | Select-Object -First 20)) { Write-Host "    $f" -ForegroundColor Red }
    exit 1
}

Write-Host ("Wrote {0}  ({1} diagramas, {2} nos, {3} fluxos, {4} decisoes rotuladas pelo analista, {5} ainda sem rotulo)" -f `
    $OutDir, $indexDoc.totals.files, $indexDoc.totals.nodes, $indexDoc.totals.flows, $indexDoc.totals.answeredByAnalyst, $indexDoc.totals.unlabelled)
Write-Host ("    auto-verificacao: {0} arquivos, {1} processos, todas as referencias resolvidas" -f `
    $emitted.Count, $knownProcessIds.Count) -ForegroundColor DarkGray
