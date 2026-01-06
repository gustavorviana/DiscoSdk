using DiscoSdk.Events;
using DiscoSdk.Models;
using System.Text.Json;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Dispatches Discord Gateway events to registered handlers.
/// </summary>
public class DiscordEventDispatcher : IDiscordEventDispatcher
{
    private bool _sealed = false;
    private readonly object _lock = new();
    private readonly List<IDiscordEventHandler> _handlers = [];
    private readonly Dictionary<Type, HashSet<int>> _handlerIndicesByType = [];

    /// <summary>
    /// Registers a Discord event handler.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    public void Register(IDiscordEventHandler handler)
    {
        if (_sealed)
            throw new InvalidOperationException("Cannot register new handlers after the dispatcher has been sealed.");

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

    public void Seal()
    {
        _sealed = true;
    }

    public void Unseal()
    {
        _sealed = false;
    }

    /// <summary>
    /// Processes a Gateway event based on the event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="payload">The JSON payload of the event.</param>
    /// <param name="jsonOptions">The JSON serializer options to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ProcessEventAsync(DiscordClient client, string? eventType, JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        if (string.IsNullOrEmpty(eventType))
            return;

        try
        {
            switch (eventType)
            {
                case "MESSAGE_CREATE":
                    await ProcessMessageCreateAsync(payload, jsonOptions);
                    break;

                case "MESSAGE_UPDATE":
                    await ProcessMessageUpdateAsync(payload, jsonOptions);
                    break;

                case "MESSAGE_DELETE":
                    await ProcessMessageDeleteAsync(payload, jsonOptions);
                    break;

                case "GUILD_CREATE":
                    await ProcessGuildCreateAsync(payload, jsonOptions);
                    break;

                case "GUILD_UPDATE":
                    await ProcessGuildUpdateAsync(payload, jsonOptions);
                    break;

                case "GUILD_DELETE":
                    await ProcessGuildDeleteAsync(payload, jsonOptions);
                    break;

                case "CHANNEL_CREATE":
                    await ProcessChannelCreateAsync(payload, jsonOptions);
                    break;

                case "CHANNEL_UPDATE":
                    await ProcessChannelUpdateAsync(payload, jsonOptions);
                    break;

                case "CHANNEL_DELETE":
                    await ProcessChannelDeleteAsync(payload, jsonOptions);
                    break;

                case "MESSAGE_REACTION_ADD":
                    await ProcessMessageReactionAddAsync(payload, jsonOptions);
                    break;

                case "MESSAGE_REACTION_REMOVE":
                    await ProcessMessageReactionRemoveAsync(payload, jsonOptions);
                    break;

                case "TYPING_START":
                    await ProcessTypingStartAsync(payload, jsonOptions);
                    break;

                case "INTERACTION_CREATE":
                    await ProcessInteractionCreateAsync(client, payload, jsonOptions);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw to prevent breaking the event loop
            Console.WriteLine($"Error processing event {eventType}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

            var eventData = new InteractionCreateEvent(client, interaction);

            foreach (var handler in GetHandlersOfType<IInteractionCreateHandler>())
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

    private IEnumerable<T> GetHandlersOfType<T>() where T : IDiscordEventHandler
    {
        if (!_sealed)
            throw new InvalidOperationException("Cannot retrieve handlers before the dispatcher has been sealed.");

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

