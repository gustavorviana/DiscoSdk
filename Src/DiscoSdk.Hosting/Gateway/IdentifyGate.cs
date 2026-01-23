namespace DiscoSdk.Hosting.Gateway;

internal sealed class IdentifyGate : IDisposable
{
    private readonly List<TaskCompletionSource> _waiters = [];
    private int _pendingReleaseCount = 0;
    private int _maxConcurrency = 1;
    private bool _disposed;

    public int PendingReleaseCount => Interlocked.CompareExchange(ref _pendingReleaseCount, 0, 0);
    public int MaxConcurrency => Interlocked.CompareExchange(ref _maxConcurrency, 0, 0);

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        lock (_waiters)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            Interlocked.Increment(ref _pendingReleaseCount);
            if (PendingReleaseCount <= MaxConcurrency)
                return Task.CompletedTask;

            var taskSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Add(taskSource);

            return WaitWithCancellationAsync(taskSource, cancellationToken);
        }
    }

    private async Task WaitWithCancellationAsync(TaskCompletionSource taskCompletionSource, CancellationToken cancellationToken)
    {
        using (cancellationToken.Register(s => SignalReleased((TaskCompletionSource)s!, true, false), taskCompletionSource))
            await taskCompletionSource.Task;
    }

    public void SetMaxConcurrency(int newValue)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(newValue, 0, nameof(newValue));

        if (newValue == MaxConcurrency)
            return;

        Interlocked.Exchange(ref _maxConcurrency, newValue);

        lock (_waiters)
        {
            for (int i = _pendingReleaseCount; i < MaxConcurrency; i++)
            {
                if (_waiters.Count == 0)
                    break;

                _waiters[0].SetResult();
                _waiters.RemoveAt(0);
            }
        }
    }

    public void Release()
    {
        lock (_waiters)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (PendingReleaseCount > 0)
                SignalReleased(_waiters.FirstOrDefault(), false, true);
        }
    }

    private void SignalReleased(TaskCompletionSource? source, bool isCancelled, bool noLock)
    {
        if (PendingReleaseCount > 0)
            Interlocked.Decrement(ref _pendingReleaseCount);

        if (source == null)
            return;

        if (noLock)
        {
            _waiters.Remove(source);
        }
        else
        {
            lock (_waiters)
                _waiters.Remove(source);
        }

        if (isCancelled) source.SetCanceled();
        else source.SetResult();
    }

    #region IDisposable

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        lock (_waiters)
        {
            for (int i = _waiters.Count - 1; i >= 0; i--)
                _waiters[i].SetCanceled();

            _pendingReleaseCount = 0;
            _waiters.Clear();
        }
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

    #endregion
}