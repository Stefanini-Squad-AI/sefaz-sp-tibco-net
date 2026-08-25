#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only endpoint: starts a real Elsa BSCENVPC workflow instance.
/// The instance runs the prologue, then suspends on the external-event bookmark,
/// correlated by PROCESS_ID. Resume it via POST /deat0050/resume with the same PROCESS_ID.
/// </summary>
public static class StartBscenvpcWorkflowEndpoint
{
    public static IEndpointRouteBuilder MapStartBscenvpcWorkflow(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/bscenvpc/start", Handle)
              .WithName("Debug-BSCENVPC-Start")
              .WithTags("Debug")
              .WithSummary("Starts a real Elsa BSCENVPC workflow instance that suspends on an external event");
        return routes;
    }

    private static async Task<IResult> Handle(
        StartBscenvpcRequest request,
        IWorkflowRuntime workflowRuntime,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");

        var client = await workflowRuntime.CreateClientAsync(ct);

        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(BscenvpcElsaWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
            },
            ct);

        return Results.Ok(new
        {
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Hint = "Now POST /deat0050/resume with the same processId to resume."
        });
    }
}

public sealed record StartBscenvpcRequest(string ProcessId);
