#nullable enable

// Oracle: composition path — fixture artifacts/POC_Epat/scenarios/SC-POC_EpatProcess-001.json (immutable).
// Proves the composed main POC_EpatProcess journey (Phase 1) equals the enumerated SC-001 node path.
// The orchestrator (Infrastructure/Workflow.Elsa/PocEpat/PocEpatMainActivity.Sc001NodePath) walks
// exactly this sequence; the runtime match is asserted at run time and this test pins the expected
// sequence to the immutable oracle so neither can drift silently.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Composition;

public sealed class PocEpatSc001JourneyTests
{
    // Canonical SC-001 journey (30 nodes, Etapas 1→7) — same sequence the orchestrator emits.
    private static readonly string[] ExpectedPath =
    {
        "_OAgPol9UEfG6Lfb98zsREQ", "_XWivF1qTEfG5K7mY0I3I6w", "_sfwu-VqUEfG5K7mY0I3I6w",
        "_sJqYklqTEfG5K7mY0I3I6w", "_tN6q4lqTEfG5K7mY0I3I6w", "_5E444FqTEfG5K7mY0I3I6w",
        "_xWNLe1qSEfG5K7mY0I3I6w", "_Faq_RFqTEfG5K7mY0I3I6w", "_IxqJMlqTEfG5K7mY0I3I6w",
        "_Faq_RVqTEfG5K7mY0I3I6w", "_0XWagFqNEfG5K7mY0I3I6w", "_0XWahVqNEfG5K7mY0I3I6w",
        "_0XWagVqNEfG5K7mY0I3I6w", "_0XWahFqNEfG5K7mY0I3I6w", "_LeuhgFqVEfG5K7mY0I3I6w",
        "_CI6l0VqREfG5K7mY0I3I6w", "_CI6lx1qREfG5K7mY0I3I6w", "_CI6lyFqREfG5K7mY0I3I6w",
        "_G4hU81qhEfG5K7mY0I3I6w", "_6WNq-lqgEfG5K7mY0I3I6w", "_30jAcFqVEfG5K7mY0I3I6w",
        "_89MVQlqVEfG5K7mY0I3I6w", "_Ei94AFqPEfG5K7mY0I3I6w", "_CtQ7BFqPEfG5K7mY0I3I6w",
        "_CtQ6-1qPEfG5K7mY0I3I6w", "_CtQ6_VqPEfG5K7mY0I3I6w", "_CtQ6-lqPEfG5K7mY0I3I6w",
        "_zE3XeV6JEfGBBLgT-R5iuw", "_nQntZ16JEfGBBLgT-R5iuw", "_H22mclqWEfG5K7mY0I3I6w",
    };

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "artifacts", "POC_Epat", "scenarios", "index.json")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Repo root not found from " + AppContext.BaseDirectory);
    }

    private static List<string> LoadFixturePath(string scenarioId)
    {
        var file = Path.Combine(RepoRoot(), "artifacts", "POC_Epat", "scenarios", scenarioId + ".json");
        using var doc = JsonDocument.Parse(File.ReadAllText(file));
        return doc.RootElement.GetProperty("path")
            .EnumerateArray()
            .Select(n => n.GetProperty("id").GetString()!)
            .ToList();
    }

    // Shared prefix (nodes 1–24) is SC-001 up to and including 'Vistas do Juiz ?'.
    private static IEnumerable<string> SharedPrefix() => ExpectedPath.Take(24);

    [Fact(DisplayName = "Composed POC_EpatProcess journey equals the immutable SC-001 node path (30 nodes)")]
    public void ComposedJourney_EqualsSc001Oracle()
    {
        var fixturePath = LoadFixturePath("SC-POC_EpatProcess-001");

        Assert.Equal(30, fixturePath.Count);
        Assert.Equal(ExpectedPath, fixturePath);
    }

    [Fact(DisplayName = "MISTA branch equals the immutable SC-012 node path (29 nodes)")]
    public void MistaBranch_EqualsSc012Oracle()
    {
        var expected = SharedPrefix().Concat(new[]
        {
            "_CtQ7BVqPEfG5K7mY0I3I6w", "_tbOD4FqPEfG5K7mY0I3I6w", "_InbWgFqQEfG5K7mY0I3I6w",
            "_CtQ67FqPEfG5K7mY0I3I6w", "_CtQ66lqPEfG5K7mY0I3I6w",
        }).ToList();

        var fixturePath = LoadFixturePath("SC-POC_EpatProcess-012");

        Assert.Equal(29, fixturePath.Count);
        Assert.Equal(expected, fixturePath);
    }

    [Fact(DisplayName = "DRF branch (timer wins) equals the immutable SC-010 node path (30 nodes)")]
    public void DrfBranch_EqualsSc010Oracle()
    {
        var expected = SharedPrefix().Concat(new[]
        {
            "_CtQ7BVqPEfG5K7mY0I3I6w", "_CtQ68lqPEfG5K7mY0I3I6w", "_CtQ7A1qPEfG5K7mY0I3I6w",
            "_CtQ66FqPEfG5K7mY0I3I6w", "_WvTQIFqQEfG5K7mY0I3I6w", "_Xw86YlqQEfG5K7mY0I3I6w",
        }).ToList();

        var fixturePath = LoadFixturePath("SC-POC_EpatProcess-010");

        Assert.Equal(30, fixturePath.Count);
        Assert.Equal(expected, fixturePath);
    }

    [Fact(DisplayName = "Existe Notificação?=Sim short-circuit equals the immutable SC-014 node path (10 nodes)")]
    public void ExisteNotificacaoSim_EqualsSc014Oracle()
    {
        // SC-014: nodes 1–9 (shared with SC-001) + endEvent (Sim branch).
        var expected = ExpectedPath.Take(9).Concat(new[] { "_Faq_Q1qTEfG5K7mY0I3I6w" }).ToList();

        var fixturePath = LoadFixturePath("SC-POC_EpatProcess-014");

        Assert.Equal(10, fixturePath.Count);
        Assert.Equal(expected, fixturePath);
    }

    [Fact(DisplayName = "Corrigir?=No branch equals the immutable SC-015 node path (6 nodes)")]
    public void CorrigirNo_EqualsSc015Oracle()
    {
        // SC-015: nodes 1–4 (shared with SC-001) + Criar Notificacao (CRNOTPC) + endEvent.
        var expected = ExpectedPath.Take(4).Concat(new[]
        {
            "_BQIgAF9KEfGqPfX31TKC3w", "_O7K3MF9LEfGqPfX31TKC3w",
        }).ToList();

        var fixturePath = LoadFixturePath("SC-POC_EpatProcess-015");

        Assert.Equal(6, fixturePath.Count);
        Assert.Equal(expected, fixturePath);
    }
}
