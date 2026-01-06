using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord emoji.
/// </summary>
public class Emoji
{
    /// <summary>
    /// Gets or sets the emoji's unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the emoji's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the roles allowed to use this emoji.
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }

    /// <summary>
    /// Gets or sets the user that created this emoji.
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this emoji must be wrapped in colons.
    /// </summary>
    [JsonPropertyName("require_colons")]
    public bool? RequireColons { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this emoji is managed.
    /// </summary>
    [JsonPropertyName("managed")]
    public bool? Managed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this emoji is animated.
    /// </summary>
    [JsonPropertyName("animated")]
    public bool? Animated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this emoji is available.
    /// </summary>
    [JsonPropertyName("available")]
    public bool? Available { get; set; }
}

