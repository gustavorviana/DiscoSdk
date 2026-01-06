using DiscoSdk.Events;
using DiscoSdk.Hosting.Rest;
using DiscoSdk.Models;
using System.Text.Json;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Dispatches Discord Gateway events to registered handlers.
/// </summary>
public class DiscordEventDispatcher
{
    private readonly List<IMessageCreateHandler> _messageCreateHandlers = [];
    private readonly List<IMessageUpdateHandler> _messageUpdateHandlers = [];
    private readonly List<IMessageDeleteHandler> _messageDeleteHandlers = [];
    private readonly List<IGuildCreateHandler> _guildCreateHandlers = [];
    private readonly List<IGuildUpdateHandler> _guildUpdateHandlers = [];
    private readonly List<IGuildDeleteHandler> _guildDeleteHandlers = [];
    private readonly List<IChannelCreateHandler> _channelCreateHandlers = [];
    private readonly List<IChannelUpdateHandler> _channelUpdateHandlers = [];
    private readonly List<IChannelDeleteHandler> _channelDeleteHandlers = [];
    private readonly List<IMessageReactionAddHandler> _messageReactionAddHandlers = [];
    private readonly List<IMessageReactionRemoveHandler> _messageReactionRemoveHandlers = [];
    private readonly List<ITypingStartHandler> _typingStartHandlers = [];
    private readonly List<IInteractionCreateHandler> _interactionCreateHandlers = [];

    /// <summary>
    /// Registers a message create handler.
    /// </summary>
    public void Register(IMessageCreateHandler handler) => _messageCreateHandlers.Add(handler);

    /// <summary>
    /// Registers a message update handler.
    /// </summary>
    public void Register(IMessageUpdateHandler handler) => _messageUpdateHandlers.Add(handler);

    /// <summary>
    /// Registers a message delete handler.
    /// </summary>
    public void Register(IMessageDeleteHandler handler) => _messageDeleteHandlers.Add(handler);

    /// <summary>
    /// Registers a guild create handler.
    /// </summary>
    public void Register(IGuildCreateHandler handler) => _guildCreateHandlers.Add(handler);

    /// <summary>
    /// Registers a guild update handler.
    /// </summary>
    public void Register(IGuildUpdateHandler handler) => _guildUpdateHandlers.Add(handler);

    /// <summary>
    /// Registers a guild delete handler.
    /// </summary>
    public void Register(IGuildDeleteHandler handler) => _guildDeleteHandlers.Add(handler);

    /// <summary>
    /// Registers a channel create handler.
    /// </summary>
    public void Register(IChannelCreateHandler handler) => _channelCreateHandlers.Add(handler);

    /// <summary>
    /// Registers a channel update handler.
    /// </summary>
    public void Register(IChannelUpdateHandler handler) => _channelUpdateHandlers.Add(handler);

    /// <summary>
    /// Registers a channel delete handler.
    /// </summary>
    public void Register(IChannelDeleteHandler handler) => _channelDeleteHandlers.Add(handler);

    /// <summary>
    /// Registers a message reaction add handler.
    /// </summary>
    public void Register(IMessageReactionAddHandler handler) => _messageReactionAddHandlers.Add(handler);

    /// <summary>
    /// Registers a message reaction remove handler.
    /// </summary>
    public void Register(IMessageReactionRemoveHandler handler) => _messageReactionRemoveHandlers.Add(handler);

    /// <summary>
    /// Registers a typing start handler.
    /// </summary>
    public void Register(ITypingStartHandler handler) => _typingStartHandlers.Add(handler);

    /// <summary>
    /// Registers an interaction create handler.
    /// </summary>
    public void Register(IInteractionCreateHandler handler) => _interactionCreateHandlers.Add(handler);

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

            foreach (var handler in _messageCreateHandlers)
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
        foreach (var handler in _messageUpdateHandlers)
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

        foreach (var handler in _messageDeleteHandlers)
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessGuildCreateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var guild = payload.Deserialize<Guild>(jsonOptions);
        if (guild == null) return;

        var eventData = new GuildCreateEvent { Guild = guild };
        foreach (var handler in _guildCreateHandlers)
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessGuildUpdateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var guild = payload.Deserialize<Guild>(jsonOptions);
        if (guild == null) return;

        var eventData = new GuildUpdateEvent { Guild = guild };
        foreach (var handler in _guildUpdateHandlers)
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

        foreach (var handler in _guildDeleteHandlers)
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessChannelCreateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var channel = payload.Deserialize<Channel>(jsonOptions);
        if (channel == null) return;

        var eventData = new ChannelCreateEvent { Channel = channel };
        foreach (var handler in _channelCreateHandlers)
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessChannelUpdateAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var channel = payload.Deserialize<Channel>(jsonOptions);
        if (channel == null) return;

        var eventData = new ChannelUpdateEvent { Channel = channel };
        foreach (var handler in _channelUpdateHandlers)
        {
            await handler.HandleAsync(eventData);
        }
    }

    private async Task ProcessChannelDeleteAsync(JsonElement payload, JsonSerializerOptions jsonOptions)
    {
        var channel = payload.Deserialize<Channel>(jsonOptions);
        if (channel == null) return;

        var eventData = new ChannelDeleteEvent { Channel = channel };
        foreach (var handler in _channelDeleteHandlers)
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

        foreach (var handler in _messageReactionAddHandlers)
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

        foreach (var handler in _messageReactionRemoveHandlers)
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

        foreach (var handler in _typingStartHandlers)
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

            foreach (var handler in _interactionCreateHandlers)
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
        catch (Exception ex)
        {
        }
    }
}

