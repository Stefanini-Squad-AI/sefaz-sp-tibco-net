#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.UseCases.CALCPRPC;

/// <summary>
/// Caso de uso para a userTask 'Manipular Excecao' (_zJIHXVqiEfG5K7mY0I3I6w).
/// </summary>
public sealed class ManipularExcecaoCalcprpcUseCase
{
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoCalcprpcResult>> decideOutcome,
        CancellationToken ct)
    {
        var result = await decideOutcome(caseRef, ct).ConfigureAwait(false);

        ctx.OUTCOME = result switch
        {
            ManipularExcecaoCalcprpcResult.RetryAgain => "R",
            ManipularExcecaoCalcprpcResult.ManuallyFixed => "OK",
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }
}
