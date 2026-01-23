using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Logging;

namespace DiscoSdk.Hosting;

/// <summary>
/// Configuration settings for the Discord client.
/// </summary>
public class DiscordClientConfig
{
    public GatewayCompressMode GatewayCompressMode { get; set; } = GatewayCompressMode.ZlibStream;

    /// <summary>
    /// Gets or sets the total number of shards. If null, the value will be determined from the gateway.
    /// </summary>
    public int? TotalShards { get; set; }

    /// <summary>
    /// Gets or sets the bot token for authentication.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the gateway intents to subscribe to.
    /// </summary>
    public required DiscordIntent Intents { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent event processing operations.
    /// Uses the managed .NET ThreadPool (similar to ASP.NET Core MaxConcurrency).
    /// If 0 or negative, defaults to ProcessorCount * 2.
    /// </summary>
    public int EventProcessorMaxConcurrency { get; set; } = 0;

    /// <summary>
    /// Gets or sets the capacity of the bounded event processing queue.
    /// When the queue is full, producers will wait (backpressure) to prevent memory growth.
    /// Default is 100. Must be at least 1.
    /// </summary>
    public int EventProcessorQueueCapacity { get; set; } = 100;

    /// <summary>
    /// Gets or sets the logger instance. If null, uses NullLogger.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Gets or sets the delay before attempting to reconnect after a connection loss.
    /// Default is 5 seconds.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);
}
