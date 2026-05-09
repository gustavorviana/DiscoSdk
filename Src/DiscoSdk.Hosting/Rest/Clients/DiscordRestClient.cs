using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.RateLimit;
using DiscoSdk.Logging;
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

    private readonly ConcurrentDictionary<string, BucketRequestQueue> _buckets = [];
    private readonly ConcurrentDictionary<string, string> _routeToHash = [];
    private readonly GlobalRateLimitManager _globalRateLimiter;

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
        _http.DefaultRequestHeaders.UserAgent.ParseAdd($"{DeviceInfo.SdkName}/1.0");
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

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

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
                    _logger.Log(LogLevel.Warning, $"Bucket dispose threw during shutdown: {ex}");
                }
            }

            _buckets.Clear();
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
