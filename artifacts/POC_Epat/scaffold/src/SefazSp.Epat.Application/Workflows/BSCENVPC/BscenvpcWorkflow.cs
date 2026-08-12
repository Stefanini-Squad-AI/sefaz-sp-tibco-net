#nullable enable

using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.BSCENVPC;
using SefazSp.Epat.Domain.Rules.BSCENVPC;

namespace SefazSp.Epat.Application.Workflows.BSCENVPC;

/// <summary>
/// Workflow do processo BSCENVPC — segmento SC-BSCENVPC-016, de 'Start Event' a 'Done - Success'.
///
/// Topologia do troco (12 passos, ordemNaJornada=1):
///   [MAIN]
///   1.  _qIDulF6BEfGBBLgT-R5iuw  Start Event          startEvent
///   2.  _qIDulV6BEfGBBLgT-R5iuw  SetParameters        scriptTask
///   3.  _qIDul16BEfGBBLgT-R5iuw  Start Loop           scriptTask
///   4.  _qIDupV6BEfGBBLgT-R5iuw  Control System Task  subProcessScope (callActivity)
///
///   [ActivitySet — descida explicita; nao existe transicao XPDL]
///   5.  _qIDu3l6BEfGBBLgT-R5iuw  startEvent           startEvent   (entrouPor=descida)
///   6.  _qIDu3F6BEfGBBLgT-R5iuw  Start TX             scriptTask
///   7.  _qIDu3V6BEfGBBLgT-R5iuw  Check Retries        gateway
///   8.  _qIDu4F6BEfGBBLgT-R5iuw  Set Technical Error  scriptTask   (ramo Maxretriesexceeded)
///   9.  _qIDu316BEfGBBLgT-R5iuw  endEvent             endEvent
///
///   [MAIN — regresso explicito; nao existe transicao XPDL]
///   10. _qIDupF6BEfGBBLgT-R5iuw  Tech Error           gateway      (entrouPor=regresso)
///   11. _qIDuo16BEfGBBLgT-R5iuw  App Error            gateway
///   12. _qIDuol6BEfGBBLgT-R5iuw  Done - Success       endEvent
///
/// Notas de implementacao:
///   • O ActivitySet corre num contexto LOCAL (copia do pai). As alteracoes feitas dentro
///     do sub-processo (ex: ISTECHERROR='Y' pelo Set Technical Error) NAO sao propagadas
///     de volta para o contexto pai — comportamento observado no legado iProcess onde o
///     ActivitySet funciona como escopo fechado neste troco.
///   • A transicao de DESCIDA (passo 4->5) e a transicao de REGRESSO (passo 9->10) sao
///     escritas explicitamente porque nao existem no XPDL como transicoes declaradas.
///   • O ramo 'Stillgood' de Check Retries (_qIDu3V6BEfGBBLgT-R5iuw) conduz ao servico
///     BSCENVPC — esse troco e coberto por outros segmentos; este metodo lanca
///     <see cref="NotImplementedException"/> se activado neste contexto.
///   • O ramo 'Yes' de Tech Error e App Error conduzem a Convergencia/Manipular Excecao —
///     cobertos por outros segmentos; este metodo lanca <see cref="NotImplementedException"/>
///     se activados neste contexto.
///
/// GAP NOEQ-iprocess-builtin (gate humano necessario — BUILTIN-SEMANTICS):
///   Dois nos deste segmento usam builtins iProcess sem equivalente .NET confirmado:
///   - Passo 2 (SetParameters): construcao de PROCESS_ID via IPESystemValues.SW_NA e
///     IPEConversionUtil.STR — portagem suspensa.
///   - Passo 3 (Start Loop): DATETIME via IPEConversionUtil.DATESTR(SW_DATE) — portagem suspensa.
/// </summary>
public sealed class BscenvpcWorkflow
{
    private const string NodeStartEvent          = "_qIDulF6BEfGBBLgT-R5iuw";
    private const string NodeSetParameters       = "_qIDulV6BEfGBBLgT-R5iuw";
    private const string NodeStartLoop           = "_qIDul16BEfGBBLgT-R5iuw";
    private const string NodeControlSystemTask   = "_qIDupV6BEfGBBLgT-R5iuw";
    private const string NodeInnerStartEvent     = "_qIDu3l6BEfGBBLgT-R5iuw";
    private const string NodeStartTX             = "_qIDu3F6BEfGBBLgT-R5iuw";
    private const string NodeCheckRetries        = "_qIDu3V6BEfGBBLgT-R5iuw";
    private const string NodeSetTechnicalError   = "_qIDu4F6BEfGBBLgT-R5iuw";
    private const string NodeInnerEndEvent       = "_qIDu316BEfGBBLgT-R5iuw";
    private const string NodeTechError           = "_qIDupF6BEfGBBLgT-R5iuw";
    private const string NodeAppError            = "_qIDuo16BEfGBBLgT-R5iuw";
    private const string NodeDoneSuccess         = "_qIDuol6BEfGBBLgT-R5iuw";

    /// <summary>
    /// Executa o troco de 'Start Event' a 'Done - Success' e devolve os identificadores
    /// dos nos visitados, por ordem de visita.
    /// </summary>
    /// <param name="ctx">Contexto de execucao do processo pai (MAIN).</param>
    /// <param name="swQRetryCount">
    ///   Valor de runtime SW_QRETRYCOUNT fornecido pelo motor (contagem de retentativas
    ///   de entrega na fila iProcess). Em .NET e passado explicitamente.
    /// </param>
    /// <returns>Sequencia de node IDs visitados.</returns>
    public IReadOnlyList<string> Execute(ProcessExecutionContext ctx, long swQRetryCount)
    {
        var path = new List<string>(capacity: 12);

        // ── Passo 1: Start Event ────────────────────────────────────────────────────
        path.Add(NodeStartEvent);

        // ── Passo 2: SetParameters ──────────────────────────────────────────────────
        // GAP NOEQ-iprocess-builtin: PROCESS_ID nao e construido (builtin SW_NA/STR).
        path.Add(NodeSetParameters);
        ctx.MAXRETRIES = SetParametersRule.Apply(ctx.MAXRETRIES);

        // ── Passo 3: Start Loop ─────────────────────────────────────────────────────
        // GAP NOEQ-iprocess-builtin: DATETIME nao e preenchido (builtin DATESTR/SW_DATE).
        path.Add(NodeStartLoop);
        StartLoopExecution.Apply(ctx);

        // ── Passo 4: Control System Task Call (subProcessScope) ─────────────────────
        // Transicao de DESCIDA (explicit): o callActivity desce para o ActivitySet.
        path.Add(NodeControlSystemTask);

        // ── Execucao do ActivitySet (escopo local — alteracoes nao propagadas ao pai) ─
        var innerPath = ExecuteActivitySet(ctx, swQRetryCount);
        path.AddRange(innerPath);

        // ── Passo 10: Tech Error (regresso explicito do ActivitySet) ────────────────
        // Transicao de REGRESSO (explicit): o ActivitySet terminou; o fluxo regressa
        // ao passo seguinte no MAIN. O contexto pai (ctx) NAO foi alterado pelo
        // ActivitySet — ISTECHERROR/ISAPPERROR reflectem o estado apos Start Loop ('N').
        path.Add(NodeTechError);
        if (ctx.ISTECHERROR == "Y")
        {
            // Ramo 'Yes': ISTECHERROR=='Y' → Convergencia → Manipular Excecao
            // (coberto por outros segmentos — fora deste troco)
            throw new NotImplementedException(
                "O ramo 'Yes' de Tech Error (Convergencia/Manipular Excecao) " +
                "pertence a outro segmento e nao e implementado neste troco.");
        }

        // Ramo 'No' (OTHERWISE — default): ISTECHERROR != 'Y' → App Error

        // ── Passo 11: App Error ─────────────────────────────────────────────────────
        path.Add(NodeAppError);
        if (ctx.ISAPPERROR == "Y")
        {
            // Ramo 'Yes': ISAPPERROR=='Y' → More Retries
            // (coberto por outros segmentos — fora deste troco)
            throw new NotImplementedException(
                "O ramo 'Yes' de App Error (More Retries) " +
                "pertence a outro segmento e nao e implementado neste troco.");
        }

        // Ramo 'No' (OTHERWISE — default): ISAPPERROR != 'Y' → Done - Success

        // ── Passo 12: Done - Success ────────────────────────────────────────────────
        path.Add(NodeDoneSuccess);

        return path;
    }

    /// <summary>
    /// Executa o ActivitySet (sub-processo embutido 'Control System Task Call').
    /// O contexto do ActivitySet e uma copia local — alteracoes nao afectam o pai.
    /// </summary>
    private static IReadOnlyList<string> ExecuteActivitySet(
        ProcessExecutionContext parentCtx,
        long swQRetryCount)
    {
        // Copia local: o ActivitySet opera em escopo fechado.
        var local = new ProcessExecutionContext
        {
            MAXRETRIES     = parentCtx.MAXRETRIES,
            NUMAPPRETRIES  = parentCtx.NUMAPPRETRIES,
            ISAPPERROR     = parentCtx.ISAPPERROR,
            ISTECHERROR    = parentCtx.ISTECHERROR,
            OUTCOME        = parentCtx.OUTCOME,
            STATUS_CODE    = parentCtx.STATUS_CODE,
            STERRORCODE    = parentCtx.STERRORCODE,
            STERRORDESC    = parentCtx.STERRORDESC,
        };

        var innerPath = new List<string>(capacity: 5);

        // ── Passo 5: startEvent (descida explicita) ──────────────────────────────────
        innerPath.Add(NodeInnerStartEvent);

        // ── Passo 6: Start TX (scriptTask) — script original: 1=1; (no-op) ──────────
        innerPath.Add(NodeStartTX);

        // ── Passo 7: Check Retries SW_QRETRYCOUNT (gateway) ─────────────────────────
        innerPath.Add(NodeCheckRetries);
        bool stillGood = CheckRetriesSWQRETRYCOUNTRule.Evaluate(swQRetryCount, local.MAXRETRIES);

        if (stillGood)
        {
            // Ramo 'Stillgood': SW_QRETRYCOUNT < MAXRETRIES → chamada de servico
            // (coberto por outros segmentos — fora deste troco)
            throw new NotImplementedException(
                "O ramo 'Stillgood' de Check Retries (chamada de servico BSCENVPC) " +
                "pertence a outro segmento e nao e implementado neste troco.");
        }

        // Ramo 'Maxretriesexceeded' (OTHERWISE): SW_QRETRYCOUNT >= MAXRETRIES

        // ── Passo 8: Set Technical Error ─────────────────────────────────────────────
        innerPath.Add(NodeSetTechnicalError);
        SetTechnicalErrorExecution.Apply(local);

        // ── Passo 9: endEvent (fim do ActivitySet) ───────────────────────────────────
        innerPath.Add(NodeInnerEndEvent);

        return innerPath;
    }
}
