#nullable enable

// Card: BUILD-POCEPATPROCESS-seg006
// Segmento: SC-POC_EpatProcess-001 · passos 7–8 · etapas 1, 2
// Processo: POC_EpatProcess · ordemNaJornada: 3

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Finalizar AIIM' (userTask)
/// até 'gateway <c>_Faq_RFqTEfG5K7mY0I3I6w</c>' (parallel AND-split) —
/// passos 7 a 8 do cenário SC-POC_EpatProcess-001, segmento ordemNaJornada=3.
///
/// Topologia (2 nós — todas as arestas vêm de transições reais do XPDL):
/// <code>
///   1  userTask  _xWNLe1qSEfG5K7mY0I3I6w  Finalizar AIIM        (entrouPor=fluxo)
///      │  regra: RI-formScript-POC_EpatProcess-FinalizarAIIM
///      │    AFR = IPEStarterUtil.GETATTRIBUTE("Name");
///      │    CNTINSTANCIASUF = 0;
///      ↓ aresta _HenM8FqTEfG5K7mY0I3I6w (UNCONDITIONAL)
///   2  gateway   _Faq_RFqTEfG5K7mY0I3I6w  (Parallel / AND-split) (entrouPor=fluxo)
///      ├─ aresta _IxqJM1qTEfG5K7mY0I3I6w → _IxqJMlqTEfG5K7mY0I3I6w  Existe Notificação?
///      └─ aresta _jIoc8FqTEfG5K7mY0I3I6w → _XWivF1qTEfG5K7mY0I3I6w  Set Nome Etapa 2
/// </code>
///
/// O gateway é do tipo <b>Parallel (AND-split)</b>: dispara os dois ramos
/// incondicionalmente e em simultâneo, sem guarda nem condição de desvio.
/// Toda a topologia vem de transições reais do XPDL — nenhuma aresta foi inventada.
/// </summary>
public sealed class PocEpatProcessSeg006Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg006) ──

    /// <summary>Nó 1 — userTask 'Finalizar AIIM'.</summary>
    public const string NodeFinalizarAiim = "_xWNLe1qSEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — gateway Parallel AND-split.</summary>
    public const string NodeGateway = "_Faq_RFqTEfG5K7mY0I3I6w";

    /// <summary>
    /// Terminal do ramo A do AND-split: 'Existe Notificação?' (<c>_IxqJMlqTEfG5K7mY0I3I6w</c>).
    /// </summary>
    public const string NodeExisteNotificacao = "_IxqJMlqTEfG5K7mY0I3I6w";

    /// <summary>
    /// Terminal do ramo B do AND-split: 'Set Nome Etapa 2' (<c>_XWivF1qTEfG5K7mY0I3I6w</c>).
    /// </summary>
    public const string NodeSetNomeEtapa2 = "_XWivF1qTEfG5K7mY0I3I6w";

    private readonly FinalizarAiimUseCase _finalizarAiim;

    /// <param name="finalizarAiim">Caso de uso para a userTask 'Finalizar AIIM'.</param>
    public PocEpatProcessSeg006Workflow(FinalizarAiimUseCase finalizarAiim)
    {
        _finalizarAiim = finalizarAiim;
    }

    /// <summary>
    /// Executa o troco: aguarda a submissão de 'Finalizar AIIM', aplica a regra
    /// <c>RI-formScript-POC_EpatProcess-FinalizarAIIM</c> e resolve o gateway
    /// Parallel AND-split, devolvendo os dois terminais alcançados.
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForFinalizarAiim">
    /// Delegate de interacção humana: suspende até o AFR submeter o formulário
    /// e devolve a função <c>getAttribute</c> que encapsula
    /// <c>IPEStarterUtil.GETATTRIBUTE</c> literalmente.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// Os dois terminais do AND-split:
    /// (<see cref="PocEpatProcessSeg006Terminal.ExisteNotificacao"/>,
    ///  <see cref="PocEpatProcessSeg006Terminal.SetNomeEtapa2"/>).
    /// Ambos são sempre devolvidos — o gateway Parallel dispara os dois ramos
    /// incondicionalmente.
    /// </returns>
    public async Task<(PocEpatProcessSeg006Terminal BranchA, PocEpatProcessSeg006Terminal BranchB)> ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task<Func<string, string>>> waitForFinalizarAiim,
        CancellationToken ct)
    {
        // ── ordem 1: userTask 'Finalizar AIIM' (_xWNLe1qSEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Aplica RI-formScript-POC_EpatProcess-FinalizarAIIM ao submeter o formulário.
        await _finalizarAiim.ExecuteAsync(caseRef, aiimCase, waitForFinalizarAiim, ct)
                            .ConfigureAwait(false);

        // ── aresta _HenM8FqTEfG5K7mY0I3I6w: UNCONDITIONAL → gateway ────────────

        // ── ordem 2: gateway _Faq_RFqTEfG5K7mY0I3I6w (Parallel / AND-split, entrouPor=fluxo) ──
        // Dois ramos incondicionais — toda a lógica vem do XPDL; nenhuma guarda foi adicionada.
        //   Ramo A — aresta _IxqJM1qTEfG5K7mY0I3I6w → Existe Notificação? (_IxqJMlqTEfG5K7mY0I3I6w)
        //   Ramo B — aresta _jIoc8FqTEfG5K7mY0I3I6w  → Set Nome Etapa 2   (_XWivF1qTEfG5K7mY0I3I6w)
        return (
            PocEpatProcessSeg006Terminal.ExisteNotificacao,
            PocEpatProcessSeg006Terminal.SetNomeEtapa2
        );
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 006 do POC_EpatProcess.
/// O gateway <c>_Faq_RFqTEfG5K7mY0I3I6w</c> é Parallel (AND-split):
/// ambos os terminais são sempre devolvidos em simultâneo.
/// </summary>
public enum PocEpatProcessSeg006Terminal
{
    /// <summary>
    /// Ramo A do AND-split — gateway 'Existe Notificação?' (<c>_IxqJMlqTEfG5K7mY0I3I6w</c>).
    /// Aresta XPDL: <c>_IxqJM1qTEfG5K7mY0I3I6w</c> (UNCONDITIONAL).
    /// </summary>
    ExisteNotificacao,

    /// <summary>
    /// Ramo B do AND-split — scriptTask 'Set Nome Etapa 2' (<c>_XWivF1qTEfG5K7mY0I3I6w</c>).
    /// Aresta XPDL: <c>_jIoc8FqTEfG5K7mY0I3I6w</c> (UNCONDITIONAL).
    /// </summary>
    SetNomeEtapa2,
}
