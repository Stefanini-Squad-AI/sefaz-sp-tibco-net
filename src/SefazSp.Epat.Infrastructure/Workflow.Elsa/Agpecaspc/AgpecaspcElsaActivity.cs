#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Runtime;
using AppWorkflows = SefazSp.Epat.Application.Workflows.AGPECASPC;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Agpecaspc;

/// <summary>
/// Tradução Elsa do subprocesso AGPECASPC. O nó 'Aguardar Interposições' suspende com DUAS
/// formas de acordar em corrida: o evento externo (interposições) e um timer de fronteira (1h).
/// A primeira a disparar resolve; a outra não age (guarda <see cref="AgpecaspcSnapshot.Resolved"/>).
///
/// Laço: Set Values → gateway "Já se esperou?" → SetPrazo → Aguardar Interposições [corrida]
///       → (evento) Controla Datas | (timer) Set Flag Decurso → Controla Datas → volta ao gateway.
/// A lógica de cada nó vem dos métodos testados de <see cref="AppWorkflows.AgpecaspcSeg040Workflow"/>.
/// </summary>
[Activity("Epat", "AGPECASPC", "Subprocesso AGPECASPC com corrida evento⇄timer em Aguardar Interposições.")]
public class AgpecaspcElsaActivity : Activity
{
    /// <summary>Nome do bookmark do evento de interposições — partilhado com o endpoint de retoma.</summary>
    public const string InterposicoesBookmarkName = "agpecaspc-interposicoes";

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var processId    = context.GetWorkflowInput<string>("ProcessId");
        var idAiim       = context.GetWorkflowInput<long>("IdAiim");
        var demoTimer    = context.GetWorkflowInput<int>("DemoTimerSeconds");

        var store = context.GetRequiredService<AgpecaspcStateStore>();
        var caseData = new AiimCase();
        store.Save(processId, new AgpecaspcSnapshot
        {
            IdAiim = idAiim, ProcessId = processId, Case = caseData,
            DemoTimerSeconds = demoTimer <= 0 ? 3 : demoTimer,
        });

        // Passo 2 — Set Values (scriptTask). Condição de domínio + envelope técnico.
        var existePeca = AppWorkflows.AgpecaspcSeg040Workflow.ExecuteSetValues(caseData);
        Console.WriteLine($"[AGPECASPC] Set Values → existePeça={existePeca}");

        await EvaluateGatewayAsync(context, store, processId);
    }

    private async ValueTask EvaluateGatewayAsync(
        ActivityExecutionContext context, AgpecaspcStateStore store, string correlationKey)
    {
        var snap = store.Load(correlationKey);
        if (snap is null) { await context.CompleteActivityAsync(); return; }

        // Passo 4 — gateway "Já se esperou pelo prazo em vigor?".
        if (!AppWorkflows.AgpecaspcSeg040Workflow.GatewayDeveAguardarInterposicoes(snap.Case))
        {
            Console.WriteLine("[AGPECASPC] gateway: DATACONTROLE == PRAZORECEBIMENT → saída do ciclo (concluído).");
            store.Clear(correlationKey);
            await context.CompleteActivityAsync();
            return;
        }

        // Passo 5 — SetPrazo (scriptTask).
        AppWorkflows.AgpecaspcSeg040Workflow.ExecuteSetPrazo(snap.Case);

        // Passo 6 — Aguardar Interposições: corrida evento externo ⇄ timer de fronteira.
        snap.Resolved = false;
        store.Save(correlationKey, snap);

        var wf = ResolveWorkflow(context);
        var instant = wf.CalcularInstanteTimerBoundary();
        var demoDelay = TimeSpan.FromSeconds(Math.Max(1, snap.DemoTimerSeconds));
        var opts = context.GetRequiredService<DeadlineDemoOptions>();
        var clock = context.GetRequiredService<IClock>();
        var delay = opts.DelayTo(instant, clock, demoDelay);
        Console.WriteLine(
            $"[AGPECASPC] Aguardar Interposições — corrida: evento externo OU timer " +
            $"(instante={instant:o}; demo={opts.Enabled}; delay={delay}).");

        // A) bookmark do evento externo.
        context.CreateBookmark(new CreateBookmarkArgs
        {
            BookmarkName = InterposicoesBookmarkName,
            Stimulus = new InterposicoesStimulus(correlationKey),
            Callback = OnEventArrivedAsync,
            IncludeActivityInstanceId = false,
        });

        // B) timer de fronteira.
        context.DelayFor(delay, OnTimerFiredAsync);
    }

    private async ValueTask OnEventArrivedAsync(ActivityExecutionContext context)
    {
        var (store, snap, correlationKey) = LoadState(context);
        if (snap is null || snap.Resolved) return; // corrida já resolvida pelo timer.
        snap.Resolved = true;
        store.Save(correlationKey, snap);

        Console.WriteLine("[AGPECASPC] evento de interposições chegou primeiro → Controla Datas.");
        // Passo 9 — Controla Datas (DATACONTROLE = PRAZORECEBIMENT).
        AppWorkflows.AgpecaspcSeg040Workflow.ExecuteControlaDatas(snap.Case);
        store.Save(correlationKey, snap);

        await EvaluateGatewayAsync(context, store, correlationKey);
    }

    private async ValueTask OnTimerFiredAsync(ActivityExecutionContext context)
    {
        var (store, snap, correlationKey) = LoadState(context);
        if (snap is null || snap.Resolved) return; // corrida já resolvida pelo evento.
        snap.Resolved = true;
        store.Save(correlationKey, snap);

        Console.WriteLine("[AGPECASPC] timer de fronteira disparou primeiro → Set Flag Decurso (FLGTERMODEC=true) → Controla Datas.");
        // Passo 8 — Set Flag Decurso (FLGTERMODEC = true).
        AppWorkflows.AgpecaspcSeg040Workflow.ExecuteSetFlagDecurso(snap.Case);
        // Passo 9 — Controla Datas.
        AppWorkflows.AgpecaspcSeg040Workflow.ExecuteControlaDatas(snap.Case);
        store.Save(correlationKey, snap);

        await EvaluateGatewayAsync(context, store, correlationKey);
    }

    private static (AgpecaspcStateStore store, AgpecaspcSnapshot? snap, string key) LoadState(ActivityExecutionContext context)
    {
        var store = context.GetRequiredService<AgpecaspcStateStore>();
        var key = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        return (store, store.Load(key), key);
    }

    private static AppWorkflows.AgpecaspcSeg040Workflow ResolveWorkflow(ActivityExecutionContext context)
        => new(context.GetRequiredService<IClock>());
}
