using DiscoSdk.Hosting.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Threading.Channels;

namespace DiscoSdk.Hosting.Gateway.Events;

/// <summary>
/// Per-shard gateway-event dispatcher. Each shard owns one instance; events received from the
/// gateway are enqueued by the shard's receive loop and drained by a dedicated single-reader
/// consumer task. Because Discord assigns every guild deterministically to one shard
/// (<c>guild_id % total_shards</c>), events for the same guild always land in the same dispatcher
/// and are processed in receive order — no cross-guild reordering is possible.
/// </summary>
/// <remarks>
/// <para>The receive loop never blocks on a handler: <see cref="EnqueueAsync"/> writes to a
/// bounded <see cref="Channel{T}"/>, the dispatcher's worker task awaits the handler. When the
/// queue fills, the writer awaits — which transparently backpressures the receive loop. The
/// heartbeat task is on its own timer and unaffected.</para>
/// <para>Handler errors are logged and swallowed; a single failing handler must not stall the
/// shard. Cancellation is cooperative: <see cref="DisposeAsync"/> completes the writer, the
/// worker drains the remaining backlog, then exits.</para>
/// </remarks>
internal sealed class ShardEventDispatcher : IAsyncDisposable
{
    private readonly Channel<ReceivedGatewayMessage> _queue;
    private readonly Task _worker;
    private readonly Func<ReceivedGatewayMessage, Task> _processor;
    private readonly int _shardId;
    private readonly ILogger _logger;
    private readonly KeyValuePair<string, object?> _shardTag;
    private int _disposed;

    public ShardEventDispatcher(
        int shardId,
        int queueCapacity,
        Func<ReceivedGatewayMessage, Task> processor,
        ILogger? logger = null)
    {
        _shardId = shardId;
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _logger = logger ?? NullLogger.Instance;
        _shardTag = new KeyValuePair<string, object?>(DiagnosticTags.ShardId, shardId);

        var options = new BoundedChannelOptions(Math.Max(1, queueCapacity))
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        };
        _queue = Channel.CreateBounded<ReceivedGatewayMessage>(options);
        _worker = Task.Run(ProcessLoopAsync);
    }

    /// <summary>
    /// Enqueues a gateway message for serial dispatch on this shard. Awaits when the queue is
    /// full so the caller (receive loop) backpressures naturally.
    /// </summary>
    public ValueTask EnqueueAsync(ReceivedGatewayMessage message, CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref _disposed) == 1)
            return ValueTask.CompletedTask;

        return _queue.Writer.WriteAsync(message, cancellationToken);
    }

    private async Task ProcessLoopAsync()
    {
        try
        {
            await foreach (var message in _queue.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                var start = Stopwatch.GetTimestamp();
                try
                {
                    await _processor(message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Log(
                        LogLevel.Error, ex,
                        "Unhandled exception while dispatching {EventType} on shard {ShardId}",
                        message.EventType, _shardId);
                }
                finally
                {
                    var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                    DiscoSdkDiagnostics.GatewayEventDispatchDuration.Record(
                        elapsedMs,
                        _shardTag,
                        new KeyValuePair<string, object?>(DiagnosticTags.EventType, message.EventType ?? "<unknown>"));
                }
            }
        }
        catch (Exception ex)
        {
            // Reader.ReadAllAsync should not throw under normal shutdown (Complete on the writer
            // ends the loop cleanly). If it does, the shard is in a broken state — log loudly.
            _logger.Log(LogLevel.Error, ex, "Shard {ShardId} dispatcher loop crashed", _shardId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _queue.Writer.TryComplete();
        try
        {
            await _worker.ConfigureAwait(false);
        }
        catch
        {
            // Worker already logs its own exceptions; swallow so dispose never throws.
        }
    }
}
