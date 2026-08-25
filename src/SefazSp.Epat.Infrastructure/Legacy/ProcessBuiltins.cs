#nullable enable

using System.Globalization;
using SefazSp.Epat.Application.Abstractions.Legacy;

namespace SefazSp.Epat.Infrastructure.Legacy;

/// <summary>
/// CAMADA ANTICORRUPCAO. Unico sitio do codigo onde os builtins do iProcess vivem.
///
/// SEARCH/SUBSTR sao BASE-1: e a unica convencao que satisfaz o vector comportamental
/// VEC-TOKENISE-PIPE-LIST de builtin-contract.json (recortar '278713|278712|' em
/// ['278713','278712']). rulings.BUILTIN-SEMANTICS continua por confirmar na documentacao
/// TIBCO; enquanto o oraculo passar, a escolha base-1 mantem-se.
/// </summary>
public sealed class ProcessBuiltins : IProcessBuiltins
{
    /// <inheritdoc />
    public string Str(long value, int format) => value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public long Num(string value) => long.Parse(value.Trim(), CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public string DateStr(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public TimeOnly CalcTime(TimeOnly baseTime, int hours, int minutes, int days)
        // TimeOnly.Add envolve a meia-noite; dias inteiros nao alteram a hora-do-dia.
        => baseTime.Add(TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes));

    /// <inheritdoc />
    public int Search(string needle, string haystack)
    {
        // Base-1: primeira ocorrencia devolve indice .NET + 1; ausencia devolve 0.
        var index = haystack.IndexOf(needle, StringComparison.Ordinal);
        return index < 0 ? 0 : index + 1;
    }

    /// <inheritdoc />
    public string Substr(string source, int start, int length)
    {
        // Base-1: 'start' 1 corresponde ao indice .NET 0. Terceiro argumento e comprimento.
        if (length <= 0)
            return string.Empty;

        var zeroBased = start - 1;
        if (zeroBased < 0 || zeroBased >= source.Length)
            return string.Empty;

        var available = source.Length - zeroBased;
        return source.Substring(zeroBased, Math.Min(length, available));
    }

    /// <inheritdoc />
    public int StrLen(string value) => value.Length;
}
