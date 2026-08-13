#nullable enable

// Card: BUILD-POCEPATPROCESS-seg025
// Segmento: SC-POC_EpatProcess-015 · passos 3–5 · etapa 2
// Processo: POC_EpatProcess · ordemNaJornada: 1

// NOEQ-non-interrupting-boundary (medium, DEFERIDO):
//   O nó _sfwu-VqUEfG5K7mY0I3I6w tem boundary event não-interrompente (deadline DTFIMCQ/HRFIMCQ).
//   Opção sugerida (review-dossier): parallel-branch. Aguarda decisão do gate humano.
//   Diferido aqui — não ignorado. O oráculo não é afectado.

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Preparar Notificacao' (userTask)
/// até 'Criar Notificacao' (callActivity) —
/// passos 3 a 5 do cenário SC-POC_EpatProcess-015, segmento ordemNaJornada=1.
///
/// Topologia (3 nós — todas as arestas vêm de transições reais do XPDL):
/// <code>
///   1  userTask     _sfwu-VqUEfG5K7mY0I3I6w  Preparar Notificacao   (entrouPor=fluxo)
///      │  [boundary não-interrompente: DTFIMCQ/HRFIMCQ — DEFERIDO, ver NOEQ-non-interrupting-boundary]
///      │  [boundary interrompente: 2 dias]
///      ↓ aresta _v7PXUFqUEfG5K7mY0I3I6w (UNCONDITIONAL)
///   2  gateway      _sJqYklqTEfG5K7mY0I3I6w  Corrigir?              (entrouPor=fluxo, Exclusive)
///      │  regra de transição: RI-transition-POC_EpatProcess-Corrigir (CORRECAO == true;)
///      ├─ aresta _tN6q41qTEfG5K7mY0I3I6w  "Sim"  CONDITION(CORRECAO == true) → _tN6q4lqTEfG5K7mY0I3I6w  Corrigir Fechamento
///      └─ aresta _80T7gFqUEfG5K7mY0I3I6w  "No"   OTHERWISE                   → _BQIgAF9KEfGqPfX31TKC3w  Criar Notificacao
///   3  callActivity _BQIgAF9KEfGqPfX31TKC3w  Criar Notificacao      (entrouPor=fluxo)
///      │  continuaEm: CRNOTPC · resolvidaPor: process · dinamica: false
/// </code>
///
/// O gateway é Exclusive (XOR): exactamente um ramo é percorrido.
/// </summary>
public sealed class PocEpatProcessSeg025Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg025) ──

    /// <summary>Nó 1 — userTask 'Preparar Notificacao'.</summary>
    public const string NodePrepararNotificacao = "_sfwu-VqUEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — gateway Exclusive 'Corrigir?'.</summary>
    public const string NodeCorrigirGateway = "_sJqYklqTEfG5K7mY0I3I6w";

    /// <summary>Nó 3 — callActivity 'Criar Notificacao' (callee: CRNOTPC).</summary>
    public const string NodeCriarNotificacao = "_BQIgAF9KEfGqPfX31TKC3w";

    /// <summary>Terminal do ramo 'Sim': linkThrow 'Corrigir Fechamento' (<c>_tN6q4lqTEfG5K7mY0I3I6w</c>).</summary>
    public const string NodeCorrigirFechamento = "_tN6q4lqTEfG5K7mY0I3I6w";

    private readonly PrepararNotificacaoUseCase _prepararNotificacao;
    private readonly Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> _crnotpc;

    /// <param name="prepararNotificacao">Caso de uso para a userTask 'Preparar Notificacao'.</param>
    /// <param name="crnotpc">
    /// Delegate que invoca o subprocesso CRNOTPC ('Criar Notificacao').
    /// Em testes, substituir pelo double <c>CRNOTPCDouble</c>; em produção, pelo adaptador real.
    /// </param>
    public PocEpatProcessSeg025Workflow(
        PrepararNotificacaoUseCase prepararNotificacao,
        Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> crnotpc)
    {
        _prepararNotificacao = prepararNotificacao;
        _crnotpc = crnotpc;
    }

    /// <summary>
    /// Executa o troco: aguarda a submissão de 'Preparar Notificacao', avalia o gateway
    /// <c>Corrigir?</c> pela regra <c>RI-transition-POC_EpatProcess-Corrigir</c>
    /// e, se o ramo for 'No', invoca o subprocesso 'Criar Notificacao' (CRNOTPC).
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForPrepararNotificacao">
    /// Delegate de interacção humana: suspende até o fiscal submeter o formulário.
    /// Devolve <see langword="Task"/> após a submissão.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O terminal alcançado após a avaliação do gateway.</returns>
    public async Task<PocEpatProcessSeg025Terminal> ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task> waitForPrepararNotificacao,
        CancellationToken ct)
    {
        // ── ordem 1: userTask 'Preparar Notificacao' (_sfwu-VqUEfG5K7mY0I3I6w, entrouPor=fluxo) ─
        await _prepararNotificacao.ExecuteAsync(caseRef, aiimCase, waitForPrepararNotificacao, ct)
                                   .ConfigureAwait(false);

        // ── aresta _v7PXUFqUEfG5K7mY0I3I6w: UNCONDITIONAL → gateway ─────────────

        // ── ordem 2: gateway 'Corrigir?' (_sJqYklqTEfG5K7mY0I3I6w, Exclusive, entrouPor=fluxo) ─
        // Regra RI-transition-POC_EpatProcess-Corrigir (POC_Epat.xpdl linha 2339):
        //   Ramo "Sim" (_tN6q41qTEfG5K7mY0I3I6w): CORRECAO == true  → Corrigir Fechamento
        //   Ramo "No"  (_80T7gFqUEfG5K7mY0I3I6w): OTHERWISE          → Criar Notificacao
        if (aiimCase.CORRECAO)
        {
            // Ramo "Sim" — aresta _tN6q41qTEfG5K7mY0I3I6w → linkThrow Corrigir Fechamento
            return PocEpatProcessSeg025Terminal.CorrigirFechamento;
        }

        // ── Ramo "No" (OTHERWISE): aresta _80T7gFqUEfG5K7mY0I3I6w → callActivity ─

        // ── ordem 3: callActivity 'Criar Notificacao' (_BQIgAF9KEfGqPfX31TKC3w, entrouPor=fluxo) ─
        // callee: CRNOTPC · resolvidaPor: process · dinamica: false
        // A chamada é estática: o destino CRNOTPC é fixo no XPDL (dynamic=false).
        await _crnotpc(caseRef, ct).ConfigureAwait(false);

        return PocEpatProcessSeg025Terminal.CriarNotificacao;
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 025 do POC_EpatProcess.
/// O gateway <c>_sJqYklqTEfG5K7mY0I3I6w</c> é Exclusive (XOR): apenas um terminal é devolvido.
/// </summary>
public enum PocEpatProcessSeg025Terminal
{
    /// <summary>
    /// Ramo "No" (OTHERWISE) — callActivity 'Criar Notificacao' (<c>_BQIgAF9KEfGqPfX31TKC3w</c>).
    /// Aresta XPDL: <c>_80T7gFqUEfG5K7mY0I3I6w</c> (OTHERWISE).
    /// O subprocesso CRNOTPC foi invocado antes de devolver este terminal.
    /// </summary>
    CriarNotificacao,

    /// <summary>
    /// Ramo "Sim" — linkThrow 'Corrigir Fechamento' (<c>_tN6q4lqTEfG5K7mY0I3I6w</c>).
    /// Aresta XPDL: <c>_tN6q41qTEfG5K7mY0I3I6w</c> (CONDITION: CORRECAO == true).
    /// O segmento SEG014 (Corrigir Fechamento → linkCatch) deve ser invocado a seguir.
    /// </summary>
    CorrigirFechamento,
}
