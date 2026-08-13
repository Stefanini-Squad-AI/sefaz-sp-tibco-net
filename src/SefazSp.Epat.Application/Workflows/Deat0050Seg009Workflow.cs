#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows;

/// <summary>
/// Topologia do segmento DEAT0050-seg009:
/// INICALC → CalculaPrazo → HoraFimSC → gateway(_lrer_VqhEfG5K7mY0I3I6w) → endEvent(_lrer2FqhEfG5K7mY0I3I6w)
///
/// Herdado de: POC_EpatProcess/Aguardar evento de Notificacao do AIIM
/// Etapas: 1, 2
///
/// Passo 1 — receiveTask INICALC (_lrer81qhEfG5K7mY0I3I6w):
///   Nó de entrada do segmento activado por evento externo (bookmark-correlation).
///   Não existe transição XPDL que chegue aqui de dentro do segmento.
///   O motor (Elsa) suspende a instância até o endpoint POST /api/deat0050/inicalc/resume
///   invocar ICorrelationStore.ResumeAsync com PROCESS_ID como chave.
///
/// Passo 2 — callActivity CalculaPrazo (_lrer3lqhEfG5K7mY0I3I6w):
///   Invoca o subprocesso CALCPRPC via contrato ICALCPRPC.
///   O duplo CALCPRPC está registado em Infrastructure/Integration.Doubles/CalcprpcDouble.
///
/// Passo 3 — scriptTask HoraFimSC (_lrer3VqhEfG5K7mY0I3I6w):
///   Cálculo de prazo; regra pura em Domain/Rules (IHoraFimScRule).
///   Envelope técnico (STATUS_CODE, contadores) em Application/Execution/HoraFimScExecution.
///   Builtin iProcess: IPEDateTimeUtil.CALCTIME → shim-tri-state (NOEQ-iprocess-builtin).
///
/// Passo 4 — gateway _lrer_VqhEfG5K7mY0I3I6w ("Já se esperou pelo prazo em vigor?"):
///   Condição XPDL (topologia como dado):
///     RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w:
///       DATACONTROLE == SW_NA || DATACONTROLE != PRAZODEFESA → ramo AguardaDefesa
///       OTHERWISE → endEvent _lrer2FqhEfG5K7mY0I3I6w  (este segmento)
///
/// Passo 5 — endEvent _lrer2FqhEfG5K7mY0I3I6w:
///   Encerra o subprocesso; regressa a POC_EpatProcess/Aguardar evento de Notificacao do AIIM.
///   Nenhuma acção adicional.
///
/// Rastreia: checklist ordens 1-5 do BUILD-DEAT0050-seg009
/// Oracle: SC-DEAT0050-002 · segmentos[1] · 1 caso · immutable
/// </summary>
public sealed class Deat0050Seg009Workflow
{
    // ── Identificadores de nó (imutáveis — não renomear) ───────────────────
    public const string NodeId_INICALC           = "_lrer81qhEfG5K7mY0I3I6w";
    public const string NodeId_CalculaPrazo      = "_lrer3lqhEfG5K7mY0I3I6w";
    public const string NodeId_HoraFimSC         = "_lrer3VqhEfG5K7mY0I3I6w";
    public const string NodeId_Gateway           = "_lrer_VqhEfG5K7mY0I3I6w";
    public const string NodeId_EndEvent          = "_lrer2FqhEfG5K7mY0I3I6w";

    // ── Bookmark de correlação para INICALC ─────────────────────────────────
    public const string Bookmark_INICALC = "INICALC";

    // ── Regra de topologia do gateway (dado, não código espalhado) ──────────
    /// <summary>
    /// RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w
    /// Expressão XPDL: DATACONTROLE == IPESystemValues.SW_NA || DATACONTROLE != PRAZODEFESA
    /// Quando verdadeiro → ramo AguardaDefesa (fora deste segmento).
    /// Quando falso (OTHERWISE) → endEvent _lrer2FqhEfG5K7mY0I3I6w.
    /// DATACONTROLE usa FieldValue&lt;DateOnly&gt; (sentinelaSwNa=true).
    /// </summary>
    public static readonly GatewayTransition GatewayRule =
        new(
            RuleId:      "RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w",
            Description: "Já se esperou pelo prazo em vigor?",
            ConditionBranch: "AguardaDefesa",
            OtherwiseBranch: NodeId_EndEvent);

    private readonly ICALCPRPC _calcPrazo;
    private readonly HoraFimScExecution _horaFimSc;

    public Deat0050Seg009Workflow(ICALCPRPC calcPrazo, HoraFimScExecution horaFimSc)
    {
        _calcPrazo = calcPrazo;
        _horaFimSc = horaFimSc;
    }

    /// <summary>
    /// Executa os passos 2-5 do segmento após a retomada de INICALC.
    /// O passo 1 (INICALC) é tratado pelo motor via bookmark — este método recebe
    /// o controlo após o resume.
    /// Devolve o identificador do nó de saída ("endEvent" ou "AguardaDefesa").
    /// </summary>
    public async Task<string> RunAsync(
        AiimCaseRef caseRef,
        AiimCase caseData,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // Passo 2 — callActivity CalculaPrazo
        await _calcPrazo.ExecuteAsync(caseRef, ct);

        // Passo 3 — scriptTask HoraFimSC (envelope técnico)
        var horaFimInput = new HoraFimScInput(
            DataControleSemana: caseData.DATACONTROLE,
            PrazoBase:          caseData.PRAZODEFESA);
        var horaFimResult = _horaFimSc.Execute(horaFimInput, ctx);

        // Actualiza o caso com o prazo calculado.
        caseData.PRAZODEFESA  = horaFimResult.Prazodefesa;
        caseData.PRAZODEFESAT = horaFimResult.Prazodefesat;

        // Passo 4 — gateway _lrer_VqhEfG5K7mY0I3I6w
        // Condição: DATACONTROLE == SW_NA || DATACONTROLE != PRAZODEFESA
        // Avaliação forçada pelo padrão exhaustivo do shim tri-estado.
        var takesConditionBranch = EvaluateGatewayRule(caseData);

        // Passo 5 — endEvent (ou ramo de desvio fora do segmento)
        return takesConditionBranch
            ? GatewayRule.ConditionBranch   // fora deste segmento
            : GatewayRule.OtherwiseBranch;  // NodeId_EndEvent — terminus do subprocesso
    }

    // ── Avaliação da regra como dado de topologia ────────────────────────────

    /// <summary>
    /// RI-transition-DEAT0050-gatewaylrerVqhEfG5K7mY0I3I6w:
    ///   DATACONTROLE == SW_NA || DATACONTROLE != PRAZODEFESA
    ///
    /// O pattern matching exaustivo em FieldValue&lt;T&gt; obriga a decisão explícita
    /// para cada um dos três estados (HasValue / NotAvailable / Empty).
    /// Colapsar SW_NA em null mudaria o ramo sem erro de compilação (shim-tri-state recusada).
    /// </summary>
    private static bool EvaluateGatewayRule(AiimCase caseData)
    {
        return caseData.DATACONTROLE.Match(
            hasValue:      v   => v != caseData.PRAZODEFESA,   // DATACONTROLE != PRAZODEFESA
            notAvailable: ()  => true,                          // DATACONTROLE == SW_NA
            empty:        ()  => true);                         // trata Empty como SW_NA por omissão
    }
}

/// <summary>
/// Estrutura de dados que captura a transição do gateway como topologia (não código espalhado).
/// Todos os desvios do gateway _lrer_VqhEfG5K7mY0I3I6w ficam declarados neste objecto.
/// </summary>
public sealed record GatewayTransition(
    string RuleId,
    string Description,
    string ConditionBranch,
    string OtherwiseBranch);
