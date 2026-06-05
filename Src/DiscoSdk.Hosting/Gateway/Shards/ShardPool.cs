using DiscoSdk.Hosting.Gateway.Compression;
using DiscoSdk.Hosting.Gateway.Payloads;
using DiscoSdk.Hosting.Rest.Messages;

namespace DiscoSdk.Hosting.Gateway.Shards;

internal class ShardPool(IShardEventListener listener,
                        DiscordClientConfig config,
                        IGatewaySocketFactory socketFactory,
                        TimeProvider timeProvider) : IShardPool, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    // Serialises lifecycle workflows (Init / Clear / Reconnect) that span awaits — prevents
    // two ops from interleaving their tear-down and rebuild work.
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    // Protects individual List<Shard> operations against the public Shards getter. Held only
    // for nanoseconds (Add / Clear / ToArray) so a reader never blocks on async lifecycle work.
    private readonly object _shardsLock = new();
    private int _totalShards = 0;
    private readonly List<Shard> _shards = [];
    private bool _disposed;

    /// <summary>
    /// Snapshot of the current shard list. Readers (metrics, IsReady checks) take only the
    /// short list-mutation lock, never the lifecycle semaphore, so a long Init / Clear that
    /// dials WebSockets does not block a dashboard polling <c>client.Shards.Count</c>.
    /// </summary>
    public IReadOnlyList<Shard> Shards
    {
        get
        {
            lock (_shardsLock)
                return _shards.ToArray();
        }
    }

    public DiscordGatewayUri GatewayUri { get; private set; }

    /// <summary>
    /// Gets the total number of shards being used.
    /// </summary>
    public int TotalShards => _totalShards;

    public IdentifyGate Gate { get; } = new();

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public IGatewaySocketFactory SocketFactory { get; } = socketFactory ?? throw new ArgumentNullException(nameof(socketFactory));

    public TimeProvider TimeProvider { get; } = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    public async Task InitShardsAsync()
    {
        await _lifecycleLock.WaitAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        try
        {
            await ClearShardsInternalAsync().ConfigureAwait(false);

            for (int i = 0; i < _totalShards; i++)
            {
                var shard = new Shard(i, config, this);
                // _shardsLock only — concurrent reader sees a consistent snapshot, but is not
                // blocked by the await shard.StartAsync() that follows.
                lock (_shardsLock) _shards.Add(shard);
                await shard.StartAsync();
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>
    /// Test-only seam — seeds the shard list without going through the gateway-info fetch or
    /// connecting the sockets. Lets action tests that need <see cref="GetShardForGuild"/> get
    /// back a shard whose <c>SendAsync</c> writes to the fake socket.
    /// </summary>
    internal void SeedShardsForTests(int totalShards)
    {
        _totalShards = Math.Max(totalShards, 1);
        lock (_shardsLock)
        {
            _shards.Clear();
            for (int i = 0; i < _totalShards; i++)
                _shards.Add(new Shard(i, config, this));
        }
    }

    public void SetGateway(DiscordGatewayInfo gatewayInfo)
    {
        _totalShards = Math.Max(config.TotalShards ?? gatewayInfo.Shards, 1);
        Gate.SetMaxConcurrency(gatewayInfo.SessionInfo.MaxConcurrency);
        GatewayUri = new DiscordGatewayUri(gatewayInfo.Url, compress: config.GatewayCompressMode == GatewayCompressMode.ZlibStream ? "zlib-stream" : null);
    }

    public async Task ClearShardsAsync()
    {
        await _lifecycleLock.WaitAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        try { await ClearShardsInternalAsync().ConfigureAwait(false); }
        finally { _lifecycleLock.Release(); }
    }

    /// <summary>
    /// Lock-free version of <see cref="ClearShardsAsync"/> for callers that already hold
    /// <see cref="_lifecycleLock"/> (currently just <see cref="InitShardsAsync"/>).
    /// SemaphoreSlim is not re-entrant, so calling ClearShardsAsync from within would deadlock.
    /// </summary>
    private async Task ClearShardsInternalAsync()
    {
        Shard[] snapshot;
        lock (_shardsLock) snapshot = [.._shards];
        if (snapshot.Length == 0)
            return;

        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            await snapshot[i].StopAsync();
            snapshot[i].Dispose();
        }

        lock (_shardsLock) _shards.Clear();
    }

    public async Task OnConnectionLostAsync(Shard shard, Exception exception)
    {
        try
        {
            await listener.OnConnectionLostAsync(shard, exception);
        }
        catch (Exception ex)
        {
            OnUnhandledError(ex);
        }
    }

    public async Task OnFatalAsync(Shard shard, Exception exception)
    {
        try
        {
            await listener.OnFatalAsync(shard, exception);
        }
        catch (Exception ex)
        {
            OnUnhandledError(ex);
        }
    }

    public async Task OnReconnectingAsync(Shard shard, int attempt, TimeSpan delay, bool isResume)
    {
        try
        {
            await listener.OnReconnectingAsync(shard, attempt, delay, isResume);
        }
        catch (Exception ex)
        {
            OnUnhandledError(ex);
        }
    }

    public async Task OnReadyAsync(Shard shard, ReadyPayload payload)
    {
        try
        {
            await listener.OnReadyAsync(shard, payload);
        }
        catch (Exception ex)
        {
            OnUnhandledError(ex);
        }
    }

    public async Task OnReceiveMessageAsync(Shard shard, ReceivedGatewayMessage message)
    {
        try
        {
            await listener.OnReceiveMessageAsync(shard, message);
        }
        catch (Exception ex)
        {
            OnUnhandledError(ex);
        }
    }

    public async Task OnResumeAsync(Shard shard)
    {
        try
        {
            await listener.OnResumeAsync(shard);
        }
        catch (Exception ex)
        {
            OnUnhandledError(ex);
        }
    }

    public void OnUnhandledError(Exception exception) => listener.OnUnhandledError(exception);

    /// <summary>
    /// Cancels the pool-wide CTS (so any shard still running observes it), disposes the gate,
    /// the lifecycle semaphore, and the CTS. <see cref="ClearShardsAsync"/> should be called
    /// before this so the shards close their sockets gracefully; calling Dispose without it
    /// is still safe (the cancellation will tear them down) but skips the WebSocket close
    /// handshake.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        try { _cancellationTokenSource.Cancel(); } catch (ObjectDisposedException) { }
        _cancellationTokenSource.Dispose();
        _lifecycleLock.Dispose();
        Gate.Dispose();
    }
}
