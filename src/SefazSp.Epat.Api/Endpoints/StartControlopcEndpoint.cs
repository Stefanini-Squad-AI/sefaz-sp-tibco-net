#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Controlopc;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only: runs a CONTROPC 'Aguardar Retorno' instance. The dynamic subprocess target is
/// resolved at runtime from <c>aguardar</c> (AGUARDAR[IDX]). AgPecas → AGPECASPC (success);
/// the 6 external-package targets fail VISIBLY (interface-registry-validated).
/// </summary>
public static class StartControlopcEndpoint
{
    public static IEndpointRouteBuilder MapStartControlopc(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/controlopc/start", Handle)
              .WithName("Debug-CONTROPC-Start")
              .WithTags("Debug")
              .WithSummary("Runs CONTROPC 'Aguardar Retorno' (dynamic subprocess resolved by AGUARDAR)");
        return routes;
    }

    private static async Task<IResult> Handle(
        StartControlopcRequest request,
        IWorkflowRuntime workflowRuntime,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");
        if (string.IsNullOrWhiteSpace(request.Aguardar))
            return Results.BadRequest("Aguardar is required (e.g. AgPecas, AgPRJ, …).");

        var client = await workflowRuntime.CreateClientAsync(ct);
        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(ControlopcElsaWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
                Input = new Dictionary<string, object>
                {
                    ["ProcessId"] = request.ProcessId,
                    ["IdAiim"] = request.IdAiim,
                    ["Aguardar"] = request.Aguardar,
                },
            },
            ct);

        return Results.Ok(new
        {
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Aguardar = request.Aguardar,
            Hint = "Check the server console: AgPecas succeeds; external-package targets fail visibly."
        });
    }
}

public sealed record StartControlopcRequest(string ProcessId, long IdAiim, string Aguardar);
