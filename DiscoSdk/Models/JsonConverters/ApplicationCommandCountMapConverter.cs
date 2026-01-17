using DiscoSdk.Models.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.JsonConverters;

// Converter MUST target the property type: Dictionary<ApplicationCommandType,int>
public sealed class ApplicationCommandCountMapConverter
    : JsonConverter<Dictionary<ApplicationCommandType, int>>
{
    public override Dictionary<ApplicationCommandType, int> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new();

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected object for application_command_counts.");

        var dict = new Dictionary<ApplicationCommandType, int>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return dict;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name.");

            var keyString = reader.GetString() ?? throw new JsonException("Null key.");
            var key = keyString switch
            {
                "chat_input" => ApplicationCommandType.ChatInput,
                "user" => ApplicationCommandType.User,
                "message" => ApplicationCommandType.Message,
                _ => throw new JsonException($"Unknown application command type key: '{keyString}'")
            };

            reader.Read();

            if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out var value))
                throw new JsonException($"Expected int value for '{keyString}'.");

            dict[key] = value;
        }

        throw new JsonException("Incomplete JSON object.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        Dictionary<ApplicationCommandType, int> value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        foreach (var kv in value)
        {
            var key = kv.Key switch
            {
                ApplicationCommandType.ChatInput => "chat_input",
                ApplicationCommandType.User => "user",
                ApplicationCommandType.Message => "message",
                _ => throw new ArgumentOutOfRangeException(nameof(kv.Key), kv.Key, null)
            };

            writer.WriteNumber(key, kv.Value);
        }

        writer.WriteEndObject();
    }
}
