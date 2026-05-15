using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class AddMemberActionTests : WrapperTestBase
{
    private readonly Snowflake _guildId = new(100);
    private readonly Snowflake _userId = new(42);
    private readonly GuildWrapper _guild;

    public AddMemberActionTests()
    {
        _guild = new GuildWrapper(new Guild { Id = _guildId, Name = "g" }, Client);
        Http.SendAsync<GuildMember>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new GuildMember { User = new User { UserId = _userId, Username = "u" } });
    }

    [Fact]
    public async Task ExecuteAsync_PutsAllSetFieldsAsync()
    {
        await _guild.AddMember(_userId, "tok-abc")
            .SetNickname("Bob")
            .SetMuted(false)
            .SetDeafened(true)
            .SetRoles([new Snowflake(5)])
            .ExecuteAsync();

        await Http.Received(1).SendAsync<GuildMember>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/members/{_userId}"),
            HttpMethod.Put,
            Arg.Is<object?>(b =>
                BodyContains(b, "access_token", "tok-abc") &&
                BodyContains(b, "nick", "Bob") &&
                BodyContains(b, "mute", false) &&
                BodyContains(b, "deaf", true)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Construct_EmptyAccessToken_Throws()
        => Assert.ThrowsAny<ArgumentException>(() => _guild.AddMember(_userId, "  "));

    [Fact]
    public async Task ExecuteAsync_NoOptionalKnobs_OnlySendsAccessTokenAsync()
    {
        await _guild.AddMember(_userId, "tok").ExecuteAsync();

        await Http.Received(1).SendAsync<GuildMember>(
            Arg.Any<DiscordRoute>(), HttpMethod.Put,
            Arg.Is<object?>(b =>
                BodyContains(b, "access_token", "tok") &&
                !BodyHasKey(b, "nick") && !BodyHasKey(b, "mute") &&
                !BodyHasKey(b, "deaf") && !BodyHasKey(b, "roles")),
            Arg.Any<CancellationToken>());
    }
}
