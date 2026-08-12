#nullable enable

namespace SefazSp.Epat.Application.Execution.ATZINTPC;

/// <summary>
/// Envelope técnico do passo Start Loop (ATZINTPC _RNdJzF6PEfGBBLgT-R5iuw).
/// </summary>
public static class StartLoopStep
{
    public static void Apply(ProcessExecutionContext ctx, bool isFirstIteration)
    {
        ctx.NUMAPPRETRIES = isFirstIteration ? 0 : ctx.NUMAPPRETRIES + 1;
        ctx.ISAPPERROR = "N";
        ctx.ISTECHERROR = "N";
        ctx.OUTCOME = "R";
    }
}
