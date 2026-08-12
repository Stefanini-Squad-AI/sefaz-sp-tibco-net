<#
.SYNOPSIS
    Catalogues the ASP.NET WebForms screens that TIBCO user tasks hand off to.

.DESCRIPTION
    Two of the nine user tasks do not render a TIBCO form at all: the XPDL points at
    a page of the production ePAT application (FormType="UserDefined"). Everything
    those steps actually do - what the operator sees, which case fields are read and
    written, how the task is completed - lives in that ASP.NET code, and nothing in
    the pipeline was reading it.

    The valuable part is not the markup. Most of the UI is inside .ascx user controls
    that were never delivered, and much is built at runtime. The valuable part is the
    WORK ITEM CONTRACT, which the code-behind states explicitly:

        iProcess.lockWorkItem(wrkI, lstWrkItemLockField)     // reads named case fields
        new WorkItemKeepField("SITUACAOCARREGA","swText","A")   // save without completing
        new WorkItemReleaseField("CORRECAO","swNumeric","0")    // complete, with default

    That is the real interface between the process and the screen, and it is what a
    .NET port has to reproduce. It also exposes a four-state lifecycle - lock, keep,
    release, undo - that is not a plain "complete the task" call.

    Fields named here that are absent from the case dictionary are reported rather
    than merged: they come from external packages that were never delivered, so
    inventing them would hide a real source gap.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$TelasRoot,
    [Parameter(Mandatory)][string]$ModelPath,
    [Parameter(Mandatory)][string]$FieldsPath,
    [Parameter(Mandatory)][string]$OutPath
)

$ErrorActionPreference = 'Stop'

$model  = Get-Content -LiteralPath $ModelPath  -Raw -Encoding UTF8 | ConvertFrom-Json
$fields = Get-Content -LiteralPath $FieldsPath -Raw -Encoding UTF8 | ConvertFrom-Json

$knownFields = @{}
foreach ($f in $fields.fields) { $knownFields[$f.name] = $true }
$knownTechnical = @{}
foreach ($t in @($fields.technicalFields)) { if ($t) { $knownTechnical[$t.name] = $true } }

# ------------------------------------------------ user tasks that open a page ----

$externalTasks = [System.Collections.Generic.List[object]]::new()
foreach ($p in $model.processes) {
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) {
            if ($n.kind -ne 'userTask' -or -not $n.form) { continue }
            if (-not $n.form.external) { continue }
            $externalTasks.Add([pscustomobject]@{
                Process = $p.name; ProcessId = $p.id; Scope = $s.scope
                Node = $n.name; NodeId = $n.id; Uri = $n.form.uri
                Leaf = ($n.form.uri -split '[/\\]')[-1]
            })
        }
    }
}

# ------------------------------------------------------------------ helpers ----

function Get-WorkItemFields {
    param([string]$Code, [string]$Kind)
    $out = [System.Collections.Generic.List[object]]::new()
    $byName = @{}
    # The third argument is often a runtime expression (retorno[0]), not a literal, so
    # the argument list is captured whole and split - a literal-only pattern silently
    # drops those declarations.
    foreach ($m in [regex]::Matches($Code, "new\s+WorkItem${Kind}Field\(((?:[^()]|\([^()]*\))*)\)")) {
        $args = @()
        $depth = 0; $cur = ''
        foreach ($ch in $m.Groups[1].Value.ToCharArray()) {
            if ($ch -eq '(') { $depth++ }
            elseif ($ch -eq ')') { $depth-- }
            if ($ch -eq ',' -and $depth -eq 0) { $args += $cur.Trim(); $cur = '' } else { $cur += $ch }
        }
        if ($cur.Trim()) { $args += $cur.Trim() }
        if ($args.Count -eq 0) { continue }

        $name = $args[0].Trim('"')
        if (-not $name -or $args[0] -notmatch '^"') { continue }

        if (-not $byName.ContainsKey($name)) {
            $origin =
                if ($knownFields.ContainsKey($name)) { 'case-field' }
                elseif ($knownTechnical.ContainsKey($name)) { 'technical-envelope' }
                else { 'undeclared' }
            $byName[$name] = [ordered]@{ field = $name; origin = $origin }
            $out.Add($byName[$name])
        }
        $rec = $byName[$name]
        if ($args.Count -ge 2 -and $args[1] -match '^"(.*)"$') { $rec.swType = $Matches[1] }
        if ($args.Count -ge 3) {
            if ($args[2] -match '^"(.*)"$') { $rec.default = $Matches[1] }
            else { $rec.defaultExpression = $args[2] }
        }
    }
    # The leading comma stops PowerShell collapsing an empty result to $null, which
    # would serialise as JSON null and break any consumer that iterates it.
    return ,@($out | Sort-Object -Property field)
}

function Get-Attr {
    param([string]$Tag, [string]$Name)
    $m = [regex]::Match($Tag, "$Name\s*=\s*""([^""]*)""", 'IgnoreCase')
    if ($m.Success) { return $m.Groups[1].Value }
    return $null
}

# ------------------------------------------------------------------- scan ----

$screens = [System.Collections.Generic.List[object]]::new()
$missingControls = [System.Collections.Generic.List[string]]::new()
$undeclared = [System.Collections.Generic.List[string]]::new()

foreach ($aspx in (Get-ChildItem -LiteralPath $TelasRoot -Recurse -Filter *.aspx -ErrorAction SilentlyContinue | Sort-Object FullName)) {
    $markup = Get-Content -LiteralPath $aspx.FullName -Raw
    $cbPath = "$($aspx.FullName).cs"
    $code = if (Test-Path -LiteralPath $cbPath) { Get-Content -LiteralPath $cbPath -Raw } else { '' }

    $pageDirective = [regex]::Match($markup, '<%@\s*Page\b[^%]*%>').Value

    # A .ascx that is registered but absent is a delivery gap, not a modelling gap.
    $controls = [System.Collections.Generic.List[object]]::new()
    foreach ($m in [regex]::Matches($markup, 'Register\s+Src="([^"]+)"\s+TagName="([^"]+)"')) {
        $src = $m.Groups[1].Value
        $resolved = Join-Path $aspx.DirectoryName $src.Replace('/', '\')
        $exists = Test-Path -LiteralPath $resolved
        if (-not $exists -and -not $missingControls.Contains($src)) { $missingControls.Add($src) }
        $controls.Add([ordered]@{ tagName = $m.Groups[2].Value; src = $src; delivered = $exists })
    }

    $inputs = [System.Collections.Generic.List[object]]::new()
    foreach ($m in [regex]::Matches($markup, '<asp:(TextBox|DropDownList|CheckBox|RadioButtonList|FileUpload|GridView|Label)\b[^>]*>')) {
        $inputs.Add([ordered]@{ control = $m.Groups[1].Value; id = (Get-Attr $m.Value 'ID') })
    }

    $actions = [System.Collections.Generic.List[object]]::new()
    foreach ($m in [regex]::Matches($markup, '<asp:Button\b[^>]*>')) {
        $id = Get-Attr $m.Value 'ID'
        if (-not $id) { continue }
        $actions.Add([ordered]@{
            id = $id; text = (Get-Attr $m.Value 'Text'); onClick = (Get-Attr $m.Value 'OnClick')
            handlerFound = [bool]($code -and $code -match "(?m)\b(?:protected|public|private)[^\r\n]*\b${id}_Click\b")
        })
    }

    $api = @{}
    foreach ($m in [regex]::Matches($code, 'iProcess\.(\w+)\s*\(')) {
        $api[$m.Groups[1].Value] = 1 + [int]$api[$m.Groups[1].Value]
    }

    $lock    = Get-WorkItemFields -Code $code -Kind 'Lock'
    $keep    = Get-WorkItemFields -Code $code -Kind 'Keep'
    $release = Get-WorkItemFields -Code $code -Kind 'Release'
    foreach ($set in @($lock, $keep, $release)) {
        foreach ($f in $set) { if ($f.origin -eq 'undeclared' -and -not $undeclared.Contains($f.field)) { $undeclared.Add($f.field) } }
    }

    # The XPDL may point at a *Redirect.aspx wrapper that was not delivered, so an
    # exact match is preferred and a prefix match is recorded as weaker evidence.
    $links = [System.Collections.Generic.List[object]]::new()
    foreach ($t in $externalTasks) {
        $match =
            if ($t.Leaf -ieq $aspx.Name) { 'exact' }
            elseif ($t.Leaf -ilike "$($aspx.BaseName)*.aspx") { 'prefix' }
            else { $null }
        if (-not $match) { continue }
        $links.Add([ordered]@{
            process = $t.Process; node = $t.Node; nodeId = $t.NodeId
            formUri = $t.Uri; match = $match
        })
    }

    $screens.Add([ordered]@{
        file       = ([IO.Path]::GetRelativePath($TelasRoot, $aspx.FullName).Replace('\', '/'))
        codeBehind = $(if ($code) { ([IO.Path]::GetRelativePath($TelasRoot, $cbPath).Replace('\', '/')) } else { $null })
        inherits   = (Get-Attr $pageDirective 'Inherits')
        masterPage = (Get-Attr $pageDirective 'MasterPageFile')
        validateRequest = (Get-Attr $pageDirective 'ValidateRequest')
        codeBehindLines = $(if ($code) { ($code -split "`n").Count } else { 0 })
        linkedFrom = @($links)
        workItemContract = [ordered]@{
            lock = $lock; keep = $keep; release = $release
            api  = [ordered]@{}
        }
        actions  = @($actions)
        controls = @($controls)
        inputs   = @($inputs)
    })
    foreach ($k in ($api.Keys | Sort-Object)) { $screens[-1].workItemContract.api[$k] = $api[$k] }
}

# --------------------------------------------------------------- unlinked ----

$linkedLeaves = @($screens | ForEach-Object { $_.linkedFrom } | ForEach-Object { $_.nodeId })
$tasksWithoutScreen = @($externalTasks | Where-Object { $_.NodeId -notin $linkedLeaves } |
    ForEach-Object { [ordered]@{ process = $_.Process; node = $_.Node; nodeId = $_.NodeId; formUri = $_.Uri } })

$doc = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/screen-catalogue/v1'
    note      = 'ASP.NET WebForms pages of the production ePAT application, opened by TIBCO user tasks with FormType="UserDefined". They are NOT POC artifacts. The reviewable content is workItemContract: lock reads case fields, keep saves without completing, release completes. Fields marked origin=undeclared are not in the case dictionary and most likely come from an external package that was never delivered.'
    statistics = [ordered]@{
        screenCount            = $screens.Count
        externalUserTaskCount  = $externalTasks.Count
        linkedScreenCount      = @($screens | Where-Object { $_.linkedFrom.Count -gt 0 }).Count
        undeclaredFieldCount   = $undeclared.Count
        missingControlCount    = $missingControls.Count
    }
    undeclaredFields   = @($undeclared | Sort-Object)
    missingControls    = @($missingControls | Sort-Object)
    userTasksWithoutScreen = @($tasksWithoutScreen)
    screens            = @($screens)
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutPath) | Out-Null
$doc | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} telas, {2} ligadas a userTask, {3} campos nao declarados, {4} controles ausentes)" -f `
    $OutPath, $screens.Count, $doc.statistics.linkedScreenCount, $undeclared.Count, $missingControls.Count)
