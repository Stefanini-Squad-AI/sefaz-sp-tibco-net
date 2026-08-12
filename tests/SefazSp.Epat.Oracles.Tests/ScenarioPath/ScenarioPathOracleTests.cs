using SefazSp.Epat.Oracles.Tests.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace SefazSp.Epat.Oracles.Tests.ScenarioPath;

/// <summary>
/// Oracle: scenario-path — 146 cases.
/// Harness links each scenario in artifacts/POC_Epat/scenarios/index.json to its
/// structural oracle.  Expected values come exclusively from the fixture; the agent
/// never writes or edits them.
/// </summary>
public sealed class ScenarioPathOracleTests
{
    private readonly ITestOutputHelper _output;

    public ScenarioPathOracleTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Provides the 146 scenario identifiers declared in the oracle fixture.
    /// </summary>
    public static IEnumerable<object[]> AllScenarios()
    {
        var index = ScenarioFixtureLoader.LoadIndex();
        return index.Scenarios.Select(s => new object[] { s.Id, s.Process, s.Kind });
    }

    /// <summary>
    /// For every scenario declared in the fixture:
    ///   1. The individual scenario file exists and is parseable.
    ///   2. The scenario carries at least one path segment.
    ///   3. Every node in every segment has a non-empty identifier.
    ///   4. Segments are ordered (ordemNaJornada is non-negative and monotonically
    ///      non-decreasing), confirming the path is a directed sequence.
    /// </summary>
    [Theory]
    [MemberData(nameof(AllScenarios))]
    public void Scenario_PathIsStructurallyValid(string scenarioId, string process, string kind)
    {
        var detail = ScenarioFixtureLoader.LoadScenario(scenarioId);

        Assert.Equal(scenarioId, detail.Id);
        Assert.Equal(process, detail.Process);
        Assert.Equal(kind, detail.Kind);

        var segments = detail.Segmentos;
        Assert.NotNull(segments);
        Assert.NotEmpty(segments);

        int previousOrder = -1;
        foreach (var seg in segments)
        {
            Assert.True(seg.OrdemNaJornada >= 0,
                $"[{scenarioId}] Segment has negative ordemNaJornada: {seg.OrdemNaJornada}");

            Assert.True(seg.OrdemNaJornada >= previousOrder,
                $"[{scenarioId}] Segment order not monotonic: {seg.OrdemNaJornada} < {previousOrder}");
            previousOrder = seg.OrdemNaJornada;

            if (seg.Nos is { Count: > 0 } nos)
            {
                foreach (var no in nos)
                {
                    Assert.False(string.IsNullOrWhiteSpace(no.Id),
                        $"[{scenarioId}] Node has empty id in segment at order {seg.OrdemNaJornada}");
                }
            }
        }

        _output.WriteLine($"[PASS] {scenarioId} ({process}, {kind}) — {segments.Count} segment(s)");
    }
}
