#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution.BSCENVPC;

/// <summary>
/// Execucao do passo 'Set Technical Error' (_qIDu4F6BEfGBBLgT-R5iuw, scriptTask, ActivitySet).
/// Marca o envelope tecnico com erro de infraestrutura.
///
/// Script original (XPDL linha 5567): ISTECHERROR='Y';
/// </summary>
public static class SetTechnicalErrorExecution
{
    /// <summary>
    /// Aplica o script ao contexto local do ActivitySet.
    /// </summary>
    public static void Apply(ProcessExecutionContext ctx)
    {
        ctx.ISTECHERROR = "Y";
    }
}
