using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Events;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// Verifies the per-shard dispatcher contract:
///   1. Events queued in order are processed in that same order on the worker.
///   2. A slow handler does not drop later events — they wait, they don't reorder.
///   3. Disposing the dispatcher drains the queue before completing.
/// These are the guarantees the SDK gives consumers in place of JDA's single-thread-per-shard
/// model — same correctness profile, implemented with a bounded <see cref="System.Threading.Channels.Channel{T}"/>.
/// </summary>
public class ShardEventDispatcherTests
{
    private static ReceivedGatewayMessage Event(string type, int seq)
        => ReceivedGatewayMessage.Parse($"{{\"op\":0,\"s\":{seq},\"t\":\"{type}\",\"d\":{{}}}}");

    [Fact]
    public async Task EnqueueAsync_ProcessesEventsInReceiveOrderAsync()
    {
        var seen = new List<int>();
        var processed = new TaskCompletionSource<bool>();

        await using var dispatcher = new ShardEventDispatcher(
            shardId: 0,
            queueCapacity: 64,
            processor: async msg =>
            {
                await Task.Yield();
                seen.Add((int)(msg.SequenceNumber ?? -1));
                if (seen.Count == 5)
                    processed.TrySetResult(true);
            });

        for (int i = 1; i <= 5; i++)
            await dispatcher.EnqueueAsync(Event("GUILD_MEMBER_ADD", i));

        await processed.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal([1, 2, 3, 4, 5], seen);
    }

    [Fact]
    public async Task SlowHandler_DoesNotReorderLaterEventsAsync()
    {
        var seen = new List<int>();
        var done = new TaskCompletionSource<bool>();

        await using var dispatcher = new ShardEventDispatcher(
            shardId: 0,
            queueCapacity: 64,
            processor: async msg =>
            {
                if (msg.SequenceNumber == 1)
                    await Task.Delay(100);
                seen.Add((int)(msg.SequenceNumber ?? -1));
                if (seen.Count == 3)
                    done.TrySetResult(true);
            });

        await dispatcher.EnqueueAsync(Event("SLOW", 1));
        await dispatcher.EnqueueAsync(Event("FAST", 2));
        await dispatcher.EnqueueAsync(Event("FAST", 3));

        await done.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal([1, 2, 3], seen);
    }

    [Fact]
    public async Task HandlerException_DoesNotStallSubsequentEventsAsync()
    {
        var seen = new List<int>();
        var done = new TaskCompletionSource<bool>();

        await using var dispatcher = new ShardEventDispatcher(
            shardId: 0,
            queueCapacity: 64,
            processor: msg =>
            {
                if (msg.SequenceNumber == 2)
                    throw new InvalidOperationException("handler boom");

                seen.Add((int)(msg.SequenceNumber ?? -1));
                if (seen.Count == 2)
                    done.TrySetResult(true);
                return Task.CompletedTask;
            });

        await dispatcher.EnqueueAsync(Event("OK", 1));
        await dispatcher.EnqueueAsync(Event("THROWS", 2));
        await dispatcher.EnqueueAsync(Event("OK", 3));

        await done.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal([1, 3], seen);
    }

    [Fact]
    public async Task DisposeAsync_DrainsBacklogBeforeCompletingAsync()
    {
        var seen = new List<int>();

        var dispatcher = new ShardEventDispatcher(
            shardId: 0,
            queueCapacity: 64,
            processor: msg =>
            {
                seen.Add((int)(msg.SequenceNumber ?? -1));
                return Task.CompletedTask;
            });

        for (int i = 1; i <= 4; i++)
            await dispatcher.EnqueueAsync(Event("PENDING", i));

        await dispatcher.DisposeAsync();

        Assert.Equal([1, 2, 3, 4], seen);
    }
}
