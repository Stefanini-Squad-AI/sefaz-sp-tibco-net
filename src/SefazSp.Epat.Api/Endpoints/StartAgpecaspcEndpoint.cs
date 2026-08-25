#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Agpecaspc;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only: starts an AGPECASPC instance. It suspends at 'Aguardar Interposições' with an
/// event⇄timer race. Deliver the event via POST /agpecaspc/{processId}/interposicoes, or let the
/// demo timer fire first — whichever wins resolves the wait.
/// </summary>
public static class StartAgpecaspcEndpoint
{
    public static IEndpointRouteBuilder MapStartAgpecaspc(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/agpecaspc/start", Handle)
              .WithName("Debug-AGPECASPC-Start")
              .WithTags("Debug")
              .WithSummary("Starts AGPECASPC (suspends on Aguardar Interposições — event⇄timer race)");
        return routes;
    }

    private static async Task<IResult> Handle(
        StartAgpecaspcRequest request,
        IWorkflowRuntime workflowRuntime,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");

        var client = await workflowRuntime.CreateClientAsync(ct);
        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(AgpecaspcElsaWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
                Input = new Dictionary<string, object>
                {
                    ["ProcessId"] = request.ProcessId,
                    ["IdAiim"] = request.IdAiim,
                    ["DemoTimerSeconds"] = request.DemoTimerSeconds <= 0 ? 3 : request.DemoTimerSeconds,
                },
            },
            ct);

        return Results.Ok(new
        {
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Hint = $"POST /agpecaspc/{request.ProcessId}/interposicoes to win with the event, or wait {(request.DemoTimerSeconds <= 0 ? 3 : request.DemoTimerSeconds)}s for the timer."
        });
    }
}

public sealed record StartAgpecaspcRequest(string ProcessId, long IdAiim, int DemoTimerSeconds);
