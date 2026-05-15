using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class GetInviteActionTests : WrapperTestBase
{
    [Fact]
    public async Task ExecuteAsync_NullInvite_ReturnsNullAsync()
    {
        Http.SendAsync<Invite>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Invite>(null!));

        var result = await Client.GetInvite("abc").ExecuteAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task WithFlags_AppendsAllQueryParamsAsync()
    {
        Http.SendAsync<Invite>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Invite>(null!));

        await Client.GetInvite("abc")
            .WithCounts()
            .WithExpiration()
            .WithScheduledEvent(new Snowflake(99))
            .ExecuteAsync();

        await Http.Received(1).SendAsync<Invite>(
            Arg.Is<DiscordRoute>(r =>
                r.ToString().StartsWith("invites/abc") &&
                r.ToString().Contains("with_counts=true") &&
                r.ToString().Contains("with_expiration=true") &&
                r.ToString().Contains("guild_scheduled_event_id=99")),
            HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Construct_EmptyCode_Throws()
        => Assert.ThrowsAny<ArgumentException>(() => Client.GetInvite("   "));
}
