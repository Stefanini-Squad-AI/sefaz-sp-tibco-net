#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Passos de envelope técnico do processo BSCENVPC — segmento 047
/// (passos 1–17 do cenário SC-BSCENVPC-015: de 'Start Event' a 'Done - Bail').
///
/// Contém apenas lógica que toca STATUS_CODE, ISAPPERROR, ISTECHERROR e
/// contadores de retentativa — nunca cálculo ou decisão de domínio.
/// A separação segue <c>classification.eRegraDeNegocio</c> do rule-catalogue.json.
///
/// Invariantes (glossário POC_Epat.yaml, confirmados 2026-08-06):
///   STATUS_CODE   : '0' = sucesso; != '0' = erro.
///   ISAPPERROR    : 'N' = sem erro de aplicação; 'Y' = erro de aplicação.
///   ISTECHERROR   : 'N' = sem erro técnico;      'Y' = erro técnico.
///   MAXRETRIES    : 5 por omissão.
///   NUMAPPRETRIES : começa em 0; incrementa a cada falha de aplicação.
///   SW_QRETRYCOUNT: lido, nunca escrito; fornecido pelo runtime iProcess.
///
/// Bloqueador NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06):
///   SW_QRETRYCOUNT é avaliado via <see cref="SefazSp.Epat.Domain.Rules.BscenvpcCheckRetriesRule"/>.
///
/// Card: BUILD-BSCENVPC-seg047
/// </summary>
public static class BscenvpcSeg047Steps
{
    // ── Nó 8: Set Technical Error (_qIDu4F6BEfGBBLgT-R5iuw, scriptTask, ActivitySet) ──

    /// <summary>
    /// Passo 'Set Technical Error' — envelope técnico (escopo ActivitySet).
    /// Alcançado quando SW_QRETRYCOUNT &gt;= MAXRETRIES (gateway Check Retries, ramo Maxretriesexceeded).
    ///
    /// Define ISTECHERROR = 'Y' e ISAPPERROR = 'Y'.
    /// A marcação simultânea de ISAPPERROR permite que o gateway 'App Error'
    /// (_qIDuo16BEfGBBLgT-R5iuw) roteie o caso para tratamento manual via
    /// 'More Retries' → 'Manipular Excecao', conforme SC-BSCENVPC-015 decisions.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do processo.</param>
    /// <param name="reason">Mensagem diagnóstica (nunca nula no registo).</param>
    public static void SetTechnicalError(ProcessExecutionContext ctx, string reason)
    {
        ctx.ISTECHERROR  = "Y";
        ctx.ISAPPERROR   = "Y";
        ctx.STERRORDESC ??= reason;
    }

    // ── Condições de gateway ──────────────────────────────────────────────────

    /// <summary>
    /// Gateway 'App Error' (_qIDuo16BEfGBBLgT-R5iuw).
    /// Ramo Yes (CONDITION): ISAPPERROR == 'Y' → More Retries.
    /// Ramo No  (OTHERWISE): sem erro de aplicação → encerra sem retentativa manual.
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway 'More Retries' (_qIDuoF6BEfGBBLgT-R5iuw).
    /// Ramo Yes: NUMAPPRETRIES &lt; MAXRETRIES → ainda há retentativas de aplicação.
    /// Ramo No  (OTHERWISE): retentativas esgotadas → tratamento manual.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    /// <summary>
    /// Gateway 'Manually Fixed' (_qIDull6BEfGBBLgT-R5iuw).
    /// Ramo Yes (CONDITION): OUTCOME == 'OK' → operador resolveu o caso manualmente.
    /// Ramo No  (OTHERWISE): operador não resolveu → Try Again.
    /// </summary>
    public static bool IsManuallyFixed(ProcessExecutionContext ctx)
        => ctx.OUTCOME == "OK";

    /// <summary>
    /// Gateway 'Try Again' (_qIDum16BEfGBBLgT-R5iuw).
    /// Ramo Yes (CONDITION): OUTCOME == 'R' → operador quer repetir o ciclo.
    /// Ramo No  (OTHERWISE): operador opta por encerrar → Done - Bail.
    /// </summary>
    public static bool IsTryAgain(ProcessExecutionContext ctx)
        => ctx.OUTCOME == "R";
}
