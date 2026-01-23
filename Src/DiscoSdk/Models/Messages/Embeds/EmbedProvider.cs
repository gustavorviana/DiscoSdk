using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Embeds;

/// <summary>
/// Represents an embed provider.
/// </summary>
public class EmbedProvider
{
    /// <summary>
    /// Gets or sets the name of the provider.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the URL of the provider.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

