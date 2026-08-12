#nullable enable

using System.Text.Json;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Workflows;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Infrastructure.Integration.Soap;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests;

public sealed class SC_BSCENVPC_007_Seg005Test
{
    [Fact]
    public async Task Retry_path_matches_fixture_segment_and_reaches_try_task()
    {
        var fixture = await ScenarioFixture.LoadAsync();
        var segment = fixture.GetSegment(ordemNaJornada: 1);
        var client = new BuscaEnvolvidosServiceClient(
            new HttpClient { BaseAddress = new Uri("https://epat.example.invalid/") },
            static (_, _) => Task.FromResult(new ServiceEnvelope("1", "APP", "service failure")));
        var workflow = new BscenvpcSegment005Workflow(client, new FixedClock());
        var context = new ProcessExecutionContext
        {
            ISAPPERROR = "N",
            ISTECHERROR = "N",
            MAXRETRIES = 5,
            NUMAPPRETRIES = 0,
            PROCESS_ID = "idAiim-123idProc-456"
        };

        var result = await workflow.ExecuteAsync(context, new AiimCaseRef(123, context.PROCESS_ID!), CancellationToken.None);

        Assert.Equal(segment.Nos.Select(node => node.Id), result.VisitedNodeIds);
        Assert.Equal("TryTask", result.Exit);
        Assert.Equal("Y", context.ISAPPERROR);
        Assert.Single(client.Calls);
    }

    [Fact]
    public async Task Success_path_is_derived_from_fixture_and_exits_on_done_success_branch()
    {
        var fixture = await ScenarioFixture.LoadAsync();
        var segment = fixture.GetSegment(ordemNaJornada: 1);
        var client = new BuscaEnvolvidosServiceClient(
            new HttpClient { BaseAddress = new Uri("https://epat.example.invalid/") },
            static (_, _) => Task.FromResult(new ServiceEnvelope("0", null, null)));
        var workflow = new BscenvpcSegment005Workflow(client, new FixedClock());
        var context = new ProcessExecutionContext
        {
            ISAPPERROR = "N",
            ISTECHERROR = "N",
            MAXRETRIES = 5,
            NUMAPPRETRIES = 0,
            PROCESS_ID = "idAiim-123idProc-456"
        };

        var result = await workflow.ExecuteAsync(context, new AiimCaseRef(123, context.PROCESS_ID!), CancellationToken.None);
        var expectedNodeIds = DeriveSuccessPath(segment);

        Assert.Equal(expectedNodeIds, result.VisitedNodeIds);
        Assert.Equal("DoneSuccess", result.Exit);
        Assert.Equal("N", context.ISAPPERROR);
        Assert.Single(client.Calls);
    }

    private static IReadOnlyList<string> DeriveSuccessPath(ScenarioSegment segment)
    {
        var removableNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Set App Error",
            "More Retries",
            "Pause",
            "Link To: Try Task",
            "Try Task"
        };

        return segment.Nos
            .Where(node => !removableNames.Contains(node.Nome))
            .Select(node => node.Id)
            .ToArray();
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset Now => new(2026, 8, 12, 19, 42, 13, TimeSpan.Zero);
        public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;
    }

    private sealed class ScenarioFixture
    {
        public required List<ScenarioSegment> Segmentos { get; init; }

        public static async Task<ScenarioFixture> LoadAsync()
        {
            var fixturePath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../../scenarios/SC-BSCENVPC-007.json"));

            await using var stream = File.OpenRead(fixturePath);
            var fixture = await JsonSerializer.DeserializeAsync<ScenarioFixture>(stream, SerializerOptions).ConfigureAwait(false);
            return fixture ?? throw new InvalidOperationException("Scenario fixture could not be deserialized.");
        }

        public ScenarioSegment GetSegment(int ordemNaJornada) =>
            Segmentos.Single(segment => segment.OrdemNaJornada == ordemNaJornada);

        private static JsonSerializerOptions SerializerOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private sealed class ScenarioSegment
    {
        public required int OrdemNaJornada { get; init; }
        public required List<ScenarioNode> Nos { get; init; }
    }

    private sealed class ScenarioNode
    {
        public required string Id { get; init; }
        public required string Nome { get; init; }
    }
}
