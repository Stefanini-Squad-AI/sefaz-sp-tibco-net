param(
    [string]$Path = "c:\Users\e_rfdbarssoles\Documents\PoCs\SEFAZ-SP\input\Arquivos Poc Camunda\POC_Camunda\POC_Epat\Process Packages\POC_Epat.xpdl",
    [string]$ProcessName = ""
)

[xml]$x = Get-Content $Path -Raw
$ns = New-Object System.Xml.XmlNamespaceManager($x.NameTable)
$ns.AddNamespace('x', 'http://www.wfmc.org/2008/XPDL2.1')
$ns.AddNamespace('e', 'http://www.tibco.com/XPD/xpdExtension1.0.0')
$XPDEXT = 'http://www.tibco.com/XPD/xpdExtension1.0.0'

# short id -> readable token
function Short($id) { if ($id) { $id.Substring(0, [Math]::Min(8, $id.Length)) } else { '' } }

function Label($a) {
    if (-not $a) { return '???' }
    $dn = $a.GetAttribute('DisplayName', $XPDEXT)
    if (-not $dn) { $dn = $a.GetAttribute('Name') }
    if (-not $dn) { $dn = '(unnamed)' }
    return "$dn <$(Short $a.GetAttribute('Id'))>"
}

function Kind($a) {
    $p = @()
    $se = $a.SelectSingleNode('x:Event/x:StartEvent', $ns); if ($se) { $p += "START:$($se.GetAttribute('Trigger'))" }
    $ee = $a.SelectSingleNode('x:Event/x:EndEvent', $ns); if ($ee) { $p += "END:$($ee.GetAttribute('Result'))" }
    $ie = $a.SelectSingleNode('x:Event/x:IntermediateEvent', $ns)
    if ($ie) {
        $t = $ie.GetAttribute('Trigger')
        $tgt = $ie.GetAttribute('Target')
        $p += "INTERMEDIATE:$t"
        $lnk = $ie.SelectSingleNode('x:TriggerResultLink', $ns)
        if ($lnk) { $p += "link(name=$($lnk.GetAttribute('Name')),catchThrow=$($lnk.GetAttribute('CatchThrow')))" }
        $sig = $ie.SelectSingleNode('x:TriggerResultSignal', $ns)
        if ($sig) { $p += "signal(name=$($sig.GetAttribute('Name')),catchThrow=$($sig.GetAttribute('CatchThrow')))" }
        $tim = $ie.SelectSingleNode('x:TimerEvent', $ns)
        if ($tim) { $p += "timer(" + (($tim.InnerText -replace '\s+', ' ').Trim()) + ")" }
        if ($tgt) { $p += "attachedTo=$(Short $tgt)" }
    }
    $r = $a.SelectSingleNode('x:Route', $ns)
    if ($r) { $g = $r.GetAttribute('GatewayType'); if (-not $g) { $g = 'Exclusive' }; $p += "GATEWAY:$g" }
    foreach ($tk in 'TaskUser', 'TaskService', 'TaskScript', 'TaskManual', 'TaskReceive', 'TaskSend', 'TaskReference') {
        if ($a.SelectSingleNode("x:Implementation/x:Task/x:$tk", $ns)) { $p += $tk.ToUpper() }
    }
    $sf = $a.SelectSingleNode('x:Implementation/x:SubFlow', $ns)
    if ($sf) { $p += "SUBFLOW->$($sf.GetAttribute('Id'))" }
    $ba = $a.SelectSingleNode('x:BlockActivity', $ns)
    if ($ba) { $p += "BLOCK->$($ba.GetAttribute('ActivitySetId'))" }
    $lp = $a.SelectSingleNode('x:Loop', $ns)
    if ($lp) { $p += "LOOP:$($lp.GetAttribute('LoopType'))" }
    if (-not $p) { $p += 'PLAIN' }
    return ($p -join ' ')
}

$laneNames = @{}
foreach ($l in $x.SelectNodes('//x:Lane', $ns)) { $laneNames[$l.GetAttribute('Id')] = $l.GetAttribute('Name') }

$procs = $x.SelectNodes('//x:WorkflowProcess', $ns)
foreach ($p in $procs) {
    $pname = $p.GetAttribute('Name')
    if ($ProcessName -and $pname -ne $ProcessName) { continue }
    Write-Output ""
    Write-Output "################ PROCESS $($p.GetAttribute('DisplayName',$XPDEXT))  [Name=$pname Id=$(Short $p.GetAttribute('Id'))]"

    $actById = @{}
    foreach ($a in $p.SelectNodes('.//x:Activity', $ns)) { $actById[$a.GetAttribute('Id')] = $a }

    # containers: main flow + activity sets
    $containers = @(, @('MAIN', $p))
    foreach ($as in $p.SelectNodes('x:ActivitySets/x:ActivitySet', $ns)) {
        $containers += , @("ACTIVITYSET $($as.GetAttribute('Name')) <$(Short $as.GetAttribute('Id'))>", $as)
    }

    foreach ($c in $containers) {
        Write-Output "  ---- $($c[0]) ----"
        Write-Output "  * NODES:"
        foreach ($a in $c[1].SelectNodes('x:Activities/x:Activity', $ns)) {
            $g = $a.SelectSingleNode('x:NodeGraphicsInfos/x:NodeGraphicsInfo', $ns)
            $lane = if ($g) { $laneNames[$g.GetAttribute('LaneId')] } else { '' }
            Write-Output ("      {0,-52} {1}   [lane:{2}]" -f (Label $a), (Kind $a), $lane)
        }
        Write-Output "  * FLOWS:"
        foreach ($t in $c[1].SelectNodes('x:Transitions/x:Transition', $ns)) {
            $cond = $t.SelectSingleNode('x:Condition', $ns)
            $ct = if ($cond) { $cond.GetAttribute('Type') } else { '' }
            $cx = if ($cond) { ($cond.InnerText -replace '\s+', ' ').Trim() } else { '' }
            $lbl = $t.GetAttribute('Name')
            $tag = @($lbl, $ct, $cx) | Where-Object { $_ } 
            Write-Output ("      {0,-52} --[{1}]--> {2}" -f (Label $actById[$t.GetAttribute('From')]), ($tag -join ' | '), (Label $actById[$t.GetAttribute('To')]))
        }
    }
}
