using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class GuildCategoryChannelWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task GetChannels_GetsGuildChannelsRouteAsync()
	{
		var guild = Substitute.For<IGuild>();
		guild.Id.Returns(new Snowflake(100));
		Http.SendAsync<Channel[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);
		var wrapper = new GuildCategoryChannelWrapper(Client,
			new Channel { Id = new Snowflake(200), Type = ChannelType.GuildCategory }, guild);

		await wrapper.GetChannels().ExecuteAsync();

		await Http.Received(1).SendAsync<Channel[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/channels"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}
