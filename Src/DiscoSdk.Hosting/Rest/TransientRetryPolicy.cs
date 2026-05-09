using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

namespace DiscoSdk.Hosting.Rest;

/// <summary>
/// Default resilience pipeline applied to every Discord HTTP attempt: per-attempt timeout,
/// transient-failure retry, and a circuit breaker that fails fast during a Discord outage.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline composes three Polly v8 strategies, executed outside-in on each call:
/// </para>
/// <list type="number">
///   <item><b>Retry</b> — 3 attempts, exponential backoff with jitter (200 ms base).
///   Triggers on transport-level failures, attempt timeouts, and 5xx/408 responses.
///   Discord 429s are handled separately by <see cref="RateLimit.BucketRequestQueue"/>;
///   retrying them at this layer would double-count attempts against the bucket window.</item>
///   <item><b>Circuit breaker</b> — opens when 50% of the last 10+ attempts in a 30 s
///   window failed. While open, calls fail fast with <see cref="BrokenCircuitException"/>
///   instead of hammering Discord. After 5 s the breaker enters half-open and lets a
///   single probe through to decide whether to close again.</item>
///   <item><b>Per-attempt timeout</b> — 30 s. Aborts a hung HTTP call (e.g. black-hole
///   network) by cancelling its <see cref="CancellationToken"/>. The retry layer treats
///   the resulting <see cref="TimeoutRejectedException"/> as transient.</item>
/// </list>
/// </remarks>
internal static class TransientRetryPolicy
{
    // --- Retry ---
    private const int RetryMaxAttempts = 3;
    private static readonly TimeSpan RetryBaseDelay = TimeSpan.FromMilliseconds(200);

    // --- Circuit breaker ---
    private const double BreakerFailureRatio = 0.5;
    private const int BreakerMinimumThroughput = 10;
    private static readonly TimeSpan BreakerSamplingDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan BreakerBreakDuration = TimeSpan.FromSeconds(5);

    // --- Timeout ---
    private static readonly TimeSpan PerAttemptTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Process-wide default pipeline shared by every <see cref="RateLimit.BucketRequestQueue"/>.
    /// </summary>
    public static readonly ResiliencePipeline<HttpResponseMessage> DefaultPipeline = BuildDefault();

    /// <summary>
    /// Builds a fresh pipeline with the default Discord-tuned configuration. Useful for tests
    /// that need an isolated circuit-breaker state separate from the process-wide default.
    /// </summary>
    public static ResiliencePipeline<HttpResponseMessage> BuildDefault()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = RetryMaxAttempts,
                BackoffType = DelayBackoffType.Exponential,
                Delay = RetryBaseDelay,
                UseJitter = true,
                ShouldHandle = static args => ValueTask.FromResult(IsTransient(args.Outcome)),
                OnRetry = static args =>
                {
                    args.Outcome.Result?.Dispose();
                    return default;
                },
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = BreakerFailureRatio,
                MinimumThroughput = BreakerMinimumThroughput,
                SamplingDuration = BreakerSamplingDuration,
                BreakDuration = BreakerBreakDuration,
                ShouldHandle = static args => ValueTask.FromResult(IsTransient(args.Outcome)),
                OnOpened = static args =>
                {
                    args.Outcome.Result?.Dispose();
                    return default;
                },
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = PerAttemptTimeout,
            })
            .Build();
    }

    private static bool IsTransient(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is HttpRequestException or TimeoutRejectedException)
            return true;

        if (outcome.Exception is not null)
            return false;

        if (outcome.Result is { } response)
        {
            var status = (int)response.StatusCode;
            return status >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }

        return false;
    }
}
