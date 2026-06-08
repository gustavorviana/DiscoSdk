using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Hosting.Gateway.Payloads;
using DiscoSdk.Hosting.Observability;
using System.Net.WebSockets;

namespace DiscoSdk.Hosting.Gateway.Shards;

/// <summary>
/// Represents a single shard connection to the Discord Gateway.
/// </summary>
internal sealed class Shard : IShard, IDisposable
{
    private readonly int _shardId;
    private readonly DiscordClientConfig _config;
    private readonly IShardPool _pool;
    private readonly IGatewaySocket _socket;

    public Shard(int shardId, DiscordClientConfig config, IShardPool pool)
    {
        _shardId = shardId;
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _socket = pool.SocketFactory.Create();
        _dispatcher = new ShardEventDispatcher(
            shardId,
            Math.Max(1, config.EventProcessorQueueCapacity),
            message => pool.OnReceiveMessageAsync(this, message));
    }
    private CancellationTokenRegistration _tokenRegistration;
    private ShardStatus _status = ShardStatus.Disconnected;
    private CancellationTokenSource? _heartbeatCts;
    // Per-shard CTS that StopAsync cancels. Combined with pool.CancellationToken via a linked
    // source inside RunLoopAsync, so stopping one shard does not require tearing down the pool.
    private CancellationTokenSource _shardCts = new();
    // Tracks the fire-and-forget RunLoopAsync so StopAsync can await its exit before Dispose
    // tears down state the loop still touches (the CTS, the socket reference).
    private Task? _runLoopTask;
    private string? _resumeGatewayUrl = null;
    // Touched by the receive loop (set true on HEARTBEAT_ACK) and the heartbeat task (set false
    // before each send, read on each interval tick). Volatile.Read/Write gives the cross-thread
    // visibility we need without a lock; Interlocked is unnecessary because writes are simple
    // assignments, not RMW.
    private bool _heartbeatAck = true;
    // TimeProvider timestamp captured immediately before each outbound heartbeat. Read by the
    // HEARTBEAT_ACK handler to compute round-trip latency and record on
    // DiscoSdkDiagnostics.GatewayHeartbeatLatency. Uses pool.TimeProvider so virtual-time tests
    // see virtual elapsed-time instead of real wall-clock noise.
    // -1 means "no heartbeat sent yet" (skips record).
    private long _heartbeatSentTimestamp = -1;
    private int _heartbeatIntervalMs;
    private string? _sessionId = null;
    // Faults raised by the fire-and-forget heartbeat task are parked here so the
    // receive loop can observe them and route the recovery through RunLoopAsync's catch.
    private Exception? _pendingTransportFault;
    // Set by the OP 7 / OP 9-resumable handler before closing the socket; consulted by
    // ReconnectWithBackoffAsync to choose Resume over fresh Identify on the next attempt.
    private bool _preferResume;
    // Set after the resume reconnect's ConnectAsync; the Hello handler reads this to decide
    // whether to send RESUME (true) or IDENTIFY (false). Discord requires either payload to be
    // sent in response to Hello, not before.
    private bool _resumeOnNextHello;
    // Per-shard event dispatcher. The receive loop only writes here; a dedicated worker task
    // drains the channel and forwards to the pool/listener serially, so two events from the same
    // shard (and therefore from the same guild) never race against each other. The receive loop
    // never blocks on a handler — when the queue fills, the writer awaits and backpressure flows
    // back through the WebSocket consumer naturally. Heartbeat is on its own timer and unaffected.
    // Cannot use a field initializer because the dispatcher's processor captures `this`; the
    // constructor below assigns it instead.
    private readonly ShardEventDispatcher _dispatcher;

    /// <inheritdoc />
    public int Id => _shardId;

    /// <inheritdoc />
    public ShardStatus Status => _status;

    /// <inheritdoc />
    public bool IsReady => _status == ShardStatus.Ready;

    /// <summary>
    /// Starts the shard and establishes a connection to the Gateway. The initial WebSocket dial
    /// is retried with exponential backoff so a Discord outage at deploy time does not crash the
    /// bot — matches the standard pattern and lets K8s rolling deploys wait the API out.
    /// </summary>
    public async Task StartAsync()
    {
        if (_status is not ShardStatus.Disconnected)
            return;

        // A CTS cannot be reused after it is cancelled — rebuild for a new lifecycle. Disposed
        // because the CTS holds a Timer registration that leaks otherwise.
        if (_shardCts.IsCancellationRequested)
        {
            _shardCts.Dispose();
            _shardCts = new CancellationTokenSource();
        }

        _tokenRegistration.Dispose();
        _tokenRegistration = _pool.CancellationToken.Register(static s => ((Shard)s!)._shardCts.Cancel(), this);

        _status = ShardStatus.Connecting;
        await ConnectInitialWithRetryAsync().ConfigureAwait(false);
        _runLoopTask = RunLoopAsync();
    }

    /// <summary>
    /// Wraps the initial <see cref="IGatewaySocket.ConnectAsync"/> in the same exponential
    /// backoff used by <see cref="ReconnectWithBackoffAsync"/>. Without this, a Discord outage at
    /// bot startup crashes <see cref="StartAsync"/> and the pod ends up in K8s CrashLoopBackoff.
    /// Cancellation is honoured — <see cref="StopAsync"/> mid-retry exits cleanly.
    /// </summary>
    private async Task ConnectInitialWithRetryAsync()
    {
        var attempt = 0;
        while (!_shardCts.IsCancellationRequested)
        {
            try
            {
                await _socket.ConnectAsync(_pool.GatewayUri.ToUri(), _shardCts.Token).ConfigureAwait(false);
                return;
            }
            catch (OperationCanceledException) when (_shardCts.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (GatewayExceptions.IsRecoverableTransport(ex)
                                       || ex is System.Net.Sockets.SocketException
                                       || ex is HttpRequestException)
            {
                attempt++;

                if (_config.MaxReconnectAttempts > 0 && attempt >= _config.MaxReconnectAttempts)
                {
                    // Propagate the last failure so Pool.InitShardsAsync — and therefore
                    // DiscordClient.StartAsync — fail with the cause instead of looping forever.
                    throw new InvalidOperationException(
                        $"Shard {Id} could not connect after {attempt} attempts.", ex);
                }

                var delay = ComputeBackoffDelay(attempt);
                await _pool.OnReconnectingAsync(this, attempt, delay, isResume: false).ConfigureAwait(false);
                await Task.Delay(delay, _pool.TimeProvider, _shardCts.Token).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Stops the shard and closes the Gateway connection. Cancelling the per-shard CTS is what
    /// actually makes <see cref="RunLoopAsync"/> exit even when <c>AutoReconnect</c> is on — the
    /// retry loop respects the same token.
    /// </summary>
    public async Task StopAsync()
    {
        try { _tokenRegistration.Dispose(); } catch { }
        try { _shardCts.Cancel(); } catch (ObjectDisposedException) { }

        StopHeartbeat();
        await _socket.Close();

        // Wait for RunLoopAsync to actually exit. Without this, ClearShardsAsync calls
        // Dispose() immediately after StopAsync — disposing _shardCts and _socket while the
        // run loop is still in its catch path reading them, surfacing as a spurious
        // ObjectDisposedException routed through OnFatalAsync.
        if (_runLoopTask is { } runLoopTask)
        {
            try { await runLoopTask.ConfigureAwait(false); }
            catch { /* RunLoopAsync swallows its own exceptions internally; this is defensive. */ }
            _runLoopTask = null;
        }

        _status = ShardStatus.Disconnected;
    }

    private async Task RunLoopAsync()
    {
        // _shardCts is cancelled directly by StopAsync, and indirectly by pool-wide shutdown via
        // the CTR registered in StartAsync — one token captures both lifecycle signals.
        var token = _shardCts.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                await ReceiveLoopAsync();
            }
            catch (OperationCanceledException)
            {
                continue;
            }
            catch (Exception ex)
            {
                // Whoever raised the transport failure first (heartbeat task or the receive loop)
                // wins; we discard a stale parked fault so it does not re-fire on the next iteration.
                Interlocked.Exchange(ref _pendingTransportFault, null);

                SignalConnectionLost();
                await _pool.OnConnectionLostAsync(this, ex);

                if (!GatewayExceptions.IsRecoverableTransport(ex))
                {
                    _status = ShardStatus.Fatal;
                    await _pool.OnFatalAsync(this, ex);
                    return;
                }

                if (!_config.AutoReconnect)
                {
                    // Stays Disconnected until the client is rebuilt via StopAsync + StartAsync,
                    // or a global IDiscordClient.ReconnectAsync tears the pool down and brings
                    // it back up.
                    return;
                }

                // Opportunistic resume: Discord's docs say the session is salvageable on any
                // non-fatal close code (4000-4003, 4005, 4006, 4008, and abnormal closures from
                // our own side like the missed-HEARTBEAT_ACK fault). If we still have the session
                // id and resume URL from the last READY, try RESUME before falling back to fresh
                // IDENTIFY. The retry loop downgrades to identify automatically if resume fails.
                if (!_preferResume
                    && !string.IsNullOrEmpty(_sessionId)
                    && !string.IsNullOrEmpty(_resumeGatewayUrl))
                {
                    _preferResume = true;
                }

                _status = ShardStatus.Reconnecting;
                if (!await ReconnectWithBackoffAsync(token).ConfigureAwait(false))
                    return; // Fatal close code mid-retry, or cancellation.
            }
        }
    }

    /// <summary>
    /// Keeps attempting to reconnect with exponential backoff until either the connection
    /// succeeds (<c>true</c>), the cancellation token fires (<c>false</c>), or a fatal close
    /// code is observed mid-retry (<c>false</c>, with <see cref="IShardEventListener.OnFatalAsync"/>
    /// already called). Never lets a transient transport failure (server down, DNS flake)
    /// escape and silently kill <see cref="RunLoopAsync"/>.
    /// </summary>
    private async Task<bool> ReconnectWithBackoffAsync(CancellationToken token)
    {
        var attempt = 0;
        while (!token.IsCancellationRequested)
        {
            try
            {
                var delay = ComputeBackoffDelay(attempt);

                // Observational hook — bot authors log / alert / emit metrics.
                await _pool.OnReconnectingAsync(this, attempt + 1, delay, _preferResume).ConfigureAwait(false);

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, _pool.TimeProvider, token).ConfigureAwait(false);

                if (_preferResume)
                {
                    _preferResume = false;
                    await ReconnectViaResumeAsync().ConfigureAwait(false);
                }
                else
                {
                    await ReconnectAsync().ConfigureAwait(false);
                }
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                attempt++;
                // If the resume attempt itself failed (server unreachable, RESUME rejected mid-flight),
                // fall back to a fresh identify on the next iteration — preserves the prior behaviour.
                _preferResume = false;

                if (!GatewayExceptions.IsRecoverableTransport(ex))
                {
                    await _pool.OnFatalAsync(this, ex).ConfigureAwait(false);
                    return false;
                }

                if (_config.MaxReconnectAttempts > 0 && attempt >= _config.MaxReconnectAttempts)
                {
                    // Gave up — promote the last transient failure to fatal so WaitShutdownAsync
                    // surfaces the cause instead of returning as if everything was fine.
                    await _pool.OnFatalAsync(this, ex).ConfigureAwait(false);
                    return false;
                }
                // Transient — loop, escalate the backoff.
            }
        }

        return false;
    }

    /// <summary>
    /// Exponential backoff: configured <see cref="DiscordClientConfig.ReconnectDelay"/> doubled
    /// per failed attempt, floored at one second so a zero-config does not spin, capped at 900s
    /// so a long outage does not stretch the wait into hours.
    /// </summary>
    private TimeSpan ComputeBackoffDelay(int attempt)
    {
        const double MaxBackoffSeconds = 900.0;
        var baseSeconds = Math.Max(1.0, _config.ReconnectDelay.TotalSeconds);
        var seconds = Math.Min(MaxBackoffSeconds, baseSeconds * Math.Pow(2, attempt));

        // Jitter ±(fraction*100)% breaks fleet-wide phase synchronisation: 1000 bots hitting the
        // same outage do not all retry at exactly t+5s, t+10s, t+20s.
        var jitterFraction = Math.Clamp(_config.ReconnectBackoffJitter, 0.0, 1.0);
        if (jitterFraction > 0)
        {
            var delta = (Random.Shared.NextDouble() - 0.5) * 2.0 * jitterFraction;
            seconds = Math.Max(0, seconds * (1 + delta));
        }

        return TimeSpan.FromSeconds(seconds);
    }

    private async Task ReceiveLoopAsync()
    {
        ThrowIfPendingTransportFault();

        var readToken = _shardCts.Token;
        CancellationTokenSource? helloTimeoutCts = null;
        var enforcingHelloTimeout = _status == ShardStatus.Connecting && _config.HelloTimeout > TimeSpan.Zero;

        try
        {
            if (enforcingHelloTimeout)
            {
                // Cap the wait for HELLO so a zombie WebSocket (TCP up, server not responding)
                // cannot stall the shard indefinitely. The retry loop in RunLoopAsync's catch
                // takes over once we surface this as a transport failure.
                helloTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_shardCts.Token);
                helloTimeoutCts.CancelAfter(_config.HelloTimeout);
                readToken = helloTimeoutCts.Token;
            }

            ReceivedGatewayMessage? message;
            try
            {
                message = await _socket.ReadAsync(readToken);
            }
            catch (OperationCanceledException) when (enforcingHelloTimeout
                && helloTimeoutCts!.IsCancellationRequested
                && !_shardCts.IsCancellationRequested)
            {
                throw new WebSocketException(GatewayMessages.HelloTimeout);
            }

            ThrowIfPendingTransportFault();

            if (message == null) return;

            if (message.IsSystem())
            {
                await OnProcessSystemMessagesAsync(message);
                return;
            }

            if (message.Opcode == OpCodes.Dispatch)
                await OnDispatchAsync(message);
        }
        finally
        {
            helloTimeoutCts?.Dispose();
        }
    }

    private void ThrowIfPendingTransportFault()
    {
        var fault = Interlocked.Exchange(ref _pendingTransportFault, null);
        if (fault != null) throw fault;
    }

    private async Task OnProcessSystemMessagesAsync(ReceivedGatewayMessage message)
    {
        using var doc = message.ToJsonDocument();
        var payload = doc.RootElement;

        switch (message.Opcode)
        {
            case OpCodes.Hello:
                _heartbeatIntervalMs = payload.GetProperty("heartbeat_interval").GetInt32();

                if (_resumeOnNextHello)
                {
                    _resumeOnNextHello = false;
                    _status = ShardStatus.Identifying;
                    await _socket.ResumeAsync(_config.Token, _sessionId!, _shardCts.Token);
                }
                else
                {
                    await SetupIdentifyAsync();
                }
                StartHeartbeat();
                break;

            case OpCodes.Heartbeat:
                MarkHeartbeatSent();
                await _socket.SendHeartbeatAsync(_shardCts.Token);
                break;

            case OpCodes.HeartbeatAck:
                Volatile.Write(ref _heartbeatAck, true);
                RecordHeartbeatLatency();
                break;

            case OpCodes.Reconnect:
            case OpCodes.InvalidSession:
                // OP 7 (Reconnect): always resumable.
                // OP 9 (InvalidSession): payload is a bool — true if the session can still be resumed.
                var resumable = message.Opcode == OpCodes.Reconnect || payload.TryGetBoolean() == true;
                _preferResume = resumable
                    && !string.IsNullOrEmpty(_sessionId)
                    && !string.IsNullOrEmpty(_resumeGatewayUrl);

                if (!resumable)
                {
                    // Discord explicitly said this session is dead. Drop the session info so the
                    // opportunistic-resume branch in RunLoopAsync's catch does not undo the signal.
                    _sessionId = null;
                    _resumeGatewayUrl = null;
                }

                StopHeartbeat();
                SignalConnectionLost();
                // Closing the socket makes the receive loop's next ReadAsync throw, which lands in
                // RunLoopAsync's catch — the shared retry / backoff path then drives the resume
                // or fresh-identify attempt. Keeps recovery logic in one place.
                await _socket.Close();
                break;
        }
    }

    private void SetReady()
    {
        if (_status != ShardStatus.Identifying)
            return;

        _status = ShardStatus.Ready;
        _pool.Gate.Release();
    }

    private void RecordLifecycle(string phase)
        => DiscoSdkDiagnostics.GatewayLifecycle.Add(
            1,
            new KeyValuePair<string, object?>(DiagnosticTags.ShardId, Id),
            new KeyValuePair<string, object?>(DiagnosticTags.GatewayPhase, phase));

    private void SignalConnectionLost()
    {
        // The IdentifyGate permit is acquired in SetupIdentifyAsync (status flips to Identifying).
        // It is normally released by SetReady on READY/RESUMED. If the connection drops *before*
        // READY arrives, the permit must still be released here — otherwise a multi-shard bot
        // can deadlock with all subsequent shards waiting on a permit that never returns.
        // SetReady already released for the Ready case; releasing again is safe (the gate is a
        // pending-count decrement that no-ops at zero), but we only release on the path where
        // the permit is still actually held.
        if (_status is ShardStatus.Identifying or ShardStatus.Ready)
            _pool.Gate.Release();

        _status = ShardStatus.Disconnected;
        RecordLifecycle(DiagnosticTags.PhaseDisconnect);
    }

    private async Task ReconnectAsync()
    {
        DiscoSdkDiagnostics.GatewayReconnects.Add(
            1,
            new KeyValuePair<string, object?>(DiagnosticTags.ShardId, Id));

        _sessionId = null;
        _resumeGatewayUrl = null;
        // Fresh identify — wipe sequence so HEARTBEAT and any subsequent RESUME do not replay
        // a stale event id against a brand new session.
        _socket.ResetSequence();
        _status = ShardStatus.Connecting;
        await _socket.ConnectAsync(_pool.GatewayUri.ToUri(), _shardCts.Token);
        // The Hello handler in OnProcessSystemMessagesAsync runs SetupIdentifyAsync once HELLO
        // arrives. Sending IDENTIFY before HELLO violates the Discord protocol — the server may
        // respond with 4001 (unknown opcode) and close the link.
    }

    private async Task ReconnectViaResumeAsync()
    {
        DiscoSdkDiagnostics.GatewayReconnects.Add(
            1,
            new KeyValuePair<string, object?>(DiagnosticTags.ShardId, Id));

        // Sequence preserved — Discord expects the RESUME payload to carry the last seq received.
        // The actual RESUME is sent from the Hello handler once Discord greets the new connection.
        _resumeOnNextHello = true;
        _status = ShardStatus.Connecting;
        await _socket.ConnectAsync(new Uri(_resumeGatewayUrl!), _shardCts.Token);
        // _status flips through Identifying (Hello handler) then Ready (RESUMED dispatch).
    }

    private async Task OnDispatchAsync(ReceivedGatewayMessage message)
    {
        if (string.Equals(message.EventType, "READY", StringComparison.Ordinal))
        {
            var obj = message.Deserialize<ReadyPayload>()!;
            _sessionId = obj.SessionId;
            _resumeGatewayUrl = obj.ResumeGatewayUrl;

            SetReady();
            RecordLifecycle(DiagnosticTags.PhaseReady);
            await _pool.OnReadyAsync(this, obj);
            return;
        }

        if (string.Equals(message.EventType, "RESUMED", StringComparison.Ordinal))
        {
            SetReady();
            RecordLifecycle(DiagnosticTags.PhaseResume);
            await _pool.OnResumeAsync(this);
            return;
        }

        await _dispatcher.EnqueueAsync(message);
    }

    private async Task SetupIdentifyAsync()
    {
        await _pool.Gate.WaitAsync();
        _status = ShardStatus.Identifying;
        await SendIdentifyAsync();
    }

    private void StartHeartbeat()
    {
        StopHeartbeat();

        _heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(_shardCts.Token);
        _ = RunHeartbeatAsync();
    }

    private async Task RunHeartbeatAsync()
    {
        var token = _heartbeatCts?.Token ?? default;
        Volatile.Write(ref _heartbeatAck, false);

        try
        {
            // Discord spec: the first heartbeat after HELLO must be delayed by a random fraction
            // of the heartbeat interval. Prevents a thundering-herd flood when a gateway node
            // restarts and every reconnecting bot would otherwise heartbeat at the same instant.
            var jitterFraction = Math.Clamp(_config.HeartbeatJitter, 0.0, 1.0);
            var jitterMs = (int)(Random.Shared.NextDouble() * jitterFraction * _heartbeatIntervalMs);
            if (jitterMs > 0)
                await Task.Delay(TimeSpan.FromMilliseconds(jitterMs), _pool.TimeProvider, token);

            MarkHeartbeatSent();
            await _socket.SendHeartbeatAsync(token);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_heartbeatIntervalMs), _pool.TimeProvider, token);

                if (!Volatile.Read(ref _heartbeatAck))
                {
                    await PublishTransportFaultAsync(new WebSocketException(GatewayMessages.MissedHeartbeatAck(Id)));
                    return;
                }

                Volatile.Write(ref _heartbeatAck, false);
                MarkHeartbeatSent();
                await _socket.SendHeartbeatAsync(token);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — the linked CTS was cancelled.
        }
        catch (Exception ex)
        {
            await PublishTransportFaultAsync(ex);
        }
    }

    /// <summary>
    /// Park a transport failure raised by the fire-and-forget heartbeat task and force-close the
    /// socket so the receive loop unblocks and routes the failure through <see cref="RunLoopAsync"/>'s
    /// catch (which decides whether to reconnect). Uses a non-1000 close code so Discord keeps
    /// the session resumable — per spec, a graceful 1000 close invalidates the session.
    /// </summary>
    private async Task PublishTransportFaultAsync(Exception fault)
    {
        Interlocked.CompareExchange(ref _pendingTransportFault, fault, null);
        try { await _socket.CloseAsync(4900, GatewayMessages.TransportFault); } catch { }
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
    {
        // Discord requires `shard: [shard_id, num_shards]` whenever the bot has more than one
        // shard. Omitting it makes the gateway treat every shard as a stand-alone connection,
        // which routes events to the wrong shard at best and breaks IDENTIFY at worst.
        var payload = _pool.TotalShards > 1
            ? (object)new
            {
                token = _config.Token,
                intents = (int)_config.Intents,
                properties = DeviceInfo.CreateDefault(),
                large_threshold = _config.LargeThreshold,
                shard = new[] { _shardId, _pool.TotalShards }
            }
            : new
            {
                token = _config.Token,
                intents = (int)_config.Intents,
                properties = DeviceInfo.CreateDefault(),
                large_threshold = _config.LargeThreshold
            };

        return _socket.SendAsync(OpCodes.Identify, payload, _shardCts.Token);
    }

    public Task SendAsync(OpCodes codes, object? data, CancellationToken cancellationToken = default)
        => _socket.SendAsync(new(codes, data), cancellationToken);

    /// <summary>
    /// Stamps the timestamp immediately before an outbound heartbeat. Uses the pool's
    /// <see cref="TimeProvider"/> so tests with <c>FakeTimeProvider</c> see virtual-time
    /// latencies instead of wall-clock noise. Paired with <see cref="RecordHeartbeatLatency"/>
    /// on the corresponding HEARTBEAT_ACK to publish the round-trip onto
    /// <c>discosdk.gateway.heartbeat.latency</c>.
    /// </summary>
    private void MarkHeartbeatSent() => Volatile.Write(ref _heartbeatSentTimestamp, _pool.TimeProvider.GetTimestamp());

    /// <summary>
    /// Records the heartbeat round-trip on the SDK's metrics surface. Called from the HEARTBEAT_ACK
    /// handler. Defensive against an ACK arriving without a recorded send (race during reconnect):
    /// in that case the helper short-circuits instead of emitting a nonsense value.
    /// </summary>
    private void RecordHeartbeatLatency()
    {
        var sentAt = Volatile.Read(ref _heartbeatSentTimestamp);
        if (sentAt < 0)
            return;

        var elapsedMs = _pool.TimeProvider.GetElapsedTime(sentAt).TotalMilliseconds;
        DiscoSdkDiagnostics.GatewayHeartbeatLatency.Record(
            elapsedMs,
            new KeyValuePair<string, object?>(DiagnosticTags.ShardId, _shardId));
    }

    public void Dispose()
    {
        try { _tokenRegistration.Dispose(); } catch { }
        try { _shardCts.Cancel(); } catch (ObjectDisposedException) { }
        try { _shardCts.Dispose(); } catch { }
        try { _heartbeatCts?.Dispose(); } catch { }
        // Complete the per-shard dispatcher channel and let the worker drain any backlog before
        // exiting. DisposeAsync swallows errors, so calling it synchronously here is safe.
        try { _dispatcher.DisposeAsync().AsTask().GetAwaiter().GetResult(); } catch { }
        if (_socket is IDisposable disposableSocket)
            try { disposableSocket.Dispose(); } catch { }
    }
}