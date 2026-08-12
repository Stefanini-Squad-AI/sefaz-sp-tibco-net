<#
.SYNOPSIS
    S1.10 - o indice unico das regras de negocio das tres fontes.

.DESCRIPTION
    A regra deste sistema esta escrita em tres sitios que nao se conhecem: a planilha
    Corticon, o diagrama XPDL e o code-behind das telas. Este gerador nao extrai nada
    de novo - le os tres artefactos que ja existem e projecta-os no mesmo eixo, para
    que as contagens possam ser somadas sem estar a somar coisas diferentes.

    O eixo esta em tools/lib-classification.ps1 e tem duas dimensoes: o EFEITO da
    regra sobre o caso, e o PORTADOR onde ela vive, que determina o que e preciso
    fazer para a mudar.

    Nada aqui e interpretado. Cada entrada aponta para a linha do ficheiro de origem.
#>
[CmdletBinding()]
param(
    [string]$Package        = 'POC_Epat',
    [string]$ArtifactsDir   = "$PSScriptRoot/../artifacts/POC_Epat",
    [string]$OutPath        = "$PSScriptRoot/../artifacts/POC_Epat/rule-catalogue.json"
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/lib-classification.ps1"

function Read-Artifact {
    param([string]$Name, [switch]$Optional)
    $p = Join-Path $ArtifactsDir $Name
    if (-not (Test-Path $p)) {
        if ($Optional) { return $null }
        throw "artifact not found: $p"
    }
    return Get-Content $p -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

$decisions   = Read-Artifact 'decision-tables.json'
$ruleInv     = Read-Artifact 'rule-inventory.json' -Optional
$screenRules = Read-Artifact 'screen-rules.json'   -Optional

$entries = [System.Collections.Generic.List[object]]::new()

# ------------------------------------------------------------- corticon ----

# Uma coluna de regra e uma regra: o conjunto de condicoes que tem de valer, e os
# atributos que ela atribui. O efeito e sempre calcular valor - quem decide o fluxo
# a partir desses valores e o XPDL, mais a frente.
foreach ($col in @(Arr $decisions.rules)) {
    $condicoes = @(Arr $col.conditions | ForEach-Object {
        [ordered]@{ termo = $_.lhs; valor = ($_.rhs -replace "^'|'$", '') }
    })
    $accoes = @(Arr $col.actions | ForEach-Object {
        [ordered]@{ atributo = $_.lhs; valor = ($_.rhs -replace "^'|'$", '') }
    })
    if ($condicoes.Count -eq 0 -and $accoes.Count -eq 0) { continue }

    $entries.Add([ordered]@{
        id             = "RC-corticon-$('{0:D3}' -f [int]$col.column)"
        classification = (New-Classification -Portador 'corticon' -LeCamposDoCaso $true `
                            -TemCondicao ($condicoes.Count -gt 0) -EscreveCampoDoCaso ($accoes.Count -gt 0))
        onde           = [ordered]@{ ficheiro = 'intimacoes_Parametros.ers'; referencia = "coluna $($col.column)"; passo = $col.name }
        processo       = $null
        inPocFlow      = $true
        expressao      = ((Arr $col.conditions | ForEach-Object { $_.expression }) -join ' E ')
        atribui        = @($accoes)
        campos         = @(Arr $col.conditions | ForEach-Object { $_.lhs } | Sort-Object -Unique)
        leitura        = $null
        detalhe        = "$($condicoes.Count) condicao(oes), $($accoes.Count) atribuicao(oes)"
    })
}

# ----------------------------------------------------------------- xpdl ----

foreach ($r in @(Arr $ruleInv.rules)) {
    $entries.Add([ordered]@{
        id             = $r.id
        classification = $r.classification
        onde           = [ordered]@{ ficheiro = 'POC_Epat.xpdl'; referencia = "linha $($r.xpdlLine)"; passo = $r.node }
        processo       = $r.process
        inPocFlow      = $r.inPocFlow
        expressao      = $(if ($r.expression) { $r.expression } else { (@(Arr $r.conditions) -join ' | ') })
        atribui        = @(Arr $r.writesBusinessFields | ForEach-Object { [ordered]@{ atributo = $_; valor = $null } })
        campos         = @(Arr $r.readsBusinessFields)
        leitura        = $r.leitura
        detalhe        = $null
    })
}

# ---------------------------------------------------------------- telas ----

foreach ($s in @(Arr $screenRules.screens)) {
    foreach ($d in @(Arr $s.decisions)) {
        $entries.Add([ordered]@{
            id             = $d.id
            classification = $d.classification
            onde           = [ordered]@{ ficheiro = $s.codeBehind; referencia = "linha $($d.line)"; passo = $d.method }
            processo       = @($s.processes)[0]
            inPocFlow      = $s.inPocFlow
            expressao      = $d.condition
            atribui        = @(Arr $d.effects.engineWrites | ForEach-Object { [ordered]@{ atributo = $_.field; valor = $_.value } })
            campos         = @(Arr $d.readsCaseFields)
            leitura        = $d.leitura
            detalhe        = $null
        })
    }
}

# ------------------------------------------------------------------ write ----

$negocio = @($entries | Where-Object { $_.classification.eRegraDeNegocio })

# A tabela cruzada e o ponto todo do artefacto: mostra o mesmo efeito escrito em
# portadores diferentes, que e o que impede alguem de mudar uma regra num sitio so.
$cruzada = [ordered]@{}
foreach ($ge in ($negocio | Group-Object { $_.classification.efeito } | Sort-Object Name)) {
    $porPortador = [ordered]@{}
    foreach ($gp in ($ge.Group | Group-Object { $_.classification.portador } | Sort-Object Name)) {
        $porPortador[$gp.Name] = $gp.Count
    }
    $cruzada[$ge.Name] = [ordered]@{
        total      = $ge.Count
        descricao  = (Get-EfeitoDescricao $ge.Name)
        porPortador = $porPortador
    }
}

$porAlteracao = [ordered]@{}
foreach ($g in ($negocio | Group-Object { $_.classification.alteracaoRequer } | Sort-Object Name)) { $porAlteracao[$g.Name] = $g.Count }

$porPortador = [ordered]@{}
foreach ($g in ($entries | Group-Object { $_.classification.portador } | Sort-Object Name)) {
    $porPortador[$g.Name] = [ordered]@{
        total          = $g.Count
        regraDeNegocio = @($g.Group | Where-Object { $_.classification.eRegraDeNegocio }).Count
        alteracaoRequer = @($g.Group)[0].classification.alteracaoRequer
    }
}

$out = [ordered]@{
    '$schema' = 'sefaz-sp/tibco-intermediate/rule-catalogue/v1'
    package   = $Package
    note      = 'Indice unico das regras de negocio das tres fontes: planilha Corticon, diagrama XPDL e code-behind das telas. Projecta os artefactos existentes no eixo de tools/lib-classification.ps1; nao extrai nem interpreta nada de novo. efeito diz o que a regra faz ao caso, portador diz onde ela vive e alteracaoRequer diz o que e preciso fazer para a mudar.'
    eixo = [ordered]@{
        efeitos = [ordered]@{}
        alteracaoPorPortador = [ordered]@{}
    }
    summary = [ordered]@{
        total          = $entries.Count
        regraDeNegocio = $negocio.Count
        tecnico        = @($entries | Where-Object { -not $_.classification.eRegraDeNegocio }).Count
        naTrilhaPoc    = @($negocio | Where-Object { $_.inPocFlow }).Count
        porEfeito      = $cruzada
        porAlteracao   = $porAlteracao
        porPortador    = $porPortador
    }
    rules = @($entries | Sort-Object @{ e = { -[int]$_.classification.eRegraDeNegocio } }, @{ e = { $_.classification.efeito } }, @{ e = { $_.classification.portador } }, @{ e = { $_.id } })
}
foreach ($e in (Get-EfeitosConhecidos)) { $out.eixo.efeitos[$e] = (Get-EfeitoDescricao $e) }
foreach ($p in $porPortador.Keys) { $out.eixo.alteracaoPorPortador[$p] = $porPortador[$p].alteracaoRequer }

$outDir = Split-Path -Parent $OutPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$out | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $OutPath -Encoding UTF8

Write-Host ("Wrote {0}  ({1} pontos das 3 fontes: {2} regras de negocio, {3} tecnicos; {4} na trilha da POC)" -f `
    $OutPath, $out.summary.total, $out.summary.regraDeNegocio, $out.summary.tecnico, $out.summary.naTrilhaPoc)
