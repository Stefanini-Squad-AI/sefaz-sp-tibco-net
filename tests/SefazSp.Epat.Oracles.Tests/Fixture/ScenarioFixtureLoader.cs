using System.Text.Json;
using System.Text.Json.Serialization;

namespace SefazSp.Epat.Oracles.Tests.Fixture;

/// <summary>
/// Scenario index entry from artifacts/POC_Epat/scenarios/index.json.
/// </summary>
public sealed record ScenarioSummary(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("process")] string Process,
    [property: JsonPropertyName("etapa")] int Etapa,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("de")] string? De,
    [property: JsonPropertyName("ate")] string? Ate,
    [property: JsonPropertyName("passos")] int Passos,
    [property: JsonPropertyName("decisoes")] int Decisoes,
    [property: JsonPropertyName("entradas")] int Entradas,
    [property: JsonPropertyName("etapas")] IReadOnlyList<int>? Etapas
);

public sealed record ScenarioIndex(
    [property: JsonPropertyName("scenarios")] IReadOnlyList<ScenarioSummary> Scenarios
);

/// <summary>
/// Node within a path segment.
/// </summary>
public sealed record ScenarioNode(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("nome")] string? Nome,
    [property: JsonPropertyName("tipo")] string? Tipo
);

/// <summary>
/// Node in the flat ordered path of a reference scenario (SC-*.json → path[]).
/// </summary>
public sealed record ScenarioPathNode(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("nome")] string? Nome,
    [property: JsonPropertyName("tipo")] string? Tipo,
    [property: JsonPropertyName("escopo")] string? Escopo,
    [property: JsonPropertyName("entrouPor")] string? EntrouPor
);

/// <summary>
/// Sub-process call declared in a reference scenario (descidas[]).
/// </summary>
public sealed record ScenarioDescida(
    [property: JsonPropertyName("passo")] string? Passo,
    [property: JsonPropertyName("passoId")] string? PassoId,
    [property: JsonPropertyName("continuaEm")] string? ContinuaEm,
    [property: JsonPropertyName("dinamica")] bool Dinamica,
    [property: JsonPropertyName("graftStep")] bool GraftStep,
    [property: JsonPropertyName("cenarios")] IReadOnlyList<string>? Cenarios
);

/// <summary>
/// One path segment (ordered slice of the scenario journey).
/// </summary>
public sealed record ScenarioSegmento(
    [property: JsonPropertyName("ordemNaJornada")] int OrdemNaJornada,
    [property: JsonPropertyName("doPasso")] int DoPasso,
    [property: JsonPropertyName("aoPasso")] int AoPasso,
    [property: JsonPropertyName("abrePor")] string? AbrePor,
    [property: JsonPropertyName("fechaEm")] string? FechaEm,
    [property: JsonPropertyName("etapas")] IReadOnlyList<int>? Etapas,
    [property: JsonPropertyName("nos")] IReadOnlyList<ScenarioNode>? Nos
);

/// <summary>
/// Full scenario file (SC-*.json).
/// </summary>
public sealed record ScenarioDetail(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("process")] string Process,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("etapas")] IReadOnlyList<int>? Etapas,
    [property: JsonPropertyName("segmentos")] IReadOnlyList<ScenarioSegmento>? Segmentos,
    [property: JsonPropertyName("path")] IReadOnlyList<ScenarioPathNode>? Path,
    [property: JsonPropertyName("descidas")] IReadOnlyList<ScenarioDescida>? Descidas
);

/// <summary>
/// Loads the scenario oracle fixture.
/// Path is resolved relative to the repository root (the directory containing 'artifacts/').
/// </summary>
public static class ScenarioFixtureLoader
{
    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "artifacts")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            "Cannot locate repository root (directory containing 'artifacts/'). " +
            $"Searched from: {AppContext.BaseDirectory}");
    }

    public static string RepoRootPath => RepoRoot;

    public static string FixturePath =>
        Path.Combine(RepoRoot, "artifacts", "POC_Epat", "scenarios", "index.json");

    public static string ScenarioDir =>
        Path.Combine(RepoRoot, "artifacts", "POC_Epat", "scenarios");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static ScenarioIndex LoadIndex()
    {
        var json = File.ReadAllText(FixturePath);
        return JsonSerializer.Deserialize<ScenarioIndex>(json, JsonOpts)
               ?? throw new InvalidOperationException("Failed to deserialize scenario index.");
    }

    public static ScenarioDetail LoadScenario(string scenarioId)
    {
        var path = Path.Combine(ScenarioDir, $"{scenarioId}.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Scenario file not found: {path}", path);

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ScenarioDetail>(json, JsonOpts)
               ?? throw new InvalidOperationException($"Failed to deserialize scenario: {scenarioId}");
    }
}
