using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a member of a Discord guild.
/// </summary>
public class GuildMember
{
    /// <summary>
    /// Gets or sets the user this guild member represents.
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the user's guild nickname.
    /// </summary>
    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    /// <summary>
    /// Gets or sets the member's guild avatar hash.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// Gets or sets the array of role object IDs.
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets when the user joined the guild.
    /// </summary>
    [JsonPropertyName("joined_at")]
    public string JoinedAt { get; set; } = default!;

    /// <summary>
    /// Gets or sets when the user started boosting the guild.
    /// </summary>
    [JsonPropertyName("premium_since")]
    public string? PremiumSince { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is deafened in voice channels.
    /// </summary>
    [JsonPropertyName("deaf")]
    public bool? Deaf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is muted in voice channels.
    /// </summary>
    [JsonPropertyName("mute")]
    public bool? Mute { get; set; }

    /// <summary>
    /// Gets or sets the member's flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public GuildMemberFlags? Flags { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has not yet passed the guild's Membership Screening requirements.
    /// </summary>
    [JsonPropertyName("pending")]
    public bool? Pending { get; set; }

    /// <summary>
    /// Gets or sets total permissions of the member in the channel, including overwrites.
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonConverter(typeof(PermissionStringNullableConverter))]
    public string? Permissions { get; set; }

    /// <summary>
    /// Gets or sets when the user's timeout will expire.
    /// </summary>
    [JsonPropertyName("communication_disabled_until")]
    public string? CommunicationDisabledUntil { get; set; }
}

