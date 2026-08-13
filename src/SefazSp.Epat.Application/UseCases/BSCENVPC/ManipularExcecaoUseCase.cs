#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.UseCases.BSCENVPC;

/// <summary>
/// Caso de uso para a userTask 'Manipular Excecao' (_qIDunF6BEfGBBLgT-R5iuw).
///
/// Esta tarefa humana e activada quando o laco de retry se esgota
/// (gateway More Retries, ramo OTHERWISE). O operador avalia o estado
/// do caso e decide:
///   - Repetir a chamada de servico (OUTCOME = 'R')
///   - Considerar o caso resolvido manualmente (OUTCOME = 'OK')
///
/// O resultado actualiza o ProcessExecutionContext.OUTCOME, que e lido
/// a seguir pelo gateway Manually Fixed (_qIDull6BEfGBBLgT-R5iuw).
/// </summary>
public sealed class ManipularExcecaoUseCase
{
    /// <summary>
    /// Aguarda a decisao do operador e aplica-a ao contexto de execucao.
    /// </summary>
    /// <param name="caseRef">Referencia do caso (para apresentacao na UI).</param>
    /// <param name="ctx">Contexto de execucao mutavel — OUTCOME e actualizado aqui.</param>
    /// <param name="decideOutcome">
    /// Delegate que representa a interacao humana: recebe a referencia do caso
    /// e devolve a decisao do operador. Em producao, este delegate suspende o
    /// workflow ate o operador submeter o formulario MANEXC. Em testes, e
    /// substituido por um valor configurado no cenario.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoResult>> decideOutcome,
        CancellationToken ct)
    {
        var result = await decideOutcome(caseRef, ct).ConfigureAwait(false);

        ctx.OUTCOME = result switch
        {
            ManipularExcecaoResult.RetryAgain    => "R",
            ManipularExcecaoResult.ManuallyFixed => "OK",
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }
}
