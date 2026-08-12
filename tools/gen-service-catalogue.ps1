#requires -version 5
<#
  Generates artifacts/service-contracts.json

  Flattens both TIBCO Service Descriptors into an implementation-ready contract
  catalogue: every portType operation, its request/response element tree
  flattened to typed paths, the CLR type for each leaf, and a cross-reference to
  the XPDL service tasks that actually invoke it (with the case-field mappings).

  Both services are SOAP-over-JMS via a BusinessWorks iProcess Service Agent, and
  they signal failure through an application-level RESULT/STATUS_CODE envelope
  rather than SOAP faults - captured explicitly under 'technicalEnvelope'.
#>
param(
    [string[]]$WsdlPaths = @(
        "$PSScriptRoot\..\input\Arquivos Poc Camunda\POC_Camunda\POC_Epat\Service Descriptors\EPAT.wsdl",
        "$PSScriptRoot\..\input\Arquivos Poc Camunda\POC_Camunda\POC_Epat\Service Descriptors\DecisionsEPAT.wsdl"
    ),
    [string]$ModelPath = "$PSScriptRoot\..\artifacts\process-model.json",
    [string]$OutPath = "$PSScriptRoot\..\artifacts\service-contracts.json"
)
$ErrorActionPreference = 'Stop'

$XSD = 'http://www.w3.org/2001/XMLSchema'
$WSDLNS = 'http://schemas.xmlsoap.org/wsdl/'

function ConvertTo-ClrType([string]$xsdType) {
    switch -Regex ($xsdType) {
        ':string$' { 'string' } ':normalizedString$' { 'string' }
        ':long$' { 'long' } ':unsignedLong$' { 'ulong' }
        ':int$' { 'int' } ':integer$' { 'int' } ':short$' { 'short' } ':byte$' { 'sbyte' }
        ':decimal$' { 'decimal' } ':float$' { 'float' } ':double$' { 'double' }
        ':boolean$' { 'bool' }
        ':date$' { 'DateOnly' } ':time$' { 'TimeOnly' } ':dateTime$' { 'DateTime' }
        ':base64Binary$' { 'byte[]' } ':hexBinary$' { 'byte[]' }
        ':anyType$' { 'object' }
        default { $null }   # complex type
    }
}

function Get-WsdlModel([string]$path) {
    [xml]$doc = Get-Content -LiteralPath $path -Raw
    $nsm = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
    $nsm.AddNamespace('w', $WSDLNS); $nsm.AddNamespace('xs', $XSD)

    # ---- index every global element / complexType by {ns}localName
    $elements = @{}; $types = @{}
    foreach ($sch in $doc.SelectNodes('//w:types/*', $nsm)) {
        if ($sch.LocalName -ne 'schema') { continue }
        $tns = $sch.GetAttribute('targetNamespace')
        foreach ($c in $sch.ChildNodes) {
            if ($c.NodeType -ne 'Element') { continue }
            $nm = $c.GetAttribute('name')
            if (-not $nm) { continue }
            if ($c.LocalName -eq 'element') { $elements["{$tns}$nm"] = $c }
            elseif ($c.LocalName -in 'complexType', 'simpleType') { $types["{$tns}$nm"] = $c }
        }
    }

    function Resolve-QName($node, [string]$qname) {
        if (-not $qname) { return $null }
        $parts = $qname.Split(':')
        if ($parts.Count -eq 2) { $uri = $node.GetNamespaceOfPrefix($parts[0]); $ln = $parts[1] }
        else { $uri = $node.GetNamespaceOfPrefix(''); $ln = $parts[0] }
        "{$uri}$ln"
    }

    # ---- flatten an element declaration into typed leaf paths
    $script:leaves = $null
    function Expand-Node($node, [string]$path, [int]$depth, [System.Collections.ArrayList]$acc, $seen) {
        if ($depth -gt 12) { return }
        $typeAttr = $node.GetAttribute('type')
        $inline = $node.SelectSingleNode('xs:complexType', $nsm)
        $minOcc = $node.GetAttribute('minOccurs'); $maxOcc = $node.GetAttribute('maxOccurs')

        if ($typeAttr) {
            $clr = ConvertTo-ClrType $typeAttr
            if ($clr) {
                [void]$acc.Add([ordered]@{
                        path = $path; xsdType = $typeAttr; clrType = $clr
                        required = ($minOcc -ne '0'); repeating = ($maxOcc -eq 'unbounded' -or ([int]0 -ne 0))
                    })
                return
            }
            $key = Resolve-QName $node $typeAttr
            if ($seen.Contains($key)) {
                [void]$acc.Add([ordered]@{ path = $path; xsdType = $typeAttr; clrType = 'recursive'; required = ($minOcc -ne '0') })
                return
            }
            $ct = $types[$key]
            if (-not $ct) {
                [void]$acc.Add([ordered]@{ path = $path; xsdType = $typeAttr; clrType = 'unresolved'; required = ($minOcc -ne '0') })
                return
            }
            $null = $seen.Add($key)
            foreach ($child in $ct.SelectNodes('.//xs:element', $nsm)) {
                if ($child.ParentNode.LocalName -notin 'sequence', 'all', 'choice') { continue }
                Expand-Node $child "$path/$($child.GetAttribute('name'))" ($depth + 1) $acc $seen
            }
            $null = $seen.Remove($key)
            return
        }
        if ($inline) {
            foreach ($child in $inline.SelectNodes('.//xs:element', $nsm)) {
                if ($child.ParentNode.LocalName -notin 'sequence', 'all', 'choice') { continue }
                Expand-Node $child "$path/$($child.GetAttribute('name'))" ($depth + 1) $acc $seen
            }
            return
        }
        [void]$acc.Add([ordered]@{ path = $path; xsdType = 'anyType'; clrType = 'object'; required = ($minOcc -ne '0') })
    }

    function Expand-Message([string]$msgQName, $ctxNode) {
        $key = Resolve-QName $ctxNode $msgQName
        $local = $key -replace '^\{[^}]*\}', ''
        $msg = $doc.SelectNodes('//w:message', $nsm) | Where-Object { $_.GetAttribute('name') -eq $local } | Select-Object -First 1
        if (-not $msg) { return @() }
        $out = @()
        foreach ($part in $msg.SelectNodes('w:part', $nsm)) {
            $elQ = $part.GetAttribute('element')
            $acc = New-Object System.Collections.ArrayList
            if ($elQ) {
                $elKey = Resolve-QName $part $elQ
                $el = $elements[$elKey]
                if ($el) {
                    $seen = New-Object 'System.Collections.Generic.HashSet[string]'
                    Expand-Node $el '' 0 $acc $seen
                }
            }
            $out += [ordered]@{
                partName = $part.GetAttribute('name')
                element  = $elQ
                fields   = @($acc)
            }
        }
        , $out
    }

    $ops = @()
    foreach ($pt in $doc.SelectNodes('//w:portType', $nsm)) {
        foreach ($op in $pt.SelectNodes('w:operation', $nsm)) {
            $inM = $op.SelectSingleNode('w:input', $nsm)
            $outM = $op.SelectSingleNode('w:output', $nsm)
            $ops += [ordered]@{
                portType  = $pt.GetAttribute('name')
                name      = $op.GetAttribute('name')
                # BW/iProcess mangles the folder path: __sol_ = '/', _sp_ = ' '
                logicalPath = ($op.GetAttribute('name') -replace '__sol_', '/' -replace '_sp_', ' ')
                input     = if ($inM) { Expand-Message $inM.GetAttribute('message') $inM } else { @() }
                output    = if ($outM) { Expand-Message $outM.GetAttribute('message') $outM } else { @() }
            }
        }
    }

    $services = @()
    foreach ($svc in $doc.SelectNodes('//w:service', $nsm)) {
        foreach ($port in $svc.SelectNodes('w:port', $nsm)) {
            $loc = $null
            foreach ($c in $port.ChildNodes) { if ($c.Attributes -and $c.Attributes['location']) { $loc = $c.Attributes['location'].Value } }
            $services += [ordered]@{
                service = $svc.GetAttribute('name'); port = $port.GetAttribute('name')
                binding = $port.GetAttribute('binding'); location = $loc
                transport = $(if ($loc -match '^tcp://|^tibjmsnaming://') { 'SOAP over JMS (TIBCO EMS)' } else { 'HTTP' })
            }
        }
    }

    # technical envelope (shared TechnicalObjects schema)
    $env = @{}
    foreach ($k in $types.Keys) {
        if ($k -match 'technicalobjects\.xsd\}(HEADER|RESULT|ERROR|APPLICATIONDATA|APPLICATIONDATAS)$') {
            $t = $types[$k]
            $env[$Matches[1]] = @(foreach ($el in $t.SelectNodes('.//xs:element', $nsm)) {
                    [ordered]@{ name = $el.GetAttribute('name'); xsdType = $el.GetAttribute('type'); clrType = (ConvertTo-ClrType $el.GetAttribute('type')); required = ($el.GetAttribute('minOccurs') -ne '0') }
                })
        }
    }

    [ordered]@{
        file            = (Split-Path $path -Leaf)
        targetNamespace = $doc.DocumentElement.GetAttribute('targetNamespace')
        endpoints       = $services
        technicalEnvelope = $env
        operations      = $ops
    }
}

# ------------------------------------------------------------- build
$wsdls = @()
foreach ($p in $WsdlPaths) { $wsdls += Get-WsdlModel $p }

# ------------------------------------------------------------- cross-reference
$bindings = @()
if (Test-Path -LiteralPath $ModelPath) {
    $model = Get-Content -LiteralPath $ModelPath -Raw | ConvertFrom-Json
    foreach ($proc in $model.processes) {
        foreach ($s in $proc.scopes) {
            foreach ($n in $s.nodes) {
                if ($n.kind -ne 'serviceTask' -or -not $n.operation) { continue }
                $bindings += [ordered]@{
                    process       = $proc.name
                    scope         = $s.scope
                    node          = $(if ($n.displayName) { $n.displayName } else { $n.name })
                    nodeId        = $n.id
                    wsdl          = $n.operation.wsdl
                    operationName = $n.operation.operationName
                    transport     = $n.operation.transport
                    serviceName   = $n.operation.serviceName
                    portName      = $n.operation.portName
                    inputs        = @($n.inputMappings | ForEach-Object { [ordered]@{ caseField = $_.actual; soapPath = ($_.formal -replace '\[\d+\]', '' -replace '[^/]+:', '') } })
                    outputs       = @($n.outputMappings | ForEach-Object { [ordered]@{ caseField = $_.actual; soapPath = ($_.formal -replace '\[\d+\]', '' -replace '[^/]+:', '') } })
                }
            }
        }
    }
}
$boundOps = @($bindings | ForEach-Object { $_.operationName } | Sort-Object -Unique)
foreach ($wd in $wsdls) {
    foreach ($op in $wd.operations) {
        $op.isInvokedByProcess = ($boundOps -contains $op.name)
    }
}

$doc = [ordered]@{
    '$schema'   = 'sefaz-sp/tibco-intermediate/service-contracts/v1'
    notes       = @(
        'Transport is SOAP over JMS through a BusinessWorks iProcess Service Agent, not HTTP. A .NET port needs either an HTTP facade in front of the same backend or an EMS/AMQP client.',
        'Failures are reported in-band via RESULT/STATUS_CODE + ERROR, not as SOAP faults. STATUS_CODE != 0 means application error; the XPDL wrappers then retry or escalate to a human exception task.',
        'Operation names are BW resource paths with __sol_ = "/" and _sp_ = " "; see logicalPath.',
        'Only the operations with isInvokedByProcess = true are reachable from this XPDL package; the rest are the wider ePAT surface area.'
    )
    statistics  = [ordered]@{
        wsdlCount      = $wsdls.Count
        operationCount = ($wsdls | ForEach-Object { $_.operations.Count } | Measure-Object -Sum).Sum
        invokedCount   = $boundOps.Count
        bindingCount   = $bindings.Count
    }
    invokedOperations = $boundOps
    processBindings = $bindings
    services    = $wsdls
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutPath) | Out-Null
$doc | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath $OutPath -Encoding UTF8
Write-Host "Wrote $OutPath  ($($doc.statistics.operationCount) operations, $($boundOps.Count) invoked by the process)"
