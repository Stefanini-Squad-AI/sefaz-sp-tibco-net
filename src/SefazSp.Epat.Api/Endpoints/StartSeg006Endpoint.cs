#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Seg006Parallel;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only: starts a Seg006 (POC_EpatProcess parallel AND-split) instance. It suspends at the
/// 'Finalizar AIIM' userTask (external event). Resume via POST /seg006/{processId}/finalizar-aiim;
/// it then forks into both branches in parallel (Existe Notificação? and Set Nome Etapa 2).
/// </summary>
public static class StartSeg006Endpoint
{
    public static IEndpointRouteBuilder MapStartSeg006(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/seg006/start", Handle)
              .WithName("Debug-SEG006-Start")
              .WithTags("Debug")
              .WithSummary("Starts Seg006 (suspends at 'Finalizar AIIM', then AND-split fires both branches)");
        return routes;
    }

    private static async Task<IResult> Handle(
        StartSeg006Request request,
        IWorkflowRuntime workflowRuntime,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");

        var client = await workflowRuntime.CreateClientAsync(ct);
        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(Seg006ParallelElsaWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
                Input = new Dictionary<string, object>
                {
                    ["ProcessId"] = request.ProcessId,
                    ["IdAiim"] = request.IdAiim,
                    ["AfrName"] = string.IsNullOrWhiteSpace(request.AfrName) ? "AFR-DEMO" : request.AfrName,
                    ["ExisteNotificacao"] = request.ExisteNotificacao,
                },
            },
            ct);

        return Results.Ok(new
        {
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Hint = $"POST /seg006/{request.ProcessId}/finalizar-aiim to submit the userTask and fire the AND-split."
        });
    }
}

public sealed record StartSeg006Request(string ProcessId, long IdAiim, string? AfrName, bool ExisteNotificacao);
