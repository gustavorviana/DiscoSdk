using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

public sealed class GuildProfile
{
    /// <summary>
    /// The user's nickname in the guild.
    /// </summary>
    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    /// <summary>
    /// The user's guild-specific avatar hash.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// The user's guild-specific banner hash.
    /// </summary>
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    /// <summary>
    /// The user's guild profile bio.
    /// </summary>
    [JsonPropertyName("bio")]
    public string? Bio { get; set; }
}
