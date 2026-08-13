#nullable enable

using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows;

/// <summary>
/// Topologia do processo BSCENVPC — troco de "Start Event" a "Check Retries SW_QRETRYCOUNT".
/// Segmento 0 (prólogo) do cenário SC-BSCENVPC-001.
///
/// Card: BUILD-BSCENVPC-seg011
/// Nós: 7 · Camadas: 2 (MAIN + ActivitySet) · Pastas: 3
///
/// ┌─ MAIN scope ─────────────────────────────────────────────────────────────────┐
/// │  [1] Start Event (_qIDulF6BEfGBBLgT-R5iuw)           — ponto de entrada, AC1 │
/// │   ↓ fluxo                                                                    │
/// │  [2] SetParameters (_qIDulV6BEfGBBLgT-R5iuw)          — scriptTask, AC2      │
/// │   ↓ fluxo                                                                    │
/// │  [3] Start Loop (_qIDul16BEfGBBLgT-R5iuw)             — scriptTask, AC3      │
/// │   ↓ fluxo                                                                    │
/// │  [4] Control System Task Call (_qIDupV6BEfGBBLgT-R5iuw) — subProcessScope    │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC5                        │
/// │       ┌─ ActivitySet scope ─────────────────────────────────────────────────┐ │
/// │       │ [5] startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw) — AC5             │ │
/// │       │  ↓ fluxo                                                            │ │
/// │       │ [6] Start TX (_qIDu3F6BEfGBBLgT-R5iuw)        — scriptTask, AC6    │ │
/// │       │  ↓ fluxo                                                            │ │
/// │       │ [7] Check Retries SW_QRETRYCOUNT (_qIDu3V6BEfGBBLgT-R5iuw) — AC7  │ │
/// │       │      ↓ Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES)                    │ │
/// │       │         → Busca Envolvidos Vista Por AIIM                           │ │
/// │       └─────────────────────────────────────────────────────────────────────┘ │
/// └──────────────────────────────────────────────────────────────────────────────┘
/// </summary>
public sealed class BscenvpcWorkflow
{
    // -------------------------------------------------------------------------
    // Identificadores de nó — invariantes: não renomear (card BUILD-BSCENVPC-seg011)
    // -------------------------------------------------------------------------

    /// <summary>Nó 1 — Start Event. Ponto de entrada sem transições de entrada (AC1).</summary>
    public const string NodeStartEvent            = "_qIDulF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 2 — SetParameters (scriptTask, escopo MAIN, AC2).</summary>
    public const string NodeSetParameters         = "_qIDulV6BEfGBBLgT-R5iuw";

    /// <summary>Nó 3 — Start Loop (scriptTask, escopo MAIN, AC3).</summary>
    public const string NodeStartLoop             = "_qIDul16BEfGBBLgT-R5iuw";

    /// <summary>Nó 4 — Control System Task Call (subProcessScope, escopo MAIN, AC4).</summary>
    public const string NodeControlSystemTaskCall = "_qIDupV6BEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó 5 — startEvent interno (escopo ActivitySet, AC5).
    /// Alcançado por DESCIDA explícita a partir de <see cref="NodeControlSystemTaskCall"/>.
    /// Esta aresta NÃO existe no XPDL; é criada explicitamente conforme AC5.
    /// </summary>
    public const string NodeInnerStartEvent       = "_qIDu3l6BEfGBBLgT-R5iuw";

    /// <summary>Nó 6 — Start TX (scriptTask, escopo ActivitySet, AC6).</summary>
    public const string NodeStartTx               = "_qIDu3F6BEfGBBLgT-R5iuw";

    /// <summary>Nó 7 — Check Retries SW_QRETRYCOUNT (gateway, escopo ActivitySet, AC7).</summary>
    public const string NodeCheckRetries          = "_qIDu3V6BEfGBBLgT-R5iuw";

    // -------------------------------------------------------------------------
    // Ramos de saída do gateway Check Retries
    // -------------------------------------------------------------------------

    /// <summary>"Stillgood" — SW_QRETRYCOUNT &lt; MAXRETRIES → Busca Envolvidos Vista Por AIIM.</summary>
    public const string BranchStillgood = "Stillgood";

    /// <summary>"Maxed" — retentativas do motor esgotadas.</summary>
    public const string BranchMaxed     = "Maxed";

    // -------------------------------------------------------------------------
    // Execução do prólogo: Start Event → ... → Check Retries SW_QRETRYCOUNT
    // -------------------------------------------------------------------------

    /// <summary>
    /// Executa o troco de "Start Event" a "Check Retries SW_QRETRYCOUNT".
    /// Devolve o ramo de saída do gateway (<see cref="BranchStillgood"/> ou <see cref="BranchMaxed"/>).
    ///
    /// Percurso dos nós:
    ///   1 Start Event  →  2 SetParameters  →  3 Start Loop
    ///   →  4 Control System Task Call
    ///      [descida explícita → 5 inner startEvent]
    ///   →  6 Start TX  →  7 Check Retries SW_QRETRYCOUNT
    /// </summary>
    /// <param name="ctx">Contexto de execução do processo (MAXRETRIES, NUMAPPRETRIES, …).</param>
    /// <param name="idProcesso">
    ///   Campo IDPROCESSO tri-estado (shim-tri-state, NOEQ-iprocess-builtin).
    ///   HasValue = preenchido; IsNotAvailable = SW_NA (nunca null); Empty = não declarado.
    /// </param>
    /// <param name="processId">Valor de PROCESS_ID a gravar no contexto, ou null.</param>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime.
    ///   Lido, nunca escrito, pelo processo.
    /// </param>
    public static string ExecutePrologue(
        ProcessExecutionContext ctx,
        FieldValue<long> idProcesso,
        string? processId,
        long swQRetryCount)
    {
        // Nó 2 — SetParameters (scriptTask)
        // Envelope técnico: inicializa MAXRETRIES e PROCESS_ID.
        // Domínio (BscenvpcSetParametersRule.ShouldInitialize) determina se o contexto precisa init,
        // mas o envelope aplica sempre o default quando MAXRETRIES==0 (não inicializado).
        BscenvpcExecutionSteps.ApplySetParameters(ctx, processId);

        // Nó 3 — Start Loop (scriptTask)
        BscenvpcExecutionSteps.ApplyStartLoop(ctx);

        // Nó 4 — Control System Task Call (subProcessScope)
        // Aresta de DESCIDA explícita → Nó 5 (inner startEvent).
        // NodeControlSystemTaskCall ──descida──► NodeInnerStartEvent
        // (aresta não existe no XPDL; criada aqui conforme AC4/AC5)

        // Nó 5 — inner startEvent (recebe o controlo por descida — AC5)
        // Nenhuma lógica de execução; apenas recepciona o token.

        // Nó 6 — Start TX (scriptTask, escopo ActivitySet)
        BscenvpcExecutionSteps.ApplyStartTx(ctx);

        // Nó 7 — Check Retries SW_QRETRYCOUNT (gateway)
        // RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT
        return BscenvpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES)
            ? BranchStillgood
            : BranchMaxed;
    }
}
