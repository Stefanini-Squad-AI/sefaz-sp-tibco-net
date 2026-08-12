<#
.SYNOPSIS
    Move issues do Jira para um estado, pela chave.

.DESCRIPTION
    Complemento do push: o envio cria e poe no estado inicial; isto move depois,
    a partir do plano de execucao ou da linha de comando.

    O Jira NAO aceita estado directamente - so transicoes. Este script pergunta ao
    proprio Jira que transicoes existem a partir do estado actual da issue e escolhe
    a que aterra no estado pedido. Se nao houver caminho directo, diz quais existem
    em vez de falhar sem explicacao.

    CREDENCIAIS por variavel de ambiente, como no push:
        JIRA_BASE_URL, JIRA_EMAIL, JIRA_API_TOKEN

    Por omissao corre em simulacao. Usar -Confirmar para mover.

.EXAMPLE
    ./tools/jira-transicao.ps1 -Estado 'In Progress' -Chaves SSTN-7,SSTN-8 -Confirmar
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string[]]$Chaves,
    [Parameter(Mandatory)][string]$Estado,
    [switch]$Confirmar
)

$ErrorActionPreference = 'Stop'

$base  = $env:JIRA_BASE_URL
$email = $env:JIRA_EMAIL
$token = $env:JIRA_API_TOKEN
if (-not $base -or -not $email -or -not $token) {
    throw "Faltam credenciais. Definir JIRA_BASE_URL, JIRA_EMAIL e JIRA_API_TOKEN no ambiente."
}
$base = $base.Trim().TrimEnd('/')
$auth = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("$($email.Trim()):$($token.Trim())"))
$cab  = @{ Authorization = $auth; 'Content-Type' = 'application/json' }

function Get-ErroJira {
    param($Erro)
    $corpo = $Erro.ErrorDetails.Message
    if (-not $corpo) { return $Erro.Exception.Message }
    try {
        $j = $corpo | ConvertFrom-Json
        $p = @()
        foreach ($m in @($j.errorMessages)) { if ($m) { $p += $m } }
        foreach ($x in @($j.errors.PSObject.Properties)) { $p += "$($x.Name): $($x.Value)" }
        if ($p.Count) { return ($p -join ' | ') }
    } catch { }
    return $corpo
}

# Vem do plano como uma unica cadeia separada por virgulas quando colado a mao.
$lista = @($Chaves | ForEach-Object { $_ -split '[,;\s]+' } | Where-Object { $_ } | Sort-Object -Unique)

Write-Host ("{0} issue(s) -> '{1}' em {2}" -f $lista.Count, $Estado, $base)
if (-not $Confirmar) { Write-Host '  SIMULACAO: nada sera movido. Usar -Confirmar.' -ForegroundColor Yellow }

$movidas = 0; $jaLa = 0; $falhadas = 0
foreach ($k in $lista) {
    try {
        $actual = (Invoke-RestMethod -Method Get -Uri "$base/rest/api/2/issue/$k`?fields=status" -Headers $cab -ErrorAction Stop).fields.status.name
        if ($actual -eq $Estado) {
            $jaLa++
            Write-Host ("  {0}  ja esta em '{1}'" -f $k, $Estado) -ForegroundColor DarkGray
            continue
        }
        $ts = (Invoke-RestMethod -Method Get -Uri "$base/rest/api/2/issue/$k/transitions" -Headers $cab -ErrorAction Stop).transitions
        $t = @($ts | Where-Object { $_.to.name -eq $Estado })[0]
        if (-not $t) {
            $falhadas++
            Write-Host ("  {0}  sem transicao de '{1}' para '{2}'. Disponiveis: {3}" -f `
                $k, $actual, $Estado, ((@($ts | ForEach-Object { $_.to.name })) -join ', ')) -ForegroundColor Yellow
            continue
        }
        if (-not $Confirmar) {
            Write-Host ("  [simulado] {0}  '{1}' -> '{2}'" -f $k, $actual, $Estado)
            continue
        }
        $corpo = @{ transition = @{ id = $t.id } } | ConvertTo-Json -Depth 4
        Invoke-RestMethod -Method Post -Uri "$base/rest/api/2/issue/$k/transitions" -Headers $cab -Body $corpo -ErrorAction Stop | Out-Null
        $movidas++
        Write-Host ("  {0}  '{1}' -> '{2}'" -f $k, $actual, $Estado) -ForegroundColor Green
    } catch {
        $falhadas++
        Write-Host ("  {0}  FALHOU: {1}" -f $k, (Get-ErroJira $_)) -ForegroundColor Red
    }
}

if ($Confirmar) { Write-Host ("Feito: {0} movida(s), {1} ja no estado, {2} falhada(s)." -f $movidas, $jaLa, $falhadas) }
else { Write-Host 'Simulacao terminada.' }
