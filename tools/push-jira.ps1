<#
.SYNOPSIS
    Envia a carga do Jira para o board, de forma idempotente.

.DESCRIPTION
    NAO faz parte do pipeline deterministico: escreve num sistema externo, por isso
    corre a mao e nunca a partir do run-extraction. Reenviar a mesma carga tem de
    ACTUALIZAR e nunca duplicar - a chave e a label 'card:<id>', procurada por JQL
    antes de cada escrita.

    CREDENCIAIS por variavel de ambiente, nunca por parametro: um parametro fica no
    historico da consola e nos logs.
        JIRA_BASE_URL   https://<instancia>.atlassian.net
        JIRA_EMAIL      o e-mail da conta
        JIRA_API_TOKEN  o token

    Por omissao corre em simulacao e nao escreve nada. Usar -Confirmar para enviar.
#>
[CmdletBinding()]
param(
    [string]$PayloadPath = "$PSScriptRoot/../artifacts/POC_Epat/jira/payload.json",
    [switch]$Confirmar
)

$ErrorActionPreference = 'Stop'

function Arr { param($v) return @(@($v) | Where-Object { $null -ne $_ }) }

$base  = $env:JIRA_BASE_URL
$email = $env:JIRA_EMAIL
$token = $env:JIRA_API_TOKEN
if (-not $base -or -not $email -or -not $token) {
    throw "Faltam credenciais. Definir JIRA_BASE_URL, JIRA_EMAIL e JIRA_API_TOKEN no ambiente - nunca como parametro, que fica no historico da consola."
}
$base = $base.TrimEnd('/')
$auth = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("${email}:${token}"))
$cab  = @{ Authorization = $auth; 'Content-Type' = 'application/json' }

$carga = Get-Content $PayloadPath -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Host ("Carga: {0} epicas, {1} issues, {2} ligacoes -> projecto {3} em {4}" -f `
    @($carga.epicas).Count, @($carga.issues).Count, @($carga.ligacoes).Count, $carga.projeto, $base)
if (-not $Confirmar) { Write-Host '  SIMULACAO: nada sera escrito. Usar -Confirmar para enviar.' -ForegroundColor Yellow }

function Find-PorChave {
    param([string]$Chave)
    $jql = "project = `"$($carga.projeto)`" AND labels = `"card:$Chave`""
    $u = "$base/rest/api/3/search/jql?jql=$([Uri]::EscapeDataString($jql))&maxResults=1&fields=key"
    try {
        $r = Invoke-RestMethod -Method Get -Uri $u -Headers $cab -ErrorAction Stop
        if (@($r.issues).Count -gt 0) { return $r.issues[0].key }
    } catch {
        Write-Host "    aviso: pesquisa falhou para $Chave - $($_.Exception.Message)" -ForegroundColor DarkYellow
    }
    return $null
}

$chaveDe = @{}
$criadas = 0; $actualizadas = 0; $falhadas = 0

foreach ($i in (Arr $carga.issues)) {
    $existente = $null
    if ($Confirmar) { $existente = Find-PorChave -Chave $i.chaveExterna }

    $campos = @{
        project     = @{ key = $carga.projeto }
        summary     = $i.campos.summary
        issuetype   = @{ name = $i.campos.issuetype }
        labels      = @($i.campos.labels)
        description = $i.campos.description
    }
    $corpo = @{ fields = $campos } | ConvertTo-Json -Depth 10

    if (-not $Confirmar) {
        Write-Host ("  [simulado] {0}  {1}" -f $i.chaveExterna, $i.campos.summary)
        continue
    }
    try {
        if ($existente) {
            Invoke-RestMethod -Method Put -Uri "$base/rest/api/3/issue/$existente" -Headers $cab -Body $corpo -ErrorAction Stop | Out-Null
            $chaveDe[$i.chaveExterna] = $existente
            $actualizadas++
            Write-Host ("  actualizada {0} -> {1}" -f $i.chaveExterna, $existente)
        } else {
            $r = Invoke-RestMethod -Method Post -Uri "$base/rest/api/3/issue" -Headers $cab -Body $corpo -ErrorAction Stop
            $chaveDe[$i.chaveExterna] = $r.key
            $criadas++
            Write-Host ("  criada      {0} -> {1}" -f $i.chaveExterna, $r.key)
        }
    } catch {
        $falhadas++
        Write-Host ("  FALHOU      {0}: {1}" -f $i.chaveExterna, $_.Exception.Message) -ForegroundColor Red
    }
}

# As ligacoes so depois de tudo existir: uma delas aponta para um card que pode
# ainda nao ter sido criado quando a sua vez chegar.
$ligadas = 0
foreach ($l in (Arr $carga.ligacoes)) {
    if (-not $Confirmar) { continue }
    $de = $chaveDe[$l.de]; $para = $chaveDe[$l.para]
    if (-not $de -or -not $para) { continue }
    $corpo = @{ type = @{ name = $l.tipo }; inwardIssue = @{ key = $de }; outwardIssue = @{ key = $para } } | ConvertTo-Json -Depth 6
    try {
        Invoke-RestMethod -Method Post -Uri "$base/rest/api/3/issueLink" -Headers $cab -Body $corpo -ErrorAction Stop | Out-Null
        $ligadas++
    } catch {
        Write-Host ("  ligacao falhou {0} -> {1}: {2}" -f $l.de, $l.para, $_.Exception.Message) -ForegroundColor DarkYellow
    }
}

if ($Confirmar) {
    Write-Host ("Feito: {0} criada(s), {1} actualizada(s), {2} falhada(s), {3} ligacao(oes)." -f $criadas, $actualizadas, $falhadas, $ligadas)
} else {
    Write-Host 'Simulacao terminada. Nada foi escrito.'
}
