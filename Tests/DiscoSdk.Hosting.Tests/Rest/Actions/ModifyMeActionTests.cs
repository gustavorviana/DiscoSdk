using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class ModifyMeActionTests : WrapperTestBase
{
    public ModifyMeActionTests()
    {
        Http.SendAsync<User>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new User { UserId = new Snowflake(1), Username = "bot" });
    }

    [Fact]
    public async Task SetUsername_PatchesMeUsernameAsync()
    {
        await Client.Me.Modify().SetUsername("new-name").ExecuteAsync();

        await Http.Received(1).SendAsync<User>(
            Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me"),
            HttpMethod.Patch,
            Arg.Is<object?>(b => BodyContains(b, "username", "new-name")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearAvatar_SendsExplicitNullAsync()
    {
        await Client.Me.Modify().ClearAvatar().ExecuteAsync();

        await Http.Received(1).SendAsync<User>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyHasKey(b, "avatar") && BodyContains(b, "avatar", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAvatar_SendsDataUriAsync()
    {
        await Client.Me.Modify().SetAvatar("data:image/png;base64,abc").ExecuteAsync();

        await Http.Received(1).SendAsync<User>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyContains(b, "avatar", "data:image/png;base64,abc")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearBanner_SendsExplicitNullAsync()
    {
        await Client.Me.Modify().ClearBanner().ExecuteAsync();

        await Http.Received(1).SendAsync<User>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyHasKey(b, "banner") && BodyContains(b, "banner", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SetUsername_Empty_Throws()
        => Assert.ThrowsAny<ArgumentException>(() => Client.Me.Modify().SetUsername(""));
}
