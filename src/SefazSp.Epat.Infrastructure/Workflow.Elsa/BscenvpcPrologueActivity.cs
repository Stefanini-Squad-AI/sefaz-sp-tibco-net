#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa;

/// <summary>
/// Elsa activity that runs the BSCENVPC prologue by delegating to the existing
/// application execution steps and domain rule. The topology (Application/Workflows)
/// stays engine-agnostic; this adapter is the only place that knows about Elsa.
/// </summary>
[Activity("Epat", "BSCENVPC", "Runs the BSCENVPC prologue (SetParameters, StartLoop, StartTx, CheckRetries).")]
public class BscenvpcPrologueActivity : CodeActivity
{
    [Input(Description = "PROCESS_ID correlation key (idAiim-<n>idProc-<n>).")]
    public Input<string?> ProcessId { get; set; } = default!;

    [Input(Description = "Engine retry counter (SW_QRETRYCOUNT).")]
    public Input<long> SwQRetryCount { get; set; } = new(0L);

    protected override void Execute(ActivityExecutionContext context)
    {
        var processId = context.Get(ProcessId);
        var swQRetryCount = context.Get(SwQRetryCount);

        var ctx = new ProcessExecutionContext();

        // Delegate to existing application/domain code — no logic duplicated here.
        BscenvpcExecutionSteps.ApplySetParameters(ctx, processId);
        BscenvpcExecutionSteps.ApplyStartLoop(ctx);
        BscenvpcExecutionSteps.ApplyStartTx(ctx);

        var branch = BscenvpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES)
            ? "Stillgood"
            : "Maxed";

        Console.WriteLine(
            $"[BSCENVPC] prologue done. PROCESS_ID={ctx.PROCESS_ID}, " +
            $"MAXRETRIES={ctx.MAXRETRIES}, SW_QRETRYCOUNT={swQRetryCount}, branch={branch}");
    }
}
