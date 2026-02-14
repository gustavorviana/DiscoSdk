using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for creating text input components in modals.
/// </summary>
public class TextInputBuilder : IModalComponentBuilder
{
    private readonly TextInputComponent _textInput = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TextInputBuilder"/> class.
    /// Custom ID, label, and style must be provided at construction time and cannot be changed.
    /// </summary>
    /// <param name="customId">The custom ID for the text input (max 100 characters).</param>
    /// <param name="label">The label for the text input (max 45 characters).</param>
    /// <param name="style">The style of the text input (Short or Paragraph).</param>
    public TextInputBuilder(string customId, string label, TextInputStyle style)
    {
        if (string.IsNullOrWhiteSpace(customId))
            throw new ArgumentException("Custom ID cannot be null or empty.", nameof(customId));
        if (customId.Length > 100)
            throw new ArgumentException("Custom ID cannot exceed 100 characters.", nameof(customId));

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be null or empty.", nameof(label));
        if (label.Length > 45)
            throw new ArgumentException("Label cannot exceed 45 characters.", nameof(label));

        _textInput.CustomId = customId;
        _textInput.Label = label;
        _textInput.Style = style;
    }

    /// <summary>
    /// Sets the placeholder text (max 100 characters).
    /// </summary>
    /// <param name="placeholder">The placeholder text shown when the input is empty.</param>
    /// <returns>The current <see cref="TextInputBuilder"/> instance.</returns>
    public TextInputBuilder WithPlaceholder(string placeholder)
    {
        if (!string.IsNullOrEmpty(placeholder) && placeholder.Length > 100)
            throw new ArgumentException("Placeholder cannot exceed 100 characters.", nameof(placeholder));

        _textInput.Placeholder = placeholder;
        return this;
    }

    /// <summary>
    /// Sets whether the input is required.
    /// </summary>
    /// <param name="required">True if the input is required, false otherwise.</param>
    /// <returns>The current <see cref="TextInputBuilder"/> instance.</returns>
    public TextInputBuilder WithRequired(bool required)
    {
        _textInput.Required = required;
        return this;
    }

    /// <summary>
    /// Sets the minimum length of the input (0-4000).
    /// </summary>
    /// <param name="minLength">The minimum number of characters required.</param>
    /// <returns>The current <see cref="TextInputBuilder"/> instance.</returns>
    public TextInputBuilder WithMinLength(int minLength)
    {
        if (minLength < 0 || minLength > 4000)
            throw new ArgumentOutOfRangeException(nameof(minLength), "Min length must be between 0 and 4000.");

        _textInput.MinLength = minLength;
        return this;
    }

    /// <summary>
    /// Sets the maximum length of the input (1-4000).
    /// </summary>
    /// <param name="maxLength">The maximum number of characters allowed.</param>
    /// <returns>The current <see cref="TextInputBuilder"/> instance.</returns>
    public TextInputBuilder WithMaxLength(int maxLength)
    {
        if (maxLength < 1 || maxLength > 4000)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be between 1 and 4000.");

        _textInput.MaxLength = maxLength;
        return this;
    }

    /// <summary>
    /// Sets a pre-filled value for the input.
    /// </summary>
    /// <param name="value">The value to pre-fill the input with.</param>
    /// <returns>The current <see cref="TextInputBuilder"/> instance.</returns>
    public TextInputBuilder WithValue(string value)
    {
        _textInput.Value = value;
        return this;
    }

	/// <summary>
	/// Builds the text input component.
	/// </summary>
	/// <returns>The built <see cref="TextInputComponent"/> instance.</returns>
	public TextInputComponent Build()
	{
		if (string.IsNullOrWhiteSpace(_textInput.CustomId))
			throw new InvalidOperationException("Text input custom ID is required.");
		if (string.IsNullOrWhiteSpace(_textInput.Label))
			throw new InvalidOperationException("Text input label is required.");

		return _textInput;
	}

	IModalComponent IModalComponentBuilder.Build() => Build();
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}