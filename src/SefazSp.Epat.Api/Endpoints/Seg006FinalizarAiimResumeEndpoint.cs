#nullable enable

using Elsa.Workflows.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Seg006Parallel;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Delivers the 'Finalizar AIIM' submission to a suspended Seg006 instance, correlated by
/// PROCESS_ID (bookmark-correlation). On resume the workflow forks into the parallel AND-split.
/// </summary>
public static class Seg006FinalizarAiimResumeEndpoint
{
    public static IEndpointRouteBuilder MapSeg006FinalizarAiimResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/seg006/{processId}/finalizar-aiim", Handle)
              .WithName("SEG006-FinalizarAIIM-Resume")
              .WithTags("SEG006")
              .WithSummary("Submits 'Finalizar AIIM' to a suspended Seg006 instance (fires the AND-split)");
        return routes;
    }

    private static async Task<IResult> Handle(
        string processId,
        IStimulusSender stimulusSender,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(processId))
            return Results.BadRequest("processId is required.");

        await stimulusSender.SendAsync(
            activityTypeName: FinalizarAiimActivity.FinalizarAiimBookmarkName,
            stimulus: new FinalizarAiimStimulus(processId),
            metadata: new StimulusMetadata { CorrelationId = processId },
            cancellationToken: ct);

        return Results.Accepted();
    }
}
