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
    private readonly GlobalRateLimitManager _globalRateLimiter = new();
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
    {
        if (string.IsNullOrWhiteSpace(botToken))
            throw new ArgumentException("Bot token is required.", nameof(botToken));

        if (timeout.HasValue)
            _http.Timeout = timeout.Value;

        _logger = logger;
        JsonOptions = jsonOptions;
        _http.BaseAddress = apiUri;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd($"{DeviceInfo.SdkName}/1.0");
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
        using var res = await GetOrCreateBucket(path).ExecuteAsync(() => CreateRequestWithBody(path, method, body), ct);
        if (res.IsSuccessStatusCode)
        {
            if (res.StatusCode == HttpStatusCode.NoContent)
                return default!;

            var result = await res.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<T>(result, JsonOptions);
            return data ?? throw new DiscordApiException("Discord API returned empty JSON.", res.StatusCode, null);
        }

        var error = await TryReadDiscordErrorAsync(res, ct);
        throw new DiscordApiException(
            error?.Message ?? $"Discord API error ({(int)res.StatusCode} {res.ReasonPhrase}).",
            res.StatusCode,
            error?.Code);
    }

    public async Task SendAsync(DiscordRoute path, HttpMethod method, object? body, CancellationToken ct)
    {
        using var res = await GetOrCreateBucket(path).ExecuteAsync(() => CreateRequestWithBody(path, method, body), ct);
        if (res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.NoContent)
            return;

        var error = await TryReadDiscordErrorAsync(res, ct);
        throw new DiscordApiException(
            error?.Message ?? $"Discord API error ({(int)res.StatusCode} {res.ReasonPhrase}).",
            res.StatusCode,
            error?.Code);
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
        using var res = await GetOrCreateBucket(path).ExecuteAsync(() => new HttpRequestMessage(method, path.ToString()), ct);

        if (res.IsSuccessStatusCode)
            return;

        var error = await TryReadDiscordErrorAsync(res, ct);
        throw new DiscordApiException(
            error?.Message ?? $"Discord API error ({(int)res.StatusCode} {res.ReasonPhrase}).",
            res.StatusCode,
            error?.Code);
    }

    private HttpRequestMessage CreateRequestWithBody(DiscordRoute path, HttpMethod method, object? body)
    {
        var req = new HttpRequestMessage(method, path.ToString());

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return req;
    }

    private async Task<DiscordErrorResponse?> TryReadDiscordErrorAsync(HttpResponseMessage res, CancellationToken ct)
    {
        try
        {
            var raw = await res.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            return JsonSerializer.Deserialize<DiscordErrorResponse>(raw, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private BucketRequestQueue GetOrCreateBucket(DiscordRoute path)
    {
        return _buckets.GetOrAdd(path.GetBucketPath() ?? string.Empty, _ => new BucketRequestQueue(_globalRateLimiter, _http, BucketQueueLimit));
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _http.Dispose();

        _disposed = true;
    }

    ~DiscordRestClient()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
