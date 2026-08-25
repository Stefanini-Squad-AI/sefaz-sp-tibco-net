#nullable enable

using Elsa.Workflows.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Agpecaspc;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Delivers the interposições external event to a suspended AGPECASPC instance (bookmark-correlation).
/// If the boundary timer already fired, this no-ops (race already resolved).
/// </summary>
public static class AgpecaspcInterposicoesResumeEndpoint
{
    public static IEndpointRouteBuilder MapAgpecaspcInterposicoesResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/agpecaspc/{processId}/interposicoes", Handle)
              .WithName("AGPECASPC-Interposicoes-Resume")
              .WithTags("AGPECASPC")
              .WithSummary("Delivers the interposições external event to a suspended AGPECASPC instance");
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
            activityTypeName: AgpecaspcElsaActivity.InterposicoesBookmarkName,
            stimulus: new InterposicoesStimulus(processId),
            metadata: new StimulusMetadata { CorrelationId = processId },
            cancellationToken: ct);

        return Results.Accepted();
    }
}
