#nullable enable

using System.Text.Json;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.UseCases.BSCENVPC;
using SefazSp.Epat.Application.Workflows.BSCENVPC;
using SefazSp.Epat.Infrastructure.Integration.Soap;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.BSCENVPC;

public sealed class ScBscenvpc009PathTests
{
    private static readonly AiimCaseRef CaseRef = new(123L, "idAiim-123idProc-456");

    [Fact]
    public async Task Full_error_path_matches_fixture_segment()
    {
        var oracle = LoadOracle();
        var workflow = CreateWorkflow(new ServiceEnvelope("9", "APP-001", "Erro de aplicacao"));
        var ctx = CreateRetryExhaustedContext();

        var trace = await workflow.RunSegmentAsync(CaseRef, ctx, operatorOutcome: null);

        Assert.Equal(oracle.ExpectedSegmentNodeIds, trace.VisitedNodes);
        Assert.Equal("Y", ctx.ISAPPERROR);
        Assert.Equal("APP-001", ctx.STERRORCODE);
        Assert.Equal("Erro de aplicacao", ctx.STERRORDESC);
    }

    [Fact]
    public async Task Regresso_edge_is_explicit_after_internal_end_event()
    {
        var oracle = LoadOracle();
        var workflow = CreateWorkflow(new ServiceEnvelope("9", "APP-001", "Erro de aplicacao"));
        var ctx = CreateRetryExhaustedContext();

        var trace = await workflow.RunSegmentAsync(CaseRef, ctx, operatorOutcome: null);
        var nodes = trace.VisitedNodes.ToArray();
        var endIndex = Array.IndexOf(nodes, BscenvpcSeg002Workflow.NodeEndEventInterno);
        var techErrorIndex = Array.IndexOf(nodes, BscenvpcSeg002Workflow.NodeTechError);

        Assert.True(endIndex >= 0);
        Assert.Equal(endIndex + 1, techErrorIndex);
        Assert.Equal("regresso", oracle.TechErrorEntryMode);
    }

    [Fact]
    public async Task App_error_gateway_bifurcates_from_status_code()
    {
        var oracle = LoadOracle();
        var successWorkflow = CreateWorkflow(new ServiceEnvelope("0", null, null));
        var errorWorkflow = CreateWorkflow(new ServiceEnvelope("9", "APP-001", "Erro de aplicacao"));

        var successTrace = await successWorkflow.RunSegmentAsync(
            CaseRef,
            new ProcessExecutionContext { MAXRETRIES = 5, NUMAPPRETRIES = 0 },
            operatorOutcome: null);

        var errorTrace = await errorWorkflow.RunSegmentAsync(
            CaseRef,
            CreateRetryExhaustedContext(),
            operatorOutcome: null);

        Assert.Equal(oracle.ExpectedSegmentNodeIds.Take(2).ToArray(), successTrace.VisitedNodes);
        Assert.Equal(BscenvpcSeg002Workflow.NodeSetAppError, errorTrace.VisitedNodes[2]);
    }

    [Fact]
    public async Task Manipular_excecao_records_outcome_and_routes_correctly()
    {
        var oracle = LoadOracle();
        var useCase = new ManipularExcecaoUseCase();

        var okWorkflow = CreateWorkflow(new ServiceEnvelope("9", "APP-001", "Erro de aplicacao"), useCase);
        var okContext = CreateRetryExhaustedContext();
        var okTrace = await okWorkflow.RunSegmentAsync(CaseRef, okContext, operatorOutcome: "OK");

        Assert.Equal(oracle.ExpectedSegmentNodeIds.Take(11).ToArray(), okTrace.VisitedNodes);
        Assert.Equal("OK", okContext.OUTCOME);
        Assert.True(useCase.IsManuallyFixed(okContext));
        Assert.False(useCase.IsTryAgain(okContext));

        var retryContext = new ProcessExecutionContext();
        useCase.RecordOutcome(retryContext, "R");
        Assert.Equal("R", retryContext.OUTCOME);
        Assert.True(useCase.IsTryAgain(retryContext));
        Assert.False(useCase.IsManuallyFixed(retryContext));
    }

    private static BscenvpcSeg002Workflow CreateWorkflow(
        ServiceEnvelope envelope,
        ManipularExcecaoUseCase? useCase = null) =>
        new(
            new BuscarVistasAtivasPorAiimSoapService(envelope),
            useCase ?? new ManipularExcecaoUseCase());

    private static ProcessExecutionContext CreateRetryExhaustedContext() =>
        new()
        {
            MAXRETRIES = 5,
            NUMAPPRETRIES = 5,
            ISAPPERROR = "N",
            ISTECHERROR = "N"
        };

    private static OracleScenario LoadOracle()
    {
        var repoRoot = FindRepositoryRoot();
        var path = Path.Combine(repoRoot, "artifacts", "POC_Epat", "scenarios", "SC-BSCENVPC-009.json");
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var scenario = JsonSerializer.Deserialize<ScenarioFixture>(json, options)
            ?? throw new InvalidOperationException("Oracle fixture could not be deserialized.");

        var segment = scenario.Segmentos.Single(s => s.DoPasso == 8 && s.AoPasso == 20);
        var techErrorPathNode = scenario.Path.Single(p => p.Id == BscenvpcSeg002Workflow.NodeTechError);

        return new OracleScenario(
            segment.Nos.Select(n => n.Id).ToArray(),
            techErrorPathNode.EntrouPor ?? string.Empty);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "artifacts", "POC_Epat", "scenarios")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }

    private sealed record OracleScenario(string[] ExpectedSegmentNodeIds, string TechErrorEntryMode);

    private sealed class ScenarioFixture
    {
        public List<SegmentFixture> Segmentos { get; set; } = [];
        public List<PathNodeFixture> Path { get; set; } = [];
    }

    private sealed class SegmentFixture
    {
        public int DoPasso { get; set; }
        public int AoPasso { get; set; }
        public List<SegmentNodeFixture> Nos { get; set; } = [];
    }

    private sealed class SegmentNodeFixture
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class PathNodeFixture
    {
        public string Id { get; set; } = string.Empty;
        public string? EntrouPor { get; set; }
    }
}
