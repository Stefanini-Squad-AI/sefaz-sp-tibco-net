param([string]$Id = "test", [string]$TipoVistas = "JUIZ", [string]$Correcao = "true", [string]$ExisteNotificacao = "false", [string]$GraftMode = "false", [string]$PrpintpcFails = "false")
$ErrorActionPreference = "Stop"
$P = "idAiim-$Id`idProc-$Id"
$base = "http://localhost:5081"
$json = "application/json"
Invoke-RestMethod -Uri "$base/debug/pocepat/start" -Method Post -Body (@{ processId = $P; idAiim = [long]$Id; existeNotificacao = [bool]::Parse($ExisteNotificacao); graftMode = [bool]::Parse($GraftMode); prpintpcFails = [bool]::Parse($PrpintpcFails) } | ConvertTo-Json) -ContentType $json | Out-Null
Invoke-RestMethod -Uri "$base/pocepat/$P/iniciar-novo-graft" -Method Post | Out-Null
Invoke-RestMethod -Uri "$base/pocepat/$P/preparar-notificacao" -Method Post -Body (@{ correcao = [bool]::Parse($Correcao) } | ConvertTo-Json) -ContentType $json | Out-Null
if (-not [bool]::Parse($Correcao)) {
    Start-Sleep -Milliseconds 600  # SC-015: Corrigir?=No → Criar Notificacao (CRNOTPC) → endEvent
    Write-Output "walk $P (SC-015 Corrigir=No) done"
    return
}
Invoke-RestMethod -Uri "$base/pocepat/$P/finalizar-aiim" -Method Post -Body (@{ afrName = "AFR-$Id" } | ConvertTo-Json) -ContentType $json | Out-Null
if ([bool]::Parse($ExisteNotificacao)) {
    Start-Sleep -Milliseconds 600  # SC-014: Existe Notificação?=Sim short-circuits at node 9 → endEvent
    Write-Output "walk $P (SC-014 Existe Notificacao=Sim) done"
    return
}
if ([bool]::Parse($GraftMode)) {
    # graft-real: two DEAT0050 children attach + complete at different times, then close the window
    Invoke-RestMethod -Uri "$base/pocepat/$P/graft-attach" -Method Post -Body (@{ childId = "DEAT0050-A" } | ConvertTo-Json) -ContentType $json | Out-Null
    Invoke-RestMethod -Uri "$base/pocepat/$P/graft-attach" -Method Post -Body (@{ childId = "DEAT0050-B" } | ConvertTo-Json) -ContentType $json | Out-Null
    Invoke-RestMethod -Uri "$base/pocepat/$P/graft-complete" -Method Post -Body (@{ childId = "DEAT0050-A" } | ConvertTo-Json) -ContentType $json | Out-Null
    Invoke-RestMethod -Uri "$base/pocepat/$P/graft-complete" -Method Post -Body (@{ childId = "DEAT0050-B" } | ConvertTo-Json) -ContentType $json | Out-Null
    Invoke-RestMethod -Uri "$base/pocepat/$P/graft-close" -Method Post | Out-Null
    Start-Sleep -Milliseconds 500
} else {
    Invoke-RestMethod -Uri "$base/pocepat/$P/deat-inicalc" -Method Post | Out-Null
    Start-Sleep -Seconds 3  # let the DEAT0050 'Aguarda Defesa' demo timer (2s) auto-fire
}
if ([bool]::Parse($PrpintpcFails)) {
    # node 18: PRPINTPC app-error suspends for the operator; OUTCOME=R retries → success
    Invoke-RestMethod -Uri "$base/pocepat/$P/operator-decision" -Method Post | Out-Null
    Start-Sleep -Milliseconds 400
}
Invoke-RestMethod -Uri "$base/pocepat/$P/verificar-retorno" -Method Post -Body (@{ tipoVistas = $TipoVistas } | ConvertTo-Json) -ContentType $json | Out-Null
switch ($TipoVistas) {
    "JUIZ"  { Invoke-RestMethod -Uri "$base/pocepat/$P/vistas-do-juiz" -Method Post | Out-Null }
    "MISTA" { Invoke-RestMethod -Uri "$base/pocepat/$P/realizar-vista-mista" -Method Post | Out-Null }
    default { Start-Sleep -Seconds 3 }  # DRF: let the boundary timer (2s) win → Fim de Prazo
}
Start-Sleep -Milliseconds 600
Write-Output "walk $P ($TipoVistas) done"
