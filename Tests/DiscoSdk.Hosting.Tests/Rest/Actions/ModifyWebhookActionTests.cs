using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class ModifyWebhookActionTests : WrapperTestBase
{
    private readonly Snowflake _webhookId = new(42);

    public ModifyWebhookActionTests()
    {
        Http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new Webhook { Id = _webhookId });
    }

    private ModifyWebhookAction Action() => new(Client, _webhookId);

    [Fact]
    public async Task ExecuteAsync_OnlySetFields_OmitsUnsetAsync()
    {
        await Action().SetName("new").ExecuteAsync();

        await Http.Received(1).SendAsync<Webhook>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"webhooks/{_webhookId}"),
            HttpMethod.Patch,
            Arg.Is<object?>(b =>
                BodyContains(b, "name", "new") &&
                !BodyHasKey(b, "avatar") && !BodyHasKey(b, "channel_id")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearAvatar_SendsExplicitNullAsync()
    {
        await Action().ClearAvatar().ExecuteAsync();

        await Http.Received(1).SendAsync<Webhook>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyHasKey(b, "avatar") && BodyContains(b, "avatar", null)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAvatar_SendsDataUriAsync()
    {
        await Action().SetAvatar("data:img").ExecuteAsync();

        await Http.Received(1).SendAsync<Webhook>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyContains(b, "avatar", "data:img")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveToChannel_SerializesChannelIdAsStringAsync()
    {
        await Action().MoveToChannel(new Snowflake(9)).ExecuteAsync();

        await Http.Received(1).SendAsync<Webhook>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch,
            Arg.Is<object?>(b => BodyContains(b, "channel_id", "9")),
            Arg.Any<CancellationToken>());
    }
}
