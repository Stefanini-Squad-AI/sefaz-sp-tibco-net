#nullable enable

using System.Globalization;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.ATZINTPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.ATZINTPC;

/// <summary>
/// Resultado possível do percurso do segmento 043 do processo ATZINTPC.
/// </summary>
public enum AtzintpcSeg043Outcome
{
    /// <summary>
    /// Chamada tratada (com ou sem erro de aplicação) e processo encerrado em Done - Success.
    /// Nenhum erro técnico fatal foi detectado, ou o fluxo chegou ao fim normal do segmento.
    /// </summary>
    DoneSuccess,
}

/// <summary>
/// Workflow do segmento 043 do processo ATZINTPC: de 'Start Event' a 'Done - Success'.
///
/// Card: BUILD-ATZINTPC-seg043 · Processo: ATZINTPC · Etapa: 4
/// Cenário de referência: SC-ATZINTPC-010, segmento 1, passos 1–15.
///
/// Herdado de CONTROPC/AtualizaIntimacao.
///
/// Topologia dos 15 nós (percurso de referência SC-ATZINTPC-010, segmento 1):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _RNdJyV6PEfGBBLgT-R5iuw  startEvent          │
/// │   ↓ fluxo                                                                     │
/// │  [2]  SetParameters             _RNdJyl6PEfGBBLgT-R5iuw  scriptTask          │
/// │        Regra: RI-script-ATZINTPC-SetParameters                                │
/// │        NOEQ-iprocess-builtin: shim-tri-state (SW_NA), ratificado 2026-08-06   │
/// │   ↓ fluxo                                                                     │
/// │  [3]  Start Loop                _RNdJzF6PEfGBBLgT-R5iuw  scriptTask          │
/// │        NOEQ-iprocess-builtin: SW_DATE (data de sistema), shim-tri-state       │
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _RNdJ2l6PEfGBBLgT-R5iuw  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC4                         │
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
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC7                       │
/// │  [13] Tech Error                _RNdJ2V6PEfGBBLgT-R5iuw  gateway             │
/// │        ramo "No" (otherwise): → App Error                                     │
/// │   ↓ ramo "No"                                                                 │
/// │  [14] App Error                 _RNdJ2F6PEfGBBLgT-R5iuw  gateway             │
/// │        ramo "No" (otherwise): → Done - Success                                │
/// │   ↓ ramo "No"                                                                 │
/// │  [15] Done - Success            _RNdJ116PEfGBBLgT-R5iuw  endEvent            │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (_RNdKFl6PEfGBBLgT-R5iuw)
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error (_RNdJ2V6PEfGBBLgT-R5iuw)
///
/// Nó excluído deste segmento (não faz parte do percurso SC-ATZINTPC-010):
///   • _RNdKGF6PEfGBBLgT-R5iuw (Set Technical Error) — visitado em SC-ATZINTPC-016.
/// </summary>
public sealed class AtzintpcSeg043Workflow
{
    // ── Identificadores de nó — invariantes (não renomear) ───────────────────

    /// <summary>Nó 1  — Start Event (ponto de entrada, MAIN).</summary>
    public const string NodeStartEvent         = "_RNdJyV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-ATZINTPC-SetParameters.</summary>
    public const string NodeSetParameters      = "_RNdJyl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN). NOEQ-iprocess-builtin: SW_DATE.</summary>
    public const string NodeStartLoop          = "_RNdJzF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeSubProcessScope    = "_RNdJ2l6PEfGBBLgT-R5iuw";

    /// <summary>Nó 5  — startEvent interno (descida explícita, ActivitySet). AC4.</summary>
    public const string NodeStartEventInternal = "_RNdKFl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx            = "_RNdKFF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries       = "_RNdKFV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 8  — AtualizarIntimacao (serviceTask, ActivitySet). Operação: AtualizarintimacaoAsync.</summary>
    public const string NodeAtualizarIntimacao = "_RNdKHF6PEfGBBLgT-R5iuw";

    /// <summary>Nó 9  — gateway (ActivitySet). Decisão: A chamada a AtualizarIntimacao foi bem sucedida? STATUS_CODE != "0".</summary>
    public const string NodeGatewayCallResult  = "_RNdKGl6PEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError        = "_RNdKGV6PEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — gateway de convergência interno (ActivitySet).</summary>
    public const string NodeGatewayInnerMerge  = "_RNdKG16PEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — endEvent interno (ActivitySet).</summary>
    public const string NodeEndEventInternal   = "_RNdKF16PEfGBBLgT-R5iuw";

    /// <summary>Nó 13 — Tech Error (gateway, MAIN). Alcançado por regresso explícito. AC7.</summary>
    public const string NodeTechError          = "_RNdJ2V6PEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError           = "_RNdJ2F6PEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — Done - Success (endEvent, MAIN).</summary>
    public const string NodeDoneSuccess        = "_RNdJ116PEfGBBLgT-R5iuw";

    /// <summary>Nó auxiliar — Set Technical Error (scriptTask, ActivitySet). Visitado noutros percursos (SC-ATZINTPC-016).</summary>
    public const string NodeSetTechnicalError  = "_RNdKGF6PEfGBBLgT-R5iuw";

    // ─────────────────────────────────────────────────────────────────────────

    private readonly IEpatServices _services;

    public AtzintpcSeg043Workflow(IEpatServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Executa o segmento completo (passos 1–15 do cenário SC-ATZINTPC-010).
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="swQRetryCount">
    ///   Valor de <c>IPESystemValues.SW_QRETRYCOUNT</c> fornecido pelo runtime iProcess.
    ///   Lido pelo gateway Check Retries; nunca escrito pelo processo.
    ///   NOEQ-iprocess-builtin, ratificado 2026-08-06.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O desfecho do segmento.</returns>
    public async Task<AtzintpcSeg043Outcome> RunAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
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
            AtzintpcSeg043Steps.ApplySetParameters(ctx, caseRef.ProcessId);

        // ── Nó 3: scriptTask 'Start Loop' (_RNdJzF6PEfGBBLgT-R5iuw) ──────────
        // NOEQ-iprocess-builtin: SW_DATE (data de sistema do iProcess) via shim-tri-state.
        AtzintpcSeg043Steps.ApplyStartLoop(ctx);

        // ── Nó 4: subProcessScope 'Control System Task Call' (_RNdJ2l6PEfGBBLgT-R5iuw) ──
        // ── Nó 5: startEvent interno (_RNdKFl6PEfGBBLgT-R5iuw) ─────────────────
        // DESCIDA EXPLÍCITA: não existe transição XPDL do subProcessScope para o startEvent
        // interno. A aresta é escrita explicitamente neste workflow (AC4).
        {
            var subResult = await ExecuteSubProcessScopeAsync(caseRef, ctx, swQRetryCount, ct);

            // ── Nó 13: gateway 'Tech Error' (_RNdJ2V6PEfGBBLgT-R5iuw) ────────
            // REGRESSO EXPLÍCITO: não existe transição XPDL do endEvent interno de volta ao MAIN.
            // A aresta de regresso é escrita explicitamente aqui (AC7).
            // Ramo "No" (otherwise): ISTECHERROR != "Y" → App Error.
            // (Neste percurso SC-ATZINTPC-010, o fluxo segue sempre o ramo "No".)

            // ── Nó 14: gateway 'App Error' (_RNdJ2F6PEfGBBLgT-R5iuw) ─────────
            // Ramo "No" (otherwise): → Done - Success.
            // (Neste percurso, ISAPPERROR pode ser "Y" mas o fluxo converge em Done - Success.)
            _ = subResult;
        }

        // ── Nó 15: endEvent 'Done - Success' (_RNdJ116PEfGBBLgT-R5iuw) ────────
        return AtzintpcSeg043Outcome.DoneSuccess;
    }

    // ── Execução do subProcessScope (ActivitySet) ─────────────────────────────

    private async Task<SubProcessResult> ExecuteSubProcessScopeAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        CancellationToken ct)
    {
        // ── Nó 6: scriptTask 'Start TX' (_RNdKFF6PEfGBBLgT-R5iuw) ─────────────
        AtzintpcSeg043Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_RNdKFV6PEfGBBLgT-R5iuw) ──
        // Regra: RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT.
        // Ramo "Stillgood": SW_QRETRYCOUNT < MAXRETRIES → prossegue para AtualizarIntimacao.
        // Ramo oposto: motor esgotado → SetTechError e termina o ActivitySet.
        if (!AtzintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            AtzintpcSeg043Steps.SetTechError(ctx, "Maxretriesexceeded");
            // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ────────────────────
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
            // A aresta de REGRESSO é escrita explicitamente aqui (AC7).
            AtzintpcSeg043Steps.SetTechError(ctx, ex.Message);
            return SubProcessResult.TechError;
        }

        // ── Nó 9: gateway _RNdKGl6PEfGBBLgT-R5iuw ───────────────────────────
        // "A chamada a AtualizarIntimacao foi bem sucedida?"
        // Condição AppError: STATUS_CODE != "0".
        if (AtzintpcSeg043Steps.IsAppError(envelope))
        {
            // ── Nó 10: scriptTask 'Set App Error' (_RNdKGV6PEfGBBLgT-R5iuw) ──
            AtzintpcSeg043Steps.SetAppError(ctx, envelope);
            // ── Nó 11: gateway _RNdKG16PEfGBBLgT-R5iuw ─────────────────────
            // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ─────────────────────
            return SubProcessResult.AppError;
        }

        // STATUS_CODE == "0": chamada bem sucedida.
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        // ── Nó 11: gateway _RNdKG16PEfGBBLgT-R5iuw ──────────────────────────
        // ── Nó 12: endEvent _RNdKF16PEfGBBLgT-R5iuw ─────────────────────────
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
