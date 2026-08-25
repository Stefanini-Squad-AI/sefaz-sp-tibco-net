#nullable enable

namespace SefazSp.Epat.Application.Abstractions.Legacy;

/// <summary>
/// CAMADA ANTICORRUPCAO — o unico contrato onde os builtins do iProcess sobrevivem.
/// A implementacao vive em SefazSp.Epat.Infrastructure/Legacy; o resto do codigo
/// depende so desta porta, nunca do TIBCO.
///
/// Superficie derivada de artifacts/POC_Epat/builtin-contract.json:
/// 7 funcoes, 30 sitios de chamada em 40 scriptTasks.
///
/// INDICE BASE-1: SEARCH/SUBSTR reproduzem a convencao iProcess (base 1), a UNICA
/// que satisfaz o vector comportamental VEC-TOKENISE-PIPE-LIST — recortar
/// '278713|278712|' em ['278713','278712']. Confirmar contra a documentacao TIBCO
/// (rulings.BUILTIN-SEMANTICS) nao muda o codigo se o oraculo continuar a passar.
/// </summary>
public interface IProcessBuiltins
{
    // ── IPEConversionUtil ──────────────────────────────────────────────────
    /// <summary>STR(n, format) — converte numero para string. format=0 observado em todos os sitios.</summary>
    string Str(long value, int format);

    /// <summary>NUM(s) — converte string para numero inteiro.</summary>
    long Num(string value);

    /// <summary>DATESTR(d) — converte data para string.</summary>
    string DateStr(DateOnly date);

    // ── IPEDateTimeUtil ────────────────────────────────────────────────────
    /// <summary>
    /// CALCTIME(base, horas, minutos, dias) — soma horas e minutos a uma hora-base.
    /// O resultado e uma hora-do-dia (<see cref="TimeOnly"/>): a componente de dias
    /// rola dias inteiros e nao afecta o relogio. Aridade observada 4.
    /// </summary>
    TimeOnly CalcTime(TimeOnly baseTime, int hours, int minutes, int days);

    // ── IPEStringUtil (base-1, forcado pelo oraculo) ───────────────────────
    /// <summary>SEARCH(agulha, palheiro) — posicao base-1 da primeira ocorrencia; 0 se ausente.</summary>
    int Search(string needle, string haystack);

    /// <summary>SUBSTR(origem, inicio, comprimento) — inicio base-1, terceiro argumento e comprimento.</summary>
    string Substr(string source, int start, int length);

    /// <summary>STRLEN(s) — comprimento da string.</summary>
    int StrLen(string value);
}
