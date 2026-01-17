using DiscoSdk.Events;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using System;
using System.Text.Json;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Dispatches Discord Gateway events to registered handlers.
/// </summary>
public class DiscordEventDispatcher : IDiscordEventRegistry
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
                case "MESSAGE_CREATE":
                    await ProcessMessageCreateAsync(payload);
                    break;

                case "MESSAGE_UPDATE":
                    await ProcessMessageUpdateAsync(payload);
                    break;

                case "MESSAGE_DELETE":
                    await ProcessMessageDeleteAsync(payload);
                    break;

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

                case "MESSAGE_REACTION_ADD":
                    await ProcessMessageReactionAddAsync(payload);
                    break;

                case "MESSAGE_REACTION_REMOVE":
                    await ProcessMessageReactionRemoveAsync(payload);
                    break;

                case "TYPING_START":
                    await ProcessTypingStartAsync(payload);
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

        var eventData = new MessageCreateEvent { Message = message };

        await ProcessAll<IMessageCreateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessMessageUpdateAsync(JsonElement payload)
    {
        var message = payload.Deserialize<Message>(_discordClient.SerializerOptions);
        if (message == null) return;

        var eventData = new MessageUpdateEvent { Message = message };
        await ProcessAll<IMessageUpdateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessMessageDeleteAsync(JsonElement payload)
    {
        var eventData = new MessageDeleteEvent
        {
            Id = payload.GetProperty("id").GetString() ?? string.Empty,
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null
        };

        await ProcessAll<IMessageDeleteHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessGuildCreateAsync(JsonElement payload)
    {
        var guild = payload.Deserialize<Guild>(_discordClient.SerializerOptions);
        if (guild == null) return;

        _discordClient.GuildManager.HandleGuildCreate(guild);

        var eventData = new GuildCreateEvent { Guild = guild };
        await ProcessAll<IGuildCreateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessGuildUpdateAsync(JsonElement payload)
    {
        var guild = payload.Deserialize<Guild>(_discordClient.SerializerOptions);
        if (guild == null) return;

        _discordClient.GuildManager.HandleGuildUpdate(guild);

        var eventData = new GuildUpdateEvent { Guild = guild };
        await ProcessAll<IGuildUpdateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessGuildDeleteAsync(JsonElement payload)
    {
        Snowflake.TryParse(payload.GetProperty("id").GetString(), out var snowflake);
        var eventData = new GuildDeleteEvent
        {
            Id = snowflake,
            Unavailable = payload.TryGetProperty("unavailable", out var unavailable) && unavailable.GetBoolean()
        };

        _discordClient.GuildManager.HandleGuildDelete(eventData.Id);
        await ProcessAll<IGuildDeleteHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessChannelCreateAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (channel == null) return;

        _discordClient.GuildManager.HandleChannelCreate(channel);

        var eventData = new ChannelCreateEvent { Channel = channel };
        await ProcessAll<IChannelCreateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessChannelUpdateAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (channel == null) return;

        _discordClient.GuildManager.HandleChannelUpdate(channel);

        var eventData = new ChannelUpdateEvent { Channel = channel };
        await ProcessAll<IChannelUpdateHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessChannelDeleteAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (channel == null) return;

        _discordClient.GuildManager.HandleChannelDelete(channel);
        var eventData = new ChannelDeleteEvent { Channel = channel };
        await ProcessAll<IChannelDeleteHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessMessageReactionAddAsync(JsonElement payload)
    {
        var eventData = new MessageReactionAddEvent
        {
            UserId = payload.GetProperty("user_id").GetString() ?? string.Empty,
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            MessageId = payload.GetProperty("message_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null,
            Member = payload.TryGetProperty("member", out var member) ? member.Deserialize<GuildMember>(_discordClient.SerializerOptions) : null,
            Emoji = payload.GetProperty("emoji").Deserialize<Emoji>(_discordClient.SerializerOptions) ?? new Emoji()
        };

        await ProcessAll<IMessageReactionAddHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessMessageReactionRemoveAsync(JsonElement payload)
    {
        var eventData = new MessageReactionRemoveEvent
        {
            UserId = payload.GetProperty("user_id").GetString() ?? string.Empty,
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            MessageId = payload.GetProperty("message_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null,
            Emoji = payload.GetProperty("emoji").Deserialize<Emoji>(_discordClient.SerializerOptions) ?? new Emoji()
        };

        await ProcessAll<IMessageReactionRemoveHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessTypingStartAsync(JsonElement payload)
    {
        var eventData = new TypingStartEvent
        {
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null,
            UserId = payload.GetProperty("user_id").GetString() ?? string.Empty,
            Timestamp = payload.GetProperty("timestamp").GetInt32(),
            Member = payload.TryGetProperty("member", out var member) ? member.Deserialize<GuildMember>(_discordClient.SerializerOptions) : null
        };

        await ProcessAll<ITypingStartHandler>(x => x.HandleAsync(eventData));
    }

    private async Task ProcessInteractionCreateAsync(JsonElement payload)
    {
        try
        {
            var interaction = payload.Deserialize<Interaction>(_discordClient.SerializerOptions);
            if (interaction == null)
                return;

            var handle = new InteractionHandle(interaction.Id, interaction.Token);

            if (interaction.Type == InteractionType.ModalSubmit)
                await _discordClient.InteractionClient.AcknowledgeAsync(handle, Rest.Clients.InteractionClient.AcknowledgeType.ModalSubmit, CancellationToken.None);

            var channel = interaction.ChannelId is not null ? await _discordClient.GetChannel<ITextBasedChannel>(interaction.ChannelId.Value).ExecuteAsync() : null;
            var guild = channel is IGuildChannelBase guildChannel ? guildChannel.Guild : null;
            var member = guild is not null && interaction.Member is not null ? new GuildMemberWrapper(interaction.Member, guild, _discordClient) : null;

            var interactionWrapper = new InteractionWrapper(interaction, _discordClient, handle, channel, member);
            var eventData = new InteractionCreateEvent(interactionWrapper, _discordClient);

            try
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                    await ProcessAll<IApplicationCommandHandler>(x => x.HandleAsync(eventData));

                if (interaction.Type == InteractionType.ModalSubmit)
                    await ProcessAll<IModalSubmitHandler>(x => x.HandleAsync(eventData));

                if (interaction.Type == InteractionType.MessageComponent)
                    await ProcessAll<IComponentInteractionHandler>(x => x.HandleAsync(eventData));

                await ProcessAll<IInteractionCreateHandler>(x => x.HandleAsync(eventData));
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

    private async Task ProcessAll<T>(Func<T, Task> calllback) where T : IDiscordEventHandler
    {
        foreach (var handler in GetHandlersOfType<T>())
        {
            try
            {
                await calllback(handler);
            }
            catch (Exception ex)
            {
                _discordClient.Logger.Log(LogLevel.Error, $"Error in {typeof(T).Name}", ex);
            }
        }
    }

    private IEnumerable<T> GetHandlersOfType<T>() where T : IDiscordEventHandler
    {
        var targetType = typeof(T);
        if (!_handlerIndicesByType.TryGetValue(targetType, out var indices))
            yield break;

        foreach (var index in indices)
        {
            if (_handlers[index] is T typedHandler)
                yield return typedHandler;
        }
    }
}