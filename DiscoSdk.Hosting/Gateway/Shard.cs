using DiscoSdk.Hosting.Gateway.Payloads;
using System.Net.WebSockets;
using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway;

/// <summary>
/// Represents a single shard connection to the Discord Gateway.
/// </summary>
internal sealed class Shard(int shardId, string token, GatewayIntent intents, IdentifyGate identifyGate, Uri gatewayUri)
{
    /// <summary>
    /// Event raised when the shard successfully resumes a connection.
    /// </summary>
    public event ShardEventHandler? OnResume;

    /// <summary>
    /// Event raised when the shard receives a READY payload from the Gateway.
    /// </summary>
    public event ShardEventHandler<ReadyPayload>? OnReady;

    /// <summary>
    /// Event raised when the shard loses connection to the Gateway.
    /// </summary>
    public event ShardEventHandler<Exception>? ConnectionLost;

    /// <summary>
    /// Event raised when the shard receives a dispatch message from the Gateway.
    /// </summary>
    public event ShardEventHandler<ReceivedGatewayMessage>? OnReceiveMessage;

    private string? _resumeGatewayUrl = null;
    private string? _sessionId = null;

    /// <summary>
    /// Gets the shard ID.
    /// </summary>
    public int ShardId => shardId;

    /// <summary>
    /// Gets the current status of the shard.
    /// </summary>
    public ShardStatus Status => _status;

    private CancellationTokenSource? _heartbeatCts;
    private readonly ShardSocket _socket = new();

    private ShardStatus _status = ShardStatus.PendingHello;
    private bool _heartbeatAck = true;
    private int _heartbeatIntervalMs;
    private long? _seq;

    private CancellationTokenRegistration _tokenRegistration;

    /// <summary>
    /// Starts the shard and establishes a connection to the Gateway.
    /// </summary>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task StartAsync()
    {
        if (_status != ShardStatus.PendingHello)
            return;

        _tokenRegistration.Dispose();
        _tokenRegistration = identifyGate.Token.Register(async () => await StopAsync());

        await _socket.ConnectAsync(gatewayUri, identifyGate.Token);
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
        while (!identifyGate.Token.IsCancellationRequested)
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
                _status = ShardStatus.ConnectionLost;
                await InvokeEvent(ConnectionLost, ex);
            }
        }
    }

    private async Task ReceiveLoopAsync()
    {
        while (!identifyGate.IsCancellationRequested && _socket.Ready)
        {
            using var message = await _socket.ReadAsync(identifyGate.Token);
            if (message == null) continue;

            if (message.IsSystem())
            {
                if (message.SequenceNumber != null)
                    _seq = message.SequenceNumber;

                await OnProcessSystemMessages(message);
                continue;
            }

            if (message.Opcode == OpCodes.Dispatch)
                await OnDispatch(message);
        }
    }

    private async Task OnProcessSystemMessages(ReceivedGatewayMessage message)
    {
        switch (message.Opcode)
        {
            case OpCodes.Hello:
                _heartbeatIntervalMs = message.Payload.GetProperty("heartbeat_interval").GetInt32();

                await SetupIdentify();
                StartHeartbeat(identifyGate.Token);
                break;

            case OpCodes.Heartbeat:
                await SendHeartbeatAsync(identifyGate.Token);
                break;

            case OpCodes.HeartbeatAck:
                _heartbeatAck = true;
                break;

            case OpCodes.Reconnect:
            case OpCodes.InvalidSession:
                _status = ShardStatus.ConnectionLost;
                StopHeartbeat();
                await _socket.Close();
                await Task.Delay(5000);

                var canReconnect = message.Opcode == OpCodes.Reconnect || message.Payload.TryGetBoolean() == true;
                if (canReconnect && !string.IsNullOrEmpty(_sessionId) && !string.IsNullOrEmpty(_resumeGatewayUrl))
                {
                    await _socket.ConnectAsync(new Uri(_resumeGatewayUrl), identifyGate.Token);
                    await _socket.SendAsync(new(OpCodes.Resume, new
                    {
                        token,
                        session_id = _sessionId,
                        seq = _seq
                    }), identifyGate.Token);
                }
                else
                {
                    _sessionId = null;
                    _resumeGatewayUrl = null;
                    _seq = null;
                    await _socket.ConnectAsync(gatewayUri, identifyGate.Token);
                    await SetupIdentify();
                }
                break;
        }
    }

    private async Task OnDispatch(ReceivedGatewayMessage message)
    {
        if (string.Equals(message.EventType, "READY", StringComparison.Ordinal))
        {
            var obj = message.Payload.Deserialize<ReadyPayload>()!;
            _sessionId = obj.SessionId;
            _resumeGatewayUrl = obj.ResumeGatewayUrl;
            _status = ShardStatus.Ready;
            await InvokeEvent(OnReady, obj);
            return;
        }

        if (string.Equals(message.EventType, "RESUMED", StringComparison.Ordinal))
        {
            _status = ShardStatus.Ready;
            await InvokeEvent(OnResume);
            return;
        }

        await InvokeEvent(OnReceiveMessage, message);
    }

    private async Task SetupIdentify()
    {
        await identifyGate.WaitTurnAsync();
        await SendIdentifyAsync(identifyGate.Token);
    }

    private void StartHeartbeat(CancellationToken parentToken)
    {
        StopHeartbeat();

        _heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
        _ = RunHeartbeatAsync(_heartbeatCts.Token);
    }

    private async Task RunHeartbeatAsync(CancellationToken ct)
    {
        _heartbeatAck = false;
        await SendHeartbeatAsync(ct);

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(_heartbeatIntervalMs, ct);

            if (!_heartbeatAck)
                throw new WebSocketException($"Shard {ShardId} missed HEARTBEAT_ACK.");

            _heartbeatAck = false;
            await SendHeartbeatAsync(ct);
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

    private Task SendHeartbeatAsync(CancellationToken ct)
        => _socket.SendAsync(new(OpCodes.Heartbeat, _seq), ct);

    private Task SendIdentifyAsync(CancellationToken ct)
        => _socket.SendAsync(new(OpCodes.Identify, new
        {
            token,
            intents = (int)intents,
            properties = DeviceInfo.CreateDefault()
        }
    ), ct);

    private async Task InvokeEvent<TEventArgs>(ShardEventHandler<TEventArgs>? @event, TEventArgs arg)
    {
        if (@event != null)
            await @event.Invoke(this, arg);
    }

    private async Task InvokeEvent(ShardEventHandler? @event)
    {
        if (@event != null)
            await @event.Invoke(this);
    }
}