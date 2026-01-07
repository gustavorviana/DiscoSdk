using DiscoSdk.Commands;
using DiscoSdk.Events;
using DiscoSdk.Logging;

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
}