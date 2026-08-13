#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.CALCPRPC;
using SefazSp.Epat.Application.UseCases.CALCPRPC;
using SefazSp.Epat.Domain.Rules;

namespace SefazSp.Epat.Application.Workflows.CALCPRPC;

public enum CalcprpcSeg029Outcome
{
    Success,
    DoneBail,
}

/// <summary>
/// Workflow do segmento 029 de CALCPRPC: de 'Start Event' a 'Done - Bail'.
///
/// A descida para o startEvent interno (_zJIublqiEfG5K7mY0I3I6w) e o regresso para
/// o gateway Tech Error (_zJIHZVqiEfG5K7mY0I3I6w) sao arestas explicitas em .NET,
/// porque nao existem como transicoes XPDL.
/// </summary>
public sealed class CalcprpcSeg029Workflow
{
    private readonly ICalcularPrazoService _calcularPrazo;
    private readonly ManipularExcecaoCalcprpcUseCase _manipularExcecao;

    public CalcprpcSeg029Workflow(
        ICalcularPrazoService calcularPrazo,
        ManipularExcecaoCalcprpcUseCase manipularExcecao)
    {
        _calcularPrazo = calcularPrazo;
        _manipularExcecao = manipularExcecao;
    }

    public async Task<CalcprpcSeg029Outcome> ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        long swQRetryCount,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoCalcprpcResult>> decideManipularExcecao,
        CancellationToken ct)
    {
        // passo 1 → 2
        CalcprpcExecutionSteps.ApplySetParameters(ctx);

        TryTaskEntry:

        // passo 3
        CalcprpcExecutionSteps.ApplyStartLoop(ctx);

        // passo 4 → 5: DESCIDA explicita para o startEvent interno _zJIublqiEfG5K7mY0I3I6w.
        // Nao existe transicao XPDL; a aresta e escrita explicitamente neste workflow.

        // passo 6
        CalcprpcExecutionSteps.ApplyStartTx(ctx);

        // passo 7
        if (!CalcprpcCheckRetriesRule.IsStillgood(swQRetryCount, ctx.MAXRETRIES))
        {
            // Ramo Maxretriesexceeded do ActivitySet: Set Technical Error → endEvent interno.
            CalcprpcExecutionSteps.SetTechError(ctx, "Maxretriesexceeded");
        }
        else
        {
            try
            {
                // passo 8
                var envelope = await _calcularPrazo
                    .CalcularPrazoAsync(caseRef, ct)
                    .ConfigureAwait(false);

                // passo 9
                if (envelope.STATUS_CODE == "0")
                {
                    // passo 11 → 12
                    CalcprpcExecutionSteps.MapServiceEnvelope(ctx, envelope);
                }
                else
                {
                    // passo 10
                    CalcprpcExecutionSteps.SetAppError(ctx, envelope);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // REGRESSO explicito: sem transicao XPDL, o catch liga o subprocesso
                // ao gateway Tech Error _zJIHZVqiEfG5K7mY0I3I6w.
                CalcprpcExecutionSteps.SetTechError(ctx, ex.Message);
            }
        }

        // passo 13 — Tech Error
        if (ctx.ISTECHERROR == "Y")
        {
            // passo 16 — gateway anonimo de convergencia
            await _manipularExcecao
                .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
                .ConfigureAwait(false);

            // passo 18
            if (ctx.OUTCOME == "OK")
                return CalcprpcSeg029Outcome.DoneBail;

            // passo 19
            if (ctx.OUTCOME == "R")
                goto TryTaskEntry;

            return CalcprpcSeg029Outcome.DoneBail;
        }

        // passo 14 — App Error
        if (!CalcprpcExecutionSteps.IsAppErrorFlag(ctx))
            return CalcprpcSeg029Outcome.Success;

        // passo 15 — More Retries
        if (CalcprpcExecutionSteps.HasMoreRetries(ctx))
            goto TryTaskEntry;

        // passo 16 → 17
        await _manipularExcecao
            .ExecuteAsync(caseRef, ctx, decideManipularExcecao, ct)
            .ConfigureAwait(false);

        // passo 18
        if (ctx.OUTCOME == "OK")
            return CalcprpcSeg029Outcome.DoneBail;

        // passo 19
        if (ctx.OUTCOME == "R")
            goto TryTaskEntry;

        // passo 20
        return CalcprpcSeg029Outcome.DoneBail;
    }
}
