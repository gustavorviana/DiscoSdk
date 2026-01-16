using DiscoSdk.Commands;
using DiscoSdk.Events;
using DiscoSdk.Logging;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

public interface IDiscordClient
{
    string? ApplicationId { get; }
    IDiscordEventRegistry EventRegistry { get; }
    bool IsFullyInitialized { get; }
    bool IsReady { get; }
    ILogger Logger { get; }
    int TotalShards { get; }
    ICurrentUser User { get; }

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
}