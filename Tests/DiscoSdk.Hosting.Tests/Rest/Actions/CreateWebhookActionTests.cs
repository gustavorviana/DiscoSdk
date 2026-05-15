using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class CreateWebhookActionTests : WrapperTestBase
{
    private readonly Snowflake _channelId = new(7);

    public CreateWebhookActionTests()
    {
        Http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new Webhook { Id = new Snowflake(1) });
    }

    [Fact]
    public async Task ExecuteAsync_PostsToChannelWebhooksAsync()
    {
        await Client.Webhooks.Create(_channelId).SetName("hooky").SetAvatar("data:img").ExecuteAsync();

        await Http.Received(1).SendAsync<Webhook>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/webhooks"),
            HttpMethod.Post,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutName_ThrowsAsync()
    {
        var action = Client.Webhooks.Create(_channelId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());
    }

    [Fact]
    public void SetName_Empty_Throws()
        => Assert.ThrowsAny<ArgumentException>(() => Client.Webhooks.Create(_channelId).SetName("   "));

    [Fact]
    public void SetName_TooLong_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            Client.Webhooks.Create(_channelId).SetName(new string('a', 81)));
}
