namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Fixed-window rate limiter for Discord Gateway IDENTIFY.
/// Allows at most <paramref name="maxCalls"/> calls per <paramref name="window"/>.
/// Extra callers wait until the next window.
/// </summary>
public sealed class IdentifyGate : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _signal = new(0, int.MaxValue);
    private readonly object _lock = new();
    private readonly Timer _timer;
    private TimeSpan _window;
    private int _available;
    private int _maxCalls;
    private int _waiters;

    public CancellationToken Token => _cancellationTokenSource.Token;

    public IdentifyGate(int maxCalls, TimeSpan window)
    {
        Configure(maxCalls, window);

        _available = _maxCalls;

        _timer = new Timer(
            _ => OnWindowTick(),
            null,
            _window,
            _window);
    }

    /// <summary>
    /// Reconfigures the gate (safe to call at runtime).
    /// </summary>
    public void Configure(int maxCalls, TimeSpan window)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCalls);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);

        lock (_lock)
        {
            _maxCalls = maxCalls;
            _window = window;

            _timer?.Change(_window, _window);
        }
    }

    /// <summary>
    /// Waits until an IDENTIFY slot is available.
    /// </summary>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _cancellationTokenSource.Token);

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            lock (_lock)
            {
                if (_available > 0)
                {
                    _available--;
                    return;
                }

                _waiters++;
            }

            await _signal.WaitAsync(linked.Token).ConfigureAwait(false);
        }
    }

    private void OnWindowTick()
    {
        int toRelease;

        lock (_lock)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            _available = _maxCalls;

            toRelease = Math.Min(_waiters, _maxCalls);
            _waiters -= toRelease;
        }

        if (toRelease > 0)
            _signal.Release(toRelease);
    }

    public void Reset()
    {
        lock (_lock)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            var toRelease = Math.Min(_waiters, _maxCalls);
            if (toRelease > 0)
                _signal.Release(toRelease);

            _available = _maxCalls;
            _waiters = 0;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _timer.Dispose();
        _signal.Dispose();
        _cancellationTokenSource.Dispose();
    }
}
