#nullable enable

using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Passos de script do segmento 048 do processo BSCENVPC (card BUILD-BSCENVPC-seg048).
/// Contem apenas logica de envelope tecnico — STATUS_CODE, contadores de retentativa.
/// Regras puras de dominio residem em <see cref="BscenvpcSetParametersRule"/> e
/// <see cref="BscenvpcCheckRetriesRule"/> (Domain/Rules).
///
/// Passos cobertos:
///   [2]  scriptTask  SetParameters      _qIDulV6BEfGBBLgT-R5iuw  (RI-script-BSCENVPC-SetParameters)
///   [3]  scriptTask  Start Loop         _qIDul16BEfGBBLgT-R5iuw
///   [6]  scriptTask  Start TX           _qIDu3F6BEfGBBLgT-R5iuw  (dentro do ActivitySet)
///   [8]  scriptTask  Set Technical Error _qIDu4F6BEfGBBLgT-R5iuw (dentro do ActivitySet)
/// </summary>
public static class BscenvpcSeg048Steps
{
    /// <summary>
    /// Passo: scriptTask SetParameters (_qIDulV6BEfGBBLgT-R5iuw).
    /// Regra de dominio: RI-script-BSCENVPC-SetParameters.
    ///
    /// Expressao legada: IDPROCESSO != IPESystemValues.SW_NA | MAXRETRIES==null
    /// Se a condicao for verdadeira: inicializa MAXRETRIES (default 5) e PROCESS_ID.
    ///
    /// IDPROCESSO usa o sentinela SW_NA — representado como <see cref="FieldValue{T}"/>
    /// conforme decisao NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06).
    /// Em ProcessExecutionContext, MAXRETRIES == 0 indica "ainda nao inicializado".
    /// </summary>
    /// <param name="ctx">Contexto de execucao mutavel.</param>
    /// <param name="idProcesso">
    ///   Campo IDPROCESSO do caso, com sentinela SW_NA preservado via FieldValue.
    /// </param>
    public static void ApplySetParameters(ProcessExecutionContext ctx, FieldValue<long> idProcesso)
    {
        // MAXRETRIES == 0 e o equivalente C# de MAXRETRIES==null do iProcess (int nao pode ser nulo).
        int? currentMaxRetries = ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES;

        if (BscenvpcSetParametersRule.ShouldInitialize(idProcesso, currentMaxRetries))
        {
            ctx.MAXRETRIES = BscenvpcSetParametersRule.ResolveMaxRetries(currentMaxRetries);

            // PROCESS_ID e derivado de IDPROCESSO quando o campo esta preenchido.
            ctx.PROCESS_ID = idProcesso.Match(
                hasValue:     v  => v.ToString(),
                notAvailable: () => ctx.PROCESS_ID,
                empty:        () => ctx.PROCESS_ID);
        }
    }

    /// <summary>
    /// Passo: scriptTask Start Loop (_qIDul16BEfGBBLgT-R5iuw).
    /// Inicializa o contador de retentativas de aplicacao para o inicio de cada ciclo.
    /// </summary>
    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        ctx.NUMAPPRETRIES = 0;
    }

    /// <summary>
    /// Passo: scriptTask Start TX (_qIDu3F6BEfGBBLgT-R5iuw) — dentro do ActivitySet.
    /// Marca o inicio da transaccao tecnica antes da chamada de servico.
    /// Em .NET nao ha transaccao distribuida equivalente ao contexto TIBCO;
    /// o passo e preservado por fidelidade ao mapa XPDL.
    /// </summary>
    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        // No-op no modelo .NET: sem contexto transaccional equivalente ao TIBCO BusinessWorks.
        _ = ctx;
    }

    /// <summary>
    /// Passo: scriptTask Set Technical Error (_qIDu4F6BEfGBBLgT-R5iuw) — dentro do ActivitySet.
    /// Activado pelo ramo OTHERWISE do gateway Check Retries SW_QRETRYCOUNT
    /// quando as tentativas do motor (SW_QRETRYCOUNT) estao esgotadas.
    /// Assinala ISTECHERROR para sinalizar falha tecnica ao escopo pai.
    /// </summary>
    public static void ApplySetTechnicalError(ProcessExecutionContext ctx)
    {
        ctx.ISTECHERROR = "Y";
    }
}
