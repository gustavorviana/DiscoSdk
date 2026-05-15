using DiscoSdk.Models.AutoModeration;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord guild (server) with all its properties and available actions.
/// </summary>
/// <remarks>
/// All Discord IDs must be of type <see cref="Snowflake"/>.
/// All methods that perform server actions return <see cref="IRestAction"/> or <see cref="IRestAction{T}"/>.
/// </remarks>
public interface IGuild
{
    /// <summary>
    /// Gets the unique identifier of this guild.
    /// </summary>
    Snowflake Id { get; }

    /// <summary>
    /// Gets the name of this guild.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the icon of this guild, returned when in the template object.
    /// </summary>
    DiscordImageUrl? Icon { get; }

    /// <summary>
    /// Gets the splash of this guild, or null if no splash is set.
    /// </summary>
    DiscordImageUrl? Splash { get; }

    /// <summary>
    /// Gets the discovery splash of this guild, or null if no discovery splash is set.
    /// </summary>
    DiscordImageUrl? DiscoverySplash { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is the owner of this guild.
    /// </summary>
    bool? Owner { get; }

    /// <summary>
    /// Gets the ID of the owner of this guild.
    /// </summary>
    Snowflake? OwnerId { get; }

    /// <summary>
    /// Gets the permissions for the current user in this guild.
    /// </summary>
    DiscordPermission Permissions { get; }

    /// <summary>
    /// Gets the voice region ID for this guild, or null if not set.
    /// </summary>
    string? Region { get; }

    /// <summary>
    /// Gets the ID of the AFK channel, or null if no AFK channel is configured.
    /// </summary>
    Snowflake? AfkChannelId { get; }

    /// <summary>
    /// Gets the AFK timeout in seconds, or null if not set.
    /// </summary>
    int? AfkTimeout { get; }

    /// <summary>
    /// Gets a value indicating whether the guild widget is enabled.
    /// </summary>
    bool? WidgetEnabled { get; }

    /// <summary>
    /// Gets the ID of the channel used for the guild widget, or null if not set.
    /// </summary>
    Snowflake? WidgetChannelId { get; }

    /// <summary>
    /// Gets the verification level required for this guild.
    /// </summary>
    VerificationLevel? VerificationLevel { get; }

    /// <summary>
    /// Gets the default message notification level for this guild.
    /// </summary>
    DefaultMessageNotificationLevel? DefaultMessageNotifications { get; }

    /// <summary>
    /// Gets the explicit content filter level for this guild.
    /// </summary>
    ExplicitContentFilterLevel? ExplicitContentFilter { get; }

    /// <summary>
    /// Gets the roles in this guild, or null if not available.
    /// </summary>
    IRole[]? Roles { get; }

    /// <summary>
    /// Gets the emojis in this guild, or null if not available.
    /// </summary>
    IEmoji[]? Emojis { get; }

    /// <summary>
    /// Gets the enabled features of this guild, or null if not available.
    /// </summary>
    string[]? Features { get; }

    /// <summary>
    /// Gets the required MFA level for this guild.
    /// </summary>
    MfaLevel? MfaLevel { get; }

    /// <summary>
    /// Gets the application ID of the guild creator if it is bot-created, or null otherwise.
    /// </summary>
    Snowflake? ApplicationId { get; }

    /// <summary>
    /// Gets the ID of the system channel where system messages are sent, or null if not configured.
    /// </summary>
    Snowflake? SystemChannelId { get; }

    /// <summary>
    /// Gets the system channel flags that control which system messages are sent to the system channel.
    /// </summary>
    SystemChannelFlags? SystemChannelFlags { get; }

    /// <summary>
    /// Gets the ID of the rules channel, or null if not configured.
    /// </summary>
    Snowflake? RulesChannelId { get; }

    /// <summary>
    /// Gets the maximum number of presences for this guild, or null if not set.
    /// </summary>
    int? MaxPresences { get; }

    /// <summary>
    /// Gets the maximum number of members for this guild, or null if not set.
    /// </summary>
    int? MaxMembers { get; }

    /// <summary>
    /// Gets the vanity URL code for this guild, or null if not set.
    /// </summary>
    string? VanityUrlCode { get; }

    /// <summary>
    /// Gets the description of this guild, or null if not set.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the banner of this guild, or null if no banner is set.
    /// </summary>
    DiscordImageUrl? Banner { get; }

    /// <summary>
    /// Gets the premium tier (boost level) of this guild.
    /// </summary>
    PremiumTier? PremiumTier { get; }

    /// <summary>
    /// Gets the number of boosters this guild currently has.
    /// </summary>
    int? PremiumSubscriptionCount { get; }

    /// <summary>
    /// Gets the preferred locale of this guild.
    /// </summary>
    string? PreferredLocale { get; }

    /// <summary>
    /// Gets the ID of the channel where guild notices are posted, or null if not configured.
    /// </summary>
    Snowflake? PublicUpdatesChannelId { get; }

    /// <summary>
    /// Gets the maximum number of users in a video channel, or null if not set.
    /// </summary>
    int? MaxVideoChannelUsers { get; }

    /// <summary>
    /// Gets the approximate number of members in this guild, or null if not available.
    /// </summary>
    int? ApproximateMemberCount { get; }

    /// <summary>
    /// Gets the approximate number of online members in this guild, or null if not available.
    /// </summary>
    int? ApproximatePresenceCount { get; }

    /// <summary>
    /// Gets a value indicating whether this guild is unavailable (e.g., due to an outage).
    /// </summary>
    bool? Unavailable { get; }

    // Guild Actions

    /// <summary>
    /// Creates a REST action to edit this guild.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to edit the guild.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IEditGuildAction Edit();

    /// <summary>
    /// Gets a REST action for deleting this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to delete the guild.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction Delete();

    /// <summary>
    /// Gets a REST action for leaving this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to leave the guild.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction Leave();

    /// <summary>
    /// Creates a REST action to create a channel in this guild.
    /// </summary>
    /// <param name="name">The name of the channel.</param>
    /// <param name="type">The type of channel to create.</param>
    /// <returns>A REST action that can be configured and executed to create the channel.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    ICreateChannelAction CreateChannel(string name, ChannelType type);

    /// <summary>
    /// Creates a REST action to create a role in this guild.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to create the role.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRoleAction CreateRole();

    /// <summary>
    /// Creates a REST action to create an emoji in this guild.
    /// </summary>
    /// <param name="name">The name of the emoji.</param>
    /// <param name="image">The image data for the emoji.</param>
    /// <returns>A REST action that can be configured and executed to create the emoji.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    ICreateEmojiAction CreateEmoji(string name, DiscordImageBuffer image);

    /// <summary>
    /// Creates a REST action to ban a member from this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to ban.</param>
    /// <param name="deleteMessageDays">The number of days of messages to delete (0-7).</param>
    /// <returns>A REST action that can be configured and executed to ban the member.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IBanMemberAction BanMember(Snowflake userId, int deleteMessageDays = 0);

    /// <summary>
    /// Creates a REST action to unban a user from this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to unban.</param>
    /// <returns>A REST action that can be executed to unban the user.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction UnbanMember(Snowflake userId);

    /// <summary>
    /// Creates a REST action to kick a member from this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to kick.</param>
    /// <returns>A REST action that can be executed to kick the member.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction KickMember(Snowflake userId);

    /// <summary>
    /// Creates a REST action to get members of this guild with pagination.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to retrieve members.</returns>
    /// <remarks>
    /// Requires the privileged <see cref="DiscordIntent.GuildMembers"/> intent — Discord's
    /// <c>List Guild Members</c> endpoint refuses to return data without it. Executing the
    /// returned action throws <see cref="DiscoSdk.Exceptions.MissingIntentException"/> when
    /// the intent isn't enabled on the client.
    /// <para>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </para>
    /// </remarks>
    /// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
    /// Thrown when <see cref="DiscordIntent.GuildMembers"/> is not enabled on the client.
    /// </exception>
    IMemberPaginationAction GetMembers();

    /// <summary>
    /// Gets a REST action to retrieve a member by their user ID in this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <returns>A REST action that can be executed to retrieve the member.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// Returns null if the user is not a member of this guild.
    /// </remarks>
    IRestAction<IMember?> GetMember(Snowflake userId);

    /// <summary>
    /// Gets a REST action to retrieve a ban by user ID in this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve the ban for.</param>
    /// <returns>A REST action that can be executed to retrieve the ban.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// Returns null if the user is not banned from this guild.
    /// </remarks>
    IRestAction<IBan?> GetBan(Snowflake userId);

    /// <summary>
    /// Creates a REST action to get audit logs of this guild with pagination.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to retrieve audit logs.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IAuditLogPaginationAction GetAuditLogs();

    /// <summary>
    /// Gets a channel by its ID in this guild.
    /// </summary>
    /// <param name="channelId">The ID of the channel to retrieve.</param>
    /// <returns>The channel if found, or null if the channel does not exist in this guild.</returns>
    IGuildChannelUnion? GetChannel(Snowflake channelId);

    /// <summary>
    /// Gets the AFK channel of this guild.
    /// </summary>
    /// <returns>The AFK channel if configured, or null if no AFK channel is set.</returns>
    IGuildVoiceChannel? GetAfkChannel();

    /// <summary>
    /// Gets the system channel of this guild.
    /// </summary>
    /// <returns>The system channel if configured, or null if no system channel is set.</returns>
    IGuildTextChannel? GetSystemChannel();

    /// <summary>
    /// Gets the rules channel of this guild.
    /// </summary>
    /// <returns>The rules channel if configured, or null if no rules channel is set.</returns>
    IGuildTextChannel? GetRulesChannel();

    /// <summary>
    /// Gets the public updates channel of this guild.
    /// </summary>
    /// <returns>The public updates channel if configured, or null if no public updates channel is set.</returns>
    IGuildTextChannel? GetPublicUpdatesChannel();

    /// <summary>
    /// Gets all channels in this guild.
    /// </summary>
    /// <returns>A read-only list of all channels in this guild.</returns>
    IReadOnlyList<IGuildChannelUnion> GetChannels();

    /// <summary>
    /// Gets all text channels in this guild.
    /// </summary>
    /// <returns>A read-only list of all text channels in this guild.</returns>
    IReadOnlyList<IGuildTextChannel> GetTextChannels();

    /// <summary>
    /// Gets all voice channels in this guild.
    /// </summary>
    /// <returns>A read-only list of all voice channels in this guild.</returns>
    IReadOnlyList<IGuildVoiceChannel> GetVoiceChannels();

    /// <summary>
    /// Gets a REST action to retrieve all roles in this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve roles.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<IReadOnlyList<IRole>> GetRoles();

    /// <summary>
    /// Gets a REST action to retrieve all invites in this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve invites.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<IReadOnlyList<IInvite>> GetInvites();

    /// <summary>
    /// Creates a REST action to get the prune count for this guild.
    /// </summary>
    /// <param name="days">The number of days to count inactive members (1-30).</param>
    /// <param name="includeRoles">The role IDs to include in the prune count.</param>
    /// <returns>A REST action that can be executed to get the prune count.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<int> GetPruneCount(int days, params Snowflake[] includeRoles);

    /// <summary>
    /// Creates a REST action to begin a prune operation on this guild.
    /// </summary>
    /// <param name="days">The number of days to prune inactive members (1-30).</param>
    /// <param name="includeRoles">The role IDs to include in the prune.</param>
    /// <returns>A REST action that can be configured and executed to begin the prune.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<int> BeginPrune(int days, params Snowflake[] includeRoles);

    /// <summary>
    /// Gets a REST action to retrieve voice regions available for this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve voice regions.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<IReadOnlyList<VoiceRegion>> GetVoiceRegions();

    /// <summary>
    /// Gets a REST action to retrieve the preview of this guild (for public guilds).
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve the guild preview.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<GuildPreview> GetPreview();

    /// <summary>
    /// Gets a REST action to retrieve the widget of this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve the widget.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<GuildWidget> GetWidget();

    /// <summary>
    /// Creates a REST action to modify the widget of this guild.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to modify the widget.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IEditGuildWidgetAction EditWidget();

    /// <summary>
    /// Gets a REST action to retrieve the welcome screen of this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve the welcome screen.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<WelcomeScreen> GetWelcomeScreen();

    /// <summary>
    /// Creates a REST action to modify the welcome screen of this guild.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to modify the welcome screen.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IEditWelcomeScreenAction EditWelcomeScreen();

    /// <summary>
    /// Gets a REST action to retrieve the vanity URL of this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve the vanity URL code.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<VanityUrl?> GetVanityUrl();

    /// <summary>
    /// Gets a REST action to retrieve the widget image of this guild.
    /// </summary>
    /// <param name="style">The style of the widget image.</param>
    /// <returns>A REST action that can be executed to retrieve the widget image as a stream.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<Stream> GetWidgetImage(string? style = null);

    /// <summary>
    /// Gets a REST action that lists this guild's auto-moderation rules.
    /// </summary>
    IRestAction<IReadOnlyList<IAutoModerationRule>> GetAutoModerationRules();

    /// <summary>
    /// Gets a REST action that retrieves a single auto-moderation rule by ID.
    /// </summary>
    IRestAction<IAutoModerationRule> GetAutoModerationRule(Snowflake ruleId);

    /// <summary>
    /// Creates a REST action that adds a new auto-moderation rule to this guild. Configure the trigger
    /// metadata, actions, etc. on the returned action before executing it.
    /// </summary>
    ICreateAutoModerationRuleAction CreateAutoModerationRule(string name, AutoModerationEventType eventType, AutoModerationTriggerType triggerType);

    /// <summary>Lists this guild's scheduled events.</summary>
    /// <param name="withUserCount">If true, each event includes its <c>UserCount</c>.</param>
    IRestAction<IReadOnlyList<IGuildScheduledEvent>> GetScheduledEvents(bool? withUserCount = null);

    /// <summary>Gets a single scheduled event by id.</summary>
    IRestAction<IGuildScheduledEvent> GetScheduledEvent(Snowflake eventId, bool? withUserCount = null);

    /// <summary>
    /// Creates a scheduled event. Returns a fluent builder — for Stage/Voice events chain
    /// <c>SetChannel(...)</c>; for External events chain <c>SetLocation(...).SetScheduledEndTime(...)</c>.
    /// Finish with <c>ExecuteAsync</c>.
    /// </summary>
    /// <param name="name">Event name (1-100 chars).</param>
    /// <param name="scheduledStartTime">When the event starts.</param>
    /// <param name="entityType">Venue type (Stage / Voice / External).</param>
    Rest.Actions.ICreateScheduledEventAction CreateScheduledEvent(
        string name,
        DateTimeOffset scheduledStartTime,
        Enums.ScheduledEventEntityType entityType);

    /// <summary>Lists every sticker owned by this guild.</summary>
    IRestAction<IReadOnlyList<ISticker>> GetStickers();

    /// <summary>Gets a single guild-owned sticker by id.</summary>
    IRestAction<ISticker> GetSticker(Snowflake stickerId);

    /// <summary>
    /// Uploads a new sticker to this guild. Returns a fluent builder — chain
    /// <c>SetDescription(...)</c> if you want a description, then <c>ExecuteAsync</c>.
    /// </summary>
    /// <param name="name">Sticker name (2-30 chars).</param>
    /// <param name="tags">Suggestion / autocomplete tag string (max 200 chars).</param>
    /// <param name="file">Sticker image file (PNG/APNG/GIF/Lottie, max 512 KiB).</param>
    Rest.Actions.ICreateGuildStickerAction CreateSticker(string name, string tags, DiscoSdk.Models.Messages.MessageFile file);

    /// <summary>Builds a Request Guild Members gateway action.</summary>
    /// <remarks>
    /// Intent requirements depend on how the action is configured at terminal time:
    /// <list type="bullet">
    /// <item>Empty query (full member list) requires <see cref="DiscordIntent.GuildMembers"/>.</item>
    /// <item><see cref="Rest.Actions.IRequestGuildMembersAction.SetPresences(bool)"/> with <c>true</c> requires <see cref="DiscordIntent.GuildPresences"/>.</item>
    /// <item>A non-empty <see cref="Rest.Actions.IRequestGuildMembersAction.SetQuery(string)"/> or explicit <see cref="Rest.Actions.IRequestGuildMembersAction.SetUserIds(Snowflake[])"/> require no extra intent.</item>
    /// </list>
    /// Missing intents throw <see cref="DiscoSdk.Exceptions.MissingIntentException"/> when the
    /// terminal <c>GetAsync</c> or <c>StreamAsync</c> is invoked, before any payload is sent.
    /// </remarks>
    Rest.Actions.IRequestGuildMembersAction RequestMembers();

    /// <summary>
    /// Gets a REST action that retrieves this guild's onboarding configuration.
    /// </summary>
    IRestAction<IGuildOnboarding> GetOnboarding();

    /// <summary>
    /// Gets a REST action that lists the templates owned by this guild.
    /// </summary>
    IRestAction<IReadOnlyList<IGuildTemplate>> GetTemplates();

    /// <summary>
    /// Gets a REST action that creates a template from this guild's current configuration.
    /// </summary>
    IRestAction<IGuildTemplate> CreateTemplate(string name, string? description = null);

    /// <summary>
    /// Gets a paginated REST action that lists this guild's bans.
    /// </summary>
    IBanPaginationAction GetBans();

    /// <summary>
    /// Gets a REST action that bans multiple users at once (up to 200 in a single call). Returns the
    /// IDs of users that were successfully banned.
    /// </summary>
    /// <param name="userIds">The users to ban.</param>
    /// <param name="deleteMessageSeconds">If set, the number of seconds of recent message history to wipe (0 to 604 800 / 7 days).</param>
    IRestAction<IReadOnlyList<Snowflake>> BulkBan(IEnumerable<Snowflake> userIds, int? deleteMessageSeconds = null);

    /// <summary>
    /// Gets a REST action that searches the guild's member list by username/nickname prefix.
    /// </summary>
    /// <param name="query">The username/nickname prefix to match.</param>
    /// <param name="limit">Maximum number of members to return (1–1000). Defaults to 1.</param>
    IRestAction<IReadOnlyList<IMember>> SearchMembers(string query, int? limit = null);

    /// <summary>
    /// Builds a REST action that adds a user to this guild using an OAuth2 access token granted with
    /// the <c>guilds.join</c> scope. Configure optional fields (nickname, roles, mute, deaf) on the
    /// returned builder, then call <see cref="IRestAction{T}.ExecuteAsync"/>. Returns <c>null</c> if
    /// the user was already in the guild.
    /// </summary>
    IAddMemberAction AddMember(Snowflake userId, string accessToken);

    /// <summary>
    /// Gets a REST action that updates the bot's own nickname in this guild. Pass <c>null</c> to clear it.
    /// </summary>
    IRestAction<IMember> ModifyCurrentMember(string? nick);

    /// <summary>
    /// Builds a REST action that modifies a guild member. Each setter on the builder corresponds to
    /// one Discord field — only the fields you set are sent. Use <see cref="IModifyMemberAction.Timeout"/>
    /// / <see cref="IModifyMemberAction.ClearTimeout"/> instead of passing a sentinel timestamp.
    /// </summary>
    /// <param name="userId">The member to modify.</param>
    IModifyMemberAction ModifyMember(Snowflake userId);

    /// <summary>
    /// Gets a REST action that adds a role to a guild member.
    /// </summary>
    IRestAction AddMemberRole(Snowflake userId, Snowflake roleId);

    /// <summary>
    /// Gets a REST action that removes a role from a guild member.
    /// </summary>
    IRestAction RemoveMemberRole(Snowflake userId, Snowflake roleId);

    /// <summary>
    /// Gets a REST action that updates the required MFA level for this guild. The caller must be the guild owner.
    /// </summary>
    IRestAction ModifyMfaLevel(MfaLevel level);

    /// <summary>
    /// Gets a REST action that reorders channels in this guild. Each item specifies a channel ID, its
    /// new position and optionally a new parent / lock_permissions flag.
    /// </summary>
    /// <param name="positions">The set of channel-position updates to apply.</param>
    IRestAction ModifyChannelPositions(IEnumerable<ChannelPosition> positions);

    /// <summary>
    /// Gets a REST action that lists this guild's integrations (Twitch / YouTube subs, Discord bots, etc.).
    /// </summary>
    IRestAction<IReadOnlyList<IIntegration>> GetIntegrations();

    /// <summary>
    /// Gets a REST action that suspends invites and/or DMs for this guild until the supplied
    /// timestamps. Pass <c>null</c> to clear either suspension.
    /// </summary>
    IRestAction<IncidentsData> ModifyIncidentActions(DateTimeOffset? invitesDisabledUntil, DateTimeOffset? dmsDisabledUntil);

    /// <summary>
    /// Gets a REST action that lists all webhooks attached to channels in this guild.
    /// </summary>
    IRestAction<IReadOnlyList<IWebhook>> GetWebhooks();

    /// <summary>
    /// Application-command operations scoped to this guild (currently the per-guild
    /// command-permission endpoints).
    /// </summary>
    IGuildCommands Commands { get; }
}