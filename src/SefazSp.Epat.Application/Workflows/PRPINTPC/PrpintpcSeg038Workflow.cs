#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.PRPINTPC;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.PRPINTPC;

/// <summary>
/// Resultado do segmento 038 do processo PRPINTPC (passos 1–18, cenário SC-PRPINTPC-007).
/// </summary>
public enum PrpintpcSeg038Outcome
{
    /// <summary>
    /// Percurso concluiu em Try Task (_KEwC4V6EEfGBBLgT-R5iuw, linkCatch).
    /// Retentativa agendada via timerEvent Pause → linkThrow → linkCatch (flatten-edge,
    /// NOEQ-link-goto, ratificado 2026-08-06).
    /// </summary>
    TryTask,

    /// <summary>
    /// Erro aplicacional sem retentativa disponível — encerrou no endEvent
    /// _KEwDU16EEfGBBLgT-R5iuw dentro do subProcessScope.
    /// </summary>
    AppErrorEnd,

    /// <summary>
    /// Retentativas do motor esgotadas (SW_QRETRYCOUNT &gt;= MAXRETRIES).
    /// </summary>
    RetriesMaxed,
}

/// <summary>
/// Workflow do segmento 038 de PRPINTPC: de 'Start Event' até 'Try Task'.
///
/// Card: BUILD-PRPINTPC-seg038 · Processo: PRPINTPC · Etapas: 3, 4
/// Cenário de referência: SC-PRPINTPC-007, segmento 1, passos 1–18.
///
/// Topologia dos 18 nós (percurso de referência SC-PRPINTPC-007, segmento 1):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _KEwC3V6EEfGBBLgT-R5iuw  startEvent             │
/// │   ↓ fluxo                                                                        │
/// │  [2]  SetParameters             _KEwC3l6EEfGBBLgT-R5iuw  scriptTask             │
/// │   ↓ fluxo                                                                        │
/// │  [3]  Start Loop                _KEwC4F6EEfGBBLgT-R5iuw  scriptTask             │
/// │   ↓ fluxo                                                                        │
/// │  [4]  Control System Task Call  _KEwC7l6EEfGBBLgT-R5iuw  subProcessScope        │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC3                            │
/// │       ┌─ ActivitySet scope ───────────────────────────────────────────────────┐  │
/// │       │ [5]  startEvent interno  _KEwDUl6EEfGBBLgT-R5iuw  startEvent         │  │
/// │       │  ↓ fluxo                                                             │  │
/// │       │ [6]  Start TX            _KEwDUF6EEfGBBLgT-R5iuw  scriptTask        │  │
/// │       │  ↓ fluxo                                                             │  │
/// │       │ [7]  Check Retries       _KEwDUV6EEfGBBLgT-R5iuw  gateway           │  │
/// │       │       ↓ Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES) — AC4              │  │
/// │       │ [8]  CaptaParametros     _KEwDWF6EEfGBBLgT-R5iuw  serviceTask       │  │
/// │       │  ↓ fluxo                                                             │  │
/// │       │ [9]  gateway             _KEwDVl6EEfGBBLgT-R5iuw  gateway           │  │
/// │       │       ramo AppError (STATUS_CODE!="0") ↓ — AC5, rulings.CLONE-PRPINTPC  │
/// │       │ [10] Set App Error       _KEwDVV6EEfGBBLgT-R5iuw  scriptTask        │  │
/// │       │  ↓ fluxo                                                             │  │
/// │       │ [11] gateway             _KEwDV16EEfGBBLgT-R5iuw  gateway           │  │
/// │       │  ↓ fluxo (→ endEvent)                                                │  │
/// │       │ [12] endEvent            _KEwDU16EEfGBBLgT-R5iuw  endEvent          │  │
/// │       └───────────────────────────────────────────────────────────────────────┘  │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC6                          │
/// │  [13] Tech Error                _KEwC7V6EEfGBBLgT-R5iuw  gateway               │
/// │   ↓ ramo "No" (otherwise, ISTECHERROR!="Y")                                     │
/// │  [14] App Error                 _KEwC7F6EEfGBBLgT-R5iuw  gateway               │
/// │   ↓ ramo "Yes" (ISAPPERROR=="Y")                                                │
/// │  [15] More Retries              _KEwC6V6EEfGBBLgT-R5iuw  gateway               │
/// │   ↓ ramo "Yes" (NUMAPPRETRIES &lt; MAXRETRIES)                                  │
/// │  [16] Pause                     _KEwC6l6EEfGBBLgT-R5iuw  timerEvent            │
/// │   ↓ fluxo (após timer)                                                          │
/// │  [17] Link To: Try Task         _KEwC6F6EEfGBBLgT-R5iuw  linkThrow             │
/// │        ↓ LINK EXPLÍCITO (flatten-edge, NOEQ-link-goto, ratificado 2026-08-06) — AC7 │
/// │  [18] Try Task                  _KEwC4V6EEfGBBLgT-R5iuw  linkCatch             │
/// └──────────────────────────────────────────────────────────────────────────────────┘
///
/// Nó 19 (Set Technical Error, _KEwDVF6EEfGBBLgT-R5iuw) existe no XPDL mas não é
/// visitado no percurso SC-PRPINTPC-007; é alcançado em SC-PRPINTPC-013.
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (AC3)
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error (AC6)
///   • ordem 18 · link      · linkThrow → linkCatch (flatten-edge, AC7)
///
/// ATENÇÃO — correcção de defeito documentada (rulings.CLONE-PRPINTPC):
///   Gateway _KEwDVl6EEfGBBLgT-R5iuw (nó 9) usa STATUS_CODE != "0".
///   O XPDL legado comparava com SW_NA. Esta correcção muda comportamento observado.
///
/// IClock é sempre injectado — nunca DateTime.Now (AC8).
/// </summary>
public sealed class PrpintpcSeg038Workflow
{
    // ── Identificadores de nó — invariantes ──────────────────────────────────

    /// <summary>Nó 1 — Start Event (startEvent, MAIN).</summary>
    public const string NodeStartEvent            = "_KEwC3V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 2 — SetParameters (scriptTask, MAIN). Regra: RI-script-PRPINTPC-SetParameters.</summary>
    public const string NodeSetParameters         = "_KEwC3l6EEfGBBLgT-R5iuw";

    /// <summary>Nó 3 — Start Loop (scriptTask, MAIN). Regra: RI-script-PRPINTPC-StartLoop.</summary>
    public const string NodeStartLoop             = "_KEwC4F6EEfGBBLgT-R5iuw";

    /// <summary>Nó 4 — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeControlSystemTaskCall = "_KEwC7l6EEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó 5 — startEvent interno (ActivitySet). Alcançado por DESCIDA explícita (AC3).
    /// Esta aresta NÃO existe no XPDL; escrita explicitamente.
    /// </summary>
    public const string NodeInnerStartEvent       = "_KEwDUl6EEfGBBLgT-R5iuw";

    /// <summary>Nó 6 — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx               = "_KEwDUF6EEfGBBLgT-R5iuw";

    /// <summary>Nó 7 — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-PRPINTPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries          = "_KEwDUV6EEfGBBLgT-R5iuw";

    /// <summary>Nó 8 — CaptaParametros (serviceTask, ActivitySet).</summary>
    public const string NodeCaptaParametros       = "_KEwDWF6EEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó 9 — gateway (ActivitySet). "A chamada a CaptaParametros foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0" (correcção rulings.CLONE-PRPINTPC, AC5).
    /// </summary>
    public const string NodeGatewayStatusCheck    = "_KEwDVl6EEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError           = "_KEwDVV6EEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — gateway anónimo de convergência (ActivitySet).</summary>
    public const string NodeGatewayConverge       = "_KEwDV16EEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — endEvent interno (ActivitySet).</summary>
    public const string NodeInnerEndEvent         = "_KEwDU16EEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó 13 — Tech Error (gateway, MAIN). Alcançado por REGRESSO explícito (AC6).
    /// Esta aresta NÃO existe no XPDL; escrita explicitamente.
    /// </summary>
    public const string NodeTechError             = "_KEwC7V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError              = "_KEwC7F6EEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries           = "_KEwC6V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 16 — Pause (timerEvent, MAIN). Timer de espera entre retentativas. Usa IClock (AC8).</summary>
    public const string NodePause                 = "_KEwC6l6EEfGBBLgT-R5iuw";

    /// <summary>Nó 17 — Link To: Try Task (linkThrow, MAIN).</summary>
    public const string NodeLinkThrow             = "_KEwC6F6EEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó 18 — Try Task (linkCatch, MAIN). Alcançado por LINK explícito (AC7).
    /// Implementado como flatten-edge (NOEQ-link-goto, ratificado 2026-08-06).
    /// Esta aresta NÃO existe no XPDL; escrita explicitamente.
    /// </summary>
    public const string NodeLinkCatch             = "_KEwC4V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 19 — Set Technical Error (scriptTask). Visitado noutra passagem (SC-PRPINTPC-013), não neste percurso.</summary>
    public const string NodeSetTechnicalError     = "_KEwDVF6EEfGBBLgT-R5iuw";

    // ── Dependências ──────────────────────────────────────────────────────────

    private readonly ICaptaParametrosSoapService _captaParametros;
    private readonly IClock _clock;

    /// <param name="captaParametros">
    /// Porta do serviço CaptaParametros (SOAP). A implementação concreta fica em
    /// <c>Infrastructure/Integration.Soap/CaptaParametrosSoapService</c>.
    /// </param>
    /// <param name="clock">
    /// Fonte de tempo controlável para o timerEvent Pause (nó 16).
    /// Nunca usar <c>DateTime.Now</c> directamente (AC8, IClock, Domain/Abstractions, status final).
    /// </param>
    public PrpintpcSeg038Workflow(
        ICaptaParametrosSoapService captaParametros,
        IClock clock)
    {
        _captaParametros = captaParametros;
        _clock           = clock;
    }

    /// <summary>
    /// Executa o segmento 038 completo, percorrendo os passos 1–18 do cenário SC-PRPINTPC-007.
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Desfecho do percurso.</returns>
    public async Task<PrpintpcSeg038Outcome> ExecuteAsync(
        AiimCaseRef caseRef,
        CancellationToken ct)
    {
        var ctx = new ProcessExecutionContext();
        return await RunAsync(caseRef, ctx, swQRetryCount: 0L, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Percurso completo, exposto para testes com contexto e SW_QRETRYCOUNT controláveis.
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ctx">Contexto de execução mutável (pode ser pré-populado em testes).</param>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime iProcess.
    ///   Em .NET é injectado — nunca escrito pelo processo (NOEQ-iprocess-builtin).
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task<PrpintpcSeg038Outcome> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_KEwC3V6EEfGBBLgT-R5iuw) ──────────────
        // Ponto de entrada. Sem efeito lateral.

        // ── Nó 2: scriptTask 'SetParameters' (_KEwC3l6EEfGBBLgT-R5iuw) ─────────────
        // Regra: RI-script-PRPINTPC-SetParameters (eRegraDeNegocio = true) — AC1.
        // IDPROCESSO comparado com SW_NA: usa FieldValue<long> (shim-tri-state,
        // NOEQ-iprocess-builtin, ratificado 2026-08-06). SW_NA ≠ null.
        var idProcesso = FieldValue<long>.NotAvailable; // SW_NA: IDPROCESSO não preenchido na entrada
        if (PrpintpcSetParametersRule.ShouldInitialize(idProcesso, ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES))
            PrpintpcSeg038Steps.ApplySetParameters(ctx);

        // ── Nó 3: scriptTask 'Start Loop' (_KEwC4F6EEfGBBLgT-R5iuw) ──────────────
        // Regra: RI-script-PRPINTPC-StartLoop — AC2.
        // Inicializa NUMAPPRETRIES=0 quando null.
        PrpintpcSeg038Steps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_KEwC7l6EEfGBBLgT-R5iuw)
        // ── Nó 5: startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw, descida) ──────────
        // DESCIDA EXPLÍCITA: o XPDL não traz transição para o startEvent interno.
        // NodeControlSystemTaskCall ──descida──► NodeInnerStartEvent
        // Escrita explicitamente como aresta .NET — AC3.

        var subOutcome = await ExecuteSubProcessScopeAsync(caseRef, ctx, swQRetryCount, ct)
            .ConfigureAwait(false);

        if (subOutcome == SubProcessOutcome.RetriesMaxed)
            return PrpintpcSeg038Outcome.RetriesMaxed;

        // ── Nó 13: gateway 'Tech Error' (_KEwC7V6EEfGBBLgT-R5iuw) ────────────────
        // Alcançado por REGRESSO EXPLÍCITO: o XPDL não traz a aresta de retorno do
        // subProcessScope ao escopo MAIN após erro. Escrita explicitamente — AC6.
        // Ramo "No" (otherwise): ISTECHERROR != "Y" → App Error.
        // (Ramo "Yes" de Tech Error leva a Set Technical Error _KEwDVF6EEfGBBLgT-R5iuw,
        //  que é visitado em SC-PRPINTPC-013 mas não neste percurso.)
        if (PrpintpcSeg038Steps.IsTechError(ctx))
        {
            // Ramo tech-error: nó 19 (Set Technical Error) — percurso de SC-PRPINTPC-013.
            // Não alcançado no percurso SC-PRPINTPC-007; o nó existe e é acessível.
            return PrpintpcSeg038Outcome.AppErrorEnd;
        }

        // ── Nó 14: gateway 'App Error' (_KEwC7F6EEfGBBLgT-R5iuw) ─────────────────
        // Ramo "Yes": ISAPPERROR == "Y" → More Retries.
        if (!PrpintpcSeg038Steps.IsAppErrorFlag(ctx))
        {
            // Ramo "No": sem erro; o fluxo continua para nós fora deste segmento.
            return PrpintpcSeg038Outcome.AppErrorEnd;
        }

        // ── Nó 15: gateway 'More Retries' (_KEwC6V6EEfGBBLgT-R5iuw) ─────────────
        // Ramo "Yes": NUMAPPRETRIES < MAXRETRIES → Pause.
        if (!PrpintpcSeg038Steps.HasMoreRetries(ctx))
        {
            // Retentativas esgotadas — sem ramo Pause.
            return PrpintpcSeg038Outcome.AppErrorEnd;
        }

        // ── Nó 16: timerEvent 'Pause' (_KEwC6l6EEfGBBLgT-R5iuw) ─────────────────
        // IClock injectado — nunca DateTime.Now (AC8).
        await PauseAsync(_clock, ct).ConfigureAwait(false);

        // ── Nó 17: linkThrow 'Link To: Try Task' (_KEwC6F6EEfGBBLgT-R5iuw) ──────
        // ── Nó 18: linkCatch 'Try Task' (_KEwC4V6EEfGBBLgT-R5iuw) ─────────────
        // LINK EXPLÍCITO: flatten-edge (NOEQ-link-goto, ratificado 2026-08-06) — AC7.
        // linkThrow ──link──► linkCatch: aresta não existe no XPDL; escrita explicitamente.
        return PrpintpcSeg038Outcome.TryTask;
    }

    // ── Escopo do subProcessScope (ActivitySet) ───────────────────────────────

    private async Task<SubProcessOutcome> ExecuteSubProcessScopeAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 5: startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw) ──────────────
        // Alcançado por DESCIDA explícita (AC3, entrouPor=descida).

        // ── Nó 6: scriptTask 'Start TX' (_KEwDUF6EEfGBBLgT-R5iuw) ────────────
        PrpintpcSeg038Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_KEwDUV6EEfGBBLgT-R5iuw)
        // Regra: RI-transition-PRPINTPC-CheckRetriesSWQRETRYCOUNT — AC4.
        // SW_QRETRYCOUNT: contador técnico do motor, distinto de NUMAPPRETRIES.
        if (!PrpintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Ramo Maxretriesexceeded.
            return SubProcessOutcome.RetriesMaxed;
        }

        // ── Nó 8: serviceTask 'CaptaParametros' (_KEwDWF6EEfGBBLgT-R5iuw) ────
        ServiceEnvelope envelope;
        try
        {
            envelope = await _captaParametros.InvokeAsync(caseRef, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Excepção de transporte → REGRESSO para Tech Error (nó 13).
            // Aresta não existe no XPDL; escrita explicitamente (AC6).
            ctx.ISTECHERROR = "Y";
            ctx.STATUS_CODE  = ex.Message;
            ctx.STERRORDESC  = ex.Message;
            // Queda para nó 12 (endEvent) → regresso implícito ao MAIN scope.
            return SubProcessOutcome.Done;
        }

        // ── Nó 9: gateway _KEwDVl6EEfGBBLgT-R5iuw ───────────────────────────
        // "A chamada a CaptaParametros foi bem sucedida?"
        // Ramo AppError: STATUS_CODE != "0" — correcção rulings.CLONE-PRPINTPC (AC5).
        // NOTA: o XPDL legado comparava com SW_NA; esta implementação usa "0" conforme decisão.
        if (!PrpintpcSeg038Steps.IsAppError(ctx))
        {
            // Ramo Good (STATUS_CODE == "0"): subprocesso bem-sucedido.
            PrpintpcSeg038Steps.MapServiceEnvelope(ctx, envelope);
            return SubProcessOutcome.Done;
        }

        // ── Nó 10: scriptTask 'Set App Error' (_KEwDVV6EEfGBBLgT-R5iuw) ──────
        PrpintpcSeg038Steps.SetAppError(ctx, envelope);

        // ── Nó 11: gateway _KEwDV16EEfGBBLgT-R5iuw ──────────────────────────
        // Gateway anónimo de convergência; encaminha para endEvent.

        // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ─────────────────────────
        // Subprocesso encerra; regresso ao MAIN scope via aresta explícita (AC6).
        return SubProcessOutcome.Done;
    }

    private static async Task PauseAsync(IClock clock, CancellationToken ct)
    {
        // timerEvent Pause: espera antes de re-tentar (AC8).
        // IClock injectado garante testabilidade com relógio controlável.
        var pauseDuration = TimeSpan.FromMinutes(1);
        var deadline = clock.Now.Add(pauseDuration);
        var remaining = deadline - clock.Now;
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining, ct).ConfigureAwait(false);
    }

    private enum SubProcessOutcome
    {
        Done,
        RetriesMaxed,
    }
}
