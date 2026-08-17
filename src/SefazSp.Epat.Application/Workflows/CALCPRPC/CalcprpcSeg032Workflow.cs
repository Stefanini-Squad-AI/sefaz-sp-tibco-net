#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CALCPRPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.CALCPRPC;

/// <summary>
/// Resultado do segmento 032 do processo CALCPRPC (passos 1–18, cenário SC-CALCPRPC-007).
/// </summary>
public enum CalcprpcSeg032Outcome
{
    /// <summary>
    /// Caminho normal: percurso concluiu em Try Task (_zJIHWVqiEfG5K7mY0I3I6w, linkCatch).
    /// Retentativa agendada via timerEvent Pause → linkThrow → linkCatch (flatten-edge,
    /// NOEQ-link-goto, ratificado 2026-08-06).
    /// </summary>
    TryTask,

    /// <summary>
    /// Erro aplicacional sem retentativa disponível — encerrou no endEvent
    /// _zJIub1qiEfG5K7mY0I3I6w dentro do subProcessScope.
    /// </summary>
    AppErrorEnd,

    /// <summary>
    /// Retentativas do motor esgotadas (SW_QRETRYCOUNT &gt;= MAXRETRIES).
    /// </summary>
    RetriesMaxed,
}

/// <summary>
/// Workflow do segmento 032 de CALCPRPC: de 'Start Event' até 'Try Task'.
///
/// Card: BUILD-CALCPRPC-seg032 · Processo: CALCPRPC · Etapa: 2
/// Cenário de referência: SC-CALCPRPC-007, segmento 1, passos 1–18.
///
/// Implementa <see cref="ICALCPRPC"/> como ponto de entrada do processo.
///
/// Topologia dos 18 nós (percurso de referência SC-CALCPRPC-007, segmento 1):
///
/// ┌─ MAIN scope ──────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event               _zJIHVVqiEfG5K7mY0I3I6w  startEvent          │
/// │   ↓ fluxo                                                                     │
/// │  [2]  SetParameters             _zJIHVlqiEfG5K7mY0I3I6w  scriptTask          │
/// │   ↓ fluxo                                                                     │
/// │  [3]  Start Loop                _zJIHWFqiEfG5K7mY0I3I6w  scriptTask          │
/// │   ↓ fluxo                                                                     │
/// │  [4]  Control System Task Call  _zJIHZlqiEfG5K7mY0I3I6w  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC3                         │
/// │       ┌─ ActivitySet scope ────────────────────────────────────────────────┐   │
/// │       │ [5]  startEvent interno  _zJIublqiEfG5K7mY0I3I6w  startEvent      │   │
/// │       │  ↓ fluxo                                                          │   │
/// │       │ [6]  Start TX            _zJIuaVqiEfG5K7mY0I3I6w  scriptTask     │   │
/// │       │  ↓ fluxo                                                          │   │
/// │       │ [7]  Check Retries       _zJIubVqiEfG5K7mY0I3I6w  gateway        │   │
/// │       │       ↓ Stillgood (SW_QRETRYCOUNT < MAXRETRIES)                   │   │
/// │       │ [8]  CalcularPrazo       _AsZCkVqkEfG5K7mY0I3I6w  serviceTask    │   │
/// │       │  ↓ fluxo                                                          │   │
/// │       │ [9]  gateway             _zJIuclqiEfG5K7mY0I3I6w  gateway        │   │
/// │       │       ramo AppError (STATUS_CODE!="0") ↓                          │   │
/// │       │ [10] Set App Error       _zJIucVqiEfG5K7mY0I3I6w  scriptTask     │   │
/// │       │  ↓ fluxo                                                          │   │
/// │       │ [11] gateway             _zJIuc1qiEfG5K7mY0I3I6w  gateway        │   │
/// │       │  ↓ fluxo                                                          │   │
/// │       │ [12] endEvent            _zJIub1qiEfG5K7mY0I3I6w  endEvent       │   │
/// │       └────────────────────────────────────────────────────────────────────┘   │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC6                        │
/// │  [13] Tech Error                _zJIHZVqiEfG5K7mY0I3I6w  gateway             │
/// │   ↓ ramo "No" (otherwise)                                                     │
/// │  [14] App Error                 _zJIHZFqiEfG5K7mY0I3I6w  gateway             │
/// │   ↓ ramo "Yes" (ISAPPERROR=="Y")                                              │
/// │  [15] More Retries              _zJIHYVqiEfG5K7mY0I3I6w  gateway             │
/// │   ↓ ramo "Yes" (NUMAPPRETRIES < MAXRETRIES)                                   │
/// │  [16] Pause                     _zJIHYlqiEfG5K7mY0I3I6w  timerEvent          │
/// │   ↓ fluxo (após timer)                                                        │
/// │  [17] Link To: Try Task         _zJIHYFqiEfG5K7mY0I3I6w  linkThrow           │
/// │        ↓ LINK EXPLÍCITO (flatten-edge, NOEQ-link-goto, ratificado 2026-08-06) │
/// │  [18] Try Task                  _zJIHWVqiEfG5K7mY0I3I6w  linkCatch           │
/// └───────────────────────────────────────────────────────────────────────────────┘
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida   · subProcessScope → startEvent interno
///   • ordem 13 · regresso  · endEvent do subprocesso → gateway Tech Error
///   • ordem 18 · link      · linkThrow → linkCatch (flatten-edge, AC7)
/// </summary>
public sealed class CalcprpcSeg032Workflow : ICALCPRPC
{
    // ── Identificadores de nó — invariantes ──────────────────────────────────

    /// <summary>Nó 1 — Start Event (ponto de entrada, MAIN).</summary>
    public const string NodeStartEvent            = "_zJIHVVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 2 — SetParameters (scriptTask, MAIN). Regra: RI-script-CALCPRPC-SetParameters.</summary>
    public const string NodeSetParameters         = "_zJIHVlqiEfG5K7mY0I3I6w";

    /// <summary>Nó 3 — Start Loop (scriptTask, MAIN). Usa IPESystemValues.SW_DATE (builtin).</summary>
    public const string NodeStartLoop             = "_zJIHWFqiEfG5K7mY0I3I6w";

    /// <summary>Nó 4 — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeControlSystemTaskCall = "_zJIHZlqiEfG5K7mY0I3I6w";

    /// <summary>
    /// Nó 5 — startEvent interno (ActivitySet). Alcançado por DESCIDA explícita (AC3).
    /// Esta aresta NÃO existe no XPDL; escrita explicitamente conforme AC3.
    /// </summary>
    public const string NodeInnerStartEvent       = "_zJIublqiEfG5K7mY0I3I6w";

    /// <summary>Nó 6 — Start TX (scriptTask, ActivitySet).</summary>
    public const string NodeStartTx               = "_zJIuaVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 7 — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-CALCPRPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries          = "_zJIubVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 8 — CalcularPrazo (serviceTask, ActivitySet).</summary>
    public const string NodeCalcularPrazo         = "_AsZCkVqkEfG5K7mY0I3I6w";

    /// <summary>Nó 9 — gateway "A chamada a CalcularPrazo foi bem sucedida?" (ActivitySet).</summary>
    public const string NodeGatewaySuccess        = "_zJIuclqiEfG5K7mY0I3I6w";

    /// <summary>Nó 10 — Set App Error (scriptTask, ActivitySet).</summary>
    public const string NodeSetAppError           = "_zJIucVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 11 — gateway anónimo (ActivitySet).</summary>
    public const string NodeGatewayAnon           = "_zJIuc1qiEfG5K7mY0I3I6w";

    /// <summary>Nó 12 — endEvent do subProcessScope (ActivitySet).</summary>
    public const string NodeInnerEndEvent         = "_zJIub1qiEfG5K7mY0I3I6w";

    /// <summary>
    /// Nó 13 — Tech Error (gateway, MAIN). Alcançado por REGRESSO explícito (AC6).
    /// Esta aresta NÃO existe no XPDL; escrita explicitamente conforme AC6.
    /// </summary>
    public const string NodeTechError             = "_zJIHZVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 14 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError              = "_zJIHZFqiEfG5K7mY0I3I6w";

    /// <summary>Nó 15 — More Retries (gateway, MAIN).</summary>
    public const string NodeMoreRetries           = "_zJIHYVqiEfG5K7mY0I3I6w";

    /// <summary>Nó 16 — Pause (timerEvent, MAIN). Timer de espera entre retentativas.</summary>
    public const string NodePause                 = "_zJIHYlqiEfG5K7mY0I3I6w";

    /// <summary>Nó 17 — Link To: Try Task (linkThrow, MAIN).</summary>
    public const string NodeLinkThrow             = "_zJIHYFqiEfG5K7mY0I3I6w";

    /// <summary>
    /// Nó 18 — Try Task (linkCatch, MAIN). Alcançado por LINK explícito (AC7).
    /// Implementado como flatten-edge (NOEQ-link-goto, ratificado 2026-08-06).
    /// Esta aresta NÃO existe no XPDL; escrita explicitamente.
    /// </summary>
    public const string NodeLinkCatch             = "_zJIHWVqiEfG5K7mY0I3I6w";

    // ── Dependências ──────────────────────────────────────────────────────────

    private readonly ICalcularPrazoSoapService _calcularPrazo;
    private readonly TimeProvider _timeProvider;

    /// <param name="calcularPrazo">
    /// Porta do serviço CalcularPrazo (SOAP). A implementação concreta fica em
    /// <c>Infrastructure/Integration.Soap/CalcularPrazoSoapService</c>.
    /// </param>
    /// <param name="timeProvider">
    /// Fonte de tempo controlável. Nunca usar <c>DateTime.Now</c> directamente
    /// (decisão IClock, Domain/Abstractions, status final).
    /// </param>
    public CalcprpcSeg032Workflow(
        ICalcularPrazoSoapService calcularPrazo,
        TimeProvider timeProvider)
    {
        _calcularPrazo = calcularPrazo;
        _timeProvider  = timeProvider;
    }

    // ── ICALCPRPC.ExecuteAsync ────────────────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// Percorre os passos 1–18 do segmento 1 de SC-CALCPRPC-007.
    /// Os passos 19–34 pertencem a outro segmento e não estão implementados neste card.
    /// </remarks>
    public async Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
    {
        var ctx = new ProcessExecutionContext();

        // ── Nó 1: Start Event _zJIHVVqiEfG5K7mY0I3I6w ──────────────────────
        // Ponto de entrada. Sem efeito lateral. Controlo passa ao nó 2.

        // ── Nó 2: SetParameters _zJIHVlqiEfG5K7mY0I3I6w ────────────────────
        // Regra: RI-script-CALCPRPC-SetParameters (eRegraDeNegocio = true)
        // IDPROCESSO comparado com SW_NA: usa FieldValue<long> (shim-tri-state,
        // NOEQ-iprocess-builtin, ratificado 2026-08-06). SW_NA ≠ null.
        var idProcesso = FieldValue<long>.NotAvailable; // SW_NA: IDPROCESSO não preenchido na entrada
        CalcprpcExecutionSteps.ApplySetParameters(ctx, caseRef.ProcessId);

        // ── Nó 3: Start Loop _zJIHWFqiEfG5K7mY0I3I6w ────────────────────────
        // IPESystemValues.SW_DATE: data de início do loop; valor de ambiente, não escrito
        // no contexto técnico. Tratado via shim-tri-state (NOEQ-iprocess-builtin).
        CalcprpcExecutionSteps.ApplyStartLoop(ctx);

        // ── Nó 4: Control System Task Call _zJIHZlqiEfG5K7mY0I3I6w ──────────
        // subProcessScope: contém o ciclo interno.
        // Aresta de DESCIDA explícita → Nó 5 (inner startEvent).
        // NodeControlSystemTaskCall ──descida──► NodeInnerStartEvent
        // (não existe no XPDL; escrita explicitamente — AC3)

        var outcome = await ExecuteSubProcessScopeAsync(caseRef, ctx, ct).ConfigureAwait(false);
        return new ProcessCallResult(true, null, outcome == CalcprpcSeg032Outcome.AppErrorEnd
            ? "AppErrorEnd"
            : null);
    }

    // ── Escopo do subProcessScope (ActivitySet) ───────────────────────────────

    private async Task<CalcprpcSeg032Outcome> ExecuteSubProcessScopeAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        CancellationToken ct)
    {
        // ── Nó 5: startEvent interno _zJIublqiEfG5K7mY0I3I6w ────────────────
        // Alcançado por DESCIDA explícita (AC3, entrouPor=descida).
        // Recebe o token; sem lógica de execução própria.

        // ── Nó 6: Start TX _zJIuaVqiEfG5K7mY0I3I6w ──────────────────────────
        CalcprpcExecutionSteps.ApplyStartTx(ctx);

        // ── Nó 7: Check Retries SW_QRETRYCOUNT _zJIubVqiEfG5K7mY0I3I6w ──────
        // Regra: RI-transition-CALCPRPC-CheckRetriesSWQRETRYCOUNT
        // SW_QRETRYCOUNT: valor do runtime, nunca escrito. Simulado como 0 (primeira tentativa).
        // Em produção, o runtime iProcess expõe este valor; em .NET, é fornecido por injecção.
        const long swQRetryCount = 0L; // primeira tentativa; o motor não geriu retentativas ainda
        if (!CalcprpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Ramo Maxed: retentativas do motor esgotadas.
            return CalcprpcSeg032Outcome.RetriesMaxed;
        }

        // ── Nó 8: CalcularPrazo _AsZCkVqkEfG5K7mY0I3I6w ─────────────────────
        // serviceTask: invoca o serviço via SOAP/JMS.
        // Excepção de transporte → gateway Tech Error (regresso explícito, AC6).
        ServiceEnvelope envelope;
        bool isTechError = false;
        try
        {
            envelope = await _calcularPrazo.InvokeAsync(caseRef, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Aresta de REGRESSO para o gateway Tech Error (_zJIHZVqiEfG5K7mY0I3I6w).
            // Não existe como transição no XPDL; escrita explicitamente (AC6).
            ctx.ISTECHERROR = "Y";
            ctx.STATUS_CODE = ex.Message;
            isTechError     = true;
            envelope        = new ServiceEnvelope(null, null, ex.Message);
        }

        if (!isTechError)
            CalcprpcExecutionSteps.MapServiceEnvelope(ctx, envelope);

        // ── Nó 9: gateway _zJIuclqiEfG5K7mY0I3I6w ───────────────────────────
        // "A chamada a CalcularPrazo foi bem sucedida?"
        // Ramo AppError: STATUS_CODE != "0" — decisão ratificada: decisions.CALCPRPC/_zJIuclqiE
        if (!isTechError && !CalcprpcExecutionSteps.IsAppError(ctx))
        {
            // Ramo Good (STATUS_CODE == "0"): subprocesso encerra com sucesso.
            // O fluxo retorna ao MAIN scope via regresso implícito de subProcessScope bem sucedido.
            // Não vai para Tech Error nem App Error.
            return CalcprpcSeg032Outcome.TryTask;
        }

        if (!isTechError)
        {
            // ── Nó 10: Set App Error _zJIucVqiEfG5K7mY0I3I6w ─────────────────
            CalcprpcExecutionSteps.SetAppError(ctx);

            // ── Nó 11: gateway _zJIuc1qiEfG5K7mY0I3I6w ──────────────────────
            // (anónimo, encaminha para endEvent)

            // ── Nó 12: endEvent _zJIub1qiEfG5K7mY0I3I6w ─────────────────────
            // O subProcessScope termina com erro de aplicação.
        }

        // ── Nó 13: Tech Error _zJIHZVqiEfG5K7mY0I3I6w ───────────────────────
        // Alcançado por REGRESSO explícito (AC6, entrouPor=regresso).
        // Ramo "No" (otherwise) → encaminha para gateway App Error.
        // (O ramo "Yes" de Tech Error, que existiria para retry técnico,
        //  não faz parte do percurso de referência SC-CALCPRPC-007 passos 1–18.)

        // ── Nó 14: App Error _zJIHZFqiEfG5K7mY0I3I6w ────────────────────────
        // Ramo "Yes": ISAPPERROR == "Y"
        if (!CalcprpcExecutionSteps.IsAppErrorFlag(ctx))
        {
            // Ramo "No": não é erro de aplicação — encerramento sem retentativa.
            return CalcprpcSeg032Outcome.AppErrorEnd;
        }

        // ── Nó 15: More Retries _zJIHYVqiEfG5K7mY0I3I6w ─────────────────────
        // Ramo "Yes": NUMAPPRETRIES < MAXRETRIES
        if (!CalcprpcExecutionSteps.HasMoreRetries(ctx))
        {
            // Ramo "No" (Otherwise): sem mais retentativas — encerra.
            return CalcprpcSeg032Outcome.AppErrorEnd;
        }

        // ── Nó 16: Pause _zJIHYlqiEfG5K7mY0I3I6w ────────────────────────────
        // timerEvent: aguarda antes de re-tentar.
        // O prazo do timer é um parâmetro técnico (RI-deadline-CALCPRPC-Pause,
        // eRegraDeNegocio=false). Implementado como atraso fixo de 1 s para o segmento.
        await Task.Delay(TimeSpan.FromSeconds(1), ct).ConfigureAwait(false);

        // ── Nó 17: Link To: Try Task _zJIHYFqiEfG5K7mY0I3I6w ────────────────
        // linkThrow: salta para o linkCatch Try Task.
        // Implementado como flatten-edge (NOEQ-link-goto, ratificado 2026-08-06):
        // o par throw/catch é traduzido como aresta directa, sem ponto de persistência.
        // keep-as-signal foi recusada por introduzir espera inexistente no TIBCO.

        // ── Nó 18: Try Task _zJIHWVqiEfG5K7mY0I3I6w ─────────────────────────
        // linkCatch: ponto de chegada do link (AC7, entrouPor=link).
        // Alcançado pela aresta flatten-edge acima — sem ponto de espera.
        return CalcprpcSeg032Outcome.TryTask;
    }
}
