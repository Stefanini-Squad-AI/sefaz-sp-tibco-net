#nullable enable

namespace SefazSp.Epat.Domain.Rules.ATZINTPC;

/// <summary>
/// RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT — parte de domínio pura.
/// Condição: IPESystemValues.SW_QRETRYCOUNT &lt; MAXRETRIES → Stillgood (AtualizarIntimacao).
/// </summary>
public static class CheckRetriesSwQRetryCountRule
{
    public static bool IsStillGood(long swQRetryCount, int maxRetries) =>
        swQRetryCount < maxRetries;
}
