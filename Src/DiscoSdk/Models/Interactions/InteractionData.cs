using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Interactions;


/// <summary>
/// Represents interaction data for application commands.
/// </summary>
public class InteractionData
{
    /// <summary>
    /// Gets or sets the ID of the invoked command.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the invoked command.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of the invoked command.
    /// </summary>
    [JsonPropertyName("type")]
    public ApplicationCommandType Type { get; set; }

    /// <summary>
    /// Gets or sets the converted users.
    /// </summary>
    [JsonPropertyName("resolved")]
    public InteractionResolved? Resolved { get; set; }

    /// <summary>
    /// Gets or sets the parameters and values from the user.
    /// </summary>
    [JsonPropertyName("options")]
    public InteractionOption[]? Options { get; set; }

    /// <summary>
    /// Gets or sets the custom ID of the component.
    /// </summary>
    [JsonPropertyName("custom_id")]
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets or sets the type of component.
    /// </summary>
    [JsonPropertyName("component_type")]
    public ComponentType? ComponentType { get; set; }

    /// <summary>
    /// Gets or sets the values the user selected.
    /// </summary>
    [JsonPropertyName("values")]
    public string[]? Values { get; set; }

    /// <summary>
    /// Gets or sets the rows of submitted fields when this interaction is MODAL_SUBMIT.
    /// </summary>
    [JsonPropertyName("components")]
    public ModalSubmitRow[]? Components { get; set; }
}

