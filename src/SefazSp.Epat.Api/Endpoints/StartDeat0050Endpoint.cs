#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Deat0050;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only: starts a DEAT0050 instance. It suspends at INICALC (external event).
/// Resume via POST /deat0050/{processId}/inicalc; it then computes the prazo and suspends
/// on the Aguarda Defesa timer (demoDeadlineSeconds), which fires and completes the flow.
/// </summary>
public static class StartDeat0050Endpoint
{
    public static IEndpointRouteBuilder MapStartDeat0050(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/deat0050/start", Handle)
              .WithName("Debug-DEAT0050-Start")
              .WithTags("Debug")
              .WithSummary("Starts DEAT0050 (suspends at INICALC external event, then Aguarda Defesa timer)");
        return routes;
    }

    private static async Task<IResult> Handle(
        StartDeat0050Request request,
        IWorkflowRuntime workflowRuntime,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");

        var client = await workflowRuntime.CreateClientAsync(ct);
        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(Deat0050ElsaWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
                Input = new Dictionary<string, object>
                {
                    ["ProcessId"] = request.ProcessId,
                    ["IdAiim"] = request.IdAiim,
                    ["DemoDeadlineSeconds"] = request.DemoDeadlineSeconds <= 0 ? 3 : request.DemoDeadlineSeconds,
                },
            },
            ct);

        return Results.Ok(new
        {
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Hint = $"POST /deat0050/{request.ProcessId}/inicalc to deliver the external event."
        });
    }
}

public sealed record StartDeat0050Request(string ProcessId, long IdAiim, int DemoDeadlineSeconds);
