<#
.SYNOPSIS
    Maps the POC design document onto the extracted model, stage by stage.

.DESCRIPTION
    The artifacts record what the process DOES. The Word document is the only source
    that records what each part is FOR - it states, per stage, which BPM concept the
    stage exists to prove and why that matters. That intent is exactly the context an
    analyst needs when answering an open question about a node, and until now it lived
    only in a .docx nobody in the pipeline read.

    The join is deliberately mechanical, never interpretive: a stage is linked to a
    process or node only when that element's own display name occurs verbatim in the
    stage text. Comparison is accent- and case-insensitive because the XPDL strips
    diacritics ("Notificacao do AIIM") while the document keeps them ("Notificação").
    Every link records the exact string that matched, so a reader can check it.

    Nothing here is authored. If the document says nothing about a node, the node
    simply gets no intent, rather than a plausible-sounding guess.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$DocPath,
    [Parameter(Mandatory)][string]$ModelPath,
    [Parameter(Mandatory)][string]$OutPath,
    [int]$MinMatchLength = 6
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$model = Get-Content -LiteralPath $ModelPath -Raw -Encoding UTF8 | ConvertFrom-Json

# ------------------------------------------------------------- read .docx ----

# Read into memory first: the .docx is often open in Word, and ZipFile.OpenRead
# takes an exclusive handle that would fail against a reader lock.
$stream = [IO.File]::Open((Resolve-Path -LiteralPath $DocPath), [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
try { $bytes = New-Object byte[] $stream.Length; [void]$stream.Read($bytes, 0, $bytes.Length) }
finally { $stream.Dispose() }

$zip = [IO.Compression.ZipArchive]::new([IO.MemoryStream]::new($bytes))
try {
    $entry = $zip.Entries | Where-Object { $_.FullName -eq 'word/document.xml' }
    if (-not $entry) { throw "word/document.xml not found inside $DocPath" }
    $reader = New-Object IO.StreamReader($entry.Open())
    $xml = $reader.ReadToEnd()
    $reader.Close()
}
finally { $zip.Dispose() }

$plain = [regex]::Replace($xml, '</w:p>', "`n")
$plain = [regex]::Replace($plain, '<[^>]+>', '')
$plain = [System.Net.WebUtility]::HtmlDecode($plain)
$lines = @($plain -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ })

function Get-Comparable {
    param([string]$Text)
    if (-not $Text) { return '' }
    $n = $Text.Normalize([Text.NormalizationForm]::FormD)
    $sb = New-Object Text.StringBuilder
    foreach ($ch in $n.ToCharArray()) {
        if ([Globalization.CharUnicodeInfo]::GetUnicodeCategory($ch) -ne [Globalization.UnicodeCategory]::NonSpacingMark) {
            [void]$sb.Append($ch)
        }
    }
    return ([regex]::Replace($sb.ToString(), '\s+', ' ')).ToLowerInvariant()
}

# ---------------------------------------------------------- split stages ----

$stages = [System.Collections.Generic.List[object]]::new()
$current = $null
foreach ($line in $lines) {
    $m = [regex]::Match($line, '^Etapa\s+(\d+)\s*[-\u2013\u2014]\s*(.+)$')
    if ($m.Success) {
        if ($current) { $stages.Add($current) }
        $current = [pscustomobject]@{
            Number = [int]$m.Groups[1].Value
            Title  = $m.Groups[2].Value.Trim()
            Body   = [System.Collections.Generic.List[string]]::new()
        }
        continue
    }
    # Everything after the last stage heading belongs to the closing sections.
    if ($current -and $line -notmatch '^\d+\.\s') { $current.Body.Add($line) }
    elseif ($current -and $line -match '^\d+\.\s') { $stages.Add($current); $current = $null }
}
if ($current) { $stages.Add($current) }

# ------------------------------------------------------------ index model ----

$elements = [System.Collections.Generic.List[object]]::new()
foreach ($p in $model.processes) {
    $label = $(if ($p.displayName) { $p.displayName } else { $p.name })
    $elements.Add([pscustomobject]@{ Kind = 'process'; Id = $p.id; Name = $p.name; Label = $label; Process = $p.name })
    foreach ($s in $p.scopes) {
        foreach ($n in $s.nodes) {
            $nl = $(if ($n.displayName) { $n.displayName } else { $n.name })
            if (-not $nl) { continue }
            $elements.Add([pscustomobject]@{ Kind = $n.kind; Id = $n.id; Name = $n.name; Label = $nl; Process = $p.name })
        }
    }
}

# A label shared by several elements ("Start Event" occurs 8 times) cannot identify
# which one the document meant, so it is never linked.
$labelCount = @{}
foreach ($el in $elements) {
    $k = Get-Comparable $el.Label
    if ($k) { $labelCount[$k] = 1 + [int]$labelCount[$k] }
}

# ------------------------------------------------------------------ match ----

$stageDocs = [System.Collections.Generic.List[object]]::new()
$intentByElement = @{}

foreach ($st in $stages) {
    $bodyText = ($st.Body -join ' ')
    $cmp = Get-Comparable "$($st.Title) $bodyText"

    # "Conceitos BPM Validados" is a flat list that runs to the end of the stage.
    $concepts = [System.Collections.Generic.List[string]]::new()
    $collect = $false
    foreach ($l in $st.Body) {
        if ((Get-Comparable $l) -eq 'conceitos bpm validados') { $collect = $true; continue }
        if ($collect) { $concepts.Add($l) }
    }

    $matches = [System.Collections.Generic.List[object]]::new()
    $ambiguous = [System.Collections.Generic.List[string]]::new()
    $seen = @{}
    foreach ($el in $elements) {
        if ($el.Label.Length -lt $MinMatchLength) { continue }
        $needle = Get-Comparable $el.Label
        if (-not $needle -or $cmp -notlike "*$needle*") { continue }
        if ($labelCount[$needle] -gt 1) {
            if (-not $ambiguous.Contains($el.Label)) { $ambiguous.Add($el.Label) }
            continue
        }
        $key = "$($el.Kind)|$($el.Id)"
        if ($seen.ContainsKey($key)) { continue }
        $seen[$key] = $true
        $matches.Add([ordered]@{
            kind = $el.Kind; id = $el.Id; name = $el.Name
            label = $el.Label; process = $el.Process
            matchedOn = $el.Label
        })
        if (-not $intentByElement.ContainsKey($el.Id)) {
            $intentByElement[$el.Id] = [ordered]@{
                stage = $st.Number; title = $st.Title
                concepts = @($concepts); matchedOn = $el.Label
            }
        }
    }

    $stageDocs.Add([ordered]@{
        stage    = $st.Number
        title    = $st.Title
        narrative = @($st.Body)
        concepts = @($concepts)
        matchedElements = @($matches)
        ambiguousMentions = @($ambiguous)
    })
}

# Document-level statements that are not tied to any single stage.
$scopeStatement = @($lines | Where-Object { (Get-Comparable $_) -like '*nao contempla migracao*' })
$coverage       = @($lines | Where-Object { (Get-Comparable $_) -like '*cobertura estimada*' })

$doc = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/intent-map/v1'
    source    = (Split-Path $DocPath -Leaf)
    note      = 'Design intent recovered from the POC document. A stage is linked to an element only when that element display name occurs verbatim (accent- and case-insensitive) in the stage text; matchedOn records the string that matched. Absence of intent means the document is silent, not that the element is unimportant.'
    scopeStatement = $scopeStatement
    coverageClaim  = $coverage
    statistics = [ordered]@{
        stageCount        = $stageDocs.Count
        linkedElementCount = $intentByElement.Count
        modelElementCount = $elements.Count
    }
    stages = @($stageDocs)
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutPath) | Out-Null
$doc | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} etapas, {2} elementos com intencao de {3})" -f `
    $OutPath, $stageDocs.Count, $intentByElement.Count, $elements.Count)
