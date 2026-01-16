using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for <see cref="Snowflake"/> that serializes/deserializes to/from string.
/// </summary>
public class SnowflakeConverter : JsonConverter<Snowflake>
{
    /// <inheritdoc />
    public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            return Snowflake.TryParse(value, out var id) ? id : default;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetUInt64();
            return new Snowflake(value);
        }

        return default;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}