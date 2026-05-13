using Microsoft.Extensions.Logging;
using System.Net;

namespace DiscoSdk.Hosting.Rest.RateLimit;

/// <summary>
/// Serialises requests for a single Discord rate-limit bucket (one instance per
/// <c>bucket-hash + major-id</c>) and enforces, in order:
/// <list type="number">
///   <item>per-bucket serialisation — only one request at a time, so the rate-limit headers from
///   response <c>N</c> are read before request <c>N+1</c> is sent;</item>
///   <item>the per-bucket window — when the last response said the bucket was exhausted, the next
///   request waits for <c>X-RateLimit-Reset-After</c> to elapse;</item>
///   <item>the bot-wide global limit, via the shared <see cref="GlobalRateLimitManager"/>;</item>
///   <item>429 retries — bounded number of attempts, backing off for the reset window each time.</item>
/// </list>
/// <para>
/// Serialisation is a <see cref="SemaphoreSlim"/> with one permit — the calling task does the work
/// itself; there is no background worker, queue object, or per-request <see cref="TaskCompletionSource{TResult}"/>.
/// Transient transport failures (5xx, 408, network errors, attempt timeouts) are handled one layer
/// down by <see cref="TransientRetryPolicy"/>.
/// </para>
/// </summary>
internal sealed class BucketRequestQueue : IDisposable
{
    private const int MaxRateLimitRetries = 5;

    /// <summary>
    /// Fallback backoff applied to a 429 that arrived without a usable <c>X-RateLimit-Reset-After</c>
    /// header — conservative on purpose, better to wait an extra second than to spam Discord.
    /// </summary>
    private static readonly TimeSpan DefaultRateLimitBackoff = TimeSpan.FromSeconds(1);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly GlobalRateLimitManager _globalRateLimiter;
    private readonly HttpClient _http;
    private readonly Action<string>? _onHashLearned;
    private readonly ILogger _logger;

    private DateTimeOffset _resetTime;
    private string _bucket;
    private string? _learnedHash;
    private int _remainingRequests;
    private long _lastUsedAtTicks;
    private volatile bool _disposed;

    /// <param name="shutdownToken">
    /// Token owned by the <see cref="DiscordRestClient"/>; cancelling it (on full client disposal)
    /// cancels every request in flight here. The bucket also has its own linked source so an
    /// individual eviction (<see cref="Dispose"/>) cancels just this bucket's in-flight request.
    /// </param>
    public BucketRequestQueue(
        GlobalRateLimitManager globalRateLimiter,
        ILogger logger,
        HttpClient http,
        string bucket,
        CancellationToken shutdownToken,
        Action<string>? onHashLearned = null)
    {
        ArgumentNullException.ThrowIfNull(globalRateLimiter);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(http);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);

        _globalRateLimiter = globalRateLimiter;
        _http = http;
        _logger = logger;
        _bucket = bucket;
        _onHashLearned = onHashLearned;
        _lastUsedAtTicks = Environment.TickCount64;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
    }

    /// <summary>
    /// Updates the human-readable bucket name. Used when a route-keyed queue is migrated to a hash-keyed slot.
    /// </summary>
    internal void SetBucketName(string bucket) => _bucket = bucket;

    /// <summary>
    /// Monotonic <see cref="Environment.TickCount64"/> snapshot taken on the last
    /// <see cref="ExecuteAsync"/> call. Used by <see cref="DiscordRestClient"/>'s eviction sweeper.
    /// </summary>
    internal long LastUsedAtTicks => Interlocked.Read(ref _lastUsedAtTicks);

    public async Task<HttpResponseMessage> ExecuteAsync(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(requestFactory);

        Interlocked.Exchange(ref _lastUsedAtTicks, Environment.TickCount64);

        // Combine the caller's token with the bucket's shutdown-linked token. A linked source is
        // only allocated when the caller actually passed a cancellable token (the common internal
        // case is CancellationToken.None, where the bucket token alone is enough).
        if (cancellationToken.CanBeCanceled)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
            return await SendSerializedAsync(requestFactory, linked.Token).ConfigureAwait(false);
        }

        return await SendSerializedAsync(requestFactory, _cancellationTokenSource.Token).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendSerializedAsync(Func<HttpRequestMessage> requestFactory, CancellationToken token)
    {
        await _gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            return await SendWithRetriesAsync(requestFactory, token).ConfigureAwait(false);
        }
        finally
        {
            // The semaphore may have been disposed by a racing shutdown between WaitAsync
            // succeeding and this Release — releasing then is harmless to us.
            try { _gate.Release(); }
            catch (ObjectDisposedException) { }
        }
    }

    private async Task<HttpResponseMessage> SendWithRetriesAsync(Func<HttpRequestMessage> requestFactory, CancellationToken token)
    {
        // Wait out the local bucket window if the last response said it was exhausted. Bound to the
        // local clock via X-RateLimit-Reset-After (not the absolute X-RateLimit-Reset) so fleet
        // clock skew never affects the delay.
        var now = DateTimeOffset.UtcNow;
        if (_remainingRequests == 0 && _resetTime > now)
            await Task.Delay(_resetTime - now, token).ConfigureAwait(false);

        for (var attempt = 0; attempt < MaxRateLimitRetries; attempt++)
        {
            await _globalRateLimiter.WaitForGlobalAsync(token).ConfigureAwait(false);

            var response = await SendOnceAsync(requestFactory, token).ConfigureAwait(false);

            // Global 429 (X-RateLimit-Global): the manager has recorded the deadline and waited.
            if (await _globalRateLimiter.ReadAndWaitForGlobalAsync(response, token).ConfigureAwait(false))
            {
                response.Dispose();
                continue;
            }

            var rateLimit = ParseHeaders(response);
            _remainingRequests = rateLimit.Remaining ?? 0;
            _resetTime = rateLimit.ResetAfter is { } resetAfter
                ? DateTimeOffset.UtcNow + TimeSpan.FromSeconds(resetAfter)
                : DateTimeOffset.UtcNow;

            if (rateLimit.Bucket is { Length: > 0 } observedHash && observedHash != _learnedHash)
            {
                _learnedHash = observedHash;
                _onHashLearned?.Invoke(observedHash);
            }

            if (_remainingRequests == 0)
                _logger.Log(LogLevel.Warning, "Bucket {Bucket} rate limit reached. Resets in {ResetSeconds:F2}s.", _bucket, (_resetTime - DateTimeOffset.UtcNow).TotalSeconds);

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                return response;

            // Bucket-scope 429: back off for the reported window (or a small fixed delay if Discord
            // omitted the header) and retry.
            var retryDelay = rateLimit.ResetAfter is { } retryAfterSeconds
                ? TimeSpan.FromSeconds(retryAfterSeconds)
                : DefaultRateLimitBackoff;
            await Task.Delay(retryDelay, token).ConfigureAwait(false);
            response.Dispose();
        }

        throw new HttpRequestException("Exceeded maximum retry attempts due to rate limiting.");
    }

    private async Task<HttpResponseMessage> SendOnceAsync(Func<HttpRequestMessage> requestFactory, CancellationToken token)
    {
        return await TransientRetryPolicy.DefaultPipeline.ExecuteAsync(
            async ct =>
            {
                using var request = requestFactory();
                return await _http
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                    .ConfigureAwait(false);
            },
            token).ConfigureAwait(false);
    }

    private static DiscordRateLimitHeader ParseHeaders(HttpResponseMessage response)
    {
        return new DiscordRateLimitHeader(
            Bucket: response.GetString("X-RateLimit-Bucket"),
            Limit: response.GetInt("X-RateLimit-Limit"),
            Remaining: response.GetInt("X-RateLimit-Remaining"),
            ResetAfter: response.GetDouble("X-RateLimit-Reset-After"),
            Scope: response.GetString("X-RateLimit-Scope"));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Cancelling propagates to any request waiting on the gate or in flight here, so the caller
        // observes the cancellation immediately rather than hanging. The semaphore itself is left
        // undisposed: only the async wait path is used (no WaitHandle is ever materialised), and
        // disposing it would clear the async-waiter queue while those cancellations are still draining.
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
