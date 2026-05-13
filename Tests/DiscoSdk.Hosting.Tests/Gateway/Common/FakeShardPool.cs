using DiscoSdk.Hosting.Gateway;
using DiscoSdk.Hosting.Gateway.Payloads;
using DiscoSdk.Hosting.Gateway.Shards;

namespace DiscoSdk.Hosting.Tests.Gateway.Common;

/// <summary>
/// In-memory <see cref="IShardPool"/> used by Shard tests. Records every listener callback so the
/// test can assert that the shard surfaced the expected events.
/// </summary>
internal sealed class FakeShardPool(IGatewaySocket socket, TimeProvider timeProvider) : IShardPool
{
	private readonly CancellationTokenSource _cts = new();

	public List<ReceivedGatewayMessage> ReceivedMessages { get; } = new();
	public List<ReadyPayload> ReadyEvents { get; } = new();
	public int ResumeEvents { get; private set; }
	public List<Exception> ConnectionLostEvents { get; } = new();
	public List<Exception> UnhandledErrors { get; } = new();

	public IdentifyGate Gate { get; } = new();
	public DiscordGatewayUri GatewayUri { get; } = new("wss://gateway.test/", 10);
	public CancellationToken CancellationToken => _cts.Token;
	public IGatewaySocketFactory SocketFactory { get; } = new FakeGatewaySocketFactory((FakeGatewaySocket)socket);
	public TimeProvider TimeProvider { get; } = timeProvider;

	public Task OnReceiveMessageAsync(Shard shard, ReceivedGatewayMessage message)
	{
		ReceivedMessages.Add(message);
		return Task.CompletedTask;
	}

	public Task OnReadyAsync(Shard shard, ReadyPayload payload)
	{
		ReadyEvents.Add(payload);
		return Task.CompletedTask;
	}

	public Task OnResumeAsync(Shard shard)
	{
		ResumeEvents++;
		return Task.CompletedTask;
	}

	public Task OnConnectionLostAsync(Shard shard, Exception exception)
	{
		ConnectionLostEvents.Add(exception);
		return Task.CompletedTask;
	}

	public void OnUnhandledError(Exception exception) => UnhandledErrors.Add(exception);

	public void Cancel() => _cts.Cancel();
}
