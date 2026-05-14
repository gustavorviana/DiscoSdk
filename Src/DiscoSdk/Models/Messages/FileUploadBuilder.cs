using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for a <see cref="FileUploadComponent"/> (type 19) — a modal input that lets
/// the user attach 1–10 files. Auto-wrapped in a <see cref="LabelComponent"/> when added via
/// <see cref="Rest.Actions.IReplyModalRestAction.AddActionRow"/>.
/// </summary>
public class FileUploadBuilder : IModalComponentBuilder
{
	private readonly FileUploadComponent _component = new();

	public FileUploadBuilder(string customId)
	{
		if (string.IsNullOrWhiteSpace(customId) || customId.Length > 100)
			throw new ArgumentException("Custom ID is required and must be ≤ 100 characters.", nameof(customId));
		_component.CustomId = customId;
	}

	/// <summary>Sets the Label container text (≤ 45 chars). Required.</summary>
	public FileUploadBuilder WithLabel(string label)
	{
		if (string.IsNullOrWhiteSpace(label) || label.Length > 45)
			throw new ArgumentException("Label is required and must be ≤ 45 characters.", nameof(label));
		_component.Label = label;
		return this;
	}

	/// <summary>Optional description shown under the label (≤ 100 chars).</summary>
	public FileUploadBuilder WithDescription(string description)
	{
		if (description?.Length > 100)
			throw new ArgumentException("Description cannot exceed 100 characters.", nameof(description));
		_component.Description = description;
		return this;
	}

	/// <summary>Minimum number of files (0–10).</summary>
	public FileUploadBuilder WithMinValues(int min)
	{
		if (min is < 0 or > 10)
			throw new ArgumentOutOfRangeException(nameof(min), "MinValues must be between 0 and 10.");
		_component.MinValues = min;
		return this;
	}

	/// <summary>Maximum number of files (1–10).</summary>
	public FileUploadBuilder WithMaxValues(int max)
	{
		if (max is < 1 or > 10)
			throw new ArgumentOutOfRangeException(nameof(max), "MaxValues must be between 1 and 10.");
		_component.MaxValues = max;
		return this;
	}

	public FileUploadBuilder WithRequired(bool required = true)
	{
		_component.Required = required;
		return this;
	}

	public FileUploadComponent Build()
	{
		if (string.IsNullOrWhiteSpace(_component.Label))
			throw new InvalidOperationException("FileUpload requires a Label (call WithLabel).");
		return _component;
	}

	IModalComponent IModalComponentBuilder.Build() => Build();
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();
}
