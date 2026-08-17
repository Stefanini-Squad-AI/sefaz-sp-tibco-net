#nullable enable

// Card: BUILD-POCEPATPROCESS-seg023
// AC1 + AC3 — endpoint de retomada por evento externo (bookmark-correlation)
//             com mecanismo de graft-step (correlation-join).
//
// Decisoes ratificadas:
//   NOEQ-external-event → bookmark-correlation (2026-08-06):
//     A retomada por identidade de caso implicita do iProcess e substituida por
//     chave de correlacao explicita (PROCESS_ID) e endpoint explicito.
//     Chave de correlacao: PROCESS_ID = 'idAiim-<n>idProc-<n>' — nao inventar;
//     montada pelos scripts legados antes de cada chamada.
//
//   NOEQ-graft-step → correlation-join (2026-08-06):
//     O contrato fica no lado do pai: o filho sinaliza; o pai agrega.
//     'Iniciar Novo Graft' (_OAgPol9UEfG6Lfb98zsREQ, l.3032 do XPDL) e uma das
//     duas valvulas de reinicio manual do graft step — IN SCOPE por decisao explicita.
//     O endpoint recebe o sinal externo e retoma a instancia suspensa via ICorrelationStore.
//
// POR DEFINIR (etapa 5 do plano):
//   • Proteccao do endpoint de retomada.
//   • Politica de idempotencia para entrega duplicada ou resposta atrasada.
//   • Criterio de encerramento do graft (timeout para filho que nunca termina).

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Endpoint de retomada do receiveTask 'Iniciar Novo Graft'
/// (<c>_OAgPol9UEfG6Lfb98zsREQ</c>) no processo POC_EpatProcess.
///
/// Este no e uma valvula de reinicio manual do graft step: apos erros de ecra ou
/// procedimento humano, permite reinicar o 'Aguardar Notificacao do AIIM'.
/// O passo NAO existe como transicao XPDL de entrada — e o ponto de abertura
/// do segmento, activado exclusivamente por evento externo.
///
/// Mecanismo: bookmark-correlation (NOEQ-external-event).
/// Padrao: correlation-join, contrato no pai (NOEQ-graft-step).
/// Chave de correlacao: PROCESS_ID = <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>.
/// </summary>
public static class IniciarNovoGraftResumeEndpoint
{
    /// <summary>Nome do bookmark registado pelo motor Elsa 3 para este receiveTask.</summary>
    public const string BookmarkName = "INICNVGR";

    /// <summary>Padrao de rota do endpoint de retomada.</summary>
    public const string RoutePattern = "/api/poc-epat-process/iniciar-novo-graft/resume";

    /// <summary>
    /// Regista a rota no <paramref name="app"/> fornecido.
    /// </summary>
    public static IEndpointRouteBuilder MapIniciarNovoGraftResume(this IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePattern, HandleAsync)
           .WithName("PocEpatProcess-IniciarNovoGraft-Resume")
           .WithTags("POC_EpatProcess")
           .WithSummary(
               "Retomada de 'Iniciar Novo Graft' (_OAgPol9UEfG6Lfb98zsREQ) " +
               "por evento externo — graft-step correlation-join (NOEQ-graft-step).")
           .WithDescription(
               "Recebe o sinal externo que reactiva a valvula de reinicio do graft. " +
               "A chave de correlacao e o PROCESS_ID no formato 'idAiim-<n>idProc-<n>'. " +
               "Idempotencia e proteccao do endpoint: POR DEFINIR (etapa 5 do plano).");

        return app;
    }

    internal static async Task<IResult> HandleAsync(
        IniciarNovoGraftResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest(
                "ProcessId e obrigatorio (chave de correlacao PROCESS_ID, formato 'idAiim-<n>idProc-<n>').");

        // Retoma o bookmark INICNVGR correlacionado pela chave PROCESS_ID.
        // O motor localiza a instancia suspensa no passo 'Iniciar Novo Graft' e avanca o fluxo.
        // correlation-join: o filho sinaliza; o pai agrega e controla o encerramento.
        var resumed = await correlationStore.ResumeAsync(
            correlationKey: request.ProcessId,
            payload: request,
            ct: ct);

        if (!resumed)
            return Results.NotFound(
                $"Nenhuma instancia de POC_EpatProcess aguardando o bookmark '{BookmarkName}' " +
                $"para PROCESS_ID='{request.ProcessId}'.");

        return Results.Accepted(value: new
        {
            ProcessId = request.ProcessId,
            Bookmark = BookmarkName,
            Status = "resumed"
        });
    }
}

/// <summary>
/// Payload do evento externo que reactiva 'Iniciar Novo Graft'.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlacao — PROCESS_ID no formato <c>idAiim-&lt;n&gt;idProc-&lt;n&gt;</c>,
/// montada pelos scripts legados antes de cada chamada.
/// </param>
public sealed record IniciarNovoGraftResumeRequest(string ProcessId);
