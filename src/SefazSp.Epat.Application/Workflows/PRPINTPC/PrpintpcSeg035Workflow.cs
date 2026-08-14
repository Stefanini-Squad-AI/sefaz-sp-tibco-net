#nullable enable

using System.Globalization;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.PRPINTPC;
using SefazSp.Epat.Application.UseCases.PRPINTPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.PRPINTPC;

/// <summary>
/// Resultado possível do percurso do segmento 035 do processo PRPINTPC.
/// </summary>
public enum PrpintpcSeg035Outcome
{
    /// <summary>
    /// Caso resolvido manualmente — fluxo encerrou via gateway Manually Fixed.
    /// Operador definiu OUTCOME = "OK".
    /// </summary>
    ManuallyFixed,

    /// <summary>
    /// Chamada bem sucedida antes de esgotar as retentativas — STATUS_CODE = "0".
    /// Nenhum erro de aplicação ou técnico foi detectado.
    /// </summary>
    Success,

    /// <summary>
    /// Retentativas esgotadas e operador não optou por tentar novamente —
    /// fluxo encerrou no endEvent 'Done - Bail' (_KEwC4l6EEfGBBLgT-R5iuw).
    /// </summary>
    DoneBail,
}

/// <summary>
/// Workflow do segmento 035 do processo PRPINTPC: de 'Start Event' a 'Done - Bail'.
///
/// Card: BUILD-PRPINTPC-seg035 · Processo: PRPINTPC · Etapas: 3, 4
/// Cenário de referência: SC-PRPINTPC-009, segmento 1, passos 1–20.
///
/// Herdado de POC_EpatProcess/Prepara Intimação.
///
/// Topologia dos 20 nós (percurso de referência SC-PRPINTPC-009, segmento 1):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _KEwC3V6EEfGBBLgT-R5iuw  startEvent          │
/// │   ↓ fluxo                                                                     │
/// │  [2]  SetParameters             _KEwC3l6EEfGBBLgT-R5iuw  scriptTask          │
/// │        Regra: RI-script-PRPINTPC-SetParameters                                │
/// │        NOEQ-iprocess-builtin: shim-tri-state (SW_NA), ratificado 2026-08-06   │
/// │   ↓ fluxo                                                                     │
/// │  [3]  Start Loop                _KEwC4F6EEfGBBLgT-R5iuw  scriptTask          │
/// │        Regra: RI-script-PRPINTPC-StartLoop                                    │
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _KEwC7l6EEfGBBLgT-R5iuw  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC3                         │
/// │       ┌─ ActivitySet scope ─────────────────────────────────────────────────┐  │
/// │       │ [5]  startEvent interno  _KEwDUl6EEfGBBLgT-R5iuw  startEvent       │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [6]  Start TX            _KEwDUF6EEfGBBLgT-R5iuw  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [7]  Check Retries       _KEwDUV6EEfGBBLgT-R5iuw  gateway         │  │
/// │       │       Regra: RI-transition-PRPINTPC-CheckRetriesSWQRETRYCOUNT      │  │
/// │       │       ↓ Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES)                  │  │
/// │       │ [8]  CaptaParametros     _KEwDWF6EEfGBBLgT-R5iuw  serviceTask     │  │
/// │       │       Operação: PrepararintimacaoAsync (DecisionsEPAT.wsdl)        │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [9]  gateway             _KEwDVl6EEfGBBLgT-R5iuw  gateway         │  │
/// │       │       "A chamada a CaptaParametros foi bem sucedida?"               │  │
/// │       │       ramo AppError: STATUS_CODE != "0" ↓                          │  │
/// │       │       ATENÇÃO — defeito de cópia corrigido (rulings.CLONE-PRPINTPC)│  │
/// │       │       XPDL original: STATUS_CODE != SW_NA                          │  │
/// │       │       Corrigido para: STATUS_CODE != "0"                           │  │
/// │       │ [10] Set App Error       _KEwDVV6EEfGBBLgT-R5iuw  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [11] gateway             _KEwDV16EEfGBBLgT-R5iuw  gateway         │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [12] endEvent            _KEwDU16EEfGBBLgT-R5iuw  endEvent        │  │
/// │       └────────────────────────────────────────────────────────────────────┘  │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC6                       │
/// │  [13] Tech Error                _KEwC7V6EEfGBBLgT-R5iuw  gateway             │
/// │        ramo "No" (otherwise): → App Error                                     │
/// │   ↓ ramo "No"                                                                 │
/// │  [14] App Error                 _KEwC7F6EEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": ISAPPERROR == "Y" → More Retries                           │
/// │   ↓ ramo "Yes"                                                                │
/// │  [15] More Retries              _KEwC6V6EEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES → Start Loop (retry)          │
/// │        ramo "No" (otherwise): retentativas esgotadas → Manipular Excecao      │
/// │   ↓ ramo "No"                                                                 │
/// │  [16] gateway                   _KEwC716EEfGBBLgT-R5iuw  gateway             │
/// │        (convergência Tech Error + More Retries esgotadas)                     │
/// │   ↓ fluxo                                                                     │
/// │  [17] Manipular Excecao         _KEwC5V6EEfGBBLgT-R5iuw  userTask            │
/// │   ↓ fluxo                                                                     │
/// │  [18] Manually Fixed            _KEwC316EEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": OUTCOME == "OK" → saída manual                             │
/// │        ramo "No" (otherwise) → Try Again                                      │
/// │   ↓ ramo "No"                                                                 │
/// │  [19] Try Again                 _KEwC5F6EEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": OUTCOME == "R" → volta ao nó 3 (Start Loop)                │
/// │        ramo "No" (otherwise) → Done - Bail                                    │
/// │   ↓ ramo "No"                                                                 │
/// │  [20] Done - Bail               _KEwC4l6EEfGBBLgT-R5iuw  endEvent            │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw)
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error (_KEwC7V6EEfGBBLgT-R5iuw)
///
/// Impacto da correcção CLONE-PRPINTPC:
///   Casos que antes passavam com STATUS_CODE = SW_NA (terceiro estado — "não disponível")
///   passavam silenciosamente como sucesso. Com a condição corrigida STATUS_CODE != "0",
///   qualquer STATUS_CODE != "0" activa o ramo AppError, incluindo SW_NA.
///   Isto alinha o comportamento com os processos irmãos ATZINTPC, BSCENVPC, CALCPRPC e CRNOTPC.
/// </summary>
public sealed class PrpintpcSeg035Workflow
{
    // ── Identificadores de nó — invariantes (não renomear) ───────────────────

    /// <summary>Nó 1  — Start Event (ponto de entrada, MAIN).</summary>
    public const string NodeStartEvent         = "_KEwC3V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-PRPINTPC-SetParameters.</summary>
    public const string NodeSetParameters      = "_KEwC3l6EEfGBBLgT-R5iuw";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN). Regra: RI-script-PRPINTPC-StartLoop.</summary>
    public const string NodeStartLoop          = "_KEwC4F6EEfGBBLgT-R5iuw";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeSubProcessScope    = "_KEwC7l6EEfGBBLgT-R5iuw";

    /// <summary>Nó 5  — startEvent interno (descida explícita, ActivitySet).</summary>
    public const string NodeStartEventInternal = "_KEwDUl6EEfGBBLgT-R5iuw";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx            = "_KEwDUF6EEfGBBLgT-R5iuw";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-PRPINTPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries       = "_KEwDUV6EEfGBBLgT-R5iuw";

    /// <summary>Nó 8  — CaptaParametros (serviceTask, ActivitySet). Operação: PrepararintimacaoAsync.</summary>
    public const string NodeCaptaParametros    = "_KEwDWF6EEfGBBLgT-R5iuw";

    /// <summary>Nó 9  — gateway (ActivitySet). Decisão: A chamada a CaptaParametros foi bem sucedida? (corrigida: STATUS_CODE != "0").</summary>
    public const string NodeGatewayCallResult  = "_KEwDVl6EEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError        = "_KEwDVV6EEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — gateway de convergência interno (ActivitySet).</summary>
    public const string NodeGatewayInnerMerge  = "_KEwDV16EEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — endEvent interno (ActivitySet).</summary>
    public const string NodeEndEventInternal   = "_KEwDU16EEfGBBLgT-R5iuw";

    /// <summary>Nó 13 — Tech Error (gateway, MAIN). Alcançado por regresso explícito.</summary>
    public const string NodeTechError          = "_KEwC7V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError           = "_KEwC7F6EEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries        = "_KEwC6V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 16 — gateway de convergência (MAIN). Une ramos Tech Error + More Retries esgotadas.</summary>
    public const string NodeGatewayMerge       = "_KEwC716EEfGBBLgT-R5iuw";

    /// <summary>Nó 17 — Manipular Excecao (userTask, MAIN).</summary>
    public const string NodeManipularExcecao   = "_KEwC5V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 18 — Manually Fixed (gateway, MAIN).</summary>
    public const string NodeManuallyFixed      = "_KEwC316EEfGBBLgT-R5iuw";

    /// <summary>Nó 19 — Try Again (gateway, MAIN).</summary>
    public const string NodeTryAgain           = "_KEwC5F6EEfGBBLgT-R5iuw";

    /// <summary>Nó 20 — Done - Bail (endEvent, MAIN).</summary>
    public const string NodeDoneBail           = "_KEwC4l6EEfGBBLgT-R5iuw";

    /// <summary>Nó auxiliar — Set Technical Error (scriptTask, ActivitySet). Visitado noutros percursos.</summary>
    public const string NodeSetTechnicalError  = "_KEwDVF6EEfGBBLgT-R5iuw";

    // ─────────────────────────────────────────────────────────────────────────

    private readonly IEpatServices _services;
    private readonly ManipularExcecaoPrpintpcUseCase _manipularExcecao;

    public PrpintpcSeg035Workflow(
        IEpatServices services,
        ManipularExcecaoPrpintpcUseCase manipularExcecao)
    {
        _services         = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento completo (passos 1–20 do cenário SC-PRPINTPC-009),
    /// incluindo o laço de retry e a resolução manual.
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="swQRetryCount">
    ///   Valor de <c>IPESystemValues.SW_QRETRYCOUNT</c> fornecido pelo runtime iProcess.
    ///   Lido pelo gateway Check Retries; nunca escrito pelo processo.
    ///   NOEQ-iprocess-builtin, ratificado 2026-08-06.
    /// </param>
    /// <param name="decideManipularExcecao">
    ///   Delegate de interacção humana para a userTask 'Manipular Excecao' (_KEwC5V6EEfGBBLgT-R5iuw).
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<PrpintpcSeg035Outcome> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoPrpintpcResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_KEwC3V6EEfGBBLgT-R5iuw) ─────────
        // Ponto de entrada. Sem efeito lateral. Controlo passa ao nó 2.

        // ── Nó 2: scriptTask 'SetParameters' (_KEwC3l6EEfGBBLgT-R5iuw) ───────
        // Regra: RI-script-PRPINTPC-SetParameters.
        // NOEQ-iprocess-builtin: IDPROCESSO comparado com SW_NA via FieldValue<T> (shim-tri-state).
        // SW_NA NUNCA é mapeado para null — FieldValue<T>.NotAvailable é o terceiro estado.
        var idProcesso = ParseIdProcesso(caseRef.ProcessId);
        if (PrpintpcSetParametersRule.ShouldInitialize(idProcesso, ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES))
            PrpintpcSeg035Steps.ApplySetParameters(ctx, caseRef.ProcessId);

        StartLoopEntry:

        // ── Nó 3: scriptTask 'Start Loop' (_KEwC4F6EEfGBBLgT-R5iuw) ──────────
        // Regra: RI-script-PRPINTPC-StartLoop.
        PrpintpcSeg035Steps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_KEwC7l6EEfGBBLgT-R5iuw) ──
        // ── Nó 5: startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw) ─────────────────
        // DESCIDA EXPLÍCITA: não existe transição XPDL do subProcessScope para o startEvent
        // interno. A aresta é escrita explicitamente neste workflow (AC3).
        {
            var subResult = await ExecuteSubProcessScopeAsync(caseRef, ctx, swQRetryCount, ct);

            if (subResult == SubProcessResult.TechError)
            {
                // ── Nó 13: gateway 'Tech Error' (_KEwC7V6EEfGBBLgT-R5iuw) ────
                // REGRESSO EXPLÍCITO: não existe transição XPDL do endEvent interno de volta ao MAIN.
                // Alcançado via excepção de transporte ou esgotamento de SW_QRETRYCOUNT (AC6).
                // Ramo "No" (otherwise): → Nó 16 (convergência antes de Manipular Excecao).
                goto ManipularExcecaoEntry;
            }

            if (subResult == SubProcessResult.AppError)
            {
                // ── Nó 13: gateway 'Tech Error' (_KEwC7V6EEfGBBLgT-R5iuw) ────
                // Regresso explícito. ISTECHERROR = "N" → ramo "No" (otherwise).
                // ── Nó 14: gateway 'App Error' (_KEwC7F6EEfGBBLgT-R5iuw) ─────
                // Ramo "Yes": ISAPPERROR == "Y".
                // ── Nó 15: gateway 'More Retries' (_KEwC6V6EEfGBBLgT-R5iuw) ──
                // Ramo "Yes": NUMAPPRETRIES < MAXRETRIES → volta ao laço.
                if (PrpintpcSeg035Steps.HasMoreRetries(ctx))
                    goto StartLoopEntry;

                // Ramo "No" (otherwise): retentativas esgotadas.
                // ── Nó 16: gateway _KEwC716EEfGBBLgT-R5iuw (convergência) ────
                goto ManipularExcecaoEntry;
            }

            // SubProcessResult.Success: STATUS_CODE = "0", sem erro.
            // ── Nó 13: gateway 'Tech Error' → ramo "No".
            // ── Nó 14: gateway 'App Error' → ramo "No" (ISAPPERROR != "Y").
            return PrpintpcSeg035Outcome.Success;
        }

        ManipularExcecaoEntry:

        // ── Nó 17: userTask 'Manipular Excecao' (_KEwC5V6EEfGBBLgT-R5iuw) ────
        await _manipularExcecao
            .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
            .ConfigureAwait(false);

        // ── Nó 18: gateway 'Manually Fixed' (_KEwC316EEfGBBLgT-R5iuw) ─────────
        if (PrpintpcSeg035Steps.IsManuallyFixed(ctx))
        {
            // OUTCOME = "OK" → caso resolvido manualmente.
            return PrpintpcSeg035Outcome.ManuallyFixed;
        }

        // ── Nó 19: gateway 'Try Again' (_KEwC5F6EEfGBBLgT-R5iuw) ────────────
        if (PrpintpcSeg035Steps.IsTryAgain(ctx))
        {
            // OUTCOME = "R" → operador opta por repetir; reinicia o laço.
            ctx.NUMAPPRETRIES = 0;
            goto StartLoopEntry;
        }

        // ── Nó 20: endEvent 'Done - Bail' (_KEwC4l6EEfGBBLgT-R5iuw) ─────────
        // Ramo "No" de Try Again (OTHERWISE): encerra por esgotamento sem resolução.
        return PrpintpcSeg035Outcome.DoneBail;
    }

    // ── Execução do subProcessScope (ActivitySet) ─────────────────────────────

    private async Task<SubProcessResult> ExecuteSubProcessScopeAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 6: scriptTask 'Start TX' (_KEwDUF6EEfGBBLgT-R5iuw) ─────────────
        PrpintpcSeg035Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_KEwDUV6EEfGBBLgT-R5iuw) ──
        // Regra: RI-transition-PRPINTPC-CheckRetriesSWQRETRYCOUNT.
        // Ramo "Stillgood": SW_QRETRYCOUNT < MAXRETRIES → prossegue para CaptaParametros.
        // Ramo oposto: motor esgotado → SetTechError e termina o ActivitySet.
        if (!PrpintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            PrpintpcSeg035Steps.SetTechError(ctx, "Maxretriesexceeded");
            // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ────────────────────
            // queda para regresso ao escopo MAIN (regresso explícito AC6)
            return SubProcessResult.TechError;
        }

        ServiceEnvelope envelope;
        try
        {
            // ── Nó 8: serviceTask 'CaptaParametros' (_KEwDWF6EEfGBBLgT-R5iuw) ──
            // Operação: PrepararintimacaoAsync (DecisionsEPAT.wsdl)
            envelope = await _services
                .PrepararintimacaoAsync(caseRef, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Excepção de transporte: não existe transição XPDL para o gateway Tech Error.
            // A aresta de REGRESSO é escrita explicitamente aqui (AC6).
            PrpintpcSeg035Steps.SetTechError(ctx, ex.Message);
            return SubProcessResult.TechError;
        }

        // ── Nó 9: gateway _KEwDVl6EEfGBBLgT-R5iuw ───────────────────────────
        // "A chamada a CaptaParametros foi bem sucedida?"
        // ATENÇÃO — defeito de cópia corrigido (rulings.CLONE-PRPINTPC, AC5):
        //   XPDL original: STATUS_CODE != IPESystemValues.SW_NA
        //   Corrigido para: STATUS_CODE != "0"   (alinhado com processos irmãos)
        if (PrpintpcSeg035Steps.IsAppError(envelope))
        {
            // ── Nó 10: scriptTask 'Set App Error' (_KEwDVV6EEfGBBLgT-R5iuw) ──
            PrpintpcSeg035Steps.SetAppError(ctx, envelope);
            // ── Nó 11: gateway _KEwDV16EEfGBBLgT-R5iuw ─────────────────────
            // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ─────────────────────
            return SubProcessResult.AppError;
        }

        // STATUS_CODE == "0": chamada bem sucedida.
        PrpintpcSeg035Steps.MapServiceEnvelopeSuccess(ctx, envelope);
        // ── Nó 11: gateway _KEwDV16EEfGBBLgT-R5iuw ──────────────────────────
        // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ─────────────────────────
        return SubProcessResult.Success;
    }

    // ── Auxiliares ────────────────────────────────────────────────────────────

    /// <summary>
    /// Extrai e classifica o campo IDPROCESSO do ProcessId do caso usando o shim tri-state.
    /// SW_NA nunca é mapeado para null; é representado como <see cref="FieldValue{T}.NotAvailable"/>.
    /// </summary>
    private static FieldValue<long> ParseIdProcesso(string processId)
    {
        const string marker = "idProc-";
        var markerIndex = processId.LastIndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return FieldValue<long>.Empty;

        var rawValue = processId[(markerIndex + marker.Length)..];

        if (string.Equals(rawValue, "NA", StringComparison.OrdinalIgnoreCase))
            return FieldValue<long>.NotAvailable;

        return long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? FieldValue<long>.Of(value)
            : FieldValue<long>.Empty;
    }

    /// <summary>Resultado interno do subProcessScope 'Control System Task Call'.</summary>
    private enum SubProcessResult
    {
        Success,
        AppError,
        TechError,
    }
}
