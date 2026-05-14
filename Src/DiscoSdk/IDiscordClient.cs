using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Monetization;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

public interface IDiscordClient
{
    IDiscordRestClient HttpClient { get; }
    Snowflake? ApplicationId { get; }
    bool IsFullyInitialized { get; }
    bool IsReady { get; }
    ILogger Logger { get; }
    int TotalShards { get; }
    ICurrentUser BotUser { get; }
    IServiceProvider Services { get; }

    event EventHandler<CommandContainer>? CommandsUpdateWindowOpened;
    event EventHandler<UnhandledErrorEventArgs>? UnhandledError;
    event EventHandler? OnConnectionLost;

    event EventHandler? OnReady;

    Task StartAsync();
    Task StopAsync();
    Task WaitReadyAsync(CancellationToken cancellationToken = default);
    Task WaitReadyAsync(TimeSpan timeout);
    Task WaitShutdownAsync(CancellationToken ct = default);
    Task WaitShutdownAsync(TimeSpan timeout);

    /// <summary>
    /// Gets a channel by its ID from the Discord API.
    /// </summary>
    /// <param name="channelId">The ID of the channel to retrieve.</param>
    /// <returns>The channel as its most specific type, or null if not found.</returns>
    IRestAction<IChannel?> GetChannel(Snowflake channelId);

    /// <summary>
    /// Gets a channel by its ID from the Discord API.
    /// </summary>
    /// <param name="channelId">The ID of the channel to retrieve.</param>
    /// <returns>The channel as its most specific type, or null if not found.</returns>
    IRestAction<TChannel?> GetChannel<TChannel>(Snowflake channelId) where TChannel : IChannel;

    /// <summary>
    /// Gets a REST action to measure the latency (ping) to the Discord API.
    /// </summary>
    /// <returns>A REST action that can be executed to measure the API latency in milliseconds.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// This measures the round-trip time to the Discord API by making a simple request.
    /// </remarks>
    IRestAction<TimeSpan> Ping();

    /// <summary>
    /// Gets a REST action to create or get a direct message channel with the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user to create a DM with.</param>
    /// <returns>A REST action that can be executed to get or create the DM channel.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// </remarks>
    IRestAction<IDmChannel> OpenDm(Snowflake userId);

    /// <summary>
    /// Gets a REST action to retrieve a user by their ID from the Discord API.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <returns>A REST action that can be executed to retrieve the user.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
    /// Returns null if the user is not found.
    /// </remarks>
    IRestAction<IUser?> GetUser(Snowflake userId);

    /// <summary>
    /// Gets a REST action to update the bot's presence (status and activities).
    /// </summary>
    /// <returns>A REST action that can be configured and executed to update the presence.</returns>
    /// <remarks>
    /// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
    /// This updates the presence through the Gateway connection.
    /// </remarks>
    IUpdatePresenceAction UpdatePresence();

    /// <summary>
    /// Gets a REST action that retrieves the application associated with this bot.
    /// </summary>
    IRestAction<IApplication> GetApplication();

    /// <summary>
    /// Gets a REST action that retrieves the SKUs (premium offerings) for this application.
    /// </summary>
    IRestAction<IReadOnlyList<ISku>> GetSkus();

    /// <summary>
    /// Gets a REST action that retrieves entitlements for this application, optionally filtered by user and/or guild.
    /// </summary>
    /// <param name="userId">If set, only entitlements granted to this user.</param>
    /// <param name="guildId">If set, only entitlements granted to this guild.</param>
    /// <param name="excludeEnded">If <c>true</c>, exclude entitlements whose period has ended.</param>
    /// <param name="excludeDeleted">If <c>false</c>, include deleted entitlements (default is to exclude them).</param>
    IRestAction<IReadOnlyList<IEntitlement>> GetEntitlements(Snowflake? userId = null, Snowflake? guildId = null, bool? excludeEnded = null, bool? excludeDeleted = null);

    /// <summary>
    /// Gets a REST action that retrieves a single entitlement by ID.
    /// </summary>
    IRestAction<IEntitlement> GetEntitlement(Snowflake entitlementId);

    /// <summary>
    /// Gets a REST action that marks a one-time-purchase consumable entitlement as consumed, by ID.
    /// </summary>
    IRestAction ConsumeEntitlement(Snowflake entitlementId);

    /// <summary>
    /// Gets a REST action that retrieves the subscriptions for an SKU.
    /// </summary>
    /// <param name="skuId">The SKU to list subscriptions for.</param>
    /// <param name="userId">If set, only the subscription owned by this user.</param>
    IRestAction<IReadOnlyList<ISubscription>> GetSkuSubscriptions(Snowflake skuId, Snowflake? userId = null);

    /// <summary>
    /// Gets a REST action that retrieves a single SKU subscription by ID.
    /// </summary>
    IRestAction<ISubscription> GetSkuSubscription(Snowflake skuId, Snowflake subscriptionId);

    /// <summary>Gets any sticker by id (built-in or guild).</summary>
    IRestAction<ISticker> GetSticker(Snowflake stickerId);

    /// <summary>Lists the built-in Nitro sticker packs.</summary>
    IRestAction<IReadOnlyList<IStickerPack>> GetStickerPacks();

    /// <summary>Lists the application-owned emojis.</summary>
    IRestAction<IReadOnlyList<IEmoji>> GetApplicationEmojis();

    /// <summary>Gets a single application-owned emoji by id.</summary>
    IRestAction<IEmoji> GetApplicationEmoji(Snowflake emojiId);

    /// <summary>Creates a new application-owned emoji from <paramref name="image"/>.</summary>
    /// <param name="name">Emoji name (2-32 chars).</param>
    /// <param name="image">Image buffer (PNG/JPEG/GIF, 128x128).</param>
    IRestAction<IEmoji> CreateApplicationEmoji(string name, DiscordImageBuffer image);

    /// <summary>
    /// Gets a REST action that retrieves the application role-connection metadata records.
    /// </summary>
    IRestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>> GetRoleConnectionMetadata();

    /// <summary>
    /// Gets a REST action that replaces the application role-connection metadata records (max 5).
    /// </summary>
    IRestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>> UpdateRoleConnectionMetadata(IEnumerable<ApplicationRoleConnectionMetadata> records);

    /// <summary>
    /// Gets a REST action that resolves an invite by its code. Returns <c>null</c> if the code does not exist.
    /// </summary>
    /// <param name="code">The invite code to resolve.</param>
    /// <param name="withCounts">If set, include approximate member and presence counts.</param>
    /// <param name="withExpiration">If set, include the invite's <c>expires_at</c> timestamp.</param>
    /// <param name="guildScheduledEventId">If set, embed the matching guild scheduled event in the response.</param>
    IRestAction<IInvite?> GetInvite(string code, bool? withCounts = null, bool? withExpiration = null, Snowflake? guildScheduledEventId = null);

    /// <summary>
    /// Gets a REST action that returns the current user (the bot).
    /// </summary>
    IRestAction<IUser> GetCurrentUser();

    /// <summary>
    /// Gets a REST action that modifies the current user. Pass <c>null</c> for any field to leave it unchanged.
    /// </summary>
    /// <param name="username">If set, the new username.</param>
    /// <param name="avatar">If set, the new avatar as a base64-encoded data URI, or empty string to remove.</param>
    /// <param name="banner">If set, the new banner as a base64-encoded data URI, or empty string to remove.</param>
    IRestAction<IUser> ModifyCurrentUser(string? username = null, string? avatar = null, string? banner = null);

    /// <summary>
    /// Gets a REST action that lists the guilds the bot is a member of. Paginate with <paramref name="before"/> / <paramref name="after"/>.
    /// </summary>
    IRestAction<IReadOnlyList<IGuild>> GetCurrentUserGuilds(int? limit = null, Snowflake? before = null, Snowflake? after = null, bool? withCounts = null);

    /// <summary>
    /// Gets a REST action that returns the bot's member object in a specific guild.
    /// </summary>
    IRestAction<IMember> GetCurrentUserGuildMember(Snowflake guildId);

    /// <summary>
    /// Gets a REST action that lists the bot's third-party account connections.
    /// </summary>
    IRestAction<IReadOnlyList<IConnection>> GetCurrentUserConnections();

    /// <summary>
    /// Gets a REST action that retrieves the bot's role-connection data for the given application (linked roles).
    /// </summary>
    IRestAction<IApplicationRoleConnection> GetCurrentUserApplicationRoleConnection(Snowflake applicationId);

    /// <summary>
    /// Gets a REST action that updates the bot's role-connection data for the given application (linked roles).
    /// </summary>
    IRestAction<IApplicationRoleConnection> UpdateCurrentUserApplicationRoleConnection(Snowflake applicationId, ApplicationRoleConnection record);

    /// <summary>
    /// Gets a REST action that resolves a webhook by ID. Returns <c>null</c> if the webhook does not exist.
    /// </summary>
    IRestAction<IWebhook?> GetWebhook(Snowflake webhookId);

    /// <summary>
    /// Gets a REST action that resolves a webhook by ID and token (no permission check). Returns <c>null</c> if not found.
    /// </summary>
    IRestAction<IWebhook?> GetWebhook(Snowflake webhookId, string token);

    /// <summary>
    /// Gets a REST action that creates a new webhook on a channel.
    /// </summary>
    /// <param name="channelId">The channel to host the webhook on.</param>
    /// <param name="name">The default name (1–80 chars).</param>
    /// <param name="avatar">Optional base64 image data URI for the default avatar.</param>
    IRestAction<IWebhook> CreateChannelWebhook(Snowflake channelId, string name, string? avatar = null);

    /// <summary>
    /// Gets a REST action that lists the webhooks attached to a channel.
    /// </summary>
    IRestAction<IReadOnlyList<IWebhook>> GetChannelWebhooks(Snowflake channelId);
}