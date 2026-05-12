using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DiscoSdk.Hosting.Rest.RateLimit;

internal sealed class BucketRequestQueue : IDisposable
{
    /// <summary>
    /// Fallback backoff applied to a 429 response that did not include a usable
    /// <c>X-RateLimit-Reset-After</c> header. Conservative on purpose — better to wait an
    /// extra second than to spam Discord during a malformed-response window.
    /// </summary>
    private static readonly TimeSpan DefaultRateLimitBackoff = TimeSpan.FromSeconds(1);

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly GlobalRateLimitManager _globalRateLimiter;
    private readonly Channel<WorkItem> _channel;
    private readonly HttpClient _http;
    private readonly Action<string>? _onHashLearned;
    private DateTimeOffset _resetTime;
    private readonly ILogger _logger;
    private string _bucket;
    private string? _learnedHash;
    private int _remainingRequests;
    private long _lastUsedAtTicks;
    private volatile bool _disposed;

    public BucketRequestQueue(
        GlobalRateLimitManager globalRateLimiter,
        ILogger logger,
        HttpClient http,
        string bucket,
        int bucketQueueLimit,
        Action<string>? onHashLearned = null)
    {
        ArgumentNullException.ThrowIfNull(globalRateLimiter);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(http);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentOutOfRangeException.ThrowIfLessThan(bucketQueueLimit, 1);

        _globalRateLimiter = globalRateLimiter;
        _http = http;
        _logger = logger;
        _bucket = bucket;
        _onHashLearned = onHashLearned;
        _lastUsedAtTicks = Environment.TickCount64;

        _channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(bucketQueueLimit)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        // Schedule the worker on the thread pool. The cancellation token argument is omitted
        // intentionally — Task.Run only checks it before the task starts, and the worker has
        // its own cancellation handling via _cancellationTokenSource inside the loop.
        _ = Task.Run(ProcessQueueAsync);
    }

    /// <summary>
    /// Updates the human-readable bucket name. Used when a route-keyed queue is migrated to a hash-keyed slot.
    /// </summary>
    internal void SetBucketName(string bucket) => _bucket = bucket;

    /// <summary>
    /// Monotonic <see cref="Environment.TickCount64"/> snapshot taken on the last
    /// <see cref="ExecuteAsync"/> call. Used by <see cref="DiscordRestClient"/>'s eviction
    /// sweeper to identify queues that have been idle long enough to be safely released.
    /// </summary>
    internal long LastUsedAtTicks => Interlocked.Read(ref _lastUsedAtTicks);

    private async Task ProcessQueueAsync()
    {
        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var workItemTask = await _channel.Reader.ReadAsync(_cancellationTokenSource.Token);

                if (workItemTask.IsCancellationRequested)
                {
                    // The caller's token (or the queue's shutdown token) was already cancelled —
                    // the WorkItem's Task is already faulted via the linked registration. Just
                    // release the registration and skip without spending a request slot.
                    workItemTask.DiscardCancelled();
                    continue;
                }

                try
                {
                    await CheckRemainingRequestsAsync(workItemTask);
                    await SendWorkItemAsync(workItemTask);
                }
                catch (Exception ex)
                {
                    workItemTask.TrySetException(ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose.
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "Bucket {Bucket} queue loop terminated unexpectedly", _bucket);
        }
    }

    private async Task CheckRemainingRequestsAsync(WorkItem workItem)
    {
        var now = DateTimeOffset.UtcNow;
        if (_remainingRequests == 0 && _resetTime > now)
        {
            var delay = _resetTime - now;
            await Task.Delay(delay, workItem.CancellationToken);
        }
    }

    private async Task SendWorkItemAsync(WorkItem workItem)
    {
        for (int i = 0; i < 5; i++)
        {
            await _globalRateLimiter.WaitForGlobalAsync(workItem.CancellationToken);

            var response = await workItem.SendAsync(_http);
            if (await _globalRateLimiter.ReadAndWaitForGlobalAsync(response, workItem.CancellationToken))
            {
                response.Dispose();
                continue;
            }

            var rateLimit = ParseHeaders(response);
            _remainingRequests = rateLimit.Remaining ?? 0;

            // Bind the reset deadline to the local clock using the relative X-RateLimit-Reset-After
            // header. The absolute X-RateLimit-Reset header would require client/Discord clocks to be
            // in sync — at fleet scale (multi-region, drifting clocks) that assumption is unsafe.
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

            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
            {
                workItem.Complete(response);
                return;
            }

            // Always back off before retrying a 429 — if Discord (or an intermediary) omitted
            // the Reset-After header, a small fixed delay is far safer than retrying immediately
            // and piling more 429s onto the bucket.
            var retryDelay = rateLimit.ResetAfter is { } retryAfterSeconds
                ? TimeSpan.FromSeconds(retryAfterSeconds)
                : DefaultRateLimitBackoff;
            await Task.Delay(retryDelay, workItem.CancellationToken);

            response.Dispose();
        }

        workItem.TrySetException(new HttpRequestException("Exceeded maximum retry attempts due to rate limiting."));
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

    public async Task<HttpResponseMessage> ExecuteAsync(Func<HttpRequestMessage> request, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);

        // Refresh the idle-tracking timestamp before any awaits so the eviction sweeper sees
        // the queue as recently active even if WriteAsync blocks under backpressure.
        Interlocked.Exchange(ref _lastUsedAtTicks, Environment.TickCount64);

        var item = new WorkItem(request, cancellationToken, _cancellationTokenSource.Token);
        try
        {
            await _channel.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // WriteAsync failed (cancellation while backpressured, channel completed, etc.) —
            // the WorkItem never reached the worker, so dispose its linked CTS / registration
            // here to avoid leaking them onto the caller's cancellation token.
            item.DiscardCancelled();
            throw;
        }

        return await item.Task.ConfigureAwait(false);
    }

    #region WorkerItem
    private sealed class WorkItem
    {
        private readonly TaskCompletionSource<HttpResponseMessage> _taskCompletionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Func<HttpRequestMessage> _requestFun;
        private readonly CancellationTokenSource _linkedCts;
        private readonly CancellationTokenRegistration _registration;

        /// <summary>
        /// Constructs a work item whose lifetime is bound to two cancellation sources:
        /// <list type="bullet">
        ///   <item>the <paramref name="callerToken"/> supplied by the caller of <see cref="ExecuteAsync"/>; and</item>
        ///   <item>the <paramref name="queueToken"/> owned by the <see cref="BucketRequestQueue"/> itself, which is
        ///   cancelled on dispose so a bot shutdown immediately fails every pending request.</item>
        /// </list>
        /// Both sources are linked into a single token; cancellation from either side faults the
        /// <see cref="TaskCompletionSource{TResult}"/> backing <see cref="Task"/> so the awaiting
        /// caller is never left hanging.
        /// </summary>
        public WorkItem(
            Func<HttpRequestMessage> requestFun,
            CancellationToken callerToken,
            CancellationToken queueToken)
        {
            _requestFun = requestFun;
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(callerToken, queueToken);

            _registration = _linkedCts.Token.Register(static state =>
            {
                ((TaskCompletionSource<HttpResponseMessage>)state!).TrySetCanceled();
            }, _taskCompletionSource);
        }

        public Task<HttpResponseMessage> Task => _taskCompletionSource.Task;

        public CancellationToken CancellationToken => _linkedCts.Token;

        public bool IsCancellationRequested => _linkedCts.IsCancellationRequested;

        public async Task<HttpResponseMessage> SendAsync(HttpClient httpClient)
        {
            // Transient failures (network errors, 5xx, 408) are handled by the process-wide
            // resilience pipeline. The request is rebuilt on every attempt because
            // HttpRequestMessage is single-use. Discord-specific 429 handling stays at the
            // bucket-queue level so it is never double-counted.
            return await TransientRetryPolicy.DefaultPipeline.ExecuteAsync(
                async ct =>
                {
                    using var request = _requestFun();
                    return await httpClient
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                        .ConfigureAwait(false);
                },
                _linkedCts.Token).ConfigureAwait(false);
        }

        public void Complete(HttpResponseMessage response)
        {
            // The caller may have already observed cancellation via the registered callback —
            // dispose the response so the underlying connection is returned to the pool promptly.
            if (!_taskCompletionSource.TrySetResult(response))
                response.Dispose();

            Cleanup();
        }

        public bool TrySetException(Exception ex)
        {
            var set = _taskCompletionSource.TrySetException(ex);
            Cleanup();
            return set;
        }

        /// <summary>
        /// Releases the linked cancellation registration when the worker discards a pre-cancelled
        /// item without invoking <see cref="Complete"/> or <see cref="TrySetException"/>.
        /// </summary>
        public void DiscardCancelled() => Cleanup();

        private void Cleanup()
        {
            _registration.Dispose();
            _linkedCts.Dispose();
        }
    }
    #endregion

    #region IDisposable
    public void Dispose()
    {
        if (_disposed)
            return;

        // Mark disposed first so concurrent ExecuteAsync calls fail fast with
        // ObjectDisposedException instead of attempting to read a token from a
        // CancellationTokenSource that is about to be disposed.
        _disposed = true;

        // Cancelling propagates through the linked tokens of every pending WorkItem,
        // faulting their TCS with OperationCanceledException so callers awaiting the
        // request observe shutdown immediately.
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
    #endregion
}