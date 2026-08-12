#nullable enable

using System.Text.Json;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Workflows.ATZINTPC;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Integration.Soap;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.ScenarioPath;

public sealed class SC_ATZINTPC_007_Tests
{
    [Theory]
    [InlineData(0, 0, 5, "ERR")]
    [InlineData(2, 0, 5, "500")]
    [InlineData(4, 3, 5, "FAIL")]
    public async Task ExecutesCanonicalPath_SC_ATZINTPC_007(
        int numAppRetries,
        int swQretrycount,
        int maxRetries,
        string statusCode)
    {
        var fixture = LoadFixture("SC-ATZINTPC-007.json");
        var expectedNodeIds = fixture.Path.Select(static p => p.Id).ToList();

        var ctx = new ProcessExecutionContext
        {
            MAXRETRIES = 0,
            NUMAPPRETRIES = numAppRetries,
            PROCESS_ID = "idAiim-123idProc-456"
        };

        var aiimCase = new AiimCase
        {
            IdAiim = 123,
            SW_QRETRYCOUNT = swQretrycount
        };

        var services = new EpatSoapServices(_ => new ServiceEnvelope(statusCode, null, null));
        var clock = new FakeClock(new DateTimeOffset(2026, 8, 12, 19, 44, 54, TimeSpan.Zero));
        var workflow = new AtualizarIntimacaoWorkflow(services, clock);

        var trace = await workflow.ExecuteAsync(ctx, aiimCase, CancellationToken.None);

        Assert.Equal(expectedNodeIds, trace.NodeIds);
        Assert.Equal(maxRetries, ctx.MAXRETRIES);
        Assert.Equal("Y", ctx.ISAPPERROR);
    }

    private static ScenarioFixture LoadFixture(string fileName)
    {
        var repoRoot = FindRepoRoot();
        var fixturePath = Path.Combine(repoRoot, "artifacts", "POC_Epat", "scenarios", fileName);
        using var stream = File.OpenRead(fixturePath);
        var fixture = JsonSerializer.Deserialize<ScenarioFixture>(stream, JsonOptions);
        return fixture ?? throw new InvalidOperationException($"Could not deserialize fixture '{fixturePath}'.");
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "artifacts")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset Now => now;
        public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;
    }

    private sealed class ScenarioFixture
    {
        public List<ScenarioPathNode> Path { get; init; } = [];
    }

    private sealed class ScenarioPathNode
    {
        public string Id { get; init; } = string.Empty;
    }
}
