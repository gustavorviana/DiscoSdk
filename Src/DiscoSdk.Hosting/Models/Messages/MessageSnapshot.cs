using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Mentions;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// A frozen subset of a <see cref="Message"/> embedded inside a forwarded message via
/// <see cref="Message.MessageSnapshots"/>. Discord populates the snapshot at forward time;
/// editing or deleting the original source message does <strong>not</strong> propagate to the
/// snapshot.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/message#message-snapshot-object"/>.
/// The wire shape is <c>{ "message": { ... subset ... } }</c> — this class flattens the inner
/// object via <see cref="Message"/> into <see cref="Message"/>.
/// </remarks>
internal sealed class MessageSnapshot
{
    /// <summary>
    /// The frozen subset of the source message. Discord includes only a curated set of fields:
    /// <c>type</c>, <c>content</c>, <c>embeds</c>, <c>attachments</c>, <c>timestamp</c>,
    /// <c>edited_timestamp</c>, <c>flags</c>, <c>mentions</c>, <c>mention_roles</c>,
    /// <c>sticker_items</c>, <c>components</c>. Other fields (id, channel_id, author, …) are
    /// intentionally absent.
    /// </summary>
    [JsonPropertyName("message")]
    public MessageSnapshotPayload Message { get; set; } = new();
}

/// <summary>
/// Curated subset of <see cref="Message"/> fields Discord includes in a
/// <see cref="MessageSnapshot"/>. Modelled as a separate class because it intentionally omits
/// id/channel/author/guild ids — a snapshot has no identity of its own.
/// </summary>
internal sealed class MessageSnapshotPayload
{
    /// <summary>Source message type at the moment of forwarding.</summary>
    [JsonPropertyName("type")]
    public MessageType Type { get; set; }

    /// <summary>Frozen text content of the source message.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>Frozen embeds of the source message (up to Discord's max).</summary>
    [JsonPropertyName("embeds")]
    public Embed[] Embeds { get; set; } = [];

    /// <summary>Frozen attachments of the source message.</summary>
    [JsonPropertyName("attachments")]
    public Attachment[] Attachments { get; set; } = [];

    /// <summary>When the source message was originally sent.</summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = default!;

    /// <summary>When the source message was last edited prior to the forward, if applicable.</summary>
    [JsonPropertyName("edited_timestamp")]
    public string? EditedTimestamp { get; set; }

    /// <summary>Flags of the source message at the moment of forwarding.</summary>
    [JsonPropertyName("flags")]
    public MessageFlags Flags { get; set; }

    /// <summary>Users mentioned by the source message.</summary>
    [JsonPropertyName("mentions")]
    public MessageMentionUser[] Mentions { get; set; } = [];

    /// <summary>Role IDs mentioned by the source message.</summary>
    [JsonPropertyName("mention_roles")]
    public List<string> MentionRoles { get; set; } = [];

    /// <summary>Frozen sticker items of the source message.</summary>
    [JsonPropertyName("sticker_items")]
    public StickerItem[]? StickerItems { get; set; }

    /// <summary>Frozen components of the source message (V1 action rows or V2 components).</summary>
    [JsonPropertyName("components")]
    [JsonConverter(typeof(InteractionComponentConverter))]
    public IInteractionComponent[]? Components { get; set; }
}
