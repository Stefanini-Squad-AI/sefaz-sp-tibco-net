#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Rules;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only showcase for the Decisions rules engine (fundacao-motor-de-regras): feed the request
/// attributes (by normalized name, e.g. "motivoIntimacao":"2") and get the 11 folded response
/// attributes. Reproduces the ratified Corticon override fold. Unset attributes are omitted (SW_NA).
/// </summary>
public static class DecisionsEvaluateEndpoint
{
    public static IEndpointRouteBuilder MapDecisionsEvaluate(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/decisions/evaluate", Handle)
              .WithName("Debug-Decisions-Evaluate").WithTags("Debug")
              .WithSummary("Evaluates the intimacoes decision table (Corticon override fold) for the given request");
        return routes;
    }

    private static IResult Handle(DecisionsEvaluateRequest request, IIntimacoesDecision decision)
    {
        var attrs = request.Attributes ?? new Dictionary<string, string?>();
        var response = decision.Evaluate(IntimacoesRequest.From(attrs));
        var set = response.Attributes.Where(kv => kv.Value is not null)
                                     .ToDictionary(kv => kv.Key, kv => kv.Value);
        return Results.Ok(new
        {
            Input = attrs,
            Output = set,             // apenas os atributos escritos por alguma regra
            UnsetCount = 11 - set.Count, // os restantes ficam SW_NA
        });
    }
}

public sealed record DecisionsEvaluateRequest(Dictionary<string, string?>? Attributes);
