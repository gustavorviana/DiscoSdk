using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord guild (server) with all its properties and available actions.
/// </summary>
/// <remarks>
/// All Discord IDs must be of type <see cref="DiscordId"/>.
/// All methods that perform server actions return <see cref="IRestAction"/> or <see cref="IRestAction{T}"/>.
/// </remarks>
public interface IGuild
{
    DiscordId Id { get; }

    string Name { get; }

    string? Icon { get; }

    string? IconHash { get; }

    string? Splash { get; }

    string? DiscoverySplash { get; }

    bool? Owner { get; }

    DiscordId? OwnerId { get; }

    int? Permissions { get; }

    string? Region { get; }

    DiscordId? AfkChannelId { get; }

    int? AfkTimeout { get; }

    bool? WidgetEnabled { get; }

    DiscordId? WidgetChannelId { get; }

    VerificationLevel? VerificationLevel { get; }

    DefaultMessageNotificationLevel? DefaultMessageNotifications { get; }

    ExplicitContentFilterLevel? ExplicitContentFilter { get; }

    IRole[]? Roles { get; }

    Emoji[]? Emojis { get; }

    string[]? Features { get; }

    MfaLevel? MfaLevel { get; }

    DiscordId? ApplicationId { get; }

    DiscordId? SystemChannelId { get; }

    SystemChannelFlags? SystemChannelFlags { get; }

    DiscordId? RulesChannelId { get; }

    int? MaxPresences { get; }

    int? MaxMembers { get; }

    string? VanityUrlCode { get; }

    string? Description { get; }

    string? Banner { get; }

    PremiumTier? PremiumTier { get; }

    int? PremiumSubscriptionCount { get; }

    string? PreferredLocale { get; }

    DiscordId? PublicUpdatesChannelId { get; }

    int? MaxVideoChannelUsers { get; }

    int? ApproximateMemberCount { get; }

    int? ApproximatePresenceCount { get; }

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
    IBanMemberAction BanMember(DiscordId userId, int deleteMessageDays = 0);

    /// <summary>
    /// Creates a REST action to unban a user from this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to unban.</param>
    /// <returns>A REST action that can be executed to unban the user.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction UnbanMember(DiscordId userId);

    /// <summary>
    /// Creates a REST action to kick a member from this guild.
    /// </summary>
    /// <param name="userId">The ID of the user to kick.</param>
    /// <returns>A REST action that can be executed to kick the member.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction KickMember(DiscordId userId);

    /// <summary>
    /// Creates a REST action to get members of this guild with pagination.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to retrieve members.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IMemberPaginationAction GetMembers();

    /// <summary>
    /// Creates a REST action to get banned members of this guild with pagination.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to retrieve banned members.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IBanPaginationAction GetBans();

    /// <summary>
    /// Creates a REST action to get audit logs of this guild with pagination.
    /// </summary>
    /// <returns>A REST action that can be configured and executed to retrieve audit logs.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IAuditLogPaginationAction GetAuditLogs();

    /// <summary>
    /// Gets a REST action to retrieve a channel by its ID in this guild.
    /// </summary>
    /// <param name="channelId">The ID of the channel to retrieve.</param>
    /// <returns>A REST action that can be executed to retrieve the channel.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<IGuildChannel?> GetChannel(DiscordId channelId);

    /// <summary>
    /// Gets a REST action to retrieve all channels in this guild.
    /// </summary>
    /// <returns>A REST action that can be executed to retrieve channels.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<IReadOnlyList<IGuildChannel>> GetChannels();

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
    IRestAction<int> GetPruneCount(int days, params DiscordId[] includeRoles);

    /// <summary>
    /// Creates a REST action to begin a prune operation on this guild.
    /// </summary>
    /// <param name="days">The number of days to prune inactive members (1-30).</param>
    /// <param name="includeRoles">The role IDs to include in the prune.</param>
    /// <param name="computePruneCount">Whether to return the prune count instead of actually pruning.</param>
    /// <returns>A REST action that can be configured and executed to begin the prune.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IBeginPruneAction BeginPrune(int days, params DiscordId[] includeRoles);

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