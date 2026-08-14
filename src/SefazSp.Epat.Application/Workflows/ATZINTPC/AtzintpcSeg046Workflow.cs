#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.ATZINTPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.ATZINTPC;

/// <summary>
/// Resultado do segmento 046 do processo ATZINTPC (passos 1–18 do cenário SC-ATZINTPC-007).
/// </summary>
public enum AtzintpcSeg046Outcome
{
    /// <summary>
    /// O laço de retentativa tem mais tentativas disponíveis (NUMAPPRETRIES &lt; MAXRETRIES).
    /// O fluxo chegou ao linkCatch "Try Task" (_RNdJzV6PEfGBBLgT-R5iuw), via
    /// linkThrow "Link To: Try Task" (_RNdJ1F6PEfGBBLgT-R5iuw) após Pause.
    /// O chamador deve reinvocar o segmento para a próxima iteração.
    /// Decisão NOEQ-link-goto (ratificado): o link-goto é implementado como
    /// retorno ao chamador com outcome TryTask — o chamador controla o laço externo.
    /// </summary>
    TryTask,

    /// <summary>
    /// Chamada bem sucedida (STATUS_CODE = "0").
    /// Nenhum erro de aplicação ou técnico foi detectado.
    /// </summary>
    Success,

    /// <summary>
    /// Erro de aplicação sem mais retentativas disponíveis
    /// (NUMAPPRETRIES &gt;= MAXRETRIES) ou erro técnico não é erro de aplicação.
    /// O fluxo encerrou no endEvent _RNdKF16PEfGBBLgT-R5iuw (dentro do subProcessScope),
    /// regressou ao MAIN, e nenhum ramo de retentativa estava disponível.
    /// </summary>
    AppErrorEnd,
}

/// <summary>
/// Workflow do segmento 046 do processo ATZINTPC: de 'Start Event' a 'Try Task'.
///
/// Card: BUILD-ATZINTPC-seg046 · Processo: ATZINTPC · Etapa: 4
/// Cenário de referência: SC-ATZINTPC-007, segmento 1, passos 1–18.
///
/// Herdado de CONTROPC/AtualizaIntimacao (profundidade 2).
///
/// Topologia dos 18 nós (percurso de referência SC-ATZINTPC-007):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _RNdJyV6PEfGBBLgT-R5iuw  startEvent          │
/// │   ↓ fluxo                                                                     │
/// │  [2]  SetParameters             _RNdJyl6PEfGBBLgT-R5iuw  scriptTask          │
/// │        Regra: RI-script-ATZINTPC-SetParameters                                │
/// │        NOEQ-iprocess-builtin: shim-tri-state (SW_NA), ratificado              │
/// │   ↓ fluxo                                                                     │
/// │  [3]  Start Loop                _RNdJzF6PEfGBBLgT-R5iuw  scriptTask          │
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _RNdJ2l6PEfGBBLgT-R5iuw  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC2                         │
/// │       ┌─ ActivitySet scope ─────────────────────────────────────────────────┐  │
/// │       │ [5]  startEvent interno  _RNdKFl6PEfGBBLgT-R5iuw  startEvent       │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [6]  Start TX            _RNdKFF6PEfGBBLgT-R5iuw  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [7]  Check Retries       _RNdKFV6PEfGBBLgT-R5iuw  gateway         │  │
/// │       │       Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT      │  │
/// │       │       ↓ Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES)                  │  │
/// │       │ [8]  AtualizarIntimacao  _RNdKHF6PEfGBBLgT-R5iuw  serviceTask     │  │
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
/// │        ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES → Pause                      │
/// │   ↓ ramo "Yes"                                                                │
/// │  [16] Pause                     _RNdJ1l6PEfGBBLgT-R5iuw  timerEvent          │
/// │   ↓ fluxo                                                                     │
/// │  [17] Link To: Try Task         _RNdJ1F6PEfGBBLgT-R5iuw  linkThrow           │
/// │        NOEQ-link-goto (ratificado): o link-goto sinaliza TryTask ao chamador  │
/// │   ↓ link                                                                      │
/// │  [18] Try Task                  _RNdJzV6PEfGBBLgT-R5iuw  linkCatch           │
/// │        (corte-de-laço — fim do segmento)                                      │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Nota AC6: o scriptTask 'Set Technical Error' (_RNdKGF6PEfGBBLgT-R5iuw, ordem 19)
/// não aparece no percurso de referência deste segmento (SC-ATZINTPC-007);
/// não está incluído neste troco.
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (_RNdKFl6PEfGBBLgT-R5iuw)
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error (_RNdJ2V6PEfGBBLgT-R5iuw)
///   • ordem 18 · link      · linkThrow → linkCatch Try Task (_RNdJzV6PEfGBBLgT-R5iuw)
/// </summary>
public sealed class AtzintpcSeg046Workflow
{
    // ── Identificadores de nó — invariantes (não renomear) ───────────────────

    /// <summary>Nó 1  — Start Event (ponto de entrada, MAIN).</summary>
    public const string NodeStartEvent          = "_RNdJyV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-ATZINTPC-SetParameters.</summary>
    public const string NodeSetParameters       = "_RNdJyl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN).</summary>
    public const string NodeStartLoop           = "_RNdJzF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeSubProcessScope     = "_RNdJ2l6PEfGBBLgT-R5iuw";

    /// <summary>Nó 5  — startEvent interno (descida explícita, ActivitySet).</summary>
    public const string NodeStartEventInternal  = "_RNdKFl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx             = "_RNdKFF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries        = "_RNdKFV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 8  — AtualizarIntimacao (serviceTask, ActivitySet). Operação: AtualizarintimacaoAsync.</summary>
    public const string NodeAtualizarIntimacao  = "_RNdKHF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 9  — gateway (ActivitySet). Decisão: A chamada a AtualizarIntimacao foi bem sucedida? (STATUS_CODE != "0").</summary>
    public const string NodeGatewayCallResult   = "_RNdKGl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError         = "_RNdKGV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — gateway de convergência interno (ActivitySet).</summary>
    public const string NodeGatewayInnerMerge   = "_RNdKG16PEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — endEvent interno (ActivitySet).</summary>
    public const string NodeEndEventInternal    = "_RNdKF16PEfGBBLgT-R5iuw";

    /// <summary>Nó 13 — Tech Error (gateway, MAIN). Alcançado por regresso explícito.</summary>
    public const string NodeTechError           = "_RNdJ2V6PEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError            = "_RNdJ2F6PEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries         = "_RNdJ1V6PEfGBBLgT-R5iuw";

    /// <summary>Nó 16 — Pause (timerEvent, MAIN).</summary>
    public const string NodePause               = "_RNdJ1l6PEfGBBLgT-R5iuw";

    /// <summary>Nó 17 — Link To: Try Task (linkThrow, MAIN). Decisão NOEQ-link-goto.</summary>
    public const string NodeLinkThrowTryTask    = "_RNdJ1F6PEfGBBLgT-R5iuw";

    /// <summary>Nó 18 — Try Task (linkCatch, MAIN). Fim do segmento (corte-de-laço). Decisão NOEQ-link-goto.</summary>
    public const string NodeTryTask             = "_RNdJzV6PEfGBBLgT-R5iuw";

    /// <summary>Nó auxiliar — Set Technical Error (_RNdKGF6PEfGBBLgT-R5iuw). Não aparece no percurso de referência deste segmento (AC6).</summary>
    public const string NodeSetTechnicalError   = "_RNdKGF6PEfGBBLgT-R5iuw";

    // ─────────────────────────────────────────────────────────────────────────

    private readonly IEpatServices _services;

    public AtzintpcSeg046Workflow(IEpatServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Executa o segmento completo (passos 1–18 do cenário SC-ATZINTPC-007).
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="swQRetryCount">
    ///   Valor de <c>IPESystemValues.SW_QRETRYCOUNT</c> fornecido pelo runtime iProcess.
    ///   Lido pelo gateway Check Retries; nunca escrito pelo processo.
    ///   NOEQ-iprocess-builtin, ratificado.
    /// </param>
    /// <param name="pause">
    ///   Delegate que implementa o timerEvent 'Pause' (_RNdJ1l6PEfGBBLgT-R5iuw).
    ///   Suspende o fluxo pelo intervalo de tempo configurado antes de relançar.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<AtzintpcSeg046Outcome> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task> pause,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_RNdJyV6PEfGBBLgT-R5iuw) ─────────
        // Ponto de entrada. Sem efeito lateral. Controlo passa ao nó 2.

        // ── Nó 2: scriptTask 'SetParameters' (_RNdJyl6PEfGBBLgT-R5iuw) ───────
        // Regra: RI-script-ATZINTPC-SetParameters.
        // NOEQ-iprocess-builtin: IDPROCESSO comparado com SW_NA via FieldValue<T> (shim-tri-state).
        // SW_NA NUNCA é mapeado para null — FieldValue<T>.NotAvailable é o terceiro estado.
        var idProcesso = ParseIdProcesso(caseRef.ProcessId);
        if (AtzintpcSetParametersRule.ShouldInitialize(idProcesso, ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES))
            AtzintpcSeg046Steps.ApplySetParameters(ctx, caseRef.ProcessId);

        // ── Nó 3: scriptTask 'Start Loop' (_RNdJzF6PEfGBBLgT-R5iuw) ──────────
        // Nó 3 é executado uma vez no início deste segmento (o laço externo é controlado pelo chamador).
        AtzintpcSeg046Steps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_RNdJ2l6PEfGBBLgT-R5iuw) ──
        // ── Nó 5: startEvent interno (_RNdKFl6PEfGBBLgT-R5iuw) ─────────────────
        // DESCIDA EXPLÍCITA: não existe transição XPDL do subProcessScope para o startEvent
        // interno. A aresta é escrita explicitamente neste workflow (AC2).
        var subResult = await ExecuteSubProcessScopeAsync(caseRef, ctx, swQRetryCount, ct)
            .ConfigureAwait(false);

        // ── Nó 13: gateway 'Tech Error' (_RNdJ2V6PEfGBBLgT-R5iuw) ─────────────
        // REGRESSO EXPLÍCITO: não existe transição XPDL do endEvent interno de volta ao MAIN.
        // Alcançado via excepção de transporte (subResult=TechError) ou fim do subProcessScope
        // após App Error (subResult=AppError) — AC5.
        // Ramo "No" (otherwise) → App Error.

        if (subResult == SubProcessResult.Success)
        {
            // STATUS_CODE = "0" → sucesso. Sem erro de aplicação ou técnico.
            // Tech Error gateway → ramo No (otherwise)
            // App Error gateway → ramo No (ISAPPERROR != "Y")
            return AtzintpcSeg046Outcome.Success;
        }

        // ── Nó 14: gateway 'App Error' (_RNdJ2F6PEfGBBLgT-R5iuw) ──────────────
        // Ramo Yes: ISAPPERROR == "Y"
        if (!AtzintpcSeg046Steps.IsAppErrorFlag(ctx))
        {
            // ISAPPERROR != "Y": erro técnico sem erro de aplicação.
            // Sem destino neste segmento → encerra.
            return AtzintpcSeg046Outcome.AppErrorEnd;
        }

        // ── Nó 15: gateway 'More Retries' (_RNdJ1V6PEfGBBLgT-R5iuw) ───────────
        // Ramo Yes: NUMAPPRETRIES < MAXRETRIES → Pause → LinkThrow
        if (!AtzintpcSeg046Steps.HasMoreRetries(ctx))
        {
            // NUMAPPRETRIES >= MAXRETRIES: retentativas esgotadas.
            return AtzintpcSeg046Outcome.AppErrorEnd;
        }

        // ── Nó 16: timerEvent 'Pause' (_RNdJ1l6PEfGBBLgT-R5iuw) ────────────────
        await pause(caseRef, ct).ConfigureAwait(false);

        // ── Nó 17: linkThrow 'Link To: Try Task' (_RNdJ1F6PEfGBBLgT-R5iuw) ────
        // NOEQ-link-goto (ratificado): o link-goto é implementado como retorno
        // ao chamador com outcome TryTask. O chamador controla o laço externo e
        // reinvoca este segmento na próxima iteração.

        // ── Nó 18: linkCatch 'Try Task' (_RNdJzV6PEfGBBLgT-R5iuw) ──────────────
        // Destino do linkThrow. Fim do segmento (corte-de-laço).
        return AtzintpcSeg046Outcome.TryTask;
    }

    // ── Sub-processo interno ──────────────────────────────────────────────────

    private enum SubProcessResult { Success, AppError, TechError }

    private async Task<SubProcessResult> ExecuteSubProcessScopeAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 5: startEvent interno (_RNdKFl6PEfGBBLgT-R5iuw) ─────────────────
        // DESCIDA EXPLÍCITA (AC2): entrada no ActivitySet embutido no subProcessScope.

        // ── Nó 6: scriptTask 'Start TX' (_RNdKFF6PEfGBBLgT-R5iuw) ──────────────
        AtzintpcSeg046Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_RNdKFV6PEfGBBLgT-R5iuw)
        // Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT
        // Ramo Stillgood: SW_QRETRYCOUNT < MAXRETRIES → prossegue para AtualizarIntimacao
        if (!AtzintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Retentativas do motor esgotadas → Tech Error.
            AtzintpcSeg046Steps.SetTechError(ctx, $"SW_QRETRYCOUNT ({swQRetryCount}) >= MAXRETRIES ({ctx.MAXRETRIES})");
            return SubProcessResult.TechError;
        }

        // ── Nó 8: serviceTask 'AtualizarIntimacao' (_RNdKHF6PEfGBBLgT-R5iuw) ───
        // Operação TIBCO: __sol_EPATInterfaceWrappers_sol_atualizarIntimacao.1
        // Uma excepção de transporte activa o gateway Tech Error (regresso explícito, AC5).
        ServiceEnvelope envelope;
        bool isTechError = false;
        try
        {
            envelope = await _services
                .AtualizarintimacaoAsync(caseRef, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Aresta de regresso para o gateway Tech Error (_RNdJ2V6PEfGBBLgT-R5iuw).
            // Registada explicitamente por não existir como transição no XPDL (AC5).
            AtzintpcSeg046Steps.SetTechError(ctx, ex.Message);
            isTechError = true;
            envelope = new ServiceEnvelope(null, null, ex.Message);
        }

        if (isTechError)
            return SubProcessResult.TechError;

        // ── Nó 9: gateway _RNdKGl6PEfGBBLgT-R5iuw ──────────────────────────────
        // "A chamada a AtualizarIntimacao foi bem sucedida?"
        // Ramo AppError: STATUS_CODE != "0"
        if (!AtzintpcSeg046Steps.IsAppError(envelope))
        {
            // Ramo de sucesso: mapeia o envelope e encerra o subProcessScope com sucesso.
            AtzintpcSeg046Steps.MapServiceEnvelopeSuccess(ctx, envelope);

            // ── Nó 11: gateway _RNdKG16PEfGBBLgT-R5iuw (convergência, ramo sucesso) ─
            // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ────────────────────────────
            return SubProcessResult.Success;
        }

        // ── Nó 10: scriptTask 'Set App Error' (_RNdKGV6PEfGBBLgT-R5iuw) ─────────
        // STATUS_CODE != "0" → erro de aplicação.
        AtzintpcSeg046Steps.SetAppError(ctx, envelope);

        // ── Nó 11: gateway _RNdKG16PEfGBBLgT-R5iuw (convergência) ───────────────
        // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ────────────────────────────
        return SubProcessResult.AppError;
    }

    // ── Mapeamento de IDPROCESSO para FieldValue<long> ───────────────────────

    private static FieldValue<long> ParseIdProcesso(string? processId)
    {
        if (processId is null)
            return FieldValue<long>.Empty;

        if (long.TryParse(processId, out var value))
            return FieldValue<long>.Of(value);

        // Qualquer valor não-numérico que não seja null é tratado como não disponível (SW_NA).
        return FieldValue<long>.NotAvailable;
    }
}
