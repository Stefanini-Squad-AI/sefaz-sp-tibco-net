#nullable enable

namespace SefazSp.Epat.Application.UseCases.CRNOTPC;

/// <summary>
/// Resultado da interação humana na tarefa 'Manipular Excecao'
/// (_NcJJ6V9KEfGqPfX31TKC3w) do processo CRNOTPC.
///
/// O operador escolhe uma de duas ações:
///   - RetryAgain: rota o fluxo de volta ao início do laço (Try Again)
///   - ManuallyFixed: considera o caso resolvido manualmente (Done - Fixed)
/// </summary>
public enum ManipularExcecaoResult
{
    /// <summary>
    /// Operador optou por repetir a chamada. OUTCOME = 'R'.
    /// O fluxo regressa ao início do laço de retry.
    /// </summary>
    RetryAgain,

    /// <summary>
    /// Operador considera o caso resolvido. OUTCOME = 'OK'.
    /// O fluxo avança para o gateway Manually Fixed e encerra em Done - Fixed.
    /// </summary>
    ManuallyFixed,
}
