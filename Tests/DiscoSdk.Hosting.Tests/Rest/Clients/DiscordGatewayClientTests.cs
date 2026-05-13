using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Rest.Messages;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class DiscordGatewayClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly DiscordGatewayClient _client;

	public DiscordGatewayClientTests()
	{
		_client = new DiscordGatewayClient(_http);
	}

	[Fact]
	public async Task GetGatewayBotInfoAsync_GetsGatewayBotRouteAsync()
	{
		var info = new DiscordGatewayInfo { Url = "wss://gateway.discord.gg", SessionInfo = new DiscordGatewaySessionInfo() };
		_http.SendAsync<DiscordGatewayInfo>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(info);

		var result = await _client.GetGatewayBotInfoAsync();

		Assert.Same(info, result);
		await _http.Received(1).SendAsync<DiscordGatewayInfo>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "gateway/bot"),
			HttpMethod.Get,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}
}
