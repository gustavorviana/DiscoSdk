using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway.Payloads.Models;

/// <summary>
/// Partial application object.
/// </summary>
public sealed class ReadyApplication
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("flags")]
    public int Flags { get; set; }
}
