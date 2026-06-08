using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class ModifySoundboardSoundActionTests : WrapperTestBase
{
    private readonly Snowflake _guildId = new(100);
    private readonly Snowflake _soundId = new(7);

    public ModifySoundboardSoundActionTests()
    {
        Http.SendAsync<SoundboardSound>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new SoundboardSound { SoundId = _soundId });
    }

    private ModifySoundboardSoundAction Action() => new(Client, _guildId, _soundId);

    [Fact]
    public async Task ExecuteAsync_Untouched_SendsEmptyBodyAsync()
    {
        await Action().ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds/{_soundId}"),
            HttpMethod.Patch,
            Arg.Is<object?>(b =>
                !BodyHasKey(b, "name") &&
                !BodyHasKey(b, "volume") &&
                !BodyHasKey(b, "emoji_id") &&
                !BodyHasKey(b, "emoji_name")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetName_OnlyNameIsSentAsync()
    {
        await Action().SetName("renamed").ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyContains(b, "name", "renamed") &&
                !BodyHasKey(b, "volume")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetVolume_OnlyVolumeIsSentAsync()
    {
        await Action().SetVolume(0.25).ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyContains(b, "volume", 0.25)),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void SetVolume_OutOfRange_Throws(double volume)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Action().SetVolume(volume));
    }

    [Fact]
    public async Task SetEmoji_CustomId_PairsExplicitNullNameAsync()
    {
        await Action().SetEmoji(new Snowflake(900)).ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyContains(b, "emoji_id", "900") &&
                BodyHasKey(b, "emoji_name") &&
                BodyContains(b, "emoji_name", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetEmoji_Unicode_PairsExplicitNullIdAsync()
    {
        await Action().SetEmoji("🎺").ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyContains(b, "emoji_name", "🎺") &&
                BodyHasKey(b, "emoji_id") &&
                BodyContains(b, "emoji_id", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearEmoji_SendsBothFieldsAsNullAsync()
    {
        await Action().ClearEmoji().ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyHasKey(b, "emoji_id") && BodyContains(b, "emoji_id", null) &&
                BodyHasKey(b, "emoji_name") && BodyContains(b, "emoji_name", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetEmoji_LatestWins_UnicodeOverwritesIdAsync()
    {
        await Action().SetEmoji(new Snowflake(900)).SetEmoji("🔥").ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyContains(b, "emoji_name", "🔥") &&
                BodyHasKey(b, "emoji_id") &&
                BodyContains(b, "emoji_id", null)),
            Arg.Any<CancellationToken>());
    }
}
