#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SefazSp.Epat.Infrastructure.Persistence.Serialization;

/// <summary>
/// Single source of the JSON contract for durable ePAT snapshots. Registers the tri-state
/// <see cref="FieldValueJsonConverterFactory"/>, includes public fields (e.g. the race/flag
/// fields on the snapshot), and populates get-only collections (Path, DecisionsSeed, Case)
/// so a restored snapshot equals the one that was suspended.
/// </summary>
public static class EpatJsonSerialization
{
    public static readonly JsonSerializerOptions Options = Build();

    private static JsonSerializerOptions Build()
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new FieldValueJsonConverterFactory());
        return options;
    }
}
