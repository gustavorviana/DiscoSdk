using System.Threading.Channels;

namespace DiscoSdk.Hosting.Rest.RateLimit;

internal sealed class BucketRequestQueue : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly GlobalRateLimitManager _globalRateLimiter;
    private readonly Channel<WorkItem> _channel;
    private readonly HttpClient _http;
    private DateTimeOffset _resetTime;
    private int _remainingRequests;
    private bool _disposed;

    public BucketRequestQueue(GlobalRateLimitManager globalRateLimiter, HttpClient http, int bucketQueueLimit)
    {
        _globalRateLimiter ??= globalRateLimiter ?? throw new ArgumentNullException(nameof(globalRateLimiter));
        _http = http ?? throw new ArgumentNullException(nameof(http));

        _channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(bucketQueueLimit)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        _ = Task.Run(ProcessQueueAsync, _cancellationTokenSource.Token);
    }

    private async Task ProcessQueueAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var workItemTask = await _channel.Reader.ReadAsync(_cancellationTokenSource.Token);
            await _globalRateLimiter.WaitForGlobalAsync(workItemTask.CancellationToken);

            try
            {
                var now = DateTimeOffset.UtcNow;
                if (_remainingRequests == 0 && _resetTime > now)
                {
                    var delay = _resetTime - now;
                    await Task.Delay(delay, workItemTask.CancellationToken);
                }

                for (int i = 0; i < 5; i++)
                {
                    var response = await workItemTask.SendAsync(_http);
                    if (await _globalRateLimiter.ReadAndWaitForGlobalAsync(response))
                        continue;

                    var rateLimit = ParseHeaders(response);
                    _remainingRequests = rateLimit.Remaining ?? 0;
                    _resetTime = rateLimit.ResetAt;

                    if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                    {
                        workItemTask.Complete(response);
                        break;
                    }

                    if (rateLimit.ResetAfter.HasValue)
                        await Task.Delay(TimeSpan.FromSeconds(rateLimit.ResetAfter.Value));
                    continue;
                }

                workItemTask.TrySetException(new HttpRequestException("Exceeded maximum retry attempts due to rate limiting."));
            }
            catch (Exception ex)
            {
                workItemTask.TrySetException(ex);
            }
        }
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

    public Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);

        var item = new WorkItem(request, cancellationToken);
        _channel.Writer.TryWrite(item);

        return item.Task;
    }

    #region WorkerItem
    private class WorkItem(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        private readonly TaskCompletionSource<HttpResponseMessage> _taskCompletionSource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<HttpResponseMessage> Task => _taskCompletionSource.Task;

        public CancellationToken CancellationToken => cancellationToken;

        public async Task<HttpResponseMessage> SendAsync(HttpClient httpClient)
        {
            return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        public void Complete(HttpResponseMessage response) => _taskCompletionSource.TrySetResult(response);

        public bool TrySetException(Exception ex) => _taskCompletionSource.TrySetException(ex);
    }
    #endregion

    #region IDisposable
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _cancellationTokenSource.Cancel();

        if (disposing)
            _cancellationTokenSource.Dispose();

        _disposed = true;
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