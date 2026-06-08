using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class CreateSoundboardSoundActionTests : WrapperTestBase
{
    private readonly Snowflake _guildId = new(100);
    // OGG header — DiscordSoundBuffer detects MIME from magic bytes.
    private static readonly byte[] OggBytes = [0x4F, 0x67, 0x67, 0x53, 0x00, 0x02];

    public CreateSoundboardSoundActionTests()
    {
        Http.SendAsync<SoundboardSound>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new SoundboardSound { SoundId = new Snowflake(7) });
    }

    private CreateSoundboardSoundAction Build(string name = "horn")
        => new(Client, _guildId, name, new DiscordSoundBuffer(OggBytes));

    [Fact]
    public async Task ExecuteAsync_RequiredFieldsOnly_SendsNameAndSoundAsync()
    {
        await Build().ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds"),
            HttpMethod.Post,
            Arg.Is<object?>(b =>
                BodyContains(b, "name", "horn") &&
                BodyHasKey(b, "sound") &&
                !BodyHasKey(b, "volume") &&
                !BodyHasKey(b, "emoji_id") &&
                !BodyHasKey(b, "emoji_name")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetVolume_AddsVolumeFieldAsync()
    {
        await Build().SetVolume(0.5).ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Post,
            Arg.Is<object?>(b => BodyContains(b, "volume", 0.5)),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void SetVolume_OutOfRange_Throws(double volume)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Build().SetVolume(volume));
    }

    [Fact]
    public async Task SetEmoji_CustomId_SerializesAsStringAsync()
    {
        await Build().SetEmoji(new Snowflake(900)).ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Post,
            Arg.Is<object?>(b =>
                BodyContains(b, "emoji_id", "900") &&
                !BodyHasKey(b, "emoji_name")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetEmoji_Unicode_SendsEmojiNameAsync()
    {
        await Build().SetEmoji("🔥").ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Post,
            Arg.Is<object?>(b =>
                BodyContains(b, "emoji_name", "🔥") &&
                !BodyHasKey(b, "emoji_id")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetEmoji_LatestWins_UnicodeOverwritesIdAsync()
    {
        await Build().SetEmoji(new Snowflake(900)).SetEmoji("🎺").ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Any<DiscordRoute>(), HttpMethod.Post,
            Arg.Is<object?>(b =>
                BodyContains(b, "emoji_name", "🎺") &&
                !BodyHasKey(b, "emoji_id")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Ctor_NullName_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new CreateSoundboardSoundAction(Client, _guildId, " ", new DiscordSoundBuffer(OggBytes)));
    }

    [Fact]
    public void Ctor_NullSound_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CreateSoundboardSoundAction(Client, _guildId, "n", null!));
    }
}
