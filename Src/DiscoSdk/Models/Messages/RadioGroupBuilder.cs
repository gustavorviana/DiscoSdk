using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for creating radio group components in modals (single choice from a list).
/// </summary>
public class RadioGroupBuilder : IModalComponentBuilder
{
	private readonly RadioGroupComponent _component = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="RadioGroupBuilder"/> class.
	/// </summary>
	/// <param name="customId">The custom ID for the radio group (1-100 characters).</param>
	public RadioGroupBuilder(string customId)
	{
		if (string.IsNullOrWhiteSpace(customId))
			throw new ArgumentException("Custom ID cannot be null or empty.", nameof(customId));
		if (customId.Length > 100)
			throw new ArgumentException("Custom ID cannot exceed 100 characters.", nameof(customId));
		_component.CustomId = customId;
	}

	/// <summary>
	/// Adds an option to the radio group.
	/// </summary>
	/// <param name="value">The dev-defined value (max 100 characters).</param>
	/// <param name="label">The user-facing label (max 100 characters).</param>
	/// <param name="description">Optional description (max 100 characters).</param>
	/// <param name="isDefault">Whether this option is selected by default.</param>
	/// <returns>The current <see cref="RadioGroupBuilder"/> instance.</returns>
	public RadioGroupBuilder AddOption(string value, string label, string? description = null, bool isDefault = false)
	{
		_component.Options = [.. _component.Options, new RadioGroupOption
		{
			Value = value,
			Label = label,
			Description = description,
			Default = isDefault ? true : null
		}];
		return this;
	}

	/// <summary>
	/// Sets whether a selection is required to submit the modal.
	/// </summary>
	/// <param name="required">True if required (default is true).</param>
	/// <returns>The current <see cref="RadioGroupBuilder"/> instance.</returns>
	public RadioGroupBuilder WithRequired(bool required = true)
	{
		_component.Required = required;
		return this;
	}

	/// <summary>
	/// Builds the radio group component. Must have between 2 and 10 options.
	/// </summary>
	/// <returns>The built <see cref="RadioGroupComponent"/> instance.</returns>
	public RadioGroupComponent Build()
	{
		if (_component.Options.Length < 2)
			throw new InvalidOperationException("Radio group must have at least 2 options.");
		if (_component.Options.Length > 10)
			throw new InvalidOperationException("Radio group cannot have more than 10 options.");
		return _component;
	}

	IModalComponent IModalComponentBuilder.Build() => Build();
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}
