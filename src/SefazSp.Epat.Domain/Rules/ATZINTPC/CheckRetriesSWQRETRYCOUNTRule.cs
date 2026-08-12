namespace SefazSp.Epat.Domain.Rules.ATZINTPC;

/// <summary>RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT</summary>
public static class CheckRetriesSWQRETRYCOUNTRule
{
    public static bool IsStillGood(int swQRetryCount, int maxRetries)
        => swQRetryCount < maxRetries;
}
