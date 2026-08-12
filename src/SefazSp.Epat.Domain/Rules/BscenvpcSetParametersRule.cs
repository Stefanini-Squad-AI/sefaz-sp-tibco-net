#nullable enable

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-BSCENVPC-SetParameters
/// Inicializa MAXRETRIES (default=5) e garante que IDPROCESSO e "0".
/// Decisao CLONE-PRPINTPC: comparar com "0", nao com SW_NA.
/// </summary>
public static class BscenvpcSetParametersRule
{
    public const int DefaultMaxRetries = 5;
    public const string ErrorProcessId = "0";

    /// <param name="maxRetries">MAXRETRIES actual (null se ainda nao inicializado).</param>
    /// <param name="idProcesso">IDPROCESSO actual.</param>
    /// <returns>Tuplo (maxRetries efectivo, isErrorBranch).</returns>
    public static (int maxRetries, bool isErrorBranch) Apply(int? maxRetries, string? idProcesso)
    {
        var effectiveMaxRetries = maxRetries ?? DefaultMaxRetries;
        var isErrorBranch = idProcesso == ErrorProcessId;
        return (effectiveMaxRetries, isErrorBranch);
    }
}
