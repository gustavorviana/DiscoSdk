using DiscoSdk.Hosting.Rest.RateLimit;
using DiscoSdk.Hosting.Tests.Rest;
using DiscoSdk.Logging;
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
        int capacity = 100,
        Action<string>? onHashLearned = null) =>
        new(NewGlobalLimiter(), _logger, http, "test-bucket", capacity, onHashLearned);

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
    /// Regression test for the silent-drop deadlock: TryWrite previously returned false
    /// when the bounded channel was full and the caller's Task never completed. With
    /// WriteAsync, surplus callers backpressure and complete once a slot opens.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenQueueFull_BackpressuresInsteadOfSilentlyDroppingAsync()
    {
        // Arrange — bound the channel to 1 slot so the third call exercises backpressure.
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
        using var queue = NewQueue(http, capacity: 1);

        // Act — three concurrent submissions
        var first = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Wait until the worker has picked up the first item.
        var spinUntil = DateTime.UtcNow.AddSeconds(5);
        while (Volatile.Read(ref requestCount) < 1 && DateTime.UtcNow < spinUntil)
            await Task.Delay(5);

        var second = queue.ExecuteAsync(NewRequest, CancellationToken.None);
        var third = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Allow time for any silent-drop bug to manifest before releasing.
        await Task.Delay(100);

        Assert.False(first.IsCompleted, "First request should still be in flight");
        Assert.False(second.IsCompleted, "Second request should be in queue");
        Assert.False(third.IsCompleted, "Third request should be backpressured behind the bound");

        releaseFirst.SetResult();

        // Assert — every submitted request completes; nothing was dropped.
        await Task.WhenAll(first, second, third).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(3, handler.RequestCount);
    }

    /// <summary>
    /// Regression test for the cancellation deadlock: a request that landed in the channel
    /// (WriteAsync already returned) and was waiting for the worker to pick it up could not
    /// be cancelled — the awaiter on <c>item.Task</c> hung forever because nothing wired the
    /// caller's token to the WorkItem's <see cref="TaskCompletionSource{T}"/>. With the fix,
    /// cancelling the token immediately faults the WorkItem's Task even before the worker
    /// reaches the item.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenCancelledWhileQueuedBehindBlockedWorker_PropagatesCancellationAsync()
    {
        // Arrange — generous capacity so the second submission is buffered without backpressure.
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new StubHttpMessageHandler(async (_, _) =>
        {
            await release.Task;
            return Ok();
        });
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http, capacity: 100);

        // First request occupies the worker (handler will not return until release).
        var first = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Wait for the worker to actually pick up the first request.
        var spinUntil = DateTime.UtcNow.AddSeconds(5);
        while (handler.RequestCount < 1 && DateTime.UtcNow < spinUntil)
            await Task.Delay(5);

        // Second request lands in the channel buffer behind the busy worker.
        using var cts = new CancellationTokenSource();
        var second = queue.ExecuteAsync(NewRequest, cts.Token);

        // Allow time to settle into the channel.
        await Task.Delay(50);
        Assert.False(second.IsCompleted, "Second submission should be queued behind the busy worker");

        // Act — cancel the token. Without the WorkItem cancellation registration, this would hang
        // because WriteAsync had already succeeded and there was no path to wake the awaiter.
        cts.Cancel();

        // Assert — second's task is observable as cancelled within a short window.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second)
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Cleanup — release the worker so the test does not leak a held request.
        release.SetResult();
        await first.WaitAsync(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Verifies the bot-shutdown scenario: when the queue is disposed while requests are
    /// pending (one in-flight on the worker, others buffered), every awaiter observes
    /// <see cref="OperationCanceledException"/>. Without the queue-token wiring on the
    /// WorkItem, the buffered items would hang forever because nothing else cancels them.
    /// </summary>
    [Fact]
    public async Task Dispose_WhilePendingRequests_CancelsEveryQueuedAwaiterAsync()
    {
        // Arrange — handler that never returns, so the worker stays busy and items pile up.
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new StubHttpMessageHandler(async (_, ct) =>
        {
            await release.Task.WaitAsync(ct);
            return Ok();
        });
        using var http = new HttpClient(handler);
        var queue = NewQueue(http, capacity: 100);

        // First occupies the worker.
        var first = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Wait for the worker to actually start the first request.
        var spinUntil = DateTime.UtcNow.AddSeconds(5);
        while (handler.RequestCount < 1 && DateTime.UtcNow < spinUntil)
            await Task.Delay(5);

        // Two more land in the channel buffer behind the busy worker.
        var second = queue.ExecuteAsync(NewRequest, CancellationToken.None);
        var third = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        await Task.Delay(50);
        Assert.False(first.IsCompleted, "First should still be in-flight");
        Assert.False(second.IsCompleted, "Second should be queued");
        Assert.False(third.IsCompleted, "Third should be queued");

        // Act — simulate bot shutdown by disposing the queue.
        queue.Dispose();

        // Assert — every pending awaiter observes cancellation, including the in-flight one.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first)
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second)
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => third)
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Allow the held handler to unwind without hanging the test runner.
        release.TrySetResult();
    }

    /// <summary>
    /// Verifies that cancelling the token while a request is backpressured on
    /// <c>WriteAsync</c> (because the channel is full) propagates as
    /// <see cref="OperationCanceledException"/> instead of hanging forever — the failure
    /// mode the previous TryWrite implementation produced.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenCancelledWhileBackpressured_PropagatesCancellationAsync()
    {
        // Arrange — capacity 1, worker held on the first request, buffer filled by the second.
        // The third submission will block on WriteAsync until a slot opens.
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new StubHttpMessageHandler(async (_, _) =>
        {
            await release.Task;
            return Ok();
        });
        using var http = new HttpClient(handler);
        using var queue = NewQueue(http, capacity: 1);

        var first = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Wait for the worker to actually start the first request.
        var spinUntil = DateTime.UtcNow.AddSeconds(5);
        while (handler.RequestCount < 1 && DateTime.UtcNow < spinUntil)
            await Task.Delay(5);

        var second = queue.ExecuteAsync(NewRequest, CancellationToken.None);

        // Give the second submission a moment to settle into the buffer.
        await Task.Delay(50);

        using var cts = new CancellationTokenSource();
        var third = queue.ExecuteAsync(NewRequest, cts.Token);

        // Confirm the third is waiting on backpressure rather than already completed.
        await Task.Delay(50);
        Assert.False(third.IsCompleted, "Third submission should be blocked on WriteAsync backpressure");

        // Act
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => third)
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Clean up the held requests so the test does not leak the worker thread.
        release.SetResult();
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Verifies the new bucket-hash discovery callback fires exactly once when the
    /// X-RateLimit-Bucket header is observed for the first time, regardless of how
    /// many subsequent responses carry the same hash.
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
        // Arrange — every attempt is bucket-429 with no wait
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
    /// Verifies the transient-failure retry policy promotes a flaky 5xx to a successful
    /// outcome without the bucket-queue retry loop being involved.
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
    /// Verifies the transient policy does <b>not</b> retry on 429 — that path is owned by the
    /// bucket-queue retry loop. Otherwise the two layers would double-retry and burn the
    /// rate-limit budget unnecessarily.
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

        // Assert — exactly two requests: one 429, one OK. If the transient policy were also
        // retrying on 429, we would see >2 attempts before the bucket loop saw a clean response.
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
