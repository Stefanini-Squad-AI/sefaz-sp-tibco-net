param(
    [string]$Path = "c:\Users\e_rfdbarssoles\Documents\PoCs\SEFAZ-SP\input\Arquivos Poc Camunda\POC_Camunda\POC_Epat\Process Packages\POC_Epat.xpdl",
    [string]$ProcessName = "POC_EpatProcess"
)
[xml]$x = Get-Content $Path -Raw
$ns = New-Object System.Xml.XmlNamespaceManager($x.NameTable)
$ns.AddNamespace('x', 'http://www.wfmc.org/2008/XPDL2.1')
$ns.AddNamespace('e', 'http://www.tibco.com/XPD/xpdExtension1.0.0')
$XPDEXT = 'http://www.tibco.com/XPD/xpdExtension1.0.0'

$procNameById = @{}
foreach ($p in $x.SelectNodes('//x:WorkflowProcess', $ns)) { $procNameById[$p.GetAttribute('Id')] = "$($p.GetAttribute('DisplayName',$XPDEXT)) [$($p.GetAttribute('Name'))]" }

$proc = $x.SelectNodes('//x:WorkflowProcess', $ns) | Where-Object { $_.GetAttribute('Name') -eq $ProcessName }
Write-Output "##### ACTIVITY DETAILS for $ProcessName"
foreach ($a in $proc.SelectNodes('.//x:Activity', $ns)) {
    $dn = $a.GetAttribute('DisplayName', $XPDEXT); if (-not $dn) { $dn = $a.GetAttribute('Name') }
    $blocks = @()

    $sc = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskScript/x:Script', $ns)
    if ($sc) { $blocks += "SCRIPT (" + $sc.GetAttribute('ScriptGrammar') + "):`n" + $sc.InnerText }

    $sf = $a.SelectSingleNode('x:Implementation/x:SubFlow', $ns)
    if ($sf) {
        $tid = $sf.GetAttribute('Id')
        $tn = if ($procNameById[$tid]) { $procNameById[$tid] } else { "EXTERNAL/UNRESOLVED $tid" }
        $mode = $sf.GetAttribute('Execution')
        $blocks += "SUBFLOW -> $tn (Execution=$mode)"
        $aps = $sf.SelectNodes('x:ActualParameters/x:ActualParameter', $ns) | ForEach-Object { $_.InnerText }
        if ($aps) { $blocks += "  params: " + ($aps -join ', ') }
    }

    $ts = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskService', $ns)
    if ($ts) {
        $msgIn = $ts.SelectSingleNode('x:MessageIn', $ns)
        $msgOut = $ts.SelectSingleNode('x:MessageOut', $ns)
        $info = @()
        if ($msgIn) { $info += "in:" + $msgIn.GetAttribute('Name') }
        if ($msgOut) { $info += "out:" + $msgOut.GetAttribute('Name') }
        $blocks += "SERVICE TASK " + ($info -join ' ')
        $ea = $a.SelectNodes('.//x:ExtendedAttribute', $ns)
        foreach ($z in $ea) { $blocks += "   ext:" + $z.GetAttribute('Name') + " = " + (($z.GetAttribute('Value') + $z.InnerXml) -replace '\s+', ' ').Substring(0, [Math]::Min(400, (($z.GetAttribute('Value') + $z.InnerXml) -replace '\s+', ' ').Length)) }
    }

    $tu = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskUser', $ns)
    if ($tu) {
        $perf = $a.SelectNodes('x:Performers/x:Performer', $ns) | ForEach-Object { $_.InnerText }
        $blocks += "USER TASK; performers: " + ($perf -join ', ')
        $ea = $a.SelectNodes('.//e:*', $ns) | Where-Object { $_.LocalName -match 'Form|Interface|Participant' }
        foreach ($z in $ea) { $blocks += "   " + $z.LocalName + ": " + (($z.OuterXml) -replace '\s+', ' ').Substring(0, [Math]::Min(300, (($z.OuterXml) -replace '\s+', ' ').Length)) }
    }

    $tr = $a.SelectSingleNode('x:Implementation/x:Task/x:TaskReceive', $ns)
    if ($tr) {
        $m = $tr.SelectSingleNode('x:Message', $ns)
        $blocks += "RECEIVE TASK; message=" + $(if ($m) { $m.GetAttribute('Name') } else { '' }) + " Instantiate=" + $tr.GetAttribute('Instantiate')
    }

    $dm = $a.SelectNodes('x:DataMappings/x:DataMapping', $ns)
    if ($dm.Count -gt 0) {
        $mm = $dm | ForEach-Object { "    [$($_.GetAttribute('Direction'))] $($_.GetAttribute('Formal')) <-> $((($_.SelectSingleNode('x:Actual',$ns)).InnerText -replace '\s+',' '))" }
        $blocks += "DATA MAPPINGS:`n" + ($mm -join "`n")
    }

    if ($blocks.Count -gt 0) {
        Write-Output ""
        Write-Output "=== $dn <$($a.GetAttribute('Id'))>"
        $blocks | ForEach-Object { Write-Output $_ }
    }
}
