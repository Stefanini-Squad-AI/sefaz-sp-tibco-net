#nullable enable

using Elsa.Workflows;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Agpecaspc;

/// <summary>Definição Elsa do subprocesso AGPECASPC (evento externo + timer de fronteira em corrida).</summary>
public class AgpecaspcElsaWorkflow : WorkflowBase
{
    public const string DefinitionId = "AGPECASPC";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Root = new AgpecaspcElsaActivity();
    }
}
