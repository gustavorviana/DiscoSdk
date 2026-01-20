using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Requests;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditMessageRestAction"/> for editing messages in Discord.
/// </summary>
internal class EditMessageRestAction : RestAction<IMessage>, IEditMessageRestAction
{
    private readonly DiscordClient _client;
    private readonly ITextBasedChannel _channel;
    private readonly Snowflake _messageId;
    private string? _content;
    private readonly List<Embed> _embeds = [];
    private readonly List<MessageComponent> _components = [];
    private AllowedMentions? _allowedMentions;
    private MessageFlags? _flags;
    private readonly InteractionHandle? _interactionHandle;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMessageRestAction"/> class.
    /// </summary>
    /// <param name="client">The Discord client.</param>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to edit.</param>
    public EditMessageRestAction(DiscordClient client, ITextBasedChannel channel, Snowflake messageId, InteractionHandle? interactionHandle)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionHandle = interactionHandle;
        _messageId = messageId;
        _channel = channel;

    }

    /// <inheritdoc />
    public IEditMessageRestAction SetContent(string? content)
    {
        if (content != null && content.Length > 2000)
            throw new ArgumentException("Message content cannot exceed 2000 characters.", nameof(content));

        _content = content;
        return this;
    }

    /// <inheritdoc />
    public IEditMessageRestAction SetEmbeds(params Embed[] embeds)
    {
        ArgumentNullException.ThrowIfNull(embeds);

        _embeds.Clear();

        foreach (var embed in embeds)
        {
            if (embed == null)
                continue;

            if (_embeds.Count >= 10)
                throw new InvalidOperationException("Message cannot have more than 10 embeds.");

            _embeds.Add(embed);
        }

        return this;
    }

    /// <inheritdoc />
    public IEditMessageRestAction SetComponents(params MessageComponent[] components)
    {
        if (components == null || components.Length == 0)
            return this;

        if (components.Length > 5)
            throw new ArgumentException("Message cannot have more than 5 component rows.", nameof(components));

        _components.Clear();

        _components.AddRange(components.Select(c =>
        {
            if (c.Type == ComponentType.ActionRow)
                return c;

            return new MessageComponent
            {
                Type = ComponentType.ActionRow,
                Components = [c]
            };
        }));

        return this;
    }

    /// <inheritdoc />
    public IEditMessageRestAction SetAllowedMentions(string[]? parse = null, string[]? users = null, string[]? roles = null)
    {
        if (parse == null && users == null && roles == null)
        {
            _allowedMentions = null;
            return this;
        }

        _allowedMentions = new AllowedMentions
        {
            Parse = parse,
            Users = users,
            Roles = roles
        };

        return this;
    }

    /// <inheritdoc />
    public IEditMessageRestAction SetFlags(MessageFlags? flags)
    {
        _flags = flags;
        return this;
    }

    /// <inheritdoc />
    public override async Task<IMessage> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_content) && _embeds.Count == 0)
            throw new InvalidOperationException("Message must have either content or at least one embed.");

        var request = new MessageEditRequest
        {
            Content = _content,
            Embeds = _embeds.Count > 0 ? [.. _embeds] : null,
            Components = _components.Count > 0 ? [.. _components] : null,
            AllowedMentions = _allowedMentions,
            Flags = _flags
        };

        Message message;
        if (_interactionHandle == null)
            message = await _client.MessageClient.EditAsync(_channel.Id, _messageId, request, cancellationToken);
        else
            message = await _client.InteractionClient.EditOriginalResponseAsync(_interactionHandle, request, cancellationToken);

        return new MessageWrapper(_client, _channel, message, _interactionHandle);
    }
}

