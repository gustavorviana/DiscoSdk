using DiscoSdk.Hosting.Rest;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;

namespace DiscoSdk.Hosting.Tests.Rest;

public class TransientRetryPolicyTests
{
    private static HttpResponseMessage Status(HttpStatusCode code) =>
        new(code) { Content = new StringContent("{}") };

    [Fact]
    public async Task Pipeline_PromotesTransient5xxToSuccess_AfterRetryAsync()
    {
        // Arrange — fresh pipeline so circuit breaker state does not leak across tests.
        var pipeline = TransientRetryPolicy.BuildDefault();
        var calls = 0;

        // Act
        var response = await pipeline.ExecuteAsync(_ =>
        {
            var n = Interlocked.Increment(ref calls);
            return ValueTask.FromResult(n < 3 ? Status(HttpStatusCode.InternalServerError) : Status(HttpStatusCode.OK));
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task Pipeline_DoesNotRetry429_BucketQueueOwnsThatPathAsync()
    {
        // Arrange
        var pipeline = TransientRetryPolicy.BuildDefault();
        var calls = 0;

        // Act
        var response = await pipeline.ExecuteAsync(_ =>
        {
            Interlocked.Increment(ref calls);
            return ValueTask.FromResult(Status(HttpStatusCode.TooManyRequests));
        });

        // Assert — exactly one call, 429 returned as-is.
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal(1, calls);
    }

    /// <summary>
    /// Verifies the circuit breaker opens after enough transient failures and short-circuits
    /// subsequent calls with <see cref="BrokenCircuitException"/>, sparing Discord during
    /// degraded windows.
    /// </summary>
    [Fact]
    public async Task Pipeline_OpensCircuitAfterRepeatedFailures_FailsFastAsync()
    {
        // Arrange — fresh pipeline. Default config: 50% failure ratio, min 10 throughput,
        // 30 s window. We force 100% failures over 16 attempts (4 logical calls × 4 attempts
        // each, since retry adds three retries on top of the initial call) so the breaker
        // crosses the threshold.
        var pipeline = TransientRetryPolicy.BuildDefault();

        async Task DriveOneFailingCallAsync()
        {
            try
            {
                await pipeline.ExecuteAsync(_ =>
                    ValueTask.FromResult(Status(HttpStatusCode.InternalServerError)));
            }
            catch (BrokenCircuitException)
            {
                // Expected once the breaker opens.
            }
        }

        // Drive enough failed attempts to trip the breaker.
        for (var i = 0; i < 4; i++)
            await DriveOneFailingCallAsync();

        // Act — the next call should short-circuit. We don't care whether the
        // BrokenCircuitException surfaces directly or is wrapped — just that we did not
        // get a normal 500 back, which would mean the breaker never opened.
        BrokenCircuitException? observed = null;
        try
        {
            await pipeline.ExecuteAsync(_ =>
                ValueTask.FromResult(Status(HttpStatusCode.InternalServerError)));
        }
        catch (BrokenCircuitException ex)
        {
            observed = ex;
        }

        // Assert
        Assert.NotNull(observed);
    }

    /// <summary>
    /// Verifies the per-attempt timeout cancels a hung HTTP call and the retry layer
    /// classifies <see cref="TimeoutRejectedException"/> as transient (so the next attempt
    /// gets a chance).
    /// </summary>
    [Fact]
    public async Task Pipeline_PerAttemptTimeout_CancelsHangingCall_RetryRecoversAsync()
    {
        // Arrange — build a custom pipeline with a short timeout so the test does not
        // depend on real-time delays of 30 s.
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 1,
                Delay = TimeSpan.Zero,
                ShouldHandle = static args => ValueTask.FromResult(
                    args.Outcome.Exception is TimeoutRejectedException),
            })
            .AddTimeout(TimeSpan.FromMilliseconds(50))
            .Build();

        var calls = 0;

        // Act — first attempt hangs longer than the timeout; second attempt completes fast.
        var response = await pipeline.ExecuteAsync(async ct =>
        {
            var n = Interlocked.Increment(ref calls);
            if (n == 1)
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            return Status(HttpStatusCode.OK);
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, calls);
    }
}
