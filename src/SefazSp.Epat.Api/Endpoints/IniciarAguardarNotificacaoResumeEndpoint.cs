#nullable enable

// AC1 — endpoint de retomada por evento externo (bookmark-correlation).
// Decisao NOEQ-external-event ratificada em 2026-08-06: bookmark-correlation.
// A chave de correlacao PROCESS_ID e montada pelos scripts antes de cada chamada
// no formato 'idAiim-<n>idProc-<n>' — nao precisa ser inventada, apenas transcrita.
//
// Este endpoint:
//   • Recebe o evento externo de notificacao do AIIM que retoma o caso suspenso em
//     'Iniciar Aguardar Notificacao' (_0XWaglqNEfG5K7mY0I3I6w, receiveTask).
//   • Usa ICorrelationStore para retomar a instancia de workflow suspensa.
//   • Correlaciona pela chave PROCESS_ID.
//   • Idempotencia (entrega duplicada ou resposta atrasada): POR DEFINIR — etapa 5 do plano.
//
// Rastreia: BUILD-POCEPATPROCESS-seg022, checklist ordem 1
//   (_0XWaglqNEfG5K7mY0I3I6w, entrouPor=ausente, gap external-event=decided)

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Endpoint de retomada do receiveTask 'Iniciar Aguardar Notificacao'
/// (<c>_0XWaglqNEfG5K7mY0I3I6w</c>) do processo POC_EpatProcess.
///
/// Este nó não existe como transição XPDL dentro do segmento — é a entrada do segmento
/// por evento externo. O endpoint recebe a notificação do AIIM e usa
/// <see cref="ICorrelationStore"/> para retomar a instância suspensa identificada por PROCESS_ID.
///
/// Chave de correlação: PROCESS_ID = 'idAiim-&lt;n&gt;idProc-&lt;n&gt;',
/// montada pelos scripts legados antes de cada chamada.
/// gaps.external-event = bookmark-correlation (NOEQ-external-event, ratificado 2026-08-06).
/// </summary>
public static class IniciarAguardarNotificacaoResumeEndpoint
{
    public const string RoutePattern = "/api/poc-epat-process/iniciar-aguardar-notificacao/resume";
    public const string BookmarkName  = "IniciarAguardarNotificacao";

    /// <summary>
    /// Regista o endpoint no grupo de rotas fornecido.
    /// </summary>
    public static IEndpointRouteBuilder MapIniciarAguardarNotificacaoResume(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePattern, HandleAsync)
           .WithName("POCEpatProcess-IniciarAguardarNotificacao-Resume")
           .WithTags("POCEpatProcess")
           .WithSummary("Retomada do receiveTask 'Iniciar Aguardar Notificacao' (POC_EpatProcess seg022)")
           .WithDescription(
               "Recebe o evento externo do AIIM que acorda a instância parada em " +
               "'Iniciar Aguardar Notificacao' (_0XWaglqNEfG5K7mY0I3I6w). " +
               "A chave de correlação é o PROCESS_ID no formato 'idAiim-<n>idProc-<n>'.");

        return app;
    }

    internal static async Task<IResult> HandleAsync(
        IniciarAguardarNotificacaoResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest(
                "ProcessId é obrigatório (chave de correlação PROCESS_ID = 'idAiim-<n>idProc-<n>').");

        var resumed = await correlationStore.ResumeAsync(
            correlationKey: request.ProcessId,
            payload: request,
            ct: ct);

        if (!resumed)
            return Results.NotFound(
                $"Nenhuma instância de POC_EpatProcess aguarda o bookmark '{BookmarkName}' " +
                $"para PROCESS_ID='{request.ProcessId}'.");

        return Results.Accepted(
            value: new
            {
                ProcessId  = request.ProcessId,
                Bookmark   = BookmarkName,
                NodeId     = "_0XWaglqNEfG5K7mY0I3I6w",
                Status     = "resumed",
            });
    }
}

/// <summary>
/// Payload do evento externo de notificação do AIIM para o processo POC_EpatProcess.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlação — PROCESS_ID no formato 'idAiim-&lt;n&gt;idProc-&lt;n&gt;'.
/// Identifica a instância suspensa em 'Iniciar Aguardar Notificacao'.
/// </param>
public sealed record IniciarAguardarNotificacaoResumeRequest(string ProcessId);
