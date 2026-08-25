#nullable enable

using Elsa.Workflows;
using Elsa.Workflows.Activities;
using ElsaParallel = Elsa.Workflows.Activities.Parallel;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Seg006Parallel;

/// <summary>
/// Definição Elsa do segmento 006 do POC_EpatProcess: userTask 'Finalizar AIIM' seguido de um
/// gateway Parallel (AND-split). O <see cref="Parallel"/> dispara os dois ramos em simultâneo e
/// só conclui quando ambos terminam (semântica WaitAll = AND-split + AND-join implícito).
/// </summary>
public class Seg006ParallelElsaWorkflow : WorkflowBase
{
    public const string DefinitionId = "SEG006-PARALLEL";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Root = new Sequence
        {
            Activities =
            {
                // userTask 'Finalizar AIIM' — suspende até submissão.
                new FinalizarAiimActivity(),

                // gateway _Faq_RFqTEfG5K7mY0I3I6w (Parallel / AND-split): ramos A e B em paralelo.
                new ElsaParallel
                {
                    Activities =
                    {
                        new BranchAExisteNotificacaoActivity(),
                        new BranchBSetNomeEtapa2Activity(),
                    },
                },
            },
        };
    }
}
