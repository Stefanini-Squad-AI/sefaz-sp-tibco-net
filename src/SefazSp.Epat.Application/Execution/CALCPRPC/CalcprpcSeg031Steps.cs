#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Execution.CALCPRPC;

/// <summary>
/// Passos de script do segmento 031 do processo CALCPRPC (Start Event → Done - Success).
/// Contém apenas lógica de envelope técnico — STATUS_CODE, contadores de retentativa.
/// Regras de negócio residem em Domain/Rules, não aqui.
/// </summary>
public static class CalcprpcSeg031Steps
{
    public static void ApplySetParameters(ProcessExecutionContext ctx, string processId)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = CalcprpcSetParametersRule.DefaultMaxRetries;

        ctx.PROCESS_ID = processId;
        ctx.DATETIME ??= DateTimeOffset.UtcNow.ToString("O");
    }

    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        ctx.NUMAPPRETRIES = 0;
        ctx.ISAPPERROR = "N";
        ctx.ISTECHERROR = "N";
        ctx.OUTCOME = null;
        ctx.DATETIME ??= DateTimeOffset.UtcNow.ToString("O");
    }

    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE = null;
        ctx.STERRORCODE = null;
        ctx.STERRORDESC = null;
        ctx.ISAPPERROR = "N";
        ctx.ISTECHERROR = "N";
    }

    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
        ctx.ISAPPERROR = "Y";
        ctx.ISTECHERROR = "N";
        ctx.NUMAPPRETRIES = ctx.NUMAPPRETRIES + 1;
    }

    public static void MapServiceEnvelopeToContext(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
        ctx.ISAPPERROR = "N";
        ctx.ISTECHERROR = "N";
    }

    public static bool IsCallError(ProcessExecutionContext ctx)
        => ctx.STATUS_CODE != "0";

    public static bool HasMoreRetries(ProcessExecutionContext ctx)
        => ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    public static bool IsTechError(ProcessExecutionContext ctx)
        => ctx.ISTECHERROR == "Y";

    public static bool IsAppError(ProcessExecutionContext ctx)
        => ctx.ISAPPERROR == "Y";
}
