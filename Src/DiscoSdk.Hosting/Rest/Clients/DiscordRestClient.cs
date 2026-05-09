using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.RateLimit;
using DiscoSdk.Logging;
using DiscoSdk.Rest;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
    private readonly HttpClient _http = new();
    private readonly ILogger _logger;
    private bool _disposed;

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
        using var res = await GetOrCreateBucket(path, method).ExecuteAsync(() => CreateRequestWithBody(path, method, body), ct);
        if (res.IsSuccessStatusCode)
        {
            if (res.StatusCode == HttpStatusCode.NoContent)
                return default!;

            var result = await res.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<T>(result, JsonOptions);
            return data ?? throw new DiscordApiException(res.StatusCode, "Discord API returned empty JSON.", null);
        }

        throw await GetDiscordExceptionAsync(res, ct);
    }

    public async Task SendAsync(DiscordRoute path, HttpMethod method, object? body, CancellationToken ct)
    {
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
            case HttpContent content:
                req.Content = content;
                break;
            case Func<HttpContent> contentFactory:
                req.Content = contentFactory();
                break;
            default:
                var json = JsonSerializer.Serialize(body, JsonOptions);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
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

        if (disposing)
        {
            foreach (var bucket in _buckets.Values)
                (bucket as IDisposable)?.Dispose();

            _buckets.Clear();
            _http.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
