#nullable enable

using Elsa.Workflows;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;

/// <summary>Definição Elsa do fluxo principal POC_EpatProcess (Fase 1 — percurso SC-001).</summary>
public class PocEpatMainElsaWorkflow : WorkflowBase
{
    public const string DefinitionId = "POC-EPAT-MAIN";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Root = new PocEpatMainActivity();
    }
}
