using DiscoSdk.Hosting.Gateway.Payloads;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Gateway.Shards;

/// <summary>
/// Represents a single shard connection to the Discord Gateway.
/// </summary>
internal sealed class Shard(int shardId, DiscordClientConfig config, IShardPool pool)
{
    private readonly ShardSocket _socket = new(pool.DecompressFactory);
    private CancellationTokenRegistration _tokenRegistration;
    private ShardStatus _status = ShardStatus.PendingHello;
    private CancellationTokenSource? _heartbeatCts;
    private string? _resumeGatewayUrl = null;
    private bool _heartbeatAck = true;
    private int _heartbeatIntervalMs;
    private string? _sessionId = null;

    /// <summary>
    /// Gets the shard ID.
    /// </summary>
    public int ShardId => shardId;

    /// <summary>
    /// Gets the current status of the shard.
    /// </summary>
    public ShardStatus Status => _status;

    /// <summary>
    /// Starts the shard and establishes a connection to the Gateway.
    /// </summary>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task StartAsync()
    {
        if (_status is not ShardStatus.PendingHello and ShardStatus.ConnectionLost)
            return;

        _tokenRegistration.Dispose();
        _tokenRegistration = pool.CancellationToken.Register(async () => await StopAsync());

        await _socket.ConnectAsync(pool.GatewayUri.ToUri(), pool.CancellationToken);
        _ = RunLoopAsync();
    }

    /// <summary>
    /// Stops the shard and closes the Gateway connection.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public async Task StopAsync()
    {
        try { _tokenRegistration.Dispose(); } catch { }

        StopHeartbeat();
        await _socket.Close();
        _status = ShardStatus.PendingHello;
    }

    private async Task RunLoopAsync()
    {
        var token = pool.CancellationToken;
        while (!token.IsCancellationRequested)
        {
            try
            {
                await ReceiveLoopAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                SignalConnectionLost();
                await pool.OnConnectionLostAsync(this, ex);

                if (ex is DiscordSocketException)
                {
                    if (config.ReconnectDelay > TimeSpan.Zero)
                        await Task.Delay(config.ReconnectDelay, pool.CancellationToken).ConfigureAwait(false);

                    await ReconnectAsync();
                }
            }
        }
    }

    private async Task ReceiveLoopAsync()
    {
        var message = await _socket.ReadAsync(pool.CancellationToken);
        if (message == null) return;

        if (message.IsSystem())
        {
            await OnProcessSystemMessages(message);
            return;
        }

        if (message.Opcode == OpCodes.Dispatch)
            await OnDispatch(message);
    }

    private async Task OnProcessSystemMessages(ReceivedGatewayMessage message)
    {
        using var doc = message.ToJsonDocument();
        var payload = doc.RootElement;

        switch (message.Opcode)
        {
            case OpCodes.Hello:
                _heartbeatIntervalMs = payload.GetProperty("heartbeat_interval").GetInt32();

                await SetupIdentify();
                StartHeartbeat();
                break;

            case OpCodes.Heartbeat:
                await _socket.SendHeartbeatAsync(pool.CancellationToken);
                break;

            case OpCodes.HeartbeatAck:
                _heartbeatAck = true;
                break;

            case OpCodes.Reconnect:
            case OpCodes.InvalidSession:
                _status = ShardStatus.ConnectionLost;
                StopHeartbeat();
                await _socket.Close();
                if (config.ReconnectDelay > TimeSpan.Zero)
                    await Task.Delay(config.ReconnectDelay, pool.CancellationToken);

                SignalConnectionLost();
                var canReconnect = message.Opcode == OpCodes.Reconnect || payload.TryGetBoolean() == true;
                if (canReconnect && !string.IsNullOrEmpty(_sessionId) && !string.IsNullOrEmpty(_resumeGatewayUrl))
                {
                    await _socket.ConnectAsync(new Uri(_resumeGatewayUrl), pool.CancellationToken);
                    await _socket.ResumeAsync(config.Token, _sessionId, pool.CancellationToken);
                }
                else
                {
                    await ReconnectAsync();
                }
                break;
        }
    }

    private void SetReady()
    {
        if (_status != ShardStatus.PendingAck)
            return;

        _status = ShardStatus.Ready;
        pool.Gate.Release();
    }

    private void SignalConnectionLost()
    {
        if (_status == ShardStatus.Ready)
            pool.Gate.Release();

        _status = ShardStatus.ConnectionLost;
    }

    private async Task ReconnectAsync()
    {
        _sessionId = null;
        _resumeGatewayUrl = null;
        await _socket.ConnectAsync(pool.GatewayUri.ToUri(), pool.CancellationToken);
        await SetupIdentify();
    }

    private async Task OnDispatch(ReceivedGatewayMessage message)
    {
        if (string.Equals(message.EventType, "READY", StringComparison.Ordinal))
        {
            var obj = message.Deserialize<ReadyPayload>()!;
            _sessionId = obj.SessionId;
            _resumeGatewayUrl = obj.ResumeGatewayUrl;

            SetReady();
            await pool.OnReadyAsync(this, obj);
            return;
        }

        if (string.Equals(message.EventType, "RESUMED", StringComparison.Ordinal))
        {
            SetReady();
            await pool.OnResumeAsync(this);
            return;
        }

        await pool.OnReceiveMessageAsync(this, message);
    }

    private async Task SetupIdentify()
    {
        await pool.Gate.WaitAsync();
        _status = ShardStatus.PendingAck;
        await SendIdentifyAsync();
    }

    private void StartHeartbeat()
    {
        StopHeartbeat();

        _heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(pool.CancellationToken);
        _ = RunHeartbeatAsync();
    }

    private async Task RunHeartbeatAsync()
    {
        var token = _heartbeatCts?.Token ?? default;
        _heartbeatAck = false;

        await _socket.SendHeartbeatAsync(token);

        while (!token.IsCancellationRequested)
        {
            await Task.Delay(_heartbeatIntervalMs, token);

            if (!_heartbeatAck)
                throw new WebSocketException($"Shard {ShardId} missed HEARTBEAT_ACK.");

            _heartbeatAck = false;
            await _socket.SendHeartbeatAsync(token);
        }
    }

    private void StopHeartbeat()
    {
        if (_heartbeatCts == null)
            return;

        try { _heartbeatCts.Cancel(); } catch { }
        try { _heartbeatCts.Dispose(); } catch { }
        _heartbeatCts = null;
    }

    private Task SendIdentifyAsync()
        => _socket.SendAsync(OpCodes.Identify, new
        {
            token = config.Token,
            intents = (int)config.Intents,
            properties = DeviceInfo.CreateDefault()
        }
    , pool.CancellationToken);

    public Task SendAsync(OpCodes codes, object? data, CancellationToken cancellationToken = default)
        => _socket.SendAsync(new(codes, data), cancellationToken);
}