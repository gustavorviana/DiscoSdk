using DiscoSdk.Contexts.Channels;
using DiscoSdk.Events;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Models;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Gateway.Events;

/// <summary>
/// Verifies CHANNEL_CREATE / CHANNEL_UPDATE / CHANNEL_DELETE dispatch. Each test first seeds a
/// GUILD_CREATE because TryGetChannelGuild requires the guild to be cached.
/// </summary>
public class ChannelDispatchTests : DispatcherTestBase
{
	[Fact]
	public async Task ChannelCreate_InvokesIChannelCreateHandlerAsync()
	{
		var handler = Substitute.For<IChannelCreateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));

		await DispatchAsync(DispatchFrames.ChannelCreate(id: 200, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IChannelContext>(ctx => ctx.Channel.Id == new Snowflake(200)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ChannelUpdate_InvokesIChannelUpdateHandlerAsync()
	{
		var handler = Substitute.For<IChannelUpdateHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.ChannelCreate(id: 200, guildId: 100));

		await DispatchAsync(DispatchFrames.ChannelUpdate(id: 200, guildId: 100, name: "renamed"));

		await handler.Received(1).HandleAsync(
			Arg.Is<IChannelContext>(ctx => ctx.Channel.Id == new Snowflake(200)),
			Arg.Any<IServiceProvider>());
	}

	[Fact]
	public async Task ChannelDelete_InvokesIChannelDeleteHandlerAsync()
	{
		var handler = Substitute.For<IChannelDeleteHandler>();
		AddHandler(handler);
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.ChannelCreate(id: 200, guildId: 100));

		await DispatchAsync(DispatchFrames.ChannelDelete(id: 200, guildId: 100));

		await handler.Received(1).HandleAsync(
			Arg.Is<IChannelDeleteContext>(ctx => ctx.ChannelId == new Snowflake(200)),
			Arg.Any<IServiceProvider>());
	}
}
