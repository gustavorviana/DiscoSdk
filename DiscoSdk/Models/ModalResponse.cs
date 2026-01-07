using DiscoSdk.Models.Interactions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents the response data from a submitted modal.
/// </summary>
public class ModalResponse
{
    /// <summary>
    /// Gets or sets the custom ID of the modal that was submitted.
    /// </summary>
    public string CustomId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the interaction that submitted the modal.
    /// </summary>
    public Interaction Interaction { get; set; } = default!;

    /// <summary>
    /// Gets the value of a text input component by its custom ID.
    /// </summary>
    /// <param name="customId">The custom ID of the text input component.</param>
    /// <returns>The value of the text input, or null if not found.</returns>
    public string? GetValue(string customId)
    {
        return Interaction.Data?.Components?
            .SelectMany(row => row.Components ?? [])
            .FirstOrDefault(c => c.CustomId == customId)?
            .Value;
    }

    /// <summary>
    /// Gets all text input values as a dictionary keyed by custom ID.
    /// </summary>
    /// <returns>A dictionary containing all text input values.</returns>
    public Dictionary<string, string> GetAllValues()
    {
        if (Interaction?.Data?.Components is null)
            return [];

        return Interaction
                .Data
                .Components
                .Where(x => x.Components != null)
                .SelectMany(x => x.Components!)
                .Where(component => !string.IsNullOrEmpty(component.CustomId) && !string.IsNullOrEmpty(component.Value))
                .ToDictionary(x => x.CustomId, x => x.Value);
    }
}