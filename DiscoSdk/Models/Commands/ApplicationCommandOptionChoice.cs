using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Represents a choice for an application command option.
/// </summary>
public class ApplicationCommandOptionChoice
{
    /// <summary>
    /// Gets or sets the name of the choice (1-100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the name field.
    /// </summary>
    [JsonPropertyName("name_localizations")]
    public Dictionary<string, string>? NameLocalizations { get; set; }

    /// <summary>
    /// Gets or sets the value of the choice (string, integer, or double).
    /// </summary>
    [JsonPropertyName("value")]
    public object Value { get; set; } = default!;
}