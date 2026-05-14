using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IReplyModalRestAction"/> for replying to interactions with modals.
/// </summary>
internal class ReplyModalRestAction : RestAction, IReplyModalRestAction
{
	private readonly List<IModalComponent> _components = [];
    private readonly InteractionHandle _interactionHandle;
    private readonly DiscordClient _client;
    private string? _customId;
    private string? _title;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplyModalRestAction"/> class.
    /// </summary>
    /// <param name="client">The Discord client.</param>
    /// <param name="interactionHandle">The interaction handle for responding to the interaction.</param>
    public ReplyModalRestAction(DiscordClient client, InteractionHandle interactionHandle)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionHandle = interactionHandle ?? throw new ArgumentNullException(nameof(interactionHandle));
    }

    /// <inheritdoc />
    public IReplyModalRestAction SetCustomId(string customId)
    {
        if (string.IsNullOrWhiteSpace(customId))
            throw new ArgumentException("Custom ID cannot be null or empty.", nameof(customId));

        if (customId.Length > 100)
            throw new ArgumentException("Custom ID cannot exceed 100 characters.", nameof(customId));

        _customId = customId;
        return this;
    }

    /// <inheritdoc />
    public IReplyModalRestAction SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        if (title.Length > 45)
            throw new ArgumentException("Title cannot exceed 45 characters.", nameof(title));

        _title = title;
        return this;
    }

	/// <inheritdoc />
	public IReplyModalRestAction AddActionRow(IModalComponent modalComponent)
	{
		ArgumentNullException.ThrowIfNull(modalComponent);

		if (modalComponent is not TextInputComponent
		    and not CheckboxGroupComponent
		    and not CheckboxComponent
		    and not RadioGroupComponent
		    and not FileUploadComponent)
			throw new ArgumentException(
				"Modal components only support TextInputComponent (wrapped in ActionRow), or " +
				"CheckboxGroupComponent / CheckboxComponent / RadioGroupComponent / FileUploadComponent " +
				"(all auto-wrapped in a Label).",
				nameof(modalComponent));

		if (_components.Count >= 5)
			throw new InvalidOperationException("Modal cannot have more than 5 components.");

		// CheckboxGroup (type 22) → wrap in Label (type 18), per Discord's modal layout.
		if (modalComponent is CheckboxGroupComponent checkboxGroup)
		{
			ValidateCheckboxGroupForModal(checkboxGroup, nameof(modalComponent));
			_components.Add(new LabelComponent
			{
				Label = checkboxGroup.Label!,
				Component = checkboxGroup,
			});
			return this;
		}

		// Single Checkbox (type 23) → wrap in Label.
		if (modalComponent is CheckboxComponent checkbox)
		{
			ValidateCheckboxForModal(checkbox, nameof(modalComponent));
			_components.Add(new LabelComponent
			{
				Label = checkbox.Label!,
				Description = checkbox.Description,
				Component = checkbox,
			});
			return this;
		}

		// RadioGroup (type 21) → wrap in Label.
		if (modalComponent is RadioGroupComponent radioGroup)
		{
			ValidateRadioGroupForModal(radioGroup, nameof(modalComponent));
			_components.Add(new LabelComponent
			{
				Label = radioGroup.Label!,
				Description = radioGroup.Description,
				Component = radioGroup,
			});
			return this;
		}

		// FileUpload (type 19) → wrap in Label.
		if (modalComponent is FileUploadComponent fileUpload)
		{
			ValidateFileUploadForModal(fileUpload, nameof(modalComponent));
			_components.Add(new LabelComponent
			{
				Label = fileUpload.Label!,
				Description = fileUpload.Description,
				Component = fileUpload,
			});
			return this;
		}

		// TextInput is the V1 path — validate then wrap in a (modal) ActionRow.
		if (modalComponent is TextInputComponent textInput)
			ValidateTextInputForModal(textInput, nameof(modalComponent));

		var actionRow = new ActionRowComponent
		{
			Components = [modalComponent]
		};

		_components.Add(actionRow);
		return this;
	}

	/// <inheritdoc />
	public IReplyModalRestAction AddActionRow(IModalComponentBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder);
		return AddActionRow(builder.Build());
	}

	/// <inheritdoc />
	public IReplyModalRestAction SetComponents(params IModalComponent[] components)
	{
		ArgumentNullException.ThrowIfNull(components);

		if (components.Length > 5)
			throw new ArgumentException("Modal cannot have more than 5 components.", nameof(components));

		_components.Clear();
		_components.AddRange(components.Where(c => c != null));
		return this;
	}

    /// <inheritdoc />
    public override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_customId))
            throw new InvalidOperationException("Modal must have a custom ID. Call SetCustomId() to set it.");

        if (string.IsNullOrWhiteSpace(_title))
            throw new InvalidOperationException("Modal must have a title. Call SetTitle() to set it.");

        if (_components.Count == 0)
            throw new InvalidOperationException("Modal must have at least one component. Call AddActionRow() to add components.");

        if (_interactionHandle.IsDeferred)
            throw new InvalidOperationException("Cannot respond with modal after deferring the interaction.");

		ValidateModalComponents(_components);

		var modalData = new ModalData
		{
			CustomId = _customId,
			Title = _title,
			Components = [.. _components]
		};

        return _client.InteractionClient.RespondWithModalAsync(_interactionHandle, modalData, cancellationToken);
    }

	private static void ValidateModalComponents(List<IModalComponent> components)
	{
		foreach (var c in components)
		{
			if (c is LabelComponent label)
			{
				if (label.Component is CheckboxGroupComponent checkboxGroup)
					ValidateCheckboxGroupForModal(checkboxGroup, "components");
				else if (label.Component is CheckboxComponent checkbox)
					ValidateCheckboxForModal(checkbox, "components");
				else if (label.Component is RadioGroupComponent radioGroup)
					ValidateRadioGroupForModal(radioGroup, "components");
				else if (label.Component is FileUploadComponent fileUpload)
					ValidateFileUploadForModal(fileUpload, "components");
			}
			else if (c is ActionRowComponent row)
			{
				foreach (var inner in row.Components)
					if (inner is TextInputComponent textInput)
						ValidateTextInputForModal(textInput, "components");
			}
		}
	}

	private static void ValidateRadioGroupForModal(RadioGroupComponent radioGroup, string paramName)
	{
		if (string.IsNullOrWhiteSpace(radioGroup.Label))
			throw new ArgumentException("RadioGroupComponent must have Label set when used in a modal (required for the Label container type 18).", paramName);
		if (radioGroup.Label.Length > 45)
			throw new ArgumentException("RadioGroupComponent.Label cannot exceed 45 characters.", paramName);
		if (radioGroup.Description is { Length: > 100 })
			throw new ArgumentException("RadioGroupComponent.Description cannot exceed 100 characters.", paramName);
		if (string.IsNullOrWhiteSpace(radioGroup.CustomId))
			throw new ArgumentException("RadioGroupComponent (type 21) requires custom_id.", paramName);
		if (radioGroup.Options == null || radioGroup.Options.Length < 2)
			throw new ArgumentException("RadioGroupComponent (type 21) requires at least two options.", paramName);
		if (radioGroup.Options.Length > 10)
			throw new ArgumentException("RadioGroupComponent (type 21) cannot have more than 10 options.", paramName);
	}

	private static void ValidateFileUploadForModal(FileUploadComponent fileUpload, string paramName)
	{
		if (string.IsNullOrWhiteSpace(fileUpload.Label))
			throw new ArgumentException("FileUploadComponent must have Label set when used in a modal (required for the Label container type 18).", paramName);
		if (fileUpload.Label.Length > 45)
			throw new ArgumentException("FileUploadComponent.Label cannot exceed 45 characters.", paramName);
		if (fileUpload.Description is { Length: > 100 })
			throw new ArgumentException("FileUploadComponent.Description cannot exceed 100 characters.", paramName);
		if (string.IsNullOrWhiteSpace(fileUpload.CustomId))
			throw new ArgumentException("FileUploadComponent (type 19) requires custom_id.", paramName);
		if (fileUpload.MinValues is < 0 or > 10)
			throw new ArgumentException("FileUploadComponent.MinValues must be between 0 and 10.", paramName);
		if (fileUpload.MaxValues is < 1 or > 10)
			throw new ArgumentException("FileUploadComponent.MaxValues must be between 1 and 10.", paramName);
	}

	private static void ValidateTextInputForModal(TextInputComponent textInput, string paramName)
	{
		if (string.IsNullOrWhiteSpace(textInput.CustomId))
			throw new ArgumentException("TextInputComponent (type 4) requires custom_id.", paramName);
		if (string.IsNullOrWhiteSpace(textInput.Label))
			throw new ArgumentException("TextInputComponent (type 4) requires label.", paramName);
		if (textInput.Label.Length > 45)
			throw new ArgumentException("TextInputComponent.Label cannot exceed 45 characters.", paramName);
		if (textInput.Placeholder is { Length: > 100 })
			throw new ArgumentException("TextInputComponent.Placeholder cannot exceed 100 characters.", paramName);
		if (textInput.Value is { Length: > 4000 })
			throw new ArgumentException("TextInputComponent.Value cannot exceed 4000 characters.", paramName);
		if (textInput.MinLength is < 0 or > 4000)
			throw new ArgumentException("TextInputComponent.MinLength must be between 0 and 4000.", paramName);
		if (textInput.MaxLength is < 1 or > 4000)
			throw new ArgumentException("TextInputComponent.MaxLength must be between 1 and 4000.", paramName);
		if (textInput.MinLength is { } min && textInput.MaxLength is { } max && min > max)
			throw new ArgumentException("TextInputComponent.MinLength cannot exceed MaxLength.", paramName);
	}

	private static void ValidateCheckboxForModal(CheckboxComponent checkbox, string paramName)
	{
		if (string.IsNullOrWhiteSpace(checkbox.Label))
			throw new ArgumentException("CheckboxComponent must have Label set when used in a modal (required for the Label container type 18).", paramName);
		if (checkbox.Label.Length > 45)
			throw new ArgumentException("CheckboxComponent.Label cannot exceed 45 characters.", paramName);
		if (checkbox.Description is { Length: > 100 })
			throw new ArgumentException("CheckboxComponent.Description cannot exceed 100 characters.", paramName);
		if (string.IsNullOrWhiteSpace(checkbox.CustomId))
			throw new ArgumentException("CheckboxComponent (type 23) requires custom_id.", paramName);
	}

	private static void ValidateCheckboxGroupForModal(CheckboxGroupComponent checkboxGroup, string paramName)
	{
		if (string.IsNullOrWhiteSpace(checkboxGroup.Label))
			throw new ArgumentException("CheckboxGroupComponent must have Label set when used in a modal (required for the Label container type 18).", paramName);
		if (checkboxGroup.Label.Length > 45)
			throw new ArgumentException("CheckboxGroupComponent.Label cannot exceed 45 characters.", paramName);
		if (string.IsNullOrWhiteSpace(checkboxGroup.CustomId))
			throw new ArgumentException("CheckboxGroupComponent (type 22) requires custom_id.", paramName);
		if (checkboxGroup.Options == null || checkboxGroup.Options.Length < 1)
			throw new ArgumentException("CheckboxGroupComponent (type 22) requires at least one option in options.", paramName);
		for (var i = 0; i < checkboxGroup.Options.Length; i++)
		{
			var opt = checkboxGroup.Options[i];
			if (opt == null)
				throw new ArgumentException($"CheckboxGroupComponent.Options[{i}] cannot be null.", paramName);
			if (string.IsNullOrWhiteSpace(opt.Value))
				throw new ArgumentException($"CheckboxGroupComponent.Options[{i}].Value is required.", paramName);
			if (string.IsNullOrWhiteSpace(opt.Label))
				throw new ArgumentException($"CheckboxGroupComponent.Options[{i}].Label is required.", paramName);
		}
	}
}