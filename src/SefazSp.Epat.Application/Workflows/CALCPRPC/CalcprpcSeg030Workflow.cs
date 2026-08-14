#nullable enable

using System.Globalization;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CALCPRPC;
using SefazSp.Epat.Application.UseCases.CALCPRPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.CALCPRPC;

/// <summary>
/// Resultado possível do percurso do segmento 030 do processo CALCPRPC.
/// </summary>
public enum CalcprpcSeg030Outcome
{
    /// <summary>
    /// Caso resolvido manualmente — fluxo encerrou no endEvent 'Done - Fixed'
    /// (_zJIHW1qiEfG5K7mY0I3I6w).
    /// </summary>
    DoneFixed,

    /// <summary>
    /// Chamada bem sucedida antes de esgotar as retentativas — STATUS_CODE = '0'.
    /// Nenhum erro de aplicação ou técnico foi detectado.
    /// </summary>
    Success,
}

/// <summary>
/// Workflow do segmento 030 do processo CALCPRPC: de 'Start Event' a 'Done - Fixed'.
///
/// Card: BUILD-CALCPRPC-seg030 · Processo: CALCPRPC · Etapa: 2
/// Cenário de referência: SC-CALCPRPC-008, segmento 1, passos 1–19.
///
/// Implementa <see cref="ICALCPRPC"/> como ponto de entrada do processo.
///
/// Topologia dos 19 nós (percurso de referência SC-CALCPRPC-008, segmento 1):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _zJIHVVqiEfG5K7mY0I3I6w  startEvent          │
/// │   ↓ fluxo                                                                     │
/// │  [2]  SetParameters             _zJIHVlqiEfG5K7mY0I3I6w  scriptTask          │
/// │        Regra: RI-script-CALCPRPC-SetParameters                                │
/// │        NOEQ-iprocess-builtin: shim-tri-state (SW_NA), ratificado 2026-08-06   │
/// │   ↓ fluxo                                                                     │
/// │  [3]  Start Loop                _zJIHWFqiEfG5K7mY0I3I6w  scriptTask          │
/// │        NOEQ-iprocess-builtin: SW_DATE tratado como valor de ambiente           │
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _zJIHZlqiEfG5K7mY0I3I6w  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC3                         │
/// │       ┌─ ActivitySet scope ─────────────────────────────────────────────────┐  │
/// │       │ [5]  startEvent interno  _zJIublqiEfG5K7mY0I3I6w  startEvent       │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [6]  Start TX            _zJIuaVqiEfG5K7mY0I3I6w  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [7]  Check Retries       _zJIubVqiEfG5K7mY0I3I6w  gateway         │  │
/// │       │       Regra: RI-transition-CALCPRPC-CheckRetriesSWQRETRYCOUNT      │  │
/// │       │       ↓ Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES)                  │  │
/// │       │ [8]  CalcularPrazo       _AsZCkVqkEfG5K7mY0I3I6w  serviceTask     │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [9]  gateway             _zJIuclqiEfG5K7mY0I3I6w  gateway         │  │
/// │       │       "A chamada a CalcularPrazo foi bem sucedida?"                │  │
/// │       │       ramo AppError: STATUS_CODE != "0" ↓                          │  │
/// │       │ [10] Set App Error       _zJIucVqiEfG5K7mY0I3I6w  scriptTask      │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [11] gateway             _zJIuc1qiEfG5K7mY0I3I6w  gateway         │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [12] endEvent            _zJIub1qiEfG5K7mY0I3I6w  endEvent        │  │
/// │       └────────────────────────────────────────────────────────────────────┘  │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC5                       │
/// │  [13] Tech Error                _zJIHZVqiEfG5K7mY0I3I6w  gateway             │
/// │        ramo "No" (otherwise): ISTECHERROR != "Y" → App Error                 │
/// │   ↓ ramo "No"                                                                 │
/// │  [14] App Error                 _zJIHZFqiEfG5K7mY0I3I6w  gateway             │
/// │        ramo "Yes": ISAPPERROR == "Y" → More Retries                           │
/// │   ↓ ramo "Yes"                                                                │
/// │  [15] More Retries              _zJIHYVqiEfG5K7mY0I3I6w  gateway             │
/// │        ramo "Yes": NUMAPPRETRIES &lt; MAXRETRIES → Start Loop (retry)          │
/// │        ramo "No" (otherwise): retentativas esgotadas → Manipular Excecao      │
/// │   ↓ ramo "No"                                                                 │
/// │  [16] gateway                   _zJIHZ1qiEfG5K7mY0I3I6w  gateway             │
/// │        (convergência Tech Error + More Retries esgotadas)                     │
/// │   ↓ fluxo                                                                     │
/// │  [17] Manipular Excecao         _zJIHXVqiEfG5K7mY0I3I6w  userTask            │
/// │   ↓ fluxo                                                                     │
/// │  [18] Manually Fixed            _zJIHV1qiEfG5K7mY0I3I6w  gateway             │
/// │        ramo "Yes": OUTCOME == "OK" → Done - Fixed                             │
/// │        ramo retorno: OUTCOME == "R" → volta ao nó 3 (Start Loop)              │
/// │   ↓ ramo "Yes"                                                                │
/// │  [19] Done - Fixed              _zJIHW1qiEfG5K7mY0I3I6w  endEvent            │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (_zJIublqiEfG5K7mY0I3I6w)
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error (_zJIHZVqiEfG5K7mY0I3I6w)
///
/// Bloqueador NOEQ-iprocess-builtin (resolvido, shim-tri-state, ratificado 2026-08-06):
///   • nó 2 SetParameters usa SW_NA via <see cref="FieldValue{T}"/>.
///   • nó 3 Start Loop usa SW_DATE como valor de ambiente (não escrito no contexto técnico).
/// </summary>
public sealed class CalcprpcSeg030Workflow : ICALCPRPC
{
    // ── Identificadores de nó — invariantes (não renomear) ───────────────────

    /// <summary>Nó 1  — Start Event (ponto de entrada, MAIN).</summary>
    public const string NodeStartEvent         = "_zJIHVVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-CALCPRPC-SetParameters.</summary>
    public const string NodeSetParameters      = "_zJIHVlqiEfG5K7mY0I3I6w";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN).</summary>
    public const string NodeStartLoop          = "_zJIHWFqiEfG5K7mY0I3I6w";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeSubProcessScope    = "_zJIHZlqiEfG5K7mY0I3I6w";

    /// <summary>Nó 5  — startEvent interno (descida explícita, ActivitySet).</summary>
    public const string NodeStartEventInternal = "_zJIublqiEfG5K7mY0I3I6w";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx            = "_zJIuaVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-CALCPRPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries       = "_zJIubVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 8  — CalcularPrazo (serviceTask, ActivitySet).</summary>
    public const string NodeCalcularPrazo      = "_AsZCkVqkEfG5K7mY0I3I6w";

    /// <summary>Nó 9  — gateway (ActivitySet). Decisão: A chamada a CalcularPrazo foi bem sucedida?</summary>
    public const string NodeGatewayCallResult  = "_zJIuclqiEfG5K7mY0I3I6w";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError        = "_zJIucVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 11 — gateway de convergência interno (ActivitySet).</summary>
    public const string NodeGatewayInnerMerge  = "_zJIuc1qiEfG5K7mY0I3I6w";

    /// <summary>Nó 12 — endEvent interno (ActivitySet).</summary>
    public const string NodeEndEventInternal   = "_zJIub1qiEfG5K7mY0I3I6w";

    /// <summary>Nó 13 — Tech Error (gateway, MAIN). Alcançado por regresso explícito.</summary>
    public const string NodeTechError          = "_zJIHZVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError           = "_zJIHZFqiEfG5K7mY0I3I6w";

    /// <summary>Nó 15 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries        = "_zJIHYVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 16 — gateway de convergência (MAIN). Une ramos Tech Error + More Retries esgotadas.</summary>
    public const string NodeGatewayMerge       = "_zJIHZ1qiEfG5K7mY0I3I6w";

    /// <summary>Nó 17 — Manipular Excecao (userTask, MAIN).</summary>
    public const string NodeManipularExcecao   = "_zJIHXVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 18 — Manually Fixed (gateway, MAIN).</summary>
    public const string NodeManuallyFixed      = "_zJIHV1qiEfG5K7mY0I3I6w";

    /// <summary>Nó 19 — Done - Fixed (endEvent, MAIN).</summary>
    public const string NodeDoneFixed          = "_zJIHW1qiEfG5K7mY0I3I6w";

    // ─────────────────────────────────────────────────────────────────────────

    private readonly ICalcularPrazoSoapService _calcularPrazo;
    private readonly ManipularExcecaoCalcprpcUseCase _manipularExcecao;

    public CalcprpcSeg030Workflow(
        ICalcularPrazoSoapService calcularPrazo,
        ManipularExcecaoCalcprpcUseCase manipularExcecao)
    {
        _calcularPrazo   = calcularPrazo;
        _manipularExcecao = manipularExcecao;
    }

    /// <inheritdoc/>
    public async Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var ctx = new ProcessExecutionContext();
        var outcome = await RunAsync(caseRef, ctx, swQRetryCount: 0,
            decideManipularExcecao: (_, _) => Task.FromResult(ManipularExcecaoCalcprpcResult.ManuallyFixed),
            ct: ct);

        return outcome == CalcprpcSeg030Outcome.DoneFixed
            ? new ProcessCallResult(Started: true,  ChildInstanceId: ctx.PROCESS_ID ?? caseRef.ProcessId, Failure: null)
            : new ProcessCallResult(Started: false, ChildInstanceId: null, Failure: outcome.ToString());
    }

    /// <summary>
    /// Executa o segmento completo (passos 1–19 do cenário SC-CALCPRPC-008),
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
    ///   Delegate de interação humana para a userTask 'Manipular Excecao' (_zJIHXVqiEfG5K7mY0I3I6w).
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<CalcprpcSeg030Outcome> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoCalcprpcResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // ── Nó 1: startEvent 'Start Event' (_zJIHVVqiEfG5K7mY0I3I6w) ─────────
        // Ponto de entrada. Sem efeito lateral. Controlo passa ao nó 2.

        // ── Nó 2: scriptTask 'SetParameters' (_zJIHVlqiEfG5K7mY0I3I6w) ───────
        // Regra: RI-script-CALCPRPC-SetParameters.
        // NOEQ-iprocess-builtin: IDPROCESSO comparado com SW_NA via FieldValue<T> (shim-tri-state).
        // SW_NA NUNCA é mapeado para null — FieldValue<T>.NotAvailable é o terceiro estado.
        var idProcesso = ParseIdProcesso(caseRef.ProcessId);
        if (CalcprpcSetParametersRule.ShouldInitialize(idProcesso, ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES))
            CalcprpcExecutionSteps.ApplySetParameters(ctx, caseRef.ProcessId);

        StartLoopEntry:

        // ── Nó 3: scriptTask 'Start Loop' (_zJIHWFqiEfG5K7mY0I3I6w) ──────────
        // NOEQ-iprocess-builtin: SW_DATE é um valor de ambiente do runtime iProcess;
        // tratado aqui como data corrente do sistema (.NET: DateTimeOffset.UtcNow).
        // A decisão shim-tri-state ratificada em 2026-08-06 exige pattern matching
        // exaustivo; SW_DATE não usa SW_NA, é apenas um valor de data, portanto
        // nenhum FieldValue<T> é necessário aqui.
        CalcprpcExecutionSteps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_zJIHZlqiEfG5K7mY0I3I6w) ──
        // ── Nó 5: startEvent interno (_zJIublqiEfG5K7mY0I3I6w) ─────────────────
        // DESCIDA EXPLÍCITA: não existe transição XPDL do subProcessScope para o startEvent
        // interno. A aresta é escrita explicitamente neste workflow (AC3).
        {
            var subResult = await ExecuteSubProcessScopeAsync(caseRef, ctx, swQRetryCount, ct);

            if (subResult == SubProcessResult.TechError)
            {
                // ── Nó 13: gateway 'Tech Error' (_zJIHZVqiEfG5K7mY0I3I6w) ────
                // REGRESSO EXPLÍCITO: não existe transição XPDL do endEvent interno de volta ao MAIN.
                // Alcançado via excepção de transporte no ActivitySet (AC5).
                // Ramo "No" (otherwise): ISTECHERROR = "Y" → nó 16 (convergência antes de Manipular Excecao).
                // Nó 16: gateway _zJIHZ1qiEfG5K7mY0I3I6w (convergência).
                goto ManipularExcecaoEntry;
            }

            if (subResult == SubProcessResult.AppError)
            {
                // ── Nó 13: gateway 'Tech Error' (_zJIHZVqiEfG5K7mY0I3I6w) ────
                // Regresso explícito. ISTECHERROR = "N" → ramo "No" (otherwise).
                // ── Nó 14: gateway 'App Error' (_zJIHZFqiEfG5K7mY0I3I6w) ─────
                // Ramo "Yes": ISAPPERROR == "Y".
                // ── Nó 15: gateway 'More Retries' (_zJIHYVqiEfG5K7mY0I3I6w) ──
                // Ramo "Yes": NUMAPPRETRIES < MAXRETRIES → volta ao laço.
                if (CalcprpcExecutionSteps.HasMoreRetries(ctx))
                    goto StartLoopEntry;

                // Ramo "No" (otherwise): retentativas esgotadas.
                // ── Nó 16: gateway _zJIHZ1qiEfG5K7mY0I3I6w (convergência) ────
                goto ManipularExcecaoEntry;
            }

            // SubProcessResult.Success: STATUS_CODE = "0", sem erro.
            // ── Nó 13: gateway 'Tech Error' → ramo "No".
            // ── Nó 14: gateway 'App Error' → ramo "No" (ISAPPERROR != "Y").
            return CalcprpcSeg030Outcome.Success;
        }

        ManipularExcecaoEntry:

        // ── Nó 17: userTask 'Manipular Excecao' (_zJIHXVqiEfG5K7mY0I3I6w) ────
        await _manipularExcecao
            .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
            .ConfigureAwait(false);

        // ── Nó 18: gateway 'Manually Fixed' (_zJIHV1qiEfG5K7mY0I3I6w) ─────────
        if (ctx.OUTCOME == "OK")
        {
            // ── Nó 19: endEvent 'Done - Fixed' (_zJIHW1qiEfG5K7mY0I3I6w) ─────
            return CalcprpcSeg030Outcome.DoneFixed;
        }

        // OUTCOME == 'R': operador optou por repetir.
        goto StartLoopEntry;
    }

    // ── Execução do subProcessScope (ActivitySet) ─────────────────────────────

    private async Task<SubProcessResult> ExecuteSubProcessScopeAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 6: scriptTask 'Start TX' (_zJIuaVqiEfG5K7mY0I3I6w) ─────────────
        CalcprpcExecutionSteps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_zJIubVqiEfG5K7mY0I3I6w) ──
        // Regra: RI-transition-CALCPRPC-CheckRetriesSWQRETRYCOUNT.
        // Ramo "Stillgood": SW_QRETRYCOUNT < MAXRETRIES → prossegue para CalcularPrazo.
        // Ramo oposto: motor esgotado → SetTechError e termina o ActivitySet.
        if (!CalcprpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            CalcprpcExecutionSteps.SetTechError(ctx, "Maxretriesexceeded");
            // ── Nó 12: endEvent _zJIub1qiEfG5K7mY0I3I6w ────────────────────
            // queda para regresso ao escopo MAIN
            return SubProcessResult.TechError;
        }

        ServiceEnvelope envelope;
        try
        {
            // ── Nó 8: serviceTask 'CalcularPrazo' (_AsZCkVqkEfG5K7mY0I3I6w) ──
            envelope = await _calcularPrazo
                .InvokeAsync(caseRef, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Excepção de transporte: não existe transição XPDL para o gateway Tech Error.
            // A aresta de REGRESSO é escrita explicitamente aqui (AC5).
            CalcprpcExecutionSteps.SetTechError(ctx, ex.Message);
            return SubProcessResult.TechError;
        }

        // ── Nó 9: gateway _zJIuclqiEfG5K7mY0I3I6w — STATUS_CODE != "0"? ──────
        if (CalcprpcExecutionSteps.IsAppError(ctx) || envelope.STATUS_CODE != "0")
        {
            // ── Nó 10: scriptTask 'Set App Error' (_zJIucVqiEfG5K7mY0I3I6w) ──
            CalcprpcExecutionSteps.SetAppError(ctx, envelope);
            // ── Nó 11: gateway _zJIuc1qiEfG5K7mY0I3I6w ─────────────────────
            // ── Nó 12: endEvent _zJIub1qiEfG5K7mY0I3I6w ─────────────────────
            return SubProcessResult.AppError;
        }

        // STATUS_CODE == "0": chamada bem sucedida.
        CalcprpcExecutionSteps.MapServiceEnvelope(ctx, envelope);
        // ── Nó 11: gateway _zJIuc1qiEfG5K7mY0I3I6w ──────────────────────────
        // ── Nó 12: endEvent _zJIub1qiEfG5K7mY0I3I6w ─────────────────────────
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
