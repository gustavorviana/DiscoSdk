using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Observability;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using Microsoft.Extensions.Time.Testing;

namespace DiscoSdk.Hosting.Tests.Observability;

/// <summary>
/// Verifies that <c>discosdk.gateway.heartbeat.latency</c> publishes a measurement when a
/// HEARTBEAT_ACK arrives after a heartbeat send, tagged with the shard id. Uses the existing
/// fake-socket / fake-time infrastructure to drive the heartbeat round-trip without real I/O.
/// </summary>
[Collection("Observability")]
public class ShardHeartbeatLatencyMetricTests
{
    // Unique shard id keeps MeterListener captures from colliding with parallel heartbeat tests
    // that exercise other Shard instances (those use shard id 0).
    private const int TestShardId = 7401;

    private readonly FakeGatewaySocket _socket = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeShardPool _pool;
    private readonly Shard _shard;

    public ShardHeartbeatLatencyMetricTests()
    {
        _pool = new FakeShardPool(_socket, _time);
        _shard = new Shard(TestShardId, new DiscordClientConfig
        {
            Token = "test-token",
            Intents = DiscordIntent.Guilds,
            ReconnectDelay = TimeSpan.FromSeconds(5),
            HeartbeatJitter = 0.0,
            ReconnectBackoffJitter = 0.0,
        }, _pool);
    }

    private IEnumerable<CapturedMeasurement<double>> LatencyForShard(MeterListenerCapture capture)
        => capture.DoubleFor("discosdk.gateway.heartbeat.latency")
                  .Where(m => (int?)m.Tag(DiagnosticTags.ShardId) == TestShardId);

    [Fact]
    public async Task HeartbeatAck_PublishesLatencyHistogramWithShardIdAsync()
    {
        using var capture = new MeterListenerCapture("discosdk.gateway.heartbeat.latency");

        await _shard.StartAsync();
        await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));
        await WaitForOpcodeCount(OpCodes.Heartbeat, 1);

        await _socket.EnqueueInbound(TestFrames.HeartbeatAck());
        await _socket.WaitForInboxDrainedAsync();

        await WaitFor(() => LatencyForShard(capture).Any());

        var measurement = Assert.Single(LatencyForShard(capture));
        Assert.True(measurement.Value >= 0d);
        Assert.Equal(TestShardId, measurement.Tag(DiagnosticTags.ShardId));
    }

    [Fact]
    public async Task MultipleAcks_PublishOneMeasurementEachAsync()
    {
        using var capture = new MeterListenerCapture("discosdk.gateway.heartbeat.latency");

        await _shard.StartAsync();
        await _socket.EnqueueInbound(TestFrames.Hello(heartbeatIntervalMs: 5000));
        await WaitForOpcodeCount(OpCodes.Heartbeat, 1);

        await _socket.EnqueueInbound(TestFrames.HeartbeatAck());
        await _socket.WaitForInboxDrainedAsync();

        _time.Advance(TimeSpan.FromSeconds(5));
        await WaitForOpcodeCount(OpCodes.Heartbeat, 2);
        await _socket.EnqueueInbound(TestFrames.HeartbeatAck());
        await _socket.WaitForInboxDrainedAsync();

        await WaitFor(() => LatencyForShard(capture).Count() >= 2);
    }

    private static async Task WaitFor(Func<bool> predicate, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!predicate() && DateTime.UtcNow < deadline)
            await Task.Delay(5);
        Assert.True(predicate(), "Timed out waiting for predicate.");
    }

    private async Task WaitForOpcodeCount(OpCodes opcode, int count, int timeoutMs = 5000)
        => await WaitFor(() => _socket.SentFrames.Count(f => f.OpCode == opcode) >= count, timeoutMs);
}
