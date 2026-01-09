using DiscoSdk.Events;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Logging;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Dispatches Discord Gateway events to registered handlers.
/// </summary>
public class DiscordEventDispatcher : IDiscordEventRegistry
{
    private readonly object _lock = new();
    private readonly List<IDiscordEventHandler> _handlers = [];
    private readonly Dictionary<Type, HashSet<int>> _handlerIndicesByType = [];
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordEventDispatcher"/> class.
    /// </summary>
    /// <param name="logger">The logger instance. If null, uses NullLogger.</param>
    public DiscordEventDispatcher(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
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
    /// <param name="jsonOptions">The JSON serializer options to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task ProcessEventAsync(DiscordClient client, ReceivedGatewayMessage message)
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
                    await ProcessMessageCreateAsync(payload, client.SerializerOptions);
                    break;

                case "MESSAGE_UPDATE":
                    await ProcessMessageUpdateAsync(payload, client.SerializerOptions);
                    break;

                case "MESSAGE_DELETE":
                    await ProcessMessageDeleteAsync(payload, client.SerializerOptions);
                    break;

                case "GUILD_CREATE":
                    await ProcessGuildCreateAsync(payload, client.SerializerOptions);
                    break;

                case "GUILD_UPDATE":
                    await ProcessGuildUpdateAsync(payload, client.SerializerOptions);
                    break;

                case "GUILD_DELETE":
                    await ProcessGuildDeleteAsync(payload, client.SerializerOptions);
                    break;

                case "CHANNEL_CREATE":
                    await ProcessChannelCreateAsync(payload, client.SerializerOptions);
                    break;

                case "CHANNEL_UPDATE":
                    await ProcessChannelUpdateAsync(payload, client.SerializerOptions);
                    break;

                case "CHANNEL_DELETE":
                    await ProcessChannelDeleteAsync(payload, client.SerializerOptions);
                    break;

                case "MESSAGE_REACTION_ADD":
                    await ProcessMessageReactionAddAsync(payload, client.SerializerOptions);
                    break;

                case "MESSAGE_REACTION_REMOVE":
                    await ProcessMessageReactionRemoveAsync(payload, client.SerializerOptions);
                    break;

                case "TYPING_START":
                    await ProcessTypingStartAsync(payload, client.SerializerOptions);
                    break;

                case "INTERACTION_CREATE":
                    await ProcessInteractionCreateAsync(client, payload, client.SerializerOptions);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw to prevent breaking the event loop
            _logger.Log(LogLevel.Error, $"Error processing event {message.EventType}", ex);
        }
    }

    private async Task ProcessMessageCreateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        try
        {
            var message = payload.Deserialize<Message>(jsonOptions);
            if (message == null)
                return;

            var eventData = new MessageCreateEvent { Message = message };

            foreach (var handler in GetHandlersOfType<IMessageCreateHandler>())
            {
                try
                {
                    await handler.HandleAsync(eventData);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    private async Task ProcessMessageUpdateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var message = payload.Deserialize<Message>(jsonOptions);
        if (message == null) return;

        var eventData = new MessageUpdateEvent { Message = message };
        foreach (var handler in GetHandlersOfType<IMessageUpdateHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessMessageDeleteAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var eventData = new MessageDeleteEvent
        {
            Id = payload.GetProperty("id").GetString() ?? string.Empty,
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null
        };

        foreach (var handler in GetHandlersOfType<IMessageDeleteHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessGuildCreateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var guild = payload.Deserialize<Guild>(jsonOptions);
        if (guild == null) return;

        var eventData = new GuildCreateEvent { Guild = guild };
        foreach (var handler in GetHandlersOfType<IGuildCreateHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessGuildUpdateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var guild = payload.Deserialize<Guild>(jsonOptions);
        if (guild == null) return;

        var eventData = new GuildUpdateEvent { Guild = guild };
        foreach (var handler in GetHandlersOfType<IGuildUpdateHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessGuildDeleteAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var eventData = new GuildDeleteEvent
        {
            Id = payload.GetProperty("id").GetString() ?? string.Empty,
            Unavailable = payload.TryGetProperty("unavailable", out var unavailable) && unavailable.GetBoolean()
        };

        foreach (var handler in GetHandlersOfType<IGuildDeleteHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessChannelCreateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var channel = payload.Deserialize<Channel>(jsonOptions);
        if (channel == null) return;

        var eventData = new ChannelCreateEvent { Channel = channel };
        foreach (var handler in GetHandlersOfType<IChannelCreateHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessChannelUpdateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var channel = payload.Deserialize<Channel>(jsonOptions);
        if (channel == null) return;

        var eventData = new ChannelUpdateEvent { Channel = channel };
        foreach (var handler in GetHandlersOfType<IChannelUpdateHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessChannelDeleteAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var channel = payload.Deserialize<Channel>(jsonOptions);
        if (channel == null) return;

        var eventData = new ChannelDeleteEvent { Channel = channel };
        foreach (var handler in GetHandlersOfType<IChannelDeleteHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessMessageReactionAddAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var eventData = new MessageReactionAddEvent
        {
            UserId = payload.GetProperty("user_id").GetString() ?? string.Empty,
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            MessageId = payload.GetProperty("message_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null,
            Member = payload.TryGetProperty("member", out var member) ? member.Deserialize<GuildMember>(jsonOptions) : null,
            Emoji = payload.GetProperty("emoji").Deserialize<Emoji>(jsonOptions) ?? new Emoji()
        };

        foreach (var handler in GetHandlersOfType<IMessageReactionAddHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessMessageReactionRemoveAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var eventData = new MessageReactionRemoveEvent
        {
            UserId = payload.GetProperty("user_id").GetString() ?? string.Empty,
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            MessageId = payload.GetProperty("message_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null,
            Emoji = payload.GetProperty("emoji").Deserialize<Emoji>(jsonOptions) ?? new Emoji()
        };

        foreach (var handler in GetHandlersOfType<IMessageReactionRemoveHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessTypingStartAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var eventData = new TypingStartEvent
        {
            ChannelId = payload.GetProperty("channel_id").GetString() ?? string.Empty,
            GuildId = payload.TryGetProperty("guild_id", out var guildId) ? guildId.GetString() : null,
            UserId = payload.GetProperty("user_id").GetString() ?? string.Empty,
            Timestamp = payload.GetProperty("timestamp").GetInt32(),
            Member = payload.TryGetProperty("member", out var member) ? member.Deserialize<GuildMember>(jsonOptions) : null
        };

        foreach (var handler in GetHandlersOfType<ITypingStartHandler>())
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessInteractionCreateAsync(DiscordClient client, JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        try
        {
            var interaction = payload.Deserialize<Interaction>(jsonOptions);
            if (interaction == null)
                return;

            var handle = new InteractionHandle(interaction.Id, interaction.Token);

            if (interaction.Type == InteractionType.ModalSubmit)
                await client.InteractionClient.AcknowledgeAsync(handle, Rest.Clients.InteractionClient.AcknowledgeType.ModalSubmit, CancellationToken.None);

            var channel = interaction.ChannelId is not null ? await client.GetChannel<ITextBasedChannel>(interaction.ChannelId.Value).ExecuteAsync() : null;
            var guild = channel is IGuildChannelBase guildChannel ? guildChannel.Guild : null;
            var member = guild is not null && interaction.Member is not null ? new GuildMemberWrapper(interaction.Member, guild, client) : null;

            var interactionWrapper = new InteractionWrapper(interaction, client, handle, channel, member);
            var eventData = new InteractionCreateEvent(interactionWrapper);

            try
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                    await ProcessAll<IApplicationCommandHandler>(eventData);

                if (interaction.Type == InteractionType.ModalSubmit)
                    await ProcessAll<IModalSubmitHandler>(eventData);

                if (interaction.Type == InteractionType.MessageComponent)
                    await ProcessAll<IComponentInteractionHandler>(eventData);

                await ProcessAll<IInteractionCreateHandler>(eventData);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error processing handlers for interaction {interaction.Id}", ex);
            }
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, "Error processing INTERACTION_CREATE", ex);
        }
    }

    private async Task ProcessAll<T>(object @event) where T : IDiscordEventHandler
    {
        foreach (var handler in GetHandlersOfType<T>())
        {
            try
            {
                var _handleAsync = handler.GetType().GetMethod("HandleAsync", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)!;
                await (Task)_handleAsync.Invoke(handler, [@event])!;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error in {typeof(T).Name}", ex);
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