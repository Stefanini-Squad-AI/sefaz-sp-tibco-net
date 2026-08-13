#nullable enable

// Card: BUILD-POCEPATPROCESS-seg026
// Segmento: SC-POC_EpatProcess-016 · passos 3–4 · etapa 2
// Processo: POC_EpatProcess · ordemNaJornada: 1

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Preparar Notificacao' (userTask)
/// até 'Encerra e retira' (timerEvent/fronteira) —
/// passos 3 a 4 do cenário SC-POC_EpatProcess-016, segmento ordemNaJornada=1.
///
/// Topologia (2 nós):
/// <code>
///   1  userTask    _sfwu-VqUEfG5K7mY0I3I6w  Preparar Notificacao  (entrouPor=fluxo)
///      │  ↑ timer de fronteira (não existe como transição no XPDL — escrito explicitamente)
///   2  timerEvent  _T4Ma8FqiEfG5K7mY0I3I6w  Encerra e retira      (entrouPor=fronteira)
///      │  regra: RI-deadline-POC_EpatProcess-Encerraeretira
///      │    expressão XPDL: Days=2
///      │    prazo absoluto = instante de entrada na tarefa + 2 dias
///      ↓ → End Event (_UiYAYFqiEfG5K7mY0I3I6w)
/// </code>
///
/// Atenção: 'Encerra e retira' (<c>_T4Ma8FqiEfG5K7mY0I3I6w</c>) alcançado por ligação
/// de <b>fronteira</b> — não existe como transição no XPDL e é escrito explicitamente
/// no fluxo .NET como timer de fronteira sobre a tarefa humana 'Preparar Notificacao'.
/// </summary>
public sealed class PocEpatProcessSeg026Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg026) ──

    /// <summary>Nó 1 — userTask 'Preparar Notificacao'.</summary>
    public const string NodePrepararNotificacao = "_sfwu-VqUEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — timerEvent de fronteira 'Encerra e retira'.</summary>
    public const string NodeEncerraeretira = "_T4Ma8FqiEfG5K7mY0I3I6w";

    private readonly PrepararNotificacaoUseCase _prepararNotificacao;
    private readonly IClock _clock;

    /// <param name="prepararNotificacao">Caso de uso para a userTask 'Preparar Notificacao'.</param>
    /// <param name="clock">Relógio injectado — nunca <c>DateTime.Now</c>.</param>
    public PocEpatProcessSeg026Workflow(
        PrepararNotificacaoUseCase prepararNotificacao,
        IClock clock)
    {
        _prepararNotificacao = prepararNotificacao;
        _clock = clock;
    }

    /// <summary>
    /// Executa o troco: aguarda a submissão de 'Preparar Notificacao' correndo em
    /// paralelo com o timer de fronteira 'Encerra e retira' (RI-deadline-POC_EpatProcess-Encerraeretira).
    /// O timer dispara ao fim de 2 dias a partir do instante de entrada na tarefa,
    /// cancelando a espera humana e desviando o fluxo para o End Event.
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="waitForSubmit">
    /// Delegate de interacção humana: suspende até o formulário ser submetido.
    /// Cancelado pelo timer de fronteira quando o prazo expira.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// O terminal alcançado: sempre
    /// <see cref="PocEpatProcessSeg026Terminal.EncerraeretiraBoundaryTimer"/>
    /// quando o prazo expira antes da submissão humana, ou
    /// <see cref="PocEpatProcessSeg026Terminal.PrepararNotificacaoCompleted"/>
    /// quando o formulário é submetido antes do prazo.
    /// </returns>
    public async Task<PocEpatProcessSeg026Terminal> ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task> waitForSubmit,
        CancellationToken ct)
    {
        // ── ordem 1: userTask 'Preparar Notificacao' (_sfwu-VqUEfG5K7mY0I3I6w, entrouPor=fluxo) ──

        // ── ordem 2: timerEvent de fronteira 'Encerra e retira' (_T4Ma8FqiEfG5K7mY0I3I6w, entrouPor=fronteira) ──
        // Ligação de fronteira — NÃO existe como transição no XPDL.
        // Escrita explicitamente no fluxo .NET (conforme content.checklist ordem 2).
        // Regra: RI-deadline-POC_EpatProcess-Encerraeretira (expressão XPDL: Days=2)
        // O timer corre em paralelo com a tarefa humana usando IClock injectado.
        var deadline = PocEpatProcessSeg026DeadlineRules.ComputeEncerraeretiraDeadline(_clock);
        var delay = deadline - _clock.Now;

        using var timerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        using var humanCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var humanTask = _prepararNotificacao.ExecuteAsync(caseRef, aiimCase, waitForSubmit, humanCts.Token);
        var timerTask = Task.Delay(delay < TimeSpan.Zero ? TimeSpan.Zero : delay, timerCts.Token);

        var completed = await Task.WhenAny(humanTask, timerTask).ConfigureAwait(false);

        if (completed == timerTask && !timerTask.IsCanceled)
        {
            // Timer de fronteira disparou primeiro — cancela a espera humana.
            await humanCts.CancelAsync().ConfigureAwait(false);
            return PocEpatProcessSeg026Terminal.EncerraeretiraBoundaryTimer;
        }

        // Formulário submetido antes do prazo — cancela o timer.
        await timerCts.CancelAsync().ConfigureAwait(false);
        // Propaga excepção se a tarefa humana falhou por razão diferente de cancelamento.
        await humanTask.ConfigureAwait(false);
        return PocEpatProcessSeg026Terminal.PrepararNotificacaoCompleted;
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 026 do POC_EpatProcess.
/// </summary>
public enum PocEpatProcessSeg026Terminal
{
    /// <summary>
    /// O timer de fronteira 'Encerra e retira' (<c>_T4Ma8FqiEfG5K7mY0I3I6w</c>) disparou:
    /// o prazo de 2 dias (RI-deadline-POC_EpatProcess-Encerraeretira) expirou antes
    /// da submissão humana. O fluxo desvia para o End Event.
    /// </summary>
    EncerraeretiraBoundaryTimer,

    /// <summary>
    /// O formulário 'Preparar Notificacao' foi submetido antes do prazo expirar.
    /// O fluxo prossegue pelo ramo normal (sem accionamento do timer de fronteira).
    /// </summary>
    PrepararNotificacaoCompleted,
}

/// <summary>
/// Regra de prazo do timer de fronteira 'Encerra e retira'
/// (<c>_T4Ma8FqiEfG5K7mY0I3I6w</c>, regra RI-deadline-POC_EpatProcess-Encerraeretira).
/// Expressão XPDL (linha 2691): <c>Days=2</c>.
/// O instante de disparo é calculado a partir do relógio injectado — nunca <c>DateTime.Now</c>.
/// </summary>
public static class PocEpatProcessSeg026DeadlineRules
{
    /// <summary>
    /// RI-deadline-POC_EpatProcess-Encerraeretira.
    /// Devolve o instante absoluto em que o timer 'Encerra e retira' deve disparar.
    /// Prazo: 2 dias a partir do instante actual (no momento do agendamento).
    /// </summary>
    /// <param name="clock">Relógio injectado — nunca <c>DateTime.Now</c>.</param>
    public static DateTimeOffset ComputeEncerraeretiraDeadline(IClock clock)
    {
        // RI-deadline-POC_EpatProcess-Encerraeretira: expressão XPDL "Days=2"
        // Prazo absoluto = now + 2 dias.
        return clock.Now.Add(TimeSpan.FromDays(2));
    }
}
