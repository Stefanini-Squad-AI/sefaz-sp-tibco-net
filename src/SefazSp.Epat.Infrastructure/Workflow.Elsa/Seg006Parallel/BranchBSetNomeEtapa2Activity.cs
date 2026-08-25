#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using SefazSp.Epat.Infrastructure.Runtime;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Seg006Parallel;

/// <summary>
/// Ramo B do AND-split: scriptTask 'Set Nome Etapa 2' (<c>_XWivF1qTEfG5K7mY0I3I6w</c>).
/// Nó síncrono — não suspende. O AND-split dispara este ramo incondicionalmente, em paralelo
/// com o ramo A (gateway 'Existe Notificação?').
/// </summary>
[Activity("Epat", "SEG006", "Ramo B — scriptTask 'Set Nome Etapa 2'.")]
public class BranchBSetNomeEtapa2Activity : Activity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var store = context.GetRequiredService<Seg006StateStore>();
        var correlationKey = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        _ = store.Load(correlationKey);

        Console.WriteLine("[SEG006][RAMO B] scriptTask 'Set Nome Etapa 2' executado (ramo incondicional do AND-split).");

        await context.CompleteActivityAsync();
    }
}
