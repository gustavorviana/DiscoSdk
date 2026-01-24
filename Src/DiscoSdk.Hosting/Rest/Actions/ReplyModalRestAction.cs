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
    private readonly List<ActionRowComponent> _components = [];
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
    public IReplyModalRestAction AddActionRow(TextInputComponent textInput)
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

    /// <inheritdoc />
    public IReplyModalRestAction AddActionRow(TextInputBuilder textInputBuilder)
    {
        ArgumentNullException.ThrowIfNull(textInputBuilder);

        var textInput = textInputBuilder.Build();
        return AddActionRow(textInput);
    }

    /// <inheritdoc />
    public IReplyModalRestAction SetComponents(params ActionRowComponent[] components)
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

        var modalData = new ModalData
        {
            CustomId = _customId,
            Title = _title,
            Components = [.. _components]
        };

        return _client.InteractionClient.RespondWithModalAsync(_interactionHandle, modalData, cancellationToken);
    }
}