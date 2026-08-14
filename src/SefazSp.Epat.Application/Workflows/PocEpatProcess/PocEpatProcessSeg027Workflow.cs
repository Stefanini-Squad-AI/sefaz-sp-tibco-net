#nullable enable

// Card: BUILD-POCEPATPROCESS-seg027
// Segmento: SC-POC_EpatProcess-017 · passos 3–4 · etapa 2
// Processo: POC_EpatProcess · ordemNaJornada: 1

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Preparar Notificacao' (userTask)
/// até 'Fim de Prazo Mantendo Atividade' (timerEvent, fronteira paralela não interruptiva) —
/// passos 3 a 4 do cenário SC-POC_EpatProcess-017, segmento ordemNaJornada=1.
///
/// Topologia (2 nós):
/// <code>
///   1  userTask   _sfwu-VqUEfG5K7mY0I3I6w  Preparar Notificacao        (entrouPor=fluxo)
///      │  sem regra RI-formScript-* declarada no rule-catalogue
///      │
///      ╠═══ ramo lateral paralelo (fronteira não interruptiva) ══════════════════╗
///      │                                                                         ↓
///      │  2  timerEvent  _XWivFlqTEfG5K7mY0I3I6w  Fim de Prazo Mantendo Atividade
///      │       regra: RI-deadline-POC_EpatProcess-FimdePrazoMantendoAtividade
///      │       deadline: DTFIMCQ (DateOnly) + HRFIMCQ (TimeOnly) → instante absoluto
///      │       classificação: fixa-prazo (compromisso de tempo); portador=deadline
///      │       ↓ (ramo lateral prossegue para 'Email CQ Fechamento')
///      ↓
///   host continua (não cancelada pelo timer)
/// </code>
///
/// <b>Decisões do glossário aplicadas:</b>
/// <list type="bullet">
///   <item>
///     <c>NOEQ-expression-deadline</c> (opção <c>absolute-instant</c>, ratificada):
///     o prazo combina <c>DTFIMCQ</c> + <c>HRFIMCQ</c> num <c>DateTime</c> absoluto
///     no momento do agendamento. <c>IClock</c> injectado — nunca <c>DateTime.Now</c>.
///   </item>
///   <item>
///     <c>NOEQ-non-interrupting-boundary</c> (opção <c>parallel-branch</c>, ratificada):
///     o timer dispara num ramo lateral concorrente; a tarefa hospedeira
///     <c>_sfwu-VqUEfG5K7mY0I3I6w</c> (<em>Preparar Notificacao</em>) <b>não é cancelada</b>.
///     O ramo lateral permanece visível no diagrama do processo.
///   </item>
/// </list>
///
/// <b>Nó sem transição XPDL — escrito explicitamente no fluxo .NET:</b><br/>
/// Ordem 2 (<c>_XWivFlqTEfG5K7mY0I3I6w</c>, entrouPor=fronteira-paralela):
/// não existe transição XPDL correspondente. O ramo paralelo de fronteira não interruptiva
/// é escrito explicitamente neste ficheiro, conforme <c>content.checklist</c> ordem 2.
/// </summary>
public sealed class PocEpatProcessSeg027Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg027) ──

    /// <summary>Nó 1 — userTask 'Preparar Notificacao' (tarefa hospedeira).</summary>
    public const string NodePrepararNotificacao = "_sfwu-VqUEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — timerEvent 'Fim de Prazo Mantendo Atividade' (ramo lateral paralelo).</summary>
    public const string NodeFimDePrazoMantendoAtividade = "_XWivFlqTEfG5K7mY0I3I6w";

    private readonly PrepararNotificacaoUseCase _prepararNotificacao;
    private readonly IClock _clock;

    /// <param name="prepararNotificacao">Caso de uso para a userTask 'Preparar Notificacao'.</param>
    /// <param name="clock">
    /// Relógio injectado — nunca <c>DateTime.Now</c> nem <c>DateTimeOffset.Now</c>.
    /// </param>
    public PocEpatProcessSeg027Workflow(PrepararNotificacaoUseCase prepararNotificacao, IClock clock)
    {
        _prepararNotificacao = prepararNotificacao;
        _clock = clock;
    }

    /// <summary>
    /// Executa o troco: arma o timer 'Fim de Prazo Mantendo Atividade' com o instante
    /// absoluto calculado a partir de <c>DTFIMCQ</c> + <c>HRFIMCQ</c>, inicia a tarefa
    /// hospedeira 'Preparar Notificacao' e o ramo lateral do timer <b>em paralelo</b>,
    /// e devolve o resultado de ambos quando terminam.
    ///
    /// A tarefa hospedeira <b>não é cancelada</b> quando o timer dispara
    /// (decisão <c>NOEQ-non-interrupting-boundary</c>, opção <c>parallel-branch</c>).
    /// Ambas as branches correm até à conclusão — <see cref="Task.WhenAll"/>.
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForPrepararNotificacao">
    /// Delegate de interacção humana: suspende até o fiscal submeter o formulário
    /// 'Preparar Notificacao'.
    /// </param>
    /// <param name="waitForDeadline">
    /// Delegate de tempo: suspende até o instante absoluto informado ser atingido.
    /// Injectado para manter o código testável sem recorrer a <c>Task.Delay</c> real.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// <see cref="PocEpatProcessSeg027Terminals"/> com ambos os terminais alcançados:
    /// <list type="bullet">
    ///   <item><see cref="PocEpatProcessSeg027Terminals.Host"/> —
    ///     <see cref="PocEpatProcessSeg027HostTerminal.PrepararNotificacaoConcluida"/>
    ///     quando a tarefa hospedeira termina normalmente.</item>
    ///   <item><see cref="PocEpatProcessSeg027Terminals.Lateral"/> —
    ///     <see cref="PocEpatProcessSeg027LateralTerminal.FimDePrazoMantendoAtividadeDisparado"/>
    ///     quando o timer dispara no ramo lateral.</item>
    /// </list>
    /// </returns>
    public async Task<PocEpatProcessSeg027Terminals> ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task> waitForPrepararNotificacao,
        Func<DateTimeOffset, CancellationToken, Task> waitForDeadline,
        CancellationToken ct)
    {
        // ── Armar o timer com instante absoluto (absolute-instant) ────────────────
        // Decisão NOEQ-expression-deadline: combina DTFIMCQ (DateOnly) + HRFIMCQ (TimeOnly)
        // num DateTimeOffset absoluto no fuso America/Sao_Paulo.
        // IClock injectado — NUNCA DateTime.Now.
        var deadline = PocEpatProcessSeg027DeadlineRules.ComputeFimDePrazoDeadline(aiimCase, _clock);

        // ── Iniciar ambos os ramos em paralelo (parallel-branch) ─────────────────
        // Decisão NOEQ-non-interrupting-boundary: o ramo lateral NÃO cancela o hospedeiro.
        // Cada ramo corre com o mesmo CancellationToken do caller — o timer não cria
        // um CancellationTokenSource próprio para o hospedeiro: são independentes.

        // Ramo hospedeiro: ordem 1 — userTask 'Preparar Notificacao'
        // nodeId: _sfwu-VqUEfG5K7mY0I3I6w · entrouPor: fluxo
        var hostTask = RunHostBranchAsync(caseRef, aiimCase, waitForPrepararNotificacao, ct);

        // Ramo lateral paralelo: ordem 2 — timerEvent 'Fim de Prazo Mantendo Atividade'
        // nodeId: _XWivFlqTEfG5K7mY0I3I6w · entrouPor: fronteira-paralela
        // NAO existe transição XPDL — aresta escrita explicitamente aqui.
        var lateralTask = RunLateralBranchAsync(deadline, waitForDeadline, ct);

        // ── Aguardar ambos (semântica parallel-branch: ambos correm até ao fim) ───
        await Task.WhenAll(hostTask, lateralTask).ConfigureAwait(false);

        return new PocEpatProcessSeg027Terminals(
            Host: PocEpatProcessSeg027HostTerminal.PrepararNotificacaoConcluida,
            Lateral: PocEpatProcessSeg027LateralTerminal.FimDePrazoMantendoAtividadeDisparado);
    }

    // ── Ramo hospedeiro ───────────────────────────────────────────────────────

    private async Task RunHostBranchAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task> waitForPrepararNotificacao,
        CancellationToken ct)
    {
        // ordem 1: userTask 'Preparar Notificacao' (_sfwu-VqUEfG5K7mY0I3I6w, entrouPor=fluxo)
        // O fiscal de renda preenche o formulário; o caso de uso aguarda a submissão.
        await _prepararNotificacao
            .ExecuteAsync(caseRef, aiimCase, waitForPrepararNotificacao, ct)
            .ConfigureAwait(false);
    }

    // ── Ramo lateral paralelo ─────────────────────────────────────────────────

    private static async Task RunLateralBranchAsync(
        DateTimeOffset deadline,
        Func<DateTimeOffset, CancellationToken, Task> waitForDeadline,
        CancellationToken ct)
    {
        // ordem 2: timerEvent 'Fim de Prazo Mantendo Atividade'
        // nodeId: _XWivFlqTEfG5K7mY0I3I6w · entrouPor: fronteira-paralela (não interruptiva)
        //
        // Aguarda o instante absoluto calculado pela regra de prazo.
        // Quando o instante é atingido, a regra RI-deadline-POC_EpatProcess-FimdePrazoMantendoAtividade
        // é considerada invocada: é ela que determina o instante do disparo (fixa-prazo,
        // portador=deadline). A tarefa hospedeira continua a correr de forma independente.
        await waitForDeadline(deadline, ct).ConfigureAwait(false);

        // RI-deadline-POC_EpatProcess-FimdePrazoMantendoAtividade — INVOCADA.
        // Efeito: o timer disparou no instante DTFIMCQ+HRFIMCQ.
        // O ramo lateral prossegue para o nó seguinte no fluxo do processo
        // (fora deste segmento: 'Email CQ Fechamento' → 'Fim E-mail').
    }
}

/// <summary>
/// Resultado do segmento 027 do POC_EpatProcess:
/// ambas as branches do ramo paralelo não interruptivo.
/// </summary>
/// <param name="Host">Terminal do ramo hospedeiro (sempre
/// <see cref="PocEpatProcessSeg027HostTerminal.PrepararNotificacaoConcluida"/>
/// quando a tarefa humana é submetida).</param>
/// <param name="Lateral">Terminal do ramo lateral (sempre
/// <see cref="PocEpatProcessSeg027LateralTerminal.FimDePrazoMantendoAtividadeDisparado"/>
/// quando o timer alcança o instante DTFIMCQ+HRFIMCQ).</param>
public readonly record struct PocEpatProcessSeg027Terminals(
    PocEpatProcessSeg027HostTerminal Host,
    PocEpatProcessSeg027LateralTerminal Lateral);

/// <summary>
/// Terminal do ramo hospedeiro do segmento 027.
/// </summary>
public enum PocEpatProcessSeg027HostTerminal
{
    /// <summary>
    /// A tarefa hospedeira 'Preparar Notificacao' (<c>_sfwu-VqUEfG5K7mY0I3I6w</c>)
    /// foi submetida normalmente pelo fiscal de renda.
    /// O fluxo principal continua após esta tarefa.
    /// </summary>
    PrepararNotificacaoConcluida,
}

/// <summary>
/// Terminal do ramo lateral paralelo do segmento 027.
/// </summary>
public enum PocEpatProcessSeg027LateralTerminal
{
    /// <summary>
    /// O timer 'Fim de Prazo Mantendo Atividade' (<c>_XWivFlqTEfG5K7mY0I3I6w</c>)
    /// disparou no instante absoluto <c>DTFIMCQ</c> + <c>HRFIMCQ</c>.
    /// A regra <c>RI-deadline-POC_EpatProcess-FimdePrazoMantendoAtividade</c>
    /// foi invocada no ramo lateral; a tarefa hospedeira não foi cancelada.
    /// O ramo lateral prossegue para 'Email CQ Fechamento'
    /// (<c>_O-rPp1qUEfG5K7mY0I3I6w</c>).
    /// </summary>
    FimDePrazoMantendoAtividadeDisparado,
}
