using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>Read-only user "primary guild" / clan-identity metadata.</summary>
public class PrimaryGuild
{
    /// <summary>The clan tag shown on the profile.</summary>
    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    /// <summary>Id of the guild backing this identity.</summary>
    [JsonPropertyName("identity_guild_id")]
    public Snowflake? IdentityGuildId { get; init; }

    /// <summary>Whether the user opted to display this identity.</summary>
    [JsonPropertyName("identity_enabled")]
    public bool IdentityEnabled { get; init; }

    /// <summary>Badge asset identifier.</summary>
    [JsonPropertyName("badge")]
    public string? Badge { get; init; }
}
