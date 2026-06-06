using System.Net;
using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Rest.RateLimit;

/// <summary>
/// Tracks "invalid" responses (HTTP 401 / 403 / 429) against Discord's Cloudflare-enforced
/// invalid-request budget — 10,000 such responses per 10 minutes per IP. Exceeding the budget
/// triggers a 1-hour Cloudflare IP ban that takes down every bot sharing the IP, regardless of
/// which one tripped it.
/// <para>
/// The tracker uses a fixed-window counter (matches what Cloudflare measures) and pauses new
/// requests when the count crosses <see cref="SafetyThreshold"/> — well before the hard cap so
/// in-flight requests still draining cannot push us over.
/// </para>
/// </summary>
internal sealed class InvalidRequestTracker
{
    /// <summary>Cloudflare's documented hard cap. Past this, IP gets banned for ~1 hour.</summary>
    public const int CloudflareInvalidLimit = 10_000;

    /// <summary>Soft cap — we pause sends when we hit this so concurrent in-flight responses
    /// finishing as invalid cannot stampede past the hard limit.</summary>
    public const int SafetyThreshold = 9_500;

    /// <summary>Width of the rolling window Cloudflare uses.</summary>
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    private readonly object _lock = new();
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;
    private long _windowStartUnixMs;
    private int _invalidCount;

    public InvalidRequestTracker(TimeProvider timeProvider, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>Current count of invalid responses observed in the active window.</summary>
    public int CurrentCount
    {
        get { lock (_lock) { RefreshWindow(); return _invalidCount; } }
    }

    /// <summary>
    /// Waits until the active window has room to take more invalid responses. Returns
    /// immediately when below <see cref="SafetyThreshold"/>; when at or above, blocks until the
    /// current window expires and the counter resets.
    /// </summary>
    public async Task WaitIfNearLimitAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TimeSpan wait;
            lock (_lock)
            {
                RefreshWindow();
                if (_invalidCount < SafetyThreshold)
                    return;

                var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
                var windowEndMs = _windowStartUnixMs + (long)Window.TotalMilliseconds;
                wait = TimeSpan.FromMilliseconds(Math.Max(1, windowEndMs - nowMs));
            }

            _logger.Log(LogLevel.Warning,
                "Cloudflare invalid-request safety threshold reached ({Count}/{Limit}); pausing for {WaitSeconds:F1}s.",
                _invalidCount, CloudflareInvalidLimit, wait.TotalSeconds);

            await Task.Delay(wait, _timeProvider, cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Records a response. Only <c>401</c>, <c>403</c>, and <c>429</c> count towards the limit —
    /// other status codes are no-ops.
    /// </summary>
    public void RecordResponse(HttpStatusCode statusCode)
    {
        if (!(statusCode is HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden
            or HttpStatusCode.TooManyRequests))
            return;

        lock (_lock)
        {
            RefreshWindow();
            _invalidCount++;
        }
    }

    private void RefreshWindow()
    {
        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        if (_windowStartUnixMs == 0 || nowMs >= _windowStartUnixMs + (long)Window.TotalMilliseconds)
        {
            _windowStartUnixMs = nowMs;
            _invalidCount = 0;
        }
    }
}
