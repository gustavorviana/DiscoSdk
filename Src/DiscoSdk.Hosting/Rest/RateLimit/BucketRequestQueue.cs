using DiscoSdk.Hosting.Observability;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
    private readonly InvalidRequestTracker _invalidRequestTracker;
    private readonly HttpClient _http;
    private readonly Action<string>? _onHashLearned;
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;

    private DateTimeOffset _resetTime;
    private string _bucket;
    private string? _learnedHash;
    private int _remainingRequests;
    private long _lastUsedAtMs;
    private volatile bool _disposed;

    /// <param name="shutdownToken">
    /// Token owned by the <see cref="DiscordRestClient"/>; cancelling it (on full client disposal)
    /// cancels every request in flight here. The bucket also has its own linked source so an
    /// individual eviction (<see cref="Dispose"/>) cancels just this bucket's in-flight request.
    /// </param>
    public BucketRequestQueue(
        GlobalRateLimitManager globalRateLimiter,
        InvalidRequestTracker invalidRequestTracker,
        ILogger logger,
        HttpClient http,
        string bucket,
        CancellationToken shutdownToken,
        TimeProvider timeProvider,
        Action<string>? onHashLearned = null)
    {
        ArgumentNullException.ThrowIfNull(globalRateLimiter);
        ArgumentNullException.ThrowIfNull(invalidRequestTracker);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(http);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _globalRateLimiter = globalRateLimiter;
        _invalidRequestTracker = invalidRequestTracker;
        _http = http;
        _logger = logger;
        _bucket = bucket;
        _onHashLearned = onHashLearned;
        _timeProvider = timeProvider;
        _lastUsedAtMs = timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
    }

    /// <summary>
    /// Updates the human-readable bucket name. Used when a route-keyed queue is migrated to a hash-keyed slot.
    /// </summary>
    internal void SetBucketName(string bucket) => _bucket = bucket;

    /// <summary>
    /// Monotonic milliseconds (from <see cref="TimeProvider.GetUtcNow"/>) snapshot taken on the last
    /// <see cref="ExecuteAsync"/> call. Used by <see cref="DiscordRestClient"/>'s eviction sweeper.
    /// </summary>
    internal long LastUsedAtMs => Interlocked.Read(ref _lastUsedAtMs);

    public async Task<HttpResponseMessage> ExecuteAsync(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(requestFactory);

        Interlocked.Exchange(ref _lastUsedAtMs, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());

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
        using var activity = DiscoSdkDiagnostics.ActivitySource.StartActivity(
            "discosdk.rest.request",
            System.Diagnostics.ActivityKind.Client);
        activity?.SetTag(DiagnosticTags.Route, _bucket);

        // Wait out the local bucket window if the last response said it was exhausted. Bound to the
        // local clock via X-RateLimit-Reset-After (not the absolute X-RateLimit-Reset) so fleet
        // clock skew never affects the delay.
        var now = _timeProvider.GetUtcNow();
        if (_remainingRequests == 0 && _resetTime > now)
            await Task.Delay(_resetTime - now, _timeProvider, token).ConfigureAwait(false);

        for (var attempt = 0; attempt < MaxRateLimitRetries; attempt++)
        {
            await _globalRateLimiter.WaitForGlobalAsync(token).ConfigureAwait(false);
            // Cloudflare safety pause — if we are near the 10k/10min invalid-request limit, hold
            // off until the window rolls so we do not push past the hard cap and earn an IP ban.
            await _invalidRequestTracker.WaitIfNearLimitAsync(token).ConfigureAwait(false);

            // HttpClient does not always reattach RequestMessage on the response after a custom
            // handler returns (test stubs in particular skip it), so observe the method directly
            // from inside the factory invocation rather than relying on response.RequestMessage.
            // Timestamp is taken off the bucket's TimeProvider so virtual-time tests measure
            // virtual elapsed time instead of wall-clock noise.
            HttpMethod? observedMethod = null;
            var sendStartedAt = _timeProvider.GetTimestamp();
            var response = await SendOnceAsync(requestFactory, token, m => observedMethod = m).ConfigureAwait(false);
            RecordResponseMetrics(response, observedMethod, sendStartedAt);

            _invalidRequestTracker.RecordResponse(response.StatusCode);

            // Global 429 (X-RateLimit-Global): the manager has recorded the deadline and waited.
            if (await _globalRateLimiter.ReadAndWaitForGlobalAsync(response, token).ConfigureAwait(false))
            {
                response.Dispose();
                continue;
            }

            var rateLimit = ParseHeaders(response);
            _remainingRequests = rateLimit.Remaining ?? 0;
            _resetTime = rateLimit.ResetAfter is { } resetAfter
                ? _timeProvider.GetUtcNow() + TimeSpan.FromSeconds(resetAfter)
                : _timeProvider.GetUtcNow();

            if (rateLimit.Bucket is { Length: > 0 } observedHash && observedHash != _learnedHash)
            {
                _learnedHash = observedHash;
                _onHashLearned?.Invoke(observedHash);
            }

            if (_remainingRequests == 0)
                _logger.Log(LogLevel.Warning, "Bucket {Bucket} rate limit reached. Resets in {ResetSeconds:F2}s.", _bucket, (_resetTime - _timeProvider.GetUtcNow()).TotalSeconds);

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                return response;

            DiscoSdkDiagnostics.RestRateLimited.Add(
                1,
                new KeyValuePair<string, object?>(DiagnosticTags.Route, _bucket),
                new KeyValuePair<string, object?>(DiagnosticTags.Scope, NormaliseScope(rateLimit.Scope)));

            // X-RateLimit-Scope = "shared" means the bucket is enforced across multiple resources
            // (e.g. all reactions on a channel share one budget). Hitting it tells us the cause
            // is not a single hot route — log at info so the operator can correlate spikes; user
            // and global already get logged elsewhere.
            if (string.Equals(rateLimit.Scope, "shared", StringComparison.OrdinalIgnoreCase))
                _logger.Log(LogLevel.Information, "Shared-scope 429 on bucket {Bucket}; budget is split with sibling resources.", _bucket);

            // Bucket-scope 429: prefer Discord's X-RateLimit-Reset-After (always seconds, never
            // null on a Discord-issued 429). When the 429 came from Cloudflare in front of Discord
            // — which only emits the standard Retry-After header — read that instead. Final
            // fallback is a small fixed backoff if neither header is parseable.
            var retryDelay = rateLimit.ResetAfter is { } resetAfterSeconds
                ? TimeSpan.FromSeconds(resetAfterSeconds)
                : ReadRetryAfterHeader(response) ?? DefaultRateLimitBackoff;
            await Task.Delay(retryDelay, _timeProvider, token).ConfigureAwait(false);
            response.Dispose();
        }

        throw new HttpRequestException("Exceeded maximum retry attempts due to rate limiting.");
    }

    /// <summary>
    /// Reads the standard HTTP <c>Retry-After</c> header from a 429 response. Discord uses
    /// <c>X-RateLimit-Reset-After</c> on rate-limit 429s, but Cloudflare in front of Discord may
    /// only emit the standard header — without parsing it we would default-backoff 1s and bang
    /// against Cloudflare's wall immediately.
    /// </summary>
    private static TimeSpan? ReadRetryAfterHeader(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter == null)
            return null;

        if (retryAfter.Delta is { } delta)
            return delta;

        if (retryAfter.Date is { } date)
        {
            var now = DateTimeOffset.UtcNow;
            return date > now ? date - now : TimeSpan.Zero;
        }

        return null;
    }

    /// <summary>
    /// Records request count + latency metrics for the just-completed HTTP attempt. The bucket
    /// name is used as the <see cref="DiagnosticTags.Route"/> tag — Discord's bucket key is a
    /// stable, low-cardinality identifier for a route family.
    /// </summary>
    private void RecordResponseMetrics(HttpResponseMessage response, HttpMethod? observedMethod, long sendStartedAt)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(sendStartedAt).TotalMilliseconds;
        var method = (observedMethod ?? response.RequestMessage?.Method)?.Method ?? "UNKNOWN";
        var statusCode = (int)response.StatusCode;

        DiscoSdkDiagnostics.RestRequests.Add(
            1,
            new KeyValuePair<string, object?>(DiagnosticTags.Route, _bucket),
            new KeyValuePair<string, object?>(DiagnosticTags.HttpMethod, method),
            new KeyValuePair<string, object?>(DiagnosticTags.HttpStatusClass, DiagnosticTags.ClassifyStatus(statusCode)));

        DiscoSdkDiagnostics.RestLatency.Record(
            elapsedMs,
            new KeyValuePair<string, object?>(DiagnosticTags.Route, _bucket),
            new KeyValuePair<string, object?>(DiagnosticTags.HttpMethod, method));
    }

    /// <summary>
    /// Discord omits the <c>X-RateLimit-Scope</c> header on bucket-scoped 429s — the implicit
    /// default is <c>user</c>. Normalising here keeps the <see cref="DiagnosticTags.Scope"/> tag
    /// to a stable closed set of values across dashboards.
    /// </summary>
    private static string NormaliseScope(string? scope)
        => string.IsNullOrWhiteSpace(scope) ? "user" : scope.ToLowerInvariant();

    private async Task<HttpResponseMessage> SendOnceAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken token,
        Action<HttpMethod>? methodObserver = null)
    {
        return await TransientRetryPolicy.DefaultPipeline.ExecuteAsync(
            async ct =>
            {
                using var request = requestFactory();
                methodObserver?.Invoke(request.Method);
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
