#nullable enable

// fundacao-motor-de-regras — avaliador da rulesheet Corticon 'intimacoes_Parametros' (Decisions).
// Fonte: artifacts/POC_Epat/decision-tables.json (fold Corticon, autoritativo), embutida como recurso.
// Semântica ratificada: override fold — todas as colunas que casam disparam por ordem; escritas
// posteriores sobrepõem. Célula '-' = don't-care; atributo não escrito fica SW_NA (null).

using System.Reflection;
using System.Text.Json;
using SefazSp.Epat.Application.Abstractions.Rules;

namespace SefazSp.Epat.Infrastructure.Rules.Dmn;

/// <summary>
/// Avaliador data-driven da tabela de decisão da intimação (49 regras × 21 condições → 11 saídas).
/// As regras vêm de <c>decision-tables.json</c> (recurso embutido); nenhuma regra é codificada.
/// </summary>
public sealed class IntimacoesDecisionEvaluator : IIntimacoesDecision
{
    private const string ResourceName = "SefazSp.Epat.Infrastructure.Rules.Dmn.decision-tables.json";

    private readonly IReadOnlyList<Rule> _rules;

    public IntimacoesDecisionEvaluator() => _rules = LoadRules();

    public IntimacoesResponse Evaluate(IntimacoesRequest request)
    {
        var response = new Dictionary<string, string?>(StringComparer.Ordinal);

        // Override fold: percorre as colunas por ordem; cada regra que casa sobrepõe as saídas.
        foreach (var rule in _rules)
        {
            if (rule.Conditions.All(c => Matches(c, request)))
                foreach (var action in rule.Actions)
                    response[action.Attribute] = action.Value;
        }

        return new IntimacoesResponse(response);
    }

    private static bool Matches(Condition c, IntimacoesRequest req)
    {
        var actual = req.Attributes.TryGetValue(c.Attribute, out var v) ? v : null;
        // equals (1 valor) e inSet (N valores) reduzem-se a "valor actual ∈ valores da condição".
        return actual is not null && c.Values.Contains(actual);
    }

    private static IReadOnlyList<Rule> LoadRules()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Recurso de regras não encontrado: {ResourceName}.");
        using var doc = JsonDocument.Parse(stream);

        var rules = new List<Rule>();
        foreach (var r in doc.RootElement.GetProperty("rules").EnumerateArray())
        {
            var column = r.GetProperty("column").GetInt32();

            var conditions = r.GetProperty("conditions").EnumerateArray()
                .Select(c => new Condition(
                    Attribute: Attr(c.GetProperty("lhs").GetString()!),
                    Values: c.GetProperty("values").EnumerateArray().Select(v => v.GetString()!).ToArray()))
                .ToArray();

            var actions = r.GetProperty("actions").EnumerateArray()
                .Select(a => new Action(
                    Attribute: Attr(a.GetProperty("lhs").GetString()!),
                    Value: NormalizeRhs(a.GetProperty("rhs"))))
                .ToArray();

            rules.Add(new Rule(column, conditions, actions));
        }

        return rules.OrderBy(r => r.Column).ToArray();
    }

    // Último segmento do caminho: 'ResultadoJulgamento.request.motivoIntimacao' → 'motivoIntimacao'.
    private static string Attr(string path) => path[(path.LastIndexOf('.') + 1)..];

    // rhs vem como string entre aspas simples ('2') ou número (1); normaliza para o código de domínio.
    private static string? NormalizeRhs(JsonElement rhs) => rhs.ValueKind switch
    {
        JsonValueKind.String => rhs.GetString()!.Trim().Trim('\''),
        JsonValueKind.Number => rhs.GetRawText(),
        _ => rhs.GetRawText(),
    };

    private sealed record Rule(int Column, IReadOnlyList<Condition> Conditions, IReadOnlyList<Action> Actions);
    private sealed record Condition(string Attribute, IReadOnlyList<string> Values);
    private sealed record Action(string Attribute, string? Value);
}
