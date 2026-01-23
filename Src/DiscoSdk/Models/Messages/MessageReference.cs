using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a message reference.
/// </summary>
public class MessageReference
{
    /// <summary>
    /// Gets or sets the ID of the originating message.
    /// </summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the originating message's channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the originating message's guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message reference may not exist.
    /// </summary>
    [JsonPropertyName("fail_if_not_exists")]
    public bool? FailIfNotExists { get; set; }
}