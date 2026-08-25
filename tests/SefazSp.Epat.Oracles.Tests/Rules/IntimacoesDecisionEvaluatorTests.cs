#nullable enable

// Oracle: rules engine — fixture artifacts/POC_Epat/decision-tables.json (immutable Corticon fold).
// Proves IntimacoesDecisionEvaluator reproduces the ratified override fold: ALL matching columns
// fire in order, later writes win. An independent reference fold (in this test) is diffed against
// the evaluator across cases seeded from rules, and the test FAILS if no override case ever fires
// (the ratified "never trust a green run without an override" guard).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SefazSp.Epat.Application.Abstractions.Rules;
using SefazSp.Epat.Infrastructure.Rules.Dmn;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Rules;

public sealed class IntimacoesDecisionEvaluatorTests
{
    private sealed record RefCondition(string Attribute, string[] Values);
    private sealed record RefAction(string Attribute, string? Value);
    private sealed record RefRule(int Column, RefCondition[] Conditions, RefAction[] Actions);

    private static string Attr(string path) => path[(path.LastIndexOf('.') + 1)..];

    private static string? NormalizeRhs(JsonElement rhs) => rhs.ValueKind switch
    {
        JsonValueKind.String => rhs.GetString()!.Trim().Trim('\''),
        JsonValueKind.Number => rhs.GetRawText(),
        _ => rhs.GetRawText(),
    };

    private static IReadOnlyList<RefRule> LoadReferenceRules()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "artifacts", "POC_Epat", "decision-tables.json")))
            dir = dir.Parent;
        if (dir is null) throw new InvalidOperationException("decision-tables.json not found.");

        using var doc = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(dir.FullName, "artifacts", "POC_Epat", "decision-tables.json")));

        return doc.RootElement.GetProperty("rules").EnumerateArray().Select(r => new RefRule(
            r.GetProperty("column").GetInt32(),
            r.GetProperty("conditions").EnumerateArray().Select(c => new RefCondition(
                Attr(c.GetProperty("lhs").GetString()!),
                c.GetProperty("values").EnumerateArray().Select(v => v.GetString()!).ToArray())).ToArray(),
            r.GetProperty("actions").EnumerateArray().Select(a => new RefAction(
                Attr(a.GetProperty("lhs").GetString()!),
                NormalizeRhs(a.GetProperty("rhs")))).ToArray()))
            .OrderBy(r => r.Column).ToArray();
    }

    // Independent reference fold; also counts how many rules wrote each attribute (override detector).
    private static (Dictionary<string, string?> Response, int Writes) ReferenceFold(
        IReadOnlyList<RefRule> rules, IReadOnlyDictionary<string, string?> request)
    {
        var response = new Dictionary<string, string?>(StringComparer.Ordinal);
        var writes = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var rule in rules)
        {
            var matches = rule.Conditions.All(c =>
                request.TryGetValue(c.Attribute, out var v) && v is not null && c.Values.Contains(v));
            if (!matches) continue;
            foreach (var a in rule.Actions)
            {
                response[a.Attribute] = a.Value;
                writes[a.Attribute] = writes.GetValueOrDefault(a.Attribute) + 1;
            }
        }
        var overrides = writes.Values.Count(w => w >= 2);
        return (response, overrides);
    }

    [Fact(DisplayName = "IntimacoesDecisionEvaluator reproduces the Corticon override fold (with override cases firing)")]
    public void Evaluator_MatchesReferenceFold_AndFiresOverrides()
    {
        var refRules = LoadReferenceRules();
        var evaluator = new IntimacoesDecisionEvaluator();

        var overrideCases = 0;
        var casesChecked = 0;

        // Seed one case per rule: satisfy that rule's conditions with their first allowed value.
        foreach (var rule in refRules)
        {
            var request = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var c in rule.Conditions)
                request[c.Attribute] = c.Values[0];

            var (expected, overrides) = ReferenceFold(refRules, request);
            if (overrides > 0) overrideCases++;

            var actual = evaluator.Evaluate(IntimacoesRequest.From(request));

            Assert.Equal(
                expected.OrderBy(kv => kv.Key),
                actual.Attributes.OrderBy(kv => kv.Key));
            casesChecked++;
        }

        Assert.Equal(49, refRules.Count);
        Assert.True(casesChecked > 0, "No cases were checked.");
        Assert.True(overrideCases > 0, "No override case fired — the fold semantics are not being exercised.");
    }
}
