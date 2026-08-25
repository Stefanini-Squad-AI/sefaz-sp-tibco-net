#nullable enable

using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Runtime.Activities;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa;

/// <summary>
/// Code-first Elsa translation of the BSCENVPC prologue topology.
/// Demonstrates the full long-running loop: run prologue → suspend on an external
/// event (bookmark-correlation) → resume via the endpoint → complete.
///
/// The wait point is an <see cref="Event"/> keyed by <see cref="ExternalEventName"/>;
/// correlation is by the workflow's CorrelationId = PROCESS_ID.
/// </summary>
public class BscenvpcElsaWorkflow : WorkflowBase
{
    /// <summary>Stable definition id so the host can start it by name.</summary>
    public const string DefinitionId = "Bscenvpc";

    /// <summary>Event name the resume endpoint publishes to release the bookmark.</summary>
    public const string ExternalEventName = "epat-external-event";

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);

        builder.Root = new Sequence
        {
            Activities =
            {
                new BscenvpcPrologueActivity
                {
                    ProcessId = new("idAiim-0idProc-0"),
                    SwQRetryCount = new(0L),
                },
                new WriteLine("[BSCENVPC] suspending — waiting for external event…"),
                new Event(ExternalEventName),
                new WriteLine("[BSCENVPC] resumed — workflow completed."),
            }
        };
    }
}
