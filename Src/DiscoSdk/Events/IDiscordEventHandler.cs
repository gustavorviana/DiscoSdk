using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Channels;
using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Contexts.Messages;

namespace DiscoSdk.Events;

public interface IDiscordEventHandler
{

}

/// <summary>
/// Base interface for Discord event handlers.
/// </summary>
public interface IDiscordEventHandler<TContext> : IDiscordEventHandler
{
    Task HandleAsync(TContext context, IServiceProvider services);
}

/// <summary>
/// Interface for handling message creation events.
/// </summary>
public interface IMessageCreateHandler : IDiscordEventHandler<IMessageCreateContext>
{
}

/// <summary>
/// Interface for handling message update events.
/// </summary>
public interface IMessageUpdateHandler : IDiscordEventHandler<IMessageUpdateContext>
{
}

/// <summary>
/// Interface for handling message deletion events.
/// </summary>
public interface IMessageDeleteHandler : IDiscordEventHandler<IMessageDeleteContext>
{
}

/// <summary>
/// Interface for handling guild creation events.
/// </summary>
public interface IGuildCreateHandler : IDiscordEventHandler<IGuildContext>
{
}

/// <summary>
/// Interface for handling guild update events.
/// </summary>
public interface IGuildUpdateHandler : IDiscordEventHandler<IGuildContext>
{
}

/// <summary>
/// Interface for handling guild deletion events.
/// </summary>
public interface IGuildDeleteHandler : IDiscordEventHandler<IGuildDeleteContext>
{
}

/// <summary>
/// Interface for handling channel creation events.
/// </summary>
public interface IChannelCreateHandler : IDiscordEventHandler<IChannelContext>
{
}

/// <summary>
/// Interface for handling channel update events.
/// </summary>
public interface IChannelUpdateHandler : IDiscordEventHandler<IChannelContext>
{
}

/// <summary>
/// Interface for handling channel deletion events.
/// </summary>
public interface IChannelDeleteHandler : IDiscordEventHandler<IChannelDeleteContext>
{
}

/// <summary>
/// Interface for handling message reaction add events.
/// </summary>
public interface IMessageReactionAddHandler : IDiscordEventHandler<IMessageAddReactionContext>
{
}

/// <summary>
/// Interface for handling message reaction remove events.
/// </summary>
public interface IMessageReactionRemoveHandler : IDiscordEventHandler<IMessageDeleteReactionContext>
{
}

/// <summary>
/// Interface for handling typing start events.
/// </summary>
public interface ITypingStartHandler : IDiscordEventHandler<ITypingContext>
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
public interface IModalSubmitHandler : IDiscordEventHandler<IModalContext>
{
}

/// <summary>
/// Interface for handling component interactions (button clicks, select menus, etc.).
/// This handler is only called when interaction.Type == InteractionType.MessageComponent.
/// </summary>
public interface IComponentInteractionHandler : IDiscordEventHandler<IInteractionContext>
{
}

/// <summary>
/// Interface for handling application command autocomplete interactions.
/// This handler is only called when interaction.Type == InteractionType.ApplicationCommandAutocomplete.
/// </summary>
public interface IAutocompleteHandler : IDiscordEventHandler<IAutocompleteContext>
{
}

/// <summary>
/// Interface for handling user context menu command interactions.
/// This handler is only called when interaction.Data.Type == ApplicationCommandType.User.
/// </summary>
public interface IUserCommandHandler : IDiscordEventHandler<IUserCommandContext>
{
}

/// <summary>
/// Interface for handling message context menu command interactions.
/// This handler is only called when interaction.Data.Type == ApplicationCommandType.Message.
/// </summary>
public interface IMessageCommandHandler : IDiscordEventHandler<IMessageCommandContext>
{
}

/// <summary>
/// Interface for handling <c>GUILD_MEMBER_ADD</c> events — a user joined a guild.
/// </summary>
public interface IGuildMemberAddHandler : IDiscordEventHandler<IGuildMemberAddContext>
{
}

/// <summary>
/// Interface for handling <c>GUILD_MEMBER_REMOVE</c> events — a user left, was kicked, or banned.
/// </summary>
public interface IGuildMemberRemoveHandler : IDiscordEventHandler<IGuildMemberRemoveContext>
{
}

/// <summary>
/// Interface for handling <c>GUILD_MEMBER_UPDATE</c> events — a member's roles/nickname/timeout/etc. changed.
/// </summary>
public interface IGuildMemberUpdateHandler : IDiscordEventHandler<IGuildMemberUpdateContext>
{
}

/// <summary>Interface for handling <c>GUILD_ROLE_CREATE</c> events.</summary>
public interface IGuildRoleCreateHandler : IDiscordEventHandler<IGuildRoleContext>
{
}

/// <summary>Interface for handling <c>GUILD_ROLE_UPDATE</c> events.</summary>
public interface IGuildRoleUpdateHandler : IDiscordEventHandler<IGuildRoleContext>
{
}

/// <summary>Interface for handling <c>GUILD_ROLE_DELETE</c> events.</summary>
public interface IGuildRoleDeleteHandler : IDiscordEventHandler<IGuildRoleDeleteContext>
{
}

/// <summary>Interface for handling <c>GUILD_BAN_ADD</c> events.</summary>
public interface IGuildBanAddHandler : IDiscordEventHandler<IGuildBanContext>
{
}

/// <summary>Interface for handling <c>GUILD_BAN_REMOVE</c> events.</summary>
public interface IGuildBanRemoveHandler : IDiscordEventHandler<IGuildBanContext>
{
}

/// <summary>Interface for handling <c>MESSAGE_DELETE_BULK</c> events.</summary>
public interface IMessageDeleteBulkHandler : IDiscordEventHandler<IMessageDeleteBulkContext>
{
}

/// <summary>Interface for handling <c>MESSAGE_REACTION_REMOVE_ALL</c> events.</summary>
public interface IMessageReactionRemoveAllHandler : IDiscordEventHandler<IMessageReactionRemoveAllContext>
{
}

/// <summary>Interface for handling <c>MESSAGE_REACTION_REMOVE_EMOJI</c> events.</summary>
public interface IMessageReactionRemoveEmojiHandler : IDiscordEventHandler<IMessageReactionRemoveEmojiContext>
{
}

/// <summary>Interface for handling <c>USER_UPDATE</c> events — the bot's own user object changed.</summary>
public interface IUserUpdateHandler : IDiscordEventHandler<IUserUpdateContext>
{
}

/// <summary>Interface for handling <c>CHANNEL_PINS_UPDATE</c> events.</summary>
public interface IChannelPinsUpdateHandler : IDiscordEventHandler<IChannelPinsUpdateContext>
{
}

/// <summary>Interface for handling <c>WEBHOOKS_UPDATE</c> events.</summary>
public interface IWebhooksUpdateHandler : IDiscordEventHandler<IWebhooksUpdateContext>
{
}

/// <summary>Interface for handling <c>INVITE_CREATE</c> events.</summary>
public interface IInviteCreateHandler : IDiscordEventHandler<IInviteCreateContext>
{
}

/// <summary>Interface for handling <c>INVITE_DELETE</c> events.</summary>
public interface IInviteDeleteHandler : IDiscordEventHandler<IInviteDeleteContext>
{
}

/// <summary>Interface for handling <c>THREAD_CREATE</c> events.</summary>
public interface IThreadCreateHandler : IDiscordEventHandler<IThreadContext>
{
}

/// <summary>Interface for handling <c>THREAD_UPDATE</c> events.</summary>
public interface IThreadUpdateHandler : IDiscordEventHandler<IThreadContext>
{
}

/// <summary>Interface for handling <c>THREAD_DELETE</c> events.</summary>
public interface IThreadDeleteHandler : IDiscordEventHandler<IThreadDeleteContext>
{
}

/// <summary>Interface for handling <c>THREAD_LIST_SYNC</c> events.</summary>
public interface IThreadListSyncHandler : IDiscordEventHandler<IThreadListSyncContext>
{
}

/// <summary>Interface for handling <c>THREAD_MEMBER_UPDATE</c> events.</summary>
public interface IThreadMemberUpdateHandler : IDiscordEventHandler<IThreadMemberUpdateContext>
{
}

/// <summary>Interface for handling <c>THREAD_MEMBERS_UPDATE</c> events.</summary>
public interface IThreadMembersUpdateHandler : IDiscordEventHandler<IThreadMembersUpdateContext>
{
}

/// <summary>Interface for handling <c>STAGE_INSTANCE_CREATE</c> events.</summary>
public interface IStageInstanceCreateHandler : IDiscordEventHandler<IStageInstanceContext>
{
}

/// <summary>Interface for handling <c>STAGE_INSTANCE_UPDATE</c> events.</summary>
public interface IStageInstanceUpdateHandler : IDiscordEventHandler<IStageInstanceContext>
{
}

/// <summary>Interface for handling <c>STAGE_INSTANCE_DELETE</c> events.</summary>
public interface IStageInstanceDeleteHandler : IDiscordEventHandler<IStageInstanceContext>
{
}

/// <summary>Interface for handling <c>AUTO_MODERATION_RULE_CREATE</c> events.</summary>
public interface IAutoModerationRuleCreateHandler : IDiscordEventHandler<IAutoModerationRuleContext>
{
}

/// <summary>Interface for handling <c>AUTO_MODERATION_RULE_UPDATE</c> events.</summary>
public interface IAutoModerationRuleUpdateHandler : IDiscordEventHandler<IAutoModerationRuleContext>
{
}

/// <summary>Interface for handling <c>AUTO_MODERATION_RULE_DELETE</c> events.</summary>
public interface IAutoModerationRuleDeleteHandler : IDiscordEventHandler<IAutoModerationRuleContext>
{
}

/// <summary>Interface for handling <c>AUTO_MODERATION_ACTION_EXECUTION</c> events.</summary>
public interface IAutoModerationActionExecutionHandler : IDiscordEventHandler<IAutoModerationActionExecutionContext>
{
}

/// <summary>Interface for handling <c>INTEGRATION_CREATE</c> events.</summary>
public interface IIntegrationCreateHandler : IDiscordEventHandler<IIntegrationContext>
{
}

/// <summary>Interface for handling <c>INTEGRATION_UPDATE</c> events.</summary>
public interface IIntegrationUpdateHandler : IDiscordEventHandler<IIntegrationContext>
{
}

/// <summary>Interface for handling <c>INTEGRATION_DELETE</c> events.</summary>
public interface IIntegrationDeleteHandler : IDiscordEventHandler<IIntegrationDeleteContext>
{
}

/// <summary>Interface for handling <c>GUILD_SCHEDULED_EVENT_CREATE</c> events.</summary>
public interface IGuildScheduledEventCreateHandler : IDiscordEventHandler<IGuildScheduledEventContext>
{
}

/// <summary>Interface for handling <c>GUILD_SCHEDULED_EVENT_UPDATE</c> events.</summary>
public interface IGuildScheduledEventUpdateHandler : IDiscordEventHandler<IGuildScheduledEventContext>
{
}

/// <summary>Interface for handling <c>GUILD_SCHEDULED_EVENT_DELETE</c> events.</summary>
public interface IGuildScheduledEventDeleteHandler : IDiscordEventHandler<IGuildScheduledEventContext>
{
}

/// <summary>Interface for handling <c>GUILD_SCHEDULED_EVENT_USER_ADD</c> events.</summary>
public interface IGuildScheduledEventUserAddHandler : IDiscordEventHandler<IGuildScheduledEventUserContext>
{
}

/// <summary>Interface for handling <c>GUILD_SCHEDULED_EVENT_USER_REMOVE</c> events.</summary>
public interface IGuildScheduledEventUserRemoveHandler : IDiscordEventHandler<IGuildScheduledEventUserContext>
{
}

/// <summary>Interface for handling <c>ENTITLEMENT_CREATE</c> events.</summary>
public interface IEntitlementCreateHandler : IDiscordEventHandler<IEntitlementContext>
{
}

/// <summary>Interface for handling <c>ENTITLEMENT_UPDATE</c> events.</summary>
public interface IEntitlementUpdateHandler : IDiscordEventHandler<IEntitlementContext>
{
}

/// <summary>Interface for handling <c>ENTITLEMENT_DELETE</c> events.</summary>
public interface IEntitlementDeleteHandler : IDiscordEventHandler<IEntitlementContext>
{
}

/// <summary>Interface for handling <c>GUILD_AUDIT_LOG_ENTRY_CREATE</c> events.</summary>
public interface IAuditLogEntryCreateHandler : IDiscordEventHandler<IAuditLogEntryCreateContext>
{
}

/// <summary>Interface for handling <c>GUILD_EMOJIS_UPDATE</c> events.</summary>
public interface IGuildEmojisUpdateHandler : IDiscordEventHandler<IGuildEmojisUpdateContext>
{
}

/// <summary>Interface for handling <c>GUILD_STICKERS_UPDATE</c> events.</summary>
public interface IGuildStickersUpdateHandler : IDiscordEventHandler<IGuildStickersUpdateContext>
{
}

/// <summary>Interface for handling <c>GUILD_INTEGRATIONS_UPDATE</c> events.</summary>
public interface IGuildIntegrationsUpdateHandler : IDiscordEventHandler<IGuildIntegrationsUpdateContext>
{
}

/// <summary>Interface for handling <c>PRESENCE_UPDATE</c> events.</summary>
public interface IPresenceUpdateHandler : IDiscordEventHandler<IPresenceUpdateContext>
{
}

/// <summary>Interface for handling <c>GUILD_MEMBERS_CHUNK</c> events.</summary>
public interface IGuildMembersChunkHandler : IDiscordEventHandler<IGuildMembersChunkContext>
{
}

/// <summary>Interface for handling <c>MESSAGE_POLL_VOTE_ADD</c> events.</summary>
public interface IMessagePollVoteAddHandler : IDiscordEventHandler<IMessagePollVoteContext>
{
}

/// <summary>Interface for handling <c>MESSAGE_POLL_VOTE_REMOVE</c> events.</summary>
public interface IMessagePollVoteRemoveHandler : IDiscordEventHandler<IMessagePollVoteContext>
{
}

/// <summary>Interface for handling <c>SUBSCRIPTION_CREATE</c> events.</summary>
public interface ISubscriptionCreateHandler : IDiscordEventHandler<ISubscriptionContext>
{
}

/// <summary>Interface for handling <c>SUBSCRIPTION_UPDATE</c> events.</summary>
public interface ISubscriptionUpdateHandler : IDiscordEventHandler<ISubscriptionContext>
{
}

/// <summary>Interface for handling <c>SUBSCRIPTION_DELETE</c> events.</summary>
public interface ISubscriptionDeleteHandler : IDiscordEventHandler<ISubscriptionContext>
{
}