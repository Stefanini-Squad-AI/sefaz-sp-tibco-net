# ePAT PoC — Guia de Execução

Migração TIBCO iProcess → .NET 8 / Elsa 3.7.1.  
Este guia explica como iniciar, executar os cenários e verificar os resultados.

---

## Pré-requisitos

- .NET 8 SDK
- PowerShell (terminal do VS Code)
- Postman (opcional — pode usar `Invoke-RestMethod` no terminal)

---

## 1. Iniciar a API

```powershell
cd 'c:\Users\e_rfdbarssoles\Documents\PoCs\SEFAZ-SP'

# Parar processos residuais
Get-Process dotnet,VBCSCompiler,SefazSp.Epat.Api -ErrorAction SilentlyContinue | Stop-Process -Force

# Compilar
dotnet build .\src\SefazSp.Epat.Api\SefazSp.Epat.Api.csproj -m:1 -nodeReuse:false -p:UseSharedCompilation=false

# Iniciar (porta 5081, sem ruído de SQL)
$env:ASPNETCORE_URLS="http://localhost:5081"
$env:Logging__LogLevel__Microsoft__EntityFrameworkCore__Database__Command="Warning"
dotnet run --no-build --project .\src\SefazSp.Epat.Api\SefazSp.Epat.Api.csproj
```

Aguarde `Now listening on: http://localhost:5081`.  
**Não usar F5 (debugger)** — a máquina não tem memória para o vsdbg + Elsa.

---

## 2. Swagger

Abra no browser: **http://localhost:5081/swagger**

Todas as rotas estão agrupadas por tag:
- **POCEPAT** — fluxo principal (start + 6 eventos)
- **Evidence** — `/workflow/{id}/journey` e `/interactions/{id}`
- **Debug** — subprocessos isolados, regras, builtins

Para importar no Postman: Import → Link → `http://localhost:5081/swagger/v1/swagger.json`

---

## 3. Executar o cenário principal (SC-001 — JUIZ, 30 nós)

Use um segundo terminal para os comandos. O primeiro mostra os logs `[POCEPAT]`.

### Passo a passo manual

```powershell
$b = "http://localhost:5081"
$P = "demo-1"

# 1. Iniciar processo
Invoke-RestMethod "$b/debug/pocepat/start" -Method Post `
  -Body '{"processId":"demo-1","idAiim":42}' -ContentType application/json

# 2. Iniciar Novo Graft
Invoke-RestMethod "$b/pocepat/$P/iniciar-novo-graft" -Method Post

# 3. Preparar Notificação (Correção = Sim)
Invoke-RestMethod "$b/pocepat/$P/preparar-notificacao" -Method Post `
  -Body '{"correcao":true}' -ContentType application/json

# 4. Finalizar AIIM
Invoke-RestMethod "$b/pocepat/$P/finalizar-aiim" -Method Post `
  -Body '{"afrName":"AFR Demo"}' -ContentType application/json

# 5. DEAT0050 INICALC (aguardar ~3s — timer Aguarda Defesa dispara automaticamente)
Invoke-RestMethod "$b/pocepat/$P/deat-inicalc" -Method Post
Start-Sleep -Seconds 3

# 6. Verificar Retorno (TIPOVISTAS = JUIZ)
Invoke-RestMethod "$b/pocepat/$P/verificar-retorno" -Method Post `
  -Body '{"tipoVistas":"JUIZ"}' -ContentType application/json

# 7. Vistas do Juiz → processo termina
Invoke-RestMethod "$b/pocepat/$P/vistas-do-juiz" -Method Post
```

### Script automatizado

```powershell
& .\tools\pocepat-walk.ps1 -Id 42 -TipoVistas JUIZ
```

---

## 4. Cenários alternativos

| Cenário | Comando | Nós | O que testa |
|---------|---------|-----|-------------|
| SC-001 JUIZ | `-TipoVistas JUIZ` | 30 | Caminho completo, todos os conceitos |
| SC-012 MISTA | `-TipoVistas MISTA` | 29 | Via MISTA, signalThrow/Catch |
| SC-010 DRF | `-TipoVistas DRF` | 30 | Timer de fronteira vence (corrida evento ⇄ timer) |
| SC-014 curto-circuito | `-ExisteNotificacao true` | 10 | Existe Notificação? = Sim |
| SC-015 curto-circuito | `-Correcao false` | 6 | Corrigir? = Não → CRNOTPC → fim |
| Graft-real | `-GraftMode true` | 30 | Filhos DEAT0050 anexam ao pai (correlation-join) |
| Erro + operador | `-PrpintpcFails true` | 30 | PRPINTPC falha → operador decide retry |

Exemplos:

```powershell
# MISTA (29 nós)
& .\tools\pocepat-walk.ps1 -Id 100 -TipoVistas MISTA

# DRF com timer (30 nós)
& .\tools\pocepat-walk.ps1 -Id 200 -TipoVistas DRF

# Curto-circuito: Não corrigir (6 nós)
& .\tools\pocepat-walk.ps1 -Id 300 -Correcao false

# Curto-circuito: Existe Notificação (10 nós)
& .\tools\pocepat-walk.ps1 -Id 400 -ExisteNotificacao true

# Graft-real (filhos DEAT0050)
& .\tools\pocepat-walk.ps1 -Id 500 -GraftMode true

# Erro PRPINTPC + retry do operador
& .\tools\pocepat-walk.ps1 -Id 600 -PrpintpcFails true
```

---

## 5. Verificar resultados

### Logs no terminal da API

Cada nó traversado aparece como:
```
[POCEPAT]   → [01] _OAgPol9UEfG6Lfb98zsREQ
[POCEPAT]   → [02] _XWivF1qTEfG5K7mY0I3I6w
...
[POCEPAT]   ↳ DEAT0050: INICALC → CalculaPrazo + HoraFimSC concluídos.
[POCEPAT]   ↳ PRPINTPC concluído (started=True, tentativa 1)
[POCEPAT]   ↳ Decisions (CaptaParametros): fold override → 5 parâmetro(s)
...
[POCEPAT] fluxo concluído — 30 nós. Comparação com oráculo SC-001 (JUIZ): IDÊNTICO ✅
```

- `→ [nn]` = nó BPMN traversado (número sequencial)
- `↳` = subprocesso ou regra a executar
- `IDÊNTICO ✅` = o percurso coincide exactamente com o oráculo imutável
- `DIVERGENTE ❌` = algo está diferente (bug)

### Endpoint Journey (percurso + interações)

```powershell
Invoke-RestMethod "http://localhost:5081/workflow/demo-1/journey" | ConvertTo-Json -Depth 5
```

Devolve:
```json
{
  "processId": "demo-1",
  "status": "Completed",
  "traversed": [ {"index":1,"nodeId":"_OAgPol9..."}, ... ],
  "currentNodeId": null,
  "interactions": [ ... ]
}
```

### Endpoint Interações (request/response de serviço)

```powershell
Invoke-RestMethod "http://localhost:5081/interactions/demo-1" | ConvertTo-Json -Depth 5
```

### Motor de regras Decisions (isolado)

```powershell
Invoke-RestMethod "http://localhost:5081/debug/decisions/evaluate" -Method Post `
  -Body '{"attributes":{"motivoIntimacao":"2","vicioRepresentacao":"1","origem":"NA"}}' `
  -ContentType application/json
```

---

## 6. Postman

### Importar a colecção

1. Postman → Import → Link
2. Colar: `http://localhost:5081/swagger/v1/swagger.json`
3. Definir variável `{{baseUrl}}` = `http://localhost:5081`

### Sequência de chamadas (SC-001 JUIZ)

| # | Método | URL | Body |
|---|--------|-----|------|
| 1 | POST | `{{baseUrl}}/debug/pocepat/start` | `{"processId":"demo-1","idAiim":42}` |
| 2 | POST | `{{baseUrl}}/pocepat/demo-1/iniciar-novo-graft` | *(vazio)* |
| 3 | POST | `{{baseUrl}}/pocepat/demo-1/preparar-notificacao` | `{"correcao":true}` |
| 4 | POST | `{{baseUrl}}/pocepat/demo-1/finalizar-aiim` | `{"afrName":"AFR Demo"}` |
| 5 | POST | `{{baseUrl}}/pocepat/demo-1/deat-inicalc` | *(vazio, aguardar 3s)* |
| 6 | POST | `{{baseUrl}}/pocepat/demo-1/verificar-retorno` | `{"tipoVistas":"JUIZ"}` |
| 7 | POST | `{{baseUrl}}/pocepat/demo-1/vistas-do-juiz` | *(vazio)* |
| 8 | GET | `{{baseUrl}}/workflow/demo-1/journey` | — |
| 9 | GET | `{{baseUrl}}/interactions/demo-1` | — |

---

## 7. Visualização (Blazor)

Para ver o diagrama BPMN a ser percorrido em tempo real:

**Terminal 2** (em paralelo com a API):
```powershell
$env:ASPNETCORE_URLS="http://localhost:5280"
dotnet run --no-build --project .\src\SefazSp.Epat.Web\SefazSp.Epat.Web.csproj
```

Abrir: **http://localhost:5280**

1. Digitar o PROCESS_ID (ex.: `demo-1`)
2. Clicar **Ligar**
3. Usar os botões do painel direito para avançar o fluxo
4. O diagrama actualiza automaticamente a cada 2 segundos

---

## 8. Testes automatizados

```powershell
# Todos os testes oracle (2013+)
dotnet test .\tests\SefazSp.Epat.Oracles.Tests\ -m:1 -nodeReuse:false -p:UseSharedCompilation=false

# Apenas os testes de composição (SC-001/012/010/014/015)
dotnet test .\tests\SefazSp.Epat.Oracles.Tests\ --filter "FullyQualifiedName~Composition"

# Apenas os testes de restart-recovery
dotnet test .\tests\SefazSp.Epat.Oracles.Tests\ --filter "FullyQualifiedName~Restart"

# Apenas os testes de contrato WSDL
dotnet test .\tests\SefazSp.Epat.Oracles.Tests\ --filter "FullyQualifiedName~Contract"
```

---

## 9. Reset (limpar estado)

Para um slate limpo, apagar o ficheiro SQLite:

```powershell
# Parar a API primeiro (Ctrl+C)
Remove-Item .\src\SefazSp.Epat.Api\epat-poc.db* -ErrorAction SilentlyContinue
# Reiniciar a API — a base é recriada automaticamente
```

Ou simplesmente usar um PROCESS_ID diferente a cada execução — cada instância é independente.

---

## Estrutura de portas

| Serviço | Porta | Propósito |
|---------|-------|-----------|
| API (ePAT) | 5081 | Backend — endpoints REST + Elsa runtime |
| Web (Blazor) | 5280 | Frontend — visualização BPMN |

---

## Flags de compilação (ambiente com pouca memória)

Sempre usar estes flags ao compilar:
```
-m:1 -nodeReuse:false -p:UseSharedCompilation=false
```

E parar processos antes de compilar:
```powershell
Get-Process dotnet,VBCSCompiler,SefazSp.Epat.Api,SefazSp.Epat.Web -EA 0 | Stop-Process -Force
```
