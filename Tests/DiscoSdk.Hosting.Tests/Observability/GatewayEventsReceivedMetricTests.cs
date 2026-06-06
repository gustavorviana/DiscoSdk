using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Shards;
using DiscoSdk.Hosting.Observability;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Rest;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Observability;

/// <summary>
/// Verifies <c>discosdk.gateway.events_received</c> publishes one tagged measurement per
/// dispatch processed by <see cref="DiscordClient"/>'s shard-event listener.
/// </summary>
[Collection("Observability")]
public class GatewayEventsReceivedMetricTests
{
    // Unique shard id keeps captures from colliding with parallel non-metric shard tests.
    private const int TestShardId = 7402;

    private readonly FakeGatewaySocket _socket = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeShardPool _pool;
    private readonly Shard _shard;
    private readonly DiscordClient _client;

    public GatewayEventsReceivedMetricTests()
    {
        _pool = new FakeShardPool(_socket, _time);
        _shard = new Shard(TestShardId, new DiscordClientConfig
        {
            Token = "test-token",
            Intents = DiscordIntent.Guilds,
            HeartbeatJitter = 0.0,
            ReconnectBackoffJitter = 0.0,
        }, _pool);

        var http = Substitute.For<IDiscordRestClient>();
        http.JsonOptions.Returns(new System.Text.Json.JsonSerializerOptions());
        _client = DiscordClientBuilder.Create("test-token")
            .WithIntents(DiscordIntent.Guilds)
            .WithRestClient(http)
            .Build();
    }

    private IEnumerable<CapturedMeasurement<long>> EventsForShard(MeterListenerCapture capture)
        => capture.LongFor("discosdk.gateway.events_received")
                  .Where(m => (int?)m.Tag(DiagnosticTags.ShardId) == TestShardId);

    [Fact]
    public async Task DispatchMessage_PublishesCounterWithShardIdAndEventTypeAsync()
    {
        using var capture = new MeterListenerCapture("discosdk.gateway.events_received");

        var message = ReceivedGatewayMessage.Parse(
            """{"op":0,"t":"MESSAGE_CREATE","s":42,"d":{}}""");

        await ((IShardEventListener)_client).OnReceiveMessageAsync(_shard, message);

        var measurement = Assert.Single(EventsForShard(capture));
        Assert.Equal(1, measurement.Value);
        Assert.Equal(TestShardId, measurement.Tag(DiagnosticTags.ShardId));
        Assert.Equal("MESSAGE_CREATE", measurement.Tag(DiagnosticTags.EventType));
    }

    [Fact]
    public async Task NonDispatchMessage_DoesNotPublishAsync()
    {
        using var capture = new MeterListenerCapture("discosdk.gateway.events_received");

        // op=11 (HeartbeatAck) is a system frame, not a dispatch — the counter must stay at zero.
        var ack = ReceivedGatewayMessage.Parse("""{"op":11}""");

        await ((IShardEventListener)_client).OnReceiveMessageAsync(_shard, ack);

        Assert.Empty(EventsForShard(capture));
    }

    [Fact]
    public async Task MultipleDispatches_PublishOnePerEventAsync()
    {
        using var capture = new MeterListenerCapture("discosdk.gateway.events_received");

        await ((IShardEventListener)_client).OnReceiveMessageAsync(_shard,
            ReceivedGatewayMessage.Parse("""{"op":0,"t":"MESSAGE_CREATE","s":1,"d":{}}"""));
        await ((IShardEventListener)_client).OnReceiveMessageAsync(_shard,
            ReceivedGatewayMessage.Parse("""{"op":0,"t":"GUILD_CREATE","s":2,"d":{}}"""));
        await ((IShardEventListener)_client).OnReceiveMessageAsync(_shard,
            ReceivedGatewayMessage.Parse("""{"op":0,"t":"MESSAGE_CREATE","s":3,"d":{}}"""));

        var measurements = EventsForShard(capture).ToList();
        Assert.Equal(3, measurements.Count);
        Assert.Equal(2, measurements.Count(m => (string?)m.Tag(DiagnosticTags.EventType) == "MESSAGE_CREATE"));
        Assert.Equal(1, measurements.Count(m => (string?)m.Tag(DiagnosticTags.EventType) == "GUILD_CREATE"));
    }
}
