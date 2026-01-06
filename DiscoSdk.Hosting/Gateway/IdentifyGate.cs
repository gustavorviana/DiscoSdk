namespace DiscoSdk.Hosting.Gateway;

public sealed class IdentifyGate : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _signal = new(0, int.MaxValue);
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private Timer? _timer;

    private int _availableInWindow;
    private TimeSpan _window;
    private bool _disposed;
    private int _maxCalls;
    private int _waiters;

    public CancellationToken Token => _cancellationTokenSource.Token;
    public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

    public IdentifyGate(int maxCalls, TimeSpan? window = null)
    {
        UpdateCheckValues(maxCalls, window);

        _availableInWindow = _maxCalls;

        _timer = new Timer(static s => ((IdentifyGate)s!).OnWindowTick(), this, _window, _window);
    }

    public void UpdateCheckValues(int maxCalls, TimeSpan? window = null)
    {
        _maxCalls = Math.Max(1, maxCalls);
        _window = window ?? TimeSpan.FromSeconds(5);
        if (_window <= TimeSpan.Zero) _window = TimeSpan.FromMilliseconds(1);
    }

    /// <summary>
    /// Fixed-window throttle: in each window (Y), allow at most X acquisitions.
    /// Extra callers block until a window boundary refills permits.
    /// </summary>
    public async Task WaitTurnAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            await _mutex.WaitAsync(Token);
            try
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                if (_availableInWindow > 0)
                {
                    _availableInWindow--;
                    return;
                }

                _waiters++;
            }
            finally
            {
                _mutex.Release();
            }

            await _signal.WaitAsync(Token);
        }
    }

    private void OnWindowTick()
    {
        _mutex.Wait();
        try
        {
            if (_disposed) return;

            _availableInWindow = _maxCalls;

            int toWake = _waiters;
            if (toWake <= 0) return;

            if (toWake > _maxCalls) toWake = _maxCalls;

            _waiters -= toWake;
            // wake outside mutex? Release is cheap; keep it simple and consistent.
            _signal.Release(toWake);
        }
        finally
        {
            _mutex.Release();
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _mutex.Wait();
        _cancellationTokenSource.Cancel();
        _mutex.Release();

        _signal.Dispose();
        _mutex.Dispose();

        if (disposing)
        {
            _cancellationTokenSource.Dispose();
            _timer?.Dispose();
            _timer = null;
        }

        _disposed = true;
    }

    ~IdentifyGate()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
