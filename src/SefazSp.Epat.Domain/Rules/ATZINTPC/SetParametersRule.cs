#nullable enable

namespace SefazSp.Epat.Domain.Rules.ATZINTPC;

public static class SetParametersRule
{
    // RI-script-ATZINTPC-SetParameters
    // iProcess: if (MAXRETRIES == null) MAXRETRIES = 5
    public static void Apply(object ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        var property = ctx.GetType().GetProperty("MAXRETRIES")
            ?? throw new ArgumentException("Context must expose MAXRETRIES.", nameof(ctx));

        var currentValue = property.GetValue(ctx);
        if (currentValue is int maxRetries && maxRetries == 0)
        {
            property.SetValue(ctx, 5);
        }
    }
}
