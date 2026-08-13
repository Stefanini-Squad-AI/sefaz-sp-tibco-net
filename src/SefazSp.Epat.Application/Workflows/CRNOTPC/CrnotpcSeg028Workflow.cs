#nullable enable

using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CRNOTPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.CRNOTPC;

/// <summary>
/// Topologia do processo CRNOTPC — troco de "Start Event" a "Check Retries SW_QRETRYCOUNT".
/// Segmento 0 (prólogo) do cenário SC-CRNOTPC-001, passos 1–7.
///
/// Card: BUILD-CRNOTPC-seg028
/// Nós: 7 · Camadas: 2 (MAIN + subProcessScope Control System Task Call)
///
/// ┌─ MAIN scope ─────────────────────────────────────────────────────────────────────┐
/// │  [1] Start Event (_NcJJ4V9KEfGqPfX31TKC3w)           — ponto de entrada (AC1)   │
/// │   ↓ fluxo                                                                        │
/// │  [2] SetParameters (_NcJJ4l9KEfGqPfX31TKC3w)          — scriptTask (AC2)        │
/// │   ↓ fluxo                                                                        │
/// │  [3] Start Loop (_NcJJ5F9KEfGqPfX31TKC3w)             — scriptTask (AC3)        │
/// │   ↓ fluxo                                                                        │
/// │  [4] Control System Task Call (_NcJw8F9KEfGqPfX31TKC3w) — subProcessScope (AC3) │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC4                           │
/// │       ┌─ subProcessScope scope ───────────────────────────────────────────────┐  │
/// │       │ [5] startEvent interno (_NcJxKl9KEfGqPfX31TKC3w) — descida (AC4/AC5) │  │
/// │       │  ↓ fluxo                                                              │  │
/// │       │ [6] Start TX (_NcJxKF9KEfGqPfX31TKC3w)        — scriptTask (AC5)     │  │
/// │       │  ↓ fluxo                                                              │  │
/// │       │ [7] Check Retries SW_QRETRYCOUNT (_NcJxKV9KEfGqPfX31TKC3w) — (AC6)  │  │
/// │       │      ↓ Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES) → CriaNotificacao    │  │
/// │       │      ↓ Maxed → retentativas esgotadas                                │  │
/// │       └───────────────────────────────────────────────────────────────────────┘  │
/// └──────────────────────────────────────────────────────────────────────────────────┘
///
/// Nós sem transição XPDL — escritos como arestas explícitas:
///   - Ordem 5 (_NcJxKl9KEfGqPfX31TKC3w, descida): aresta de entrada explícita desde
///     o subProcessScope NodeControlSystemTaskCall até ao startEvent interno. Esta aresta
///     NÃO existe no XPDL; sem ela o fluxo não entra no subprocesso embutido (AC4).
/// </summary>
public sealed class CrnotpcSeg028Workflow
{
    // -------------------------------------------------------------------------
    // Identificadores de nó — invariantes: não renomear (card BUILD-CRNOTPC-seg028)
    // -------------------------------------------------------------------------

    /// <summary>Nó 1 — Start Event. Ponto de entrada sem transições de entrada (AC1).</summary>
    public const string NodeStartEvent            = "_NcJJ4V9KEfGqPfX31TKC3w";

    /// <summary>Nó 2 — SetParameters (scriptTask, escopo MAIN, AC2).</summary>
    public const string NodeSetParameters         = "_NcJJ4l9KEfGqPfX31TKC3w";

    /// <summary>Nó 3 — Start Loop (scriptTask, escopo MAIN, AC3).</summary>
    public const string NodeStartLoop             = "_NcJJ5F9KEfGqPfX31TKC3w";

    /// <summary>Nó 4 — Control System Task Call (subProcessScope, escopo MAIN, AC3).</summary>
    public const string NodeControlSystemTaskCall = "_NcJw8F9KEfGqPfX31TKC3w";

    /// <summary>
    /// Nó 5 — startEvent interno (escopo subProcessScope, AC4/AC5).
    /// Alcançado por DESCIDA explícita a partir de <see cref="NodeControlSystemTaskCall"/>.
    /// Esta aresta NÃO existe no XPDL; é criada explicitamente conforme AC4.
    /// </summary>
    public const string NodeInnerStartEvent       = "_NcJxKl9KEfGqPfX31TKC3w";

    /// <summary>Nó 6 — Start TX (scriptTask, escopo subProcessScope, AC5).</summary>
    public const string NodeStartTx               = "_NcJxKF9KEfGqPfX31TKC3w";

    /// <summary>Nó 7 — Check Retries SW_QRETRYCOUNT (gateway, escopo subProcessScope, AC6).</summary>
    public const string NodeCheckRetries          = "_NcJxKV9KEfGqPfX31TKC3w";

    // -------------------------------------------------------------------------
    // Ramos de saída do gateway Check Retries
    // -------------------------------------------------------------------------

    /// <summary>"Stillgood" — SW_QRETRYCOUNT &lt; MAXRETRIES → CriaNotificacao.</summary>
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
    ///   Lido, nunca escrito, pelo processo. (NOEQ-iprocess-builtin, ratificado 2026-08-06)
    /// </param>
    /// <returns>
    ///   <see cref="BranchStillgood"/> quando SW_QRETRYCOUNT &lt; MAXRETRIES;<br/>
    ///   <see cref="BranchMaxed"/> quando retentativas do motor esgotadas.
    /// </returns>
    public static string ExecutePrologue(
        ProcessExecutionContext ctx,
        FieldValue<long> idProcesso,
        string? processId,
        long swQRetryCount)
    {
        // Nó 2 — SetParameters (_NcJJ4l9KEfGqPfX31TKC3w, scriptTask, entrouPor=fluxo)
        // RI-script-CRNOTPC-SetParameters: inicializa MAXRETRIES e PROCESS_ID.
        // Domínio em CrnotpcSetParametersRule; envelope técnico aplicado aqui.
        CrnotpcSeg028Steps.ApplySetParameters(ctx, processId);

        // Nó 3 — Start Loop (_NcJJ5F9KEfGqPfX31TKC3w, scriptTask, entrouPor=fluxo)
        // Inicializa NUMAPPRETRIES=0 quando null (primeira entrada no laço).
        CrnotpcSeg028Steps.ApplyStartLoop(ctx);

        // Nó 4 — Control System Task Call (_NcJw8F9KEfGqPfX31TKC3w, subProcessScope, entrouPor=fluxo)
        // O escopo é instanciado; o fluxo desce para o interior por aresta explícita.

        // Nó 5 — startEvent interno (_NcJxKl9KEfGqPfX31TKC3w, entrouPor=descida)
        // ARESTA EXPLÍCITA: NodeControlSystemTaskCall ──descida──► NodeInnerStartEvent
        // Esta aresta NÃO existe no XPDL; sem ela o fluxo não entra no subprocesso embutido.
        // O startEvent recebe o token de controlo — nenhuma lógica de execução própria.

        // Nó 6 — Start TX (_NcJxKF9KEfGqPfX31TKC3w, scriptTask, entrouPor=fluxo)
        // Reinicia indicadores de erro antes da chamada de serviço.
        CrnotpcSeg028Steps.ApplyStartTx(ctx);

        // Nó 7 — Check Retries SW_QRETRYCOUNT (_NcJxKV9KEfGqPfX31TKC3w, gateway, entrouPor=fluxo)
        // RI-transition-CRNOTPC-CheckRetriesSWQRETRYCOUNT
        // SW_QRETRYCOUNT: lido do runtime, nunca escrito pelo processo de domínio.
        return CrnotpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES)
            ? BranchStillgood
            : BranchMaxed;
    }
}
