using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a message application.
/// </summary>
public class MessageApplication
{
    /// <summary>
    /// Gets or sets the ID of the application.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the icon hash of the application.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the ID of the embed's image asset.
    /// </summary>
    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; set; }
}

