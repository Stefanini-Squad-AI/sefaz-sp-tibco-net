#nullable enable

namespace SefazSp.Epat.Application.Execution;

public static class SetAppErrorScript
{
    public static void Execute(ProcessExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.ISAPPERROR = "Y";
    }
}
