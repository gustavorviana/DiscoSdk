using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Pools;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests.Messages;

/// <summary>
/// Represents a request to edit a message in Discord.
/// Contains all fields supported by the Discord API for message editing.
/// </summary>
internal class MessageEditRequest
{
    /// <summary>
    /// Gets or sets the message content (max 2000 characters).
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the embeds to include in the message (max 10).
    /// </summary>
    [JsonPropertyName("embeds")]
    public Embed[]? Embeds { get; set; }

    /// <summary>
    /// Gets or sets the allowed mentions configuration.
    /// </summary>
    [JsonPropertyName("allowed_mentions")]
    public AllowedMentions? AllowedMentions { get; set; }

    /// <summary>
    /// Gets or sets the message components (buttons, select menus, etc.) (max 5 rows).
    /// </summary>
    [JsonPropertyName("components")]
    public MessageComponent[]? Components { get; set; }

    /// <summary>
    /// Gets or sets the poll configuration.
    /// </summary>
    [JsonPropertyName("poll")]
    public Poll? Poll { get; set; }

    /// <summary>
    /// Gets or sets the attachment metadata.
    /// This describes files sent via multipart upload.
    /// </summary>
    [JsonPropertyName("attachments")]
    public MessageAttachmentMetadata[]? Attachments { get; set; }

    /// <summary>
    /// Gets or sets the message flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public MessageFlags? Flags { get; set; }
}
