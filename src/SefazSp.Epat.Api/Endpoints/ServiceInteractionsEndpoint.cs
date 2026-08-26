#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Evidence endpoint: the recorded external-service interactions (request/response) for a PROCESS_ID.
/// Satisfies the DoD evidence package's "service request/response or test-double interaction record".
/// </summary>
public static class ServiceInteractionsEndpoint
{
    public static IEndpointRouteBuilder MapServiceInteractions(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/interactions/{processId}", async (
                string processId, IServiceInteractionLog log, CancellationToken ct) =>
            {
                var interactions = await log.GetAsync(processId, ct);
                return Results.Ok(new { processId, count = interactions.Count, interactions });
            })
            .WithName("Service-Interactions")
            .WithTags("Evidence")
            .WithSummary("Recorded external-service interactions (request/response) for a PROCESS_ID");
        return routes;
    }
}
