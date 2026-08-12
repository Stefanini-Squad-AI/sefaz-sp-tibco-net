#nullable enable

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules;

/// <summary>
/// RI-script-AGPECASPC-SetValues.
/// </summary>
public static class AgpecaspcSetValuesRule
{
    public static void Apply(AiimCase aiimCase)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);

        var activePecas = GetPecas(aiimCase)
            .Where(static peca => peca.Value.HasValue && peca.Value.Match(static value => value != "9", static () => false, static () => false))
            .ToArray();

        aiimCase.FIELDSNAMES = string.Join('|', activePecas.Select(static peca => peca.Name));
        aiimCase.FIELDSTYPES = string.Join('|', activePecas.Select(static _ => "STRING"));
        aiimCase.FIELDSVALUES = string.Join('|', activePecas.Select(static peca => peca.Value.Match(static value => value, static () => string.Empty, static () => string.Empty)));
        aiimCase.IDPECAS = string.Join(' ', activePecas.Select(static peca => peca.Name));
        aiimCase.PERIODOEMDIAS = activePecas.LongLength;
    }

    private static (string Name, FieldValue<string> Value)[] GetPecas(AiimCase aiimCase) =>
    [
        (nameof(aiimCase.CNTPECA1), aiimCase.CNTPECA1),
        (nameof(aiimCase.CNTPECA2), aiimCase.CNTPECA2),
        (nameof(aiimCase.CNTPECA3), aiimCase.CNTPECA3),
        (nameof(aiimCase.CNTPECA4), aiimCase.CNTPECA4)
    ];
}
