using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Builders;

/// <summary>
/// Fluent builder for creating Discord modals.
/// </summary>
public class ModalBuilder
{
    private readonly string _customId;
    private readonly string _title;
    private List<ActionRowComponent> _components = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalBuilder"/> class.
    /// </summary>
    /// <param name="id">The custom ID for the modal (max 100 characters).</param>
    /// <param name="title">The title of the modal (max 45 characters).</param>
    public ModalBuilder(string id, string title)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Custom ID cannot be null or empty.", nameof(id));

        if (id.Length > 100)
            throw new ArgumentException("Custom ID cannot exceed 100 characters.", nameof(id));

        _customId = id;

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        if (title.Length > 45)
            throw new ArgumentException("Title cannot exceed 45 characters.", nameof(title));

        _title = title;
    }

    /// <summary>
    /// Adds an action row containing a text input component to the modal.
    /// This is the primary method for adding text inputs.
    /// </summary>
    /// <param name="textInput">The text input component to add.</param>
    /// <returns>The current <see cref="ModalBuilder"/> instance.</returns>
    public ModalBuilder AddActionRow(TextInputComponent textInput)
    {
        ArgumentNullException.ThrowIfNull(textInput);

        if (_components.Count >= 5)
            throw new InvalidOperationException("Modal cannot have more than 5 components.");

        var actionRow = new ActionRowComponent
        {
            Components = [textInput]
        };

        _components.Add(actionRow);
        return this;
    }

    /// <summary>
    /// Builds the modal data.
    /// </summary>
    /// <returns>The built <see cref="ModalData"/> instance.</returns>
    public ModalData Build()
    {
        if (_components.Count == 0)
            throw new InvalidOperationException("Modal must have at least one component.");
        if (_components.Count > 5)
            throw new InvalidOperationException("Modal cannot have more than 5 components.");

        return new ModalData
        {
            Components = [.. _components],
            CustomId = _customId,
            Title = _title
        };
    }
}