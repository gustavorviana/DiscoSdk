namespace DiscoSdk.Events;

/// <summary>
/// Base interface for Discord event handlers.
/// </summary>
public interface IDiscordEventHandler
{
}

/// <summary>
/// Interface for handling message creation events.
/// </summary>
public interface IMessageCreateHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a message creation event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(MessageCreateEvent eventData);
}

/// <summary>
/// Interface for handling message update events.
/// </summary>
public interface IMessageUpdateHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a message update event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(MessageUpdateEvent eventData);
}

/// <summary>
/// Interface for handling message deletion events.
/// </summary>
public interface IMessageDeleteHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a message deletion event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(MessageDeleteEvent eventData);
}

/// <summary>
/// Interface for handling guild creation events.
/// </summary>
public interface IGuildCreateHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a guild creation event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(GuildCreateEvent eventData);
}

/// <summary>
/// Interface for handling guild update events.
/// </summary>
public interface IGuildUpdateHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a guild update event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(GuildUpdateEvent eventData);
}

/// <summary>
/// Interface for handling guild deletion events.
/// </summary>
public interface IGuildDeleteHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a guild deletion event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(GuildDeleteEvent eventData);
}

/// <summary>
/// Interface for handling channel creation events.
/// </summary>
public interface IChannelCreateHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a channel creation event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(ChannelCreateEvent eventData);
}

/// <summary>
/// Interface for handling channel update events.
/// </summary>
public interface IChannelUpdateHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a channel update event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(ChannelUpdateEvent eventData);
}

/// <summary>
/// Interface for handling channel deletion events.
/// </summary>
public interface IChannelDeleteHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a channel deletion event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(ChannelDeleteEvent eventData);
}

/// <summary>
/// Interface for handling message reaction add events.
/// </summary>
public interface IMessageReactionAddHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a message reaction add event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(MessageReactionAddEvent eventData);
}

/// <summary>
/// Interface for handling message reaction remove events.
/// </summary>
public interface IMessageReactionRemoveHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a message reaction remove event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(MessageReactionRemoveEvent eventData);
}

/// <summary>
/// Interface for handling typing start events.
/// </summary>
public interface ITypingStartHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles a typing start event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TypingStartEvent eventData);
}

/// <summary>
/// Interface for handling interaction creation events (slash commands, buttons, etc.).
/// </summary>
public interface IInteractionCreateHandler : IDiscordEventHandler
{
    /// <summary>
    /// Handles an interaction creation event.
    /// </summary>
    /// <param name="eventData">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(IInteractionCreateEvent eventData);
}

