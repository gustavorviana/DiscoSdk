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
internal sealed class DefaultGatewaySocket(GatewayDecompressFactory decompressFactory, DiscordClientConfig config) : IGatewaySocket
{
    // ClientWebSocket.SendAsync is NOT thread-safe; concurrent senders (heartbeat task + receive
    // loop responses + user presence updates) would interleave bytes and corrupt frames. Serialise.
    private readonly SemaphoreSlim _sendLock = new(1, 1);
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
        return SendAsync(OpCodes.Heartbeat, seq < 0 ? (long?)null : seq, cancellationToken);
    }

    public Task SendAsync(OpCodes codes, object? data, CancellationToken cancellationToken = default)
        => SendAsync(new(codes, data), cancellationToken);

    /// <summary>
    /// Sends a message to the Gateway WebSocket. Serialised against every other sender via
    /// <c>_sendLock</c> so concurrent frames cannot interleave bytes on the wire — the underlying
    /// <see cref="ClientWebSocket.SendAsync(System.ArraySegment{byte}, WebSocketMessageType, bool, CancellationToken)"/>
    /// is documented as NOT thread-safe.
    /// </summary>
    public async Task SendAsync(SendGatewayMessage payload, CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var websocket = _websocket;
        if (websocket == null)
            throw new InvalidOperationException("Cannot send on a closed gateway socket.");

        var json = JsonSerializer.Serialize(new { op = payload.OpCode, d = payload.Data });
        var bytes = Encoding.UTF8.GetBytes(json);

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