using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Represents an option (parameter) for an application command.
/// </summary>
public class ApplicationCommandOption
{
    /// <summary>
    /// Gets or sets the type of option.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the option (1-32 characters, lowercase).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the name field.
    /// </summary>
    [JsonPropertyName("name_localizations")]
    public Dictionary<string, string>? NameLocalizations { get; set; }

    /// <summary>
    /// Gets or sets the description of the option (1-100 characters).
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the description field.
    /// </summary>
    [JsonPropertyName("description_localizations")]
    public Dictionary<string, string>? DescriptionLocalizations { get; set; }

    /// <summary>
    /// Gets or sets whether the option is required.
    /// </summary>
    [JsonPropertyName("required")]
    public bool? Required { get; set; }

    /// <summary>
    /// Gets or sets the choices for the option (only for STRING, INTEGER, and NUMBER types).
    /// </summary>
    [JsonPropertyName("choices")]
    public List<ApplicationCommandOptionChoice>? Choices { get; set; }

    /// <summary>
    /// Gets or sets the nested options (for subcommands and subcommand groups).
    /// </summary>
    [JsonPropertyName("options")]
    public List<ApplicationCommandOption>? Options { get; set; }

    /// <summary>
    /// Gets or sets the channel types to include (only for CHANNEL type).
    /// </summary>
    [JsonPropertyName("channel_types")]
    public List<int>? ChannelTypes { get; set; }

    /// <summary>
    /// Gets or sets the minimum value (for INTEGER and NUMBER types).
    /// </summary>
    [JsonPropertyName("min_value")]
    public object? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value (for INTEGER and NUMBER types).
    /// </summary>
    [JsonPropertyName("max_value")]
    public object? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the minimum length (for STRING type).
    /// </summary>
    [JsonPropertyName("min_length")]
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum length (for STRING type).
    /// </summary>
    [JsonPropertyName("max_length")]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets whether autocomplete is enabled.
    /// </summary>
    [JsonPropertyName("autocomplete")]
    public bool? Autocomplete { get; set; }
}
