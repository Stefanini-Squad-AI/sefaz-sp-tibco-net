#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.UseCases.PocEpatProcess;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Runtime;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Seg006Parallel;

/// <summary>
/// userTask 'Finalizar AIIM' (<c>_xWNLe1qSEfG5K7mY0I3I6w</c>) do segmento 006.
/// Suspende num bookmark correlacionado por PROCESS_ID; ao ser retomado aplica a regra
/// <c>RI-formScript-POC_EpatProcess-FinalizarAIIM</c> (via <see cref="FinalizarAiimUseCase"/>)
/// e completa — o fluxo segue então para o AND-split paralelo.
/// </summary>
[Activity("Epat", "SEG006", "userTask 'Finalizar AIIM' — suspende até submissão (bookmark-correlation).")]
public class FinalizarAiimActivity : Activity
{
    /// <summary>Nome do bookmark 'Finalizar AIIM' — partilhado com o endpoint de retoma.</summary>
    public const string FinalizarAiimBookmarkName = "seg006-finalizar-aiim";

    protected override ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var processId         = context.GetWorkflowInput<string>("ProcessId");
        var idAiim            = context.GetWorkflowInput<long>("IdAiim");
        var afrName           = context.GetWorkflowInput<string>("AfrName");
        var existeNotificacao = context.GetWorkflowInput<bool>("ExisteNotificacao");

        var store = context.GetRequiredService<Seg006StateStore>();
        store.Save(processId, new Seg006Snapshot(idAiim, processId, new AiimCase(), afrName, existeNotificacao));

        Console.WriteLine("[SEG006] suspenso em 'Finalizar AIIM' — aguarda submissão (bookmark-correlation)…");
        context.CreateBookmark(new CreateBookmarkArgs
        {
            BookmarkName = FinalizarAiimBookmarkName,
            Stimulus = new FinalizarAiimStimulus(processId),
            Callback = OnFinalizarResumedAsync,
            IncludeActivityInstanceId = false,
        });
        return default;
    }

    private async ValueTask OnFinalizarResumedAsync(ActivityExecutionContext context)
    {
        var store = context.GetRequiredService<Seg006StateStore>();
        var correlationKey = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        var snap = store.Load(correlationKey);
        if (snap is null) { await context.CompleteActivityAsync(); return; }

        // RI-formScript-POC_EpatProcess-FinalizarAIIM: AFR = GETATTRIBUTE("Name"); CNTINSTANCIASUF = 0.
        // A suspensão já ocorreu no bookmark; o delegate de submissão devolve imediatamente.
        var useCase = new FinalizarAiimUseCase();
        var caseRef = new AiimCaseRef(snap.IdAiim, snap.ProcessId);
        await useCase.ExecuteAsync(
            caseRef,
            snap.Case,
            (_, _) => Task.FromResult<Func<string, string>>(attr => attr == "Name" ? snap.AfrName : string.Empty),
            context.CancellationToken);
        store.Save(correlationKey, snap);

        Console.WriteLine(
            $"[SEG006] 'Finalizar AIIM' submetido → AFR='{snap.Case.AFR}', " +
            $"CNTINSTANCIASUF={snap.Case.CNTINSTANCIASUF}. AND-split paralelo a seguir (fires both).");
        await context.CompleteActivityAsync();
    }
}
