#nullable enable

// Card: BUILD-POCEPATPROCESS-seg053
// Passo 5 — receiveTask Pedido de Vistas (_CtQ68lqPEfG5K7mY0I3I6w)
// Decisão NOEQ-external-event = bookmark-correlation (ratificado 2026-08-06).
//
// Este endpoint:
//   • Recebe o evento externo de retomada de Pedido de Vistas.
//   • Usa ICorrelationStore para retomar a instância de workflow suspensa.
//   • Correlaciona pela chave ProcessId.
//
// POR DEFINIR (etapa 5 do plano de cumprimento):
//   • Protecção do endpoint de retomada.
//   • Política de idempotência para entrega duplicada ou resposta atrasada.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Endpoint de retomada do processo POC_EpatProcess por evento externo (Pedido de Vistas).
///
/// <para>
/// Pedido de Vistas (<c>_CtQ68lqPEfG5K7mY0I3I6w</c>) é um receiveTask que suspende
/// o fluxo como bookmark do Elsa 3, aguardando correlação pela chave <c>ProcessId</c>.
/// Quando o evento de vistas chega, este endpoint entrega o sinal ao motor via
/// <see cref="ICorrelationStore"/>, que retoma o workflow.
/// </para>
///
/// <para>
/// Decisão <c>NOEQ-external-event = bookmark-correlation</c> (ratificado 2026-08-06).
/// </para>
///
/// <para>
/// POR DEFINIR (etapa 5): protecção do endpoint e idempotência para entrega duplicada.
/// </para>
/// </summary>
public static class PedidoDeVistasResumeEndpoint
{
    /// <summary>
    /// Regista a rota <c>POST /poc-epat-process/pedido-de-vistas/resume</c> no <paramref name="routes"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapPedidoDeVistasResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/poc-epat-process/pedido-de-vistas/resume", HandleAsync)
              .WithName("POC-EpatProcess-PedidoDeVistas-Resume")
              .WithTags("POC_EpatProcess")
              .WithSummary(
                  "Retomada de POC_EpatProcess por evento externo " +
                  "(Pedido de Vistas bookmark-correlation)");

        return routes;
    }

    private static async Task<IResult> HandleAsync(
        PedidoDeVistasResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId é obrigatório.");

        // Retoma o bookmark Pedido de Vistas correlacionado pela chave ProcessId.
        // O motor Elsa 3 localiza a instância suspensa e avança o fluxo.
        var resumed = await correlationStore.ResumeAsync(
            correlationKey: request.ProcessId,
            payload: request,
            ct: ct);

        if (!resumed)
            return Results.NotFound(
                $"Nenhuma instância de POC_EpatProcess aguardando correlação para ProcessId='{request.ProcessId}'.");

        return Results.Accepted();
    }
}

/// <summary>
/// Payload do evento externo de Pedido de Vistas no processo POC_EpatProcess.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlação — identifica a instância a retomar.
/// Formato dependente do legado; normalmente <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>.
/// </param>
public sealed record PedidoDeVistasResumeRequest(string ProcessId);
