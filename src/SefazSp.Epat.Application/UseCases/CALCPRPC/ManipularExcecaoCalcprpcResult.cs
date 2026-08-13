#nullable enable

namespace SefazSp.Epat.Application.UseCases.CALCPRPC;

/// <summary>
/// Resultado da interacao humana na tarefa 'Manipular Excecao'
/// (_zJIHXVqiEfG5K7mY0I3I6w).
/// </summary>
public enum ManipularExcecaoCalcprpcResult
{
    /// <summary>
    /// Operador optou por repetir a chamada. OUTCOME = 'R'.
    /// </summary>
    RetryAgain,

    /// <summary>
    /// Operador considera o caso resolvido manualmente. OUTCOME = 'OK'.
    /// </summary>
    ManuallyFixed,
}
