#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Infrastructure.Runtime;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Read model for live workflow visibility (Part 2, Phase 1): the traversed BPMN path for a
/// PROCESS_ID plus its recorded service interactions, assembled from the two durable stores that
/// already exist (the snapshot's <c>Path</c> and the Part 1 interaction log). No new tables.
/// </summary>
public static class WorkflowJourneyEndpoint
{
    private const string BpmnKey = "POC_EpatProcess__MAIN";

    public static IEndpointRouteBuilder MapWorkflowJourney(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/workflow/{processId}/journey", async (
                string processId,
                PocEpatProcessState state,
                IServiceInteractionLog log,
                CancellationToken ct) =>
            {
                var snap = state.Load(processId);
                if (snap is null)
                    return Results.NotFound(new { processId, status = "Unknown" });

                var steps = snap.Path
                    .Select((nodeId, i) => new JourneyStep(i + 1, nodeId))
                    .ToArray();

                var completed = steps.Length > 0 && TerminalNodes.Contains(steps[^1].NodeId);
                var status = completed ? "Completed" : "Suspended";
                var currentNodeId = completed ? null : steps.LastOrDefault()?.NodeId;

                var interactions = (await log.GetAsync(processId, ct))
                    .Select(x => new InteractionView(
                        x.Port, x.Operation, x.Success, x.Failure, x.At, x.DurationMs))
                    .ToArray();

                return Results.Ok(new JourneyView(
                    processId, BpmnKey, status, steps, currentNodeId, interactions));
            })
            .WithName("Workflow-Journey")
            .WithTags("Evidence")
            .WithSummary("Traversed BPMN path + service interactions for a PROCESS_ID");

        return routes;
    }

    // Terminal (endEvent) node ids, derived from the canonical paths so there is one source of truth.
    private static readonly HashSet<string> TerminalNodes = new(new[]
    {
        PocEpatMainActivity.Sc001NodePath[^1],
        PocEpatMainActivity.Sc012MistaPath[^1],
        PocEpatMainActivity.Sc010DrfPath[^1],
        PocEpatMainActivity.Sc014NodePath[^1],
        PocEpatMainActivity.Sc015NodePath[^1],
    });
}

/// <summary>The traversed-path + interactions view for one PROCESS_ID.</summary>
public sealed record JourneyView(
    string ProcessId,
    string BpmnKey,
    string Status,
    IReadOnlyList<JourneyStep> Traversed,
    string? CurrentNodeId,
    IReadOnlyList<InteractionView> Interactions);

/// <summary>One traversed BPMN node; <see cref="Index"/> is its 1-based position in the path.</summary>
public sealed record JourneyStep(int Index, string NodeId);

/// <summary>A recorded service interaction, projected without request/response payloads.</summary>
public sealed record InteractionView(
    string Port, string Operation, bool Success, string? Failure, DateTimeOffset At, long DurationMs);
