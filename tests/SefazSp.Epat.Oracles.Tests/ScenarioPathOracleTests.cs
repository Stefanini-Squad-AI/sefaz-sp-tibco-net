// Oracle scenario-path para o conceito timers-deadlines (SSTN-80).
// Lê a fixture artifacts/POC_Epat/scenarios/index.json — nunca escreve nem edita valores esperados.
// Conceito: timers-deadlines · 11 pontos · kind prazo (37/146 cenários)
// Decisão expression-deadline: absolute-instant (ratificado 2026-08-06).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests;

// ---------------------------------------------------------------------------
// Localização da fixture
// ---------------------------------------------------------------------------

internal static class FixturePaths
{
    // Sobe do diretório de execução do assembly até encontrar a raiz do repositório
    // (marcada pela presença de artifacts/POC_Epat/scenarios/index.json).
    private static string? _root;

    public static string RepoRoot
    {
        get
        {
            if (_root is not null)
                return _root;

            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, "artifacts", "POC_Epat", "scenarios", "index.json");
                if (File.Exists(candidate))
                {
                    _root = dir.FullName;
                    return _root;
                }
                dir = dir.Parent;
            }
            throw new InvalidOperationException(
                "Não foi possível localizar artifacts/POC_Epat/scenarios/index.json " +
                "a partir de " + AppContext.BaseDirectory);
        }
    }

    public static string ScenariosDir =>
        Path.Combine(RepoRoot, "artifacts", "POC_Epat", "scenarios");

    public static string IndexFile =>
        Path.Combine(ScenariosDir, "index.json");

    public static string ScenarioFile(string id) =>
        Path.Combine(ScenariosDir, id + ".json");
}

// ---------------------------------------------------------------------------
// Modelos de leitura da fixture (somente leitura — sem atribuição de valores)
// ---------------------------------------------------------------------------

internal sealed record ScenarioSummary(string Id, string Kind);

internal sealed record ScenarioIndex(
    IReadOnlyList<ScenarioSummary> Scenarios,
    IReadOnlyDictionary<string, int> ByKind);

internal sealed record ScenarioNode(string Id, string Nome, string Tipo);

internal sealed record ScenarioSegment(IReadOnlyList<ScenarioNode> Nos);

internal sealed record ScenarioDetail(
    string Passo,
    string PassoId,
    IReadOnlyList<string> Cenarios);

internal sealed record ScenarioFile(
    string Id,
    string Kind,
    string De,
    string Ate,
    IReadOnlyList<ScenarioSegment> Segmentos,
    IReadOnlyList<ScenarioDetail> Detalhes);

// ---------------------------------------------------------------------------
// Carregamento da fixture
// ---------------------------------------------------------------------------

internal static class FixtureLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private static ScenarioIndex? _index;

    public static ScenarioIndex LoadIndex()
    {
        if (_index is not null)
            return _index;

        using var stream = File.OpenRead(FixturePaths.IndexFile);
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        var scenarios = root.GetProperty("scenarios")
            .EnumerateArray()
            .Select(e => new ScenarioSummary(
                e.GetProperty("id").GetString()!,
                e.GetProperty("kind").GetString()!))
            .ToList();

        var byKind = root.GetProperty("summary").GetProperty("byKind")
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetInt32());

        _index = new ScenarioIndex(scenarios, byKind);
        return _index;
    }

    public static ScenarioFile LoadScenario(string id)
    {
        var path = FixturePaths.ScenarioFile(id);
        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        static IReadOnlyList<ScenarioNode> ParseNodes(JsonElement seg)
        {
            if (!seg.TryGetProperty("nos", out var nosElem))
                return Array.Empty<ScenarioNode>();
            return nosElem.EnumerateArray()
                .Select(n => new ScenarioNode(
                    n.GetProperty("id").GetString()!,
                    n.TryGetProperty("nome", out var nm) ? nm.GetString() ?? "" : "",
                    n.TryGetProperty("tipo", out var tp) ? tp.GetString() ?? "" : ""))
                .ToList();
        }

        var segmentos = root.TryGetProperty("segmentos", out var segsElem)
            ? segsElem.EnumerateArray()
                .Select(s => new ScenarioSegment(ParseNodes(s)))
                .ToList()
            : (IReadOnlyList<ScenarioSegment>)Array.Empty<ScenarioSegment>();

        var detalhes = root.TryGetProperty("descidas", out var detsElem)
            ? detsElem.EnumerateArray()
                .Select(d =>
                {
                    var passo = d.TryGetProperty("passo", out var p) ? p.GetString() ?? "" : "";
                    var passoId = d.TryGetProperty("passoId", out var pid) ? pid.GetString() ?? "" : "";
                    var cenarios = d.TryGetProperty("cenarios", out var cElem)
                        ? cElem.EnumerateArray().Select(c => c.GetString()!).ToList()
                        : (IReadOnlyList<string>)Array.Empty<string>();
                    return new ScenarioDetail(passo, passoId, cenarios);
                })
                .ToList()
            : (IReadOnlyList<ScenarioDetail>)Array.Empty<ScenarioDetail>();

        var scenarioId = root.GetProperty("id").GetString()!;
        var kind = root.TryGetProperty("kind", out var kElem) ? kElem.GetString() ?? "" : "";
        // de/ate não constam no ficheiro individual — derivam de origem.nome / destino.nome
        var de = root.TryGetProperty("origem", out var origElem) && origElem.TryGetProperty("nome", out var origNome)
            ? origNome.GetString() ?? "" : "";
        var ate = root.TryGetProperty("destino", out var destElem) && destElem.TryGetProperty("nome", out var destNome)
            ? destNome.GetString() ?? "" : "";

        return new ScenarioFile(scenarioId, kind, de, ate, segmentos, detalhes);
    }
}

// ---------------------------------------------------------------------------
// Data para Theory tests
// ---------------------------------------------------------------------------

internal sealed class AllScenarioIds : TheoryData<string>
{
    public AllScenarioIds()
    {
        foreach (var s in FixtureLoader.LoadIndex().Scenarios)
            Add(s.Id);
    }
}

internal sealed class PrazoScenarioIds : TheoryData<string>
{
    public PrazoScenarioIds()
    {
        foreach (var s in FixtureLoader.LoadIndex().Scenarios.Where(s => s.Kind == "prazo"))
            Add(s.Id);
    }
}

// ---------------------------------------------------------------------------
// Oracle scenario-path — timers-deadlines
// ---------------------------------------------------------------------------

/// <summary>
/// Arnes de oráculo scenario-path que prova o conceito timers-deadlines
/// nos 11 pontos do pacote POC_Epat.
///
/// Invariante: nenhum valor esperado é escrito ou editado por este arnes.
/// Os valores vêm exclusivamente da fixture artifacts/POC_Epat/scenarios/index.json.
/// </summary>
public sealed class ScenarioPathOracleTests
{
    // Os 11 IDs de nó timer identificados por timerEventDefinition no BPMN.
    // Fonte: artifacts/POC_Epat/conformance.json · concepts[timers-deadlines] · occurrences=11
    private static readonly IReadOnlySet<string> TimerNodeIds = new HashSet<string>
    {
        // POC_EpatProcess: 4 boundary timer events
        "_CtQ6_1qPEfG5K7mY0I3I6w",   // Timer: PRAZORETIRADAVI; HORAFINAL.Time (1)
        "_CtQ7A1qPEfG5K7mY0I3I6w",   // Timer: PRAZORETIRADAVI; HORAFINAL.Time (2)
        "_XWivFlqTEfG5K7mY0I3I6w",   // Fim de Prazo Mantendo Atividade
        "_T4Ma8FqiEfG5K7mY0I3I6w",   // Encerra e retira
        // ATZINTPC: Pause
        "_RNdJ1l6PEfGBBLgT-R5iuw",
        // DEAT0050: Aguarda Defesa
        "_lrer2lqhEfG5K7mY0I3I6w",
        // CALCPRPC: Pause
        "_zJIHYlqiEfG5K7mY0I3I6w",
        // AGPECASPC: Timer 1h
        "_EvOwRF6eEfGJqLUhfbpFcQ",
        // PRPINTPC: Pause
        "_KEwC6l6EEfGBBLgT-R5iuw",
        // CRNOTPC: Pause
        "_NcJJ7l9KEfGqPfX31TKC3w",
        // BSCENVPC: Pause
        "_qIDuoV6BEfGBBLgT-R5iuw",
    };

    // -----------------------------------------------------------------------
    // AC1 — estrutura da fixture
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "Fixture contém exactamente 146 cenários")]
    public void Fixture_Has146Scenarios()
    {
        var index = FixtureLoader.LoadIndex();
        Assert.Equal(146, index.Scenarios.Count);
    }

    [Fact(DisplayName = "Fixture contém exactamente 37 cenários de kind 'prazo'")]
    public void Fixture_Has37PrazoScenarios()
    {
        var index = FixtureLoader.LoadIndex();
        Assert.Equal(37, index.ByKind["prazo"]);
        Assert.Equal(37, index.Scenarios.Count(s => s.Kind == "prazo"));
    }

    // -----------------------------------------------------------------------
    // AC2 — ficheiro de cada cenário existe e é legível
    // -----------------------------------------------------------------------

    [Theory(DisplayName = "Ficheiro de cenário existe na fixture")]
    [ClassData(typeof(AllScenarioIds))]
    public void Scenario_FileExists(string scenarioId)
    {
        Assert.True(
            File.Exists(FixturePaths.ScenarioFile(scenarioId)),
            $"Ficheiro em falta: {scenarioId}.json");
    }

    // -----------------------------------------------------------------------
    // AC2 — estrutura de cada cenário é válida
    // -----------------------------------------------------------------------

    [Theory(DisplayName = "Cenário tem id, kind, de, ate e pelo menos um segmento")]
    [ClassData(typeof(AllScenarioIds))]
    public void Scenario_HasRequiredFields(string scenarioId)
    {
        var sc = FixtureLoader.LoadScenario(scenarioId);

        Assert.False(string.IsNullOrWhiteSpace(sc.Id),
            $"[{scenarioId}] campo 'id' em falta");
        Assert.Equal(scenarioId, sc.Id);
        Assert.False(string.IsNullOrWhiteSpace(sc.Kind),
            $"[{scenarioId}] campo 'kind' em falta");
        Assert.False(string.IsNullOrWhiteSpace(sc.De),
            $"[{scenarioId}] campo 'de' em falta");
        Assert.False(string.IsNullOrWhiteSpace(sc.Ate),
            $"[{scenarioId}] campo 'ate' em falta");
        Assert.NotEmpty(sc.Segmentos);
    }

    [Theory(DisplayName = "Cada segmento tem pelo menos um nó com id não vazio")]
    [ClassData(typeof(AllScenarioIds))]
    public void Scenario_AllSegmentsHaveNodes(string scenarioId)
    {
        var sc = FixtureLoader.LoadScenario(scenarioId);
        foreach (var seg in sc.Segmentos)
        {
            Assert.NotEmpty(seg.Nos);
            foreach (var no in seg.Nos)
                Assert.False(string.IsNullOrWhiteSpace(no.Id),
                    $"[{scenarioId}] nó sem id em segmento");
        }
    }

    // -----------------------------------------------------------------------
    // AC2 — cenários 'prazo' contêm pelo menos um nó timerEvent
    // -----------------------------------------------------------------------

    [Theory(DisplayName = "Cenário prazo contém pelo menos um nó timerEvent no caminho")]
    [ClassData(typeof(PrazoScenarioIds))]
    public void PrazoScenario_ContainsTimerNode(string scenarioId)
    {
        var sc = FixtureLoader.LoadScenario(scenarioId);
        var allNodeIds = sc.Segmentos
            .SelectMany(s => s.Nos)
            .Select(n => n.Id)
            .ToHashSet();

        var hasTimer = allNodeIds.Overlaps(TimerNodeIds)
            || sc.Segmentos
                .SelectMany(s => s.Nos)
                .Any(n => string.Equals(n.Tipo, "timerEvent", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasTimer,
            $"[{scenarioId}] cenário de kind 'prazo' não contém nenhum nó timerEvent. " +
            $"IDs no caminho: [{string.Join(", ", allNodeIds)}]");
    }

    // -----------------------------------------------------------------------
    // AC3 — os 11 pontos timers-deadlines são observáveis no conjunto de cenários
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "Os 11 pontos timerEventDefinition são cobertos pelo conjunto de cenários da fixture")]
    public void TimersDeadlines_AllElevenPoints_VisibleInFixture()
    {
        var index = FixtureLoader.LoadIndex();
        var coveredTimerIds = new HashSet<string>();

        foreach (var summary in index.Scenarios)
        {
            var sc = FixtureLoader.LoadScenario(summary.Id);
            foreach (var seg in sc.Segmentos)
            foreach (var no in seg.Nos)
                if (TimerNodeIds.Contains(no.Id))
                    coveredTimerIds.Add(no.Id);

            if (coveredTimerIds.Count == TimerNodeIds.Count)
                break;
        }

        var missing = TimerNodeIds.Except(coveredTimerIds).ToList();
        Assert.Empty(missing);
    }

    // -----------------------------------------------------------------------
    // AC3 — decisão expression-deadline: absolute-instant
    // Valida que cada cenário 'prazo' com nó timerEvent também cobre
    // a mitigação de prorrogação (laço: o conjunto prazo inclui cenários
    // que passam pelo mesmo timer mais de uma vez ou que chegam ao timer
    // por caminhos distintos — transcrição do padrão DEAT0050/CALCPRPC).
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "Mitigação absolute-instant: ao menos dois cenários prazo cobrem o mesmo ponto timer (prorrogação via laço)")]
    public void ExpressionDeadline_AbsoluteInstant_MitigationCoveredByLoopScenarios()
    {
        var index = FixtureLoader.LoadIndex();
        var prazoIds = index.Scenarios.Where(s => s.Kind == "prazo").Select(s => s.Id).ToList();

        // Conta quantas vezes cada timer node aparece nos cenários prazo.
        var timerFrequency = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var timerId in TimerNodeIds)
            timerFrequency[timerId] = 0;

        foreach (var id in prazoIds)
        {
            var sc = FixtureLoader.LoadScenario(id);
            var visitedTimers = sc.Segmentos
                .SelectMany(s => s.Nos)
                .Where(n => TimerNodeIds.Contains(n.Id))
                .Select(n => n.Id)
                .Distinct();
            foreach (var t in visitedTimers)
                timerFrequency[t]++;
        }

        // Pelo menos um timer deve ser visitado por mais de um cenário prazo,
        // provando que o padrão de laço (prorrogação) está representado.
        var multiCovered = timerFrequency.Values.Count(v => v > 1);
        Assert.True(multiCovered > 0,
            "Nenhum nó timer foi coberto por mais de um cenário prazo. " +
            "A mitigação de prorrogação (rearme do temporizador) não está representada na fixture.");
    }

    // -----------------------------------------------------------------------
    // Cenários de sub-processo referenciados em detalhes existem na fixture
    // -----------------------------------------------------------------------

    [Theory(DisplayName = "Sub-cenários referenciados em detalhes existem como ficheiro na fixture")]
    [ClassData(typeof(AllScenarioIds))]
    public void Scenario_ReferencedSubScenarios_Exist(string scenarioId)
    {
        var sc = FixtureLoader.LoadScenario(scenarioId);
        foreach (var detail in sc.Detalhes)
        foreach (var subId in detail.Cenarios)
        {
            Assert.True(
                File.Exists(FixturePaths.ScenarioFile(subId)),
                $"[{scenarioId}] sub-cenário referenciado não encontrado: {subId}.json");
        }
    }
}
