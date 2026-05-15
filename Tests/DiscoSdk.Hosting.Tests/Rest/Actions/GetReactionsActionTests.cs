using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class GetReactionsActionTests : WrapperTestBase
{
    private readonly Snowflake _channelId = new(1);
    private readonly Snowflake _messageId = new(2);

    private GetReactionsAction Action(string emoji = "👍")
        => new(Client, _channelId, _messageId, emoji);

    [Fact]
    public async Task ExecuteAsync_BuildsRouteWithEmojiAsync()
    {
        Http.SendAsync<User[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await Action().SetLimit(50).After(new Snowflake(7)).ExecuteAsync();

        await Http.Received(1).SendAsync<User[]>(
            Arg.Is<DiscordRoute>(r =>
                r.ToString().Contains($"channels/{_channelId}/messages/{_messageId}/reactions/") &&
                r.ToString().Contains("limit=50") &&
                r.ToString().Contains("after=7")),
            HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void SetLimit_OutOfRange_Throws(int badLimit)
        => Assert.Throws<ArgumentOutOfRangeException>(() => Action().SetLimit(badLimit));

    [Fact]
    public void Construct_EmptyEmoji_Throws()
        => Assert.ThrowsAny<ArgumentException>(() => Action(""));
}
