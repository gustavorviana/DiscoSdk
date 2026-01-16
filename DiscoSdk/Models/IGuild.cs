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
    /// Gets the icon hash of this guild, or null if no icon is set.
    /// </summary>
    string? Icon { get; }

    /// <summary>
    /// Gets the icon hash of this guild, returned when in the template object.
    /// </summary>
    string? IconHash { get; }

    /// <summary>
    /// Gets the splash hash of this guild, or null if no splash is set.
    /// </summary>
    string? Splash { get; }

    /// <summary>
    /// Gets the discovery splash hash of this guild, or null if no discovery splash is set.
    /// </summary>
    string? DiscoverySplash { get; }

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
    Emoji[]? Emojis { get; }

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
    /// Gets the banner hash of this guild, or null if no banner is set.
    /// </summary>
    string? Banner { get; }

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
    IEditGuildRestAction Edit();

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
    ICreateEmojiAction CreateEmoji(string name, byte[] image);

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
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
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
    /// Creates a REST action to get banned members of this guild with pagination.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to retrieve banned members.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IBanPaginationAction GetBans();

    /// <summary>
    /// Gets a REST action to retrieve a ban by user ID in this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve the ban for.</param>
    /// <returns>A REST action that can be executed to retrieve the ban.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// Returns null if the user is not banned from this guild.
    /// </remarks>
    IRestAction<Ban?> GetBan(Snowflake userId);

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
    /// Gets a REST action to retrieve all emojis in this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve emojis.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<IReadOnlyList<Emoji>> GetEmojis();

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
    IBeginPruneAction BeginPrune(int days, params Snowflake[] includeRoles);

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
    IRestAction<string?> GetVanityUrl();

    /// <summary>
    /// Gets a REST action to retrieve the widget image of this guild.
    /// </summary>
    /// <param name="style">The style of the widget image.</param>
    /// <returns>A REST action that can be executed to retrieve the widget image as a stream.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<Stream> GetWidgetImage(string? style = null);
}