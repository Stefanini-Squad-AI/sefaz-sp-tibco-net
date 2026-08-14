#nullable enable

namespace SefazSp.Epat.Application.UseCases.PRPINTPC;

/// <summary>
/// Resultado da interacção humana na userTask 'Manipular Excecao'
/// (_KEwC5V6EEfGBBLgT-R5iuw) do processo PRPINTPC.
///
/// Card: BUILD-PRPINTPC-seg035 · AC7
/// </summary>
public enum ManipularExcecaoPrpintpcResult
{
    /// <summary>Caso resolvido manualmente. Gateway Manually Fixed → ramo 'Yes'. OUTCOME = "OK".</summary>
    ManuallyFixed,

    /// <summary>Operador opta por repetir a chamada. Gateway Try Again → ramo 'Yes'. OUTCOME = "R".</summary>
    RetryAgain,
}
