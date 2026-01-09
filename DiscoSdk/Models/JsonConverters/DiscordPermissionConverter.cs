using DiscoSdk.Models.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

public sealed class DiscordPermissionConverter : JsonConverter<DiscordPermission>
{
    public override DiscordPermission Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return (DiscordPermission)reader.GetUInt64();

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (ulong.TryParse(value, out var permissions))
                return (DiscordPermission)permissions;
        }

        throw new JsonException("Invalid Discord permission value.");
    }

    public override void Write(Utf8JsonWriter writer, DiscordPermission value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(((ulong)value).ToString());
    }
}
