using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.RateLimit;
using DiscoSdk.Rest;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Base implementation for making HTTP requests to the Discord REST API.
/// </summary>
public class DiscordRestClient : IDisposable, IDiscordRestClient
{
    /// <summary>
    /// Hard ceiling on the number of requests that may be "in the system" at once (waiting on a
    /// bucket gate or in HTTP flight). It is a memory guard, not a rate limit: Discord's own
    /// per-bucket and global windows control throughput; this just stops a runaway worker that
    /// fires hundreds of thousands of requests at once from allocating that many request states
    /// before any per-bucket serialisation applies backpressure. Generous on purpose — at
    /// Discord's 50 req/s global cap a healthy bot never approaches it.
    /// </summary>
    private const int MaxConcurrentRequests = 2048;

    /// <summary>
    /// How often the background sweeper walks the bucket dictionary looking for idle queues
    /// to evict. The interval is intentionally coarse — eviction is a memory-leak guard for
    /// long-running processes, not a hot-path concern.
    /// </summary>
    private static readonly TimeSpan BucketEvictionInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// A bucket queue must have been idle (no <see cref="BucketRequestQueue.ExecuteAsync"/>
    /// activity) for at least this long before the sweeper releases it. Generous on purpose:
    /// at fleet scale a brief 429-induced pause should not look like idleness.
    /// </summary>
    private static readonly TimeSpan BucketEvictionIdleThreshold = TimeSpan.FromMinutes(15);

    private readonly ConcurrentDictionary<string, BucketRequestQueue> _buckets = [];
    private readonly ConcurrentDictionary<string, string> _routeToHash = [];
    private readonly GlobalRateLimitManager _globalRateLimiter;
    private readonly SemaphoreSlim _inflightGate = new(MaxConcurrentRequests, MaxConcurrentRequests);
    private readonly CancellationTokenSource _shutdownCts = new();

    /// <summary>
    /// Process-wide HTTP transport handler shared across every <see cref="DiscordRestClient"/>
    /// instance. The single connection pool maximises HTTP/2 stream reuse, lets DNS refresh on
    /// every <see cref="SocketsHttpHandler.PooledConnectionLifetime"/> tick (so Discord failovers
    /// don't pin us to a stale IP), and enables transparent <c>gzip / deflate / brotli</c>
    /// decoding of responses. The handler is never disposed — it lives for the process lifetime.
    /// </summary>
    private static readonly SocketsHttpHandler s_handler = new()
    {
        AutomaticDecompression = DecompressionMethods.All,
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        EnableMultipleHttp2Connections = true,
    };

    // disposeHandler:false because s_handler is static and shared.
    private readonly HttpClient _http = new(s_handler, disposeHandler: false)
    {
        // Negotiate HTTP/2 with HTTP/1.1 fallback. Discord supports HTTP/2 today; multiplexing
        // many concurrent requests over a single TCP connection cuts handshake overhead in
        // bursts and reduces socket churn at fleet scale.
        DefaultRequestVersion = HttpVersion.Version20,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
    };

    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;
    private volatile bool _disposed;

    public JsonSerializerOptions JsonOptions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordRestClient"/> class.
    /// </summary>
    /// <param name="botToken">The bot token for authentication.</param>
    /// <param name="apiUri">The base URI of the Discord API.</param>
    /// <exception cref="ArgumentException">Thrown when the bot token is null or whitespace.</exception>
    public DiscordRestClient(string botToken, Uri apiUri, JsonSerializerOptions jsonOptions, ILogger logger, TimeProvider timeProvider, TimeSpan? timeout = null)
    : this(apiUri, jsonOptions, logger, timeProvider, timeout)
    {
        if (string.IsNullOrWhiteSpace(botToken))
            throw new ArgumentException("Bot token is required.", nameof(botToken));

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordRestClient"/> class.
    /// </summary>
    /// <param name="apiUri">The base URI of the Discord API.</param>
    /// <exception cref="ArgumentException">Thrown when the bot token is null or whitespace.</exception>
    public DiscordRestClient(Uri apiUri, JsonSerializerOptions jsonOptions, ILogger logger, TimeProvider timeProvider, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(apiUri);
        ArgumentNullException.ThrowIfNull(jsonOptions);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (timeout.HasValue)
            _http.Timeout = timeout.Value;

        _logger = logger;
        _timeProvider = timeProvider;
        JsonOptions = jsonOptions;
        _http.BaseAddress = apiUri;
        _globalRateLimiter = new GlobalRateLimitManager(_logger, _timeProvider);

        // Read the SDK version from assembly metadata so the User-Agent stays in sync with
        // package releases automatically. AssemblyName.Version is preferred over the
        // InformationalVersion attribute because it is always valid token format (digits +
        // dots) — InformationalVersion may carry SemVer build metadata (e.g. "+gitsha")
        // that ProductInfoHeaderValue would reject.
        var version = typeof(DiscordRestClient).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(DeviceInfo.SdkName, version));

        _ = Task.Run(EvictionLoopAsync);
    }

    public Task<T> SendAsync<T>(DiscordRoute path, HttpMethod method, CancellationToken ct)
    {
        return SendAsync<T>(path, method, null, ct);
    }

    /// <summary>
    /// Sends a JSON request to the Discord API and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="body">The request body object to serialize, or null.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains the deserialized response.</returns>
    /// <exception cref="DiscordApiException">Thrown when the API request fails.</exception>
    public Task<T> SendAsync<T>(DiscordRoute path, HttpMethod method, object? body, CancellationToken ct)
        => SendAsync<T>(path, method, body, null, ct);

    public Task SendAsync(DiscordRoute path, HttpMethod method, object? body, CancellationToken ct)
        => SendAsync(path, method, body, null, ct);

    /// <inheritdoc />
    public async Task<T> SendAsync<T>(DiscordRoute path, HttpMethod method, object? body, AuthenticationHeaderValue? authOverride, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(method);

        using var res = await DispatchAsync(path, method, BuildFactory(path, method, body, authOverride), ct);
        if (res.IsSuccessStatusCode)
        {
            if (res.StatusCode == HttpStatusCode.NoContent)
                return default!;

            // Stream-based deserialization avoids materialising the response body as a string before parsing —
            // at fleet scale the duplicate allocation per response is meaningful GC pressure.
            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
            return data ?? throw new DiscordApiException(res.StatusCode, "Discord API returned empty JSON.", null);
        }

        throw await GetDiscordExceptionAsync(res, ct);
    }

    /// <inheritdoc />
    public async Task SendAsync(DiscordRoute path, HttpMethod method, object? body, AuthenticationHeaderValue? authOverride, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(method);

        using var res = await DispatchAsync(path, method, BuildFactory(path, method, body, authOverride), ct);
        if (res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.NoContent)
            return;

        throw await GetDiscordExceptionAsync(res, ct);
    }

    /// <summary>
    /// Builds the request factory used by the bucket queue. When <paramref name="authOverride"/>
    /// is non-null the override is applied to each freshly-built <see cref="HttpRequestMessage"/>,
    /// never to <see cref="HttpClient.DefaultRequestHeaders"/> — so concurrent calls using
    /// different auth schemes don't race on shared state.
    /// </summary>
    private Func<HttpRequestMessage> BuildFactory(DiscordRoute path, HttpMethod method, object? body, AuthenticationHeaderValue? authOverride)
    {
        if (authOverride is null)
            return () => CreateRequestWithBody(path, method, body);

        return () =>
        {
            var req = CreateRequestWithBody(path, method, body);
            req.Headers.Authorization = authOverride;
            return req;
        };
    }

    /// <summary>
    /// Sends a request to the Discord API that expects no content in the response.
    /// </summary>
    /// <param name="path">The API endpoint path.</param>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="DiscordApiException">Thrown when the API request fails.</exception>
    public async Task SendAsync(DiscordRoute path, HttpMethod method, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(method);

        using var res = await DispatchAsync(path, method, () => new HttpRequestMessage(method, path.ToString()), ct);

        if (res.IsSuccessStatusCode)
            return;

        throw await GetDiscordExceptionAsync(res, ct);
    }

    /// <summary>
    /// Acquires a slot from the process-wide concurrency gate, then runs the request through its
    /// per-bucket queue. Holding the gate slot for the whole request (gate wait + bucket wait +
    /// HTTP round trip) is what bounds the number of in-flight request states.
    /// </summary>
    private async Task<HttpResponseMessage> DispatchAsync(DiscordRoute path, HttpMethod method, Func<HttpRequestMessage> requestFactory, CancellationToken ct)
    {
        await _inflightGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await GetOrCreateBucket(path, method).ExecuteAsync(requestFactory, ct).ConfigureAwait(false);
        }
        finally
        {
            try { _inflightGate.Release(); }
            catch (ObjectDisposedException) { }
        }
    }

    internal HttpRequestMessage CreateRequestWithBody(DiscordRoute path, HttpMethod method, object? body)
    {
        var req = new HttpRequestMessage(method, path.ToString());

        switch (body)
        {
            case null:
                break;
            case Func<HttpContent> contentFactory:
                // Caller-supplied factory rebuilds the HttpContent on every attempt; required for
                // multipart uploads (single-use streams) and any retry-safe arbitrary content.
                req.Content = contentFactory();
                break;
            default:
                // Serialise straight to UTF-8 bytes — avoids the intermediate UTF-16 string +
                // re-encoding pass that StringContent would force.
                var bytes = JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
                req.Content = content;
                break;
        }

        return req;
    }

    private async Task<DiscordApiException> GetDiscordExceptionAsync(HttpResponseMessage res, CancellationToken ct)
    {
        DiscordApiError? error = null;
        try
        {
            var raw = await res.Content.ReadAsStringAsync(ct);
            error = DiscordErrorParser.Parse(raw);
        }
        catch
        {
        }

        return BuildException(res.StatusCode, res.ReasonPhrase, error);
    }

    /// <summary>
    /// Picks the right <see cref="DiscordApiException"/> subtype for a Discord failure response.
    /// Returns <see cref="InsufficientPermissionException"/> when Discord answered with HTTP 403
    /// and a JSON error code that signals the bot lacks the permission required for the
    /// operation. Two codes qualify per Discord's JSON Error Codes table:
    /// <c>50001 Missing Access</c> (bot cannot see/access the resource) and
    /// <c>50013 Missing Permissions</c> (bot is missing a specific permission for the call).
    /// Other 403 responses (token problems, age-gating, edit-others'-message constraints) stay
    /// as the generic <see cref="DiscordApiException"/>.
    /// </summary>
    /// <remarks>
    /// Source: Discord JSON Error Codes —
    /// <see href="https://discord.com/developers/docs/topics/opcodes-and-status-codes#json"/>.
    /// </remarks>
    internal static DiscordApiException BuildException(HttpStatusCode statusCode, string? httpReasonPhrase, DiscordApiError? error)
    {
        if (statusCode == HttpStatusCode.Forbidden && error?.Code is 50001 or 50013)
            return new InsufficientPermissionException(statusCode, httpReasonPhrase, error);

        return new DiscordApiException(statusCode, httpReasonPhrase, error);
    }


    /// <summary>
    /// Resolves the queue for a request using a two-level mapping:
    /// route → bucket-hash (learned from <c>X-RateLimit-Bucket</c>) → queue.
    /// Until the hash is known, the queue is keyed by the route itself.
    /// </summary>
    internal BucketRequestQueue GetOrCreateBucket(DiscordRoute path, HttpMethod method)
    {
        var bucketPath = path.GetBucketPath() ?? path.Template;
        var routeKey = $"{method.Method} {bucketPath}";
        var bucketKey = _routeToHash.TryGetValue(routeKey, out var knownHashKey) ? knownHashKey : routeKey;

        return _buckets.GetOrAdd(bucketKey, key => new BucketRequestQueue(
            _globalRateLimiter,
            _logger,
            _http,
            key,
            _shutdownCts.Token,
            _timeProvider,
            observedHash => OnBucketHashLearned(routeKey, bucketPath, observedHash)));
    }

    /// <summary>
    /// Registers the route → bucket-hash mapping discovered from a Discord response and migrates
    /// the existing route-keyed queue under the hash key so subsequent requests for unrelated
    /// routes that share the same hash converge on a single queue.
    /// </summary>
    internal void OnBucketHashLearned(string routeKey, string bucketPath, string observedHash)
    {
        var hashKey = $"{observedHash}:{bucketPath}";

        if (_routeToHash.TryGetValue(routeKey, out var existing) && existing == hashKey)
            return;

        _routeToHash[routeKey] = hashKey;

        if (!_buckets.TryGetValue(routeKey, out var queue))
            return;

        if (_buckets.TryAdd(hashKey, queue))
        {
            _buckets.TryRemove(new KeyValuePair<string, BucketRequestQueue>(routeKey, queue));
            queue.SetBucketName(hashKey);
        }
    }

    /// <summary>
    /// Background loop that periodically evicts <see cref="BucketRequestQueue"/> instances
    /// that have been idle for longer than <see cref="BucketEvictionIdleThreshold"/>. Without
    /// this sweeper, <see cref="_buckets"/> and <see cref="_routeToHash"/> accumulate one
    /// entry per unique <c>(method, route, major-id)</c> for the entire process lifetime —
    /// at fleet scale that is a linear memory leak and a parked worker task per slot.
    /// </summary>
    private async Task EvictionLoopAsync()
    {
        try
        {
            while (!_disposed)
            {
                await Task.Delay(BucketEvictionInterval, _timeProvider, _shutdownCts.Token).ConfigureAwait(false);
                if (_disposed)
                    return;

                try
                {
                    EvictIdleBuckets(BucketEvictionIdleThreshold);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning, ex, "Bucket eviction sweep failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose.
        }
    }

    /// <summary>
    /// Walks every bucket queue and removes those whose last activity was more than
    /// <paramref name="idleThreshold"/> ago. Returns the number of queues evicted.
    /// </summary>
    /// <remarks>
    /// Exposed as <c>internal</c> so tests can drive the sweep with a short threshold rather
    /// than waiting on the production interval.
    /// </remarks>
    internal int EvictIdleBuckets(TimeSpan idleThreshold)
    {
        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        var thresholdMs = (long)idleThreshold.TotalMilliseconds;
        var evicted = 0;

        foreach (var entry in _buckets)
        {
            if (nowMs - entry.Value.LastUsedAtMs < thresholdMs)
                continue;

            // Atomic remove keyed by both bucket-key and instance — guards against the
            // (very narrow) race where the queue was just replaced via OnBucketHashLearned.
            if (!_buckets.TryRemove(new KeyValuePair<string, BucketRequestQueue>(entry.Key, entry.Value)))
                continue;

            // Drop any route-to-hash mappings pointing at the evicted bucket so the next
            // request for the route gets a fresh queue rather than a dangling hash key.
            foreach (var routeEntry in _routeToHash)
            {
                if (routeEntry.Value == entry.Key)
                    _routeToHash.TryRemove(new KeyValuePair<string, string>(routeEntry.Key, routeEntry.Value));
            }

            entry.Value.Dispose();
            evicted++;
        }

        return evicted;
    }

    /// <summary>Test seam: number of live bucket queues.</summary>
    internal int BucketCount => _buckets.Count;

    /// <summary>Test seam: number of learned route-to-hash mappings.</summary>
    internal int RouteToHashCount => _routeToHash.Count;

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        // Wake the eviction loop so it does not sleep through shutdown.
        try { _shutdownCts.Cancel(); } catch (ObjectDisposedException) { }

        if (disposing)
        {
            // Dispose every bucket defensively — a single bucket throwing during Dispose must
            // not prevent the rest from being released, otherwise shutdown leaks worker tasks.
            foreach (var bucket in _buckets.Values)
            {
                try
                {
                    bucket.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning, ex, "Bucket dispose threw during shutdown");
                }
            }

            _buckets.Clear();
            _inflightGate.Dispose();
            _shutdownCts.Dispose();
            _http.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
