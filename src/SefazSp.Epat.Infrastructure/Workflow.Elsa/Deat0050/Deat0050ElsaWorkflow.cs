#nullable enable

using Elsa.Workflows;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Deat0050;

/// <summary>Definição Elsa do subprocesso DEAT0050 (INICALC + Aguarda Defesa).</summary>
public class Deat0050ElsaWorkflow : WorkflowBase
{
    public const string DefinitionId = "DEAT0050";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Root = new Deat0050ElsaActivity();
    }
}
