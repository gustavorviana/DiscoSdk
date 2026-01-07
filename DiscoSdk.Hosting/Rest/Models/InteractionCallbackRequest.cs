using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Rest.Models;

/// <summary>
/// Represents a request to respond to a Discord interaction.
/// </summary>
public class InteractionCallbackRequest
{
    /// <summary>
    /// Gets or sets the type of interaction callback.
    /// </summary>
    [JsonPropertyName("type")]
    public InteractionCallbackType Type { get; set; }

    /// <summary>
    /// Gets or sets the callback data. This is required for <see cref="InteractionCallbackType.ChannelMessageWithSource"/>
    /// and optional for <see cref="InteractionCallbackType.DeferredChannelMessageWithSource"/>.
    /// </summary>
    [JsonPropertyName("data")]
    public InteractionCallbackData? Data { get; set; }
}

