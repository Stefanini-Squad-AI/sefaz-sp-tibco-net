#nullable enable

namespace SefazSp.Epat.Application.UseCases.ATZINTPC;

/// <summary>
/// Resultado possível da userTask 'Manipular Excecao' (_RNdJ0V6PEfGBBLgT-R5iuw)
/// do processo ATZINTPC.
///
/// O operador decide entre repetir a chamada ou considerar o caso resolvido.
/// Card: BUILD-ATZINTPC-seg041 · AC6
/// </summary>
public enum ManipularExcecaoAtzintpcResult
{
    /// <summary>Tentar novamente — OUTCOME = 'R'.</summary>
    RetryAgain,

    /// <summary>Caso resolvido manualmente — OUTCOME = 'OK'.</summary>
    ManuallyFixed,
}
