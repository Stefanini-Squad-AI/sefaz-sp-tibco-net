#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Endpoint de recepção de evento externo de interposição.
/// Retoma a instância AGPECASPC suspensa em "Aguardar Interposicoes"
/// via bookmark de correlação (ICorrelationStore).
/// Nó: _EvOwQl6eEfGJqLUhfbpFcQ (receive task).
/// </summary>
public static class AgpecaspcInterposicoesEndpoint
{
    public static IEndpointRouteBuilder MapAgpecaspcInterposicoes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/agpecaspc/interposicoes/{correlationKey}", HandleAsync)
           .WithName("AgpecaspcInterposicoes")
           .WithTags("AGPECASPC");
        return app;
    }

    private static async Task<IResult> HandleAsync(
        string correlationKey,
        InterposicaoPayload payload,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        var resumed = await correlationStore.ResumeAsync(correlationKey, payload, ct);
        return resumed
            ? Results.Accepted()
            : Results.NotFound($"Nenhuma instância aguarda correlationKey='{correlationKey}'.");
    }
}

/// <summary>Payload de evento externo de interposição.</summary>
public sealed record InterposicaoPayload(string IdAiim, string TipoInterposicao, DateOnly DataInterposicao);
