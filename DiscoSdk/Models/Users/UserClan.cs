using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Users;

/// Represents clan identity information attached to a user.
///
/// This structure describes an identity that is backed by a specific guild
/// and can be displayed on the user's profile.
/// </summary>
public sealed class UserClan
{
    /// <summary>
    /// The guild ID associated with this clan identity.
    /// </summary>
    [JsonPropertyName("identity_guild_id")]
    public Snowflake IdentityGuildId { get; set; }

    /// <summary>
    /// Indicates whether this identity is currently enabled for the user.
    /// </summary>
    [JsonPropertyName("identity_enabled")]
    public bool IdentityEnabled { get; set; }

    /// <summary>
    /// The short tag displayed for this clan identity.
    /// </summary>
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// The badge identifier or hash associated with this identity.
    /// </summary>
    [JsonPropertyName("badge")]
    public string? Badge { get; set; }
}