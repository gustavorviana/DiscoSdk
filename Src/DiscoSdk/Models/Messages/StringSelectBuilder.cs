using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for a String Select (type 3) — a dropdown of dev-defined string options.
/// </summary>
public class StringSelectBuilder : IInteractionComponentBuilder
{
	private readonly StringSelectComponent _component = new();
	private readonly List<SelectOption> _options = [];

	private const int MaxOptions = 25;

	public StringSelectBuilder(string customId)
	{
		if (string.IsNullOrWhiteSpace(customId))
			throw new ArgumentException("Custom ID cannot be null or empty.", nameof(customId));
		if (customId.Length > 100)
			throw new ArgumentException("Custom ID cannot exceed 100 characters.", nameof(customId));

		_component.CustomId = customId;
	}

	/// <summary>Placeholder shown when nothing is selected (≤ 150 chars).</summary>
	public StringSelectBuilder WithPlaceholder(string placeholder)
	{
		if (placeholder?.Length > 150)
			throw new ArgumentException("Placeholder cannot exceed 150 characters.", nameof(placeholder));
		_component.Placeholder = placeholder;
		return this;
	}

	/// <summary>Adds an option. <paramref name="label"/> and <paramref name="value"/> ≤ 100 chars; <paramref name="description"/> ≤ 100 chars.</summary>
	public StringSelectBuilder AddOption(string label, string value, string? description = null, Emoji? emoji = null, bool isDefault = false)
	{
		if (_options.Count >= MaxOptions)
			throw new InvalidOperationException($"StringSelect cannot have more than {MaxOptions} options.");
		if (string.IsNullOrWhiteSpace(label) || label.Length > 100)
			throw new ArgumentException("Option label is required and must be ≤ 100 characters.", nameof(label));
		if (string.IsNullOrWhiteSpace(value) || value.Length > 100)
			throw new ArgumentException("Option value is required and must be ≤ 100 characters.", nameof(value));
		if (description?.Length > 100)
			throw new ArgumentException("Option description cannot exceed 100 characters.", nameof(description));

		_options.Add(new SelectOption
		{
			Label = label,
			Value = value,
			Description = description,
			Emoji = emoji,
			Default = isDefault ? true : null,
		});
		return this;
	}

	/// <summary>Minimum picks required (default 1, range 0–25).</summary>
	public StringSelectBuilder WithMinValues(int min)
	{
		if (min is < 0 or > 25)
			throw new ArgumentOutOfRangeException(nameof(min), "MinValues must be between 0 and 25.");
		_component.MinValues = min;
		return this;
	}

	/// <summary>Maximum picks allowed (default 1, range 1–25).</summary>
	public StringSelectBuilder WithMaxValues(int max)
	{
		if (max is < 1 or > 25)
			throw new ArgumentOutOfRangeException(nameof(max), "MaxValues must be between 1 and 25.");
		_component.MaxValues = max;
		return this;
	}

	public StringSelectBuilder WithDisabled(bool disabled = true)
	{
		_component.Disabled = disabled;
		return this;
	}

	public StringSelectComponent Build()
	{
		if (_options.Count < 1)
			throw new InvalidOperationException("StringSelect must have at least one option.");
		_component.Options = [.. _options];
		return _component;
	}

	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}
