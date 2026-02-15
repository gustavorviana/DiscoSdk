using DiscoSdk.Events;
using DiscoSdk.Hosting.Contexts;
using DiscoSdk.Hosting.Contexts.Channels;
using DiscoSdk.Hosting.Contexts.Guilds;
using DiscoSdk.Hosting.Contexts.Messages;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Contexts.Wrappers;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway.Events;

/// <summary>
/// Dispatches Discord Gateway events to registered handlers.
/// </summary>
internal class DiscordEventDispatcher : IDiscordEventRegistry
{
    private readonly List<IDiscordEventHandler> _handlers = [];
    private readonly Dictionary<Type, HashSet<int>> _handlerIndicesByType = [];
    private readonly DiscordClient _discordClient;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordEventDispatcher"/> class.
    /// </summary>
    /// <param name="logger">The logger instance. If null, uses NullLogger.</param>
    public DiscordEventDispatcher(DiscordClient discordClient)
    {
        _discordClient = discordClient;
    }

    /// <summary>
    /// Registers a Discord event handler.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    public void Add(IDiscordEventHandler handler)
    {
        lock (_lock)
        {
            // Add handler to the main list and get its index
            var index = _handlers.Count;
            if (!_handlers.Contains(handler))
                _handlers.Add(handler);

            // Discover all implemented interfaces that derive from IDiscordEventHandler
            var handlerType = handler.GetType();
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i != typeof(IDiscordEventHandler) &&
                            typeof(IDiscordEventHandler).IsAssignableFrom(i));

            // Store only the handler index (not the reference) per handler type
            foreach (var interfaceType in interfaces)
            {
                if (!_handlerIndicesByType.TryGetValue(interfaceType, out var indices))
                {
                    indices = [];
                    _handlerIndicesByType[interfaceType] = indices;
                }
                indices.Add(index);
            }
        }
    }

    /// <summary>
    /// Processes a Gateway event based on the event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="payload">The JSON payload of the event.</param>
    /// <param name="_discordClient.SerializerOptions">The JSON serializer options to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task ProcessEventAsync(ReceivedGatewayMessage message)
    {
        if (string.IsNullOrEmpty(message.EventType))
            return;

        using var doc = message.ToJsonDocument();
        var payload = doc.RootElement;

        try
        {
            switch (message.EventType)
            {
                case "GUILD_CREATE":
                    await ProcessGuildCreateAsync(payload);
                    break;

                case "GUILD_UPDATE":
                    await ProcessGuildUpdateAsync(payload);
                    break;

                case "GUILD_DELETE":
                    await ProcessGuildDeleteAsync(payload);
                    break;

                case "CHANNEL_CREATE":
                    await ProcessChannelCreateAsync(payload);
                    break;

                case "CHANNEL_UPDATE":
                    await ProcessChannelUpdateAsync(payload);
                    break;

                case "CHANNEL_DELETE":
                    await ProcessChannelDeleteAsync(payload);
                    break;

                case "MESSAGE_CREATE":
                    await ProcessMessageCreateAsync(payload);
                    break;

                case "MESSAGE_UPDATE":
                    await ProcessMessageUpdateAsync(payload);
                    break;

                case "MESSAGE_DELETE":
                    await ProcessMessageDeleteAsync(new JsonElementParser(payload));
                    break;

                case "MESSAGE_REACTION_ADD":
                    await ProcessMessageReactionAddAsync(new JsonElementParser(payload));
                    break;

                case "MESSAGE_REACTION_REMOVE":
                    await ProcessMessageReactionRemoveAsync(new JsonElementParser(payload));
                    break;

                case "TYPING_START":
                    await ProcessTypingStartAsync(new JsonElementParser(payload));
                    break;

                case "INTERACTION_CREATE":
                    await ProcessInteractionCreateAsync(payload);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw to prevent breaking the event loop
            _discordClient.Logger.Log(LogLevel.Error, $"Error processing event {message.EventType}", ex);
        }
    }

    private async Task ProcessMessageCreateAsync(JsonElement payload)
    {
        var message = payload.Deserialize<Message>(_discordClient.SerializerOptions);
        if (message == null)
            return;

        var guild = _discordClient.Guilds.GetWrapped(message.GuildId);
        var member = message.Member != null ? new GuildMemberWrapper(_discordClient, message.Member, guild!) : null;
        var channel = await _discordClient.Channels.Get(message.ChannelId, guild).ExecuteAsync();
        var wrappedMessage = new MessageWrapper(_discordClient, (ITextBasedChannel)channel!, message, null);

        var eventData = new MessageCreateContextWrapper(_discordClient, member, wrappedMessage, (ITextBasedChannel)channel);

        await ProcessAllAsync<IMessageCreateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessMessageUpdateAsync(JsonElement payload)
    {
        var message = payload.Deserialize<Message>(_discordClient.SerializerOptions);
        if (message == null) return;

        var guild = _discordClient.Guilds.GetWrapped(message.GuildId);
        var member = message.Member != null ? new GuildMemberWrapper(_discordClient, message.Member, guild!) : null;
        var channel = _discordClient.Channels.GetWrappedTextChannel(message.ChannelId);
        var wrappedMessage = new MessageWrapper(_discordClient, channel!, message, null);
        var author = new UserWrapper(_discordClient, message.Author);

        var eventData = new MessageUpdateContextWrapper(_discordClient, guild, author, member, wrappedMessage, channel);
        await ProcessAllAsync<IMessageUpdateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessMessageDeleteAsync(JsonElementParser payload)
    {
        var id = payload.GetSnowflake("id")!.Value;
        var channelId = payload.GetSnowflake("channel_id")!.Value;
        var guildId = payload.GetSnowflake("guild_id");
        var wrappedChannel = _discordClient.Channels.GetWrappedTextChannel(channelId);
        var wrappedGuild = _discordClient.Guilds.GetWrapped(guildId);

        var eventData = new MessageDeleteContextWrapper(_discordClient, id, wrappedChannel!, wrappedGuild);
        await ProcessAllAsync<IMessageDeleteHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessGuildCreateAsync(JsonElement payload)
    {
        var guild = payload.Deserialize<Guild>(_discordClient.SerializerOptions);
        if (_discordClient.Guilds.HandleGuildCreate(guild) is not IGuild wrappedGuild)
            return;

        var eventData = new GuildContextWrapper(_discordClient, wrappedGuild);
        await ProcessAllAsync<IGuildCreateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessGuildUpdateAsync(JsonElement payload)
    {
        var guild = payload.Deserialize<Guild>(_discordClient.SerializerOptions);
        if (_discordClient.Guilds.HandleGuildUpdate(guild) is not IGuild wrappedGuild) return;

        var eventData = new GuildContextWrapper(_discordClient, wrappedGuild);
        await ProcessAllAsync<IGuildUpdateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessGuildDeleteAsync(JsonElement payload)
    {
        var eventData = new GuildDeleteContextWrapper(_discordClient, payload);
        _discordClient.Guilds.HandleGuildDelete(eventData.Id);

        await ProcessAllAsync<IGuildDeleteHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessChannelCreateAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (TryGetChannelGuild(channel, out var guild)) return;

        _discordClient.Guilds.HandleChannelCreate(channel!);

        var eventData = new ChannelContext(_discordClient, new GuildChannelUnionWrapper(_discordClient, channel!, guild!));
        await ProcessAllAsync<IChannelCreateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessChannelUpdateAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (TryGetChannelGuild(channel, out var guild)) return;

        _discordClient.Guilds.HandleChannelUpdate(channel!);

        var eventData = new ChannelContext(_discordClient, new GuildChannelUnionWrapper(_discordClient, channel!, guild!));
        await ProcessAllAsync<IChannelUpdateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessChannelDeleteAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (TryGetChannelGuild(channel, out var guild)) return;

        _discordClient.Guilds.HandleChannelDelete(channel);
        var eventData = new ChannelDeleteContext(_discordClient, guild!, channel.Id);
        await ProcessAllAsync<IChannelDeleteHandler>(x => x.HandleAsync(eventData));
    }

    private bool TryGetChannelGuild(Channel? channel, out GuildWrapper? guild)
    {
        if (channel == null || channel.Id.Empty || !_discordClient.Guilds.TryGet(channel.GuildId!.Value, out var iGuild))
        {
            guild = null;
            return false;
        }

        guild = (GuildWrapper)iGuild!;
        return true;
    }

    private async Task ProcessMessageReactionAddAsync(JsonElementParser payload)
    {
        var userId = payload.GetSnowflake("user_id")!.Value;
        var messageId = payload.GetSnowflake("message_id")!.Value;

        var member = payload.Deserialize<GuildMember>("member", _discordClient.SerializerOptions);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        var channel = _discordClient.Channels.GetWrappedTextChannel(payload.GetSnowflake("channel_id")!.Value);
        var user = await GetUserAsync(member, userId);
        var wrappedMember = member is not null ? new GuildMemberWrapper(_discordClient, member, guild) : null;
        var emoji = payload.Deserialize<Emoji>("emoji", _discordClient.SerializerOptions);


        var eventData = new MessageAddReactionContext(_discordClient,
            channel,
            guild,
            user,
            messageId,
            wrappedMember,
            new EmojiWrapper(_discordClient, emoji, guild));

        await ProcessAllAsync<IMessageReactionAddHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessMessageReactionRemoveAsync(JsonElementParser payload)
    {
        var userId = payload.GetSnowflake("user_id")!.Value;
        var messageId = payload.GetSnowflake("message_id")!.Value;

        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        var channel = _discordClient.Channels.GetWrappedTextChannel(payload.GetSnowflake("channel_id")!.Value);
        var emoji = payload.Deserialize<Emoji>("emoji", _discordClient.SerializerOptions);

        var json = payload.Payload.ToString();
        var eventData = new MessageDeleteReactionContextWrapper(_discordClient,
            channel,
            guild,
            userId,
            messageId,
            new EmojiWrapper(_discordClient, emoji, guild));

        await ProcessAllAsync<IMessageReactionRemoveHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessTypingStartAsync(JsonElementParser payload)
    {
        var userId = payload.GetSnowflake("user_id")!.Value;
        var startedAt = payload.GetDateTimeOffset("timestamp")!.Value;

        var member = payload.Deserialize<GuildMember>("member", _discordClient.SerializerOptions);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        var channel = _discordClient.Channels.GetWrappedTextChannel(payload.GetSnowflake("channel_id")!.Value);
        var user = await GetUserAsync(member, userId);
        var wrappedMember = member is not null ? new GuildMemberWrapper(_discordClient, member, guild) : null;

        var eventData = new TypingContextWrapper(_discordClient, startedAt, channel!, guild, user, wrappedMember);

        await ProcessAllAsync<ITypingStartHandler>(x => x.HandleAsync(eventData));
    }

    private async Task<IUser?> GetUserAsync(GuildMember? member, Snowflake userId)
    {
        if (member?.User is not null)
            return new UserWrapper(_discordClient, member.User);

        return await _discordClient.Users.Get(userId).ExecuteAsync();
    }

    private async Task ProcessInteractionCreateAsync(JsonElement payload)
    {
        try
        {
            var interaction = payload.Deserialize<Interaction>(_discordClient.SerializerOptions);
            if (interaction == null)
                return;

            var handle = new InteractionHandle(interaction.Id, interaction.Token);
            var channel = interaction.ChannelId is not null ? await _discordClient.GetChannel<ITextBasedChannel>(interaction.ChannelId.Value).ExecuteAsync() : null;
            var guild = channel is IGuildChannelBase guildChannel ? guildChannel.Guild : null;
            var member = guild is not null && interaction.Member is not null ? new GuildMemberWrapper(_discordClient, interaction.Member, guild) : null;

            var interactionWrapper = new InteractionWrapper(interaction, _discordClient, handle, channel, member);
            var interactionContext = new InteractionContextWrapper(_discordClient, interactionWrapper);

            try
            {
                if (interaction.Type == InteractionType.ApplicationCommandAutocomplete)
                    await HandleAutocompleteAsync(interactionWrapper);

                if (interaction.Type == InteractionType.ApplicationCommand)
                    await HandleCommandAsync(interactionWrapper);

                if (interaction.Type == InteractionType.ModalSubmit)
                    await HandleModalAsync(interactionWrapper);

                if (interaction.Type == InteractionType.MessageComponent)
                    await ProcessAllAsync<IComponentInteractionHandler>(handle, x => x.HandleAsync(interactionContext));

                await ProcessAllAsync<IInteractionCreateHandler>(handle, x => x.HandleAsync(interactionContext));
            }
            catch (Exception ex)
            {
                _discordClient.Logger.Log(LogLevel.Error, $"Error processing handlers for interaction {interaction.Id}", ex);
            }
        }
        catch (Exception ex)
        {
            _discordClient.Logger.Log(LogLevel.Error, "Error processing INTERACTION_CREATE", ex);
        }
    }

    private async Task HandleModalAsync(InteractionWrapper interactionWrapper)
    {
        var commandHandlers = GetHandlersOfType<IModalSubmitHandler>();
        if (commandHandlers.Count == 0)
            return;

        var commandContext = new ModalContext(_discordClient, interactionWrapper);
        await ProcessAllAsync(interactionWrapper.Handle, commandHandlers, x => x.HandleAsync(commandContext));
    }

    private async Task HandleAutocompleteAsync(InteractionWrapper interactionWrapper)
    {
        var autocompleteHandlers = GetHandlersOfType<IAutocompleteHandler>();
        if (autocompleteHandlers.Count == 0)
            return;

        var autocompleteContext = new AutocompleteContext(_discordClient, interactionWrapper);
        await ProcessAllAsync(interactionWrapper.Handle, autocompleteHandlers, x => x.HandleAsync(autocompleteContext));
    }

    private async Task HandleCommandAsync(InteractionWrapper interactionWrapper)
    {
        var commandHandlers = GetHandlersOfType<IApplicationCommandHandler>();
        if (commandHandlers.Count == 0)
            return;

        var commandContext = new CommandContext(_discordClient, interactionWrapper);
        await ProcessAllAsync(interactionWrapper.Handle, commandHandlers, x => x.HandleAsync(commandContext));
    }

    private async Task ProcessAllAsync<T>(Func<T, Task> calllback) where T : IDiscordEventHandler
    {
        await ProcessAllAsync(GetHandlersOfType<T>(), async x =>
        {
            await calllback(x);
            return false;
        });
    }

    private async Task ProcessAllAsync<T>(InteractionHandle handle, Func<T, Task> calllback) where T : IDiscordEventHandler
    {
        await ProcessAllAsync(handle, GetHandlersOfType<T>(), calllback);
    }

    private async Task ProcessAllAsync<T>(InteractionHandle handle, List<T> handlers, Func<T, Task> calllback) where T : IDiscordEventHandler
    {
        if (!handle.Responded)
            await ProcessAllAsync(handlers, async x =>
            {
                await calllback(x);
                return handle.Responded;
            });
    }

    private async Task ProcessAllAsync<T>(List<T> handlers, Func<T, Task<bool>> calllback) where T : IDiscordEventHandler
    {
        foreach (var handler in handlers)
        {
            try
            {
                var responded = await calllback(handler);
                if (responded)
                    break;
            }
            catch (Exception ex)
            {
                _discordClient.Logger.Log(LogLevel.Error, $"Error in {typeof(T).Name}", ex);
            }
        }
    }

    private List<T> GetHandlersOfType<T>() where T : IDiscordEventHandler
    {
        var targetType = typeof(T);
        if (!_handlerIndicesByType.TryGetValue(targetType, out var indexes))
            return [];

        var items = new List<T>(indexes.Count);

        foreach (var index in indexes)
            if (_handlers[index] is T typedHandler)
                items.Add(typedHandler);

        return items;
    }
}