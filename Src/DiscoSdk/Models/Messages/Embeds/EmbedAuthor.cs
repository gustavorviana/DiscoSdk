using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Embeds;

/// <summary>
/// Represents an embed author.
/// </summary>
public class EmbedAuthor
{
    /// <summary>
    /// Gets or sets the name of the author.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the URL of the author.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the URL of the author icon.
    /// </summary>
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets a proxied URL of the author icon.
    /// </summary>
    [JsonPropertyName("proxy_icon_url")]
    public string? ProxyIconUrl { get; set; }
}
