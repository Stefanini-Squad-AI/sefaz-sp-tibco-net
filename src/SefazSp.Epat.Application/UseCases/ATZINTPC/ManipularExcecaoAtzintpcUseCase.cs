#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.UseCases.ATZINTPC;

/// <summary>
/// Caso de uso para a userTask 'Manipular Excecao' (_RNdJ0V6PEfGBBLgT-R5iuw).
///
/// Esta tarefa humana e activada quando o laco de retry se esgota
/// (gateway More Retries, ramo OTHERWISE). O operador avalia o estado
/// do caso e decide:
///   - Repetir a chamada de servico (OUTCOME = 'R')
///   - Considerar o caso resolvido manualmente (OUTCOME = 'OK')
///
/// O resultado actualiza o ProcessExecutionContext.OUTCOME, que e lido
/// a seguir pelo gateway Manually Fixed (_RNdJy16PEfGBBLgT-R5iuw).
/// </summary>
public sealed class ManipularExcecaoAtzintpcUseCase
{
    /// <summary>
    /// Aguarda a decisao do operador e aplica-a ao contexto de execucao.
    /// </summary>
    /// <param name="caseRef">Referencia do caso (para apresentacao na UI).</param>
    /// <param name="ctx">Contexto de execucao mutavel — OUTCOME e actualizado aqui.</param>
    /// <param name="decideOutcome">
    /// Delegate que representa a interacao humana: recebe a referencia do caso
    /// e devolve a decisao do operador.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoAtzintpcResult>> decideOutcome,
        CancellationToken ct)
    {
        var result = await decideOutcome(caseRef, ct).ConfigureAwait(false);

        ctx.OUTCOME = result switch
        {
            ManipularExcecaoAtzintpcResult.RetryAgain    => "R",
            ManipularExcecaoAtzintpcResult.ManuallyFixed => "OK",
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }
}
