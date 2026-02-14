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

		if (modalComponent is not TextInputComponent and not CheckboxGroupComponent)
			throw new ArgumentException("Action row in modal only supports TextInputComponent or CheckboxGroupComponent.", nameof(modalComponent));

		if (_components.Count >= 5)
			throw new InvalidOperationException("Modal cannot have more than 5 components.");

		if (modalComponent is CheckboxGroupComponent checkboxGroup)
		{
			ValidateCheckboxGroupForModal(checkboxGroup, nameof(modalComponent));

			var labelContainer = new LabelComponent
			{
				Label = checkboxGroup.Label,
				Component = checkboxGroup
			};
			_components.Add(labelContainer);
			return this;
		}

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
			if (c is LabelComponent label && label.Component is CheckboxGroupComponent checkboxGroup)
				ValidateCheckboxGroupForModal(checkboxGroup, "components");
		}
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