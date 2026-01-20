using DiscoSdk.Events;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

public interface IDiscordClient
{
    IDiscordRestClientBase HttpClient { get; }
    string? ApplicationId { get; }
    IDiscordEventRegistry EventRegistry { get; }
    bool IsFullyInitialized { get; }
    bool IsReady { get; }
    ILogger Logger { get; }
    int TotalShards { get; }
    ICurrentUser BotUser { get; }

    event EventHandler? OnConnectionLost;
    event EventHandler? OnReady;

    Task StartAsync();
    Task StopAsync();
    ICommandUpdateAction UpdateCommands();
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
}