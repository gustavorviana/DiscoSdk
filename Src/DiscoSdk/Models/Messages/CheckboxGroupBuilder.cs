using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for creating checkbox group components in modals (multi-select from a list).
/// </summary>
public class CheckboxGroupBuilder : IModalComponentBuilder
{
	private readonly CheckboxGroupComponent _component = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="CheckboxGroupBuilder"/> class.
	/// </summary>
	/// <param name="customId">The custom ID for the checkbox group (1-100 characters).</param>
	public CheckboxGroupBuilder(string customId)
	{
		if (string.IsNullOrWhiteSpace(customId))
			throw new ArgumentException("Custom ID cannot be null or empty.", nameof(customId));
		if (customId.Length > 100)
			throw new ArgumentException("Custom ID cannot exceed 100 characters.", nameof(customId));
		_component.CustomId = customId;
	}

	/// <summary>
	/// Sets the label for the Label container (type 18) when this component is used in a modal (max 45 characters).
	/// Required when adding the checkbox group to a modal.
	/// </summary>
	/// <param name="label">The label text displayed above the checkbox group.</param>
	/// <returns>The current <see cref="CheckboxGroupBuilder"/> instance.</returns>
	public CheckboxGroupBuilder WithLabel(string label)
	{
		if (string.IsNullOrWhiteSpace(label))
			throw new ArgumentException("Label cannot be null or empty.", nameof(label));
		if (label.Length > 45)
			throw new ArgumentException("Label cannot exceed 45 characters.", nameof(label));
		_component.Label = label;
		return this;
	}

	/// <summary>
	/// Adds an option to the checkbox group.
	/// </summary>
	/// <param name="value">The dev-defined value (max 100 characters).</param>
	/// <param name="label">The user-facing label (max 100 characters).</param>
	/// <param name="description">Optional description (max 100 characters).</param>
	/// <param name="isDefault">Whether this option is selected by default.</param>
	/// <returns>The current <see cref="CheckboxGroupBuilder"/> instance.</returns>
	public CheckboxGroupBuilder AddOption(string value, string label, string? description = null, bool isDefault = false)
	{
		_component.Options = [.. _component.Options, new CheckboxGroupOption
		{
			Value = value,
			Label = label,
			Description = description,
			Default = isDefault ? true : null
		}];
		return this;
	}

	/// <summary>
	/// Sets the minimum number of items that must be chosen (0-10).
	/// If set to 0, <see cref="WithRequired"/> must be false.
	/// </summary>
	/// <param name="minValues">Minimum number of selections.</param>
	/// <returns>The current <see cref="CheckboxGroupBuilder"/> instance.</returns>
	public CheckboxGroupBuilder WithMinValues(int minValues)
	{
		if (minValues < 0 || minValues > 10)
			throw new ArgumentOutOfRangeException(nameof(minValues), "Min values must be between 0 and 10.");
		_component.MinValues = minValues;
		return this;
	}

	/// <summary>
	/// Sets the maximum number of items that can be chosen (1-10).
	/// </summary>
	/// <param name="maxValues">Maximum number of selections.</param>
	/// <returns>The current <see cref="CheckboxGroupBuilder"/> instance.</returns>
	public CheckboxGroupBuilder WithMaxValues(int maxValues)
	{
		if (maxValues < 1 || maxValues > 10)
			throw new ArgumentOutOfRangeException(nameof(maxValues), "Max values must be between 1 and 10.");
		_component.MaxValues = maxValues;
		return this;
	}

	/// <summary>
	/// Sets whether selecting within the group is required.
	/// </summary>
	/// <param name="required">True if required (default is true).</param>
	/// <returns>The current <see cref="CheckboxGroupBuilder"/> instance.</returns>
	public CheckboxGroupBuilder WithRequired(bool required = true)
	{
		_component.Required = required;
		return this;
	}

	/// <summary>
	/// Builds the checkbox group component. Must have between 1 and 10 options.
	/// </summary>
	/// <returns>The built <see cref="CheckboxGroupComponent"/> instance.</returns>
	public CheckboxGroupComponent Build()
	{
		if (_component.Options.Length < 1)
			throw new InvalidOperationException("Checkbox group must have at least 1 option.");
		if (_component.Options.Length > 10)
			throw new InvalidOperationException("Checkbox group cannot have more than 10 options.");
		return _component;
	}

	IModalComponent IModalComponentBuilder.Build() => Build();
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}
