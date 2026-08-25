#nullable enable

using Elsa.Workflows;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Graft;

/// <summary>Definição Elsa do passo pai do graft-step (Etapa 2 — 'Aguardar Notificação do AIIM').</summary>
public class GraftParentElsaWorkflow : WorkflowBase
{
    public const string DefinitionId = "GRAFT-PARENT";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Root = new GraftParentActivity();
    }
}
