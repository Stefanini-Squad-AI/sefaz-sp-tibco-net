#nullable enable

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Workflows.CONTROPC;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Integration.Doubles;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa.Controlopc;

/// <summary>
/// Tradução Elsa do segmento CONTROPC-seg039 ('Aguardar Retorno' → 'Desativa Subs' → regresso).
/// Demonstra o dynamic-subprocess (interface-registry-validated): o destino é resolvido em runtime
/// pelo valor de <c>AGUARDAR[IDX_AGUARDAR]</c> via <see cref="AGUARDARRegistry"/> (Keyed DI, .NET 8).
/// Não suspende — a resolução e a chamada ao subprocesso são síncronas.
/// AgPecas resolve para AGPECASPC (sucesso); os 6 destinos de pacote externo falham VISIVELMENTE.
/// </summary>
[Activity("Epat", "CONTROPC", "callActivity 'Aguardar Retorno' — subprocesso dinâmico resolvido em runtime.")]
public class ControlopcElsaActivity : Activity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var processId = context.GetWorkflowInput<string>("ProcessId");
        var idAiim    = context.GetWorkflowInput<long>("IdAiim");
        var aguardar  = context.GetWorkflowInput<string>("Aguardar");

        var registry = context.GetRequiredService<AGUARDARRegistry>();
        var caso = new AiimCase { AGUARDAR = new[] { aguardar }, IDX_AGUARDAR = 0 };
        var caseRef = new AiimCaseRef(idAiim, processId);

        Console.WriteLine($"[CONTROPC] 'Aguardar Retorno' — resolvendo destino AGUARDAR[0]='{aguardar}'…");

        var workflow = new ControlopcSeg039Workflow(registry.Resolve);
        try
        {
            var endEvent = await workflow.ExecuteAsync(caso, caseRef, context.CancellationToken);
            Console.WriteLine(
                $"[CONTROPC] 'Desativa Subs' → STATUSSUBPROC limpo; regresso ao chamador (endEvent {endEvent}).");
        }
        catch (InvalidOperationException ex)
        {
            // interface-registry-validated: destino sem implementação (ou pacote externo) falha VISIVELMENTE.
            Console.WriteLine($"[CONTROPC] FALHA VISÍVEL (destino '{aguardar}'): {ex.Message}");
        }

        await context.CompleteActivityAsync();
    }
}
