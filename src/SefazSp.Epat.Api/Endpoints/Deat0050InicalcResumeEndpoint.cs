#nullable enable

using Elsa.Workflows.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Deat0050;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Delivers the INICALC external event to a suspended DEAT0050 instance, correlated by
/// PROCESS_ID (bookmark-correlation, NOEQ-external-event).
/// </summary>
public static class Deat0050InicalcResumeEndpoint
{
    public static IEndpointRouteBuilder MapDeat0050InicalcResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/deat0050/{processId}/inicalc", Handle)
              .WithName("DEAT0050-INICALC-Resume")
              .WithTags("DEAT0050")
              .WithSummary("Delivers the INICALC external event to a suspended DEAT0050 instance");
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
            activityTypeName: Deat0050ElsaActivity.InicalcBookmarkName,
            stimulus: new InicalcStimulus(processId),
            metadata: new StimulusMetadata { CorrelationId = processId },
            cancellationToken: ct);

        return Results.Accepted();
    }
}
