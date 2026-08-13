#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Execution.CALCPRPC;

/// <summary>
/// Passos de script do segmento 029 do processo CALCPRPC.
/// Contem apenas logica de envelope tecnico — STATUS_CODE, flags e contadores de retentativa.
/// Regras de negocio residem em Domain/Rules, nao aqui.
/// </summary>
public static class CalcprpcExecutionSteps
{
    public static void ApplySetParameters(ProcessExecutionContext ctx)
    {
        if (ctx.MAXRETRIES == 0)
            ctx.MAXRETRIES = CalcprpcSetParametersRule.DefaultMaxRetries;

        if (ctx.OUTCOME is null)
            ctx.OUTCOME = CalcprpcSetParametersRule.DefaultOutcome;
    }

    public static void ApplyStartLoop(ProcessExecutionContext ctx)
    {
        // NUMAPPRETRIES já nasce em 0 no contexto .NET.
    }

    public static void ApplyStartTx(ProcessExecutionContext ctx)
    {
        ctx.STATUS_CODE = null;
        ctx.ISAPPERROR = "N";
        ctx.ISTECHERROR = "N";
    }

    public static void SetAppError(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        MapServiceEnvelope(ctx, envelope);
        ctx.ISAPPERROR = "Y";
        ctx.ISTECHERROR = "N";
        ctx.NUMAPPRETRIES = ctx.NUMAPPRETRIES + 1;
    }

    public static void SetTechError(ProcessExecutionContext ctx, string message)
    {
        ctx.ISTECHERROR = "Y";
        ctx.STATUS_CODE = message;
    }

    public static void MapServiceEnvelope(ProcessExecutionContext ctx, ServiceEnvelope envelope)
    {
        ctx.STATUS_CODE = envelope.STATUS_CODE;
        ctx.STERRORCODE = envelope.STERRORCODE;
        ctx.STERRORDESC = envelope.STERRORDESC;
    }

    public static bool IsAppError(ProcessExecutionContext ctx) =>
        ctx.STATUS_CODE != "0";

    public static bool HasMoreRetries(ProcessExecutionContext ctx) =>
        ctx.NUMAPPRETRIES < ctx.MAXRETRIES;

    public static bool IsAppErrorFlag(ProcessExecutionContext ctx) =>
        ctx.ISAPPERROR == "Y";
}
