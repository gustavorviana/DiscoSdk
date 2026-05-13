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

    /// <summary>
    /// Gets a REST action that retrieves the application role-connection metadata records.
    /// </summary>
    IRestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>> GetRoleConnectionMetadata();

    /// <summary>
    /// Gets a REST action that replaces the application role-connection metadata records (max 5).
    /// </summary>
    IRestAction<IReadOnlyList<IApplicationRoleConnectionMetadata>> UpdateRoleConnectionMetadata(IEnumerable<ApplicationRoleConnectionMetadata> records);
}