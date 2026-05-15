using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a message reference — used for replies, forwards, crossposts, channel-follows
/// and pinned messages. See <see cref="MessageReferenceType"/> for the distinction between
/// the historical default behaviour and the forward semantics added later.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/message#message-reference-structure"/>.
/// </remarks>
public class MessageReference
{
    /// <summary>
    /// Kind of reference. Defaults to <see cref="MessageReferenceType.Default"/> when omitted —
    /// matches the historical (pre-forward) shape Discord still emits on existing replies.
    /// Set to <see cref="MessageReferenceType.Forward"/> when sending a forwarded message.
    /// </summary>
    [JsonPropertyName("type")]
    public MessageReferenceType? Type { get; set; }

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