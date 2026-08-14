#nullable enable

// Card: BUILD-POCEPATPROCESS-seg051
// Passo 5 — receiveTask Pedido de Vistas (_CtQ68lqPEfG5K7mY0I3I6w)
// Decisao NOEQ-external-event = bookmark-correlation (ratificado 2026-08-06).
//
// Este endpoint:
//   • Recebe o evento externo de pedido de vistas (chegada das vistas).
//   • Usa ICorrelationStore para retomar a instancia de workflow suspensa em POC_EpatProcess.
//   • Correlaciona pela chave PROCESS_ID.
//   • Chave PROCESS_ID: 'idAiim-<n>idProc-<n>' — montada pelos scripts antes de cada
//     chamada; nao precisa de ser inventada, so transcrita.
//
// POR DEFINIR (etapa 5 do plano de cumprimento):
//   • Proteccao do endpoint de retomada.
//   • Politica de idempotencia para entrega duplicada ou resposta atrasada.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Endpoint de retomada do processo POC_EpatProcess por evento externo
/// (Pedido de Vistas).
///
/// <para>
/// Pedido de Vistas (<c>_CtQ68lqPEfG5K7mY0I3I6w</c>) é um receiveTask que suspende
/// o fluxo como bookmark do Elsa 3, aguardando correlação pela chave <c>PROCESS_ID</c>.
/// Quando a resposta de vistas chega, este endpoint entrega o sinal ao motor via
/// <see cref="ICorrelationStore"/>, que retoma o workflow e avança para o nó seguinte.
/// </para>
///
/// <para>
/// Decisão <c>NOEQ-external-event = bookmark-correlation</c> (ratificado 2026-08-06).
/// A chave de correlação <c>PROCESS_ID</c> tem o formato <c>'idAiim-&lt;n&gt;idProc-&lt;n&gt;'</c>,
/// montada pelos scripts antes de cada chamada.
/// </para>
///
/// <para>
/// POR DEFINIR (etapa 5): protecção do endpoint e idempotência para entrega duplicada.
/// </para>
/// </summary>
public static class PocEpatProcessResumeEndpoint
{
    /// <summary>
    /// Regista a rota <c>POST /poc-epat-process/resume</c> no <paramref name="routes"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapPocEpatProcessResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/poc-epat-process/resume", HandleAsync)
              .WithName("POCEpatProcess-Resume")
              .WithTags("POCEpatProcess")
              .WithSummary(
                  "Retomada de POC_EpatProcess por evento externo " +
                  "(Pedido de Vistas bookmark-correlation)");

        return routes;
    }

    private static async Task<IResult> HandleAsync(
        PocEpatProcessResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId e obrigatorio.");

        // Retoma o bookmark Pedido de Vistas correlacionado pela chave PROCESS_ID.
        // O motor Elsa 3 localiza a instancia suspensa e avanca o fluxo para o nó seguinte.
        var resumed = await correlationStore.ResumeAsync(
            correlationKey: request.ProcessId,
            payload: request,
            ct: ct);

        if (!resumed)
            return Results.NotFound(
                $"Nenhuma instancia de POC_EpatProcess aguardando correlacao para PROCESS_ID='{request.ProcessId}'.");

        return Results.Accepted();
    }
}

/// <summary>
/// Payload do evento externo de pedido de vistas no processo POC_EpatProcess.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlação — formato <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>,
/// montada pelos scripts antes de cada chamada. Identifica a instância a retomar.
/// </param>
public sealed record PocEpatProcessResumeRequest(string ProcessId);
