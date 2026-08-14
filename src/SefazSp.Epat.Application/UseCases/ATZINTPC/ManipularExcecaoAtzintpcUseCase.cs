#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.UseCases.ATZINTPC;

/// <summary>
/// Caso de uso para a userTask 'Manipular Excecao' (_RNdJ0V6PEfGBBLgT-R5iuw)
/// do processo ATZINTPC, passo 17 do segmento 041 (SC-ATZINTPC-009).
///
/// Activada quando o laço de retry se esgota (gateway More Retries, ramo OTHERWISE).
/// O operador avalia o estado do caso e decide:
///   - Repetir a chamada de serviço (OUTCOME = 'R')
///   - Considerar o caso resolvido manualmente (OUTCOME = 'OK')
///
/// O resultado actualiza ProcessExecutionContext.OUTCOME, lido a seguir
/// pelo gateway Manually Fixed (_RNdJy16PEfGBBLgT-R5iuw).
///
/// Card: BUILD-ATZINTPC-seg041 · AC6
/// </summary>
public sealed class ManipularExcecaoAtzintpcUseCase
{
    /// <summary>
    /// Aguarda a decisão do operador e aplica-a ao contexto de execução.
    /// </summary>
    /// <param name="caseRef">Referência do caso (para apresentação na UI).</param>
    /// <param name="ctx">Contexto de execução mutável — OUTCOME é actualizado aqui.</param>
    /// <param name="decideOutcome">
    /// Delegate que representa a interação humana. Em produção, suspende o workflow
    /// até o operador submeter o formulário MANEXC. Em testes, substituído por um
    /// valor configurado no cenário.
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
