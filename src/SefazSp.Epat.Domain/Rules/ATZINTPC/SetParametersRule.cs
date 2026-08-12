namespace SefazSp.Epat.Domain.Rules.ATZINTPC;

/// <summary>RI-script-ATZINTPC-SetParameters</summary>
public static class SetParametersRule
{
    public static void Apply(ref int maxRetries)
    {
        if (maxRetries == 0)
        {
            maxRetries = 5;
        }
    }
}
