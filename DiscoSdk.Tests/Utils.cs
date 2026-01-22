using DiscoSdk.Models.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscoSdk.Tests;

public static class Utils
{
    public static T? Read<T>(this JsonConverter<T> converter, string json, Type targetType, JsonSerializerOptions options)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); 

        return converter.Read(ref reader, typeof(DiscordPermission), options);
    }
}
