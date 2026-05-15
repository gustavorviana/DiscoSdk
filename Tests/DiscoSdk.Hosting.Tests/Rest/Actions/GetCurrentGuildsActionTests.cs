using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class GetCurrentGuildsActionTests : WrapperTestBase
{
    public GetCurrentGuildsActionTests()
    {
        Http.SendAsync<Guild[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns([]);
    }

    [Fact]
    public async Task ExecuteAsync_NoKnobs_HitsBaseRouteAsync()
    {
        await Client.Me.GetGuilds().ExecuteAsync();

        await Http.Received(1).SendAsync<Guild[]>(
            Arg.Is<DiscordRoute>(r => r.ToString() == "users/@me/guilds"),
            HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetLimit_AndPagination_AddsQueryStringAsync()
    {
        await Client.Me.GetGuilds()
            .SetLimit(50)
            .After(new Snowflake(123))
            .WithCounts()
            .ExecuteAsync();

        await Http.Received(1).SendAsync<Guild[]>(
            Arg.Is<DiscordRoute>(r =>
                r.ToString().Contains("limit=50") &&
                r.ToString().Contains("after=123") &&
                r.ToString().Contains("with_counts=true")),
            HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public void SetLimit_OutOfRange_Throws(int badLimit)
        => Assert.Throws<ArgumentOutOfRangeException>(() => Client.Me.GetGuilds().SetLimit(badLimit));
}
