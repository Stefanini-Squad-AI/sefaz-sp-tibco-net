#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.UseCases.CRNOTPC;

/// <summary>
/// Caso de uso para a userTask 'Manipular Excecao' (_NcJJ6V9KEfGqPfX31TKC3w)
/// do processo CRNOTPC.
///
/// Esta tarefa humana é ativada quando o laço de retry se esgota
/// (gateway More Retries, ramo OTHERWISE / gateway _NcJw8V9KEfGqPfX31TKC3w).
/// O operador avalia o estado do caso e decide:
///   - Repetir a chamada de serviço (OUTCOME = 'R')
///   - Considerar o caso resolvido manualmente (OUTCOME = 'OK')
///
/// O resultado atualiza o ProcessExecutionContext.OUTCOME, que é lido
/// a seguir pelo gateway Manually Fixed (_NcJJ419KEfGqPfX31TKC3w).
/// </summary>
public sealed class ManipularExcecaoUseCase
{
    /// <summary>
    /// Aguarda a decisão do operador e aplica-a ao contexto de execução.
    /// </summary>
    /// <param name="caseRef">Referência do caso (para apresentação na UI).</param>
    /// <param name="ctx">Contexto de execução mutável — OUTCOME é atualizado aqui.</param>
    /// <param name="decideOutcome">
    /// Delegate que representa a interação humana: recebe a referência do caso
    /// e devolve a decisão do operador. Em produção, este delegate suspende o
    /// workflow até o operador submeter o formulário MANEXC. Em testes, é
    /// substituído por um valor configurado no cenário.
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
