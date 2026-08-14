#nullable enable

// Card: BUILD-POCEPATPROCESS-seg053
// Segmento: SC-POC_EpatProcess-026 · passos 17–30 · etapa 53
// Processo: POC_EpatProcess
//
// Topologia (14 nós do caminho primário):
// <code>
//   1  linkThrow   _89MVQlqVEfG5K7mY0I3I6w  "Validar Paralelos"        (entrouPor=fluxo)
//   2  linkCatch   _Ei94AFqPEfG5K7mY0I3I6w  "Validação Paralelos"      (entrouPor=**link**)
//      │  NOEQ-link-goto: flatten-edge — aresta explícita, não signal/event
//   3  gateway     _CtQ7BFqPEfG5K7mY0I3I6w  "Vistas do Juiz?"          (VistasdoJuizRule)
//   4  gateway     _CtQ7BVqPEfG5K7mY0I3I6w  (passthrough)
//   5  receiveTask _CtQ68lqPEfG5K7mY0I3I6w  "Pedido de Vistas"         (bookmark-correlation)
//      │  ↑ timer de fronteira (não existe como transição no XPDL)
//   6  timerEvent  _CtQ7A1qPEfG5K7mY0I3I6w  (boundary of Pedido de Vistas)
//      │  Deadline: PRAZORETIRADAVI + HORAFINAL.TimeOfDay (absolute-instant)
//   7  signalThrow _CtQ66FqPEfG5K7mY0I3I6w  "FimDRF"                   (throws signal)
//   8  callActivity_CtQ691qPEfG5K7mY0I3I6w  "Busca Emails"             (calls BSCENVPC)
//   9  scriptTask  _CtQ7AVqPEfG5K7mY0I3I6w  "VoltaAgenda"              (VoltaAgendaStep)
//  10  gateway     _CtQ7AlqPEfG5K7mY0I3I6w  "Tipo de Vista Mista?"     (TipodeVistaMistaRule)
//  11  gateway     _CtQ7AFqPEfG5K7mY0I3I6w  (passthrough/join)
//  12  gateway     _CtQ69FqPEfG5K7mY0I3I6w  (passthrough/join)
//  13  gateway     _CtQ6-lqPEfG5K7mY0I3I6w  (passthrough/join)
//  14  scriptTask  _zE3XeV6JEfGBBLgT-R5iuw  "prepSub"                  (PrepSubStep)
// </code>
//
// Decisões:
//   - NOEQ-link-goto: flatten-edge — aresta explícita em .NET
//   - NOEQ-external-event: bookmark-correlation via ICorrelationStore
//   - NOEQ-expression-deadline: absolute-instant (PRAZORETIRADAVI + HORAFINAL.TimeOfDay)
//
// Cenário de referência (SC-026, "prazo"):
//   Timer boundary dispara → FimDRF → Busca Emails → VoltaAgenda →
//   Tipo de Vista Mista? (TIPOVISTAS != 'MISTA') → gateways 11→12→13 → prepSub

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution.PocEpatProcess;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.Rules.PocEpatProcess;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Validar Paralelos' (linkThrow)
/// até 'prepSub' (scriptTask) — passos 17 a 30 do cenário SC-POC_EpatProcess-026.
///
/// Topologia: 14 nós incluindo receiveTask com timer de fronteira.
///
/// <para>
/// <b>Decisões de lacunas:</b>
/// <list type="bullet">
///   <item>NOEQ-link-goto: flatten-edge — linkCatch é aresta explícita, não signal/event.</item>
///   <item>NOEQ-external-event: bookmark-correlation via ICorrelationStore.</item>
///   <item>NOEQ-expression-deadline: absolute-instant (PRAZORETIRADAVI + HORAFINAL.TimeOfDay).</item>
/// </list>
/// </para>
/// </summary>
public sealed class PocEpatProcessSeg053Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg053) ──

    /// <summary>Nó 1 — linkThrow 'Validar Paralelos'.</summary>
    public const string NodeValidarParalelos = "_89MVQlqVEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — linkCatch 'Validação Paralelos' (entrouPor=link, flatten-edge).</summary>
    public const string NodeValidacaoParalelos = "_Ei94AFqPEfG5K7mY0I3I6w";

    /// <summary>Nó 3 — gateway 'Vistas do Juiz?'.</summary>
    public const string NodeVistasdoJuiz = "_CtQ7BFqPEfG5K7mY0I3I6w";

    /// <summary>Nó 4 — gateway (passthrough, unnamed).</summary>
    public const string NodeGateway4 = "_CtQ7BVqPEfG5K7mY0I3I6w";

    /// <summary>Nó 5 — receiveTask 'Pedido de Vistas'.</summary>
    public const string NodePedidoDeVistas = "_CtQ68lqPEfG5K7mY0I3I6w";

    /// <summary>Nó 6 — timerEvent boundary de Pedido de Vistas.</summary>
    public const string NodePedidoDeVistasBoundary = "_CtQ7A1qPEfG5K7mY0I3I6w";

    /// <summary>Nó 7 — signalThrow 'FimDRF'.</summary>
    public const string NodeFimDRF = "_CtQ66FqPEfG5K7mY0I3I6w";

    /// <summary>Nó 8 — callActivity 'Busca Emails' (calls BSCENVPC).</summary>
    public const string NodeBuscaEmails = "_CtQ691qPEfG5K7mY0I3I6w";

    /// <summary>Nó 9 — scriptTask 'VoltaAgenda'.</summary>
    public const string NodeVoltaAgenda = "_CtQ7AVqPEfG5K7mY0I3I6w";

    /// <summary>Nó 10 — gateway 'Tipo de Vista Mista?'.</summary>
    public const string NodeTipodeVistaMista = "_CtQ7AlqPEfG5K7mY0I3I6w";

    /// <summary>Nó 11 — gateway (passthrough/join).</summary>
    public const string NodeGateway11 = "_CtQ7AFqPEfG5K7mY0I3I6w";

    /// <summary>Nó 12 — gateway (passthrough/join).</summary>
    public const string NodeGateway12 = "_CtQ69FqPEfG5K7mY0I3I6w";

    /// <summary>Nó 13 — gateway (passthrough/join).</summary>
    public const string NodeGateway13 = "_CtQ6-lqPEfG5K7mY0I3I6w";

    /// <summary>Nó 14 — scriptTask 'prepSub'.</summary>
    public const string NodePrepSub = "_zE3XeV6JEfGBBLgT-R5iuw";

    private readonly IClock _clock;
    private readonly Func<AiimCaseRef, CancellationToken, Task> _waitForPedidoDeVistas;
    private readonly Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> _buscaEmails;

    /// <param name="clock">Relógio injectado — nunca <c>DateTime.Now</c>.</param>
    /// <param name="waitForPedidoDeVistas">
    /// Delegate de evento externo: suspende até Pedido de Vistas ser retomado (bookmark-correlation).
    /// </param>
    /// <param name="buscaEmails">
    /// Delegate de callActivity: invoca o processo BSCENVPC (Busca Emails).
    /// </param>
    public PocEpatProcessSeg053Workflow(
        IClock clock,
        Func<AiimCaseRef, CancellationToken, Task> waitForPedidoDeVistas,
        Func<AiimCaseRef, CancellationToken, Task<ProcessCallResult>> buscaEmails)
    {
        _clock = clock;
        _waitForPedidoDeVistas = waitForPedidoDeVistas;
        _buscaEmails = buscaEmails;
    }

    /// <summary>
    /// Executa o troco: de Validar Paralelos até prepSub.
    ///
    /// <para>
    /// Cenário SC-026 ("prazo"): timer boundary dispara em Pedido de Vistas →
    /// FimDRF → Busca Emails → VoltaAgenda → Tipo de Vista Mista? (false) →
    /// gateways 11→12→13 → prepSub.
    /// </para>
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O terminal alcançado.</returns>
    public async Task<PocEpatProcessSeg053Terminal> ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        CancellationToken ct)
    {
        // ── ordem 1: linkThrow 'Validar Paralelos' (_89MVQlqVEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Comportamento: throws link, immediately caught by linkCatch — flatten-edge.

        // ── ordem 2: linkCatch 'Validação Paralelos' (_Ei94AFqPEfG5K7mY0I3I6w, entrouPor=link) ──
        // NOEQ-link-goto: flatten-edge — não há transição XPDL, aresta explícita em .NET.
        // O fluxo prossegue directamente do linkThrow para aqui.

        // ── ordem 3: gateway 'Vistas do Juiz?' (_CtQ7BFqPEfG5K7mY0I3I6w) ──
        // Regra: RI-transition-POC_EpatProcess-VistasdoJuiz
        // Se true: caminho juiz (default quando SW_NA — hipótese 3)
        // Se false: caminho alternativo (não juiz)
        if (!VistasdoJuizRule.Evaluate(aiimCase))
        {
            // Caminho alternativo (não juiz) — fluxo simplificado para terminal alternativo.
            // Este segmento foca no caminho prazo (SC-026).
            return PocEpatProcessSeg053Terminal.VistasdoJuizAlternative;
        }

        // ── ordem 4: gateway passthrough (_CtQ7BVqPEfG5K7mY0I3I6w) ──
        // Exclusive gateway — passthrough, sem lógica.

        // ── ordem 5: receiveTask 'Pedido de Vistas' (_CtQ68lqPEfG5K7mY0I3I6w) ──
        // NOEQ-external-event: bookmark-correlation via ICorrelationStore.
        // ── ordem 6: timerEvent boundary (_CtQ7A1qPEfG5K7mY0I3I6w, entrouPor=fronteira) ──
        // NOEQ-expression-deadline: absolute-instant (PRAZORETIRADAVI + HORAFINAL.TimeOfDay).
        // Timer corre em paralelo com a espera do bookmark.
        var deadline = PocEpatProcessSeg053DeadlineRules.ComputePedidoDeVistasDeadline(aiimCase, _clock);
        var delay = deadline - _clock.Now;

        using var timerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        using var eventCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var eventTask = _waitForPedidoDeVistas(caseRef, eventCts.Token);
        var timerTask = Task.Delay(delay < TimeSpan.Zero ? TimeSpan.Zero : delay, timerCts.Token);

        var completed = await Task.WhenAny(eventTask, timerTask).ConfigureAwait(false);

        bool timerFired;
        if (completed == timerTask && !timerTask.IsCanceled)
        {
            // Timer de fronteira disparou primeiro — cancela a espera do evento.
            await eventCts.CancelAsync().ConfigureAwait(false);
            timerFired = true;
        }
        else
        {
            // Evento externo chegou antes do prazo — cancela o timer.
            await timerCts.CancelAsync().ConfigureAwait(false);
            // Propaga excepção se a tarefa falhou por razão diferente de cancelamento.
            await eventTask.ConfigureAwait(false);
            timerFired = false;
        }

        if (!timerFired)
        {
            // Pedido de Vistas completado por evento externo (não pelo timer).
            return PocEpatProcessSeg053Terminal.PedidoDeVistasCompleted;
        }

        // ── ordem 7: signalThrow 'FimDRF' (_CtQ66FqPEfG5K7mY0I3I6w) ──
        // Throws signal FimDRF — no wait, prossegue imediatamente.
        // naoSabemos: o comportamento exacto do signal no Elsa 3 pode exigir adaptação.
        // O fluxo prossegue sem espera.

        // ── ordem 8: callActivity 'Busca Emails' (_CtQ691qPEfG5K7mY0I3I6w) ──
        // Chama o processo BSCENVPC.
        var buscaResult = await _buscaEmails(caseRef, ct).ConfigureAwait(false);
        if (!buscaResult.Started)
        {
            // Falha ao iniciar BSCENVPC — terminal de erro.
            return PocEpatProcessSeg053Terminal.BuscaEmailsFailed;
        }

        // ── ordem 9: scriptTask 'VoltaAgenda' (_CtQ7AVqPEfG5K7mY0I3I6w) ──
        // Regra: RI-script-POC_EpatProcess-VoltaAgenda (eRegraDeNegocio=false, technical).
        VoltaAgendaStep.Apply(aiimCase);

        // ── ordem 10: gateway 'Tipo de Vista Mista?' (_CtQ7AlqPEfG5K7mY0I3I6w) ──
        // Regra: RI-transition-POC_EpatProcess-TipodeVistaMista
        if (TipodeVistaMistaRule.Evaluate(aiimCase))
        {
            // Caminho MISTA — prossegue para lógica de vista mista.
            // Este ramo junta-se mais abaixo nos gateways 11→12→13.
            // Para o cenário SC-026 ("prazo"), TIPOVISTAS != 'MISTA'.
            return PocEpatProcessSeg053Terminal.TipodeVistaMistaPath;
        }

        // ── ordem 11: gateway passthrough/join (_CtQ7AFqPEfG5K7mY0I3I6w) ──
        // ── ordem 12: gateway passthrough/join (_CtQ69FqPEfG5K7mY0I3I6w) ──
        // ── ordem 13: gateway passthrough/join (_CtQ6-lqPEfG5K7mY0I3I6w) ──
        // Todos são exclusive gateways sem lógica — passthrough.

        // ── ordem 14: scriptTask 'prepSub' (_zE3XeV6JEfGBBLgT-R5iuw) ──
        // Regra: RI-script-POC_EpatProcess-prepSub (eRegraDeNegocio=true).
        // Envelope técnico em PrepSubStep, lógica de domínio em PrepSubRule.
        PrepSubStep.Apply(aiimCase);

        return PocEpatProcessSeg053Terminal.PrepSubCompleted;
    }
}

/// <summary>
/// Terminais alcançáveis no segmento 053 do POC_EpatProcess.
/// </summary>
public enum PocEpatProcessSeg053Terminal
{
    /// <summary>
    /// O gateway 'Vistas do Juiz?' (<c>_CtQ7BFqPEfG5K7mY0I3I6w</c>) avaliou <c>false</c>:
    /// TIPOVISTAS != "JUIZ" e TIPOVISTAS != SW_NA.
    /// O fluxo desvia para o caminho alternativo (não juiz).
    /// </summary>
    VistasdoJuizAlternative,

    /// <summary>
    /// O receiveTask 'Pedido de Vistas' (<c>_CtQ68lqPEfG5K7mY0I3I6w</c>) foi completado
    /// por evento externo (bookmark-correlation) antes do timer boundary expirar.
    /// </summary>
    PedidoDeVistasCompleted,

    /// <summary>
    /// O callActivity 'Busca Emails' (<c>_CtQ691qPEfG5K7mY0I3I6w</c>) falhou ao iniciar
    /// o processo BSCENVPC.
    /// </summary>
    BuscaEmailsFailed,

    /// <summary>
    /// O gateway 'Tipo de Vista Mista?' (<c>_CtQ7AlqPEfG5K7mY0I3I6w</c>) avaliou <c>true</c>:
    /// TIPOVISTAS == "MISTA". O fluxo segue pelo caminho de vista mista.
    /// </summary>
    TipodeVistaMistaPath,

    /// <summary>
    /// O scriptTask 'prepSub' (<c>_zE3XeV6JEfGBBLgT-R5iuw</c>) foi executado com sucesso.
    /// Cenário de referência SC-026 ("prazo"): timer boundary disparou, TIPOVISTAS != "MISTA".
    /// </summary>
    PrepSubCompleted,
}

/// <summary>
/// Regra de prazo do timer boundary 'Pedido de Vistas'
/// (<c>_CtQ7A1qPEfG5K7mY0I3I6w</c>, entrouPor=fronteira).
///
/// Implementa <c>NOEQ-expression-deadline = absolute-instant</c>:
/// combina o campo de data (<see cref="AiimCase.PRAZORETIRADAVI"/>)
/// e o campo de hora (TimeOfDay de <see cref="AiimCase.HORAFINAL"/>)
/// num instante absoluto no momento do agendamento.
///
/// Classificação: <c>fixa-prazo</c>; portador: <c>deadline</c>.
/// </summary>
public static class PocEpatProcessSeg053DeadlineRules
{
    // POR CONFIRMAR com a SEFAZ: fuso horário actual do iProcess.
    // Assumido America/Sao_Paulo conforme card BUILD-POCEPATPROCESS-seg053.
    private static readonly TimeZoneInfo SaoPauloTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// Calcula o instante absoluto em que o timer boundary 'Pedido de Vistas'
    /// (<c>_CtQ7A1qPEfG5K7mY0I3I6w</c>) deve disparar.
    ///
    /// <para>
    /// Expressão legado: PRAZORETIRADAVI (DateOnly) + HORAFINAL.TimeOfDay (TimeSpan).
    /// O par data+hora é fixado no momento do agendamento (<c>absolute-instant</c>).
    /// </para>
    ///
    /// <para>
    /// <b>RISCO RESIDUAL:</b> o timer não acompanha prorrogação posterior dos campos.
    /// </para>
    ///
    /// <para>
    /// <b>POR CONFIRMAR:</b> fuso horário — assumido America/Sao_Paulo.
    /// </para>
    /// </summary>
    /// <param name="caseData">Estado do caso no momento do agendamento.</param>
    /// <param name="clock">
    /// Relógio injectado — <b>nunca</b> <c>DateTime.Now</c> nem
    /// <c>DateTimeOffset.Now</c>: o teste de prazo exige relógio controlável.
    /// </param>
    public static DateTimeOffset ComputePedidoDeVistasDeadline(AiimCase caseData, IClock clock)
    {
        _ = clock; // clock used for reference; deadline is absolute from case fields

        var date = caseData.PRAZORETIRADAVI;          // XPDL: PRAZORETIRADAVI (DateOnly)
        var time = caseData.HORAFINAL.TimeOfDay;      // XPDL: HORAFINAL.TimeOfDay (TimeSpan)

        // Combina data e hora num DateTime local sem deslocamento.
        var localDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(time));

        // Converte para instante absoluto no fuso de São Paulo (POR CONFIRMAR).
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, SaoPauloTimeZone);
    }
}
