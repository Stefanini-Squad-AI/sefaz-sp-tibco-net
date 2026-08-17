#nullable enable

using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Resultado possível do percurso do segmento 049 do processo BSCENVPC.
/// </summary>
public enum BscenvpcSeg049Outcome
{
    /// <summary>
    /// Percurso completo: Done - Success (_qIDuol6BEfGBBLgT-R5iuw).
    /// ISTECHERROR != "Y" e ISAPPERROR != "Y" após a saída do ActivitySet.
    /// </summary>
    DoneSuccess,

    /// <summary>
    /// Ramo "Yes" no gateway Tech Error (_qIDupF6BEfGBBLgT-R5iuw):
    /// ISTECHERROR == "Y". O chamador é responsável pelo tratamento.
    /// </summary>
    TechError,

    /// <summary>
    /// Ramo "Yes" no gateway App Error (_qIDuo16BEfGBBLgT-R5iuw):
    /// ISAPPERROR == "Y". O chamador é responsável pelo tratamento.
    /// </summary>
    AppError,

    /// <summary>
    /// Ramo "Stillgood" no gateway Check Retries (_qIDu3V6BEfGBBLgT-R5iuw):
    /// SW_QRETRYCOUNT &lt; MAXRETRIES. O motor ainda tem retentativas disponíveis;
    /// a chamada de serviço é tratada noutro segmento.
    /// </summary>
    Stillgood,
}

/// <summary>
/// Workflow do segmento 049 do processo BSCENVPC:
/// de 'Start Event' a 'Done - Success' (12 nós, percurso SC-BSCENVPC-016, segmento 1).
///
/// Card: BUILD-BSCENVPC-seg049 · Processo: BSCENVPC · Etapa(s): 5
/// Cenário de referência: SC-BSCENVPC-016, ordemNaJornada=1, passos 1–12.
///
/// ┌─ MAIN scope ───────────────────────────────────────────────────────────────────┐
/// │  [1]  Start Event (_qIDulF6BEfGBBLgT-R5iuw)               startEvent          │
/// │        Ponto de entrada; chamado por POC_EpatProcess/Busca Emails.             │
/// │   ↓ fluxo                                                                      │
/// │  [2]  SetParameters (_qIDulV6BEfGBBLgT-R5iuw)             scriptTask          │
/// │        Regra: RI-script-BSCENVPC-SetParameters                                 │
/// │        NOEQ-iprocess-builtin: IDPROCESSO vs SW_NA → shim-tri-state            │
/// │   ↓ fluxo                                                                      │
/// │  [3]  Start Loop (_qIDul16BEfGBBLgT-R5iuw)                scriptTask          │
/// │        NOEQ-iprocess-builtin: SW_DATE como valor de ambiente                   │
/// │   ↓ fluxo                                                                      │
/// │  [4]  Control System Task Call (_qIDupV6BEfGBBLgT-R5iuw)  subProcessScope     │
/// │        ↓ DESCIDA EXPLÍCITA (não existe no XPDL) — AC4/AC5                      │
/// │       ┌─ ActivitySet scope ────────────────────────────────────────────────┐   │
/// │       │ [5]  startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw) startEvent      │   │
/// │       │       entrouPor=descida — aresta explícita, não existe no XPDL     │   │
/// │       │  ↓ fluxo                                                           │   │
/// │       │ [6]  Start TX (_qIDu3F6BEfGBBLgT-R5iuw)           scriptTask      │   │
/// │       │       Reinicia STATUS_CODE=null, ISAPPERROR="N", ISTECHERROR="N"   │   │
/// │       │  ↓ fluxo                                                           │   │
/// │       │ [7]  Check Retries SW_QRETRYCOUNT                  gateway         │   │
/// │       │      (_qIDu3V6BEfGBBLgT-R5iuw)                                    │   │
/// │       │       Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT     │   │
/// │       │       Stillgood (SW_QRETRYCOUNT &lt; MAXRETRIES) → serviço (outro seg.) │ │
/// │       │       Maxretriesexceeded (OTHERWISE) ↓                             │   │
/// │       │ [8]  Set Technical Error (_qIDu4F6BEfGBBLgT-R5iuw) scriptTask     │   │
/// │       │       Documenta esgotamento; NÃO escreve ISTECHERROR="Y"           │   │
/// │       │  ↓ fluxo                                                           │   │
/// │       │ [9]  endEvent (_qIDu316BEfGBBLgT-R5iuw)            endEvent        │   │
/// │       └────────────────────────────────────────────────────────────────────┘   │
/// │        ↓ REGRESSO EXPLÍCITO (não existe no XPDL) — AC6                        │
/// │  [10] Tech Error (_qIDupF6BEfGBBLgT-R5iuw)                gateway             │
/// │        entrouPor=regresso — aresta explícita, não existe no XPDL              │
/// │        Ramo "Yes": ISTECHERROR == "Y" → caminho de erro técnico               │
/// │        Ramo "No" (OTHERWISE) ↓                                                 │
/// │  [11] App Error (_qIDuo16BEfGBBLgT-R5iuw)                 gateway             │
/// │        Ramo "Yes": ISAPPERROR == "Y" → caminho de erro de aplicação           │
/// │        Ramo "No" (OTHERWISE) ↓                                                 │
/// │  [12] Done - Success (_qIDuol6BEfGBBLgT-R5iuw)            endEvent            │
/// └────────────────────────────────────────────────────────────────────────────────┘
///
/// Passos com entrouPor != fluxo — escritos explicitamente (não existem no XPDL):
///   • ordem 5  · descida  · _qIDupV6BEfGBBLgT-R5iuw → _qIDu3l6BEfGBBLgT-R5iuw
///   • ordem 10 · regresso · _qIDu316BEfGBBLgT-R5iuw  → _qIDupF6BEfGBBLgT-R5iuw
///
/// NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06):
///   • nó 2 SetParameters: IDPROCESSO comparado com SW_NA via <see cref="FieldValue{T}"/>.
///     SW_NA é um terceiro estado; NUNCA mapeado para null.
///   • nó 3 Start Loop: IPESystemValues.SW_DATE tratado como valor de ambiente
///     (fornecido pelo runtime; não escrito no contexto técnico).
/// </summary>
public sealed class BscenvpcSeg049Workflow
{
    // ── Identificadores de nó — invariantes (não renomear) ───────────────────

    /// <summary>Nó 1  — Start Event (MAIN, ponto de entrada).</summary>
    public const string NodeStartEvent            = "_qIDulF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 2  — SetParameters (scriptTask, MAIN). Regra: RI-script-BSCENVPC-SetParameters.</summary>
    public const string NodeSetParameters         = "_qIDulV6BEfGBBLgT-R5iuw";

    /// <summary>Nó 3  — Start Loop (scriptTask, MAIN). IPESystemValues.SW_DATE como valor de ambiente.</summary>
    public const string NodeStartLoop             = "_qIDul16BEfGBBLgT-R5iuw";

    /// <summary>Nó 4  — Control System Task Call (subProcessScope, MAIN).</summary>
    public const string NodeControlSystemTaskCall = "_qIDupV6BEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó 5  — startEvent interno (ActivitySet, entrouPor=descida).
    /// Alcançado por DESCIDA EXPLÍCITA a partir de <see cref="NodeControlSystemTaskCall"/>.
    /// Esta aresta NÃO existe no XPDL; criada explicitamente (AC4/AC5).
    /// </summary>
    public const string NodeInnerStartEvent       = "_qIDu3l6BEfGBBLgT-R5iuw";

    /// <summary>Nó 6  — Start TX (scriptTask, ActivitySet). Reinicia STATUS_CODE/ISAPPERROR/ISTECHERROR.</summary>
    public const string NodeStartTx               = "_qIDu3F6BEfGBBLgT-R5iuw";

    /// <summary>Nó 7  — Check Retries SW_QRETRYCOUNT (gateway, ActivitySet). Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT.</summary>
    public const string NodeCheckRetries          = "_qIDu3V6BEfGBBLgT-R5iuw";

    /// <summary>Nó 8  — Set Technical Error (scriptTask, ActivitySet). Executado quando SW_QRETRYCOUNT >= MAXRETRIES.</summary>
    public const string NodeSetTechnicalError     = "_qIDu4F6BEfGBBLgT-R5iuw";

    /// <summary>Nó 9  — endEvent interno (ActivitySet). Fim do ActivitySet; segue para regresso ao MAIN.</summary>
    public const string NodeInnerEndEvent         = "_qIDu316BEfGBBLgT-R5iuw";

    /// <summary>
    /// Nó 10 — Tech Error (gateway, MAIN, entrouPor=regresso).
    /// Alcançado por REGRESSO EXPLÍCITO a partir de <see cref="NodeInnerEndEvent"/>.
    /// Esta aresta NÃO existe no XPDL; criada explicitamente (AC6).
    /// </summary>
    public const string NodeTechError             = "_qIDupF6BEfGBBLgT-R5iuw";

    /// <summary>Nó 11 — App Error (gateway, MAIN).</summary>
    public const string NodeAppError              = "_qIDuo16BEfGBBLgT-R5iuw";

    /// <summary>Nó 12 — Done - Success (endEvent, MAIN). Terminal da jornada.</summary>
    public const string NodeDoneSuccess           = "_qIDuol6BEfGBBLgT-R5iuw";

    // ── Execução do segmento completo ─────────────────────────────────────────

    /// <summary>
    /// Executa o percurso completo do segmento 049 de BSCENVPC:
    /// de 'Start Event' a 'Done - Success' (12 nós).
    ///
    /// Percurso feliz (SC-BSCENVPC-016):
    ///   nós 1–3 (prólogo MAIN) → nó 4 (subProcessScope)
    ///   → [descida explícita] nó 5 (inner startEvent)
    ///   → nós 6–7 (Start TX, Check Retries)
    ///   → ramo OTHERWISE (Maxretriesexceeded) → nó 8 (Set Technical Error)
    ///   → nó 9 (endEvent ActivitySet)
    ///   → [regresso explícito] nó 10 (Tech Error: "No")
    ///   → nó 11 (App Error: "No")
    ///   → nó 12 (Done - Success).
    ///
    /// Devolve <see cref="BscenvpcSeg049Outcome.DoneSuccess"/> no percurso acima.
    /// Os outros outcomes sinalizam saídas alternativas para o chamador.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="idProcesso">
    ///   Campo IDPROCESSO tri-estado (shim-tri-state, NOEQ-iprocess-builtin).
    ///   HasValue = preenchido; IsNotAvailable = SW_NA (nunca null); Empty = não declarado.
    /// </param>
    /// <param name="processId">
    ///   Valor de PROCESS_ID a gravar no contexto, ou null quando IDPROCESSO == SW_NA.
    /// </param>
    /// <param name="swQRetryCount">
    ///   Valor de IPESystemValues.SW_QRETRYCOUNT fornecido pelo runtime.
    ///   Lido, nunca escrito, pelo processo.
    /// </param>
    /// <returns>O desfecho do segmento.</returns>
    public static BscenvpcSeg049Outcome Execute(
        ProcessExecutionContext ctx,
        FieldValue<long> idProcesso,
        string? processId,
        long swQRetryCount)
    {
        // ── Nó 1 — Start Event (_qIDulF6BEfGBBLgT-R5iuw) ────────────────────
        // Ponto de entrada sem transição de entrada; chamado por POC_EpatProcess/Busca Emails.
        // Nenhuma lógica de execução — o token é recebido pelo processo.

        // ── Nó 2 — SetParameters (_qIDulV6BEfGBBLgT-R5iuw) ──────────────────
        // Regra: RI-script-BSCENVPC-SetParameters
        //   IDPROCESSO != SW_NA | MAXRETRIES==null → inicializa MAXRETRIES e PROCESS_ID.
        // NOEQ-iprocess-builtin: SW_NA é um terceiro estado via FieldValue<T>.
        // Domínio (BscenvpcSetParametersRule.ShouldInitialize) decide se há algo a fazer;
        // o envelope técnico (BscenvpcExecutionSteps.ApplySetParameters) aplica o estado.
        BscenvpcExecutionSteps.ApplySetParameters(ctx, processId);

        // ── Nó 3 — Start Loop (_qIDul16BEfGBBLgT-R5iuw) ─────────────────────
        // NOEQ-iprocess-builtin: IPESystemValues.SW_DATE é um valor de ambiente do runtime.
        // Neste nó, SW_DATE é lido (nunca escrito) para iniciar o loop de retentativas.
        // O .NET não tem equivalente directo; o valor é tratado como dado de ambiente
        // fornecido pelo executor (IProcessValues) — não escrito no ProcessExecutionContext.
        BscenvpcExecutionSteps.ApplyStartLoop(ctx);

        // ── Nó 4 — Control System Task Call (_qIDupV6BEfGBBLgT-R5iuw) ────────
        // subProcessScope: descida para o ActivitySet interno.
        // ── DESCIDA EXPLÍCITA (aresta NÃO existe no XPDL) ───────────────────
        // _qIDupV6BEfGBBLgT-R5iuw ──descida──► _qIDu3l6BEfGBBLgT-R5iuw

        // ── Nó 5 — startEvent interno (_qIDu3l6BEfGBBLgT-R5iuw) ─────────────
        // entrouPor=descida — recebe o token por descida explícita; nenhuma lógica.

        // ── Nó 6 — Start TX (_qIDu3F6BEfGBBLgT-R5iuw) ───────────────────────
        // Reinicia os indicadores de erro antes de iniciar a transacção:
        // STATUS_CODE=null, ISAPPERROR="N", ISTECHERROR="N".
        BscenvpcExecutionSteps.ApplyStartTx(ctx);

        // ── Nó 7 — Check Retries SW_QRETRYCOUNT (_qIDu3V6BEfGBBLgT-R5iuw) ───
        // Regra: RI-transition-BSCENVPC-CheckRetriesSWQRETRYCOUNT
        //   Stillgood:          SW_QRETRYCOUNT < MAXRETRIES  → chamada de serviço (outro segmento)
        //   Maxretriesexceeded: SW_QRETRYCOUNT >= MAXRETRIES (OTHERWISE) → Set Technical Error
        if (BscenvpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Ramo "Stillgood": motor ainda tem retentativas; chamada de serviço noutro segmento.
            return BscenvpcSeg049Outcome.Stillgood;
        }

        // ── Nó 8 — Set Technical Error (_qIDu4F6BEfGBBLgT-R5iuw) ─────────────
        // Ramo OTHERWISE ("Maxretriesexceeded"): SW_QRETRYCOUNT >= MAXRETRIES.
        // Documenta o esgotamento das retentativas do motor; NÃO escreve ISTECHERROR="Y".
        // (ISTECHERROR foi reiniciado para "N" pelo Start TX; o gateway Tech Error,
        //  avaliado após o regresso, tomará o ramo "No" conforme SC-BSCENVPC-016.)
        BscenvpcSeg049Steps.ApplySetTechnicalError(ctx, swQRetryCount);

        // ── Nó 9 — endEvent (_qIDu316BEfGBBLgT-R5iuw) ────────────────────────
        // Fim do ActivitySet (escopo interno); controlo regressa ao MAIN.
        // ── REGRESSO EXPLÍCITO (aresta NÃO existe no XPDL) ──────────────────
        // _qIDu316BEfGBBLgT-R5iuw ──regresso──► _qIDupF6BEfGBBLgT-R5iuw

        // ── Nó 10 — Tech Error (_qIDupF6BEfGBBLgT-R5iuw) ────────────────────
        // entrouPor=regresso — alcançado por aresta explícita.
        // Ramo "Yes": ISTECHERROR == "Y"  → caminho de erro técnico.
        // Ramo "No" (OTHERWISE): ISTECHERROR != "Y" → App Error.
        if (BscenvpcSeg049Steps.IsTechError(ctx))
        {
            return BscenvpcSeg049Outcome.TechError;
        }

        // ── Nó 11 — App Error (_qIDuo16BEfGBBLgT-R5iuw) ─────────────────────
        // Ramo "Yes": ISAPPERROR == "Y"  → caminho de erro de aplicação.
        // Ramo "No" (OTHERWISE): ISAPPERROR != "Y" → Done - Success.
        if (BscenvpcSeg049Steps.IsAppError(ctx))
        {
            return BscenvpcSeg049Outcome.AppError;
        }

        // ── Nó 12 — Done - Success (_qIDuol6BEfGBBLgT-R5iuw) ────────────────
        // Terminal da jornada: ISTECHERROR="N" e ISAPPERROR="N" após a saída do ActivitySet.
        return BscenvpcSeg049Outcome.DoneSuccess;
    }
}
