#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.CRNOTPC;

/// <summary>
/// Passos de script do segmento 017 do processo CRNOTPC (passos 8–19 do cenário SC-CRNOTPC-008).
/// Contém apenas lógica de envelope técnico — STATUS_CODE, contadores de retentativa.
/// Regras de negócio residem em Domain/Rules, não aqui.
/// </summary>
public static class CrnotpcSeg017Steps
{
    /// <summary>
    /// Passo: Set App Error (_NcJxLV9KEfGqPfX31TKC3w, scriptTask).
    /// Escreve o código de erro aplicacional no envelope técnico e incrementa o contador de retentativas.
    /// Chamado quando STATUS_CODE != "0" após a chamada a CriaNotificacao.
    /// </summary>
    /// <param name="ctx">Contexto de execução mutável do subprocesso de serviço.</param>
    /// <param name="envelope">Envelope técnico devolvido pela chamada de serviço.</param>
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
    /// Mapeamento do envelope de serviço para o contexto de execução (passo pós-chamada).
    /// Chamado quando o serviço retorna com sucesso (STATUS_CODE == "0").
    /// </summary>
    public static void MapServiceEnvelopeToContext(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
        ctx.ISAPPERROR  = "N";
        ctx.ISTECHERROR = "N";
    }

    // ── Condições de gateway (topologia como dado) ─────────────────────────────

    /// <summary>
    /// Gateway _NcJxLl9KEfGqPfX31TKC3w.
    /// "A chamada a CriaNotificacao foi bem sucedida?"
    /// Ramo AppError: STATUS_CODE != "0".
    /// </summary>
    public static bool IsCallFailed(ProcessExecutionContext ctx)
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
    /// Ramo No (OTHERWISE): vai para Manipular Excecao.
    /// </summary>
    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    /// <summary>
    /// Gateway Manually Fixed (_NcJJ419KEfGqPfX31TKC3w).
    /// Ramo Yes: OUTCOME == "OK".
    /// </summary>
    public static bool IsManuallyFixed(ProcessExecutionContext ctx)
        => ctx.OUTCOME == "OK";
}
