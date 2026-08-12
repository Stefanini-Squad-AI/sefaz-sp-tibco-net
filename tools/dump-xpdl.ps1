param(
    [string]$Path = "c:\Users\e_rfdbarssoles\Documents\PoCs\SEFAZ-SP\input\Arquivos Poc Camunda\POC_Camunda\POC_Epat\Process Packages\POC_Epat.xpdl",
    [string]$ProcessName = ""
)

[xml]$x = Get-Content $Path -Raw
$ns = New-Object System.Xml.XmlNamespaceManager($x.NameTable)
$ns.AddNamespace('x', 'http://www.wfmc.org/2008/XPDL2.1')
$ns.AddNamespace('e', 'http://www.tibco.com/XPD/xpdExtension1.0.0')
$XPDEXT = 'http://www.tibco.com/XPD/xpdExtension1.0.0'

function Get-ActivityKind {
    param($a, $ns)
    $parts = @()
    if ($a.SelectSingleNode('x:Event/x:StartEvent', $ns)) {
        $se = $a.SelectSingleNode('x:Event/x:StartEvent', $ns)
        $parts += "START[$($se.Trigger)]"
        $tm = $a.SelectSingleNode('x:Event/x:StartEvent/x:TriggerResultMessage', $ns)
        if ($tm) { $parts += "msg:$($tm.GetAttribute('Name'))" }
    }
    if ($a.SelectSingleNode('x:Event/x:EndEvent', $ns)) {
        $ee = $a.SelectSingleNode('x:Event/x:EndEvent', $ns)
        $parts += "END[$($ee.Result)]"
    }
    if ($a.SelectSingleNode('x:Event/x:IntermediateEvent', $ns)) {
        $ie = $a.SelectSingleNode('x:Event/x:IntermediateEvent', $ns)
        $parts += "INTERMEDIATE[$($ie.Trigger)]"
    }
    if ($a.SelectSingleNode('x:Route', $ns)) {
        $r = $a.SelectSingleNode('x:Route', $ns)
        $gw = $r.GetAttribute('GatewayType')
        if (-not $gw) { $gw = 'Exclusive' }
        $parts += "GATEWAY[$gw]"
    }
    if ($a.SelectSingleNode('x:Implementation/x:Task/x:TaskUser', $ns)) { $parts += 'USER_TASK' }
    if ($a.SelectSingleNode('x:Implementation/x:Task/x:TaskService', $ns)) { $parts += 'SERVICE_TASK' }
    if ($a.SelectSingleNode('x:Implementation/x:Task/x:TaskScript', $ns)) { $parts += 'SCRIPT_TASK' }
    if ($a.SelectSingleNode('x:Implementation/x:Task/x:TaskManual', $ns)) { $parts += 'MANUAL_TASK' }
    if ($a.SelectSingleNode('x:Implementation/x:Task/x:TaskReceive', $ns)) { $parts += 'RECEIVE_TASK' }
    if ($a.SelectSingleNode('x:Implementation/x:Task/x:TaskSend', $ns)) { $parts += 'SEND_TASK' }
    if ($a.SelectSingleNode('x:Implementation/x:SubFlow', $ns)) {
        $sf = $a.SelectSingleNode('x:Implementation/x:SubFlow', $ns)
        $parts += "SUBFLOW->$($sf.GetAttribute('Id'))"
    }
    if ($a.SelectSingleNode('x:BlockActivity', $ns)) {
        $ba = $a.SelectSingleNode('x:BlockActivity', $ns)
        $parts += "BLOCK->$($ba.GetAttribute('ActivitySetId'))"
    }
    if ($parts.Count -eq 0) { $parts += 'NONE' }
    return ($parts -join ' ')
}

$procs = $x.SelectNodes('//x:WorkflowProcess', $ns)
foreach ($p in $procs) {
    $pname = $p.GetAttribute('Name')
    if ($ProcessName -and $pname -ne $ProcessName) { continue }
    $disp = $p.GetAttribute('DisplayName', $XPDEXT)
    Write-Output ""
    Write-Output "################################################################"
    Write-Output "# PROCESS: $disp  (Name=$pname)"
    Write-Output "################################################################"

    Write-Output "--- FORMAL PARAMETERS ---"
    foreach ($fp in $p.SelectNodes('x:FormalParameters/x:FormalParameter', $ns)) {
        $t = $fp.SelectSingleNode('x:DataType', $ns)
        $tt = if ($t) { $t.InnerXml -replace '\s+', ' ' } else { '' }
        Write-Output ("  [{0}] {1} : {2}" -f $fp.GetAttribute('Mode'), $fp.GetAttribute('Name'), $tt)
    }

    Write-Output "--- DATA FIELDS ---"
    foreach ($df in $p.SelectNodes('x:DataFields/x:DataField', $ns)) {
        $t = $df.SelectSingleNode('x:DataType', $ns)
        $tt = if ($t) { $t.InnerXml -replace '\s+', ' ' } else { '' }
        Write-Output ("  {0} : {1}" -f $df.GetAttribute('Name'), $tt)
    }

    Write-Output "--- ACTIVITIES ---"
    $actById = @{}
    foreach ($a in $p.SelectNodes('.//x:Activity', $ns)) {
        $actById[$a.GetAttribute('Id')] = $a
        $kind = Get-ActivityKind $a $ns
        $nm = $a.GetAttribute('Name')
        $dn = $a.GetAttribute('DisplayName', $XPDEXT)
        $lane = $a.SelectSingleNode('x:NodeGraphicsInfos/x:NodeGraphicsInfo', $ns)
        $laneId = if ($lane) { $lane.GetAttribute('LaneId') } else { '' }
        Write-Output ("  {0} | {1} | {2} | lane={3}" -f $a.GetAttribute('Id'), $dn, $kind, $laneId)
    }

    Write-Output "--- TRANSITIONS ---"
    foreach ($t in $p.SelectNodes('.//x:Transition', $ns)) {
        $from = $t.GetAttribute('From'); $to = $t.GetAttribute('To')
        $fn = if ($actById[$from]) { $actById[$from].GetAttribute('DisplayName', $XPDEXT) } else { $from }
        $tn = if ($actById[$to]) { $actById[$to].GetAttribute('DisplayName', $XPDEXT) } else { $to }
        $cond = $t.SelectSingleNode('x:Condition', $ns)
        $ctype = if ($cond) { $cond.GetAttribute('Type') } else { '' }
        $cexpr = if ($cond) { ($cond.InnerText -replace '\s+', ' ').Trim() } else { '' }
        $lbl = $t.GetAttribute('Name')
        Write-Output ("  {0}  --[{1}{2}]-->  {3}" -f $fn, $lbl, $(if ($cexpr) { " $ctype : $cexpr" } else { $ctype }), $tn)
    }
}
