#nullable enable

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.UseCases.PRPINTPC;

/// <summary>
/// Caso de uso para a userTask 'Manipular Excecao' (_KEwC5V6EEfGBBLgT-R5iuw, ordem 17).
///
/// O operador recebe o estado de erro do caso e decide:
///   • OUTCOME = 'OK' → caso resolvido manualmente (gateway Manually Fixed, _KEwC316EEfGBBLgT-R5iuw)
///   • OUTCOME = 'R'  → tentar novamente (gateway Try Again, _KEwC5F6EEfGBBLgT-R5iuw)
///
/// A implementação concreta da UI (form MANEXC) vive na camada de apresentação.
/// Este caso de uso define o contrato da camada de aplicação.
///
/// Card: BUILD-PRPINTPC-seg035 · AC7
/// Fonte TIBCO: POC_Epat.xpdl //xpdl2:Activity[@Id='_KEwC5V6EEfGBBLgT-R5iuw']
/// </summary>
public sealed class ManipularExcecaoPrpintpcUseCase
{
    /// <summary>
    /// Apresenta o estado de erro ao operador e aguarda a decisão,
    /// escrevendo o resultado em <see cref="ProcessExecutionContext.OUTCOME"/>.
    /// </summary>
    /// <param name="caseRef">Referência do caso AIIM.</param>
    /// <param name="ctx">Contexto de execução mutável — OUTCOME é escrito aqui.</param>
    /// <param name="decideOutcome">
    ///   Delegate que representa a interacção humana.
    ///   Em produção, suspende o workflow até o operador submeter o formulário MANEXC.
    ///   Em testes, substituído por um valor configurado no cenário.
    /// </param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExecuteAsync(
        AiimCaseRef caseRef,
        ProcessExecutionContext ctx,
        Func<AiimCaseRef, CancellationToken, Task<ManipularExcecaoPrpintpcResult>> decideOutcome,
        CancellationToken ct)
    {
        var result = await decideOutcome(caseRef, ct).ConfigureAwait(false);

        ctx.OUTCOME = result switch
        {
            ManipularExcecaoPrpintpcResult.ManuallyFixed => "OK",
            ManipularExcecaoPrpintpcResult.RetryAgain    => "R",
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }
}
