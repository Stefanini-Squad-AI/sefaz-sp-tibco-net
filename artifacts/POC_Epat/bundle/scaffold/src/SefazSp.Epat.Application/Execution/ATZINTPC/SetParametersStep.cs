#nullable enable
using SefazSp.Epat.Domain.Rules.ATZINTPC;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

/// <summary>
/// Envelope técnico do passo SetParameters (ATZINTPC _RNdJyl6PEfGBBLgT-R5iuw).
/// A lógica de domínio pura está em SetParametersRule.
/// </summary>
public static class SetParametersStep
{
    public static void Apply(ProcessExecutionContext ctx, long idAiim, FieldValue<int> idProcesso)
    {
        ctx.PROCESS_ID = SetParametersRule.ComputeProcessId(idAiim, idProcesso);
        ctx.MAXRETRIES = SetParametersRule.ComputeMaxRetries(ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES);
        ctx.NUMAPPRETRIES = 0;
        ctx.STATUS_CODE = null;
    }

    public static void Apply(ProcessExecutionContext ctx, long idAiim, FieldValue<long> idProcesso)
    {
        ctx.PROCESS_ID = SetParametersRule.ComputeProcessId(idAiim, idProcesso);
        ctx.MAXRETRIES = SetParametersRule.ComputeMaxRetries(ctx.MAXRETRIES == 0 ? null : ctx.MAXRETRIES);
        ctx.NUMAPPRETRIES = 0;
        ctx.STATUS_CODE = null;
    }
}
