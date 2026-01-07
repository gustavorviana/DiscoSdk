using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Models;

/// <summary>
/// Represents a request to send a follow-up message to a deferred interaction.
/// </summary>
public class FollowUpMessageRequest
{
    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the message flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public MessageFlags? Flags { get; set; }

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
}

