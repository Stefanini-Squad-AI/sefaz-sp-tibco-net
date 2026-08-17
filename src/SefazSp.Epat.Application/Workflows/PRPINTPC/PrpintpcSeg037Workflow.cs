#nullable enable

using System.Globalization;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.PRPINTPC;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.PRPINTPC;

/// <summary>
/// Workflow do segmento 037 do processo PRPINTPC: de 'Start Event' a 'Done - Success'.
///
/// Card: BUILD-PRPINTPC-seg037 · Processo: PRPINTPC · Etapas: 3, 4
/// Cenário de referência: SC-PRPINTPC-010, segmento 1, passos 1–15.
///
/// Herdado de POC_EpatProcess/Prepara Intimação.
///
/// Topologia dos 15 nós (percurso de referência SC-PRPINTPC-010, segmento 1):
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
/// │        NOEQ-iprocess-builtin: STSADMTITCNT e STSADMTITDRF comparados com SW_NA│
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _KEwC7l6EEfGBBLgT-R5iuw  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC4                         │
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
/// │       │       (convergência: AppError ou Success)                          │  │
/// │       │  ↓ fluxo                                                           │  │
/// │       │ [12] endEvent            _KEwDU16EEfGBBLgT-R5iuw  endEvent        │  │
/// │       └────────────────────────────────────────────────────────────────────┘  │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC7                       │
/// │  [13] Tech Error                _KEwC7V6EEfGBBLgT-R5iuw  gateway             │
/// │        ramo "No" (otherwise): → App Error                                     │
/// │   ↓ ramo "No"                                                                 │
/// │  [14] App Error                 _KEwC7F6EEfGBBLgT-R5iuw  gateway             │
/// │        ramo "No" (otherwise): → Done - Success                                │
/// │   ↓ ramo "No"                                                                 │
/// │  [15] Done - Success            _KEwC616EEfGBBLgT-R5iuw  endEvent            │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw)
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error (_KEwC7V6EEfGBBLgT-R5iuw)
///
/// Este segmento é one-shot: o percurso corre o sub-processo uma única vez e
/// encerra sempre em 'Done - Success', seja STATUS_CODE == "0" (sucesso) ou != "0"
/// (AppError registado em Set App Error). Não há laço de retentativas nem
/// tarefa de Manipular Excecao no âmbito MAIN deste segmento.
///
/// Impacto da correcção CLONE-PRPINTPC:
///   Com a condição corrigida STATUS_CODE != "0", o cenário SC-PRPINTPC-010 testa
///   especificamente o caso em que CaptaParametros devolve STATUS_CODE = SW_NA
///   (terceiro estado, não disponível). No XPDL original, STATUS_CODE != SW_NA
///   seria FALSE para STATUS_CODE = SW_NA, pelo que o ramo AppError NÃO seria tomado.
///   Com a correcção (STATUS_CODE != "0"), SW_NA != "0" é TRUE, e o ramo AppError
///   É tomado — confirmando que o defeito de cópia foi corrigido.
/// </summary>
public sealed class PrpintpcSeg037Workflow
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

    /// <summary>
    /// Nó 9  — gateway (ActivitySet). Decisão: A chamada a CaptaParametros foi bem sucedida?
    /// Condição corrigida: STATUS_CODE != "0"
    /// (rulings.CLONE-PRPINTPC: XPDL original usava STATUS_CODE != SW_NA — defeito de cópia).
    /// </summary>
    public const string NodeGatewayCallResult  = "_KEwDVl6EEfGBBLgT-R5iuw";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError        = "_KEwDVV6EEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — gateway de convergência interno (ActivitySet).</summary>
    public const string NodeGatewayInnerMerge  = "_KEwDV16EEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — endEvent interno (ActivitySet).</summary>
    public const string NodeEndEventInternal   = "_KEwDU16EEfGBBLgT-R5iuw";

    /// <summary>Nó 13 — Tech Error (gateway, MAIN). Alcançado por regresso explícito — não existe transição XPDL.</summary>
    public const string NodeTechError          = "_KEwC7V6EEfGBBLgT-R5iuw";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError           = "_KEwC7F6EEfGBBLgT-R5iuw";

    /// <summary>Nó 15 — Done - Success (endEvent, MAIN). Ponto de saída do segmento.</summary>
    public const string NodeDoneSuccess        = "_KEwC616EEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó auxiliar — Set Technical Error (scriptTask, ActivitySet).
    /// Visitado noutra passagem pelo mesmo troco (SC-PRPINTPC-016).
    /// Não faz parte do percurso de referência SC-PRPINTPC-010.
    /// </summary>
    public const string NodeSetTechnicalError  = "_KEwDVF6EEfGBBLgT-R5iuw";

    // ─────────────────────────────────────────────────────────────────────────

    private readonly IEpatServices _services;

    public PrpintpcSeg037Workflow(IEpatServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Executa o segmento completo (passos 1–15 do cenário SC-PRPINTPC-010),
    /// percorrendo Start Event → SetParameters → Start Loop → sub-processo (one-shot)
    /// → Tech Error (No) → App Error (No) → Done - Success.
    ///
    /// O segmento é one-shot: não há laço de retentativas nem Manipular Excecao
    /// no âmbito MAIN. Seja STATUS_CODE == "0" (sucesso) ou != "0" (AppError
    /// registado em Set App Error), o desfecho é sempre Done - Success.
    /// </summary>
    /// <param name="caseRef">Identidade do caso.</param>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="aiimCase">
    ///   Estado de negócio do caso. Usado para avaliar a regra
    ///   RI-script-PRPINTPC-StartLoop (campos INSTANCIA, STSADMTITCNT, STSADMTITDRF).
    /// </param>
    /// <param name="swQRetryCount">
    ///   Valor de <c>IPESystemValues.SW_QRETRYCOUNT</c> fornecido pelo runtime iProcess.
    ///   Lido pelo gateway Check Retries; nunca escrito pelo processo.
    ///   NOEQ-iprocess-builtin, ratificado 2026-08-06.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task RunAsync(
        AiimCaseRef           caseRef,
        ProcessExecutionContext ctx,
        AiimCase              aiimCase,
        long                  swQRetryCount,
        CancellationToken     ct)
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

        // ── Nó 3: scriptTask 'Start Loop' (_KEwC4F6EEfGBBLgT-R5iuw) ──────────
        // Regra: RI-script-PRPINTPC-StartLoop.
        // NOEQ-iprocess-builtin: STSADMTITCNT e STSADMTITDRF comparados com SW_NA
        //   via FieldValue<T> (shim-tri-state). SW_NA NUNCA é mapeado para null.
        // Segmento one-shot: NUMAPPRETRIES é sempre reinicializado a 0 neste percurso.
        if (PrpintpcStartLoopRule.ShouldInitialize(
                numAppRetries: null,  // first (and only) pass: treat as not yet initialized
                instancia:     aiimCase.INSTANCIA,
                stsadmTitCnt:  aiimCase.STSADMTITCNT,
                stsadmTitDrf:  aiimCase.STSADMTITDRF))
        {
            PrpintpcSeg037Steps.ApplyStartLoop(ctx);
        }

        // ── Nó 4: subProcessScope 'Control System Task Call' (_KEwC7l6EEfGBBLgT-R5iuw) ──
        // ── Nó 5: startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw) ─────────────────
        // DESCIDA EXPLÍCITA: não existe transição XPDL do subProcessScope para o startEvent
        // interno. A aresta é escrita explicitamente neste workflow (AC4).
        await ExecuteSubProcessScopeAsync(caseRef, ctx, swQRetryCount, ct)
            .ConfigureAwait(false);

        // ── Nó 13: gateway 'Tech Error' (_KEwC7V6EEfGBBLgT-R5iuw) ────────────
        // REGRESSO EXPLÍCITO: não existe transição XPDL do endEvent interno de volta ao MAIN.
        // Alcançado por regresso do sub-processo (AC7).
        // Ramo "No" (otherwise): ISTECHERROR != "Y" → App Error.
        // Ramo "Yes": ISTECHERROR == "Y" (tratado noutros cenários, e.g. SC-PRPINTPC-016).
        // Neste segmento one-shot, o controlo prossegue sempre para o nó 14.

        // ── Nó 14: gateway 'App Error' (_KEwC7F6EEfGBBLgT-R5iuw) ─────────────
        // Ramo "No" (otherwise) → Done - Success.
        // Ramo "Yes": ISAPPERROR == "Y" → laço de retentativas (tratado em seg035/SC-PRPINTPC-009).
        // Neste segmento one-shot, o controlo prossegue sempre para o nó 15.

        // ── Nó 15: endEvent 'Done - Success' (_KEwC616EEfGBBLgT-R5iuw) ────────
        // Ponto de saída. O segmento regressa ao chamador (POC_EpatProcess/Prepara Intimação).
    }

    // ── Execução do subProcessScope (ActivitySet) ─────────────────────────────

    /// <summary>
    /// Modela o sub-processo 'Control System Task Call' (_KEwC7l6EEfGBBLgT-R5iuw)
    /// com descida explícita para o startEvent interno (_KEwDUl6EEfGBBLgT-R5iuw)
    /// e regresso explícito para o gateway Tech Error (_KEwC7V6EEfGBBLgT-R5iuw).
    ///
    /// A descida e o regresso NÃO existem como transições no XPDL.
    /// São escritos explicitamente neste método (AC4, AC7).
    /// </summary>
    private async Task ExecuteSubProcessScopeAsync(
        AiimCaseRef           caseRef,
        ProcessExecutionContext ctx,
        long                  swQRetryCount,
        CancellationToken     ct)
    {
        // ── Nó 6: scriptTask 'Start TX' (_KEwDUF6EEfGBBLgT-R5iuw) ─────────────
        // Reinicia os indicadores de erro antes de iniciar a transacção de serviço.
        PrpintpcSeg035Steps.ApplyStartTx(ctx);

        // ── Nó 7: gateway 'Check Retries SW_QRETRYCOUNT' (_KEwDUV6EEfGBBLgT-R5iuw) ──
        // Regra: RI-transition-PRPINTPC-CheckRetriesSWQRETRYCOUNT.
        // Ramo "Stillgood": SW_QRETRYCOUNT < MAXRETRIES → prossegue para CaptaParametros.
        // Ramo oposto: retentativas do motor esgotadas; SetTechError e encerra o ActivitySet
        // (este ramo é coberto pelo cenário SC-PRPINTPC-016, não pelo SC-PRPINTPC-010).
        if (!PrpintpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            PrpintpcSeg035Steps.SetTechError(ctx, "MaxRetriesExceeded");
            // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ────────────────────
            // Regresso explícito ao MAIN (não existe transição XPDL).
            return;
        }

        ServiceEnvelope envelope;
        try
        {
            // ── Nó 8: serviceTask 'CaptaParametros' (_KEwDWF6EEfGBBLgT-R5iuw) ──
            // Operação: PrepararintimacaoAsync (DecisionsEPAT.wsdl).
            // A chamada ao Decisions integra e verifica o retorno conforme AC5/AC6.
            envelope = await _services
                .PrepararintimacaoAsync(caseRef, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Excepção de transporte: não existe transição XPDL para o gateway Tech Error.
            // A aresta de REGRESSO é escrita explicitamente aqui (AC7).
            PrpintpcSeg035Steps.SetTechError(ctx, ex.Message);
            // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ────────────────────
            return;
        }

        // ── Nó 9: gateway _KEwDVl6EEfGBBLgT-R5iuw ───────────────────────────
        // "A chamada a CaptaParametros foi bem sucedida?"
        // ATENÇÃO — defeito de cópia corrigido (rulings.CLONE-PRPINTPC, AC6):
        //   XPDL original: STATUS_CODE != IPESystemValues.SW_NA
        //   Corrigido para: STATUS_CODE != "0"   (alinhado com processos irmãos)
        // Impacto (nota para demonstração):
        //   STATUS_CODE = SW_NA (terceiro estado — "não disponível") agora activa
        //   correctamente o ramo AppError (SW_NA != "0" = true), ao contrário do
        //   XPDL original onde STATUS_CODE != SW_NA seria false para SW_NA.
        if (PrpintpcSeg035Steps.IsAppError(envelope))
        {
            // ── Nó 10: scriptTask 'Set App Error' (_KEwDVV6EEfGBBLgT-R5iuw) ──
            PrpintpcSeg035Steps.SetAppError(ctx, envelope);
            // ── Nó 11: gateway _KEwDV16EEfGBBLgT-R5iuw (convergência) ────────
            // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ────────────────────
            return;
        }

        // STATUS_CODE == "0": chamada bem sucedida.
        PrpintpcSeg035Steps.MapServiceEnvelopeSuccess(ctx, envelope);
        // ── Nó 11: gateway _KEwDV16EEfGBBLgT-R5iuw (convergência) ──────────
        // ── Nó 12: endEvent _KEwDU16EEfGBBLgT-R5iuw ────────────────────────
    }

    // ── Auxiliares ────────────────────────────────────────────────────────────

    /// <summary>
    /// Extrai e classifica o campo IDPROCESSO do ProcessId do caso usando o shim tri-estado.
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
}
