#nullable enable

using Elsa.Workflows.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.ServiceTemplate;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Resume endpoint for the 'Manipular Excecao' operator step of the 5 service subprocesses.
/// The operator submits OUTCOME ('R' = try again, 'OK' = manually fixed). The decision is
/// deposited in the inbox (correlated by PROCESS_ID) and the workflow bookmark is released.
/// </summary>
public static class ManipularExcecaoResumeEndpoint
{
    public static IEndpointRouteBuilder MapManipularExcecaoResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/service/{process}/manipular-excecao", Handle)
              .WithName("Service-ManipularExcecao-Resume")
              .WithTags("Service")
              .WithSummary("Resumes a service subprocess suspended on Manipular Excecao");
        return routes;
    }

    private static async Task<IResult> Handle(
        string process,
        ManipularExcecaoRequest request,
        IOperatorDecisionInbox inbox,
        IStimulusSender stimulusSender,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");
        if (request.Outcome is not ("R" or "OK"))
            return Results.BadRequest("Outcome must be 'R' (try again) or 'OK' (manually fixed).");

        inbox.Set(request.ProcessId, request.Outcome);

        // Release the 'Manipular Excecao' bookmark, correlated by PROCESS_ID.
        await stimulusSender.SendAsync(
            activityTypeName: ServiceRetryActivity.BookmarkName,
            stimulus: new ManipularExcecaoStimulus(process.ToUpperInvariant(), request.ProcessId),
            metadata: new StimulusMetadata { CorrelationId = request.ProcessId },
            cancellationToken: ct);

        return Results.Accepted();
    }
}

public sealed record ManipularExcecaoRequest(string ProcessId, string Outcome);
