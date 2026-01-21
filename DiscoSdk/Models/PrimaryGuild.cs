using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a primary guild for a user.
/// </summary>
public class PrimaryGuild
{
    /// <summary>
    /// Gets or sets the tag.
    /// </summary>
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the identity guild ID.
    /// </summary>
    [JsonPropertyName("identity_guild_id")]
    public Snowflake? IdentityGuildId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether identity is enabled.
    /// </summary>
    [JsonPropertyName("identity_enabled")]
    public bool IdentityEnabled { get; set; }

    /// <summary>
    /// Gets or sets the badge.
    /// </summary>
    [JsonPropertyName("badge")]
    public string? Badge { get; set; }
}