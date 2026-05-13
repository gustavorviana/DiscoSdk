using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Channels;
using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Contexts.Messages;
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
using DiscoSdk.Models;
using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Models.Presences;
using DiscoSdk.Models.Users;
using DiscoSdk.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway.Events;

/// <summary>
/// Dispatches Discord Gateway events to registered handlers.
/// </summary>
internal class DiscordEventDispatcher
{
    private readonly Dictionary<Type, HashSet<int>> _handlerIndicesByType = [];
    private readonly List<IDiscordEventHandler> _handlers = [];
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

    public DiscordEventDispatcher AddAll(IEnumerable<IDiscordEventHandler> handlers)
    {
        foreach (var handler in handlers)
            Add(handler);

        return this;
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
            if (_handlers.Contains(handler))
                return;

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

                case "GUILD_MEMBER_ADD":
                    await ProcessGuildMemberAddAsync(payload);
                    break;

                case "GUILD_MEMBER_REMOVE":
                    await ProcessGuildMemberRemoveAsync(payload);
                    break;

                case "GUILD_MEMBER_UPDATE":
                    await ProcessGuildMemberUpdateAsync(payload);
                    break;

                case "GUILD_ROLE_CREATE":
                    await ProcessGuildRoleCreateAsync(payload);
                    break;

                case "GUILD_ROLE_UPDATE":
                    await ProcessGuildRoleUpdateAsync(payload);
                    break;

                case "GUILD_ROLE_DELETE":
                    await ProcessGuildRoleDeleteAsync(payload);
                    break;

                case "GUILD_BAN_ADD":
                    await ProcessGuildBanAddAsync(payload);
                    break;

                case "GUILD_BAN_REMOVE":
                    await ProcessGuildBanRemoveAsync(payload);
                    break;

                case "MESSAGE_DELETE_BULK":
                    await ProcessMessageDeleteBulkAsync(new JsonElementParser(payload));
                    break;

                case "MESSAGE_REACTION_REMOVE_ALL":
                    await ProcessMessageReactionRemoveAllAsync(new JsonElementParser(payload));
                    break;

                case "MESSAGE_REACTION_REMOVE_EMOJI":
                    await ProcessMessageReactionRemoveEmojiAsync(new JsonElementParser(payload));
                    break;

                case "USER_UPDATE":
                    await ProcessUserUpdateAsync(payload);
                    break;

                case "CHANNEL_PINS_UPDATE":
                    await ProcessChannelPinsUpdateAsync(new JsonElementParser(payload));
                    break;

                case "WEBHOOKS_UPDATE":
                    await ProcessWebhooksUpdateAsync(new JsonElementParser(payload));
                    break;

                case "INVITE_CREATE":
                    await ProcessInviteCreateAsync(new JsonElementParser(payload));
                    break;

                case "INVITE_DELETE":
                    await ProcessInviteDeleteAsync(new JsonElementParser(payload));
                    break;

                case "THREAD_CREATE":
                    await ProcessThreadCreateOrUpdateAsync<IThreadCreateHandler>(payload);
                    break;

                case "THREAD_UPDATE":
                    await ProcessThreadCreateOrUpdateAsync<IThreadUpdateHandler>(payload);
                    break;

                case "THREAD_DELETE":
                    await ProcessThreadDeleteAsync(payload);
                    break;

                case "THREAD_LIST_SYNC":
                    await ProcessThreadListSyncAsync(new JsonElementParser(payload));
                    break;

                case "THREAD_MEMBER_UPDATE":
                    await ProcessThreadMemberUpdateAsync(new JsonElementParser(payload));
                    break;

                case "THREAD_MEMBERS_UPDATE":
                    await ProcessThreadMembersUpdateAsync(new JsonElementParser(payload));
                    break;

                case "STAGE_INSTANCE_CREATE":
                    await ProcessStageInstanceAsync<IStageInstanceCreateHandler>(payload);
                    break;

                case "STAGE_INSTANCE_UPDATE":
                    await ProcessStageInstanceAsync<IStageInstanceUpdateHandler>(payload);
                    break;

                case "STAGE_INSTANCE_DELETE":
                    await ProcessStageInstanceAsync<IStageInstanceDeleteHandler>(payload);
                    break;

                case "AUTO_MODERATION_RULE_CREATE":
                    await ProcessAutoModerationRuleAsync<IAutoModerationRuleCreateHandler>(payload);
                    break;

                case "AUTO_MODERATION_RULE_UPDATE":
                    await ProcessAutoModerationRuleAsync<IAutoModerationRuleUpdateHandler>(payload);
                    break;

                case "AUTO_MODERATION_RULE_DELETE":
                    await ProcessAutoModerationRuleAsync<IAutoModerationRuleDeleteHandler>(payload);
                    break;

                case "AUTO_MODERATION_ACTION_EXECUTION":
                    await ProcessAutoModerationActionExecutionAsync(new JsonElementParser(payload));
                    break;

                case "INTEGRATION_CREATE":
                    await ProcessIntegrationAsync<IIntegrationCreateHandler>(payload);
                    break;

                case "INTEGRATION_UPDATE":
                    await ProcessIntegrationAsync<IIntegrationUpdateHandler>(payload);
                    break;

                case "INTEGRATION_DELETE":
                    await ProcessIntegrationDeleteAsync(new JsonElementParser(payload));
                    break;

                case "GUILD_SCHEDULED_EVENT_CREATE":
                    await ProcessGuildScheduledEventAsync<IGuildScheduledEventCreateHandler>(payload);
                    break;

                case "GUILD_SCHEDULED_EVENT_UPDATE":
                    await ProcessGuildScheduledEventAsync<IGuildScheduledEventUpdateHandler>(payload);
                    break;

                case "GUILD_SCHEDULED_EVENT_DELETE":
                    await ProcessGuildScheduledEventAsync<IGuildScheduledEventDeleteHandler>(payload);
                    break;

                case "GUILD_SCHEDULED_EVENT_USER_ADD":
                    await ProcessGuildScheduledEventUserAsync<IGuildScheduledEventUserAddHandler>(payload);
                    break;

                case "GUILD_SCHEDULED_EVENT_USER_REMOVE":
                    await ProcessGuildScheduledEventUserAsync<IGuildScheduledEventUserRemoveHandler>(payload);
                    break;

                case "ENTITLEMENT_CREATE":
                    await ProcessEntitlementAsync<IEntitlementCreateHandler>(payload);
                    break;

                case "ENTITLEMENT_UPDATE":
                    await ProcessEntitlementAsync<IEntitlementUpdateHandler>(payload);
                    break;

                case "ENTITLEMENT_DELETE":
                    await ProcessEntitlementAsync<IEntitlementDeleteHandler>(payload);
                    break;

                case "GUILD_AUDIT_LOG_ENTRY_CREATE":
                    await ProcessAuditLogEntryCreateAsync(payload);
                    break;

                case "GUILD_EMOJIS_UPDATE":
                    await ProcessGuildEmojisUpdateAsync(new JsonElementParser(payload));
                    break;

                case "GUILD_STICKERS_UPDATE":
                    await ProcessGuildStickersUpdateAsync(new JsonElementParser(payload));
                    break;

                case "GUILD_INTEGRATIONS_UPDATE":
                    await ProcessGuildIntegrationsUpdateAsync(new JsonElementParser(payload));
                    break;

                case "PRESENCE_UPDATE":
                    await ProcessPresenceUpdateAsync(payload);
                    break;

                case "GUILD_MEMBERS_CHUNK":
                    await ProcessGuildMembersChunkAsync(new JsonElementParser(payload));
                    break;

                case "MESSAGE_POLL_VOTE_ADD":
                    await ProcessMessagePollVoteAsync<IMessagePollVoteAddHandler>(payload);
                    break;

                case "MESSAGE_POLL_VOTE_REMOVE":
                    await ProcessMessagePollVoteAsync<IMessagePollVoteRemoveHandler>(payload);
                    break;

                case "SUBSCRIPTION_CREATE":
                    await ProcessSubscriptionAsync<ISubscriptionCreateHandler>(payload);
                    break;

                case "SUBSCRIPTION_UPDATE":
                    await ProcessSubscriptionAsync<ISubscriptionUpdateHandler>(payload);
                    break;

                case "SUBSCRIPTION_DELETE":
                    await ProcessSubscriptionAsync<ISubscriptionDeleteHandler>(payload);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw to prevent breaking the event loop
            _discordClient.Logger.Log(LogLevel.Error, ex, "Error processing event {EventType}", message.EventType);
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

        await HandleAllAsync<IMessageCreateHandler, IMessageCreateContext>(eventData);
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
        await HandleAllAsync<IMessageUpdateHandler, IMessageUpdateContext>(eventData);
    }

    private async Task ProcessMessageDeleteAsync(JsonElementParser payload)
    {
        var id = payload.GetSnowflake("id")!.Value;
        var channelId = payload.GetSnowflake("channel_id")!.Value;
        var guildId = payload.GetSnowflake("guild_id");
        var wrappedChannel = _discordClient.Channels.GetWrappedTextChannel(channelId);
        var wrappedGuild = _discordClient.Guilds.GetWrapped(guildId);

        var eventData = new MessageDeleteContextWrapper(_discordClient, id, wrappedChannel!, wrappedGuild);
        await HandleAllAsync<IMessageDeleteHandler, IMessageDeleteContext>(eventData);
    }

    private async Task ProcessGuildCreateAsync(JsonElement payload)
    {
        var guild = payload.Deserialize<Guild>(_discordClient.SerializerOptions);
        if (_discordClient.Guilds.HandleGuildCreate(guild) is not IGuild wrappedGuild)
            return;

        var eventData = new GuildContextWrapper(_discordClient, wrappedGuild);
        await HandleAllAsync<IGuildCreateHandler, IGuildContext>(eventData);
    }

    private async Task ProcessGuildUpdateAsync(JsonElement payload)
    {
        var guild = payload.Deserialize<Guild>(_discordClient.SerializerOptions);
        if (_discordClient.Guilds.HandleGuildUpdate(guild) is not IGuild wrappedGuild) return;

        var eventData = new GuildContextWrapper(_discordClient, wrappedGuild);
        await HandleAllAsync<IGuildUpdateHandler, IGuildContext>(eventData);
    }

    private async Task ProcessGuildDeleteAsync(JsonElement payload)
    {
        var eventData = new GuildDeleteContextWrapper(_discordClient, payload);
        _discordClient.Guilds.HandleGuildDelete(eventData.Id);

        await HandleAllAsync<IGuildDeleteHandler, IGuildDeleteContext>(eventData);
    }

    private async Task ProcessChannelCreateAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (!TryGetChannelGuild(channel, out var guild)) return;

        _discordClient.Guilds.HandleChannelCreate(channel!);

        var eventData = new ChannelContext(_discordClient, new GuildChannelUnionWrapper(_discordClient, channel!, guild!));
        await HandleAllAsync<IChannelCreateHandler, IChannelContext>(eventData);
    }

    private async Task ProcessChannelUpdateAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (!TryGetChannelGuild(channel, out var guild)) return;

        _discordClient.Guilds.HandleChannelUpdate(channel!);

        var eventData = new ChannelContext(_discordClient, new GuildChannelUnionWrapper(_discordClient, channel!, guild!));
        await HandleAllAsync<IChannelUpdateHandler, IChannelContext>(eventData);
    }

    private async Task ProcessChannelDeleteAsync(JsonElement payload)
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (!TryGetChannelGuild(channel, out var guild)) return;

        _discordClient.Guilds.HandleChannelDelete(channel);
        var eventData = new ChannelDeleteContext(_discordClient, guild!, channel.Id);
        await HandleAllAsync<IChannelDeleteHandler, IChannelDeleteContext>(eventData);
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

        await HandleAllAsync<IMessageReactionAddHandler, IMessageAddReactionContext>(eventData);
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

        await HandleAllAsync<IMessageReactionRemoveHandler, IMessageDeleteReactionContext>(eventData);
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

        await HandleAllAsync<ITypingStartHandler, ITypingContext>(eventData);
    }

    private async Task<IUser?> GetUserAsync(GuildMember? member, Snowflake userId)
    {
        if (member?.User is not null)
            return new UserWrapper(_discordClient, member.User);

        return await _discordClient.Users.Get(userId).ExecuteAsync();
    }

    private async Task ProcessGuildMemberAddAsync(JsonElement payload)
    {
        var member = payload.Deserialize<GuildMember>(_discordClient.SerializerOptions);
        if (member is null) return;

        var guildId = new JsonElementParser(payload).GetSnowflake("guild_id");
        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var wrappedMember = new GuildMemberWrapper(_discordClient, member, guild);
        var eventData = new GuildMemberAddContextWrapper(_discordClient, wrappedMember, guild);
        await HandleAllAsync<IGuildMemberAddHandler, IGuildMemberAddContext>(eventData);
    }

    private async Task ProcessGuildMemberRemoveAsync(JsonElement payload)
    {
        var parser = new JsonElementParser(payload);
        var guildId = parser.GetSnowflake("guild_id");
        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var user = parser.Deserialize<User>("user", _discordClient.SerializerOptions);
        if (user is null) return;

        var wrappedUser = new UserWrapper(_discordClient, user);
        var eventData = new GuildMemberRemoveContextWrapper(_discordClient, wrappedUser, guild);
        await HandleAllAsync<IGuildMemberRemoveHandler, IGuildMemberRemoveContext>(eventData);
    }

    private async Task ProcessGuildMemberUpdateAsync(JsonElement payload)
    {
        var member = payload.Deserialize<GuildMember>(_discordClient.SerializerOptions);
        if (member is null) return;

        var guildId = new JsonElementParser(payload).GetSnowflake("guild_id");
        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var wrappedMember = new GuildMemberWrapper(_discordClient, member, guild);
        var eventData = new GuildMemberUpdateContextWrapper(_discordClient, wrappedMember, guild);
        await HandleAllAsync<IGuildMemberUpdateHandler, IGuildMemberUpdateContext>(eventData);
    }

    private Task ProcessGuildRoleCreateAsync(JsonElement payload)
        => ProcessGuildRoleCreateOrUpdateAsync<IGuildRoleCreateHandler>(payload);

    private Task ProcessGuildRoleUpdateAsync(JsonElement payload)
        => ProcessGuildRoleCreateOrUpdateAsync<IGuildRoleUpdateHandler>(payload);

    private async Task ProcessGuildRoleCreateOrUpdateAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IGuildRoleContext>
    {
        var parser = new JsonElementParser(payload);
        var guild = _discordClient.Guilds.GetWrapped(parser.GetSnowflake("guild_id"));
        if (guild is null) return;

        var role = parser.Deserialize<Role>("role", _discordClient.SerializerOptions);
        if (role is null) return;

        var wrappedRole = new RoleWrapper(_discordClient, role, guild);
        var eventData = new GuildRoleContextWrapper(_discordClient, wrappedRole, guild);
        await HandleAllAsync<THandler, IGuildRoleContext>(eventData);
    }

    private async Task ProcessGuildRoleDeleteAsync(JsonElement payload)
    {
        var parser = new JsonElementParser(payload);
        var guild = _discordClient.Guilds.GetWrapped(parser.GetSnowflake("guild_id"));
        if (guild is null) return;

        var roleId = parser.GetSnowflake("role_id");
        if (roleId is null) return;

        var eventData = new GuildRoleDeleteContextWrapper(_discordClient, roleId.Value, guild);
        await HandleAllAsync<IGuildRoleDeleteHandler, IGuildRoleDeleteContext>(eventData);
    }

    private Task ProcessGuildBanAddAsync(JsonElement payload)
        => ProcessGuildBanChangeAsync<IGuildBanAddHandler>(payload);

    private Task ProcessGuildBanRemoveAsync(JsonElement payload)
        => ProcessGuildBanChangeAsync<IGuildBanRemoveHandler>(payload);

    private async Task ProcessGuildBanChangeAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IGuildBanContext>
    {
        var parser = new JsonElementParser(payload);
        var guild = _discordClient.Guilds.GetWrapped(parser.GetSnowflake("guild_id"));
        if (guild is null) return;

        var user = parser.Deserialize<User>("user", _discordClient.SerializerOptions);
        if (user is null) return;

        var eventData = new GuildBanContextWrapper(_discordClient, new UserWrapper(_discordClient, user), guild);
        await HandleAllAsync<THandler, IGuildBanContext>(eventData);
    }

    private async Task ProcessMessageDeleteBulkAsync(JsonElementParser payload)
    {
        var channelId = payload.GetSnowflake("channel_id");
        if (channelId is null) return;

        var idsElement = payload.Get("ids");
        if (idsElement is null) return;

        var ids = ImmutableArray.CreateBuilder<Snowflake>();
        foreach (var item in idsElement.Value.EnumerateArray())
            if (Snowflake.TryParse(item.GetString(), out var id))
                ids.Add(id);

        var channel = _discordClient.Channels.GetWrappedTextChannel(channelId.Value);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));

        var eventData = new MessageDeleteBulkContextWrapper(_discordClient, ids.ToImmutable(), channel!, guild);
        await HandleAllAsync<IMessageDeleteBulkHandler, IMessageDeleteBulkContext>(eventData);
    }

    private async Task ProcessMessageReactionRemoveAllAsync(JsonElementParser payload)
    {
        var channelId = payload.GetSnowflake("channel_id");
        var messageId = payload.GetSnowflake("message_id");
        if (channelId is null || messageId is null) return;

        var channel = _discordClient.Channels.GetWrappedTextChannel(channelId.Value);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));

        var eventData = new MessageReactionRemoveAllContextWrapper(_discordClient, messageId.Value, channel!, guild);
        await HandleAllAsync<IMessageReactionRemoveAllHandler, IMessageReactionRemoveAllContext>(eventData);
    }

    private async Task ProcessMessageReactionRemoveEmojiAsync(JsonElementParser payload)
    {
        var channelId = payload.GetSnowflake("channel_id");
        var messageId = payload.GetSnowflake("message_id");
        if (channelId is null || messageId is null) return;

        var channel = _discordClient.Channels.GetWrappedTextChannel(channelId.Value);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        var emoji = payload.Deserialize<Emoji>("emoji", _discordClient.SerializerOptions);
        if (emoji is null) return;

        var eventData = new MessageReactionRemoveEmojiContextWrapper(
            _discordClient, messageId.Value, new EmojiWrapper(_discordClient, emoji, guild), channel!, guild);
        await HandleAllAsync<IMessageReactionRemoveEmojiHandler, IMessageReactionRemoveEmojiContext>(eventData);
    }

    private async Task ProcessUserUpdateAsync(JsonElement payload)
    {
        var user = payload.Deserialize<User>(_discordClient.SerializerOptions);
        if (user is null) return;

        var eventData = new UserUpdateContextWrapper(_discordClient, new UserWrapper(_discordClient, user));
        await HandleAllAsync<IUserUpdateHandler, IUserUpdateContext>(eventData);
    }

    private async Task ProcessChannelPinsUpdateAsync(JsonElementParser payload)
    {
        var channelId = payload.GetSnowflake("channel_id");
        if (channelId is null) return;

        var channel = _discordClient.Channels.GetWrappedTextChannel(channelId.Value);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        var lastPin = payload.GetDateTimeOffset("last_pin_timestamp");

        var eventData = new ChannelPinsUpdateContextWrapper(_discordClient, channel!, guild, lastPin);
        await HandleAllAsync<IChannelPinsUpdateHandler, IChannelPinsUpdateContext>(eventData);
    }

    private async Task ProcessWebhooksUpdateAsync(JsonElementParser payload)
    {
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (guild is null) return;

        var channelId = payload.GetSnowflake("channel_id");
        if (channelId is null) return;

        var eventData = new WebhooksUpdateContextWrapper(_discordClient, guild, channelId.Value);
        await HandleAllAsync<IWebhooksUpdateHandler, IWebhooksUpdateContext>(eventData);
    }

    private async Task ProcessInviteCreateAsync(JsonElementParser payload)
    {
        var code = payload.GetString("code");
        var channelId = payload.GetSnowflake("channel_id");
        if (code is null || channelId is null) return;

        var channel = _discordClient.Channels.GetWrappedTextChannel(channelId.Value);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        var inviter = payload.Deserialize<User>("inviter", _discordClient.SerializerOptions);
        var wrappedInviter = inviter is not null ? new UserWrapper(_discordClient, inviter) : null;
        var createdAt = payload.GetDateTimeOffset("created_at") ?? default;
        var maxAge = payload.Get("max_age")?.GetInt32() ?? 0;
        var maxUses = payload.Get("max_uses")?.GetInt32() ?? 0;
        var temporary = payload.Get("temporary")?.GetBoolean() ?? false;

        var eventData = new InviteCreateContextWrapper(_discordClient, code, channel!, guild, wrappedInviter,
            createdAt, maxAge, maxUses, temporary);
        await HandleAllAsync<IInviteCreateHandler, IInviteCreateContext>(eventData);
    }

    private async Task ProcessInviteDeleteAsync(JsonElementParser payload)
    {
        var code = payload.GetString("code");
        var channelId = payload.GetSnowflake("channel_id");
        if (code is null || channelId is null) return;

        var channel = _discordClient.Channels.GetWrappedTextChannel(channelId.Value);
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));

        var eventData = new InviteDeleteContextWrapper(_discordClient, code, channel!, guild);
        await HandleAllAsync<IInviteDeleteHandler, IInviteDeleteContext>(eventData);
    }

    private async Task ProcessThreadCreateOrUpdateAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IThreadContext>
    {
        var channel = payload.Deserialize<Channel>(_discordClient.SerializerOptions);
        if (channel is null || channel.GuildId is null) return;

        var guild = _discordClient.Guilds.GetWrapped(channel.GuildId.Value);
        if (guild is null) return;

        var thread = new GuildThreadChannelWrapper(_discordClient, channel, guild);
        var eventData = new ThreadContextWrapper(_discordClient, thread, guild);
        await HandleAllAsync<THandler, IThreadContext>(eventData);
    }

    private async Task ProcessThreadDeleteAsync(JsonElement payload)
    {
        var parser = new JsonElementParser(payload);
        var threadId = parser.GetSnowflake("id");
        var guildId = parser.GetSnowflake("guild_id");
        if (threadId is null || guildId is null) return;

        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var parentId = parser.GetSnowflake("parent_id");
        var eventData = new ThreadDeleteContextWrapper(_discordClient, threadId.Value, parentId, guild);
        await HandleAllAsync<IThreadDeleteHandler, IThreadDeleteContext>(eventData);
    }

    private async Task ProcessThreadListSyncAsync(JsonElementParser payload)
    {
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (guild is null) return;

        var threadsEl = payload.Get("threads");
        if (threadsEl is null) return;

        var threads = ImmutableArray.CreateBuilder<IGuildThreadChannel>();
        foreach (var item in threadsEl.Value.EnumerateArray())
        {
            var channel = item.Deserialize<Channel>(_discordClient.SerializerOptions);
            if (channel is null) continue;
            threads.Add(new GuildThreadChannelWrapper(_discordClient, channel, guild));
        }

        var eventData = new ThreadListSyncContextWrapper(_discordClient, guild, threads.ToImmutable());
        await HandleAllAsync<IThreadListSyncHandler, IThreadListSyncContext>(eventData);
    }

    private async Task ProcessThreadMemberUpdateAsync(JsonElementParser payload)
    {
        var threadId = payload.GetSnowflake("id");
        var userId = payload.GetSnowflake("user_id");
        var guildId = payload.GetSnowflake("guild_id");
        if (threadId is null || userId is null || guildId is null) return;

        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var eventData = new ThreadMemberUpdateContextWrapper(_discordClient, threadId.Value, userId.Value, guild);
        await HandleAllAsync<IThreadMemberUpdateHandler, IThreadMemberUpdateContext>(eventData);
    }

    private async Task ProcessStageInstanceAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IStageInstanceContext>
    {
        var instance = payload.Deserialize<StageInstance>(_discordClient.SerializerOptions);
        if (instance is null) return;

        var guild = _discordClient.Guilds.GetWrapped(instance.GuildId);
        if (guild is null) return;

        var wrapped = new StageInstanceWrapper(_discordClient, instance);
        var eventData = new StageInstanceContextWrapper(_discordClient, wrapped, guild);
        await HandleAllAsync<THandler, IStageInstanceContext>(eventData);
    }

    private async Task ProcessAutoModerationRuleAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IAutoModerationRuleContext>
    {
        var rule = payload.Deserialize<AutoModerationRule>(_discordClient.SerializerOptions);
        if (rule is null) return;

        var guild = _discordClient.Guilds.GetWrapped(rule.GuildId);
        if (guild is null) return;

        var eventData = new AutoModerationRuleContextWrapper(_discordClient, rule, guild);
        await HandleAllAsync<THandler, IAutoModerationRuleContext>(eventData);
    }

    private async Task ProcessIntegrationAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IIntegrationContext>
    {
        var model = payload.Deserialize<Integration>(_discordClient.SerializerOptions);
        if (model is null) return;

        var guildId = new JsonElementParser(payload).GetSnowflake("guild_id");
        if (guildId is null) return;

        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var integration = new IntegrationWrapper(_discordClient, guildId.Value, model);
        var eventData = new IntegrationContextWrapper(_discordClient, integration, guild);
        await HandleAllAsync<THandler, IIntegrationContext>(eventData);
    }

    private async Task ProcessIntegrationDeleteAsync(JsonElementParser payload)
    {
        var integrationId = payload.GetSnowflake("id");
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (integrationId is null || guild is null) return;

        var applicationId = payload.GetSnowflake("application_id");
        var eventData = new IntegrationDeleteContextWrapper(_discordClient, integrationId.Value, guild, applicationId);
        await HandleAllAsync<IIntegrationDeleteHandler, IIntegrationDeleteContext>(eventData);
    }

    private async Task ProcessGuildScheduledEventAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IGuildScheduledEventContext>
    {
        var scheduledEvent = payload.Deserialize<GuildScheduledEvent>(_discordClient.SerializerOptions);
        if (scheduledEvent is null) return;

        var guild = _discordClient.Guilds.GetWrapped(scheduledEvent.GuildId);
        if (guild is null) return;

        var wrapped = new GuildScheduledEventWrapper(_discordClient, scheduledEvent);
        var eventData = new GuildScheduledEventContextWrapper(_discordClient, wrapped, guild);
        await HandleAllAsync<THandler, IGuildScheduledEventContext>(eventData);
    }

    private async Task ProcessGuildScheduledEventUserAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IGuildScheduledEventUserContext>
    {
        var parser = new JsonElementParser(payload);
        var scheduledEventId = parser.GetSnowflake("guild_scheduled_event_id");
        var userId = parser.GetSnowflake("user_id");
        var guild = _discordClient.Guilds.GetWrapped(parser.GetSnowflake("guild_id"));
        if (scheduledEventId is null || userId is null || guild is null) return;

        var eventData = new GuildScheduledEventUserContextWrapper(
            _discordClient, scheduledEventId.Value, userId.Value, guild);
        await HandleAllAsync<THandler, IGuildScheduledEventUserContext>(eventData);
    }

    private async Task ProcessEntitlementAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IEntitlementContext>
    {
        var entitlement = payload.Deserialize<Entitlement>(_discordClient.SerializerOptions);
        if (entitlement is null) return;

        var eventData = new EntitlementContextWrapper(_discordClient, entitlement);
        await HandleAllAsync<THandler, IEntitlementContext>(eventData);
    }

    private async Task ProcessAuditLogEntryCreateAsync(JsonElement payload)
    {
        var entry = payload.Deserialize<AuditLogEntry>(_discordClient.SerializerOptions);
        if (entry is null) return;

        var guildId = new JsonElementParser(payload).GetSnowflake("guild_id");
        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var eventData = new AuditLogEntryCreateContextWrapper(_discordClient, entry, guild);
        await HandleAllAsync<IAuditLogEntryCreateHandler, IAuditLogEntryCreateContext>(eventData);
    }

    private async Task ProcessGuildEmojisUpdateAsync(JsonElementParser payload)
    {
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (guild is null) return;

        var emojisEl = payload.Get("emojis");
        var emojis = ImmutableArray.CreateBuilder<Emoji>();
        if (emojisEl is not null)
            foreach (var item in emojisEl.Value.EnumerateArray())
            {
                var emoji = item.Deserialize<Emoji>(_discordClient.SerializerOptions);
                if (emoji is not null) emojis.Add(emoji);
            }

        var eventData = new GuildEmojisUpdateContextWrapper(_discordClient, guild, emojis.ToImmutable());
        await HandleAllAsync<IGuildEmojisUpdateHandler, IGuildEmojisUpdateContext>(eventData);
    }

    private async Task ProcessGuildStickersUpdateAsync(JsonElementParser payload)
    {
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (guild is null) return;

        var stickersEl = payload.Get("stickers");
        var stickers = ImmutableArray.CreateBuilder<Sticker>();
        if (stickersEl is not null)
            foreach (var item in stickersEl.Value.EnumerateArray())
            {
                var sticker = item.Deserialize<Sticker>(_discordClient.SerializerOptions);
                if (sticker is not null) stickers.Add(sticker);
            }

        var eventData = new GuildStickersUpdateContextWrapper(_discordClient, guild, stickers.ToImmutable());
        await HandleAllAsync<IGuildStickersUpdateHandler, IGuildStickersUpdateContext>(eventData);
    }

    private async Task ProcessGuildIntegrationsUpdateAsync(JsonElementParser payload)
    {
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (guild is null) return;

        var eventData = new GuildIntegrationsUpdateContextWrapper(_discordClient, guild);
        await HandleAllAsync<IGuildIntegrationsUpdateHandler, IGuildIntegrationsUpdateContext>(eventData);
    }

    private async Task ProcessPresenceUpdateAsync(JsonElement payload)
    {
        var presence = payload.Deserialize<Presence>(_discordClient.SerializerOptions);
        if (presence is null) return;

        var guildId = new JsonElementParser(payload).GetSnowflake("guild_id");
        var guild = _discordClient.Guilds.GetWrapped(guildId);

        var eventData = new PresenceUpdateContextWrapper(_discordClient, presence, guild);
        await HandleAllAsync<IPresenceUpdateHandler, IPresenceUpdateContext>(eventData);
    }

    private async Task ProcessMessagePollVoteAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<IMessagePollVoteContext>
    {
        var parser = new JsonElementParser(payload);
        var userId = parser.GetSnowflake("user_id");
        var messageId = parser.GetSnowflake("message_id");
        var channelId = parser.GetSnowflake("channel_id");
        if (userId is null || messageId is null || channelId is null) return;

        var answerId = parser.Get("answer_id")?.GetInt32() ?? 0;
        var channel = _discordClient.Channels.GetWrappedTextChannel(channelId.Value);
        var guild = _discordClient.Guilds.GetWrapped(parser.GetSnowflake("guild_id"));

        var eventData = new MessagePollVoteContextWrapper(
            _discordClient, userId.Value, messageId.Value, answerId, channel!, guild);
        await HandleAllAsync<THandler, IMessagePollVoteContext>(eventData);
    }

    private async Task ProcessSubscriptionAsync<THandler>(JsonElement payload)
        where THandler : IDiscordEventHandler<ISubscriptionContext>
    {
        var subscription = payload.Deserialize<Subscription>(_discordClient.SerializerOptions);
        if (subscription is null) return;

        var eventData = new SubscriptionContextWrapper(_discordClient, subscription);
        await HandleAllAsync<THandler, ISubscriptionContext>(eventData);
    }

    private async Task ProcessGuildMembersChunkAsync(JsonElementParser payload)
    {
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (guild is null) return;

        var membersEl = payload.Get("members");
        var members = ImmutableArray.CreateBuilder<IMember>();
        if (membersEl is not null)
            foreach (var item in membersEl.Value.EnumerateArray())
            {
                var member = item.Deserialize<GuildMember>(_discordClient.SerializerOptions);
                if (member is not null)
                    members.Add(new GuildMemberWrapper(_discordClient, member, guild));
            }

        var chunkIndex = payload.Get("chunk_index")?.GetInt32() ?? 0;
        var chunkCount = payload.Get("chunk_count")?.GetInt32() ?? 1;
        var nonce = payload.Get("nonce")?.GetString();

        var eventData = new GuildMembersChunkContextWrapper(
            _discordClient, guild, members.ToImmutable(), chunkIndex, chunkCount, nonce);
        await HandleAllAsync<IGuildMembersChunkHandler, IGuildMembersChunkContext>(eventData);
    }

    private async Task ProcessAutoModerationActionExecutionAsync(JsonElementParser payload)
    {
        var guild = _discordClient.Guilds.GetWrapped(payload.GetSnowflake("guild_id"));
        if (guild is null) return;

        var ruleId = payload.GetSnowflake("rule_id");
        var userId = payload.GetSnowflake("user_id");
        if (ruleId is null || userId is null) return;

        var action = payload.Deserialize<AutoModerationAction>("action", _discordClient.SerializerOptions);
        if (action is null) return;

        var channelId = payload.GetSnowflake("channel_id");
        var matched = payload.Get("matched_content")?.GetString();

        var eventData = new AutoModerationActionExecutionContextWrapper(
            _discordClient, guild, ruleId.Value, action, userId.Value, channelId, matched);
        await HandleAllAsync<IAutoModerationActionExecutionHandler, IAutoModerationActionExecutionContext>(eventData);
    }

    private async Task ProcessThreadMembersUpdateAsync(JsonElementParser payload)
    {
        var threadId = payload.GetSnowflake("id");
        var guildId = payload.GetSnowflake("guild_id");
        if (threadId is null || guildId is null) return;

        var guild = _discordClient.Guilds.GetWrapped(guildId);
        if (guild is null) return;

        var memberCount = payload.Get("member_count")?.GetInt32() ?? 0;

        var added = ImmutableArray.CreateBuilder<Snowflake>();
        if (payload.Get("added_members") is { } addedEl)
            foreach (var item in addedEl.EnumerateArray())
                if (item.TryGetProperty("user_id", out var uidEl) && Snowflake.TryParse(uidEl.GetString(), out var uid))
                    added.Add(uid);

        var removed = ImmutableArray.CreateBuilder<Snowflake>();
        if (payload.Get("removed_member_ids") is { } removedEl)
            foreach (var item in removedEl.EnumerateArray())
                if (Snowflake.TryParse(item.GetString(), out var uid))
                    removed.Add(uid);

        var eventData = new ThreadMembersUpdateContextWrapper(_discordClient,
            threadId.Value, guild, memberCount, added.ToImmutable(), removed.ToImmutable());
        await HandleAllAsync<IThreadMembersUpdateHandler, IThreadMembersUpdateContext>(eventData);
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
            var interactionContext = GetInteractionContext(interactionWrapper);

            try
            {
                using var scoped = _discordClient.Services.CreateAsyncScope();
                await ConfigureScopeAsync(scoped.ServiceProvider, interactionContext);

                if (interaction.Type == InteractionType.ApplicationCommandAutocomplete)
                    await HandleAllAsync<IAutocompleteHandler, IAutocompleteContext>(
                        interactionWrapper.Handle,
                        scoped.ServiceProvider,
                        (IAutocompleteContext)interactionContext);

                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    if (interaction.Data?.Type == ApplicationCommandType.User)
                        await HandleAllAsync<IUserCommandHandler, IUserCommandContext>(
                            interactionWrapper.Handle,
                            scoped.ServiceProvider,
                            (IUserCommandContext)interactionContext);
                    else if (interaction.Data?.Type == ApplicationCommandType.Message)
                        await HandleAllAsync<IMessageCommandHandler, IMessageCommandContext>(
                            interactionWrapper.Handle,
                            scoped.ServiceProvider,
                            (IMessageCommandContext)interactionContext);
                    else
                        await HandleAllAsync<IApplicationCommandHandler, ICommandContext>(
                            interactionWrapper.Handle,
                            scoped.ServiceProvider,
                            (ICommandContext)interactionContext);
                }

                if (interaction.Type == InteractionType.ModalSubmit)
                    await HandleAllAsync<IModalSubmitHandler, IModalContext>(
                        interactionWrapper.Handle,
                        scoped.ServiceProvider,
                        (IModalContext)interactionContext);

                if (interaction.Type == InteractionType.MessageComponent)
                    await HandleAllAsync<IComponentInteractionHandler, IInteractionContext>(
                        interactionWrapper.Handle,
                        scoped.ServiceProvider,
                        interactionContext);

                await HandleAllAsync<IInteractionCreateHandler, IInteractionContext>(
                    interactionWrapper.Handle,
                    scoped.ServiceProvider,
                    interactionContext);
            }
            catch (Exception ex)
            {
                _discordClient.Logger.Log(LogLevel.Error, ex, "Error processing handlers for interaction {InteractionId}", interaction.Id);
            }
        }
        catch (Exception ex)
        {
            _discordClient.Logger.Log(LogLevel.Error, ex, "Error processing INTERACTION_CREATE");
        }
    }

    internal InteractionContextWrapper GetInteractionContext(InteractionWrapper interactionWrapper)
    {
        if (interactionWrapper.Type == InteractionType.ApplicationCommandAutocomplete)
            return new AutocompleteContext(_discordClient, interactionWrapper);

        if (interactionWrapper.Type == InteractionType.ApplicationCommand)
        {
            var commandType = interactionWrapper.RawInteraction.Data?.Type;
            if (commandType == ApplicationCommandType.User)
                return new UserCommandContext(_discordClient, interactionWrapper);
            if (commandType == ApplicationCommandType.Message)
                return new MessageCommandContext(_discordClient, interactionWrapper);
            return new CommandContext(_discordClient, interactionWrapper);
        }

        if (interactionWrapper.Type == InteractionType.ModalSubmit)
            return new ModalContext(_discordClient, interactionWrapper);

        return new InteractionContextWrapper(_discordClient, interactionWrapper);
    }

    private async Task HandleAllAsync<THandler, TContext>(TContext context)
        where THandler : IDiscordEventHandler<TContext>
        where TContext : IContext
    {
        foreach (var handler in GetHandlersOfType<THandler>())
            await SafeRunHandlerAsync(handler, context);
    }

    private async Task HandleAllAsync<THandler, TContext>(InteractionHandle handle, IServiceProvider service, TContext context)
        where THandler : IDiscordEventHandler<TContext>
        where TContext : IContext
    {
        if (handle.Responded)
            return;

        foreach (var handler in GetHandlersOfType<THandler>())
            if (!handle.Responded)
                await SafeRunHandlerAsync(handler, context);
    }

    private async Task SafeRunHandlerAsync<THandler, TContext>(THandler handler, TContext context)
        where THandler : IDiscordEventHandler<TContext>
        where TContext : IContext
    {
        try
        {
            using var scoped = _discordClient.Services.CreateAsyncScope();
            await ConfigureScopeAsync(scoped.ServiceProvider, context);
            await handler.HandleAsync(context, scoped.ServiceProvider);
        }
        catch (Exception ex)
        {
            _discordClient.Logger.Log(LogLevel.Error, ex, "Error in {HandlerType}", typeof(THandler).Name);
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

    private async Task ConfigureScopeAsync(IServiceProvider provider, IContext context)
    {
        provider
            .GetRequiredService<SdkContextProvider>()
            .SetContext(context);

        var modules = _discordClient.Modules.OfType<IDependencyScopeDiscoModule>();
        foreach (var module in modules)
            await module.OnScopeCreatedAsync(context, provider);
    }
}