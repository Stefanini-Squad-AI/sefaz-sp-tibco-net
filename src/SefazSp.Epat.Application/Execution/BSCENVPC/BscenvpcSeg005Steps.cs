#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Passos de script do segmento 005 do processo BSCENVPC (passos 8–18 do cenário SC-BSCENVPC-007).
/// Contem apenas logica de envelope tecnico — STATUS_CODE, contadores de retentativa.
/// Regras de negocio residem em Domain/Rules, nao aqui.
/// </summary>
public static class BscenvpcSeg005Steps
{
    /// <summary>
    /// Passo: Set App Error (_qIDu4V6BEfGBBLgT-R5iuw, scriptTask).
    /// Escreve o codigo de erro aplicacional no envelope tecnico e incrementa o contador de retentativas.
    /// Chamado quando STATUS_CODE != "0" apos a chamada a Busca Envolvidos Vista Por AIIM.
    /// </summary>
    /// <param name="ctx">Contexto de execucao mutavel do subprocesso de servico.</param>
    /// <param name="envelope">Envelope tecnico devolvido pela chamada de servico.</param>
    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE   = envelope.STATUS_CODE;
        ctx.STERRORCODE   = envelope.STERRORCODE;
        ctx.STERRORDESC   = envelope.STERRORDESC;
        ctx.ISAPPERROR    = "Y";
        ctx.ISTECHERROR   = "N";
        ctx.NUMAPPRETRIES = ctx.NUMAPPRETRIES + 1;
    }

    /// <summary>
    /// Mapeamento do envelope de servico para o contexto de execucao (passo pos-chamada).
    /// Chamado quando o servico retorna com sucesso (STATUS_CODE == "0").
    /// </summary>
    public static void MapServiceEnvelopeToContext(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE  = envelope.STATUS_CODE;
        ctx.STERRORCODE  = envelope.STERRORCODE;
        ctx.STERRORDESC  = envelope.STERRORDESC;
        ctx.ISAPPERROR   = "N";
        ctx.ISTECHERROR  = "N";
    }

    // ── Condicoes de gateway (topologia como dado) ────────────────────────────

    /// <summary>
    /// Gateway _qIDu4l6BEfGBBLgT-R5iuw.
    /// "A chamada a Busca Envolvidos Vista Por AIIM foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0".
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.STATUS_CODE != "0";

    /// <summary>
    /// Gateway App Error (_qIDuo16BEfGBBLgT-R5iuw).
    /// Ramo Yes: ISAPPERROR == "Y".
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_qIDuoF6BEfGBBLgT-R5iuw).
    /// Ramo Yes: NUMAPPRETRIES &lt; MAXRETRIES.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;
}
