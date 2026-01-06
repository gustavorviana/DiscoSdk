using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents an option in an interaction.
/// </summary>
public class InteractionOption
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of option.
    /// </summary>
    [JsonPropertyName("type")]
    public ApplicationCommandOptionType Type { get; set; }

    /// <summary>
    /// Gets or sets the value of the option.
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the nested options.
    /// </summary>
    [JsonPropertyName("options")]
    public List<InteractionOption>? Options { get; set; }

    /// <summary>
    /// Gets or sets whether this option is the currently focused option for autocomplete.
    /// </summary>
    [JsonPropertyName("focused")]
    public bool? Focused { get; set; }
}
