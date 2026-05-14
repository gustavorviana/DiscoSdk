using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

/// <summary>Tests for the application-owned emoji methods on <see cref="ApplicationClient"/>.</summary>
public class ApplicationEmojiClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly ApplicationClient _client;
	private readonly Snowflake _appId = new(900);
	private readonly Snowflake _emojiId = new(901);

	public ApplicationEmojiClientTests()
	{
		_client = new ApplicationClient(_http);
	}

	[Fact]
	public async Task ListApplicationEmojisAsync_GetsEnvelopeAsync()
	{
		_http.SendAsync<ApplicationEmojiList>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new ApplicationEmojiList());

		await _client.ListApplicationEmojisAsync(_appId);

		await _http.Received(1).SendAsync<ApplicationEmojiList>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/emojis"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetApplicationEmojiAsync_GetsByIdAsync()
	{
		_http.SendAsync<Emoji>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Emoji());

		await _client.GetApplicationEmojiAsync(_appId, _emojiId);

		await _http.Received(1).SendAsync<Emoji>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/emojis/{_emojiId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateApplicationEmojiAsync_PostsAsync()
	{
		_http.SendAsync<Emoji>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Emoji());

		var req = new { name = "smile", image = "data:image/png;base64,xxx" };
		await _client.CreateApplicationEmojiAsync(_appId, req);

		await _http.Received(1).SendAsync<Emoji>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/emojis"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyApplicationEmojiAsync_PatchesAsync()
	{
		_http.SendAsync<Emoji>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Emoji());

		var req = new { name = "renamed" };
		await _client.ModifyApplicationEmojiAsync(_appId, _emojiId, req);

		await _http.Received(1).SendAsync<Emoji>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/emojis/{_emojiId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteApplicationEmojiAsync_DeletesAsync()
	{
		await _client.DeleteApplicationEmojiAsync(_appId, _emojiId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/emojis/{_emojiId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
