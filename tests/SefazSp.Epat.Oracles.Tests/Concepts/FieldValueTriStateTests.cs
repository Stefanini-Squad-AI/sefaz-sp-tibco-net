#nullable enable

// Concept: iProcess-builtin compatibility — shim-tri-state (NOEQ-iprocess-builtin, 2026-08-06).
// SW_NA is a distinct THIRD state: not null and not empty. 18 fields compare against it, and
// collapsing SW_NA into null/empty would silently flip a branch. This proves the three states are
// distinct and that Match is exhaustive (the compiler forces all three cases).

using SefazSp.Epat.Domain.ValueObjects;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Concepts;

public sealed class FieldValueTriStateTests
{
    [Fact(DisplayName = "SW_NA (IsNotAvailable) is a distinct third state — not HasValue, not Empty")]
    public void SwNa_IsDistinctThirdState()
    {
        var na = FieldValue<string>.NotAvailable;
        var empty = FieldValue<string>.Empty;
        var val = FieldValue<string>.Of("X");

        Assert.True(na.IsNotAvailable);
        Assert.False(na.HasValue);
        Assert.False(na.IsEmpty);

        Assert.True(empty.IsEmpty);
        Assert.False(empty.IsNotAvailable);

        Assert.True(val.HasValue);
        Assert.False(val.IsNotAvailable);

        // SW_NA is NOT the same as Empty and NOT the same as a value.
        Assert.NotEqual(na, empty);
        Assert.NotEqual(na, val);
        Assert.NotEqual(empty, val);
    }

    [Fact(DisplayName = "Match dispatches each of the three states exhaustively")]
    public void Match_IsExhaustive()
    {
        static string Classify(FieldValue<string> f) =>
            f.Match(v => $"value:{v}", () => "na", () => "empty");

        Assert.Equal("value:X", Classify(FieldValue<string>.Of("X")));
        Assert.Equal("na", Classify(FieldValue<string>.NotAvailable));
        Assert.Equal("empty", Classify(FieldValue<string>.Empty));
    }
}
