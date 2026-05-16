using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Users;

/// <summary>Read-only clan-identity metadata attached to a user.</summary>
public sealed class UserClan
{
    /// <summary>Guild backing this clan identity.</summary>
    [JsonPropertyName("identity_guild_id")]
    public Snowflake IdentityGuildId { get; init; }

    /// <summary>Whether the user opted to display this identity.</summary>
    [JsonPropertyName("identity_enabled")]
    public bool IdentityEnabled { get; init; }

    /// <summary>The short clan tag.</summary>
    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    /// <summary>Badge asset identifier.</summary>
    [JsonPropertyName("badge")]
    public string? Badge { get; init; }
}
