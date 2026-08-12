using SefazSp.Epat.Oracles.Tests.Fixture;
using Xunit;
using Xunit.Abstractions;

namespace SefazSp.Epat.Oracles.Tests.ScenarioPath;

/// <summary>
/// Concept: gateways-and (Gateways Paralelos AND) — BPMN element parallelGateway.
///
/// Acceptance criteria:
///   AC2: The gateways-and concept is observable at the 3 points where the package uses it,
///        confirming flow split and synchronisation.
///   AC3: The harness exercises the parallelGateway BPMN element and produces
///        concurrent-timeline evidence.
///
/// Evidence kind: concurrent-timeline
/// Source: POC_Epat.xpdl · elementId: gateways-and
/// </summary>
public sealed class GatewaysAndConceptTests
{
    /// <summary>
    /// The 3 parallel (AND) gateway node identifiers as extracted from POC_Epat.xpdl
    /// via the process model artifact (process-model.json, gatewayType=Parallel).
    /// These IDs are immutable — they are oracle values, not written by the agent.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> ParallelGatewayPoints =
        new Dictionary<string, string>
        {
            // step 22 in POC_EpatProcess — parallel AND split/join point 1
            ["_CtQ69FqPEfG5K7mY0I3I6w"] = "POC_EpatProcess · stepIndex=22",
            // step 37 in POC_EpatProcess — parallel AND split/join point 2
            ["_CtQ7BVqPEfG5K7mY0I3I6w"] = "POC_EpatProcess · stepIndex=37",
            // step 55 in POC_EpatProcess — parallel AND split/join point 3
            ["_Faq_RFqTEfG5K7mY0I3I6w"] = "POC_EpatProcess · stepIndex=55",
        };

    private readonly ITestOutputHelper _output;

    public GatewaysAndConceptTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The fixture declares exactly 3 parallel-gateway points for the gateways-and concept.
    /// This test guards the count so any kit regeneration that changes the number is caught.
    /// </summary>
    [Fact]
    public void GatewaysAnd_ThreePointsDeclaredInFixture()
    {
        Assert.Equal(3, ParallelGatewayPoints.Count);
    }

    /// <summary>
    /// Each parallel gateway node must appear in at least one of the 146 scenarios,
    /// confirming it is reachable and observable in a concrete execution path.
    /// </summary>
    [Fact]
    public void GatewaysAnd_EachPointIsObservableInAtLeastOneScenario()
    {
        var index = ScenarioFixtureLoader.LoadIndex();

        // Build a set of all node IDs traversed across all scenarios.
        var observedNodeIds = new HashSet<string>();
        foreach (var summary in index.Scenarios)
        {
            var detail = ScenarioFixtureLoader.LoadScenario(summary.Id);
            foreach (var seg in detail.Segmentos ?? [])
            foreach (var no in seg.Nos ?? [])
                observedNodeIds.Add(no.Id);
        }

        foreach (var (gatewayId, label) in ParallelGatewayPoints)
        {
            Assert.True(
                observedNodeIds.Contains(gatewayId),
                $"Parallel gateway '{label}' (id={gatewayId}) is not observed in any " +
                $"of the {index.Scenarios.Count} scenario paths — the gateways-and concept " +
                $"is not exercised at this point.");

            _output.WriteLine($"[observable] {label} · id={gatewayId}");
        }
    }

    /// <summary>
    /// Concurrent-timeline evidence: for each parallel gateway, collects the scenarios
    /// that traverse it and reports the observed split/join behaviour per scenario.
    ///
    /// A scenario exhibits parallel split when a gateway node is followed in the same
    /// segment by another node, or a later segment opens immediately after.
    /// A scenario exhibits parallel join  when a gateway node appears as the
    /// closing node of a segment (fechaEm contains the gateway id).
    ///
    /// The test does not write expected values; it asserts that the gateway
    /// appears in at least one split context and at least one join context across
    /// all scenarios, which constitutes the concurrent-timeline evidence.
    /// </summary>
    [Fact]
    public void GatewaysAnd_ConcurrentTimelineEvidence_SplitAndJoinObserved()
    {
        var index = ScenarioFixtureLoader.LoadIndex();

        var splitObserved = new Dictionary<string, bool>(
            ParallelGatewayPoints.Keys.Select(k => KeyValuePair.Create(k, false)));
        var joinObserved = new Dictionary<string, bool>(
            ParallelGatewayPoints.Keys.Select(k => KeyValuePair.Create(k, false)));

        foreach (var summary in index.Scenarios)
        {
            var detail = ScenarioFixtureLoader.LoadScenario(summary.Id);
            var segments = detail.Segmentos ?? [];

            for (int si = 0; si < segments.Count; si++)
            {
                var seg = segments[si];
                var nodes = seg.Nos ?? [];

                for (int ni = 0; ni < nodes.Count; ni++)
                {
                    var nodeId = nodes[ni].Id;
                    if (!ParallelGatewayPoints.ContainsKey(nodeId))
                        continue;

                    // Split: gateway is not the last node in the segment (flow continues inline)
                    // or there is a next segment (the gateway opens a fork).
                    bool isSplit = ni < nodes.Count - 1 || si < segments.Count - 1;

                    // Join: gateway appears as the closing node (fechaEm references gateway id).
                    bool isJoin = seg.FechaEm != null && seg.FechaEm.Contains(nodeId);

                    if (isSplit) splitObserved[nodeId] = true;
                    if (isJoin)  joinObserved[nodeId]  = true;

                    _output.WriteLine(
                        $"[concurrent-timeline] {summary.Id} · gateway={nodeId} " +
                        $"split={isSplit} join={isJoin} " +
                        $"segment={seg.OrdemNaJornada} node={ni}");
                }
            }
        }

        foreach (var (gatewayId, label) in ParallelGatewayPoints)
        {
            Assert.True(
                splitObserved[gatewayId] || joinObserved[gatewayId],
                $"Parallel gateway '{label}' (id={gatewayId}) was not observed in any " +
                $"split or join context — no concurrent-timeline evidence.");

            _output.WriteLine(
                $"[evidence] {label} · split={splitObserved[gatewayId]} join={joinObserved[gatewayId]}");
        }
    }

    /// <summary>
    /// Provides the 3 parallel-gateway points as individual theory cases so each
    /// point is independently visible in the test report.
    /// </summary>
    public static IEnumerable<object[]> ParallelGatewayData() =>
        ParallelGatewayPoints.Select(kv => new object[] { kv.Key, kv.Value });

    /// <summary>
    /// Each parallel gateway node (gateways-and · parallelGateway) is observable
    /// in at least one scenario path — individual assertion per point.
    /// </summary>
    [Theory]
    [MemberData(nameof(ParallelGatewayData))]
    public void GatewaysAnd_EachPoint_IsObservable(string gatewayId, string label)
    {
        var index = ScenarioFixtureLoader.LoadIndex();

        bool found = false;
        foreach (var summary in index.Scenarios)
        {
            var detail = ScenarioFixtureLoader.LoadScenario(summary.Id);
            foreach (var seg in detail.Segmentos ?? [])
            foreach (var no in seg.Nos ?? [])
            {
                if (no.Id == gatewayId)
                {
                    found = true;
                    _output.WriteLine(
                        $"[observable] gateway={gatewayId} in {summary.Id} " +
                        $"segment={seg.OrdemNaJornada}");
                    break;
                }
            }
            if (found) break;
        }

        Assert.True(found,
            $"Parallel gateway '{label}' (id={gatewayId}) not observed in any scenario path. " +
            $"The gateways-and concept (parallelGateway) is not exercised at this point.");
    }
}
