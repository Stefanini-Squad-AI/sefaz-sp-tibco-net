// Tests/SefazSp.Epat.Oracles.Tests/EquivalenciaCorticonDmnTests.cs
// Oracle: decision-table | Fixture: artifacts/POC_Epat/dmn | CaseCount: 1
// Proves DMN equivalence to Corticon planilha. Values are immutable — never edit expected outputs here.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests;

/// <summary>
/// Oracle permanente: prova que o DMN gerado (artifacts/POC_Epat/dmn) é estruturalmente
/// equivalente às planilhas Corticon (decision-tables.json).
/// Cada coluna do Corticon deve aparecer no DMN com as mesmas condições e valores de saída.
/// Este teste não pode ser marcado com Skip nem depender de flags de feature.
/// </summary>
public sealed class EquivalenciaCorticonDmnTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string FixtureDmnPath = Path.Combine(RepoRoot, "artifacts", "POC_Epat", "dmn", "intimacoes_parametros.dmn");
    private static readonly string FixtureTablesPath = Path.Combine(RepoRoot, "artifacts", "POC_Epat", "decision-tables.json");

    // DMN namespace
    private static readonly XNamespace DmnNs = "https://www.omg.org/spec/DMN/20191111/MODEL/";

    // Corticon single-quoted string: 'value' → value
    private static readonly Regex CorticonStringRx = new(@"^'(.*)'$", RegexOptions.Compiled);
    // Corticon inSet: {'a', 'b'} → a, b
    private static readonly Regex CorticonSetRx = new(@"^\{(.*)\}$", RegexOptions.Compiled);

    [Fact]
    public void EquivalenciaDmn_vs_Corticon_ZeroDivergencias()
    {
        // ── Load Corticon rules ──────────────────────────────────────────────
        using var tablesStream = File.OpenRead(FixtureTablesPath);
        using var tablesDoc = JsonDocument.Parse(tablesStream);
        var cortRules = ParseCorticonRules(tablesDoc.RootElement);

        // ── Load DMN ─────────────────────────────────────────────────────────
        var dmnDoc = XDocument.Load(FixtureDmnPath);
        var dmnDecisions = ParseDmnDecisions(dmnDoc);

        // ── Compare ──────────────────────────────────────────────────────────
        var divergences = new List<string>();

        foreach (var cortRule in cortRules)
        {
            // For each action attribute written by this Corticon rule:
            foreach (var action in cortRule.Actions)
            {
                if (!dmnDecisions.TryGetValue(action.Attribute, out var dmnDecision))
                {
                    divergences.Add($"Col.{cortRule.Column} attr={action.Attribute}: DMN decision not found");
                    continue;
                }

                var dmnRule = dmnDecision.Rules.FirstOrDefault(r => r.CorticonColumn == cortRule.Column);
                if (dmnRule is null)
                {
                    divergences.Add($"Col.{cortRule.Column} attr={action.Attribute}: no DMN rule annotated 'col. {cortRule.Column}'");
                    continue;
                }

                // Check output value
                var expectedOutput = NormaliseCorticonOutput(action.Rhs);
                var actualOutput = NormaliseDmnOutput(dmnRule.OutputValue);
                if (expectedOutput != actualOutput)
                {
                    divergences.Add(
                        $"Col.{cortRule.Column} attr={action.Attribute}: output mismatch " +
                        $"corticon={expectedOutput} dmn={actualOutput}");
                }

                // Check conditions: every Corticon condition must appear in the DMN rule
                foreach (var cond in cortRule.Conditions)
                {
                    if (!dmnDecision.InputExpressions.TryGetValue(cond.Lhs, out var colIndex))
                    {
                        divergences.Add(
                            $"Col.{cortRule.Column} attr={action.Attribute}: " +
                            $"DMN decision '{action.Attribute}' has no input column for '{cond.Lhs}'");
                        continue;
                    }

                    if (colIndex >= dmnRule.InputValues.Count)
                    {
                        divergences.Add(
                            $"Col.{cortRule.Column} attr={action.Attribute}: " +
                            $"DMN rule has fewer input entries than expected (index {colIndex})");
                        continue;
                    }

                    var dmnInputVal = dmnRule.InputValues[colIndex];
                    var expectedCond = NormaliseCorticonCondition(cond.Rhs, cond.MatchType);
                    var actualCond = NormaliseDmnCondition(dmnInputVal);

                    if (!ConditionSetsMatch(expectedCond, actualCond))
                    {
                        divergences.Add(
                            $"Col.{cortRule.Column} attr={action.Attribute} input={cond.Lhs}: " +
                            $"condition mismatch corticon={expectedCond} dmn={actualCond}");
                    }
                }
            }
        }

        // Oracle report: casos=1, divergencias esperadas=0
        var report = $"Oracle decision-table: fixture=artifacts/POC_Epat/dmn, casos=1, divergencias={divergences.Count}";

        Assert.True(
            divergences.Count == 0,
            report + Environment.NewLine + string.Join(Environment.NewLine, divergences));
    }

    // ── Parsers ───────────────────────────────────────────────────────────────

    private static List<CorticonRule> ParseCorticonRules(JsonElement root)
    {
        var rules = new List<CorticonRule>();
        foreach (var ruleEl in root.GetProperty("rules").EnumerateArray())
        {
            var col = ruleEl.GetProperty("column").GetInt32();
            var conditions = ruleEl.GetProperty("conditions").EnumerateArray()
                .Select(c => new CorticonCondition(
                    c.GetProperty("lhs").GetString()!,
                    c.GetProperty("rhs").GetString()!,
                    c.GetProperty("matchType").GetString()!))
                .ToList();
            var actions = ruleEl.GetProperty("actions").EnumerateArray()
                .Select(a => new CorticonAction(
                    a.GetProperty("lhs").GetString()!.Split('.').Last(),
                    a.GetProperty("rhs").GetString()!))
                .ToList();
            rules.Add(new CorticonRule(col, conditions, actions));
        }
        return rules;
    }

    private static Dictionary<string, DmnDecision> ParseDmnDecisions(XDocument dmnDoc)
    {
        var result = new Dictionary<string, DmnDecision>();
        var root = dmnDoc.Root!;

        foreach (var decisionEl in root.Elements(DmnNs + "decision"))
        {
            var name = decisionEl.Attribute("name")!.Value;
            var tableEl = decisionEl.Descendants(DmnNs + "decisionTable").First();

            // Map input expression text → column index
            var inputExprs = tableEl.Elements(DmnNs + "input")
                .Select((inp, idx) => (
                    Expr: inp.Element(DmnNs + "inputExpression")!
                              .Element(DmnNs + "text")!.Value,
                    Index: idx))
                .ToDictionary(x => x.Expr, x => x.Index);

            // Parse rules
            var dmnRules = new List<DmnRule>();
            foreach (var ruleEl in tableEl.Elements(DmnNs + "rule"))
            {
                var ann = ruleEl.Element(DmnNs + "annotationEntry")
                                ?.Element(DmnNs + "text")?.Value ?? string.Empty;
                var colNum = ParseCorticonColumnAnnotation(ann);

                var inputVals = ruleEl.Elements(DmnNs + "inputEntry")
                    .Select(ie => ie.Element(DmnNs + "text")?.Value ?? "-")
                    .ToList();

                var outputVal = ruleEl.Elements(DmnNs + "outputEntry")
                    .Select(oe => oe.Element(DmnNs + "text")?.Value ?? string.Empty)
                    .FirstOrDefault() ?? string.Empty;

                dmnRules.Add(new DmnRule(colNum, inputVals, outputVal));
            }

            result[name] = new DmnDecision(name, inputExprs, dmnRules);
        }

        return result;
    }

    private static int ParseCorticonColumnAnnotation(string annotation)
    {
        var m = Regex.Match(annotation.Trim(), @"^col\.\s*(\d+)$");
        return m.Success ? int.Parse(m.Groups[1].Value) : -1;
    }

    // ── Normalisation helpers ─────────────────────────────────────────────────

    // Corticon output: '2' → 2  |  1 → 1
    private static string NormaliseCorticonOutput(string rhs)
    {
        var m = CorticonStringRx.Match(rhs);
        return m.Success ? m.Groups[1].Value : rhs.Trim();
    }

    // DMN output: "2" → 2  |  1 → 1
    private static string NormaliseDmnOutput(string val)
    {
        return val.Trim('"');
    }

    // Returns a canonical (sorted, pipe-joined) set string for comparison
    private static string NormaliseCorticonCondition(string rhs, string matchType)
    {
        if (matchType == "equals")
        {
            var m = CorticonStringRx.Match(rhs);
            return m.Success ? m.Groups[1].Value : rhs.Trim();
        }
        // inSet: {'0', '1'} → sorted values joined with |
        var setMatch = CorticonSetRx.Match(rhs);
        if (setMatch.Success)
        {
            var values = setMatch.Groups[1].Value
                .Split(',', StringSplitOptions.TrimEntries)
                .Select(v => CorticonStringRx.Match(v) is { Success: true } m2 ? m2.Groups[1].Value : v.Trim('\''))
                .OrderBy(v => v)
                .ToList();
            return string.Join("|", values);
        }
        return rhs.Trim();
    }

    // DMN condition: "-" → skip | "2" → 2 | "0","1" → 0|1
    private static string NormaliseDmnCondition(string val)
    {
        if (val == "-" || string.IsNullOrEmpty(val)) return "-";
        var parts = val.Split(',', StringSplitOptions.TrimEntries)
            .Select(p => p.Trim('"'))
            .OrderBy(p => p)
            .ToList();
        return string.Join("|", parts);
    }

    // A Corticon don't-care ('-') is only implied by absence from conditions list.
    // When we have an explicit condition, the DMN must also have it (non '-').
    private static bool ConditionSetsMatch(string cortNorm, string dmnNorm)
    {
        if (dmnNorm == "-")
            return false; // Corticon has a specific condition; DMN must not be don't-care
        return cortNorm == dmnNorm;
    }

    // ── Repo root discovery ───────────────────────────────────────────────────

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir, "artifacts")) &&
                Directory.Exists(Path.Combine(dir, "artifacts", "POC_Epat")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        throw new DirectoryNotFoundException("Could not locate repo root containing artifacts/POC_Epat");
    }

    // ── Domain models ─────────────────────────────────────────────────────────

    private sealed record CorticonRule(int Column, List<CorticonCondition> Conditions, List<CorticonAction> Actions);
    private sealed record CorticonCondition(string Lhs, string Rhs, string MatchType);
    private sealed record CorticonAction(string Attribute, string Rhs);
    private sealed record DmnDecision(string Name, Dictionary<string, int> InputExpressions, List<DmnRule> Rules);
    private sealed record DmnRule(int CorticonColumn, List<string> InputValues, string OutputValue);
}
