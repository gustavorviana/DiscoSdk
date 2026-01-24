using System.Text.Json;

namespace DiscoSdk.Hosting.Rest.RateLimit;

/// <summary>
/// Manages Discord's global rate limit synchronization across all REST requests.
/// Ensures that when a global rate limit is hit, all callers are blocked until
/// the retry window has elapsed.
/// </summary>
internal sealed class GlobalRateLimitManager
{
    /// <summary>
    /// Semaphore used as a global gate. When acquired, all callers attempting to
    /// pass through <see cref="WaitForGlobalAsync"/> will be blocked.
    /// </summary>
    private readonly SemaphoreSlim _globalGate = new(1, 1);

    /// <summary>
    /// Waits for the global rate limit gate to be available.
    /// If a global rate limit is currently active, this call will block until it is released.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    public async Task WaitForGlobalAsync(CancellationToken cancellationToken = default)
    {
        await _globalGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        _globalGate.Release();
    }

    /// <summary>
    /// Inspects an HTTP response and, if it represents a global rate limit hit (HTTP 429
    /// with <c>X-RateLimit-Global</c>), waits for the specified retry window before releasing
    /// all blocked callers.
    /// </summary>
    /// <param name="message">The HTTP response to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the wait operation.</param>
    /// <returns>
    /// <c>true</c> if a global rate limit was detected and waited for; otherwise <c>false</c>.
    /// </returns>
    public async Task<bool> ReadAndWaitForGlobalAsync(HttpResponseMessage message, CancellationToken cancellationToken = default)
    {
        if (message.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
            return false;

        var retryAfter = await GetGlobalRetryAfterSecondsAsync(message, cancellationToken);
        if (retryAfter == null)
            return false;

        var now = DateTimeOffset.UtcNow;
        await _globalGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var waitUntil = now.AddSeconds(retryAfter.Value);

            var delay = waitUntil - now;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, cancellationToken);

            return true;
        }
        finally
        {
            _globalGate.Release();
        }
    }

    /// <summary>
    /// Attempts to extract the global retry delay from a Discord rate limit response.
    /// Only returns a value if the response is marked as global via
    /// <c>X-RateLimit-Global</c> and contains a <c>retry_after</c> field in the body.
    /// </summary>
    /// <param name="message">The HTTP response message.</param>
    /// <param name="token">A token to cancel the read operation.</param>
    /// <returns>
    /// The number of seconds to wait before retrying, or <c>null</c> if the response
    /// does not represent a global rate limit.
    /// </returns>
    private static async Task<double?> GetGlobalRetryAfterSecondsAsync(HttpResponseMessage message, CancellationToken token)
    {
        var globalHeader = message.GetString("X-RateLimit-Global");
        if (string.IsNullOrEmpty(globalHeader))
            return null;

        var raw = await message.Content.ReadAsStringAsync(token);
        var json = JsonDocument.Parse(raw);
        if (!json.RootElement.TryGetProperty("retry_after", out var retryAfterProp))
            return null;

        return retryAfterProp.GetDouble();
    }
}