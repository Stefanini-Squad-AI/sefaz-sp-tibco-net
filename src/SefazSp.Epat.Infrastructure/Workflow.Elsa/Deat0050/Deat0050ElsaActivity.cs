#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Infrastructure.Runtime;
using AppWorkflows = SefazSp.Epat.Application.Workflows;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Deat0050;

/// <summary>
/// Tradução Elsa do subprocesso DEAT0050. Duas suspensões sequenciais:
///   1. INICALC (receiveTask) — evento externo, bookmark correlacionado por PROCESS_ID.
///   2. Aguarda Defesa (timerEvent) — timer até ao instante absoluto (Elsa Scheduling).
///
/// Após INICALC: CalculaPrazo → HoraFimSC → gateway → [Aguarda Defesa → Controlar Data → gateway].
/// A lógica de cada nó vem dos métodos testados de <see cref="AppWorkflows.Deat0050Workflow"/>.
/// </summary>
[Activity("Epat", "DEAT0050", "Subprocesso DEAT0050 com evento externo (INICALC) e timer (Aguarda Defesa).")]
public class Deat0050ElsaActivity : Activity
{
    /// <summary>Nome do bookmark INICALC — partilhado com o endpoint de retoma.</summary>
    public const string InicalcBookmarkName = "deat0050-inicalc";

    protected override ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var processId    = context.GetWorkflowInput<string>("ProcessId");
        var idAiim       = context.GetWorkflowInput<long>("IdAiim");
        var demoDeadline = context.GetWorkflowInput<int>("DemoDeadlineSeconds");

        var store = context.GetRequiredService<Deat0050StateStore>();
        store.Save(processId, new Deat0050Snapshot(
            idAiim, processId, new Domain.Cases.AiimCase(), new Application.Execution.ProcessExecutionContext(), demoDeadline));

        // Suspensão #1 — INICALC (evento externo).
        Console.WriteLine("[DEAT0050] suspenso em INICALC — aguarda evento externo (bookmark-correlation)…");
        context.CreateBookmark(new CreateBookmarkArgs
        {
            BookmarkName = InicalcBookmarkName,
            Stimulus = new InicalcStimulus(processId),
            Callback = OnInicalcResumedAsync,
            IncludeActivityInstanceId = false,
        });
        return default;
    }

    private async ValueTask OnInicalcResumedAsync(ActivityExecutionContext context)
    {
        var store = context.GetRequiredService<Deat0050StateStore>();
        var correlationKey = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        var snap = store.Load(correlationKey);
        if (snap is null) { await context.CompleteActivityAsync(); return; }

        var wf = ResolveWorkflow(context);
        var caseRef = new AiimCaseRef(snap.IdAiim, snap.ProcessId);

        // Passo 2 — CalculaPrazo (callActivity, INOTFAIIM).
        await wf.ExecuteCalculaPrazoAsync(caseRef, context.CancellationToken);

        // Passo 3 — HoraFimSC (compute PRAZODEFESA/PRAZODEFESAT via CALCTIME shim + IClock).
        wf.ExecuteHoraFimSc(snap.Case, snap.Case.DAYSOVER);
        store.Save(correlationKey, snap);
        Console.WriteLine("[DEAT0050] INICALC retomado → CalculaPrazo + HoraFimSC concluídos.");

        await EvaluateGatewayAsync(context, wf, snap, correlationKey);
    }

    private async ValueTask EvaluateGatewayAsync(
        ActivityExecutionContext context, AppWorkflows.Deat0050Workflow wf, Deat0050Snapshot snap, string correlationKey)
    {
        // Passo 4 — gateway "Já se esperou pelo prazo em vigor?".
        if (!AppWorkflows.Deat0050Workflow.GatewayDeveAguardarDefesa(snap.Case))
        {
            Console.WriteLine("[DEAT0050] gateway: DATACONTROLE == PRAZODEFESA → endEvent (concluído).");
            context.GetRequiredService<Deat0050StateStore>().Clear(correlationKey);
            await context.CompleteActivityAsync();
            return;
        }

        // Passo 5 — Aguarda Defesa (timerEvent → instante absoluto).
        var instant = wf.CalcularInstanteAguardaDefesa(snap.Case);
        var demoDelay = TimeSpan.FromSeconds(Math.Max(1, snap.DemoDeadlineSeconds));
        var opts = context.GetRequiredService<DeadlineDemoOptions>();
        var clock = context.GetRequiredService<IClock>();
        var delay = opts.DelayTo(instant, clock, demoDelay);
        Console.WriteLine(
            $"[DEAT0050] Aguarda Defesa: instante={instant:o}; demo={opts.Enabled}; delay={delay} " +
            $"(mesmo mecanismo; ON encurta para o smoke test, OFF dispara no instante calculado).");
        context.DelayFor(delay, OnTimerFiredAsync);
    }

    private async ValueTask OnTimerFiredAsync(ActivityExecutionContext context)
    {
        var store = context.GetRequiredService<Deat0050StateStore>();
        var correlationKey = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        var snap = store.Load(correlationKey);
        if (snap is null) { await context.CompleteActivityAsync(); return; }

        // Passo 6 — Controlar Data (DATACONTROLE = PRAZODEFESA; sentinela que encerra o laço).
        AppWorkflows.Deat0050Workflow.ExecuteControlarData(snap.Case);
        store.Save(correlationKey, snap);
        Console.WriteLine("[DEAT0050] timer disparou → Controlar Data (DATACONTROLE = PRAZODEFESA).");

        await EvaluateGatewayAsync(context, ResolveWorkflow(context), snap, correlationKey);
    }

    private static AppWorkflows.Deat0050Workflow ResolveWorkflow(ActivityExecutionContext context)
    {
        var calculaPrazo = context.GetRequiredService<INOTFAIIM>();
        var clock = context.GetRequiredService<IClock>();
        return new AppWorkflows.Deat0050Workflow(calculaPrazo, clock);
    }
}
