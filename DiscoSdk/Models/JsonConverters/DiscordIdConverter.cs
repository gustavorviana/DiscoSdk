using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

/// <summary>
/// JSON converter for <see cref="DiscordId"/> that serializes/deserializes to/from string.
/// </summary>
public class DiscordIdConverter : JsonConverter<DiscordId>
{
    /// <inheritdoc />
    public override DiscordId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            return DiscordId.TryParse(value, out var id) ? id : default;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetUInt64();
            return new DiscordId(value);
        }

        return default;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DiscordId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}