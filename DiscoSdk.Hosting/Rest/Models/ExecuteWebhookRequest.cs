using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Pools;
using DiscoSdk.Models.Requests.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Models;

/// <summary>
/// Represents a request to send a follow-up message to a deferred interaction
/// or to execute a webhook.
/// </summary>
internal class ExecuteWebhookRequest
{
    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets whether the message should be sent using TTS.
    /// </summary>
    [JsonPropertyName("tts")]
    public bool? Tts { get; set; }

    /// <summary>
    /// Gets or sets the override username for the webhook message.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the override avatar URL for the webhook message.
    /// </summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the name of the thread to be created with the message.
    /// </summary>
    [JsonPropertyName("thread_name")]
    public string? ThreadName { get; set; }

    /// <summary>
    /// Gets or sets the applied forum tag ids.
    /// </summary>
    [JsonPropertyName("applied_tags")]
    public ulong[]? AppliedTags { get; set; }

    /// <summary>
    /// Gets or sets the allowed mentions configuration.
    /// </summary>
    [JsonPropertyName("allowed_mentions")]
    public AllowedMentions? AllowedMentions { get; set; }

    /// <summary>
    /// Gets or sets the message components (buttons, select menus, etc.).
    /// </summary>
    [JsonPropertyName("components")]
    public MessageComponent[]? Components { get; set; }

    /// <summary>
    /// Gets or sets the embeds to include in the message (max 10).
    /// </summary>
    [JsonPropertyName("embeds")]
    public Embed[]? Embeds { get; set; }

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
