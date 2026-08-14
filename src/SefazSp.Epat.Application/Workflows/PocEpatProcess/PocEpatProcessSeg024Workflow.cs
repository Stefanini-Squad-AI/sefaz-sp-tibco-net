#nullable enable

// Card: BUILD-POCEPATPROCESS-seg024
// Segmento: SC-POC_EpatProcess-001 · passos 3–4 · etapa 2
// Processo: POC_EpatProcess · ordemNaJornada: 1

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Preparar Notificacao' (userTask)
/// até o gateway 'Corrigir?' (Exclusive / XOR) —
/// passos 3 a 4 do cenário SC-POC_EpatProcess-001, segmento ordemNaJornada=1.
///
/// Topologia (2 nós — todas as arestas vêm de transições reais do XPDL):
/// <code>
///   1  userTask  _sfwu-VqUEfG5K7mY0I3I6w  Preparar Notificacao  (entrouPor=fluxo)
///      │  (sem regra RI-formScript identificada; a submissão produz CORRECAO)
///      ↓ aresta _v7PXUFqUEfG5K7mY0I3I6w (UNCONDITIONAL)
///   2  gateway   _sJqYklqTEfG5K7mY0I3I6w  Corrigir?             (entrouPor=fluxo)
///      ├─ aresta _tN6q41qTEfG5K7mY0I3I6w (CONDITION: CORRECAO == true)
///      │    → _tN6q4lqTEfG5K7mY0I3I6w  linkThrow "Corrigir Fechamento"
///      └─ aresta _80T7gFqUEfG5K7mY0I3I6w (OTHERWISE, isDefault=true)
///           → _BQIgAF9KEfGqPfX31TKC3w  "Criar Notificacao" (call activity)
/// </code>
///
/// A condição de desvio é implementada integralmente pela regra de negócio
/// <c>RI-transition-POC_EpatProcess-Corrigir</c>; nenhuma lógica de decisão
/// foi adicionada fora dessa regra.
///
/// O campo <c>CORRECAO</c> (tipo bool; glossário POC_Epat.yaml) é lido
/// directamente de <see cref="AiimCase"/> sem conversão nem renomeação,
/// e comparado com <c>== true</c> conforme a expressão original do XPDL.
/// </summary>
public sealed class PocEpatProcessSeg024Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg024) ──

    /// <summary>Nó 1 — userTask 'Preparar Notificacao'.</summary>
    public const string NodePrepararNotificacao = "_sfwu-VqUEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — gateway 'Corrigir?' (Exclusive / XOR).</summary>
    public const string NodeCorrigir = "_sJqYklqTEfG5K7mY0I3I6w";

    /// <summary>Terminal "Sim" — linkThrow "Corrigir Fechamento" (<c>_tN6q4lqTEfG5K7mY0I3I6w</c>).</summary>
    public const string NodeCorrigirFechamento = "_tN6q4lqTEfG5K7mY0I3I6w";

    /// <summary>Terminal "Não" — call activity "Criar Notificacao" (<c>_BQIgAF9KEfGqPfX31TKC3w</c>).</summary>
    public const string NodeCriarNotificacao = "_BQIgAF9KEfGqPfX31TKC3w";

    private readonly PrepararNotificacaoUseCase _prepararNotificacao;

    /// <param name="prepararNotificacao">Caso de uso para a userTask 'Preparar Notificacao'.</param>
    public PocEpatProcessSeg024Workflow(PrepararNotificacaoUseCase prepararNotificacao)
    {
        _prepararNotificacao = prepararNotificacao;
    }

    /// <summary>
    /// Executa o troco: aguarda a submissão de 'Preparar Notificacao' e avalia o
    /// gateway 'Corrigir?' pela regra <c>RI-transition-POC_EpatProcess-Corrigir</c>,
    /// devolvendo o terminal alcançado.
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">
    /// Estado de negócio do caso — fornece o campo <c>CORRECAO</c>
    /// (<see cref="AiimCase.CORRECAO"/>) que determina o ramo de saída do gateway.
    /// </param>
    /// <param name="waitForPrepararNotificacao">
    /// Delegate de interacção humana: suspende o workflow até o fiscal submeter
    /// o formulário 'Preparar Notificacao'. Devolve <see langword="Task"/> após a submissão.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// <see cref="PocEpatProcessSeg024Terminal.CorrigirFechamento"/> quando
    /// <c>CORRECAO == true</c> (ramo "Sim", aresta XPDL <c>_tN6q41qTEfG5K7mY0I3I6w</c>);
    /// caso contrário <see cref="PocEpatProcessSeg024Terminal.CriarNotificacao"/>
    /// (ramo OTHERWISE/default, aresta XPDL <c>_80T7gFqUEfG5K7mY0I3I6w</c>).
    /// </returns>
    public async Task<PocEpatProcessSeg024Terminal> ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task> waitForPrepararNotificacao,
        CancellationToken ct)
    {
        // ── ordem 1: userTask 'Preparar Notificacao' (_sfwu-VqUEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Sem regra RI-formScript identificada; a submissão produz o campo CORRECAO.
        await _prepararNotificacao.ExecuteAsync(caseRef, aiimCase, waitForPrepararNotificacao, ct)
                                  .ConfigureAwait(false);

        // ── aresta _v7PXUFqUEfG5K7mY0I3I6w: UNCONDITIONAL → gateway ─────────────

        // ── ordem 2: gateway 'Corrigir?' (_sJqYklqTEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Regra de negócio: RI-transition-POC_EpatProcess-Corrigir
        // Expressão original (POC_Epat.xpdl, linha 2339): CORRECAO == true;
        // O campo é lido sem conversão nem renomeação — o nome iProcess é o identificador de domínio.
        if (aiimCase.CORRECAO == true)
        {
            // Ramo "Sim" — aresta _tN6q41qTEfG5K7mY0I3I6w → linkThrow "Corrigir Fechamento" _tN6q4lqTEfG5K7mY0I3I6w
            return PocEpatProcessSeg024Terminal.CorrigirFechamento;
        }

        // Ramo "Não" (OTHERWISE, isDefault=true) — aresta _80T7gFqUEfG5K7mY0I3I6w
        // → call activity "Criar Notificacao" _BQIgAF9KEfGqPfX31TKC3w
        return PocEpatProcessSeg024Terminal.CriarNotificacao;
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 024 do POC_EpatProcess.
/// O gateway <c>_sJqYklqTEfG5K7mY0I3I6w</c> é Exclusive (XOR): exactamente um terminal
/// é devolvido por execução.
/// </summary>
public enum PocEpatProcessSeg024Terminal
{
    /// <summary>
    /// Ramo "Sim" — linkThrow "Corrigir Fechamento" (<c>_tN6q4lqTEfG5K7mY0I3I6w</c>).
    /// Condição XPDL (aresta <c>_tN6q41qTEfG5K7mY0I3I6w</c>): <c>CORRECAO == true</c>.
    /// Regra: <c>RI-transition-POC_EpatProcess-Corrigir</c>.
    /// </summary>
    CorrigirFechamento,

    /// <summary>
    /// Ramo "Não" (OTHERWISE, default) — call activity "Criar Notificacao" (<c>_BQIgAF9KEfGqPfX31TKC3w</c>).
    /// Aresta XPDL: <c>_80T7gFqUEfG5K7mY0I3I6w</c> (conditionType=OTHERWISE, isDefault=true).
    /// </summary>
    CriarNotificacao,
}
