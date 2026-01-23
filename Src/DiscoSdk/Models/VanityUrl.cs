using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

public sealed class VanityUrl
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("uses")]
    public int Uses { get; set; }
}