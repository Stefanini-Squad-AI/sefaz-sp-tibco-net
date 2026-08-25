#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Workflows.ServiceTemplate;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.ServiceTemplate;

/// <summary>
/// Motor do molde dos 5 subprocessos de serviço. Corre a fase 1 (laço de retry); se as
/// retentativas esgotarem, suspende num bookmark (a userTask 'Manipular Excecao'), retoma
/// com a decisão do operador e aplica a fase 2 — repetindo o laço se OUTCOME == "R".
///
/// A lógica de negócio vive no <see cref="IServiceRetryTemplate"/> escolhido por ProcessKey;
/// esta activity só orquestra a suspensão/retoma (bookmark-correlation).
/// </summary>
[Activity("Epat", "ServiceTemplate", "Executa o molde de serviço com suspensão na tarefa Manipular Excecao.")]
public class ServiceRetryActivity : Activity
{
    /// <summary>Nome do bookmark de 'Manipular Excecao' — partilhado com o endpoint de retoma.</summary>
    public const string BookmarkName = "epat-manipular-excecao";

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var processKey = context.GetWorkflowInput<string>("ProcessKey");
        var processId  = context.GetWorkflowInput<string>("ProcessId");
        var idAiim     = context.GetWorkflowInput<long>("IdAiim");

        var template = ResolveTemplate(context, processKey);
        var ctx = new ProcessExecutionContext();
        template.InitializeContext(ctx, processId);

        var state = context.GetRequiredService<IServiceExecutionState>();
        state.Save(processId, new ServiceExecutionSnapshot(processKey, processId, idAiim, ctx));

        await RunPhaseAsync(context, template, new AiimCaseRef(idAiim, processId), ctx, processId);
    }

    private async ValueTask RunPhaseAsync(
        ActivityExecutionContext context, IServiceRetryTemplate template,
        AiimCaseRef caseRef, ProcessExecutionContext ctx, string correlationKey)
    {
        var outcome = await template.RunUntilOperatorAsync(caseRef, ctx, swQRetryCount: 0, context.CancellationToken);

        var state = context.GetRequiredService<IServiceExecutionState>();
        state.Save(correlationKey, new ServiceExecutionSnapshot(template.ProcessKey, correlationKey, caseRef.IdAiim, ctx));

        Console.WriteLine(
            $"[{template.ProcessKey}] loop → {outcome} " +
            $"(NUMAPPRETRIES={ctx.NUMAPPRETRIES}, MAXRETRIES={ctx.MAXRETRIES})");

        if (outcome != ServiceCallOutcome.RequiresOperator)
        {
            state.Clear(correlationKey);
            await context.CompleteActivityAsync();
            return;
        }

        // Suspende na tarefa humana: bookmark correlacionado por PROCESS_ID.
        Console.WriteLine($"[{template.ProcessKey}] retentativas esgotadas — a suspender em Manipular Excecao…");
        context.CreateBookmark(new CreateBookmarkArgs
        {
            BookmarkName = BookmarkName,
            Stimulus = new ManipularExcecaoStimulus(template.ProcessKey, correlationKey),
            Callback = OnOperatorDecidedAsync,
            IncludeActivityInstanceId = false,
        });
    }

    private async ValueTask OnOperatorDecidedAsync(ActivityExecutionContext context)
    {
        var state = context.GetRequiredService<IServiceExecutionState>();
        var inbox = context.GetRequiredService<IOperatorDecisionInbox>();

        // A correlação (PROCESS_ID) foi fixada no arranque como CorrelationId da instância.
        var correlationKey = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;

        var snapshot = state.Load(correlationKey);
        if (snapshot is null)
        {
            await context.CompleteActivityAsync();
            return;
        }

        var template = ResolveTemplate(context, snapshot.ProcessKey);
        var ctx = snapshot.Ctx;

        if (inbox.TryTake(correlationKey, out var operatorOutcome))
            ctx.OUTCOME = operatorOutcome;

        var decision = template.ApplyOperatorDecision(ctx);
        Console.WriteLine($"[{template.ProcessKey}] operador → OUTCOME={ctx.OUTCOME} → {decision}");

        if (decision == OperatorDecisionOutcome.TryAgain)
        {
            await RunPhaseAsync(context, template,
                new AiimCaseRef(snapshot.IdAiim, snapshot.ProcessId), ctx, correlationKey);
            return;
        }

        state.Clear(correlationKey);
        await context.CompleteActivityAsync();
    }

    private static IServiceRetryTemplate ResolveTemplate(ActivityExecutionContext context, string processKey)
    {
        var templates = context.GetRequiredService<IEnumerable<IServiceRetryTemplate>>();
        return templates.First(t => string.Equals(t.ProcessKey, processKey, StringComparison.OrdinalIgnoreCase));
    }
}
