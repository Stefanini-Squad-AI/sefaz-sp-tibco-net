using System.Text.Json;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Execution.ATZINTPC;
using SefazSp.Epat.Application.UseCases.ATZINTPC;
using SefazSp.Epat.Application.Workflows.ATZINTPC;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests;

public sealed class ATZINTPCWorkflowOracleTests
{
    private static readonly string[] ExpectedPath = LoadExpectedPath();

    public static IEnumerable<object[]> OracleCases()
    {
        yield return ["case-1", 5, 5, 0, "1", "BAIL", 1001L, 2001];
        yield return ["case-2", 5, 6, 1, "ERR", "BAIL", 1002L, 2002];
        yield return ["case-3", 3, 3, 2, "-1", "BAIL", 1003L, 2003];
        yield return ["case-4", 7, 7, 3, "500", "BAIL", 1004L, 2004];
        yield return ["case-5", 2, 2, 0, "X", "BAIL", 1005L, 2005];
        yield return ["case-6", 9, 10, 8, "NA", "BAIL", 1006L, 2006];
    }

    [Theory]
    [MemberData(nameof(OracleCases))]
    public async Task ExecuteAsync_matches_scenario_path(
        string _,
        int maxRetries,
        int numAppRetries,
        int swQRetryCount,
        string statusCode,
        string manualOutcome,
        long idAiim,
        int idProcesso)
    {
        var workflow = new ATZINTPCWorkflow(
            new StubAtualizarIntimacaoOperation(new ServiceEnvelope(statusCode, "ST-001", "Simulated failure")),
            new ManipularExcecaoUseCase());

        var state = new ATZINTPCWorkflowState(
            new AiimCase
            {
                IDAIIM = idAiim,
                IDPROCESSO = FieldValue<int>.Of(idProcesso)
            },
            new ProcessExecutionContext
            {
                MAXRETRIES = maxRetries,
                NUMAPPRETRIES = numAppRetries
            })
        {
            SW_QRETRYCOUNT = swQRetryCount,
            ManualExceptionOutcome = manualOutcome,
            CurrentDateTimeText = "2026-08-12T19:41:09.2110000Z"
        };

        var visitedNodeIds = await workflow.ExecuteAsync(state);

        Assert.Equal(ExpectedPath, visitedNodeIds);
        Assert.Equal("Y", state.ExecutionContext.ISAPPERROR);
        Assert.Equal("N", state.ExecutionContext.ISTECHERROR);
        Assert.Equal(statusCode, state.ExecutionContext.STATUS_CODE);
        Assert.Equal("BAIL", state.ExecutionContext.OUTCOME);
        Assert.Equal("AtualizarIntimacao", state.ExecutionContext.SERVICE_NAME);
    }

    private static string[] LoadExpectedPath()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "SC-ATZINTPC-009.json");
        using var stream = File.OpenRead(fixturePath);
        using var document = JsonDocument.Parse(stream);

        return document.RootElement
            .GetProperty("path")
            .EnumerateArray()
            .Select(node => node.GetProperty("id").GetString())
            .OfType<string>()
            .ToArray();
    }

    private sealed class StubAtualizarIntimacaoOperation : IAtualizarIntimacaoOperation
    {
        private readonly ServiceEnvelope _envelope;

        public StubAtualizarIntimacaoOperation(ServiceEnvelope envelope)
        {
            _envelope = envelope;
        }

        public Task<ServiceEnvelope> ExecuteAsync(
            AiimCase caseData,
            ProcessExecutionContext executionContext,
            CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(caseData);
            ArgumentNullException.ThrowIfNull(executionContext);
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(_envelope);
        }
    }
}
