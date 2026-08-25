#nullable enable

namespace SefazSp.Epat.Application.Abstractions.Rules;

/// <summary>
/// Pedido ao motor de regras Decisions (rulesheet Corticon intimacoes_Parametros).
/// Atributos indexados pelo nome normalizado (último segmento do caminho, p.ex. "motivoIntimacao").
/// Um atributo ausente ou nulo é tratado como não informado (SW_NA).
/// </summary>
public sealed record IntimacoesRequest(IReadOnlyDictionary<string, string?> Attributes)
{
    public static IntimacoesRequest From(IEnumerable<KeyValuePair<string, string?>> attrs)
        => new(new Dictionary<string, string?>(attrs, StringComparer.Ordinal));
}

/// <summary>
/// Resposta do motor de regras: os 11 atributos de saída após o fold de override.
/// Um atributo sem valor (nenhuma regra o escreveu) é <see langword="null"/> = SW_NA.
/// </summary>
public sealed record IntimacoesResponse(IReadOnlyDictionary<string, string?> Attributes)
{
    public string? Get(string attribute) => Attributes.TryGetValue(attribute, out var v) ? v : null;
}

/// <summary>
/// Motor de decisão da intimação (Etapa 3 — 'Integração com Decisions'), invocado via
/// PrepararIntimacao (DecisionsEPAT.wsdl) a partir de PRPINTPC/CaptaParametros.
///
/// <para>
/// Semântica Corticon (ratificada): TODAS as colunas que casam disparam, por ordem de coluna;
/// escritas posteriores sobrepõem as anteriores. Não é first-match. Célula '-' = don't-care;
/// atributo que nenhuma regra escreve fica não informado (SW_NA).
/// </para>
/// </summary>
public interface IIntimacoesDecision
{
    IntimacoesResponse Evaluate(IntimacoesRequest request);
}
