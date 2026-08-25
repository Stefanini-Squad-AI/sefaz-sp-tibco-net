#nullable enable

using Elsa.Workflows;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Controlopc;

/// <summary>Definição Elsa do segmento CONTROPC-seg039 (subprocesso dinâmico 'Aguardar Retorno').</summary>
public class ControlopcElsaWorkflow : WorkflowBase
{
    public const string DefinitionId = "CONTROPC";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Root = new ControlopcElsaActivity();
    }
}
