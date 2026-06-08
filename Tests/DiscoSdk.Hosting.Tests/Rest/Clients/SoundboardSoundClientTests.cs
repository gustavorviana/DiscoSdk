using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class SoundboardSoundClientTests
{
    private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
    private readonly SoundboardSoundClient _client;
    private readonly Snowflake _guildId = new(100);
    private readonly Snowflake _soundId = new(7777);

    public SoundboardSoundClientTests()
    {
        _http.JsonOptions.Returns(new System.Text.Json.JsonSerializerOptions());
        _client = new SoundboardSoundClient(_http);
    }

    [Fact]
    public async Task ListGuildSoundboardSoundsAsync_GetsEnvelopeAndUnwrapsItemsAsync()
    {
        var items = new[] { new SoundboardSound { SoundId = _soundId, Name = "horn" } };
        _http.SendAsync<SoundboardSoundListResponse>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new SoundboardSoundListResponse { Items = items });

        var result = await _client.ListGuildSoundboardSoundsAsync(_guildId);

        Assert.Same(items, result);
        await _http.Received(1).SendAsync<SoundboardSoundListResponse>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds"),
            HttpMethod.Get,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetGuildSoundboardSoundAsync_GetsByIdAsync()
    {
        _http.SendAsync<SoundboardSound>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new SoundboardSound());

        await _client.GetGuildSoundboardSoundAsync(_guildId, _soundId);

        await _http.Received(1).SendAsync<SoundboardSound>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds/{_soundId}"),
            HttpMethod.Get,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateGuildSoundboardSoundAsync_PostsJsonBodyAsync()
    {
        _http.SendAsync<SoundboardSound>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new SoundboardSound());

        var body = new { name = "horn", sound = "data:audio/mpeg;base64,AAA" };
        await _client.CreateGuildSoundboardSoundAsync(_guildId, body);

        await _http.Received(1).SendAsync<SoundboardSound>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds"),
            HttpMethod.Post,
            body,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ModifyGuildSoundboardSoundAsync_PatchesAsync()
    {
        _http.SendAsync<SoundboardSound>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new SoundboardSound());

        var body = new { name = "renamed" };
        await _client.ModifyGuildSoundboardSoundAsync(_guildId, _soundId, body);

        await _http.Received(1).SendAsync<SoundboardSound>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds/{_soundId}"),
            HttpMethod.Patch,
            body,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteGuildSoundboardSoundAsync_DeletesAsync()
    {
        await _client.DeleteGuildSoundboardSoundAsync(_guildId, _soundId);

        await _http.Received(1).SendAsync(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds/{_soundId}"),
            HttpMethod.Delete,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateGuildSoundboardSoundAsync_NullBody_ThrowsAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.CreateGuildSoundboardSoundAsync(_guildId, null!));
    }

    [Fact]
    public async Task ModifyGuildSoundboardSoundAsync_NullBody_ThrowsAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _client.ModifyGuildSoundboardSoundAsync(_guildId, _soundId, null!));
    }
}
