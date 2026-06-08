using DiscoSdk.Hosting.Gateway;

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
    /// Returns the configuration with the token masked — safe to log or include in exception
    /// messages. Discord bot tokens leaking into a shared log aggregator (Datadog, ELK, Sentry)
    /// is one of the most common ways bots get hijacked; routing every config dump through this
    /// override means no caller has to remember to redact.
    /// </summary>
    public override string ToString()
        => $"{nameof(DiscordClientConfig)} {{ Token = {TokenSanitizer.Mask(Token)}, Intents = {Intents}, TotalShards = {TotalShards?.ToString() ?? "auto"} }}";

    /// <summary>
    /// Gets or sets the gateway intents to subscribe to.
    /// </summary>
    public required DiscordIntent Intents { get; set; }

    /// <summary>
    /// Gets or sets the bounded capacity of each shard's per-shard dispatch queue. When a shard's
    /// queue is full the receive loop awaits — backpressure flows back through the WebSocket. Set
    /// per-shard, not global. Default is 100; must be at least 1.
    /// </summary>
    public int EventProcessorQueueCapacity { get; set; } = 100;

    /// <summary>
    /// Gets or sets the delay before attempting to reconnect after a connection loss.
    /// Default is 5 seconds.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum number of members in a guild before the gateway omits offline members from
    /// <c>GUILD_CREATE</c>. Range 50–250. Default 250 — the upper bound preserves the most
    /// information per chunk. Below this threshold, all members come in the initial payload.
    /// </summary>
    public int LargeThreshold { get; set; } = 250;

    /// <summary>
    /// Fraction of <c>heartbeat_interval</c> applied as random jitter on the first heartbeat
    /// after HELLO. Discord's spec recommends jitter to avoid a thundering-herd reconnect storm.
    /// Default <c>1.0</c> (full spec compliance, jitter ∈ [0, heartbeat_interval]). Tests can
    /// set <c>0.0</c> for a deterministic immediate first send.
    /// </summary>
    public double HeartbeatJitter { get; set; } = 1.0;

    /// <summary>
    /// Fraction of the exponential backoff added as random jitter to break reconnect-storm
    /// synchronisation across a fleet of bots. Default <c>0.2</c> (±20%). Tests can set
    /// <c>0.0</c> for a deterministic backoff sequence.
    /// </summary>
    public double ReconnectBackoffJitter { get; set; } = 0.2;

    /// <summary>
    /// How long to wait for the gateway's HELLO frame after the WebSocket dial completes before
    /// treating the connection as broken and forcing a retry. Defaults to 30 seconds — Discord
    /// normally sends HELLO within ~1 second; longer waits mean the connection is dead even
    /// though the WebSocket layer has not noticed.
    /// </summary>
    public TimeSpan HelloTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Default <see cref="GatewayUserAgent"/>; identifies the SDK in Discord-compliant form.</summary>
    public const string DefaultGatewayUserAgent = "DiscordBot (https://github.com/gustavorviana/DiscoSdk, 1.0)";

    /// <summary>
    /// User-Agent header sent on the gateway WebSocket dial. Discord requires the form
    /// <c>DiscordBot ($url, $version)</c> and may reject connections with a non-conforming UA.
    /// Override to identify your bot product (analytics, abuse reports).
    /// </summary>
    public string GatewayUserAgent { get; set; } = DefaultGatewayUserAgent;

    /// <summary>
    /// Maximum time the WebSocket close handshake is allowed before the SDK force-disposes the
    /// socket. Default 5 seconds. Above the K8s default <c>terminationGracePeriodSeconds=30</c>
    /// minus other shutdown work, a hung close drags the pod past the SIGKILL deadline.
    /// </summary>
    public TimeSpan CloseTimeout { get; set; } = DefaultCloseTimeout;

    /// <summary>Default <see cref="CloseTimeout"/> — 5 seconds.</summary>
    public static readonly TimeSpan DefaultCloseTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum number of reconnect attempts before the shard gives up and routes through
    /// <see cref="Gateway.Shards.IShardEventListener.OnFatalAsync"/> with the last failure. Default 20 —
    /// at the 900s backoff cap that is roughly 5 hours of trying. Set <c>0</c> or a negative value
    /// for unlimited retries (the original unlimited-retry behaviour).
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 20;

    /// <summary>
    /// Whether shards reconnect automatically when the gateway link drops. Default <c>true</c>.
    /// </summary>
    /// <remarks>
    /// When <c>false</c>, a dropped shard stays in <c>ConnectionLost</c>; the bot author is
    /// expected to subscribe to <see cref="IDiscordClient.GatewayDisconnected"/> and call
    /// <see cref="IDiscordClient.ReconnectShardAsync"/> when ready (e.g. after the network is
    /// confirmed back via an external healthcheck). The <c>GatewayDisconnected</c> event still
    /// fires either way — only the SDK-initiated retry is gated by this flag.
    /// </remarks>
    public bool AutoReconnect { get; set; } = true;
}
