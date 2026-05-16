using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IVanityUrl"/>
internal sealed class VanityUrl : IVanityUrl
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("uses")]
    public int Uses { get; init; }
}
