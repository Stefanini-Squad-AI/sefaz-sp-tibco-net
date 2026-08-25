#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Infrastructure.Persistence.Serialization;

/// <summary>
/// Round-trips the tri-state <see cref="FieldValue{T}"/> (HasValue / IsNotAvailable / Empty).
/// Default STJ collapses the SW_NA sentinel to null because the struct exposes no public
/// setter or parameterised ctor — which would silently flip a branch. See the shim-tri-state decision.
/// </summary>
public sealed class FieldValueJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType
           && typeToConvert.GetGenericTypeDefinition() == typeof(FieldValue<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(FieldValueJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class FieldValueJsonConverter<T> : JsonConverter<FieldValue<T>>
{
    private const string StateHasValue = "HasValue";
    private const string StateNotAvailable = "IsNotAvailable";
    private const string StateEmpty = "Empty";

    public override FieldValue<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return FieldValue<T>.Empty;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected object for FieldValue<{typeof(T).Name}>.");

        string? state = null;
        T? value = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var prop = reader.GetString();
            reader.Read();

            if (string.Equals(prop, "state", StringComparison.OrdinalIgnoreCase))
                state = reader.GetString();
            else if (string.Equals(prop, "value", StringComparison.OrdinalIgnoreCase))
                value = JsonSerializer.Deserialize<T>(ref reader, options);
        }

        return state switch
        {
            StateNotAvailable => FieldValue<T>.NotAvailable,
            StateEmpty => FieldValue<T>.Empty,
            StateHasValue => FieldValue<T>.Of(value!),
            _ => FieldValue<T>.Empty,
        };
    }

    public override void Write(Utf8JsonWriter writer, FieldValue<T> value, JsonSerializerOptions options)
    {
        var (state, payload, hasPayload) = value.Match(
            v => (StateHasValue, v, true),
            () => (StateNotAvailable, default(T)!, false),
            () => (StateEmpty, default(T)!, false));

        writer.WriteStartObject();
        writer.WriteString("state", state);
        if (hasPayload)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, payload, options);
        }
        writer.WriteEndObject();
    }
}
