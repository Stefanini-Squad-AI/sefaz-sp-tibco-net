#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Resultado possível do percurso do segmento 050 do processo BSCENVPC.
/// </summary>
public enum BscenvpcSeg050Result
{
    /// <summary>
    /// Fluxo chegou ao linkCatch 'Try Task' (_qIDumF6BEfGBBLgT-R5iuw) após Pause.
    /// Continuar com o segmento de chamada de serviço.
    /// </summary>
    ReachedTryTask,

    /// <summary>
    /// ISTECHERROR == "Y" após regresso do subProcessScope —
    /// erro técnico explícito de infraestrutura; encaminha para intervenção manual.
    /// </summary>
    TechErrorBranch,

    /// <summary>
    /// ISAPPERROR != "Y" após Tech Error Otherwise —
    /// erro não classificado como aplicacional; encaminha para tratamento alternativo.
    /// </summary>
    NotAppError,

    /// <summary>
    /// NUMAPPRETRIES &gt;= MAXRETRIES — retentativas de aplicação esgotadas;
    /// encaminha para intervenção manual (Manipular Excecao).
    /// </summary>
    AppRetryExhausted,

    /// <summary>
    /// SW_QRETRYCOUNT &lt; MAXRETRIES — motor tem capacidade de retry;
    /// prosseguir com a chamada de serviço (caminho Stillgood do subProcessScope).
    /// </summary>
    StillgoodProceedToServiceCall,
}

/// <summary>
/// Workflow do segmento 050 do processo BSCENVPC: de 'Start Event' a 'Try Task'.
///
/// Card: BUILD-BSCENVPC-seg050 · Processo: BSCENVPC · Etapa: 5
/// Cenário de referência: SC-BSCENVPC-013, segmento 1, passos 1–15.
///
/// Herdado de POC_EpatProcess/Busca Emails.
///
/// Topologia dos 15 nós (percurso de referência SC-BSCENVPC-013, segmento 1):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _qIDulF6BEfGBBLgT-R5iuw  startEvent          │
/// │   ↓ (sem transição XPDL no arranque — ponto de entrada do subprocesso)       │
/// │  [2]  SetParameters             _qIDulV6BEfGBBLgT-R5iuw  scriptTask          │
/// │        Regra: RI-script-BSCENVPC-SetParameters                                │
/// │        NOEQ-iprocess-builtin: shim-tri-state (SW_NA), ratificado 2026-08-06   │
/// │   ↓ fluxo                                                                     │
/// │  [3]  Start Loop                _qIDul16BEfGBBLgT-R5iuw  scriptTask          │
/// │        NOEQ-iprocess-builtin: shim-tri-state, ratificado 2026-08-06           │
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _qIDupV6BEfGBBLgT-R5iuw  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC3                         │
/// │       ┌─ ActivitySet scope ─────────────────────────────────────────────────┐  │
/// │       │ [5]  startEvent interno  _qIDu3l6BEfGBBLgT-R5iuw  startEvent       │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [6]  Start TX            _qIDu3F6BEfGBBLgT-R5iuw  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [7]  Check Retries       _qIDu3V6BEfGBBLgT-R5iuw  gateway         │  │
/// │       │       Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT      │  │
/// │       │       ramo Stillgood: SW_QRETRYCOUNT &lt; MAXRETRIES                │  │
/// │       │       ramo Maxretriesexceeded (OTHERWISE): → Set Technical Error    │  │
/// │       │  ↓ OTHERWISE                                                        │  │
/// │       │ [8]  Set Technical Error _qIDu4F6BEfGBBLgT-R5iuw  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [9]  endEvent interno    _qIDu316BEfGBBLgT-R5iuw  endEvent         │  │
/// │       └────────────────────────────────────────────────────────────────────┘  │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC5                       │
/// │  [10] Tech Error                _qIDupF6BEfGBBLgT-R5iuw  gateway             │
/// │        ramo "No" (otherwise): → App Error                                     │
/// │   ↓ ramo "No"                                                                 │
/// │  [11] App Error                 _qIDuo16BEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": ISAPPERROR == "Y" → More Retries                           │
/// │   ↓ ramo "Yes"                                                                │
/// │  [12] More Retries              _qIDuoF6BEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES → Pause                      │
/// │   ↓ ramo "Yes"                                                                │
/// │  [13] Pause                     _qIDuoV6BEfGBBLgT-R5iuw  timerEvent          │
/// │        IClock injectado — DateTime.Now proibido (scaffold final)              │
/// │        Duração: 30 minutos (RI-deadline-BSCENVPC-Pause: Minutes=30)          │
/// │   ↓ fluxo                                                                     │
/// │  [14] Link To: Try Task         _qIDun16BEfGBBLgT-R5iuw  linkThrow           │
/// │        NOEQ-link-goto: flatten-edge — sem evento de sinal intermediário       │
/// │   ↓ aresta explícita flatten-edge — AC7                                       │
/// │  [15] Try Task                  _qIDumF6BEfGBBLgT-R5iuw  linkCatch           │
/// │        entrouPor=link — aresta explícita escrita neste workflow                │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw)
///   • ordem 10 · regresso  · endEvent do subprocesso → gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw)
///   • ordem 15 · link      · linkThrow (_qIDun16BEfGBBLgT-R5iuw) → linkCatch Try Task (_qIDumF6BEfGBBLgT-R5iuw)
///
/// NOEQ-link-goto (decisão flatten-edge, ratificado 2026-08-06):
///   O par linkThrow/linkCatch é achatado numa aresta explícita de fluxo.
///   Não usar evento de sinal intermediário (introduziria pontos de espera inexistentes no TIBCO).
/// </summary>
public sealed class BscenvpcSeg050Workflow
{
    // ── Identificadores de nó — invariantes (não renomear) ───────────────────

    /// <summary>Nó 1  — Start Event (ponto de entrada, MAIN).</summary>
    public const string NodeStartEvent         = "_qIDulF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-BSCENVPC-SetParameters.</summary>
    public const string NodeSetParameters      = "_qIDulV6BEfGBBLgT-R5iuw";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN).</summary>
    public const string NodeStartLoop          = "_qIDul16BEfGBBLgT-R5iuw";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeSubProcessScope    = "_qIDupV6BEfGBBLgT-R5iuw";

    /// <summary>Nó 5  — startEvent interno (descida explícita, ActivitySet).</summary>
    public const string NodeStartEventInternal = "_qIDu3l6BEfGBBLgT-R5iuw";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx            = "_qIDu3F6BEfGBBLgT-R5iuw";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries       = "_qIDu3V6BEfGBBLgT-R5iuw";

    /// <summary>Nó 8  — Set Technical Error (scriptTask, ActivitySet). Executado no ramo Maxretriesexceeded.</summary>
    public const string NodeSetTechnicalError  = "_qIDu4F6BEfGBBLgT-R5iuw";

    /// <summary>Nó 9  — endEvent interno (ActivitySet). Dispara o regresso ao escopo MAIN.</summary>
    public const string NodeEndEventInternal   = "_qIDu316BEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Tech Error (gateway, MAIN). Alcançado por regresso explícito desde o endEvent interno.</summary>
    public const string NodeTechError          = "_qIDupF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError           = "_qIDuo16BEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries        = "_qIDuoF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 13 — Pause (timerEvent, MAIN). Duração: 30 minutos. IClock injectado.</summary>
    public const string NodePause              = "_qIDuoV6BEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — Link To: Try Task (linkThrow, MAIN). Achatado em aresta explícita (flatten-edge).</summary>
    public const string NodeLinkThrow          = "_qIDun16BEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — Try Task (linkCatch, MAIN). entrouPor=link; aresta explícita flatten-edge.</summary>
    public const string NodeTryTask            = "_qIDumF6BEfGBBLgT-R5iuw";

    // ─────────────────────────────────────────────────────────────────────────

    private readonly IClock _clock;

    /// <param name="clock">
    ///   Abstracção de tempo injectada. Usada pelo timerEvent Pause (nó 13).
    ///   Nunca usar <see cref="DateTime.Now"/> directamente —
    ///   o scaffold <c>Domain/Abstractions</c> tem status=final e o contrato de teste
    ///   depende de relógio controlável.
    /// </param>
    public BscenvpcSeg050Workflow(IClock clock)
    {
        _clock = clock;
    }

    /// <summary>
    /// Executa o segmento 050 (passos 1–15 do cenário SC-BSCENVPC-013).
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ctx">Contexto de execução mutável partilhado com o resto do subprocesso.</param>
    /// <param name="swQRetryCount">
    ///   Valor de <c>IPESystemValues.SW_QRETRYCOUNT</c> fornecido pelo runtime.
    ///   Lido pelo gateway Check Retries (nó 7); nunca escrito pelo processo.
    ///   NOEQ-iprocess-builtin: shim-tri-state, ratificado 2026-08-06.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<BscenvpcSeg050Result> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_qIDulF6BEfGBBLgT-R5iuw) ─────────
        // Ponto de entrada do subprocesso BSCENVPC chamado por POC_EpatProcess/Busca Emails.
        // Sem efeito lateral. Controlo passa ao nó 2.

        // ── Nó 2: scriptTask 'SetParameters' (_qIDulV6BEfGBBLgT-R5iuw) ───────
        // Regra: RI-script-BSCENVPC-SetParameters (eRegraDeNegocio=true → Domain/Rules).
        // NOEQ-iprocess-builtin: IDPROCESSO comparado com SW_NA via shim-tri-state.
        var idProcesso = BscenvpcSeg050Steps.ParseIdProcesso(caseRef.ProcessId);
        if (BscenvpcSetParametersRule.ShouldInitialize(idProcesso, ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES))
            BscenvpcSeg050Steps.ApplySetParameters(ctx);

        // ── Nó 3: scriptTask 'Start Loop' (_qIDul16BEfGBBLgT-R5iuw) ──────────
        // NOEQ-iprocess-builtin: SW_NA do iProcess; em .NET o estado é mantido no contexto.
        BscenvpcSeg050Steps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_qIDupV6BEfGBBLgT-R5iuw) ──
        // ── Nó 5: startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw, entrouPor=descida) ─────
        // DESCIDA EXPLÍCITA: não existe transição XPDL do subProcessScope para o startEvent
        // interno. A aresta é escrita explicitamente neste workflow (AC3).
        {
            var subResult = ExecuteSubProcessScope(ctx, swQRetryCount);

            // ── Nó 10: gateway 'Tech Error' (_qIDupF6BEfGBBLgT-R5iuw, entrouPor=regresso) ──
            // REGRESSO EXPLÍCITO: não existe transição XPDL do endEvent interno (_qIDu316BEfGBBLgT-R5iuw)
            // de volta ao escopo MAIN. A aresta é escrita explicitamente aqui (AC5).
            if (subResult == SubProcessResult.TechError)
            {
                // Ramo "Yes" de Tech Error: ISTECHERROR == "Y".
                // Encaminha para intervenção manual (fora do escopo deste segmento).
                return BscenvpcSeg050Result.TechErrorBranch;
            }

            if (subResult == SubProcessResult.StillgoodProceedToServiceCall)
            {
                // Ramo "Stillgood" do Check Retries: SW_QRETRYCOUNT < MAXRETRIES.
                // O segmento de chamada de serviço toma o controlo a partir daqui.
                return BscenvpcSeg050Result.StillgoodProceedToServiceCall;
            }

            // subResult == SubProcessResult.Maxretriesexceeded:
            // Set Technical Error foi executado. Regresso ao MAIN com ISTECHERROR != "Y".

            // ── Nó 10: gateway 'Tech Error' (_qIDupF6BEfGBBLgT-R5iuw) ─────────
            // Ramo "No" (OTHERWISE): ISTECHERROR != "Y" → App Error.
            if (BscenvpcSeg050Steps.IsTechError(ctx))
                return BscenvpcSeg050Result.TechErrorBranch;

            // ── Nó 11: gateway 'App Error' (_qIDuo16BEfGBBLgT-R5iuw) ──────────
            // Ramo "Yes": ISAPPERROR == "Y" → More Retries.
            // Ramo "No" (otherwise): não é erro aplicacional.
            if (!BscenvpcSeg050Steps.IsAppError(ctx))
                return BscenvpcSeg050Result.NotAppError;

            // ── Nó 12: gateway 'More Retries' (_qIDuoF6BEfGBBLgT-R5iuw) ───────
            // Ramo "Yes": NUMAPPRETRIES < MAXRETRIES → Pause.
            // Ramo "No" (otherwise): retentativas esgotadas.
            if (!BscenvpcSeg050Steps.HasMoreRetries(ctx))
                return BscenvpcSeg050Result.AppRetryExhausted;
        }

        // ── Nó 13: timerEvent 'Pause' (_qIDuoV6BEfGBBLgT-R5iuw) ─────────────
        // IClock injectado — DateTime.Now proibido (AC6).
        // Duração: 30 minutos (RI-deadline-BSCENVPC-Pause: Minutes=30).
        await PauseAsync(_clock, ct).ConfigureAwait(false);

        // ── Nó 14: linkThrow 'Link To: Try Task' (_qIDun16BEfGBBLgT-R5iuw) ───
        // ── Nó 15: linkCatch 'Try Task' (_qIDumF6BEfGBBLgT-R5iuw, entrouPor=link) ──
        // NOEQ-link-goto (flatten-edge, ratificado 2026-08-06):
        // o par throw/catch é achatado numa aresta directa (AC7).
        // Não usar evento de sinal intermediário — introduziria pontos de espera inexistentes no TIBCO.
        return BscenvpcSeg050Result.ReachedTryTask;
    }

    // ── Execução do subProcessScope (ActivitySet) ─────────────────────────────

    /// <summary>
    /// Executa o escopo embutido 'Control System Task Call' (_qIDupV6BEfGBBLgT-R5iuw).
    ///
    /// A DESCIDA para o startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw) é escrita aqui
    /// como transição explícita — não existe no XPDL (AC3).
    ///
    /// O REGRESSO ao escopo MAIN pela aresta explícita está no chamador (AC5).
    /// </summary>
    private static SubProcessResult ExecuteSubProcessScope(
        ProcessExecutionContext ctx,
        long swQRetryCount)
    {
        // ── Nó 5: startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw) ──────────────
        // Descida explícita: aresta de entrada no ActivitySet escrita pelo workflow .NET.

        // ── Nó 6: scriptTask 'Start TX' (_qIDu3F6BEfGBBLgT-R5iuw) ───────────
        BscenvpcSeg050Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_qIDu3V6BEfGBBLgT-R5iuw) ──
        // Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT (eRegraDeNegocio=true → Domain/Rules).
        // Ramo "Stillgood": SW_QRETRYCOUNT < MAXRETRIES → prossegue para chamada de serviço.
        // Ramo "Maxretriesexceeded" (OTHERWISE): motor esgotou → Set Technical Error.
        if (BscenvpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Ramo Stillgood: a chamada de serviço fica a cargo do segmento seguinte.
            // ── Nó 9: endEvent interno (_qIDu316BEfGBBLgT-R5iuw) ────────────
            return SubProcessResult.StillgoodProceedToServiceCall;
        }

        // ── Nó 8: scriptTask 'Set Technical Error' (_qIDu4F6BEfGBBLgT-R5iuw) ──
        // Executado quando SW_QRETRYCOUNT >= MAXRETRIES.
        // Regista a causa; não altera ISTECHERROR (o flag permanece conforme o ciclo anterior).
        // Fonte: RI-script-BSCENVPC-SetTechnicalError (expressão vazia, eRegraDeNegocio=false).
        BscenvpcSeg050Steps.ApplySetTechnicalError(ctx, "SW_QRETRYCOUNT >= MAXRETRIES");

        // ── Nó 9: endEvent interno (_qIDu316BEfGBBLgT-R5iuw) ─────────────────
        // Fim do ActivitySet. O regresso ao MAIN é escrito explicitamente no chamador (AC5).
        return SubProcessResult.Maxretriesexceeded;
    }

    // ── timerEvent Pause ──────────────────────────────────────────────────────

    /// <summary>
    /// Implementa a pausa do timerEvent Pause (_qIDuoV6BEfGBBLgT-R5iuw).
    /// Duração de 30 minutos — fonte: RI-deadline-BSCENVPC-Pause (Minutes=30).
    /// O instante de retoma é calculado a partir de <see cref="IClock.Now"/> —
    /// nunca de <see cref="DateTime.Now"/> (AC6).
    /// </summary>
    private static async Task PauseAsync(IClock clock, CancellationToken ct)
    {
        var pauseDuration = TimeSpan.FromMinutes(30);
        var deadline      = clock.Now.Add(pauseDuration);
        var remaining     = deadline - clock.Now;
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining, ct).ConfigureAwait(false);
    }

    /// <summary>Resultado interno do subProcessScope 'Control System Task Call'.</summary>
    private enum SubProcessResult
    {
        /// <summary>SW_QRETRYCOUNT &lt; MAXRETRIES — chamada de serviço disponível.</summary>
        StillgoodProceedToServiceCall,

        /// <summary>SW_QRETRYCOUNT &gt;= MAXRETRIES — Set Technical Error executado.</summary>
        Maxretriesexceeded,

        /// <summary>Erro técnico explícito de infraestrutura (ISTECHERROR = "Y").</summary>
        TechError,
    }
}
