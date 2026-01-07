using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Embeds;

/// <summary>
/// Represents an embed footer.
/// </summary>
public class EmbedFooter
{
    /// <summary>
    /// Gets or sets the footer text.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = default!;

    /// <summary>
    /// Gets or sets the URL of the footer icon.
    /// </summary>
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets a proxied URL of the footer icon.
    /// </summary>
    [JsonPropertyName("proxy_icon_url")]
    public string? ProxyIconUrl { get; set; }
}
