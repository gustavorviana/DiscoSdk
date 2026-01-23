using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway.Payloads.Models;

/// <summary>
/// Unavailable Guild object.
/// </summary>
internal sealed class UnavailableGuild
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("unavailable")]
    public bool Unavailable { get; set; }
}
