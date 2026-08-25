#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using SefazSp.Epat.Infrastructure.Runtime;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Seg006Parallel;

/// <summary>
/// Ramo A do AND-split: gateway XOR 'Existe Notificação?' (<c>_IxqJMlqTEfG5K7mY0I3I6w</c>).
/// Avalia <c>EXISTENOTIFICAC == true</c> → ramo "Sim"; caso contrário → ramo "No" (OTHERWISE).
/// Nó síncrono — não suspende (faithful: o segmento 006 não tem ponto de espera aqui).
/// </summary>
[Activity("Epat", "SEG006", "Ramo A — gateway XOR 'Existe Notificação?'.")]
public class BranchAExisteNotificacaoActivity : Activity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var store = context.GetRequiredService<Seg006StateStore>();
        var correlationKey = context.WorkflowExecutionContext.CorrelationId ?? string.Empty;
        var snap = store.Load(correlationKey);

        if (snap is not null)
        {
            snap.Case.EXISTENOTIFICAC = snap.ExisteNotificacao;
            store.Save(correlationKey, snap);

            // RI-transition-POC_EpatProcess: Sim = EXISTENOTIFICAC == true; No = OTHERWISE.
            var branch = snap.Case.EXISTENOTIFICAC
                ? "Sim → _Faq_Q1qTEfG5K7mY0I3I6w (EXISTENOTIFICAC == true)"
                : "No → _Faq_RVqTEfG5K7mY0I3I6w (OTHERWISE)";
            Console.WriteLine($"[SEG006][RAMO A] gateway 'Existe Notificação?' (XOR) → {branch}");
        }

        await context.CompleteActivityAsync();
    }
}
