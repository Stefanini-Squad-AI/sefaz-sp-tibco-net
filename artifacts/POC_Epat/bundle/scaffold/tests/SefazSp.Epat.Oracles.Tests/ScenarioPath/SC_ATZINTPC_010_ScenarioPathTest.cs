#nullable enable
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using SefazSp.Epat.Application.Workflows.ATZINTPC;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.ScenarioPath;

public sealed class SC_ATZINTPC_010_ScenarioPathTest
{
    [Fact]
    public void PathMatchesFixture()
    {
        var fixture = LoadFixture();
        var expectedPathIds = fixture.Path.Select(entry => entry.Id).ToArray();
        var workflow = new AtzintpcWorkflow();

        var actualPath = workflow.RunSegment043(
            checkRetriesStillGood: true,
            serviceCallFailed: true,
            isTechError: false,
            isAppError: false);

        Assert.Equal(expectedPathIds, actualPath);
    }

    [Fact]
    public void DescentTransitionIsExplicit()
    {
        var fixture = LoadFixture();
        var descentIndex = FindPathIndex(fixture.Path, "descida");
        var expectedTo = fixture.Path[descentIndex].Id;
        var expectedFrom = fixture.Path[descentIndex - 1].Id;

        Assert.Contains(
            AtzintpcWorkflow.ExplicitTransitions,
            transition => transition.From == expectedFrom
                && transition.To == expectedTo
                && transition.Kind == "descida");
    }

    [Fact]
    public void AscentTransitionIsExplicit()
    {
        var fixture = LoadFixture();
        var ascentIndex = FindPathIndex(fixture.Path, "regresso");
        var expectedTo = fixture.Path[ascentIndex].Id;
        var expectedFrom = fixture.Path[ascentIndex - 1].Id;

        Assert.Contains(
            AtzintpcWorkflow.ExplicitTransitions,
            transition => transition.From == expectedFrom
                && transition.To == expectedTo
                && transition.Kind == "regresso");
    }

    private static ScenarioFixture LoadFixture([CallerFilePath] string sourceFilePath = "")
    {
        var fixturePath = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(sourceFilePath)!,
            "../../../../../../../artifacts/POC_Epat/scenarios/SC-ATZINTPC-010.json"));

        var json = File.ReadAllText(fixturePath);
        return JsonSerializer.Deserialize<ScenarioFixture>(json)
            ?? throw new InvalidOperationException("Fixture SC-ATZINTPC-010 não pôde ser desserializada.");
    }

    private static int FindPathIndex(IReadOnlyList<PathEntry> path, string entrouPor)
    {
        for (var i = 0; i < path.Count; i++)
        {
            if (path[i].EntrouPor == entrouPor)
            {
                return i;
            }
        }

        throw new InvalidOperationException($"Nenhuma entrada com entrouPor='{entrouPor}' foi encontrada no fixture.");
    }

    private sealed class ScenarioFixture
    {
        [JsonPropertyName("path")]
        public IReadOnlyList<PathEntry> Path { get; init; } = [];
    }

    private sealed class PathEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("entrouPor")]
        public string? EntrouPor { get; init; }
    }
}
