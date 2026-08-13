#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.CRNOTPC;

/// <summary>
/// Passos de script do segmento 019 do processo CRNOTPC (passos 8–18 do cenário SC-CRNOTPC-007).
/// Contem apenas logica de envelope tecnico — STATUS_CODE, contadores de retentativa.
/// Regras de negocio residem em Domain/Rules, nao aqui.
/// </summary>
public static class CrnotpcSeg019Steps
{
    /// <summary>
    /// Passo: Set App Error (_NcJxLV9KEfGqPfX31TKC3w, scriptTask).
    /// Escreve o codigo de erro aplicacional no envelope tecnico e incrementa o contador de retentativas.
    /// Chamado quando STATUS_CODE != "0" apos a chamada a CriaNotificacao.
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
    /// Gateway _NcJxLl9KEfGqPfX31TKC3w.
    /// "A chamada a CriaNotificacao foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0".
    /// </summary>
    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.STATUS_CODE != "0";

    /// <summary>
    /// Gateway App Error (_NcJJ8F9KEfGqPfX31TKC3w).
    /// Ramo Yes: ISAPPERROR == "Y".
    /// </summary>
    public static bool IsAppErrorFlag(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";

    /// <summary>
    /// Gateway More Retries (_NcJJ7V9KEfGqPfX31TKC3w).
    /// Ramo Yes: NUMAPPRETRIES &lt; MAXRETRIES.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;
}
