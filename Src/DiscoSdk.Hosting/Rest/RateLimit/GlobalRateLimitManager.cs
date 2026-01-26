using System.Text.Json;

namespace DiscoSdk.Hosting.Rest.RateLimit;

/// <summary>
/// Synchronizes Discord's <b>global</b> REST rate limit across all requests.
/// <para>
/// When Discord responds with a global rate limit (HTTP 429 + <c>X-RateLimit-Global</c>),
/// this manager records a global "blocked until" timestamp and ensures all callers wait
/// until that moment has elapsed.
/// </para>
/// <para>
/// This is intentionally separate from per-route/per-bucket rate limiting.
/// </para>
/// </summary>
internal sealed class GlobalRateLimitManager
{
    private readonly SemaphoreSlim _globalGate = new(1, 1);

    private long _globalUntilMs;

    /// <summary>
    /// Waits until the current global rate limit window (if any) has elapsed.
    /// <para>
    /// This method is safe to call concurrently from many request pipelines. When a global rate limit
    /// is active, a single caller will sleep until the deadline; other callers are coordinated via
    /// <see cref="_globalGate"/> to avoid a thundering herd of delays.
    /// </para>
    /// <para>
    /// When no global limit is active, this method returns immediately without acquiring the semaphore.
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel waiting.</param>
    public async Task WaitForGlobalAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            // Fast-path: read the current deadline without taking the semaphore.
            // Volatile ensures we observe the latest value published by other threads.
            var untilMs = Volatile.Read(ref _globalUntilMs);
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (nowMs >= untilMs)
                return;

            // Coordinate the sleep so only one caller performs Task.Delay at a time.
            await _globalGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Re-check after acquiring the coordinator in case:
                // - another thread extended/shortened the deadline, or
                // - the deadline elapsed while waiting to acquire the semaphore.
                untilMs = Volatile.Read(ref _globalUntilMs);
                nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (nowMs >= untilMs)
                    return;

                var delayMs = (int)Math.Min(int.MaxValue, untilMs - nowMs);
                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _globalGate.Release();
            }
        }
    }

    /// <summary>
    /// Inspects an HTTP response and handles Discord's global rate limit (if present).
    /// <para>
    /// If the response is HTTP 429 and includes <c>X-RateLimit-Global</c>, this method reads
    /// the JSON body for <c>retry_after</c>, updates the global deadline, and then waits until
    /// the global window has elapsed.
    /// </para>
    /// </summary>
    /// <param name="message">The HTTP response to inspect.</param>
    /// <param name="cancellationToken">A token used to cancel reading or waiting.</param>
    /// <returns>
    /// <c>true</c> if a global rate limit was detected and waited for; otherwise <c>false</c>.
    /// </returns>
    public async Task<bool> ReadAndWaitForGlobalAsync(HttpResponseMessage message, CancellationToken cancellationToken = default)
    {
        if (message.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
            return false;

        var retryAfterSeconds = await GetGlobalRetryAfterSecondsAsync(message, cancellationToken).ConfigureAwait(false);
        if (retryAfterSeconds == null)
            return false;

        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var candidateUntilMs = nowMs + (long)Math.Ceiling(retryAfterSeconds.Value * 1000.0);

        // Ensure the global deadline is monotonic (never moves backwards).
        AtomicMax(candidateUntilMs);

        await WaitForGlobalAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Extracts the global retry delay from a Discord rate limit response.
    /// <para>
    /// Returns a value only when the response is explicitly marked as global via
    /// <c>X-RateLimit-Global</c> and the JSON body contains <c>retry_after</c>.
    /// </para>
    /// </summary>
    /// <param name="message">The HTTP response message.</param>
    /// <param name="token">A token used to cancel reading the response content.</param>
    /// <returns>
    /// The number of seconds to wait before retrying, or <c>null</c> when the response does not represent a global rate limit.
    /// </returns>
    private static async Task<double?> GetGlobalRetryAfterSecondsAsync(HttpResponseMessage message, CancellationToken token)
    {
        var globalHeader = message.GetString("X-RateLimit-Global");
        if (string.IsNullOrEmpty(globalHeader))
            return null;

        var raw = await message.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        var json = JsonDocument.Parse(raw);

        if (!json.RootElement.TryGetProperty("retry_after", out var retryAfterProp))
            return null;

        return retryAfterProp.GetDouble();
    }

    /// <summary>
    /// Atomically updates <see cref="_globalUntilMs"/> to <c>max(current, value)</c>.
    /// <para>
    /// This prevents the global deadline from moving backwards when multiple concurrent responses
    /// attempt to set the retry window.
    /// </para>
    /// </summary>
    /// <param name="value">The candidate Unix timestamp (milliseconds) to apply.</param>
    private void AtomicMax(long value)
    {
        while (true)
        {
            var current = Volatile.Read(ref _globalUntilMs);
            if (value <= current)
                return;

            var original = Interlocked.CompareExchange(ref _globalUntilMs, value, current);
            if (original == current)
                return;
        }
    }
}