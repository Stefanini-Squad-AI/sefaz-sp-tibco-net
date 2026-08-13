#nullable enable

// AC1 — endpoint de retomada por evento externo (bookmark-correlation).
// Decisao NOEQ-external-event ratificada em 2026-08-06: bookmark-correlation.
// A chave de correlacao PROCESS_ID e montada pelos scripts antes de cada chamada
// no formato 'idAiim-<n>idProc-<n>' — nao precisa ser inventada, apenas transcrita.
//
// Este endpoint:
//   • Recebe o evento externo de notificacao do AIIM.
//   • Usa ICorrelationStore para retomar a instancia de workflow suspensa em INICALC.
//   • Correlaciona pela chave PROCESS_ID.
//   • Idempotencia (entrega duplicada ou resposta atrasada): POR DEFINIR — etapa 5 do plano.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Endpoint de retomada do processo DEAT0050 por evento externo.
///
/// INICALC (<c>_lrer81qhEfG5K7mY0I3I6w</c>) é um receiveTask que suspende o fluxo
/// como bookmark do Elsa 3, aguardando correlacao pela chave <c>PROCESS_ID</c>.
/// Quando o evento de notificacao do AIIM chega, este endpoint entrega o sinal
/// ao motor via <see cref="ICorrelationStore"/>, que retoma o workflow e avanca
/// para CalculaPrazo.
/// </summary>
public static class Deat0050ResumeEndpoint
{
    /// <summary>
    /// Regista a rota <c>POST /deat0050/resume</c> no <paramref name="routes"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapDeat0050Resume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/deat0050/resume", HandleAsync)
              .WithName("DEAT0050-Resume")
              .WithTags("DEAT0050")
              .WithSummary("Retomada de DEAT0050 por evento externo (INICALC bookmark-correlation)");

        return routes;
    }

    private static async Task<IResult> HandleAsync(
        Deat0050ResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId e obrigatorio.");

        // Retoma o bookmark INICALC correlacionado pela chave PROCESS_ID.
        // O motor do Elsa 3 localiza a instancia suspensa e avanca o fluxo.
        var resumed = await correlationStore.ResumeAsync(
            correlationKey: request.ProcessId,
            payload: request,
            ct: ct);

        if (!resumed)
            return Results.NotFound(
                $"Nenhuma instancia de DEAT0050 aguardando correlacao para PROCESS_ID='{request.ProcessId}'.");

        return Results.Accepted();
    }
}

/// <summary>
/// Payload do evento externo de notificacao do AIIM.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlacao do processo — formato <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>,
/// montada pelos scripts antes de cada chamada. Identifica a instancia a retomar.
/// </param>
public sealed record Deat0050ResumeRequest(string ProcessId);
