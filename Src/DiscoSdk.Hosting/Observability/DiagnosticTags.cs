namespace DiscoSdk.Hosting.Observability;

/// <summary>
/// Central registry of tag keys emitted on <see cref="DiscoSdkDiagnostics"/> instruments and
/// activities. Centralising lets dashboards, alerts, and exporters reference a single source —
/// renaming a key here is the only edit needed for the SDK to stay in sync.
/// </summary>
/// <remarks>
/// Key naming policy: OpenTelemetry semantic conventions are used as-is for HTTP
/// (<see cref="HttpMethod"/>, <see cref="HttpStatusCode"/>) so OTel collectors and dashboards
/// recognise them. Discord-specific keys are prefixed <c>discord.</c> to avoid collisions with
/// other libraries; SDK-internal keys use <c>discosdk.</c>.
/// </remarks>
internal static class DiagnosticTags
{
    /// <summary>Numeric shard id (<c>int</c>) — emitted on every gateway-side instrument.</summary>
    public const string ShardId = "discord.shard.id";

    /// <summary>Discord dispatch event name (<c>string</c>), e.g. <c>MESSAGE_CREATE</c>.</summary>
    public const string EventType = "discord.event.type";

    /// <summary>
    /// Route template (<c>string</c>) — the unsubstituted Discord route, e.g.
    /// <c>guilds/{guild_id}/channels</c>. Using the template keeps tag cardinality bounded;
    /// the substituted path is intentionally not exposed.
    /// </summary>
    public const string Route = "discord.route";

    /// <summary>
    /// Value of the Discord <c>X-RateLimit-Scope</c> header on 429 responses — one of
    /// <c>user</c>, <c>shared</c>, or <c>global</c>.
    /// </summary>
    public const string Scope = "discord.scope";

    /// <summary>OTel-standard HTTP method tag (<c>string</c>, uppercase: <c>GET</c>, <c>POST</c>, …).</summary>
    public const string HttpMethod = "http.method";

    /// <summary>OTel-standard HTTP status code tag (<c>int</c>).</summary>
    public const string HttpStatusCode = "http.status_code";

    /// <summary>
    /// Bucketed status class (<c>string</c>): <c>2xx</c>, <c>3xx</c>, <c>4xx</c>, or <c>5xx</c>.
    /// Emitted alongside <see cref="HttpStatusCode"/> on counters where the full code would
    /// blow up tag cardinality.
    /// </summary>
    public const string HttpStatusClass = "http.status_class";

    /// <summary>Status-class constant for 2xx responses.</summary>
    public const string Status2xx = "2xx";

    /// <summary>Status-class constant for 3xx responses.</summary>
    public const string Status3xx = "3xx";

    /// <summary>Status-class constant for 4xx responses.</summary>
    public const string Status4xx = "4xx";

    /// <summary>Status-class constant for 5xx responses.</summary>
    public const string Status5xx = "5xx";

    /// <summary>Maps an <see cref="System.Net.HttpStatusCode"/> integer to its status-class string.</summary>
    public static string ClassifyStatus(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => Status2xx,
        >= 300 and < 400 => Status3xx,
        >= 400 and < 500 => Status4xx,
        >= 500 and < 600 => Status5xx,
        _ => statusCode.ToString(),
    };

    // ---- Cache + handler + lifecycle tags (Phase 2+) -----------------------------------------

    /// <summary>Entity the cache instrument applies to (<c>string</c>): <c>member</c>, <c>sticker</c>, <c>presence</c>.</summary>
    public const string CacheEntity = "discosdk.cache.entity";

    /// <summary>Cache-entity constant for the member cache.</summary>
    public const string CacheEntityMember = "member";

    /// <summary>Cache-entity constant for the sticker cache.</summary>
    public const string CacheEntitySticker = "sticker";

    /// <summary>Cache-entity constant for the presence cache.</summary>
    public const string CacheEntityPresence = "presence";

    /// <summary>Cache-fetch result (<c>string</c>): <c>hit</c>, <c>miss</c>, <c>rest</c> (cache miss + REST fallback).</summary>
    public const string CacheResult = "discosdk.cache.result";

    /// <summary>Event-handler concrete type (<c>string</c>) — produced once per registered handler.</summary>
    public const string HandlerType = "discosdk.handler.type";

    /// <summary>
    /// Event-handler outcome (<c>string</c>): <c>ok</c>, <c>error</c>. <c>error</c> rows are
    /// also tagged with <see cref="ExceptionType"/>.
    /// </summary>
    public const string HandlerOutcome = "discosdk.handler.outcome";

    /// <summary>Outcome constant for handler invocations that completed cleanly.</summary>
    public const string OutcomeOk = "ok";

    /// <summary>Outcome constant for handler invocations that threw.</summary>
    public const string OutcomeError = "error";

    /// <summary>Fully qualified exception type (<c>string</c>) emitted on error rows.</summary>
    public const string ExceptionType = "exception.type";

    /// <summary>Gateway lifecycle phase (<c>string</c>): <c>connect</c>, <c>identify</c>, <c>resume</c>, <c>ready</c>, <c>disconnect</c>, <c>invalidate</c>.</summary>
    public const string GatewayPhase = "discord.gateway.phase";

    /// <summary>Gateway-phase constant for the initial transport handshake.</summary>
    public const string PhaseConnect = "connect";

    /// <summary>Gateway-phase constant for an IDENTIFY exchange.</summary>
    public const string PhaseIdentify = "identify";

    /// <summary>Gateway-phase constant for a RESUME exchange.</summary>
    public const string PhaseResume = "resume";

    /// <summary>Gateway-phase constant for the READY event.</summary>
    public const string PhaseReady = "ready";

    /// <summary>Gateway-phase constant for an observed disconnect (close handshake or abort).</summary>
    public const string PhaseDisconnect = "disconnect";

    /// <summary>Gateway-phase constant for an INVALID_SESSION dispatch.</summary>
    public const string PhaseInvalidate = "invalidate";
}
