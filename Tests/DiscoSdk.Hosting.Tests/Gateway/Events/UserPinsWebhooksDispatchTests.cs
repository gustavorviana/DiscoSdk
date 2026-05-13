using DiscoSdk.Contexts;
using DiscoSdk.Contexts.Channels;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>USER_UPDATE / CHANNEL_PINS_UPDATE / WEBHOOKS_UPDATE dispatch routes.</summary>
public class UserPinsWebhooksDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task UserUpdate_InvokesIUserUpdateHandlerAsync()
	{
		var handler = Substitute.For<IUserUpdateHandler>();
		AddHandler(handler);

		await DispatchAsync(DispatchFrames.UserUpdate(userId: 42, username: "newname"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IUserUpdateContext>(ctx => ctx.User.Id == new Snowflake(42) && ctx.User.Username == "newname"),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ChannelPinsUpdate_InvokesIChannelPinsUpdateHandlerAsync()
	{
		var handler = Substitute.For<IChannelPinsUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ChannelPinsUpdate(channelId: 200, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Any<IChannelPinsUpdateContext>(),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task WebhooksUpdate_InvokesIWebhooksUpdateHandlerAsync()
	{
		var handler = Substitute.For<IWebhooksUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.WebhooksUpdate(guildId: 100, channelId: 200));

		await handler.Received(1).HandleAsync(
			Arg.Is<IWebhooksUpdateContext>(ctx =>
				ctx.Guild.Id == new Snowflake(100) &&
				ctx.ChannelId == new Snowflake(200)),
			Arg.Any<IServiceProvider>());
	}
}
