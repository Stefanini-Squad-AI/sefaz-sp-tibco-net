#nullable enable

namespace SefazSp.Epat.Application.UseCases.CRNOTPC;

/// <summary>
/// Resultado da interacao humana na tarefa 'Manipular Excecao'
/// (_NcJJ6V9KEfGqPfX31TKC3w) do processo CRNOTPC.
///
/// O operador escolhe uma de duas acoes:
///   - RetryAgain: rota o fluxo de volta ao inicio do laco (Try Again)
///   - ManuallyFixed: considera o caso resolvido manualmente (Manually Fixed)
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
    /// O fluxo avanca para o gateway Manually Fixed e encerra.
    /// </summary>
    ManuallyFixed,
}
