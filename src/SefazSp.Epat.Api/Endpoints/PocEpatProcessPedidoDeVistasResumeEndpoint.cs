#nullable enable

// Card: BUILD-POCEPATPROCESS-seg052
// Checklist ordem 8: receiveTask _CtQ68lqPEfG5K7mY0I3I6w "Pedido de Vistas"
// Decisão NOEQ-external-event = bookmark-correlation (ratificado 2026-08-06).
//
// Este endpoint:
//   • Recebe o evento externo de pedido de vistas.
//   • Usa ICorrelationStore para retomar a instância de workflow suspensa em POC_EpatProcess.
//   • Correlaciona pela chave PROCESS_ID.
//   • Chave PROCESS_ID: 'idAiim-<n>idProc-<n>' — montada pelos scripts antes de cada
//     chamada; não precisa de ser inventada, só transcrita.
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
/// o fluxo como bookmark do Elsa 3, aguardando correlação pela chave <c>PROCESS_ID</c>.
/// Quando o evento de pedido de vistas chega, este endpoint entrega o sinal ao motor via
/// <see cref="ICorrelationStore"/>, que retoma o workflow.
/// </para>
///
/// <para>
/// Decisão <c>NOEQ-external-event = bookmark-correlation</c> (ratificado 2026-08-06).
/// A chave de correlação <c>PROCESS_ID</c> tem o formato <c>'idAiim-&lt;n&gt;idProc-&lt;n&gt;'</c>,
/// montada pelos scripts antes de cada chamada.
/// </para>
///
/// <para>
/// Nota: este nó é visitado noutra passagem pelo mesmo troco (SC-POC_EpatProcess-007)
/// e não aparece no percurso de referência do segmento 052 (passos 22–28).
/// </para>
///
/// <para>
/// POR DEFINIR (etapa 5): protecção do endpoint e idempotência para entrega duplicada.
/// </para>
/// </summary>
public static class PocEpatProcessPedidoDeVistasResumeEndpoint
{
    /// <summary>
    /// Regista a rota <c>POST /poc-epat-process/pedido-de-vistas/resume</c> no <paramref name="routes"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapPocEpatProcessPedidoDeVistasResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/poc-epat-process/pedido-de-vistas/resume", HandleAsync)
              .WithName("POCEpatProcess-PedidoDeVistas-Resume")
              .WithTags("POCEpatProcess")
              .WithSummary(
                  "Retomada de POC_EpatProcess por evento externo " +
                  "(Pedido de Vistas bookmark-correlation)");

        return routes;
    }

    private static async Task<IResult> HandleAsync(
        PocEpatProcessPedidoDeVistasResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId e obrigatorio.");

        // Retoma o bookmark Pedido de Vistas correlacionado pela chave PROCESS_ID.
        // O motor Elsa 3 localiza a instância suspensa e avança o fluxo.
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
/// Payload do evento externo de Pedido de Vistas no processo POC_EpatProcess.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlação — formato <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>,
/// montada pelos scripts antes de cada chamada. Identifica a instância a retomar.
/// </param>
public sealed record PocEpatProcessPedidoDeVistasResumeRequest(string ProcessId);
