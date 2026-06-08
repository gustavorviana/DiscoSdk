using DiscoSdk.Caching;
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
    bool Owner { get; }

    /// <summary>
    /// ID of the owner of this guild. Always present in payloads Discord delivers; a default
    /// <see cref="Snowflake"/> only surfaces when the wrapper is constructed from an incomplete
    /// stub (e.g. a deleted/unavailable guild).
    /// </summary>
    Snowflake OwnerId { get; }

    /// <summary>
    /// Gets the permissions for the current user in this guild.
    /// </summary>
    DiscordPermission Permissions { get; }

    /// <summary>
    /// Gets the voice region ID for this guild, or null if not set.
    /// </summary>
    string? Region { get; }

    /// <summary>Verification level required for this guild. Defaults to <see cref="VerificationLevel.None"/>.</summary>
    VerificationLevel VerificationLevel { get; }

    /// <summary>Default message-notification level. Defaults to <see cref="DefaultMessageNotificationLevel.AllMessages"/>.</summary>
    DefaultMessageNotificationLevel DefaultMessageNotifications { get; }

    /// <summary>Explicit-content filter level. Defaults to <see cref="ExplicitContentFilterLevel.Disabled"/>.</summary>
    ExplicitContentFilterLevel ExplicitContentFilter { get; }


    /// <summary>
    /// Enabled features of this guild. Empty when none are enabled.
    /// </summary>
    string[] Features { get; }

    /// <summary>Required MFA level for moderation actions. Defaults to <see cref="MfaLevel.None"/>.</summary>
    MfaLevel MfaLevel { get; }

    /// <summary>
    /// Gets the application ID of the guild creator if it is bot-created, or null otherwise.
    /// </summary>
    Snowflake? ApplicationId { get; }

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

    /// <summary>Premium (boost) tier of this guild. Defaults to <see cref="PremiumTier.None"/>.</summary>
    PremiumTier PremiumTier { get; }

    /// <summary>
    /// Gets the number of boosters this guild currently has.
    /// </summary>
    int? PremiumSubscriptionCount { get; }

    /// <summary>Preferred locale of this guild. Defaults to <c>"en-US"</c> when Discord omits it.</summary>
    string PreferredLocale { get; }

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
    /// Whether this guild is unavailable (e.g., due to an outage). Defaults to <c>false</c>.
    /// </summary>
    bool Unavailable { get; }

    /// <summary>
    /// Gets the hub type of this guild (Student Hub program), or null if this guild is not a hub.
    /// </summary>
    GuildHubType? HubType { get; }

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
    IReasonedRestAction Delete();

    /// <summary>
    /// Gets a REST action for leaving this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to leave the guild.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction Leave();

    /// <summary>
    /// Channel surface for this guild — every operation on <c>/guilds/:id/channels</c>
    /// (<c>Create</c>, <c>Get</c>, <c>GetAll</c>, <c>GetText</c>, <c>GetVoice</c>, <c>GetAfk</c>,
    /// <c>GetSystem</c>, <c>GetRules</c>, <c>GetPublicUpdates</c>, <c>ModifyPositions</c>,
    /// <c>ListActiveThreads</c>).
    /// </summary>
    IGuildChannels Channels { get; }

    /// <summary>
    /// Role surface for this guild — every operation on <c>/guilds/:id/roles</c>
    /// (<c>Create</c>, <c>Get</c>, <c>GetAll</c>, <c>ModifyPositions</c>).
    /// </summary>
    IGuildRoles Roles { get; }

    /// <summary>
    /// Emoji surface for this guild — every operation on <c>/guilds/:id/emojis*</c>
    /// (<c>Create</c>, <c>GetCached</c>, <c>GetCachedCount</c>).
    /// </summary>
    IGuildEmojis Emojis { get; }

    /// <summary>
    /// Member surface for this guild — cache reads, REST builders, gateway Request Guild Members
    /// (op 8), and the member-resource mutations (<c>Add</c>, <c>Modify</c>, <c>ModifyCurrent</c>,
    /// <c>AddRole</c>, <c>RemoveRole</c>, <c>Kick</c>). Pre-bound to this guild id; wraps the
    /// cross-guild <see cref="IDiscordClient.Members"/> manager. For banning use
    /// <see cref="Bans"/>.
    /// </summary>
    IGuildMembers Members { get; }

    /// <summary>
    /// Ban surface for this guild — every operation on <c>/guilds/:id/bans/*</c>
    /// (<c>Get</c>, <c>List</c>, <c>Ban</c>, <c>Unban</c>, <c>BulkBan</c>).
    /// </summary>
    IGuildBans Bans { get; }

    /// <summary>Builds a deferred REST action that retrieves the audit logs of this guild.</summary>
    IAuditLogPaginationAction GetAuditLogs();

    /// <summary>
    /// Scheduled event surface for this guild — every operation on
    /// <c>/guilds/:id/scheduled-events*</c> (<c>Get</c>, <c>GetAll</c>, <c>Create</c>).
    /// </summary>
    IGuildScheduledEvents ScheduledEvents { get; }

    /// <summary>
    /// Auto-moderation surface for this guild — every operation on
    /// <c>/guilds/:id/auto-moderation/rules*</c> (<c>Get</c>, <c>GetAll</c>, <c>Create</c>).
    /// </summary>
    IGuildAutoModeration AutoModeration { get; }

    /// <summary>
    /// Widget surface for this guild — every operation on <c>/guilds/:id/widget*</c>
    /// (<c>Get</c>, <c>Edit</c>, <c>GetImage</c>).
    /// </summary>
    IGuildWidgetSurface Widget { get; }

    /// <summary>
    /// Welcome-screen surface for this guild — every operation on
    /// <c>/guilds/:id/welcome-screen*</c> (<c>Get</c>, <c>Edit</c>).
    /// </summary>
    IGuildWelcomeScreen WelcomeScreen { get; }

    /// <summary>
    /// Onboarding surface for this guild — every operation on <c>/guilds/:id/onboarding*</c>
    /// (<c>Get</c>, <c>Edit</c>).
    /// </summary>
    IGuildOnboardingSurface Onboarding { get; }

    /// <summary>
    /// Template surface for this guild — every operation on <c>/guilds/:id/templates*</c>
    /// (<c>GetAll</c>, <c>Create</c>).
    /// </summary>
    IGuildTemplates Templates { get; }

    /// <summary>
    /// Prune surface for this guild — every operation on <c>/guilds/:id/prune*</c>
    /// (<c>Count</c>, <c>Begin</c>).
    /// </summary>
    IGuildPrune Prune { get; }

    /// <summary>
    /// Sticker access scope bound to this guild. Read operations route through the SDK's
    /// in-memory sticker cache (when enabled via
    /// <c>DiscordClientBuilder.WithStickerCache</c>) and fall back to REST per
    /// <see cref="DiscoSdk.Caching.StickerFetchMode"/>.
    /// </summary>
    DiscoSdk.Caching.IGuildStickers Stickers { get; }

    /// <summary>Builds a deferred REST action that retrieves all invites in this guild.</summary>
    IRestAction<IReadOnlyList<IInvite>> GetInvites();

    /// <summary>Builds a deferred REST action that retrieves voice regions available for this guild.</summary>
    IRestAction<IReadOnlyList<IVoiceRegion>> GetVoiceRegions();

    /// <summary>Builds a deferred REST action that retrieves the public preview of this guild.</summary>
    IRestAction<GuildPreview> GetPreview();

    /// <summary>Builds a deferred REST action that retrieves the vanity URL of this guild.</summary>
    IRestAction<IVanityUrl?> GetVanityUrl();

    /// <summary>
    /// Builds a deferred REST action that updates the required MFA level for this guild. The
    /// caller must be the guild owner.
    /// </summary>
    IReasonedRestAction ModifyMfaLevel(MfaLevel level);

    /// <summary>Builds a deferred REST action that lists this guild's integrations.</summary>
    IRestAction<IReadOnlyList<IIntegration>> GetIntegrations();

    /// <summary>
    /// Builds a deferred REST action that suspends invites and/or DMs for this guild until the
    /// supplied timestamps. Pass <c>null</c> to clear either suspension.
    /// </summary>
    IReasonedRestAction<IIncidentsData> ModifyIncidentActions(DateTimeOffset? invitesDisabledUntil, DateTimeOffset? dmsDisabledUntil);

    /// <summary>
    /// Builds a deferred REST action that lists all webhooks attached to channels in this guild.
    /// </summary>
    IRestAction<IReadOnlyList<IWebhook>> GetWebhooks();

    /// <summary>
    /// Application-command operations scoped to this guild (currently the per-guild
    /// command-permission endpoints).
    /// </summary>
    IGuildCommands Commands { get; }
}