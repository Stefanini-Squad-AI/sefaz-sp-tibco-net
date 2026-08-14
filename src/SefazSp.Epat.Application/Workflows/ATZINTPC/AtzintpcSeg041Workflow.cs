#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.ATZINTPC;
using SefazSp.Epat.Application.UseCases.ATZINTPC;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Workflows.ATZINTPC;

/// <summary>
/// Resultado possível do percurso do segmento 041 do processo ATZINTPC.
/// </summary>
public enum AtzintpcSeg041Outcome
{
    /// <summary>
    /// Caso resolvido manualmente — fluxo encerrou via gateway Manually Fixed.
    /// Operador definiu OUTCOME = "OK".
    /// </summary>
    ManuallyFixed,

    /// <summary>
    /// Chamada bem sucedida antes de esgotar as retentativas — STATUS_CODE = "0".
    /// </summary>
    Success,

    /// <summary>
    /// Retentativas esgotadas e operador não optou por tentar novamente —
    /// fluxo encerrou no endEvent 'Done - Bail' (_RNdJzl6PEfGBBLgT-R5iuw).
    /// </summary>
    DoneBail,
}

/// <summary>
/// Workflow do segmento 041 do processo ATZINTPC: de 'Start Event' a 'Done - Bail'.
///
/// Card: BUILD-ATZINTPC-seg041 · Processo: ATZINTPC · Etapa: 4
/// Cenário de referência: SC-ATZINTPC-009, segmento 1, passos 1–20.
///
/// Herdado de CONTROPC/AtualizaIntimacao.
///
/// Topologia dos 20 nós (percurso de referência SC-ATZINTPC-009, segmento 1):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _RNdJyV6PEfGBBLgT-R5iuw  startEvent          │
/// │   ↓ fluxo                                                                     │
/// │  [2]  SetParameters             _RNdJyl6PEfGBBLgT-R5iuw  scriptTask          │
/// │        Regra: RI-script-ATZINTPC-SetParameters                                │
/// │        NOEQ-iprocess-builtin: shim-tri-state (SW_NA), ratificado 2026-08-06   │
/// │   ↓ fluxo                                                                     │
/// │  [3]  Start Loop                _RNdJzF6PEfGBBLgT-R5iuw  scriptTask          │
/// │        NOEQ-iprocess-builtin: shim-tri-state (SW_DATE), ratificado 2026-08-06 │
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _RNdJ2l6PEfGBBLgT-R5iuw  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC3                         │
/// │       ┌─ ActivitySet scope ─────────────────────────────────────────────────┐  │
/// │       │ [5]  startEvent interno  _RNdKFl6PEfGBBLgT-R5iuw  startEvent       │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [6]  Start TX            _RNdKFF6PEfGBBLgT-R5iuw  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [7]  Check Retries       _RNdKFV6PEfGBBLgT-R5iuw  gateway         │  │
/// │       │       Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT      │  │
/// │       │       ↓ Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES)                  │  │
/// │       │ [8]  AtualizarIntimacao  _RNdKHF6PEfGBBLgT-R5iuw  serviceTask     │  │
/// │       │       Operação: AtualizarintimacaoAsync (EPAT.wsdl)                │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [9]  gateway             _RNdKGl6PEfGBBLgT-R5iuw  gateway         │  │
/// │       │       "A chamada a AtualizarIntimacao foi bem sucedida?"            │  │
/// │       │       ramo AppError: STATUS_CODE != "0" ↓                          │  │
/// │       │ [10] Set App Error       _RNdKGV6PEfGBBLgT-R5iuw  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [11] gateway             _RNdKG16PEfGBBLgT-R5iuw  gateway         │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [12] endEvent            _RNdKF16PEfGBBLgT-R5iuw  endEvent        │  │
/// │       └────────────────────────────────────────────────────────────────────┘  │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC5                       │
/// │  [13] Tech Error                _RNdJ2V6PEfGBBLgT-R5iuw  gateway             │
/// │        ramo "No" (otherwise): → App Error                                     │
/// │   ↓ ramo "No"                                                                 │
/// │  [14] App Error                 _RNdJ2F6PEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": ISAPPERROR == "Y" → More Retries                           │
/// │   ↓ ramo "Yes"                                                                │
/// │  [15] More Retries              _RNdJ1V6PEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES → Start Loop (retry)          │
/// │        ramo "No" (otherwise): retentativas esgotadas → Manipular Excecao      │
/// │   ↓ ramo "No"                                                                 │
/// │  [16] gateway                   _RNdJ216PEfGBBLgT-R5iuw  gateway             │
/// │        (convergência Tech Error + More Retries esgotadas)                     │
/// │   ↓ fluxo                                                                     │
/// │  [17] Manipular Excecao         _RNdJ0V6PEfGBBLgT-R5iuw  userTask            │
/// │   ↓ fluxo                                                                     │
/// │  [18] Manually Fixed            _RNdJy16PEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": OUTCOME == "OK" → saída manual                             │
/// │        ramo "No" (otherwise) → Try Again                                      │
/// │   ↓ ramo "No"                                                                 │
/// │  [19] Try Again                 _RNdJ0F6PEfGBBLgT-R5iuw  gateway             │
/// │        ramo "Yes": OUTCOME == "R" → volta ao nó 3 (Start Loop)                │
/// │        ramo "No" (otherwise) → Done - Bail                                    │
/// │   ↓ ramo "No"                                                                 │
/// │  [20] Done - Bail               _RNdJzl6PEfGBBLgT-R5iuw  endEvent            │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Nó auxiliar (não no percurso de referência SC-ATZINTPC-009):
///   [21] Set Technical Error        _RNdKGF6PEfGBBLgT-R5iuw  scriptTask (SC-ATZINTPC-015)
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (_RNdKFl6PEfGBBLgT-R5iuw)
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error (_RNdJ2V6PEfGBBLgT-R5iuw)
/// </summary>
public sealed class AtzintpcSeg041Workflow
{
    // ── Identificadores de nó — invariantes (não renomear) ───────────────────

    /// <summary>Nó 1  — Start Event (ponto de entrada, MAIN).</summary>
    public const string NodeStartEvent         = "_RNdJyV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-ATZINTPC-SetParameters.</summary>
    public const string NodeSetParameters      = "_RNdJyl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN).</summary>
    public const string NodeStartLoop          = "_RNdJzF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeSubProcessScope    = "_RNdJ2l6PEfGBBLgT-R5iuw";

    /// <summary>Nó 5  — startEvent interno (descida explícita, ActivitySet).</summary>
    public const string NodeStartEventInternal = "_RNdKFl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx            = "_RNdKFF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries       = "_RNdKFV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 8  — AtualizarIntimacao (serviceTask, ActivitySet). Operação: AtualizarintimacaoAsync (EPAT.wsdl).</summary>
    public const string NodeAtualizarIntimacao = "_RNdKHF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 9  — gateway (ActivitySet). Decisão: A chamada a AtualizarIntimacao foi bem sucedida?</summary>
    public const string NodeGatewayCallResult  = "_RNdKGl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError        = "_RNdKGV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — gateway de convergência interno (ActivitySet).</summary>
    public const string NodeGatewayInnerMerge  = "_RNdKG16PEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — endEvent interno (ActivitySet).</summary>
    public const string NodeEndEventInternal   = "_RNdKF16PEfGBBLgT-R5iuw";

    /// <summary>Nó 13 — Tech Error (gateway, MAIN). Alcançado por regresso explícito.</summary>
    public const string NodeTechError          = "_RNdJ2V6PEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError           = "_RNdJ2F6PEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries        = "_RNdJ1V6PEfGBBLgT-R5iuw";

    /// <summary>Nó 16 — gateway de convergência (MAIN). Une ramos Tech Error + More Retries esgotadas.</summary>
    public const string NodeGatewayMerge       = "_RNdJ216PEfGBBLgT-R5iuw";

    /// <summary>Nó 17 — Manipular Excecao (userTask, MAIN).</summary>
    public const string NodeManipularExcecao   = "_RNdJ0V6PEfGBBLgT-R5iuw";

    /// <summary>Nó 18 — Manually Fixed (gateway, MAIN).</summary>
    public const string NodeManuallyFixed      = "_RNdJy16PEfGBBLgT-R5iuw";

    /// <summary>Nó 19 — Try Again (gateway, MAIN).</summary>
    public const string NodeTryAgain           = "_RNdJ0F6PEfGBBLgT-R5iuw";

    /// <summary>Nó 20 — Done - Bail (endEvent, MAIN).</summary>
    public const string NodeDoneBail           = "_RNdJzl6PEfGBBLgT-R5iuw";

    /// <summary>Nó auxiliar — Set Technical Error (scriptTask, ActivitySet). Visitado noutros percursos (SC-ATZINTPC-015).</summary>
    public const string NodeSetTechnicalError  = "_RNdKGF6PEfGBBLgT-R5iuw";

    // ─────────────────────────────────────────────────────────────────────────

    private readonly IEpatServices _services;
    private readonly ManipularExcecaoAtzintpcUseCase _manipularExcecao;

    public AtzintpcSeg041Workflow(
        IEpatServices services,
        ManipularExcecaoAtzintpcUseCase manipularExcecao)
    {
        _services         = services;
        _manipularExcecao = manipularExcecao;
    }

    /// <summary>
    /// Executa o segmento completo (passos 1–20 do cenário SC-ATZINTPC-009),
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
    ///   Delegate de interacção humana para a userTask 'Manipular Excecao' (_RNdJ0V6PEfGBBLgT-R5iuw).
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<AtzintpcSeg041Outcome> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoAtzintpcResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_RNdJyV6PEfGBBLgT-R5iuw) ─────────
        // Ponto de entrada. Sem efeito lateral. Controlo passa ao nó 2.

        // ── Nó 2: scriptTask 'SetParameters' (_RNdJyl6PEfGBBLgT-R5iuw) ───────
        // Regra: RI-script-ATZINTPC-SetParameters.
        // NOEQ-iprocess-builtin: SW_NA via FieldValue<T> (shim-tri-state, ratificado 2026-08-06).
        if (AtzintpcSetParametersRule.ShouldInitialize(ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES))
            AtzintpcSeg041Steps.ApplySetParameters(ctx);

        StartLoopEntry:

        // ── Nó 3: scriptTask 'Start Loop' (_RNdJzF6PEfGBBLgT-R5iuw) ──────────
        // NOEQ-iprocess-builtin: SW_DATE do iProcess substituído pela data UTC do runtime .NET.
        AtzintpcSeg041Steps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_RNdJ2l6PEfGBBLgT-R5iuw) ──
        // ── Nó 5: startEvent interno (_RNdKFl6PEfGBBLgT-R5iuw) ─────────────────
        // DESCIDA EXPLÍCITA: não existe transição XPDL do subProcessScope para o startEvent
        // interno. A aresta é escrita explicitamente neste workflow (AC3).
        {
            var subResult = await ExecuteSubProcessScopeAsync(caseRef, ctx, swQRetryCount, ct);

            if (subResult == SubProcessResult.TechError)
            {
                // ── Nó 13: gateway 'Tech Error' (_RNdJ2V6PEfGBBLgT-R5iuw) ────
                // REGRESSO EXPLÍCITO: não existe transição XPDL do endEvent interno de volta ao MAIN.
                // Alcançado via excepção de transporte ou esgotamento de SW_QRETRYCOUNT (AC5).
                // Ramo "No" (otherwise): → Nó 16 (convergência antes de Manipular Excecao).
                goto ManipularExcecaoEntry;
            }

            if (subResult == SubProcessResult.AppError)
            {
                // ── Nó 13: gateway 'Tech Error' (_RNdJ2V6PEfGBBLgT-R5iuw) ────
                // Regresso explícito. ISTECHERROR = "N" → ramo "No" (otherwise).
                // ── Nó 14: gateway 'App Error' (_RNdJ2F6PEfGBBLgT-R5iuw) ─────
                // Ramo "Yes": ISAPPERROR == "Y".
                // ── Nó 15: gateway 'More Retries' (_RNdJ1V6PEfGBBLgT-R5iuw) ──
                // Ramo "Yes": NUMAPPRETRIES < MAXRETRIES → volta ao laço.
                if (AtzintpcSeg041Steps.HasMoreRetries(ctx))
                    goto StartLoopEntry;

                // Ramo "No" (otherwise): retentativas esgotadas.
                // ── Nó 16: gateway _RNdJ216PEfGBBLgT-R5iuw (convergência) ─────
                goto ManipularExcecaoEntry;
            }

            // SubProcessResult.Success: STATUS_CODE = "0", sem erro.
            // ── Nó 13: gateway 'Tech Error' → ISTECHERROR != "Y" → ramo "No".
            // ── Nó 14: gateway 'App Error' → ISAPPERROR != "Y" → ramo "No".
            return AtzintpcSeg041Outcome.Success;
        }

        ManipularExcecaoEntry:

        // ── Nó 17: userTask 'Manipular Excecao' (_RNdJ0V6PEfGBBLgT-R5iuw) ────
        await _manipularExcecao
            .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
            .ConfigureAwait(false);

        // ── Nó 18: gateway 'Manually Fixed' (_RNdJy16PEfGBBLgT-R5iuw) ─────────
        if (AtzintpcSeg041Steps.IsManuallyFixed(ctx))
        {
            // OUTCOME = "OK" → caso resolvido manualmente.
            return AtzintpcSeg041Outcome.ManuallyFixed;
        }

        // ── Nó 19: gateway 'Try Again' (_RNdJ0F6PEfGBBLgT-R5iuw) ────────────
        if (AtzintpcSeg041Steps.IsTryAgain(ctx))
        {
            // OUTCOME = "R" → operador opta por repetir; reinicia o laço.
            ctx.NUMAPPRETRIES = 0;
            goto StartLoopEntry;
        }

        // ── Nó 20: endEvent 'Done - Bail' (_RNdJzl6PEfGBBLgT-R5iuw) ─────────
        // Ramo "No" de Try Again (OTHERWISE): encerra por esgotamento sem resolução.
        return AtzintpcSeg041Outcome.DoneBail;
    }

    // ── Execução do subProcessScope (ActivitySet) ─────────────────────────────

    private async Task<SubProcessResult> ExecuteSubProcessScopeAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 6: scriptTask 'Start TX' (_RNdKFF6PEfGBBLgT-R5iuw) ─────────────
        AtzintpcSeg041Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_RNdKFV6PEfGBBLgT-R5iuw) ──
        // Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT.
        // Ramo "Stillgood": SW_QRETRYCOUNT < MAXRETRIES → prossegue para AtualizarIntimacao.
        // Ramo oposto: motor esgotado → SetTechError e termina o ActivitySet.
        if (!AtzintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // ── Nó 21: scriptTask 'Set Technical Error' (_RNdKGF6PEfGBBLgT-R5iuw) ──
            // Nó auxiliar: presente no fluxo mas fora do percurso SC-ATZINTPC-009
            // (visitado em SC-ATZINTPC-015). O passo de SetTechError é executado
            // igualmente neste ramo de esgotamento de SW_QRETRYCOUNT.
            AtzintpcSeg041Steps.SetTechError(ctx, "SW_QRETRYCOUNT >= MAXRETRIES");
            // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ────────────────────
            // regresso ao escopo MAIN (regresso explícito AC5)
            return SubProcessResult.TechError;
        }

        ServiceEnvelope envelope;
        try
        {
            // ── Nó 8: serviceTask 'AtualizarIntimacao' (_RNdKHF6PEfGBBLgT-R5iuw) ──
            // Operação: AtualizarintimacaoAsync (EPAT.wsdl)
            envelope = await _services
                .AtualizarintimacaoAsync(caseRef, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Excepção de transporte: não existe transição XPDL para o gateway Tech Error.
            // A aresta de REGRESSO é escrita explicitamente aqui (AC5).
            AtzintpcSeg041Steps.SetTechError(ctx, ex.Message);
            return SubProcessResult.TechError;
        }

        // ── Nó 9: gateway _RNdKGl6PEfGBBLgT-R5iuw ───────────────────────────
        // "A chamada a AtualizarIntimacao foi bem sucedida?"
        // Ramo AppError: STATUS_CODE != "0".
        if (AtzintpcSeg041Steps.IsAppError(envelope))
        {
            // ── Nó 10: scriptTask 'Set App Error' (_RNdKGV6PEfGBBLgT-R5iuw) ──
            AtzintpcSeg041Steps.SetAppError(ctx, envelope);
            // ── Nó 11: gateway _RNdKG16PEfGBBLgT-R5iuw ─────────────────────
            // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ─────────────────────
            return SubProcessResult.AppError;
        }

        // STATUS_CODE == "0": chamada bem sucedida.
        AtzintpcSeg041Steps.MapServiceEnvelopeSuccess(ctx, envelope);
        // ── Nó 11: gateway _RNdKG16PEfGBBLgT-R5iuw ──────────────────────────
        // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ─────────────────────────
        return SubProcessResult.Success;
    }

    /// <summary>Resultado interno do subProcessScope 'Control System Task Call'.</summary>
    private enum SubProcessResult
    {
        Success,
        AppError,
        TechError,
    }
}
