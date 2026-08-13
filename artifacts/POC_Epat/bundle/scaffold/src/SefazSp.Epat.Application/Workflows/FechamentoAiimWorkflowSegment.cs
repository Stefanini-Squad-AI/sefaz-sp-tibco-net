// Topologia do segmento ordemNaJornada=1 — SC-POC_EpatProcess-032 (passos 2–4).
// Gateway _Faq_RFqTEfG5K7mY0I3I6w: parallelGateway (AND), step 55 do processo.
// Todos os identificadores são imutáveis: transcrição directa do XPDL.
#nullable enable

namespace SefazSp.Epat.Application.Workflows;

/// <summary>
/// Topologia do segmento de "Finalizar AIIM" até "Set Nome Etapa 2"
/// no processo POC_EpatProcess (cenário de referência SC-POC_EpatProcess-032, passos 2–4).
///
/// Nós:
///   1. _xWNLe1qSEfG5K7mY0I3I6w  userTask    "Finalizar AIIM"
///   2. _Faq_RFqTEfG5K7mY0I3I6w  gateway     parallel AND (step 55) — condição UNCONDITIONAL
///   3. _XWivF1qTEfG5K7mY0I3I6w  scriptTask  "Set Nome Etapa 2"
///
/// O gateway é AND (parallelGateway); a condição extraída do XPDL é UNCONDITIONAL.
/// A topologia é dado de fluxo, não código espalhado (entrouPor=fluxo nos três nós).
/// </summary>
public static class FechamentoAiimWorkflowSegment
{
    /// <summary>id XPDL da tarefa humana "Finalizar AIIM".</summary>
    public const string FinalizarAiimTaskId = "_xWNLe1qSEfG5K7mY0I3I6w";

    /// <summary>
    /// id XPDL do gateway paralelo AND.
    /// Tipo: parallelGateway (AND), step 55 — condição UNCONDITIONAL (sem desvio condicional).
    /// </summary>
    public const string GatewayId = "_Faq_RFqTEfG5K7mY0I3I6w";

    /// <summary>id XPDL do scriptTask "Set Nome Etapa 2".</summary>
    public const string SetNomeEtapa2TaskId = "_XWivF1qTEfG5K7mY0I3I6w";

    /// <summary>
    /// Sequência de nós do segmento ordemNaJornada=1 do cenário SC-POC_EpatProcess-032.
    /// Corresponde aos passos 2–4 da jornada do caso.
    /// </summary>
    public static readonly IReadOnlyList<string> PathNodes =
    [
        FinalizarAiimTaskId,
        GatewayId,
        SetNomeEtapa2TaskId,
    ];
}
