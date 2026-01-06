using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter that converts InteractionOption.Value to the appropriate type (int, bool, string, double).
/// </summary>
public class InteractionOptionValueConverter : JsonConverter<object?>
{
    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => ConvertNumber(ref reader),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (value)
        {
            case string str:
                writer.WriteStringValue(str);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            default:
                JsonSerializer.Serialize(writer, value, options);
                break;
        }
    }

    private static object ConvertNumber(ref Utf8JsonReader reader)
    {
        // Try to get as int32 first, then int64, then double
        if (reader.TryGetInt32(out var intValue))
        {
            return intValue;
        }

        if (reader.TryGetInt64(out var longValue))
        {
            return longValue;
        }

        return reader.GetDouble();
    }
}

