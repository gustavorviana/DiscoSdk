using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Models.Channels;
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

    /// <summary>
    /// Gateway intents configured for this client via
    /// <see cref="DiscoSdk.Hosting.Builders.DiscordClientBuilder.WithIntents(DiscordIntent)"/>.
    /// Use it to short-circuit operations that depend on intent-gated data before sending them
    /// to Discord — see <see cref="DiscoSdk.Exceptions.MissingIntentException"/>.
    /// </summary>
    DiscordIntent Intents { get; }

    // ---- Grouped surfaces (cohesive feature subsystems live here, not as flat methods) ----

    /// <summary>OAuth2 token-flow operations: authorize URL, code exchange, refresh, revoke, <c>@me</c>.</summary>
    IOAuth2 OAuth2 { get; }

    /// <summary>SKU / entitlement / subscription operations.</summary>
    IMonetization Monetization { get; }

    /// <summary>Endpoints scoped to the current bot user (the <c>@me</c> namespace).</summary>
    IMe Me { get; }

    /// <summary>Webhook operations not naturally owned by a channel/guild entity.</summary>
    IWebhooks Webhooks { get; }

    /// <summary>Application-owned (global) emoji operations.</summary>
    IApplicationEmojis ApplicationEmojis { get; }

    event Func<IDiscordClient, ICommandUpdateSession, Task>? CommandsUpdateWindowOpened;
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
    IRestAction<TimeSpan> Ping();

    /// <summary>
    /// Gets a REST action to create or get a direct message channel with the specified user.
    /// </summary>
    IRestAction<IDmChannel> OpenDm(Snowflake userId);

    /// <summary>
    /// Builds a REST action that creates a new group DM channel with 2–10 recipients. Each
    /// recipient must have granted the application the OAuth2 <c>gdm.join</c> scope — bot tokens
    /// alone are not enough.
    /// </summary>
    /// <remarks>
    /// Discord docs: <see href="https://discord.com/developers/docs/resources/user#create-group-dm"/>.
    /// </remarks>
    ICreateGroupDmAction CreateGroupDm();

    /// <summary>
    /// Gets a REST action to retrieve a user by their ID from the Discord API.
    /// </summary>
    IRestAction<IUser?> GetUser(Snowflake userId);

    /// <summary>
    /// Gets a REST action to update the bot's presence (status and activities).
    /// </summary>
    IUpdatePresenceAction UpdatePresence();

    /// <summary>
    /// Gets a REST action that retrieves the application associated with this bot.
    /// </summary>
    IRestAction<IApplication> GetApplication();

    /// <summary>Gets any sticker by id (built-in or guild).</summary>
    IRestAction<ISticker> GetSticker(Snowflake stickerId);

    /// <summary>Lists the built-in Nitro sticker packs.</summary>
    IRestAction<IReadOnlyList<IStickerPack>> GetStickerPacks();

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
    IRestAction<IInvite?> GetInvite(string code, bool? withCounts = null, bool? withExpiration = null, Snowflake? guildScheduledEventId = null);

    /// <summary>
    /// Gets a REST action that retrieves a running embedded-activity instance by its id, scoped
    /// to the bot's own application. Returns <c>null</c> when the instance does not exist
    /// (Discord 404).
    /// </summary>
    /// <remarks>
    /// Typical use: pair with a <c>LaunchActivity</c> interaction callback executed with
    /// <c>with_response=true</c> — the callback response carries the new instance id, which can
    /// then be inspected here for the full <see cref="ActivityInstance"/> (location, users).
    /// Discord docs:
    /// <see href="https://discord.com/developers/docs/resources/application#get-application-activity-instance"/>.
    /// </remarks>
    IRestAction<ActivityInstance?> GetActivityInstance(string instanceId);
}
