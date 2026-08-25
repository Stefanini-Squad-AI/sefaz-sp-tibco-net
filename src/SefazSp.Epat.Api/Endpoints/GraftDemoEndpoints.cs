#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Runtime;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Graft;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only endpoints for the graft-step (correlation-join). The parent parks; children attach
/// and complete at different times; an explicit close valve shuts the window; the parent proceeds
/// when the window is closed and all attached children completed (or on the timeout safety net).
/// </summary>
public static class GraftDemoEndpoints
{
    public static IEndpointRouteBuilder MapGraftDemo(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/graft/start", StartHandle)
              .WithName("Debug-GRAFT-Start").WithTags("Debug")
              .WithSummary("Parks a graft parent ('Aguardar Notificação do AIIM'); waits for children (correlation-join)");

        routes.MapPost("/graft/{processId}/attach", AttachHandle)
              .WithName("GRAFT-Attach").WithTags("GRAFT")
              .WithSummary("A child instance attaches to the parked graft parent");

        routes.MapPost("/graft/{processId}/complete", CompleteHandle)
              .WithName("GRAFT-Complete").WithTags("GRAFT")
              .WithSummary("A child signals completion; parent proceeds when window closed + all done");

        routes.MapPost("/graft/{processId}/close", CloseHandle)
              .WithName("GRAFT-Close").WithTags("GRAFT")
              .WithSummary("Explicit close valve: shuts the graft window (ratified closure criterion)");

        return routes;
    }

    private static async Task<IResult> StartHandle(
        GraftStartRequest request, IWorkflowRuntime workflowRuntime, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");

        var client = await workflowRuntime.CreateClientAsync(ct);
        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(GraftParentElsaWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
                Input = new Dictionary<string, object>
                {
                    ["ProcessId"] = request.ProcessId,
                    ["IdAiim"] = request.IdAiim,
                    ["DemoTimeoutSeconds"] = request.DemoTimeoutSeconds <= 0 ? 30 : request.DemoTimeoutSeconds,
                },
            },
            ct);

        return Results.Ok(new
        {
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Hint = $"POST /graft/{request.ProcessId}/attach (childId), then /complete, then /close."
        });
    }

    private static async Task<IResult> AttachHandle(
        string processId, GraftChildRequest request, InMemoryGraftJoin graft, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ChildId))
            return Results.BadRequest("childId is required.");

        await graft.AttachAsync(processId, request.ChildId, ct);
        var (attached, completed) = graft.Snapshot(processId);
        Console.WriteLine($"[GRAFT] filho '{request.ChildId}' ANEXADO (attach). Estado: {completed}/{attached} concluídos.");
        return Results.Accepted(value: new { processId, request.ChildId, attached, completed });
    }

    private static async Task<IResult> CompleteHandle(
        string processId, GraftChildRequest request,
        InMemoryGraftJoin graft, IStimulusSender stimulusSender, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ChildId))
            return Results.BadRequest("childId is required.");

        await graft.SignalCompletedAsync(processId, request.ChildId, ct);
        var (attached, completed) = graft.Snapshot(processId);
        Console.WriteLine($"[GRAFT] filho '{request.ChildId}' CONCLUÍDO (complete). Estado: {completed}/{attached} concluídos.");

        await TryProceedAsync(processId, graft, stimulusSender, ct);
        return Results.Accepted(value: new { processId, request.ChildId, attached, completed });
    }

    private static async Task<IResult> CloseHandle(
        string processId, InMemoryGraftJoin graft, IStimulusSender stimulusSender, CancellationToken ct)
    {
        graft.Close(processId);
        var (attached, completed) = graft.Snapshot(processId);
        Console.WriteLine($"[GRAFT] valve CLOSE — janela fechada. Estado: {completed}/{attached} concluídos.");

        await TryProceedAsync(processId, graft, stimulusSender, ct);
        return Results.Accepted(value: new { processId, closed = true, attached, completed });
    }

    // Resume the parked parent when the window is closed and all attached children completed.
    private static async Task TryProceedAsync(
        string processId, InMemoryGraftJoin graft, IStimulusSender stimulusSender, CancellationToken ct)
    {
        if (!graft.IsReadyToProceed(processId)) return;

        await stimulusSender.SendAsync(
            activityTypeName: GraftParentActivity.GraftProceedBookmarkName,
            stimulus: new GraftProceedStimulus(processId),
            metadata: new StimulusMetadata { CorrelationId = processId },
            cancellationToken: ct);
    }
}

public sealed record GraftStartRequest(string ProcessId, long IdAiim, int DemoTimeoutSeconds);
public sealed record GraftChildRequest(string ChildId);
