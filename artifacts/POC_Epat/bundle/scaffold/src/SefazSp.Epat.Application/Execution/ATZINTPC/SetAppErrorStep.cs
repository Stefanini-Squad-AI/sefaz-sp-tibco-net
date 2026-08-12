#nullable enable

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

/// <summary>
/// Envelope técnico do passo Set App Error (ATZINTPC _RNdKGV6PEfGBBLgT-R5iuw).
/// </summary>
public static class SetAppErrorStep
{
    public static void Apply(ProcessExecutionContext ctx)
    {
        ctx.ISAPPERROR = "Y";
        ctx.OUTCOME = "R";
    }
}
