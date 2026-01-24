using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Rest.Actions.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests.Messages;

/// <summary>
/// Represents a request to edit a message in Discord.
/// <para>
/// Reference:
/// https://discord.com/developers/docs/resources/message#edit-message
/// </para>
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
    /// To remove all embeds, send an empty array.
    /// </summary>
    [JsonPropertyName("embeds")]
    public Embed[]? Embeds { get; set; }

    /// <summary>
    /// Gets or sets the allowed mentions configuration.
    /// If omitted, the server/client defaults are used.
    /// </summary>
    [JsonPropertyName("allowed_mentions")]
    public AllowedMentions? AllowedMentions { get; set; }

    /// <summary>
    /// Gets or sets the message components (max 5 rows).
    /// To remove all components, send an empty array.
    /// </summary>
    [JsonPropertyName("components")]
    public MessageComponent[]? Components { get; set; }

    /// <summary>
    /// Gets or sets the attachment objects to keep and/or add.
    /// When editing with files, this field specifies which attachments should remain on the message.
    /// </summary>
    [JsonPropertyName("attachments")]
    public MessageAttachmentMetadata[]? Attachments { get; set; }

    /// <summary>
    /// Gets or sets the message flags.
    /// Only certain flags can be toggled via edit (e.g. SUPPRESS_EMBEDS, SUPPRESS_NOTIFICATIONS).
    /// </summary>
    [JsonPropertyName("flags")]
    public MessageFlags? Flags { get; set; }
}