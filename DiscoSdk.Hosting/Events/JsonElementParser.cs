using DiscoSdk.Models;
using System.Text.Json;

namespace DiscoSdk.Hosting.Events;

internal readonly struct JsonElementParser(JsonElement payload)
{
    public JsonElement Payload => payload;
    public string? GetString(string key) => payload.GetProperty(key).GetString() ?? string.Empty;
    public Snowflake? GetSnowflake(string key)
    {
        var item = Get(key);
        if (item == null)
            return null;

        if (item.Value.ValueKind == JsonValueKind.Number)
            return new Snowflake(item.Value.GetUInt64());

        if (Snowflake.TryParse(item.ToString(), out var snowflake))
            return snowflake;

        return null;
    }

    public DateTimeOffset? GetDateTimeOffset(string key)
    {
        var item = Get(key);
        if (item == null)
            return null;

        if (item.Value.ValueKind == JsonValueKind.Number)
            return DateTimeOffset.FromUnixTimeSeconds(item.Value.GetInt32());

        if (item.Value.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(item.ToString(), out var value))
            return value;

        return null;
    }

    public TValue? Deserialize<TValue>(string key, JsonSerializerOptions options)
    {
        var item = Get(key);
        if (item is null || item.Value.ValueKind == JsonValueKind.Null)
            return default;

        return item.Value.Deserialize<TValue>(options);
    }

    public JsonElement? Get(string key)
    {
        if (payload.TryGetProperty(key, out var value))
            return value;

        return null;
    }

    public override string ToString()
    {
        return payload.ToString();
    }
}