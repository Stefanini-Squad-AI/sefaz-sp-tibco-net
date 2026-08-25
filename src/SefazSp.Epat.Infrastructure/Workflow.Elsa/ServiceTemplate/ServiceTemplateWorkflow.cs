#nullable enable

using Elsa.Workflows;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.ServiceTemplate;

/// <summary>
/// Definição Elsa única do molde de serviço, parametrizada por ProcessKey (input).
/// Serve os 5 subprocessos: CALCPRPC, BSCENVPC, PRPINTPC, ATZINTPC, CRNOTPC.
/// </summary>
public class ServiceTemplateWorkflow : WorkflowBase
{
    public const string DefinitionId = "ServiceTemplate";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Root = new ServiceRetryActivity();
    }
}
