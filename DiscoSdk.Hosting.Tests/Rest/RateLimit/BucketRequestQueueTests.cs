using DiscoSdk.Hosting.Rest.RateLimit;
using DiscoSdk.Hosting.Tests.Rest;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace DiscoSdk.Hosting.Tests.Rest.RateLimit;

public class BucketRequestQueueTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();

    private static HttpResponseMessage Ok(string? bucketHash = null, int? remaining = null, double? resetAfter = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };

        if (bucketHash is not null)
            response.Headers.Add("X-RateLimit-Bucket", bucketHash);
        if (remaining.HasValue)
            response.Headers.Add("X-RateLimit-Remaining", remaining.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (resetAfter.HasValue)
            response.Headers.Add("X-RateLimit-Reset-After", resetAfter.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        return response;
    }

    private GlobalRateLimitManager NewGlobalLimiter() => new(_logger);

    private BucketRequestQueue NewQueue(
        HttpClient http,
        CancellationToken shutdownToken = default,
        Action<string>? onHashLearned = null) =>
        new(NewGlobalLimiter(), _logger, http, "test-bucket", shutdownToken, onHashLearned);

    private static HttpRequestMessage NewRequest() =>
        new(HttpMethod.Get, "https://discord.local/test");

    [Fact]
    public async Task ExecuteAsync_SuccessfulRequest_ReturnsResponseAsync()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(_ => Ok());
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);

        // Act
        var response = await queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, handler.RequestCount);
    }

    /// <summary>
    /// The bucket serialises with a one-permit semaphore: while one request is in flight, the
    /// others wait their turn — none is dropped, and all complete once the first finishes.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhileBusy_SerializesAndCompletesAllAsync()
    {
        // Arrange — the first request blocks until released; the others queue on the gate.
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;
        var handler = new StubHttpMessageHandler(async (_, _) =>
        {
            var n = Interlocked.Increment(ref requestCount);
            if (n == 1)
                await releaseFirst.Task;
            return Ok();
        });
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);

        // Act — three concurrent submissions.
        var first = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Wait until the first request has actually started executing.
        var spinUntil = DateTime.UtcNow.AddSeconds(5);
        while (Volatile.Read(ref requestCount) < 1 && DateTime.UtcNow < spinUntil)
            await Task.Delay(5);

        var second = queue.ExecuteAsync(NewRequest, CancellationToken.None);
        var third = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        await Task.Delay(100);
        Assert.False(first.IsCompleted, "First request should still be in flight");
        Assert.False(second.IsCompleted, "Second request should be waiting on the bucket gate");
        Assert.False(third.IsCompleted, "Third request should be waiting on the bucket gate");

        releaseFirst.SetResult();

        // Assert — every submitted request completes; nothing was dropped.
        await Task.WhenAll(first, second, third).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(3, handler.RequestCount);
    }

    /// <summary>
    /// Cancelling the caller's token while a request is waiting on the bucket gate (because a
    /// prior request is in flight) propagates as <see cref="OperationCanceledException"/> instead
    /// of hanging.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenCancelledWhileWaitingForGate_PropagatesCancellationAsync()
    {
        // Arrange — first request occupies the gate; handler won't return until released.
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new StubHttpMessageHandler(async (_, _) =>
        {
            await release.Task;
            return Ok();
        });
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);

        var first = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        var spinUntil = DateTime.UtcNow.AddSeconds(5);
        while (handler.RequestCount < 1 && DateTime.UtcNow < spinUntil)
            await Task.Delay(5);

        using var cts = new CancellationTokenSource();
        var second = queue.ExecuteAsync(NewRequest, cts.Token);

        await Task.Delay(50);
        Assert.False(second.IsCompleted, "Second submission should be waiting on the gate");

        // Act
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second)
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Cleanup
        release.SetResult();
        await first.WaitAsync(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Disposing the bucket while requests are pending (one in flight, others waiting on the gate)
    /// cancels every awaiter — the in-flight one via the cancelled HTTP token, the queued ones via
    /// the cancelled <c>WaitAsync</c>. This is the bot-shutdown path.
    /// </summary>
    [Fact]
    public async Task Dispose_WhilePendingRequests_CancelsEveryAwaiterAsync()
    {
        // Arrange — handler that only returns once released, but honours the request's token.
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new StubHttpMessageHandler(async (_, ct) =>
        {
            await release.Task.WaitAsync(ct);
            return Ok();
        });
        using var http = new HttpClient(handler);
        var queue = NewQueue(http);

        var first = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        var spinUntil = DateTime.UtcNow.AddSeconds(5);
        while (handler.RequestCount < 1 && DateTime.UtcNow < spinUntil)
            await Task.Delay(5);

        var second = queue.ExecuteAsync(NewRequest, CancellationToken.None);
        var third = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        await Task.Delay(50);
        Assert.False(first.IsCompleted, "First should still be in flight");
        Assert.False(second.IsCompleted, "Second should be waiting on the gate");
        Assert.False(third.IsCompleted, "Third should be waiting on the gate");

        // Act — simulate shutdown.
        queue.Dispose();

        // Assert — every pending awaiter observes cancellation.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first)
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second)
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => third)
            .WaitAsync(TimeSpan.FromSeconds(5));

        release.TrySetResult();
    }

    /// <summary>
    /// The bucket-hash discovery callback fires exactly once when the X-RateLimit-Bucket header is
    /// first observed, no matter how many subsequent responses carry the same hash.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenBucketHashHeaderObserved_InvokesCallbackOnceAsync()
    {
        // Arrange
        var observed = new List<string>();
        var handler = new StubHttpMessageHandler(_ => Ok(bucketHash: "abc-hash-123"));
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http, onHashLearned: hash =>
        {
            lock (observed) observed.Add(hash);
        });

        // Act
        await queue.ExecuteAsync(NewRequest, CancellationToken.None);
        await queue.ExecuteAsync(NewRequest, CancellationToken.None);
        await queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Assert — header observed three times, callback fired once.
        Assert.Equal(3, handler.RequestCount);
        Assert.Single(observed);
        Assert.Equal("abc-hash-123", observed[0]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoHashHeader_DoesNotInvokeCallbackAsync()
    {
        // Arrange
        var observed = new List<string>();
        var handler = new StubHttpMessageHandler(_ => Ok());
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http, onHashLearned: hash =>
        {
            lock (observed) observed.Add(hash);
        });

        // Act
        await queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Assert
        Assert.Empty(observed);
    }

    [Fact]
    public async Task ExecuteAsync_OnTransient429WithResetAfter_RetriesAndCompletesAsync()
    {
        // Arrange — first response is bucket-429 (no global header), second is 200.
        var responses = new Queue<HttpResponseMessage>(
        [
            CreateBucket429(resetAfterSeconds: 0.01),
            Ok()
        ]);
        var handler = new StubHttpMessageHandler(_ => responses.Dequeue());
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);

        // Act
        var response = await queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllRetriesAreRateLimited_ThrowsHttpRequestExceptionAsync()
    {
        // Arrange — every attempt is bucket-429 with a near-zero wait.
        var handler = new StubHttpMessageHandler(_ => CreateBucket429(resetAfterSeconds: 0.01));
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await queue.ExecuteAsync(NewRequest, CancellationToken.None));

        // The retry loop is bounded at 5 attempts.
        Assert.Equal(5, handler.RequestCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisposed_ThrowsObjectDisposedExceptionAsync()
    {
        // Arrange
        var handler = new StubHttpMessageHandler(_ => Ok());
        using var http = new HttpClient(handler);
        var queue = NewQueue(http);
        queue.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await queue.ExecuteAsync(NewRequest, CancellationToken.None));
    }

    /// <summary>
    /// The transient-failure retry policy promotes a flaky 5xx to a successful outcome without the
    /// bucket-queue retry loop being involved.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_OnTransient5xxFollowedBySuccess_RetriesAndCompletesAsync()
    {
        // Arrange — first response is 503 (transient), second is 200.
        var responses = new Queue<HttpResponseMessage>(
        [
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("{}") },
            Ok()
        ]);
        var handler = new StubHttpMessageHandler(_ => responses.Dequeue());
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);

        // Act
        var response = await queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Assert — Polly retried the 5xx and the eventual 200 was returned.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.RequestCount);
    }

    /// <summary>
    /// The transient policy does <b>not</b> retry on 429 — that path is owned by the bucket-queue
    /// retry loop. Otherwise the two layers would double-retry and burn the rate-limit budget.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_On429_TransientPolicyDoesNotRetryAsync()
    {
        // Arrange — first response is bucket-429 (no global header), second is 200.
        var responses = new Queue<HttpResponseMessage>(
        [
            CreateBucket429(resetAfterSeconds: 0.01),
            Ok()
        ]);
        var handler = new StubHttpMessageHandler(_ => responses.Dequeue());
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http);

        // Act
        var response = await queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Assert — exactly two requests: one 429, one OK. A transient retry on 429 would show >2.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.RequestCount);
    }

    private static HttpResponseMessage CreateBucket429(double resetAfterSeconds)
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent($$"""{"retry_after":{{resetAfterSeconds}}}""")
        };
        response.Headers.Add("X-RateLimit-Reset-After", resetAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        // No X-RateLimit-Global header → bucket-scope 429.
        return response;
    }
}
