#nullable enable

using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;
using SefazSp.Epat.Infrastructure.Runtime;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only endpoints for the main POC_EpatProcess flow (Phase 1, SC-001 path). Start the
/// instance, then deliver the 5 external events in order: iniciar-novo-graft → preparar-notificacao
/// (correcao) → finalizar-aiim (afrName) → verificar-retorno (tipoVistas) → vistas-do-juiz.
/// </summary>
public static class PocEpatMainEndpoints
{
    public static IEndpointRouteBuilder MapPocEpatMain(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/pocepat/start", StartHandle)
              .WithName("Debug-POCEPAT-Start").WithTags("Debug")
              .WithSummary("Starts the main POC_EpatProcess flow (SC-001 path); suspends at 'Iniciar Novo Graft'");

        routes.MapPost("/pocepat/{processId}/iniciar-novo-graft", (string processId, IStimulusSender s, CancellationToken ct)
            => ResumeAsync(processId, PocEpatMainActivity.BkIniciarNovoGraft, s, ct))
              .WithTags("POCEPAT").WithSummary("Event 1/5 — Iniciar Novo Graft");

        routes.MapPost("/pocepat/{processId}/preparar-notificacao", PrepararNotificacaoHandle)
              .WithTags("POCEPAT").WithSummary("Event 2/5 — Preparar Notificacao (body: correcao)");

        routes.MapPost("/pocepat/{processId}/finalizar-aiim", FinalizarAiimHandle)
              .WithTags("POCEPAT").WithSummary("Event 3/6 — Finalizar AIIM (body: afrName)");

        routes.MapPost("/pocepat/{processId}/deat-inicalc", (string processId, IStimulusSender s, CancellationToken ct)
            => ResumeAsync(processId, PocEpatMainActivity.BkDeatInicalc, s, ct))
              .WithTags("POCEPAT").WithSummary("Event 4/6 — DEAT0050 INICALC (then Aguarda Defesa timer auto-fires)");

        routes.MapPost("/pocepat/{processId}/verificar-retorno", VerificarRetornoHandle)
              .WithTags("POCEPAT").WithSummary("Event 5/6 — Verificar Retorno Decisions (body: tipoVistas)");

        routes.MapPost("/pocepat/{processId}/vistas-do-juiz", (string processId, IStimulusSender s, CancellationToken ct)
            => ResumeAsync(processId, PocEpatMainActivity.BkVistasDoJuiz, s, ct))
              .WithTags("POCEPAT").WithSummary("Event 6/6 (JUIZ) — Vistas do Juiz");

        routes.MapPost("/pocepat/{processId}/realizar-vista-mista", (string processId, IStimulusSender s, CancellationToken ct)
            => ResumeAsync(processId, PocEpatMainActivity.BkRealizarVistaMista, s, ct))
              .WithTags("POCEPAT").WithSummary("Etapa 5 (MISTA) — Realizar Atividade Vista Mista → endEvent (SC-012)");

        routes.MapPost("/pocepat/{processId}/pedido-de-vistas", (string processId, IStimulusSender s, CancellationToken ct)
            => ResumeAsync(processId, PocEpatMainActivity.BkPedidoDeVistas, s, ct))
              .WithTags("POCEPAT").WithSummary("Etapa 5 (DRF) — Pedido de Vistas event (races the boundary timer; timer→SC-010)");

        // Etapa 2 graft-real (GraftMode=true): DEAT0050 children attach/complete; parent proceeds on close+all-done.
        routes.MapPost("/pocepat/{processId}/graft-attach", GraftAttachHandle)
              .WithTags("POCEPAT").WithSummary("Graft-real — a DEAT0050 child attaches (body: childId)");
        routes.MapPost("/pocepat/{processId}/graft-complete", GraftCompleteHandle)
              .WithTags("POCEPAT").WithSummary("Graft-real — a child completes; parent proceeds when window closed + all done");
        routes.MapPost("/pocepat/{processId}/graft-close", GraftCloseHandle)
              .WithTags("POCEPAT").WithSummary("Graft-real — close the graft window");

        // Node 18 (erro): PRPINTPC app-error suspends for an operator decision; OUTCOME=R retries.
        routes.MapPost("/pocepat/{processId}/operator-decision", (string processId, IStimulusSender s, CancellationToken ct)
            => ResumeAsync(processId, PocEpatMainActivity.BkOperatorDecision, s, ct))
              .WithTags("POCEPAT").WithSummary("MANEXC operator decision (OUTCOME=R → retry PRPINTPC)");

        return routes;
    }

    private static async Task<IResult> StartHandle(
        PocEpatStartRequest request, IWorkflowRuntime workflowRuntime, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProcessId))
            return Results.BadRequest("ProcessId is required.");

        var client = await workflowRuntime.CreateClientAsync(ct);
        var result = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(PocEpatMainElsaWorkflow.DefinitionId),
                CorrelationId = request.ProcessId,
                Input = new Dictionary<string, object>
                {
                    ["ProcessId"] = request.ProcessId,
                    ["IdAiim"] = request.IdAiim,
                    ["ExisteNotificacao"] = request.ExisteNotificacao,
                    ["GraftMode"] = request.GraftMode,
                    ["PrpintpcFails"] = request.PrpintpcFails,
                    ["DecisionsSeed"] = System.Text.Json.JsonSerializer.Serialize(request.DecisionsSeed ?? new()),
                },
            },
            ct);

        return Results.Ok(new
        {
            InstanceId = result.WorkflowInstanceId,
            Status = result.Status.ToString(),
            SubStatus = result.SubStatus.ToString(),
            CorrelationId = request.ProcessId,
            Hint = $"POST /pocepat/{request.ProcessId}/iniciar-novo-graft to begin the SC-001 walk."
        });
    }

    private static async Task<IResult> PrepararNotificacaoHandle(
        string processId, PocEpatCorrecaoRequest request,
        PocEpatProcessState state, IStimulusSender sender, CancellationToken ct)
    {
        var snap = state.Load(processId);
        if (snap is null) return Results.NotFound($"No POC_EpatProcess instance for '{processId}'.");
        snap.Case.CORRECAO = request.Correcao;
        state.Save(processId, snap); // persist-then-resume: o callback recarrega o snapshot atualizado
        return await ResumeAsync(processId, PocEpatMainActivity.BkPrepararNotificacao, sender, ct);
    }

    private static async Task<IResult> FinalizarAiimHandle(
        string processId, PocEpatAfrRequest request,
        PocEpatProcessState state, IStimulusSender sender, CancellationToken ct)
    {
        var snap = state.Load(processId);
        if (snap is null) return Results.NotFound($"No POC_EpatProcess instance for '{processId}'.");
        snap.PendingAfrName = request.AfrName;
        state.Save(processId, snap); // persist-then-resume
        return await ResumeAsync(processId, PocEpatMainActivity.BkFinalizarAiim, sender, ct);
    }

    private static async Task<IResult> VerificarRetornoHandle(
        string processId, PocEpatTipoVistasRequest request,
        PocEpatProcessState state, IStimulusSender sender, CancellationToken ct)
    {
        var snap = state.Load(processId);
        if (snap is null) return Results.NotFound($"No POC_EpatProcess instance for '{processId}'.");
        snap.Case.TIPOVISTAS = FieldValue<string>.Of(request.TipoVistas);
        state.Save(processId, snap); // persist-then-resume
        return await ResumeAsync(processId, PocEpatMainActivity.BkVerificarRetorno, sender, ct);
    }

    private static async Task<IResult> ResumeAsync(
        string processId, string bookmark, IStimulusSender sender, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(processId))
            return Results.BadRequest("processId is required.");

        await sender.SendAsync(
            activityTypeName: bookmark,
            stimulus: new PocEpatStimulus(processId),
            metadata: new StimulusMetadata { CorrelationId = processId },
            cancellationToken: ct);

        return Results.Accepted();
    }

    private static async Task<IResult> GraftAttachHandle(
        string processId, GraftChildRequest request, InMemoryGraftJoin graft, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ChildId)) return Results.BadRequest("childId is required.");
        await graft.AttachAsync(processId, request.ChildId, ct);
        var (att, comp) = graft.Snapshot(processId);
        Console.WriteLine($"[POCEPAT][GRAFT] filho '{request.ChildId}' ANEXADO. Estado: {comp}/{att}.");
        return Results.Accepted(value: new { processId, request.ChildId, attached = att, completed = comp });
    }

    private static async Task<IResult> GraftCompleteHandle(
        string processId, GraftChildRequest request,
        InMemoryGraftJoin graft, IStimulusSender sender, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ChildId)) return Results.BadRequest("childId is required.");
        await graft.SignalCompletedAsync(processId, request.ChildId, ct);
        var (att, comp) = graft.Snapshot(processId);
        Console.WriteLine($"[POCEPAT][GRAFT] filho '{request.ChildId}' CONCLUÍDO. Estado: {comp}/{att}.");
        await GraftTryProceedAsync(processId, graft, sender, ct);
        return Results.Accepted(value: new { processId, request.ChildId, attached = att, completed = comp });
    }

    private static async Task<IResult> GraftCloseHandle(
        string processId, InMemoryGraftJoin graft, IStimulusSender sender, CancellationToken ct)
    {
        graft.Close(processId);
        var (att, comp) = graft.Snapshot(processId);
        Console.WriteLine($"[POCEPAT][GRAFT] janela FECHADA. Estado: {comp}/{att}.");
        await GraftTryProceedAsync(processId, graft, sender, ct);
        return Results.Accepted(value: new { processId, closed = true, attached = att, completed = comp });
    }

    private static async Task GraftTryProceedAsync(
        string processId, InMemoryGraftJoin graft, IStimulusSender sender, CancellationToken ct)
    {
        if (!graft.IsReadyToProceed(processId)) return;
        await sender.SendAsync(
            activityTypeName: PocEpatMainActivity.BkGraftProceed,
            stimulus: new PocEpatStimulus(processId),
            metadata: new StimulusMetadata { CorrelationId = processId },
            cancellationToken: ct);
    }
}

public sealed record PocEpatStartRequest(string ProcessId, long IdAiim, bool ExisteNotificacao = false, bool GraftMode = false, bool PrpintpcFails = false, Dictionary<string, string?>? DecisionsSeed = null);
public sealed record PocEpatCorrecaoRequest(bool Correcao);
public sealed record PocEpatAfrRequest(string? AfrName);
public sealed record PocEpatTipoVistasRequest(string TipoVistas);
