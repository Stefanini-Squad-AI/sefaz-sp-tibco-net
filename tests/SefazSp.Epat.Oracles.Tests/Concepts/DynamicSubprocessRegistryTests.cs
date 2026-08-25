#nullable enable

// Concept: dynamic-subprocess (NOEQ-dynamic-subprocess = interface-registry-validated, 2026-08-06).
// DoD acceptance check (section 4): "Known target starts; unknown target fails visibly and audibly."
// The AGUARDARRegistry validates the closed set of AGUARDAR destinations at startup and resolves
// the runtime callee by AGUARDAR[IDX_AGUARDAR] — an unknown callee throws, it does NOT fail silently
// (does NOT inherit HaltOnBadSubProcess=false from the TIBCO legacy).

using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Infrastructure.Integration.Doubles;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Concepts;

public sealed class DynamicSubprocessRegistryTests
{
    private sealed class StubAguretpc : IAGURETPC
    {
        public Task<ProcessCallResult> ExecuteAsync(AiimCaseRef caseRef, CancellationToken ct)
            => Task.FromResult(new ProcessCallResult(Started: true, ChildInstanceId: "stub", Failure: null));
    }

    private static readonly string[] AllDestinations =
        { "AgPecas", "AgPRJ", "AgRecPRJ", "AgPRJR", "AgRCRaz", "AgCRaz", "AgPetica" };

    private static Dictionary<string, IAGURETPC> AllSeven()
        => AllDestinations.ToDictionary(k => k, _ => (IAGURETPC)new StubAguretpc(), StringComparer.Ordinal);

    [Fact(DisplayName = "Known AGUARDAR target resolves to its registered implementation")]
    public void KnownTarget_Resolves()
    {
        var registry = new AGUARDARRegistry(AllSeven());
        Assert.NotNull(registry.Resolve("AgPecas"));
        Assert.Equal(AllDestinations.OrderBy(x => x), registry.RegisteredDestinations.OrderBy(x => x));
    }

    [Fact(DisplayName = "Unknown AGUARDAR target fails visibly (throws), not silently")]
    public void UnknownTarget_FailsVisibly()
    {
        var registry = new AGUARDARRegistry(AllSeven());
        var ex = Assert.Throws<InvalidOperationException>(() => registry.Resolve("AgDesconhecido"));
        Assert.Contains("AgDesconhecido", ex.Message);
    }

    [Fact(DisplayName = "A missing expected destination fails at startup (registry construction throws)")]
    public void MissingDestination_FailsAtStartup()
    {
        var incomplete = AllSeven();
        incomplete.Remove("AgPRJ");
        var ex = Assert.Throws<InvalidOperationException>(() => new AGUARDARRegistry(incomplete));
        Assert.Contains("AgPRJ", ex.Message);
    }
}
