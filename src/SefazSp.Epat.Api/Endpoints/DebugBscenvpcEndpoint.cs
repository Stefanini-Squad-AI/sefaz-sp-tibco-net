using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Workflows;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only endpoint: runs BscenvpcWorkflow synchronously so you can see the flow.
/// </summary>
public static class DebugBscenvpcEndpoint
{
    public static IEndpointRouteBuilder MapDebugBscenvpc(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/bscenvpc/run", Handle)
              .WithName("Debug-BSCENVPC-Run")
              .WithTags("Debug")
              .WithSummary("Runs BSCENVPC prologue synchronously and returns the execution trace");
        return routes;
    }

    private static IResult Handle(DebugBscenvpcRequest request)
    {
        var ctx = new ProcessExecutionContext();

        var branch = BscenvpcWorkflow.ExecutePrologue(
            ctx,
            idProcesso: FieldValue<long>.Of(request.IdAiim),
            processId: $"idAiim-{request.IdAiim}idProc-1",
            swQRetryCount: request.SwQRetryCount);

        return Results.Ok(new
        {
            Branch = branch,
            ctx.MAXRETRIES,
            ctx.NUMAPPRETRIES,
            ctx.PROCESS_ID,
            SwQRetryCount = request.SwQRetryCount,
            Explanation = branch == BscenvpcWorkflow.BranchStillgood
                ? "Retries OK → would proceed to call SOAP service"
                : "Retries exhausted → would go to error handling"
        });
    }
}

public sealed record DebugBscenvpcRequest(long IdAiim, long SwQRetryCount);
