#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using SefazSp.Epat.Infrastructure.Runtime;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Graft;

/// <summary>
/// Passo pai do graft-step ('Aguardar evento de Notificação do AIIM', Etapa 2). O pai NÃO inicia
/// os filhos: estaciona (Park) e prossegue apenas quando a janela é fechada e todos os filhos
/// anexados concluíram (correlation-join, ratificado 2026-08-06).
///
/// Mecanismo (mesma corrida guardada do AGPECASPC): DOIS bookmarks —
///   • proceed  — retomado pelos endpoints attach/complete/close quando <see cref="InMemoryGraftJoin.IsReadyToProceed"/>;
///   • timeout  — safety net (DelayFor) para um filho que nunca termina.
/// A guarda <see cref="InMemoryGraftJoin.TryResolve"/> garante que o pai prossegue uma só vez.
/// </summary>
[Activity("Epat", "GRAFT", "Passo pai do graft-step (correlation-join): estaciona e junta filhos.")]
public class GraftParentActivity : Activity
{
    /// <summary>Nome do bookmark de prosseguimento — partilhado com os endpoints attach/complete/close.</summary>
    public const string GraftProceedBookmarkName = "graft-proceed";

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var processId  = context.GetWorkflowInput<string>("ProcessId");
        var timeoutSec = context.GetWorkflowInput<int>("DemoTimeoutSeconds");

        var graft = context.GetRequiredService<InMemoryGraftJoin>();
        await graft.ParkAsync(processId, context.CancellationToken);

        Console.WriteLine(
            $"[GRAFT] pai estacionado (Park) em 'Aguardar Notificação do AIIM' — aguarda filhos " +
            $"(correlation-join), PROCESS_ID={processId}.");

        // Bookmark de prosseguimento — retomado quando a janela fecha e todos os filhos concluem.
        context.CreateBookmark(new CreateBookmarkArgs
        {
            BookmarkName = GraftProceedBookmarkName,
            Stimulus = new GraftProceedStimulus(processId),
            Callback = OnProceedAsync,
            IncludeActivityInstanceId = false,
        });

        // Safety net: um filho que nunca termina não pode prender o pai indefinidamente.
        var demo = TimeSpan.FromSeconds(Math.Max(1, timeoutSec));
        Console.WriteLine($"[GRAFT] timeout de segurança armado: {demo.TotalSeconds}s.");
        context.DelayFor(demo, OnTimeoutAsync);
    }

    private async ValueTask OnProceedAsync(ActivityExecutionContext context)
    {
        var graft = context.GetRequiredService<InMemoryGraftJoin>();
        var key = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        if (!graft.TryResolve(key)) return; // o timeout já resolveu

        var (attached, completed) = graft.Snapshot(key);
        Console.WriteLine(
            $"[GRAFT] fecho: valve close + {completed}/{attached} filhos concluídos → pai prossegue.");
        graft.Clear(key);
        await context.CompleteActivityAsync();
    }

    private async ValueTask OnTimeoutAsync(ActivityExecutionContext context)
    {
        var graft = context.GetRequiredService<InMemoryGraftJoin>();
        var key = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        if (!graft.TryResolve(key)) return; // o prosseguimento normal já resolveu

        var (attached, completed) = graft.Snapshot(key);
        Console.WriteLine(
            $"[GRAFT] TIMEOUT ({completed}/{attached} concluídos) → pai prossegue sem esperar por " +
            $"filhos em aberto (safety net).");
        graft.Clear(key);
        await context.CompleteActivityAsync();
    }
}
