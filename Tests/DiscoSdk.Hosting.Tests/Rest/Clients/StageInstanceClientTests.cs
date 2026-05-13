using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class StageInstanceClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly StageInstanceClient _client;
	private readonly Snowflake _channelId = new(200);

	public StageInstanceClientTests()
	{
		_client = new StageInstanceClient(_http);
	}

	[Fact]
	public async Task CreateAsync_PostsToStageInstancesAsync()
	{
		_http.SendAsync<StageInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new StageInstance());

		var req = new { channel_id = _channelId.ToString(), topic = "Town Hall" };
		await _client.CreateAsync(req);

		await _http.Received(1).SendAsync<StageInstance>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "stage-instances"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAsync_GetsByChannelAsync()
	{
		_http.SendAsync<StageInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new StageInstance());

		await _client.GetAsync(_channelId);

		await _http.Received(1).SendAsync<StageInstance>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"stage-instances/{_channelId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyAsync_PatchesByChannelAsync()
	{
		_http.SendAsync<StageInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new StageInstance());

		var req = new { topic = "New Topic" };
		await _client.ModifyAsync(_channelId, req);

		await _http.Received(1).SendAsync<StageInstance>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"stage-instances/{_channelId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_DeletesByChannelAsync()
	{
		await _client.DeleteAsync(_channelId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"stage-instances/{_channelId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
