using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers;

public class SoundboardSoundWrapperTests : WrapperTestBase
{
    private readonly Snowflake _guildId = new(100);
    private readonly Snowflake _soundId = new(7777);
    private readonly Snowflake _userId = new(42);

    private SoundboardSound Model(Snowflake guildId, Snowflake? emojiId) => new()
    {
        SoundId = _soundId,
        Name = "horn",
        Volume = 0.8,
        EmojiId = emojiId,
        EmojiName = "🔥",
        GuildId = guildId,
        Available = true,
        UserId = _userId,
    };

    private SoundboardSound DefaultModel(Snowflake? emojiId = null) => Model(_guildId, emojiId);

    [Fact]
    public void Properties_ExposeModelFields()
    {
        var w = new SoundboardSoundWrapper(Client, DefaultModel(emojiId: new Snowflake(900)));

        Assert.Equal(_soundId, w.Id);
        Assert.Equal(_soundId.CreatedAt, w.CreatedAt);
        Assert.Equal("horn", w.Name);
        Assert.Equal(0.8, w.Volume);
        Assert.Equal(new Snowflake(900), w.EmojiId);
        Assert.Equal("🔥", w.EmojiName);
        Assert.Equal(_guildId, w.GuildId);
        Assert.True(w.Available);
        Assert.Equal(_userId, w.UserId);
    }

    [Fact]
    public void GuildId_DefaultSnowflake_ReturnsNull()
    {
        var w = new SoundboardSoundWrapper(Client, Model(default(Snowflake), null));
        Assert.Null(w.GuildId);
    }

    [Fact]
    public void EmojiId_DefaultSnowflake_ReturnsNull()
    {
        var w = new SoundboardSoundWrapper(Client, DefaultModel(emojiId: default(Snowflake)));
        Assert.Null(w.EmojiId);
    }

    [Fact]
    public async Task Modify_BuildsActionAgainstSoundIdAsync()
    {
        Http.SendAsync<SoundboardSound>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new SoundboardSound { SoundId = _soundId });

        var w = new SoundboardSoundWrapper(Client, DefaultModel());
        await w.Modify().SetName("renamed").ExecuteAsync();

        await Http.Received(1).SendAsync<SoundboardSound>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds/{_soundId}"),
            HttpMethod.Patch,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_SendsDeleteToSoundRouteAsync()
    {
        var w = new SoundboardSoundWrapper(Client, DefaultModel());
        await w.Delete().ExecuteAsync();

        await Http.Received(1).SendAsync(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/soundboard-sounds/{_soundId}"),
            HttpMethod.Delete,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Modify_OnDefaultDiscordSound_Throws()
    {
        var w = new SoundboardSoundWrapper(Client, Model(default(Snowflake), null));
        Assert.Throws<InvalidOperationException>(() => w.Modify());
    }

    [Fact]
    public void Delete_OnDefaultDiscordSound_Throws()
    {
        var w = new SoundboardSoundWrapper(Client, Model(default(Snowflake), null));
        Assert.Throws<InvalidOperationException>(() => w.Delete());
    }
}
