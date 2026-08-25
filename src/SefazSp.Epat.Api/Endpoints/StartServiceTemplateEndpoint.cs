#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.ServiceTemplate;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only: starts a service-template workflow instance for one of the 5 processes.
/// With the in-memory EpatServices double returning an app error, the retry loop exhausts
/// and the instance suspends on 'Manipular Excecao'. Resume via /service/{process}/manipular-excecao.
/// </summary>
public static class StartServiceTemplateEndpoint
{
    public static IEndpointRouteBuilder MapStartServiceTemplate(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/service/{process}/start", Handle)
              .WithName("Debug-Service-Start")
              .WithTags("Debug")
              .WithSummary("Starts a service-template workflow (CALCPRPC/BSCENVPC/PRPINTPC/ATZINTPC/CRNOTPC)");
        return routes;
    }

    private static async Task<IResult> Handle(
        string process,
        StartServiceRequest request,
        IWorkflowRuntime workflowRuntime,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");

        var client = await workflowRuntime.CreateClientAsync(ct);
        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(ServiceTemplateWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
                Input = new Dictionary<string, object>
                {
                    ["ProcessKey"] = process.ToUpperInvariant(),
                    ["ProcessId"] = request.ProcessId,
                    ["IdAiim"] = request.IdAiim,
                },
            },
            ct);

        return Results.Ok(new
        {
            Process = process.ToUpperInvariant(),
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Hint = $"POST /service/{process}/manipular-excecao with {{processId, outcome:'R'|'OK'}} to resume."
        });
    }
}

public sealed record StartServiceRequest(string ProcessId, long IdAiim);
