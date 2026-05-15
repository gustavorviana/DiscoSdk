using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class ModifyMemberActionTests : WrapperTestBase
{
    private readonly Snowflake _guildId = new(100);
    private readonly Snowflake _userId = new(42);
    private readonly GuildWrapper _guild;

    public ModifyMemberActionTests()
    {
        _guild = new GuildWrapper(new Guild { Id = _guildId, Name = "g" }, Client);
        Http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new GuildMember { User = new User { UserId = _userId, Username = "u" } });
    }

    [Fact]
    public async Task ExecuteAsync_EmitsOnlySetFieldsAsync()
    {
        await _guild.ModifyMember(_userId).SetNickname("new").SetMuted(true).ExecuteAsync();

        await Http.Received(1).SendAsync<GuildMember>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}"),
            HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyContains(b, "nick", "new") &&
                BodyContains(b, "mute", true) &&
                !BodyHasKey(b, "deaf") &&
                !BodyHasKey(b, "roles")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Timeout_SetsCommunicationDisabledUntilIsoStringAsync()
    {
        var until = DateTimeOffset.UtcNow.AddMinutes(10);

        await _guild.ModifyMember(_userId).Timeout(until).ExecuteAsync();

        await Http.Received(1).SendAsync<GuildMember>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyContains(b, "communication_disabled_until", until.ToString("o"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearTimeout_SetsCommunicationDisabledUntilToNullAsync()
    {
        await _guild.ModifyMember(_userId).ClearTimeout().ExecuteAsync();

        await Http.Received(1).SendAsync<GuildMember>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyHasKey(b, "communication_disabled_until") &&
                BodyContains(b, "communication_disabled_until", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveToVoiceChannel_Null_DisconnectsAsync()
    {
        await _guild.ModifyMember(_userId).MoveToVoiceChannel(null).ExecuteAsync();

        await Http.Received(1).SendAsync<GuildMember>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyHasKey(b, "channel_id") && BodyContains(b, "channel_id", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRoles_SerializesAsStringArrayAsync()
    {
        object? captured = null;
        Http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                captured = call.Arg<object?>();
                return new GuildMember { User = new User { UserId = _userId, Username = "u" } };
            });

        await _guild.ModifyMember(_userId).SetRoles([new Snowflake(7), new Snowflake(8)]).ExecuteAsync();

        var dict = Assert.IsAssignableFrom<IDictionary<string, object?>>(captured!);
        var arr = Assert.IsType<string[]>(dict["roles"]);
        Assert.Equal(["7", "8"], arr);
    }

    [Fact]
    public void SetRoles_NullThrows()
        => Assert.Throws<ArgumentNullException>(() => _guild.ModifyMember(_userId).SetRoles(null!));
}
