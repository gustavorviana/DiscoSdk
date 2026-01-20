using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Models;
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
/// Implementation of <see cref="ISendMessageRestAction"/> for sending messages to Discord.
/// Supports both regular channel messages and interaction responses.
/// </summary>
internal class SendMessageRestAction : RestAction<IMessage>, ISendMessageRestAction
{
    private readonly InteractionHandle? _interactionHandle;
    private readonly List<MessageComponent> _components = [];
    private MessageReference? _messageReference;
    private readonly ITextBasedChannel _channel;
    private AllowedMentions? _allowedMentions;
    private readonly List<Embed> _embeds = [];
    private readonly DiscordClient _client;
    private string? _content;
    private bool _ephemeral;
    private bool _tts;
    private List<Snowflake>? _stickers;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendMessageRestAction"/> class for regular channel messages.
    /// </summary>
    /// <param name="client">The REST client base to use for requests.</param>
    /// <param name="channelId">The ID of the channel to send the message to.</param>
    /// <param name="content">The initial message content.</param>
    public SendMessageRestAction(DiscordClient client, InteractionHandle? interactionHandle, ITextBasedChannel channel, string? content)
    {
        _channel = channel ?? throw new ArgumentException(nameof(channel));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionHandle = interactionHandle;
        _content = content;
    }

    /// <inheritdoc />
    public ISendMessageRestAction SetContent(string? content)
    {
        if (content != null && content.Length > 2000)
            throw new ArgumentException("Message content cannot exceed 2000 characters.", nameof(content));

        _content = content;
        return this;
    }

    /// <inheritdoc />
    public ISendMessageRestAction AddEmbeds(params Embed[] embeds)
    {
        ArgumentNullException.ThrowIfNull(embeds);

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
    public ISendMessageRestAction AddActionRow(params MessageComponent[] items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Length == 0)
            throw new ArgumentException("At least one component must be provided.", nameof(items));

        if (_components.Count >= 5)
            throw new InvalidOperationException("Message cannot have more than 5 component rows.");

        // Create an ActionRow containing the items
        var actionRow = new MessageComponent
        {
            Type = ComponentType.ActionRow,
            Components = [.. items]
        };

        _components.Add(actionRow);
        return this;
    }

    /// <inheritdoc />
    public ISendMessageRestAction SetComponents(params MessageComponent[] components)
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

            // Wrap non-ActionRow components in an ActionRow
            return new MessageComponent
            {
                Type = ComponentType.ActionRow,
                Components = [c]
            };
        }));

        return this;
    }

    /// <inheritdoc />
    public ISendMessageRestAction SetTts(bool tts)
    {
        _tts = tts;
        return this;
    }

    /// <inheritdoc />
    public ISendMessageRestAction SetMessageReference(string? messageId, string? channelId = null, string? guildId = null, bool? failIfNotExists = null)
    {
        if (messageId == null)
        {
            _messageReference = null;
            return this;
        }

        _messageReference = new MessageReference
        {
            MessageId = messageId,
            ChannelId = channelId,
            GuildId = guildId,
            FailIfNotExists = failIfNotExists
        };

        return this;
    }

    /// <inheritdoc />
    public ISendMessageRestAction SetAllowedMentions(string[]? parse = null, string[]? users = null, string[]? roles = null)
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
    public ISendMessageRestAction SetEphemeral(bool ephemeral = true)
    {
        _ephemeral = ephemeral;
        return this;
    }

    /// <inheritdoc />
    public ISendMessageRestAction SetStickers(IEnumerable<Snowflake> stickers)
    {
        _stickers = stickers?.ToList();
        return this;
    }

    /// <inheritdoc />
    public override Task<IMessage> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_content) && _embeds.Count == 0)
            throw new InvalidOperationException("Message must have either content or at least one embed.");

        if (_interactionHandle != null)
        {
            if (_tts)
                throw new InvalidOperationException("TTS is not supported in interaction responses.");
            if (_messageReference != null)
                throw new InvalidOperationException("Message references are not supported in interaction responses.");
        }

        if (_interactionHandle == null)
            return SendChannelMessageAsync(cancellationToken);

        try
        {
            if (_interactionHandle.IsDeferred)
                return SendFollowUpAsync(cancellationToken);

            return SendInteractionResponseAsync(cancellationToken);
        }
        finally
        {
            _interactionHandle.Responded = true;
        }
    }

    private async Task<IMessage> SendChannelMessageAsync(CancellationToken cancellationToken)
    {
        var request = new MessageCreateRequest
        {
            Content = _content,
            Tts = _tts ? true : null,
            Embeds = [.. _embeds],
            Components = [.. _components],
            MessageReference = _messageReference,
            AllowedMentions = _allowedMentions,
            Flags = _ephemeral ? MessageFlags.Ephemeral : null,
            StickerIds = _stickers?.Select(s => s.ToString()).ToArray()
        };

        var message = await _client.MessageClient.CreateAsync(_channel.Id, request, cancellationToken);
        return new MessageWrapper(_client, _channel, message, _interactionHandle);
    }

    private async Task<IMessage> SendInteractionResponseAsync(CancellationToken cancellationToken)
    {
        var flags = MessageFlags.None;
        if (_ephemeral)
            flags |= MessageFlags.Ephemeral;

        var callbackData = new InteractionCallbackData
        {
            Content = _content,
            Flags = flags,
            Components = EnsureActionRows(),
            Embeds = [.. _embeds]
        };

        await _client.InteractionClient.RespondAsync(_interactionHandle!, callbackData, cancellationToken);
        var message = await _client.InteractionClient.GetOriginalResponseAsync(_interactionHandle!, cancellationToken);
        return new MessageWrapper(_client, _channel, message, EnsureInteractionHandle());
    }

    private async Task<IMessage> SendFollowUpAsync(CancellationToken cancellationToken)
    {
        var flags = MessageFlags.None;
        if (_ephemeral)
            flags |= MessageFlags.Ephemeral;

        var actionRows = EnsureActionRows();
        await _client.InteractionClient.FollowUpAsync(_interactionHandle!, new FollowUpMessageRequest
        {
            Components = actionRows.OfType<MessageComponent>().ToArray(),
            Content = _content,
            Embeds = [.. _embeds],
            Flags = flags != MessageFlags.None ? flags : null
        }, cancellationToken);

        var message = await _client.InteractionClient.GetOriginalResponseAsync(_interactionHandle!, cancellationToken);
        return new MessageWrapper(_client, _channel, message, EnsureInteractionHandle());
    }

    private InteractionHandle? EnsureInteractionHandle()
    {
        return _interactionHandle is not null && (_ephemeral || _interactionHandle!.IsDeferred) ? _interactionHandle : null;
    }

    private IInteractionComponent[] EnsureActionRows()
    {
        if (_components == null || _components.Count == 0)
            return [];

        return [.._components.Select(c =>
        {
            if (c.Type == ComponentType.ActionRow)
                return c;

            return new MessageComponent
            {
                Type = ComponentType.ActionRow,
                Components = [c]
            };
        })];
    }
}

