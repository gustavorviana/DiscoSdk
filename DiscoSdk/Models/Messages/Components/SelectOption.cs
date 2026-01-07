using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Represents a select option.
/// </summary>
public class SelectOption
{
    /// <summary>
    /// Gets or sets the user-facing name of the option.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dev-defined value of the option.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the description of the option.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the emoji of the option.
    /// </summary>
    [JsonPropertyName("emoji")]
    public Emoji? Emoji { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this option should be selected by default.
    /// </summary>
    [JsonPropertyName("default")]
    public bool? Default { get; set; }
}
