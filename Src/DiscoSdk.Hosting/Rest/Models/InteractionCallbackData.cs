using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Models;

/// <summary>
/// Represents the data payload for an interaction callback response.
/// </summary>
public class InteractionCallbackData
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
    public MessageFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the modal data. Used when responding with a modal (type 9).
    /// </summary>
    [JsonPropertyName("custom_id")]
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets or sets the modal title. Used when responding with a modal (type 9).
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the components (action rows for modals or message components for messages).
    /// </summary>
    [JsonPropertyName("components")]
    [JsonConverter(typeof(InteractionComponentConverter))]
    public IInteractionComponent[] Components { get; set; } = [];

    /// <summary>
    /// Gets or sets the embeds to include in the message (max 10).
    /// </summary>
    [JsonPropertyName("embeds")]
    public Embed[] Embeds { get; set; } = [];
}

