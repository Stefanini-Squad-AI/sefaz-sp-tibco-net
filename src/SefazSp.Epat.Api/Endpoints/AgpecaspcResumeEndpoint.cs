#nullable enable

// Card: BUILD-AGPECASPC-seg040
// Passo 6 — receiveTask Aguardar Interposicoes (_EvOwQl6eEfGJqLUhfbpFcQ)
// Decisao NOEQ-external-event = bookmark-correlation (ratificado 2026-08-06).
//
// Este endpoint:
//   • Recebe o evento externo de interposicao (chegada das pecas).
//   • Usa ICorrelationStore para retomar a instancia de workflow suspensa em AGPECASPC.
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
/// Endpoint de retomada do processo AGPECASPC por evento externo (interposição).
///
/// <para>
/// Aguardar Interposições (<c>_EvOwQl6eEfGJqLUhfbpFcQ</c>) é um receiveTask que suspende
/// o fluxo como bookmark do Elsa 3, aguardando correlação pela chave <c>PROCESS_ID</c>.
/// Quando o evento de interposição chega, este endpoint entrega o sinal ao motor via
/// <see cref="ICorrelationStore"/>, que retoma o workflow e avança para Controla Datas.
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
public static class AgpecaspcResumeEndpoint
{
    /// <summary>
    /// Regista a rota <c>POST /agpecaspc/resume</c> no <paramref name="routes"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapAgpecaspcResume(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/agpecaspc/resume", HandleAsync)
              .WithName("AGPECASPC-Resume")
              .WithTags("AGPECASPC")
              .WithSummary(
                  "Retomada de AGPECASPC por evento externo " +
                  "(Aguardar Interposições bookmark-correlation)");

        return routes;
    }

    private static async Task<IResult> HandleAsync(
        AgpecaspcResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId e obrigatorio.");

        // Retoma o bookmark Aguardar Interposicoes correlacionado pela chave PROCESS_ID.
        // O motor Elsa 3 localiza a instancia suspensa e avanca o fluxo para Controla Datas.
        var resumed = await correlationStore.ResumeAsync(
            correlationKey: request.ProcessId,
            payload: request,
            ct: ct);

        if (!resumed)
            return Results.NotFound(
                $"Nenhuma instancia de AGPECASPC aguardando correlacao para PROCESS_ID='{request.ProcessId}'.");

        return Results.Accepted();
    }
}

/// <summary>
/// Payload do evento externo de interposição no processo AGPECASPC.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlação — formato <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>,
/// montada pelos scripts antes de cada chamada. Identifica a instância a retomar.
/// </param>
public sealed record AgpecaspcResumeRequest(string ProcessId);
