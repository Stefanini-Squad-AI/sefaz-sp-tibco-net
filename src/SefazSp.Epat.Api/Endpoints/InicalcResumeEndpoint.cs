#nullable enable

using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Endpoint de retomada do receiveTask INICALC (_lrer81qhEfG5K7mY0I3I6w).
///
/// AC1 — INICALC não é alcançado por transição XPDL dentro do segmento:
/// é o nó de entrada do segmento, activado por evento externo.
/// Deve ser escrito explicitamente como ponto de entrada, ligado ao motor via
/// <see cref="ICorrelationStore"/> com a chave PROCESS_ID.
///
/// Chave de correlação: PROCESS_ID = 'idAiim-&lt;n&gt;idProc-&lt;n&gt;'
/// Construída pelos scripts legados antes de cada chamada — não inventar.
/// gaps.external-event = bookmark-correlation (NOEQ-external-event, ratificado 2026-08-06).
///
/// Rastreia: checklist ordem 1 (_lrer81qhEfG5K7mY0I3I6w, entrouPor=ausente, gap external-event=decided)
/// Processo: DEAT0050 · Segmento: BUILD-DEAT0050-seg009
/// </summary>
public static class InicalcResumeEndpoint
{
    public const string RoutePattern = "/api/deat0050/inicalc/resume";
    public const string BookmarkName = "INICALC";

    /// <summary>
    /// Regista o endpoint no grupo de rotas fornecido.
    /// </summary>
    public static IEndpointRouteBuilder MapInicalcResume(this IEndpointRouteBuilder app)
    {
        app.MapPost(RoutePattern, HandleAsync)
           .WithName("Deat0050-Inicalc-Resume")
           .WithSummary("Retomada do receiveTask INICALC (DEAT0050 seg009)")
           .WithDescription(
               "Recebe o evento externo que acorda a instância parada em INICALC. " +
               "A chave de correlação é o PROCESS_ID do caso no formato 'idAiim-<n>idProc-<n>'.");

        return app;
    }

    internal static async Task<IResult> HandleAsync(
        InicalcResumeRequest request,
        ICorrelationStore correlationStore,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId é obrigatório (chave de correlação PROCESS_ID).");

        var resumed = await correlationStore.ResumeAsync(request.ProcessId, payload: null, ct);
        if (!resumed)
            return Results.NotFound(
                $"Nenhuma instância aguarda o bookmark INICALC para PROCESS_ID='{request.ProcessId}'.");

        return Results.Accepted(
            value: new { ProcessId = request.ProcessId, Bookmark = BookmarkName, Status = "resumed" });
    }
}

/// <summary>
/// Corpo do pedido POST de retomada de INICALC.
/// </summary>
/// <param name="ProcessId">
/// Chave de correlação — PROCESS_ID no formato 'idAiim-&lt;n&gt;idProc-&lt;n&gt;'.
/// </param>
public sealed record InicalcResumeRequest(string ProcessId);
