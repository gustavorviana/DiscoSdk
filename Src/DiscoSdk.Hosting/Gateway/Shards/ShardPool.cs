using DiscoSdk.Hosting.Gateway.Compression;
using DiscoSdk.Hosting.Gateway.Payloads;
using DiscoSdk.Hosting.Rest.Messages;

namespace DiscoSdk.Hosting.Gateway.Shards;

internal class ShardPool(IShardEventListener listener, DiscordClientConfig config) : IShardPool
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private int _totalShards = 0;
    private readonly List<Shard> _shards = [];

    public IReadOnlyList<Shard> Shards => _shards;

    public DiscordGatewayUri GatewayUri { get; private set; }

    /// <summary>
    /// Gets the total number of shards being used.
    /// </summary>
    public int TotalShards => _totalShards;

    public IdentifyGate Gate { get; } = new();

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public GatewayDecompressFactory DecompressFactory { get; } = new(config.GatewayCompressMode);

    public async Task InitShardsAsync()
    {
        await ClearShardsAsync();
        _shards.Clear();

        for (int i = 0; i < _totalShards; i++)
        {
            var shard = new Shard(i, config, this);
            _shards.Add(shard);
            await shard.StartAsync();
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
        if (_shards.Count == 0)
            return;

        for (int i = _shards.Count - 1; i >= 0; i--)
        {
            var shard = _shards[i];
            await shard.StopAsync();
            _shards.RemoveAt(i);
        }
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
}