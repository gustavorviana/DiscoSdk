using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Envelope returned by <c>GET /guilds/:id/soundboard-sounds</c>. Discord wraps the
/// soundboard list in a single-field object instead of returning a bare array.
/// </summary>
internal sealed class SoundboardSoundListResponse
{
    [JsonPropertyName("items")]
    public SoundboardSound[] Items { get; set; } = [];
}
