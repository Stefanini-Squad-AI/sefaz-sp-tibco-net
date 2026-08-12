#nullable enable

namespace SefazSp.Epat.Domain.Rules.ATZINTPC;

public static class CheckRetriesSwQretrycountRule
{
    // RI-transition-ATZINTPC-CheckRetriesSWQRETRYCOUNT
    // iProcess: IPESystemValues.SW_QRETRYCOUNT < MAXRETRIES
    public static bool IsStillGood(long swQretrycount, int maxRetries) =>
        swQretrycount < maxRetries;
}
