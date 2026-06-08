using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DiscoSdk.Hosting.Observability;

/// <summary>
/// Entry point for DiscoSdk metrics and distributed traces. Bot authors subscribe via
/// OpenTelemetry — <c>builder.AddMeter(DiscoSdkDiagnostics.MeterName)</c> for metrics and
/// <c>builder.AddSource(DiscoSdkDiagnostics.ActivitySourceName)</c> for traces — or via the
/// raw <see cref="MeterListener"/> / <see cref="ActivityListener"/> APIs for non-OTel pipelines.
/// </summary>
/// <remarks>
/// <para>
/// Every instrument and activity is a no-op while no listener is attached: there is no allocation,
/// locking, or network cost on the hot path. Bots that do not opt in pay zero overhead.
/// </para>
/// <para>
/// Tag names follow OpenTelemetry semantic conventions where one exists (<c>http.method</c>,
/// <c>http.status_code</c>); Discord-specific tags are prefixed <c>discord.</c>
/// (<see cref="DiagnosticTags.ShardId"/>, <see cref="DiagnosticTags.Route"/>, etc.). The
/// <see cref="DiagnosticTags"/> class centralises every key the SDK emits so cross-instrument
/// dashboards do not drift.
/// </para>
/// </remarks>
public static class DiscoSdkDiagnostics
{
    /// <summary>
    /// Name of the <see cref="System.Diagnostics.Metrics.Meter"/> every DiscoSdk instrument lives
    /// on. Reference this constant from OpenTelemetry's
    /// <c>builder.AddMeter(DiscoSdkDiagnostics.MeterName)</c> so a future SDK rename ripples through
    /// without code change.
    /// </summary>
    public const string MeterName = "DiscoSdk";

    /// <summary>
    /// Name of the <see cref="System.Diagnostics.ActivitySource"/> every DiscoSdk activity is
    /// created on. Reference from <c>builder.AddSource(DiscoSdkDiagnostics.ActivitySourceName)</c>.
    /// </summary>
    public const string ActivitySourceName = "DiscoSdk";

    private static readonly string Version =
        typeof(DiscoSdkDiagnostics).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    /// <summary>Shared <see cref="System.Diagnostics.Metrics.Meter"/> instance. Internal — the SDK is the only producer.</summary>
    internal static readonly Meter Meter = new(MeterName, Version);

    /// <summary>Shared <see cref="System.Diagnostics.ActivitySource"/> instance. Internal — the SDK is the only producer.</summary>
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version);

    // ---- Gateway instruments -----------------------------------------------------------------

    /// <summary>
    /// Counter incremented once per inbound gateway dispatch event the SDK observed. Tagged with
    /// <see cref="DiagnosticTags.ShardId"/> and <see cref="DiagnosticTags.EventType"/>. Useful for
    /// detecting traffic spikes per shard or per event family.
    /// </summary>
    internal static readonly Counter<long> GatewayEventsReceived = Meter.CreateCounter<long>(
        "discosdk.gateway.events_received",
        unit: "{event}",
        description: "Inbound gateway dispatch events received, tagged by shard and event type.");

    /// <summary>
    /// Histogram recording the round-trip from heartbeat send to <c>HEARTBEAT_ACK</c> in
    /// milliseconds. Tagged with <see cref="DiagnosticTags.ShardId"/>. Equivalent to the "ping"
    /// figure Discord clients display — a sustained high value indicates a degraded link.
    /// </summary>
    internal static readonly Histogram<double> GatewayHeartbeatLatency = Meter.CreateHistogram<double>(
        "discosdk.gateway.heartbeat.latency",
        unit: "ms",
        description: "Round-trip latency between heartbeat send and HEARTBEAT_ACK per shard.");

    // ---- REST instruments --------------------------------------------------------------------

    /// <summary>
    /// Counter incremented per Discord REST request issued, tagged with
    /// <see cref="DiagnosticTags.Route"/>, <see cref="DiagnosticTags.HttpMethod"/>, and
    /// <see cref="DiagnosticTags.HttpStatusClass"/> (<c>2xx</c>/<c>4xx</c>/<c>5xx</c>). Status codes
    /// are bucketed into classes to keep tag cardinality bounded.
    /// </summary>
    internal static readonly Counter<long> RestRequests = Meter.CreateCounter<long>(
        "discosdk.rest.requests",
        unit: "{request}",
        description: "Discord REST requests sent, tagged by route template, method, and status class.");

    /// <summary>
    /// Histogram of REST end-to-end latency in milliseconds (HTTP send + receive). Tagged with
    /// <see cref="DiagnosticTags.Route"/> and <see cref="DiagnosticTags.HttpMethod"/>. Use for
    /// p95/p99 SLOs and to detect a slow upstream route.
    /// </summary>
    internal static readonly Histogram<double> RestLatency = Meter.CreateHistogram<double>(
        "discosdk.rest.latency",
        unit: "ms",
        description: "REST request latency (HTTP round-trip), tagged by route template and method.");

    /// <summary>
    /// Counter incremented once per 429 response observed. Tagged with
    /// <see cref="DiagnosticTags.Route"/> and <see cref="DiagnosticTags.Scope"/>
    /// (<c>bucket</c>/<c>shared</c>/<c>global</c>). Routine <c>bucket</c> hits are normal; a sudden
    /// surge in <c>global</c> events is an emergency.
    /// </summary>
    internal static readonly Counter<long> RestRateLimited = Meter.CreateCounter<long>(
        "discosdk.rest.rate_limited",
        unit: "{response}",
        description: "Discord 429 responses observed, tagged by route and X-RateLimit-Scope.");

    // ---- Cache instruments (Phase 3) ---------------------------------------------------------

    /// <summary>
    /// Counter incremented once per cache lookup. Tagged with
    /// <see cref="DiagnosticTags.CacheEntity"/> (<c>member</c>/<c>sticker</c>/<c>presence</c>) and
    /// <see cref="DiagnosticTags.CacheResult"/> (<c>hit</c>/<c>miss</c>/<c>rest</c>). The
    /// <c>rest</c> result row indicates a cache miss that was resolved by a REST fallback.
    /// </summary>
    internal static readonly Counter<long> CacheLookups = Meter.CreateCounter<long>(
        "discosdk.cache.lookups",
        unit: "{lookup}",
        description: "Cache lookups, tagged by entity and result (hit / miss / rest).");

    /// <summary>
    /// Counter of cache entries actively evicted by the SDK — for example a member whose
    /// updated state no longer satisfies the configured policy. Tagged with
    /// <see cref="DiagnosticTags.CacheEntity"/>.
    /// </summary>
    internal static readonly Counter<long> CacheEvictions = Meter.CreateCounter<long>(
        "discosdk.cache.evictions",
        unit: "{entry}",
        description: "Cache entries evicted by policy decisions, tagged by entity.");

    // ---- Event-handler instruments (Phase 4) -------------------------------------------------

    /// <summary>
    /// Counter incremented once per event handler invocation. Tagged with
    /// <see cref="DiagnosticTags.HandlerType"/>, <see cref="DiagnosticTags.EventType"/>, and
    /// <see cref="DiagnosticTags.HandlerOutcome"/> (<c>ok</c>/<c>error</c>). Error rows also
    /// carry <see cref="DiagnosticTags.ExceptionType"/>.
    /// </summary>
    internal static readonly Counter<long> HandlerInvocations = Meter.CreateCounter<long>(
        "discosdk.handler.invocations",
        unit: "{invocation}",
        description: "Event-handler invocations, tagged by handler type, event type, and outcome.");

    /// <summary>
    /// Histogram of handler runtime in milliseconds. Tagged with
    /// <see cref="DiagnosticTags.HandlerType"/> and <see cref="DiagnosticTags.EventType"/>.
    /// Use to detect slow handlers backing up the dispatcher.
    /// </summary>
    internal static readonly Histogram<double> HandlerLatency = Meter.CreateHistogram<double>(
        "discosdk.handler.latency",
        unit: "ms",
        description: "Event-handler execution time, tagged by handler type and event type.");

    // ---- Gateway lifecycle instruments (Phase 5) ---------------------------------------------

    /// <summary>
    /// Counter that records every gateway lifecycle transition the SDK observes. Tagged with
    /// <see cref="DiagnosticTags.ShardId"/> and <see cref="DiagnosticTags.GatewayPhase"/>.
    /// Use to chart shard health (READY counts, reconnect cadence, invalidate spikes).
    /// </summary>
    internal static readonly Counter<long> GatewayLifecycle = Meter.CreateCounter<long>(
        "discosdk.gateway.lifecycle",
        unit: "{transition}",
        description: "Gateway lifecycle transitions, tagged by shard and phase.");

    /// <summary>
    /// Counter incremented every time a shard begins a reconnect attempt. Tagged with
    /// <see cref="DiagnosticTags.ShardId"/>.
    /// </summary>
    internal static readonly Counter<long> GatewayReconnects = Meter.CreateCounter<long>(
        "discosdk.gateway.reconnects",
        unit: "{attempt}",
        description: "Reconnect attempts per shard.");
}
