#nullable enable

namespace SefazSp.Epat.Application.UseCases.BSCENVPC;

/// <summary>
/// Resultado da interacao humana na tarefa 'Manipular Excecao'
/// (_qIDunF6BEfGBBLgT-R5iuw).
///
/// O operador escolhe uma de duas acoes:
///   - RetryAgain: rota o fluxo de volta ao inicio do laco (Try Again)
///   - ManuallyFixed: considera o caso resolvido manualmente (Done - Fixed)
/// </summary>
public enum ManipularExcecaoResult
{
    /// <summary>
    /// Operador optou por repetir a chamada. OUTCOME = 'R'.
    /// O fluxo regressa ao inicio do laco de retry.
    /// </summary>
    RetryAgain,

    /// <summary>
    /// Operador considera o caso resolvido. OUTCOME = 'OK'.
    /// O fluxo avanca para o gateway Manually Fixed e encerra em Done - Fixed.
    /// </summary>
    ManuallyFixed,
}
