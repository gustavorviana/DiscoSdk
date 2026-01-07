using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a thread member.
/// </summary>
public class ThreadMember
{
    /// <summary>
    /// Gets or sets the ID of the thread.
    /// </summary>
    [JsonPropertyName("id")]
    public DiscordId? Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the time the user last joined the thread.
    /// </summary>
    [JsonPropertyName("join_timestamp")]
    public string JoinTimestamp { get; set; } = default!;

    /// <summary>
    /// Gets or sets any user-thread settings.
    /// </summary>
    [JsonPropertyName("flags")]
    public ThreadMemberFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets additional information about the user.
    /// </summary>
    [JsonPropertyName("member")]
    public GuildMember? Member { get; set; }
}