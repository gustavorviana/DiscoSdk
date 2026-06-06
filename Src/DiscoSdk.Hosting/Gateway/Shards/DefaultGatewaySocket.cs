using DiscoSdk.Hosting.Gateway.Compression;
using DiscoSdk.Hosting.Gateway.Payloads;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DiscoSdk.Hosting.Gateway.Shards;

/// <summary>
/// Default <see cref="IGatewaySocket"/> implementation backed by <see cref="ClientWebSocket"/>.
/// Owns the WebSocket and the decompressor; <see cref="ReadAsync"/> yields already-decompressed
/// <see cref="ReceivedGatewayMessage"/> instances.
/// </summary>
internal sealed class DefaultGatewaySocket(GatewayDecompressFactory decompressFactory, DiscordClientConfig config, TimeProvider timeProvider) : IGatewaySocket
{
    // Discord enforces 120 gateway commands per 60-second window per connection. Going over
    // earns a 4008 close — so we self-throttle. 10 tokens are reserved exclusively for the
    // heartbeat path so user-code spam (presence updates, voice state changes, request guild
    // members in a loop) cannot starve the heartbeat and trigger a zombie-link reconnect.
    private const int NormalTokensPerWindow = 110;
    private const int HeartbeatTokensPerWindow = 10;
    private static readonly TimeSpan SendWindow = TimeSpan.FromSeconds(60);

    // ClientWebSocket.SendAsync is NOT thread-safe; concurrent senders (heartbeat task + receive
    // loop responses + user presence updates) would interleave bytes and corrupt frames. Serialise.
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    // Token bucket. _bucketLock is held only for nanoseconds (refill window check + decrement).
    private readonly object _bucketLock = new();
    private long _windowStartUnixMs;
    private int _normalTokens = NormalTokensPerWindow;
    private int _heartbeatTokens = HeartbeatTokensPerWindow;
    private GatewayDecompress? _decompressor;
    private ClientWebSocket? _websocket;
    private bool _disposed;
    // Stored as a raw long with -1 = "no event seen yet" so Interlocked.Read/Exchange gives an
    // atomic read across the receive loop, heartbeat task, and resume payload sender.
    private long _seq = -1;

    /// <summary>
    /// Gets a value indicating whether the WebSocket is ready and open.
    /// </summary>
    public bool Ready => _websocket?.State == WebSocketState.Open;

    /// <summary>
    /// Connects to the Discord Gateway WebSocket endpoint.
    /// </summary>
    /// <param name="gatewayUri">The Gateway WebSocket URI.</param>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous connect operation.</returns>
    public async Task ConnectAsync(Uri gatewayUri, CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Note: _seq is preserved across Connect so a Resume call can replay the last event.
        // Fresh-identify reconnects call ResetSequence() explicitly before ConnectAsync.
        _decompressor?.Dispose();
        _websocket?.Dispose();
        _websocket = new ClientWebSocket();
        _websocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        // 64 KB receive matches Discord's typical dispatch frame size after zlib decompression;
        // 4 KB send is plenty for IDENTIFY / RESUME / heartbeat (gateway commands cap at 4096
        // bytes anyway). Defaults would fragment large GUILD_CREATE frames excessively.
        _websocket.Options.SetBuffer(receiveBufferSize: 64 * 1024, sendBufferSize: 4 * 1024);
        // Discord rejects connections whose User-Agent does not match `DiscordBot ($url, $version)`.
        // Config value lets the bot author identify their product for analytics / abuse reports.
        _websocket.Options.SetRequestHeader("User-Agent", config.GatewayUserAgent);

        _decompressor = decompressFactory.Create(_websocket);
        await _websocket.ConnectAsync(gatewayUri, token);
    }

    public void ResetSequence() => Interlocked.Exchange(ref _seq, -1);

    public Task ResumeAsync(string token, string sessionId, CancellationToken cancellationToken)
    {
        var seq = Interlocked.Read(ref _seq);
        // Discord expects a non-null integer; if we have not yet observed any dispatch (rare —
        // resume before READY) send 0 so the schema validates rather than null.
        var resumeSeq = seq < 0 ? 0L : seq;
        return SendAsync(new(OpCodes.Resume, new
        {
            token,
            session_id = sessionId,
            seq = resumeSeq
        }), cancellationToken);
    }

    /// <summary>
    /// Reads a message from the WebSocket connection.
    /// </summary>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation. The result contains the received message, or null if parsing fails.</returns>
    public async Task<ReceivedGatewayMessage?> ReadAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_websocket == null)
            throw new InvalidOperationException("Cannot read from a closed gateway socket.");

        var json = await _decompressor!.ReceiveAsync(cancellationToken);

        // Parse exceptions are NOT swallowed — a malformed gateway frame is either a Discord-side
        // schema change or an internal bug; either way, silent null returns mask the symptom and
        // tight-loop the receive path. The catch in RunLoopAsync classifies it as non-recoverable
        // and routes to OnFatalAsync.
        var message = ReceivedGatewayMessage.Parse(json);

        if (message.SequenceNumber != null)
            Interlocked.Exchange(ref _seq, message.SequenceNumber.Value);

        return message;
    }

    public Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        var seq = Interlocked.Read(ref _seq);
        // null is the correct heartbeat value before any event has been seen.
        // Heartbeat is the only path that can dip into the reserved heartbeat-tokens bucket —
        // see SendInternalAsync.
        return SendInternalAsync(new(OpCodes.Heartbeat, seq < 0 ? (long?)null : seq), priority: true, cancellationToken);
    }

    public Task SendAsync(OpCodes codes, object? data, CancellationToken cancellationToken = default)
        => SendAsync(new(codes, data), cancellationToken);

    /// <summary>
    /// Sends a message to the Gateway WebSocket. Serialised against every other sender via
    /// <c>_sendLock</c> so concurrent frames cannot interleave bytes on the wire — the underlying
    /// <see cref="ClientWebSocket.SendAsync(System.ArraySegment{byte}, WebSocketMessageType, bool, CancellationToken)"/>
    /// is documented as NOT thread-safe.
    /// </summary>
    public Task SendAsync(SendGatewayMessage payload, CancellationToken token)
        => SendInternalAsync(payload, priority: false, token);

    private async Task SendInternalAsync(SendGatewayMessage payload, bool priority, CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var websocket = _websocket;
        if (websocket == null)
            throw new InvalidOperationException("Cannot send on a closed gateway socket.");

        var json = JsonSerializer.Serialize(new { op = payload.OpCode, d = payload.Data });
        var bytes = Encoding.UTF8.GetBytes(json);

        // Bucket gate first — if Discord's 120/60s budget is empty, wait until the window
        // refills. Heartbeats (priority=true) have a reserved 10-token side bucket so they keep
        // flowing even when user-code saturates the 110 normal tokens.
        await WaitForSendTokenAsync(priority, token).ConfigureAwait(false);

        await _sendLock.WaitAsync(token).ConfigureAwait(false);
        try
        {
            // Re-check after acquiring the lock — Close may have nulled the socket while we waited.
            websocket = _websocket;
            if (websocket == null)
                throw new InvalidOperationException("Cannot send on a closed gateway socket.");

            await websocket.SendAsync(bytes, WebSocketMessageType.Text, true, token).ConfigureAwait(false);
        }
        finally
        {
            try { _sendLock.Release(); } catch (ObjectDisposedException) { }
        }
    }

    /// <summary>
    /// Waits for a send token from the per-connection 120/60s bucket. Heartbeat callers
    /// (<paramref name="priority"/> = true) first try the reserved heartbeat side bucket and only
    /// fall back to the normal bucket when both are full — the fallback is intentional so a
    /// heartbeat under extreme starvation can still preempt user-code instead of letting the
    /// link go zombie.
    /// </summary>
    private async Task WaitForSendTokenAsync(bool priority, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            long waitMs;
            lock (_bucketLock)
            {
                var nowMs = timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
                var windowEndMs = _windowStartUnixMs + (long)SendWindow.TotalMilliseconds;
                if (_windowStartUnixMs == 0 || nowMs >= windowEndMs)
                {
                    _windowStartUnixMs = nowMs;
                    _normalTokens = NormalTokensPerWindow;
                    _heartbeatTokens = HeartbeatTokensPerWindow;
                    windowEndMs = nowMs + (long)SendWindow.TotalMilliseconds;
                }

                if (priority && _heartbeatTokens > 0)
                {
                    _heartbeatTokens--;
                    return;
                }

                if (_normalTokens > 0)
                {
                    _normalTokens--;
                    return;
                }

                // Last-resort fallback for heartbeat: dip into the (drained) normal bucket as
                // soon as the next token frees. For non-priority callers the wait is the full
                // window expiry.
                waitMs = Math.Max(1, windowEndMs - nowMs);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(waitMs), timeProvider, token).ConfigureAwait(false);
        }

        token.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Closes the WebSocket connection gracefully.
    /// </summary>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    public Task Close() => CloseAsync(1000, GatewayMessages.ClientShutdown);

    public async Task CloseAsync(int closeCode, string reason)
    {
        if (_websocket == null)
            return;

        // Hold the send lock for the entire close: prevents a concurrent SendAsync from running
        // the close handshake against a half-disposed WebSocket, which would either throw
        // ObjectDisposedException or corrupt the wire by writing after the close frame.
        try { await _sendLock.WaitAsync().ConfigureAwait(false); }
        catch (ObjectDisposedException) { return; }

        try
        {
            var websocket = _websocket;
            if (websocket == null)
                return;

            try
            {
                if (websocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    // Bounded: if Discord never sends the close-ack the call would block until
                    // the OS TCP timeout. K8s would then exceed terminationGracePeriodSeconds and
                    // SIGKILL the bot instead of letting it shut down cleanly.
                    using var closeCts = new CancellationTokenSource(config.CloseTimeout);
                    await websocket.CloseAsync((WebSocketCloseStatus)closeCode, reason, closeCts.Token);
                }
            }
            catch { }
            finally
            {
                try { websocket.Dispose(); } catch { }
                _websocket = null;
            }
        }
        finally
        {
            try { _sendLock.Release(); } catch (ObjectDisposedException) { }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _decompressor?.Dispose();
        _websocket?.Dispose();
        _sendLock.Dispose();
    }
}