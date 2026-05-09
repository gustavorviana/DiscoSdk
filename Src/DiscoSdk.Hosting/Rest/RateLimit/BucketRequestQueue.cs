using DiscoSdk.Logging;
using System.Threading.Channels;

namespace DiscoSdk.Hosting.Rest.RateLimit;

internal sealed class BucketRequestQueue : IDisposable
{
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
    private bool _disposed;

    public BucketRequestQueue(
        GlobalRateLimitManager globalRateLimiter,
        ILogger logger,
        HttpClient http,
        string bucket,
        int bucketQueueLimit,
        Action<string>? onHashLearned = null)
    {
        _globalRateLimiter = globalRateLimiter ?? throw new ArgumentNullException(nameof(globalRateLimiter));
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger;
        _bucket = bucket;
        _onHashLearned = onHashLearned;

        _channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(bucketQueueLimit)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        _ = Task.Run(ProcessQueueAsync, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Updates the human-readable bucket name. Used when a route-keyed queue is migrated to a hash-keyed slot.
    /// </summary>
    internal void SetBucketName(string bucket) => _bucket = bucket;

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
            _logger?.Log(LogLevel.Error, $"Bucket \"{_bucket}\" queue loop terminated unexpectedly: {ex}");
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
            _resetTime = rateLimit.ResetAt;

            if (rateLimit.Bucket is { Length: > 0 } observedHash && observedHash != _learnedHash)
            {
                _learnedHash = observedHash;
                _onHashLearned?.Invoke(observedHash);
            }

            if (_remainingRequests == 0)
                _logger.Log(LogLevel.Warning, $"Bucket \"{_bucket}\" rate limit reached. Next reset at {_resetTime}.");

            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
            {
                workItem.Complete(response);
                return;
            }

            if (rateLimit.ResetAfter.HasValue)
                await Task.Delay(TimeSpan.FromSeconds(rateLimit.ResetAfter.Value), workItem.CancellationToken);

            response.Dispose();
        }

        workItem.TrySetException(new HttpRequestException("Exceeded maximum retry attempts due to rate limiting."));
    }

    private static DiscordRateLimitHeader ParseHeaders(HttpResponseMessage response)
    {
        var resetAfter = response.GetDouble("X-RateLimit-Reset-After");
        var reset = (long)((response.GetDouble("X-RateLimit-Reset") ?? 0) * 1000);
        return new DiscordRateLimitHeader
        {
            Bucket = response.GetString("X-RateLimit-Bucket"),
            Limit = response.GetInt("X-RateLimit-Limit"),
            Remaining = response.GetInt("X-RateLimit-Remaining"),
            Scope = response.GetString("X-RateLimit-Scope"),
            ResetAfter = resetAfter,
            ResetAt = DateTimeOffset.FromUnixTimeMilliseconds(reset)
        };
    }

    public async Task<HttpResponseMessage> ExecuteAsync(Func<HttpRequestMessage> request, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);

        var item = new WorkItem(request, cancellationToken, _cancellationTokenSource.Token);
        await _channel.Writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);

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
        public WorkItem(Func<HttpRequestMessage> requestFun, CancellationToken callerToken, CancellationToken queueToken)
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
            using var request = _requestFun();
            return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _linkedCts.Token);
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
    private void Dispose(bool disposing)
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

        if (disposing)
            _cancellationTokenSource.Dispose();
    }

    ~BucketRequestQueue()
    {
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}