#requires -version 5
<#
  Generates artifacts/case-field-dictionary.json

  iProcess keeps process state in a flat bag of scalar "case fields" (there is no
  BOM in this package). This script unifies every FormalParameter / DataField
  across all 9 processes, infers a .NET type, and records how each field is
  actually used (scripts, gateway conditions, service mappings, e-mail tokens,
  sub-process mappings, forms). It also flags the SW_NA three-valued-logic
  sentinel, which does NOT map cleanly onto C# null.
#>
param(
    [string]$ModelPath = "$PSScriptRoot\..\artifacts\process-model.json",
    [string]$FormsRoot = "$PSScriptRoot\..\input\Arquivos Poc Camunda\POC_Camunda\POC_Epat\Forms",
    [string]$OutPath = "$PSScriptRoot\..\artifacts\case-field-dictionary.json"
)
$ErrorActionPreference = 'Stop'
$model = Get-Content -LiteralPath $ModelPath -Raw | ConvertFrom-Json

function Get-ClrType($dt) {
    if (-not $dt) { return @{ clr = 'string'; note = 'no datatype declared' } }
    $t = $dt.type
    switch ($t) {
        'STRING' {
            $len = if ($dt.length) { [int]$dt.length } else { $null }
            return @{ clr = 'string'; maxLength = $len; note = $(if ($len) { "iProcess fixed width $len" } else { 'unbounded' }) }
        }
        'INTEGER' {
            $p = if ($dt.precision) { [int]$dt.precision } else { 10 }
            $clr = if ($p -le 9) { 'int' } else { 'long' }
            return @{ clr = $clr; precision = $p; note = "iProcess numeric precision $p" }
        }
        'FLOAT' {
            $p = if ($dt.precision) { [int]$dt.precision } else { 15 }
            $s = if ($dt.scale) { [int]$dt.scale } else { 0 }
            return @{ clr = 'decimal'; precision = $p; scale = $s; note = "monetary/decimal ($p,$s) - do NOT use double" }
        }
        'BOOLEAN' { return @{ clr = 'bool'; note = $null } }
        'DATE' { return @{ clr = 'DateOnly'; note = 'iProcess swDate - date component only' } }
        'TIME' { return @{ clr = 'TimeOnly'; note = 'iProcess swTime - time component only' } }
        'DATETIME' { return @{ clr = 'DateTime'; note = 'iProcess swDateTime' } }
        'PERFORMER' { return @{ clr = 'string'; note = 'participant / user or role identifier (swUser)'; semantic = 'principal' } }
        default { return @{ clr = 'string'; note = "unmapped XPDL type '$t'" } }
    }
}

# ---------------------------------------------------------------- collect
$fields = @{}   # name -> record

function Ensure-Field([string]$name) {
    if (-not $fields.ContainsKey($name)) {
        $fields[$name] = [ordered]@{
            name          = $name
            fullName      = $null
            nameTruncated = $false
            labelSuggestion = $null
            labelConflictsWith = $null
            clrType       = $null
            clrNullable   = $false
            xpdlType      = $null
            declaredType  = $null
            maxLength     = $null
            precision     = $null
            scale         = $null
            typeNote      = $null
            declaredIn    = @()
            modes         = @()
            isArray       = $false
            usesSwNaSentinel = $false
            readBy        = @()
            writtenBy     = @()
            usedInConditions = @()
            boundToService = @()
            boundToSubProcess = @()
            usedInEmail   = @()
            usedInForm    = @()
        }
    }
    $fields[$name]
}

foreach ($p in $model.processes) {
    foreach ($fp in $p.formalParameters) {
        $f = Ensure-Field $fp.name
        $f.declaredIn += [ordered]@{ process = $p.name; kind = 'formalParameter'; mode = $fp.mode }
        if ($fp.mode -and $f.modes -notcontains $fp.mode) { $f.modes += $fp.mode }
        if (-not $f.xpdlType) {
            $ct = Get-ClrType $fp.dataType
            $f.xpdlType = $fp.dataType.type
            $f.declaredType = $fp.dataType.typeName
            $f.clrType = $ct.clr; $f.maxLength = $ct.maxLength; $f.precision = $ct.precision; $f.scale = $ct.scale; $f.typeNote = $ct.note
        }
    }
    foreach ($df in $p.dataFields) {
        $f = Ensure-Field $df.name
        $f.declaredIn += [ordered]@{ process = $p.name; kind = 'dataField'; mode = 'LOCAL' }
        if ($df.isArray) { $f.isArray = $true }
        if (-not $f.xpdlType) {
            $ct = Get-ClrType $df.dataType
            $f.xpdlType = $df.dataType.type
            $f.declaredType = $df.dataType.typeName
            $f.clrType = $ct.clr; $f.maxLength = $ct.maxLength; $f.precision = $ct.precision; $f.scale = $ct.scale; $f.typeNote = $ct.note
        }
    }
}

$knownNames = [System.Collections.Generic.HashSet[string]]::new([string[]]@($fields.Keys))

# ---------------------------------------------------------------- usage scan
foreach ($p in $model.processes) {
    foreach ($s in $p.scopes) {

        foreach ($n in $s.nodes) {
            $where = [ordered]@{ process = $p.name; scope = $s.scope; node = $(if ($n.displayName) { $n.displayName } else { $n.name }); nodeId = $n.id }

            # --- scripts: assignment target = write, everything else = read
            if ($n.kind -eq 'scriptTask' -and $n.script.body) {
                $body = $n.script.body
                if ($body -match 'SW_NA') {
                    foreach ($m in [regex]::Matches($body, '([A-Z0-9_]{2,})\s*(?:!=|==)\s*IPESystemValues\.SW_NA')) {
                        $nm = $m.Groups[1].Value
                        if ($knownNames.Contains($nm)) { $fields[$nm].usesSwNaSentinel = $true }
                    }
                    foreach ($m in [regex]::Matches($body, 'IPESystemValues\.SW_NA\s*(?:!=|==)\s*([A-Z0-9_]{2,})')) {
                        $nm = $m.Groups[1].Value
                        if ($knownNames.Contains($nm)) { $fields[$nm].usesSwNaSentinel = $true }
                    }
                    foreach ($m in [regex]::Matches($body, '([A-Z0-9_]{2,})\s*=\s*IPESystemValues\.SW_NA')) {
                        $nm = $m.Groups[1].Value
                        if ($knownNames.Contains($nm)) { $fields[$nm].usesSwNaSentinel = $true }
                    }
                }
                $written = @([regex]::Matches($body, '(?m)^\s*([A-Z0-9_]{2,})\s*(?:\[[^\]]*\])?\s*=(?!=)') | ForEach-Object { $_.Groups[1].Value })
                $arrayed = @([regex]::Matches($body, '([A-Z0-9_]{2,})\s*\[') | ForEach-Object { $_.Groups[1].Value })
                foreach ($nm in ($written | Sort-Object -Unique)) {
                    if ($knownNames.Contains($nm)) { $fields[$nm].writtenBy += $where }
                }
                foreach ($nm in ($arrayed | Sort-Object -Unique)) {
                    if ($knownNames.Contains($nm)) { $fields[$nm].isArray = $true }
                }
                foreach ($nm in ([regex]::Matches($body, '\b([A-Z0-9_]{2,})\b') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)) {
                    if ($knownNames.Contains($nm) -and $written -notcontains $nm) { $fields[$nm].readBy += $where }
                }
            }

            # --- service task mappings
            if ($n.kind -eq 'serviceTask') {
                foreach ($m in $n.inputMappings) {
                    if ($m.actual -and $knownNames.Contains($m.actual)) {
                        $fields[$m.actual].boundToService += [ordered]@{ operation = $n.operation.operationName; wsdl = $n.operation.wsdl; direction = 'IN'; formal = $m.formal; node = $where.node; process = $p.name }
                    }
                }
                foreach ($m in $n.outputMappings) {
                    if ($m.actual -and $knownNames.Contains($m.actual)) {
                        $fields[$m.actual].boundToService += [ordered]@{ operation = $n.operation.operationName; wsdl = $n.operation.wsdl; direction = 'OUT'; formal = $m.formal; node = $where.node; process = $p.name }
                    }
                }
            }

            # --- sub-process mappings
            if ($n.kind -eq 'callActivity') {
                foreach ($m in $n.mappings) {
                    if ($m.actual -and $knownNames.Contains($m.actual)) {
                        $fields[$m.actual].boundToSubProcess += [ordered]@{ target = $(if ($n.call.targetName) { $n.call.targetName } else { $n.call.processIdentifierField }); direction = $m.direction; formal = $m.formal; node = $where.node; process = $p.name }
                    }
                }
                if ($n.call.processIdentifierField -and $knownNames.Contains($n.call.processIdentifierField)) {
                    $fields[$n.call.processIdentifierField].readBy += $where
                    $fields[$n.call.processIdentifierField].semanticRole = 'dynamic sub-process selector'
                }
            }

            # --- e-mail tokens
            if ($n.kind -eq 'emailTask' -and $n.email.tokens) {
                foreach ($tk in $n.email.tokens) {
                    if ($knownNames.Contains($tk)) { $fields[$tk].usedInEmail += [ordered]@{ node = $where.node; process = $p.name } }
                }
            }

            # --- deadlines
            if ($n.deadline -and $n.deadline.expression) {
                foreach ($nm in ([regex]::Matches($n.deadline.expression, '\b([A-Z0-9_]{2,})\b') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)) {
                    if ($knownNames.Contains($nm)) {
                        $fields[$nm].readBy += $where
                        $fields[$nm].semanticRole = 'deadline component'
                    }
                }
            }
        }

        # --- gateway conditions
        foreach ($ed in $s.edges) {
            if (-not $ed.condition) { continue }
            if ($ed.condition -match 'SW_NA') {
                foreach ($m in [regex]::Matches($ed.condition, '([A-Z0-9_]{2,})\s*(?:!=|==)\s*IPESystemValues\.SW_NA')) {
                    $nm = $m.Groups[1].Value
                    if ($knownNames.Contains($nm)) { $fields[$nm].usesSwNaSentinel = $true }
                }
            }
            foreach ($nm in ([regex]::Matches($ed.condition, '\b([A-Z0-9_]{2,})\b') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)) {
                if ($knownNames.Contains($nm)) {
                    $fields[$nm].usedInConditions += [ordered]@{ process = $p.name; label = $ed.label; expression = $ed.condition }
                }
            }
        }
    }
}

# --- form declarations
# The .form files are the only place the technical envelope is actually declared
# with a type, so they resolve identifiers that appear nowhere in the XPDL.
# .data.json is only preview data and is ignored.
$BomToClr = @{
    'Text' = 'string'; 'Integer' = 'int'; 'Boolean' = 'bool'; 'Date' = 'DateOnly'
    'Time' = 'TimeOnly'; 'DateTime' = 'DateTime'; 'Decimal' = 'decimal'
    'Fixed Point Number' = 'decimal'; 'URI' = 'string'; 'Duration' = 'TimeSpan'
}
$technical = @{}
$typeDisagreements = [System.Collections.Generic.List[object]]::new()

# iProcess caps case field names at 15 characters, so a name sitting exactly at the
# cap is very likely cut. The form label keeps the original, and when the name is a
# prefix of it the recovery is mechanical - no interpretation involved.
function Get-Normalized([string]$s) { return ($s -replace '[^A-Za-z0-9]', '').ToUpper() }


if (Test-Path -LiteralPath $FormsRoot) {
    foreach ($ff in Get-ChildItem -Recurse -LiteralPath $FormsRoot -Filter *.form) {
        $formName = $ff.BaseName
        $ownerProc = $ff.Directory.Parent.Name
        [xml]$formXml = Get-Content -LiteralPath $ff.FullName -Raw
        foreach ($p in $formXml.SelectNodes('//*[local-name()="parameter"]')) {
            $nm = ($p.name -replace '^data\.', '')
            if (-not $nm) { continue }
            $bom = ($p.type -replace '^BomPrimitiveTypes::', '')
            $len = $(if ($p.length) { [int]$p.length } else { $null })
            $site = [ordered]@{
                form = $formName; process = $ownerProc; label = $p.label
                declaredType = $bom; inout = $p.inout; maxLength = $len
            }

            if ($knownNames.Contains($nm)) {
                $fields[$nm].usedInForm += $site
                if ($p.label) {
                    $nl = Get-Normalized $p.label
                    $nn = Get-Normalized $nm
                    if ($nl -ne $nn -and $nl.StartsWith($nn)) {
                        $fields[$nm].fullName = $p.label
                        $fields[$nm].nameTruncated = ($nm.Length -eq 15)
                    }
                    elseif ($nl -ne $nn) {
                        $fields[$nm].labelSuggestion = $p.label
                    }
                }
                $formClr = $BomToClr[$bom]
                if ($formClr -and $fields[$nm].clrType -and $formClr -ne $fields[$nm].clrType) {
                    $typeDisagreements.Add([ordered]@{
                        field = $nm; fromXpdl = $fields[$nm].clrType; fromForm = $formClr
                        xpdlPrecision = $fields[$nm].precision; form = "$ownerProc/$formName"
                    })
                }
                continue
            }

            if (-not $technical.ContainsKey($nm)) {
                $technical[$nm] = [ordered]@{
                    name = $nm; clrType = $($BomToClr[$bom] ?? 'string'); declaredType = $bom
                    maxLength = $len; inout = $p.inout; label = $p.label
                    isEngineVariable = [bool]($nm -like 'SW_*')
                    declaredIn = @(); note = $null
                }
            }
            $technical[$nm].declaredIn += "$ownerProc/$formName"
        }
    }
}

# ---------------------------------------------------------------- finalize

# A label that spells out a DIFFERENT field's name would silently mislabel this one
# if it were ever accepted, so it is marked rather than offered.
$normToName = @{}
foreach ($n in $fields.Keys) { $normToName[(Get-Normalized $n)] = $n }
foreach ($n in $fields.Keys) {
    $sug = $fields[$n].labelSuggestion
    if (-not $sug) { continue }
    $hit = $normToName[(Get-Normalized $sug)]
    if ($hit -and $hit -ne $n) { $fields[$n].labelConflictsWith = $hit }
}

$out = @()
foreach ($name in ($fields.Keys | Sort-Object)) {
    $f = $fields[$name]
    # nullability: SW_NA sentinel or an OUT-only parameter => value may be absent
    $f.clrNullable = [bool]($f.usesSwNaSentinel -or ($f.modes -contains 'OUT'))
    if ($f.usesSwNaSentinel) {
        $f.sentinelNote = "Compared against IPESystemValues.SW_NA. iProcess SW_NA is a distinct 'not available' value, NOT null and NOT empty string. Port as an explicit Optional<T>/HasValue wrapper or a well-known sentinel constant - a plain C# null will silently change branch behaviour."
    }
    if ($f.isArray) {
        $f.arrayNote = 'Indexed in script code. iProcess array case fields are fixed-width, 1-based and frequently backed by a pipe-delimited string. Verify cardinality before choosing List<T> vs string split.'
    }
    $f.readBy = @($f.readBy | Sort-Object -Property nodeId -Unique)
    $f.writtenBy = @($f.writtenBy | Sort-Object -Property nodeId -Unique)
    $f.usedInForm = @($f.usedInForm | Sort-Object -Property { "$($_.process)/$($_.form)" } -Unique)
    $f.usedInEmail = @($f.usedInEmail | Sort-Object -Property node -Unique)
    $out += $f
}

$techOut = @()
foreach ($name in ($technical.Keys | Sort-Object)) {
    $t = $technical[$name]
    $t.declaredIn = @($t.declaredIn | Sort-Object -Unique)
    $t.note = if ($t.isEngineVariable) {
        'iProcess engine variable, supplied by the runtime. In .NET it must come from the workflow execution context, not from the case data.'
    }
    elseif ($t.inout -eq 'IN') {
        'Read-only for the process: supplied by the service envelope or engine. Reproduce as an input of the step, never as persisted case data.'
    }
    else {
        'Mutated by the process or by the user on the form. Needs a real home in the .NET execution state.'
    }
    $techOut += $t
}

$doc = [ordered]@{
    '$schema'   = 'sefaz-sp/tibco-intermediate/case-field-dictionary/v1'
    note        = 'Derived from XPDL FormalParameters + DataFields. The package contains no BOM, so these flat case fields ARE the domain model. Group them into aggregates before generating C#.'
    typeMapping = [ordered]@{
        'STRING(n)'  = 'string (MaxLength n)'
        'INTEGER(p)' = 'int when p<=9, otherwise long'
        'FLOAT(p,s)' = 'decimal - monetary values, never double'
        'BOOLEAN'    = 'bool'
        'DATE'       = 'DateOnly'
        'TIME'       = 'TimeOnly'
        'DATETIME'   = 'DateTime'
        'PERFORMER'  = 'string (user/role principal identifier)'
    }
    statistics  = [ordered]@{
        fieldCount        = $out.Count
        swNaSentinelCount = @($out | Where-Object { $_.usesSwNaSentinel }).Count
        arrayFieldCount   = @($out | Where-Object { $_.isArray }).Count
        serviceBoundCount = @($out | Where-Object { $_.boundToService.Count -gt 0 }).Count
        formDeclaredCount = @($out | Where-Object { $_.usedInForm.Count -gt 0 }).Count
        fullNameRecovered = @($out | Where-Object { $_.fullName }).Count
        labelSuggestionCount = @($out | Where-Object { $_.labelSuggestion }).Count
        labelConflictCount = @($out | Where-Object { $_.labelConflictsWith }).Count
        technicalFieldCount = $techOut.Count
        typeDisagreementCount = $typeDisagreements.Count
        unusedCount       = @($out | Where-Object { $_.readBy.Count -eq 0 -and $_.writtenBy.Count -eq 0 -and $_.boundToService.Count -eq 0 -and $_.boundToSubProcess.Count -eq 0 -and $_.usedInConditions.Count -eq 0 -and $_.usedInForm.Count -eq 0 -and $_.usedInEmail.Count -eq 0 }).Count
    }
    technicalNote = 'Declared in the TIBCO .form files but NOT among the case fields. These are the service envelope and iProcess engine variables. They steer branching, so the .NET model must expose them - but they are deliberately kept out of fields[] because they are not part of the business domain.'
    technicalFields = $techOut
    typeDisagreements = @($typeDisagreements)
    fields      = $out
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutPath) | Out-Null
$doc | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $OutPath -Encoding UTF8
Write-Host "Wrote $OutPath  ($($out.Count) fields; $($doc.statistics.swNaSentinelCount) use SW_NA; $($doc.statistics.arrayFieldCount) arrays; $($doc.statistics.unusedCount) unreferenced)"
