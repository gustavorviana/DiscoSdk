using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.RateLimit;
using Microsoft.Extensions.Logging;
using DiscoSdk.Rest;
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
    private const int BucketQueueLimit = 100;

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
    private volatile bool _disposed;

    public JsonSerializerOptions JsonOptions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordRestClient"/> class.
    /// </summary>
    /// <param name="botToken">The bot token for authentication.</param>
    /// <param name="apiUri">The base URI of the Discord API.</param>
    /// <exception cref="ArgumentException">Thrown when the bot token is null or whitespace.</exception>
    public DiscordRestClient(string botToken, Uri apiUri, JsonSerializerOptions jsonOptions, ILogger logger, TimeSpan? timeout = null)
    : this(apiUri, jsonOptions, logger, timeout)
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
    public DiscordRestClient(Uri apiUri, JsonSerializerOptions jsonOptions, ILogger logger, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(apiUri);
        ArgumentNullException.ThrowIfNull(jsonOptions);
        ArgumentNullException.ThrowIfNull(logger);

        if (timeout.HasValue)
            _http.Timeout = timeout.Value;

        _logger = logger;
        JsonOptions = jsonOptions;
        _http.BaseAddress = apiUri;
        _globalRateLimiter = new GlobalRateLimitManager(_logger);

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
    public async Task<T> SendAsync<T>(DiscordRoute path, HttpMethod method, object? body, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(method);

        using var res = await GetOrCreateBucket(path, method).ExecuteAsync(() => CreateRequestWithBody(path, method, body), ct);
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

    public async Task SendAsync(DiscordRoute path, HttpMethod method, object? body, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(method);

        using var res = await GetOrCreateBucket(path, method).ExecuteAsync(() => CreateRequestWithBody(path, method, body), ct);
        if (res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.NoContent)
            return;

        throw await GetDiscordExceptionAsync(res, ct);
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

        using var res = await GetOrCreateBucket(path, method).ExecuteAsync(() => new HttpRequestMessage(method, path.ToString()), ct);

        if (res.IsSuccessStatusCode)
            return;

        throw await GetDiscordExceptionAsync(res, ct);
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

        return new DiscordApiException(res.StatusCode, res.ReasonPhrase, error);
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
            BucketQueueLimit,
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
                await Task.Delay(BucketEvictionInterval, _shutdownCts.Token).ConfigureAwait(false);
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
        var nowTicks = Environment.TickCount64;
        var thresholdMs = (long)idleThreshold.TotalMilliseconds;
        var evicted = 0;

        foreach (var entry in _buckets)
        {
            if (nowTicks - entry.Value.LastUsedAtTicks < thresholdMs)
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
