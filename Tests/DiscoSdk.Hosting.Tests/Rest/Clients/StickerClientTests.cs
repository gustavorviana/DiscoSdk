using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class StickerClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly StickerClient _client;
	private readonly Snowflake _guildId = new(100);
	private readonly Snowflake _stickerId = new(1200);

	public StickerClientTests()
	{
		_http.JsonOptions.Returns(new System.Text.Json.JsonSerializerOptions());
		_client = new StickerClient(_http);
	}

	[Fact]
	public async Task GetStickerAsync_GetsByIdAsync()
	{
		_http.SendAsync<Sticker>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Sticker());

		await _client.GetStickerAsync(_stickerId);

		await _http.Received(1).SendAsync<Sticker>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"stickers/{_stickerId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListStickerPacksAsync_GetsPacksAsync()
	{
		_http.SendAsync<StickerPackList>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new StickerPackList());

		await _client.ListStickerPacksAsync();

		await _http.Received(1).SendAsync<StickerPackList>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "sticker-packs"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ListGuildStickersAsync_GetsAsync()
	{
		_http.SendAsync<Sticker[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await _client.ListGuildStickersAsync(_guildId);

		await _http.Received(1).SendAsync<Sticker[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/stickers"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetGuildStickerAsync_GetsAsync()
	{
		_http.SendAsync<Sticker>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Sticker());

		await _client.GetGuildStickerAsync(_guildId, _stickerId);

		await _http.Received(1).SendAsync<Sticker>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/stickers/{_stickerId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateGuildStickerAsync_PostsMultipartAsync()
	{
		_http.SendAsync<Sticker>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<Func<HttpContent>>(), Arg.Any<CancellationToken>())
			.Returns(new Sticker());

		var file = new MessageFile("sticker.png", description: null, buffer: [1, 2, 3], contentType: "image/png");
		await _client.CreateGuildStickerAsync(_guildId, "fancy", "fancy mood", "fancy", file);

		await _http.Received(1).SendAsync<Sticker>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/stickers"),
			HttpMethod.Post,
			Arg.Any<Func<HttpContent>>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyGuildStickerAsync_PatchesAsync()
	{
		_http.SendAsync<Sticker>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Sticker());

		var req = new { name = "renamed" };
		await _client.ModifyGuildStickerAsync(_guildId, _stickerId, req);

		await _http.Received(1).SendAsync<Sticker>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/stickers/{_stickerId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteGuildStickerAsync_DeletesAsync()
	{
		await _client.DeleteGuildStickerAsync(_guildId, _stickerId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/stickers/{_stickerId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
