using DiscoSdk.Contexts.Interactions;

namespace DiscoSdk.Events;

public interface IDiscordEventHandler
{

}

/// <summary>
/// Base interface for Discord event handlers.
/// </summary>
public interface IDiscordEventHandler<TEventData> : IDiscordEventHandler
{
    Task HandleAsync(TEventData eventData);
}

/// <summary>
/// Interface for handling message creation events.
/// </summary>
public interface IMessageCreateHandler : IDiscordEventHandler<MessageCreateEvent>
{
}

/// <summary>
/// Interface for handling message update events.
/// </summary>
public interface IMessageUpdateHandler : IDiscordEventHandler<MessageUpdateEvent>
{
}

/// <summary>
/// Interface for handling message deletion events.
/// </summary>
public interface IMessageDeleteHandler : IDiscordEventHandler<MessageDeleteEvent>
{
}

/// <summary>
/// Interface for handling guild creation events.
/// </summary>
public interface IGuildCreateHandler : IDiscordEventHandler<GuildCreateEvent>
{
}

/// <summary>
/// Interface for handling guild update events.
/// </summary>
public interface IGuildUpdateHandler : IDiscordEventHandler<GuildUpdateEvent>
{
}

/// <summary>
/// Interface for handling guild deletion events.
/// </summary>
public interface IGuildDeleteHandler : IDiscordEventHandler<GuildDeleteEvent>
{
}

/// <summary>
/// Interface for handling channel creation events.
/// </summary>
public interface IChannelCreateHandler : IDiscordEventHandler<ChannelCreateEvent>
{
}

/// <summary>
/// Interface for handling channel update events.
/// </summary>
public interface IChannelUpdateHandler : IDiscordEventHandler<ChannelUpdateEvent>
{
}

/// <summary>
/// Interface for handling channel deletion events.
/// </summary>
public interface IChannelDeleteHandler : IDiscordEventHandler<ChannelDeleteEvent>
{
}

/// <summary>
/// Interface for handling message reaction add events.
/// </summary>
public interface IMessageReactionAddHandler : IDiscordEventHandler<MessageReactionAddEvent>
{
}

/// <summary>
/// Interface for handling message reaction remove events.
/// </summary>
public interface IMessageReactionRemoveHandler : IDiscordEventHandler<MessageReactionRemoveEvent>
{
}

/// <summary>
/// Interface for handling typing start events.
/// </summary>
public interface ITypingStartHandler : IDiscordEventHandler<TypingStartEvent>
{
}

/// <summary>
/// Interface for handling interaction creation events (slash commands, buttons, etc.).
/// </summary>
public interface IInteractionCreateHandler : IDiscordEventHandler<IInteractionContext>
{
}

/// <summary>
/// Interface for handling application command interactions (slash commands only).
/// This handler is only called when interaction.Type == InteractionType.ApplicationCommand.
/// </summary>
public interface IApplicationCommandHandler : IDiscordEventHandler<ICommandContext>
{
}

/// <summary>
/// Interface for handling modal submission interactions.
/// This handler is only called when interaction.Type == InteractionType.ModalSubmit.
/// </summary>
public interface IModalSubmitHandler : IDiscordEventHandler<IInteractionContext>
{
}

/// <summary>
/// Interface for handling component interactions (button clicks, select menus, etc.).
/// This handler is only called when interaction.Type == InteractionType.MessageComponent.
/// </summary>
public interface IComponentInteractionHandler : IDiscordEventHandler<IInteractionContext>
{
}

