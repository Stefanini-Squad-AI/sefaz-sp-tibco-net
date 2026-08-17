#nullable enable

// Card: BUILD-POCEPATPROCESS-seg051
// Segmento: SC-POC_EpatProcess-010 · passos 22–29 · etapa 5
// Processo: POC_EpatProcess · ordemNaJornada: 8
//
// Gap link-goto (decided, NOEQ-link-goto, 2026-08-06):
//   flatten-edge: o par linkThrow/linkCatch é implementado como aresta explícita.
//   Manter como sinal introduziria pontos de espera inexistentes no TIBCO.
//
// Gap external-event (decided, NOEQ-external-event, 2026-08-06):
//   bookmark-correlation: ICorrelationStore suspende e retoma sem infraestrutura extra.
//   POR DEFINIR (etapa 5): protecção do endpoint e idempotência.
//
// Gap expression-deadline (decided, NOEQ-expression-deadline, 2026-08-06):
//   absolute-instant: combina PRAZORETIRADAVI + HORAFINAL.Time num DateTimeOffset fixo.
//   IClock injectado — nunca DateTime.Now.

using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Validar Paralelos' (linkThrow)
/// até 'Catch Signal Event' (signalCatch) —
/// passos 22 a 29 do cenário SC-POC_EpatProcess-010, segmento ordemNaJornada=8.
///
/// Topologia (8 nós):
/// <code>
///   1  linkThrow    _89MVQlqVEfG5K7mY0I3I6w  Validar Paralelos          (entrouPor=fluxo)
///      │  [flatten-edge: GOTO — aresta explícita, não sinal; NOEQ-link-goto]
///      ↓ aresta explícita .NET
///   2  linkCatch    _Ei94AFqPEfG5K7mY0I3I6w  Validação Paralelos        (entrouPor=link)
///      │  [este nó NÃO existe como transição no XPDL — escrito explicitamente; AC2]
///      ↓ aresta fluxo
///   3  gateway      _CtQ7BFqPEfG5K7mY0I3I6w  Vistas do Juiz ?           (entrouPor=fluxo)
///      │  regra: RI-transition-POC_EpatProcess-VistasdoJuiz
///      │    Ramo "VistasdoJuiz": TIPOVISTAS == 'JUIZ' OU TIPOVISTAS == SW_NA
///      │      → terminal VistasdoJuiz (fora deste segmento)
///      └─ Ramo OTHERWISE → aresta → gateway _CtQ7BVqPEfG5K7mY0I3I6w
///   4  gateway      _CtQ7BVqPEfG5K7mY0I3I6w  gateway _CtQ7BVqPEfG5K7mY0I3I6w (entrouPor=fluxo)
///      │  topologia XPDL: aresta directa → Pedido de Vistas
///      ↓ aresta fluxo
///   5  receiveTask  _CtQ68lqPEfG5K7mY0I3I6w  Pedido de Vistas           (entrouPor=fluxo)
///      │  [suspende; bookmark-correlation via ICorrelationStore; NOEQ-external-event]
///      │
///      ╠═══ (a) evento externo chega → terminal PedidoDeVistasConcluido ═══════════╗
///      │                                                                            │
///      │  (b) timerEvent de fronteira interruptivo dispara                         │
///      │       timer: _CtQ7A1qPEfG5K7mY0I3I6w · entrouPor=fronteira              │
///      │       regra: RI-deadline-POC_EpatProcess-passosemrotulo                  │
///      │       deadline: PRAZORETIRADAVI (DateOnly) + HORAFINAL.Time → absoluto   │
///      │       [NÃO existe transição XPDL — escrito explicitamente; AC6]          │
///      │       ↓ cancela receiveTask, continua:                                   │
///   6     timerEvent  _CtQ7A1qPEfG5K7mY0I3I6w  (fronteira interruptiva)          │
///           ↓                                                                      │
///   7     signalThrow _CtQ66FqPEfG5K7mY0I3I6w  FimDRF                 (entrouPor=fluxo)
///           │  identificador do sinal: "FimDRF" — não renomeado (AC7)             │
///           ↓                                                                      │
///   8     signalCatch _WvTQIFqQEfG5K7mY0I3I6w  Catch Signal Event     (entrouPor=sinal)
///               [NÃO existe como transição no XPDL — escrito explicitamente; AC8] │
///               → terminal CatchSignalEvent                                        │
///                                                                                  │
///                                  ←──────────────────────────────────────────────╝
/// </code>
///
/// <b>Decisões do glossário aplicadas:</b>
/// <list type="bullet">
///   <item>
///     <c>NOEQ-link-goto</c> (opção <c>flatten-edge</c>, ratificada):
///     o par linkThrow/linkCatch (<c>_89MVQlqVEfG5K7mY0I3I6w</c> /
///     <c>_Ei94AFqPEfG5K7mY0I3I6w</c>) é aplanado para aresta directa —
///     sem evento intermediário, sem ponto de espera.
///   </item>
///   <item>
///     <c>NOEQ-external-event</c> (opção <c>bookmark-correlation</c>, ratificada):
///     o receiveTask <em>Pedido de Vistas</em> (<c>_CtQ68lqPEfG5K7mY0I3I6w</c>)
///     suspende via <c>ICorrelationStore</c> com chave <c>PROCESS_ID</c>.
///   </item>
///   <item>
///     <c>NOEQ-expression-deadline</c> (opção <c>absolute-instant</c>, ratificada):
///     o timer combina <c>PRAZORETIRADAVI</c> + <c>HORAFINAL.Time</c> num
///     <c>DateTimeOffset</c> absoluto. <c>IClock</c> injectado — nunca
///     <c>DateTime.Now</c>.
///   </item>
/// </list>
///
/// <b>Nós sem transição XPDL — escritos explicitamente no fluxo .NET:</b>
/// <list type="bullet">
///   <item>
///     Ordem 2 (<c>_Ei94AFqPEfG5K7mY0I3I6w</c>, entrouPor=link): linkCatch aplanado.
///   </item>
///   <item>
///     Ordem 6 (<c>_CtQ7A1qPEfG5K7mY0I3I6w</c>, entrouPor=fronteira): timerEvent
///     registado como boundary event anexo ao receiveTask; prazo via
///     <see cref="PocEpatProcessSeg051DeadlineRules.ComputePedidoDeVistasDeadline"/>.
///   </item>
///   <item>
///     Ordem 8 (<c>_WvTQIFqQEfG5K7mY0I3I6w</c>, entrouPor=sinal): signalCatch
///     registado explicitamente; captura o sinal "<c>FimDRF</c>".
///   </item>
/// </list>
/// </summary>
public sealed class PocEpatProcessSeg051Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg051) ──

    /// <summary>Nó 1 — linkThrow 'Validar Paralelos'.</summary>
    public const string NodeValidarParalelosThrow = "_89MVQlqVEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — linkCatch 'Validação Paralelos' (aresta explícita — não existe no XPDL).</summary>
    public const string NodeValidacaoParalelosCatch = "_Ei94AFqPEfG5K7mY0I3I6w";

    /// <summary>Nó 3 — gateway 'Vistas do Juiz ?' (Exclusive / XOR).</summary>
    public const string NodeVistasdoJuizGateway = "_CtQ7BFqPEfG5K7mY0I3I6w";

    /// <summary>Nó 4 — gateway <c>_CtQ7BVqPEfG5K7mY0I3I6w</c> (passagem topológica).</summary>
    public const string NodeGatewayCtQ7BVqP = "_CtQ7BVqPEfG5K7mY0I3I6w";

    /// <summary>Nó 5 — receiveTask 'Pedido de Vistas' (bookmark-correlation).</summary>
    public const string NodePedidoDeVistas = "_CtQ68lqPEfG5K7mY0I3I6w";

    /// <summary>
    /// Nó 6 — timerEvent de fronteira interruptivo (NÃO existe como transição no XPDL).
    /// Regra: RI-deadline-POC_EpatProcess-passosemrotulo.
    /// </summary>
    public const string NodeTimerPedidoDeVistas = "_CtQ7A1qPEfG5K7mY0I3I6w";

    /// <summary>Nó 7 — signalThrow 'FimDRF'. Identificador do sinal preservado.</summary>
    public const string NodeFimDrfSignalThrow = "_CtQ66FqPEfG5K7mY0I3I6w";

    /// <summary>
    /// Identificador do sinal FimDRF — invariante: não renomear (card BUILD-POCEPATPROCESS-seg051).
    /// </summary>
    public const string SignalFimDrf = "FimDRF";

    /// <summary>
    /// Nó 8 — signalCatch 'Catch Signal Event' (NÃO existe como transição no XPDL).
    /// Captura o sinal <see cref="SignalFimDrf"/>.
    /// </summary>
    public const string NodeCatchSignalEvent = "_WvTQIFqQEfG5K7mY0I3I6w";

    private readonly IClock _clock;

    /// <param name="clock">
    /// Relógio injectado — nunca <c>DateTime.Now</c> nem <c>DateTimeOffset.Now</c>.
    /// O teste de prazo exige relógio controlável.
    /// </param>
    public PocEpatProcessSeg051Workflow(IClock clock)
    {
        _clock = clock;
    }

    /// <summary>
    /// Executa o troco: atravessa os gateways 'Validação Paralelos' e 'Vistas do Juiz ?',
    /// e quando o ramo OTHERWISE é seguido, aguarda o evento externo 'Pedido de Vistas'
    /// em corrida com o timerEvent de fronteira; publica o sinal 'FimDRF' e retoma
    /// no 'Catch Signal Event'.
    /// </summary>
    /// <param name="aiimCase">
    /// Estado de negócio mutável do caso.
    /// <see cref="AiimCase.TIPOVISTAS"/> determina o ramo do gateway 'Vistas do Juiz ?'.
    /// <see cref="AiimCase.PRAZORETIRADAVI"/> e <see cref="AiimCase.HORAFINAL"/>
    /// determinam o instante absoluto do timerEvent de fronteira.
    /// </param>
    /// <param name="waitForPedidoDeVistas">
    /// Delegate de evento externo: suspende o workflow até a resposta de vistas chegar
    /// via endpoint de retomada (bookmark-correlation, NOEQ-external-event).
    /// Recebe a chave de correlação <c>PROCESS_ID</c> e o token de cancelamento.
    /// O token de cancelamento será activado quando o timerEvent de fronteira disparar.
    /// </param>
    /// <param name="waitForDeadline">
    /// Delegate de tempo: suspende até o instante absoluto informado ser atingido.
    /// Injectado para manter o código testável sem recorrer a <c>Task.Delay</c> real.
    /// </param>
    /// <param name="publishFimDrfSignal">
    /// Delegate de publicação: emite o sinal '<c>FimDRF</c>' no barramento de sinais
    /// do processo (<c>_CtQ66FqPEfG5K7mY0I3I6w</c>). Sinal preservado sem renomeação.
    /// </param>
    /// <param name="waitForFimDrfSignal">
    /// Delegate de captura: aguarda o sinal '<c>FimDRF</c>' no nó
    /// '<c>_WvTQIFqQEfG5K7mY0I3I6w</c>' (Catch Signal Event). Escrito explicitamente
    /// porque não existe como transição no XPDL.
    /// </param>
    /// <param name="processId">
    /// Chave de correlação — formato <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>,
    /// montada pelos scripts antes de cada chamada. Identifica a instância a retomar.
    /// </param>
    /// <param name="ct">Token de cancelamento (do caller).</param>
    /// <returns>
    /// Terminal alcançado:
    /// <list type="bullet">
    ///   <item><see cref="PocEpatProcessSeg051Terminal.VistasdoJuiz"/> quando
    ///     <c>TIPOVISTAS == 'JUIZ'</c> ou <c>TIPOVISTAS == SW_NA</c>
    ///     (ramo VistasdoJuiz do gateway, aresta RI-transition-POC_EpatProcess-VistasdoJuiz).</item>
    ///   <item><see cref="PocEpatProcessSeg051Terminal.PedidoDeVistasConcluido"/> quando
    ///     o evento externo chega antes do timer.</item>
    ///   <item><see cref="PocEpatProcessSeg051Terminal.CatchSignalEvent"/> quando
    ///     o timerEvent de fronteira dispara, FimDRF é lançado e apanhado
    ///     (<c>_WvTQIFqQEfG5K7mY0I3I6w</c>).</item>
    /// </list>
    /// </returns>
    public async Task<PocEpatProcessSeg051Terminal> ExecuteAsync(
        AiimCase aiimCase,
        Func<string, CancellationToken, Task> waitForPedidoDeVistas,
        Func<DateTimeOffset, CancellationToken, Task> waitForDeadline,
        Func<string, CancellationToken, Task> publishFimDrfSignal,
        Func<string, CancellationToken, Task> waitForFimDrfSignal,
        string processId,
        CancellationToken ct)
    {
        // ── ordem 1: linkThrow 'Validar Paralelos' (_89MVQlqVEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // NOEQ-link-goto (flatten-edge): aplanado para aresta directa — sem evento, sem espera.

        // ── ordem 2: linkCatch 'Validação Paralelos' (_Ei94AFqPEfG5K7mY0I3I6w, entrouPor=link) ──
        // AC2: escrito explicitamente — NÃO existe transição XPDL de origem para este nó.
        // NOEQ-link-goto (flatten-edge): aplanado, continuação imediata.

        // ── ordem 3: gateway 'Vistas do Juiz ?' (_CtQ7BFqPEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Regra: RI-transition-POC_EpatProcess-VistasdoJuiz
        // Expressão XPDL: TIPOVISTAS=='JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA
        // TIPOVISTAS é FieldValue<string>: SW_NA → IsNotAvailable; 'JUIZ' → HasValue("JUIZ")
        var vistasdoJuiz = aiimCase.TIPOVISTAS.Match(
            hasValue:      v => v == "JUIZ",
            notAvailable: () => true,   // SW_NA: iProcess comparava como verdadeiro
            empty:        () => false);

        if (vistasdoJuiz)
        {
            // Ramo VistasdoJuiz: TIPOVISTAS == 'JUIZ' ou SW_NA
            // → terminal VistasdoJuiz (fluxo continua fora deste segmento)
            return PocEpatProcessSeg051Terminal.VistasdoJuiz;
        }

        // Ramo OTHERWISE (TIPOVISTAS != 'JUIZ' e não é SW_NA)
        // ── ordem 4: gateway _CtQ7BVqPEfG5K7mY0I3I6w (_CtQ7BVqPEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Passagem topológica — aresta directa para Pedido de Vistas.
        // AC4: saídas mapeadas como arestas explícitas conforme topologia XPDL.

        // ── ordem 5: receiveTask 'Pedido de Vistas' (_CtQ68lqPEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // NOEQ-external-event (bookmark-correlation): suspende até retomada por PROCESS_ID.
        // Arma o timer de fronteira com instante absoluto (absolute-instant).
        var deadline = PocEpatProcessSeg051DeadlineRules.ComputePedidoDeVistasDeadline(
            aiimCase, _clock);

        // Corrida entre evento externo e timerEvent de fronteira (interruptivo).
        // Usa CancellationTokenSource para cancelar o receiveTask quando o timer dispara,
        // e vice-versa — quem chegar primeiro cancela o outro.
        using var timerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        using var externalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var externalEventTask = waitForPedidoDeVistas(processId, timerCts.Token);

        // ── ordem 6: timerEvent _CtQ7A1qPEfG5K7mY0I3I6w (entrouPor=fronteira) ──
        // AC6: escrito explicitamente — NÃO existe transição XPDL para este nó.
        // Prazo: RI-deadline-POC_EpatProcess-passosemrotulo (PRAZORETIRADAVI + HORAFINAL.Time).
        // IClock injectado — NUNCA DateTime.Now.
        var deadlineTask = waitForDeadline(deadline, externalCts.Token);

        var first = await Task.WhenAny(externalEventTask, deadlineTask).ConfigureAwait(false);

        if (first == deadlineTask)
        {
            // Timer disparou primeiro — cancela o receiveTask
            await timerCts.CancelAsync().ConfigureAwait(false);

            // Propaga excepção do timer se houver (ex.: ct cancelado externamente)
            await deadlineTask.ConfigureAwait(false);

            // ── ordem 7: signalThrow 'FimDRF' (_CtQ66FqPEfG5K7mY0I3I6w, entrouPor=fluxo) ──
            // AC7: identificador do sinal "FimDRF" preservado sem renomeação.
            await publishFimDrfSignal(SignalFimDrf, ct).ConfigureAwait(false);

            // ── ordem 8: signalCatch 'Catch Signal Event' (_WvTQIFqQEfG5K7mY0I3I6w, entrouPor=sinal) ──
            // AC8: escrito explicitamente — NÃO existe como transição no XPDL.
            // A instância retoma aqui ao receber o sinal "FimDRF".
            await waitForFimDrfSignal(SignalFimDrf, ct).ConfigureAwait(false);

            return PocEpatProcessSeg051Terminal.CatchSignalEvent;
        }
        else
        {
            // Evento externo chegou primeiro — cancela o timer
            await externalCts.CancelAsync().ConfigureAwait(false);

            // Propaga excepção do external event se houver
            await externalEventTask.ConfigureAwait(false);

            // Pedido de Vistas concluído pelo evento externo (sem timer)
            return PocEpatProcessSeg051Terminal.PedidoDeVistasConcluido;
        }
    }
}

/// <summary>
/// Terminal de saída do segmento 051 do POC_EpatProcess.
/// </summary>
public enum PocEpatProcessSeg051Terminal
{
    /// <summary>
    /// Gateway 'Vistas do Juiz ?' (<c>_CtQ7BFqPEfG5K7mY0I3I6w</c>) avaliou
    /// <c>RI-transition-POC_EpatProcess-VistasdoJuiz</c> como verdadeiro:
    /// <c>TIPOVISTAS == 'JUIZ'</c> ou <c>TIPOVISTAS == SW_NA</c>.
    /// O fluxo segue para o ramo 'Vistas do Juiz' (fora deste segmento).
    /// </summary>
    VistasdoJuiz,

    /// <summary>
    /// O receiveTask 'Pedido de Vistas' (<c>_CtQ68lqPEfG5K7mY0I3I6w</c>) recebeu o
    /// evento externo antes do timer disparar. O fluxo segue pelo caminho normal
    /// (fora deste segmento).
    /// </summary>
    PedidoDeVistasConcluido,

    /// <summary>
    /// O timerEvent de fronteira (<c>_CtQ7A1qPEfG5K7mY0I3I6w</c>) disparou no instante
    /// absoluto <c>PRAZORETIRADAVI</c>+<c>HORAFINAL.Time</c>, cancelou o receiveTask,
    /// o sinal '<c>FimDRF</c>' foi lançado (<c>_CtQ66FqPEfG5K7mY0I3I6w</c>)
    /// e apanhado pelo 'Catch Signal Event' (<c>_WvTQIFqQEfG5K7mY0I3I6w</c>).
    /// </summary>
    CatchSignalEvent,
}
