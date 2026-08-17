#nullable enable

// Card: BUILD-POCEPATPROCESS-seg052
// Segmento: SC-POC_EpatProcess-012 · passos 22–28 · etapa 5
// Processo: POC_EpatProcess · ordemNaJornada: 8

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Cases;

namespace SefazSp.Epat.Application.Workflows.PocEpatProcess;

/// <summary>
/// Troco do workflow POC_EpatProcess: de 'Validar Paralelos' (linkThrow)
/// até 'Fim Vista Mista' (signalCatch) —
/// passos 22 a 28 do cenário SC-POC_EpatProcess-012, segmento ordemNaJornada=8.
///
/// Topologia (8 nós):
/// <code>
///   1  linkThrow  _89MVQlqVEfG5K7mY0I3I6w  Validar Paralelos          (entrouPor=fluxo)
///      │  GOTO implícito. Decidido flatten-edge (NOEQ-link-goto): vira aresta explícita.
///      ↓ aresta directa (flatten-edge — sem evento de sinal)
///   2  linkCatch  _Ei94AFqPEfG5K7mY0I3I6w  Validação Paralelos        (entrouPor=link)
///      │  Escrito explicitamente — não existe transição XPDL.
///      ↓ aresta directa
///   3  gateway    _CtQ7BFqPEfG5K7mY0I3I6w  Vistas do Juiz ?           (entrouPor=fluxo)
///      │  Regra: RI-transition-POC_EpatProcess-VistasdoJuiz
///      │  Expressão XPDL (linha 1755): TIPOVISTAS=='JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA;
///      │  Ramo "Juiz" (CONDITION) → leva a ramo JUIZ (fora deste segmento)
///      └─ Ramo "DRF"  (OTHERWISE) → _CtQ7BVqPEfG5K7mY0I3I6w
///   4  gateway    _CtQ7BVqPEfG5K7mY0I3I6w  gateway _CtQ7BVqPEfG5K7mY0I3I6w (entrouPor=fluxo)
///      │  Traversal sem lógica adicional — segue para _tbOD4FqPEfG5K7mY0I3I6w.
///      ↓ aresta directa
///   5  userTask   _tbOD4FqPEfG5K7mY0I3I6w  Realizar Atividade Vista Mista (entrouPor=fluxo)
///      ↓ aresta directa
///   6  signalThrow _InbWgFqQEfG5K7mY0I3I6w Fim Vista Mista             (entrouPor=fluxo)
///      │  GOTO implícito (signalThrow/signalCatch). Escrito como aresta explícita de
///      │  continuação — sem evento de sinal intermediário (manteria ponto de espera
///      │  que o TIBCO não tem).
///      ↓ aresta explícita (sem evento de sinal — flatten-edge equivalente)
///   7  signalCatch _CtQ67FqPEfG5K7mY0I3I6w Fim Vista Mista             (entrouPor=sinal)
///      │  Escrito explicitamente — não existe transição XPDL.
///      ↓ aresta directa
///   8  receiveTask _CtQ68lqPEfG5K7mY0I3I6w Pedido de Vistas            (bookmark)
///      │  Evento externo — suspende até correlação PROCESS_ID (bookmark-correlation).
///      │  Visitado noutra passagem (SC-POC_EpatProcess-007); não aparece no
///      │  percurso de referência dos passos 22–28.
///      │  Exposto via endpoint de retomada em Api/Endpoints.
/// </code>
///
/// Arestas explícitas (não existem como transição XPDL):
///   • Ordem 2 (_Ei94AFqPEfG5K7mY0I3I6w, entrouPor=link): par linkThrow/linkCatch
///     achatado em aresta directa (decisão flatten-edge, NOEQ-link-goto).
///   • Ordem 7 (_CtQ67FqPEfG5K7mY0I3I6w, entrouPor=sinal): par signalThrow/signalCatch
///     escrito como aresta de continuação explícita; manter como evento de sinal
///     introduziria ponto de espera que o TIBCO não tem.
/// </summary>
public sealed class PocEpatProcessSeg052Workflow
{
    // ── identificadores de nó — invariantes: não renomear (card BUILD-POCEPATPROCESS-seg052) ──

    /// <summary>Nó 1 — linkThrow 'Validar Paralelos'.</summary>
    public const string NodeValidarParalelos = "_89MVQlqVEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — linkCatch 'Validação Paralelos' (flatten-edge, NOEQ-link-goto).</summary>
    public const string NodeValidacaoParalelos = "_Ei94AFqPEfG5K7mY0I3I6w";

    /// <summary>Nó 3 — gateway 'Vistas do Juiz ?' (Exclusive / XOR).</summary>
    public const string NodeVistasdoJuiz = "_CtQ7BFqPEfG5K7mY0I3I6w";

    /// <summary>Nó 4 — gateway <c>_CtQ7BVqPEfG5K7mY0I3I6w</c> (traversal sem lógica).</summary>
    public const string NodeGatewayDRF = "_CtQ7BVqPEfG5K7mY0I3I6w";

    /// <summary>Nó 5 — userTask 'Realizar Atividade Vista Mista'.</summary>
    public const string NodeRealizarAtividadeVistaMista = "_tbOD4FqPEfG5K7mY0I3I6w";

    /// <summary>Nó 6 — signalThrow 'Fim Vista Mista'.</summary>
    public const string NodeFimVistaMistaThrow = "_InbWgFqQEfG5K7mY0I3I6w";

    /// <summary>Nó 7 — signalCatch 'Fim Vista Mista' (aresta explícita, sem evento de sinal).</summary>
    public const string NodeFimVistaMistaCatch = "_CtQ67FqPEfG5K7mY0I3I6w";

    /// <summary>Nó 8 — receiveTask 'Pedido de Vistas' (bookmark-correlation, NOEQ-external-event).</summary>
    public const string NodePedidoDeVistas = "_CtQ68lqPEfG5K7mY0I3I6w";

    private readonly RealizarAtividadeVistaMistaUseCase _realizarAtividadeVistaMista;
    private readonly ICorrelationStore _correlationStore;

    /// <param name="realizarAtividadeVistaMista">Caso de uso para a userTask 'Realizar Atividade Vista Mista'.</param>
    /// <param name="correlationStore">Porta de correlação por bookmark para o evento externo 'Pedido de Vistas'.</param>
    public PocEpatProcessSeg052Workflow(
        RealizarAtividadeVistaMistaUseCase realizarAtividadeVistaMista,
        ICorrelationStore correlationStore)
    {
        _realizarAtividadeVistaMista = realizarAtividadeVistaMista;
        _correlationStore = correlationStore;
    }

    /// <summary>
    /// Executa o troco dos passos 22 a 28: valida paralelos, avalia 'Vistas do Juiz ?',
    /// traversa o gateway DRF, aguarda a submissão de 'Realizar Atividade Vista Mista',
    /// emite 'Fim Vista Mista' e continua explicitamente para o signalCatch.
    /// </summary>
    /// <param name="caseRef">Referência do caso.</param>
    /// <param name="aiimCase">Estado de negócio do caso — fornece <c>TIPOVISTAS</c>.</param>
    /// <param name="waitForVistaMista">
    /// Delegate de interacção humana: suspende o workflow até o responsável submeter
    /// o formulário 'Realizar Atividade Vista Mista'.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O terminal alcançado após este troco.</returns>
    public async Task<PocEpatProcessSeg052Terminal> ExecuteAsync(
        AiimCaseRef caseRef,
        AiimCase aiimCase,
        Func<AiimCaseRef, CancellationToken, Task<RealizarAtividadeVistaMistaFormData>> waitForVistaMista,
        CancellationToken ct)
    {
        // ── ordem 1: linkThrow 'Validar Paralelos' (_89MVQlqVEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // GOTO implícito. Decidido flatten-edge (NOEQ-link-goto): sem evento de sinal.

        // ── aresta explícita (flatten-edge) → linkCatch _Ei94AFqPEfG5K7mY0I3I6w ──

        // ── ordem 2: linkCatch 'Validação Paralelos' (_Ei94AFqPEfG5K7mY0I3I6w, entrouPor=link) ──
        // Escrito explicitamente — não existe transição XPDL.

        // ── aresta directa → gateway Vistas do Juiz ? ──────────────────────────

        // ── ordem 3: gateway 'Vistas do Juiz ?' (_CtQ7BFqPEfG5K7mY0I3I6w, Exclusive, entrouPor=fluxo) ──
        // Regra de negócio: RI-transition-POC_EpatProcess-VistasdoJuiz
        // Expressão original (POC_Epat.xpdl, linha 1755):
        //   TIPOVISTAS=='JUIZ' || TIPOVISTAS == IPESystemValues.SW_NA;
        //   → verdadeiro: ramo "Juiz" (fora do percurso de referência deste segmento)
        //   → falso:      ramo "DRF" (OTHERWISE) → _CtQ7BVqPEfG5K7mY0I3I6w
        var vistasdoJuizBranch = aiimCase.TIPOVISTAS.Match(
            hasValue:     v => string.Equals(v, "JUIZ", StringComparison.Ordinal),
            notAvailable: () => true,   // SW_NA: condição verdadeira (TIPOVISTAS == SW_NA)
            empty:        () => false); // vazio: OTHERWISE → DRF

        if (vistasdoJuizBranch)
            return PocEpatProcessSeg052Terminal.VistasdoJuiz;

        // ── aresta OTHERWISE → gateway _CtQ7BVqPEfG5K7mY0I3I6w ────────────────

        // ── ordem 4: gateway _CtQ7BVqPEfG5K7mY0I3I6w (entrouPor=fluxo) ──────────
        // Traversal sem lógica adicional — segue para _tbOD4FqPEfG5K7mY0I3I6w.

        // ── aresta directa → userTask ──────────────────────────────────────────

        // ── ordem 5: userTask 'Realizar Atividade Vista Mista' (_tbOD4FqPEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        await _realizarAtividadeVistaMista.ExecuteAsync(caseRef, aiimCase, waitForVistaMista, ct)
                                          .ConfigureAwait(false);

        // ── aresta directa → signalThrow ───────────────────────────────────────

        // ── ordem 6: signalThrow 'Fim Vista Mista' (_InbWgFqQEfG5K7mY0I3I6w, entrouPor=fluxo) ──
        // Decidido: aresta explícita de continuação — sem evento de sinal intermediário.
        // Manter como evento de sinal introduziria ponto de espera que o TIBCO não tem.

        // ── aresta explícita (sem evento de sinal) → signalCatch ───────────────

        // ── ordem 7: signalCatch 'Fim Vista Mista' (_CtQ67FqPEfG5K7mY0I3I6w, entrouPor=sinal) ──
        // Escrito explicitamente — não existe transição XPDL.

        // ── aresta directa → fim do segmento (receiveTask Pedido de Vistas fora do percurso 22–28) ──

        return PocEpatProcessSeg052Terminal.FimVistaMista;
    }

    /// <summary>
    /// Suspende o fluxo em 'Pedido de Vistas' (<c>_CtQ68lqPEfG5K7mY0I3I6w</c>) aguardando
    /// correlação por bookmark (NOEQ-external-event, bookmark-correlation).
    ///
    /// <para>
    /// Este nó é visitado noutra passagem (SC-POC_EpatProcess-007) e não aparece no percurso
    /// de referência dos passos 22–28. Exposto via endpoint em Api/Endpoints.
    /// </para>
    /// </summary>
    /// <param name="caseRef">Referência do caso — <c>ProcessId</c> é a chave de correlação.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>
    /// <c>true</c> se a instância foi retomada; <c>false</c> se nenhuma instância aguardava.
    /// </returns>
    public Task<bool> ResumePedidoDeVistasAsync(AiimCaseRef caseRef, CancellationToken ct)
        => _correlationStore.ResumeAsync(caseRef.ProcessId, payload: null, ct);
}

/// <summary>
/// Terminais alcançáveis no segmento 052 do POC_EpatProcess.
/// </summary>
public enum PocEpatProcessSeg052Terminal
{
    /// <summary>
    /// Ramo "Juiz" do gateway 'Vistas do Juiz ?' — TIPOVISTAS == 'JUIZ' || TIPOVISTAS == SW_NA.
    /// Fora do percurso de referência dos passos 22–28 do cenário SC-POC_EpatProcess-012.
    /// </summary>
    VistasdoJuiz,

    /// <summary>
    /// Ramo "DRF" (OTHERWISE) do gateway 'Vistas do Juiz ?' — percurso dos passos 22–28.
    /// Atravessa o gateway <c>_CtQ7BVqPEfG5K7mY0I3I6w</c>, executa 'Realizar Atividade Vista Mista'
    /// e conclui em 'Fim Vista Mista' (<c>_CtQ67FqPEfG5K7mY0I3I6w</c>).
    /// </summary>
    FimVistaMista,
}
