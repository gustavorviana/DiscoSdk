using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

/// <summary>
/// Tests <see cref="GuildTextBasedChannelWrapper"/>'s message-specific actions through the concrete
/// <see cref="GuildTextChannelWrapper"/> (the base is abstract).
/// </summary>
public class GuildTextBasedChannelWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public GuildTextBasedChannelWrapperTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private GuildTextChannelWrapper NewWrapper()
		=> new(Client, new Channel { Id = new Snowflake(200), Type = ChannelType.GuildText }, _guild);

	[Fact]
	public async Task DeleteAllReactionsAsync_HitsReactionsRouteAsync()
	{
		var wrapper = NewWrapper();

		await wrapper.DeleteAllReactionsAsync(messageId: new Snowflake(300));

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300/reactions"),
			HttpMethod.Delete, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveReactionAsync_HitsReactionMeRouteAsync()
	{
		var wrapper = NewWrapper();

		await wrapper.RemoveReactionAsync(messageId: new Snowflake(300), new Emoji { Name = "smile" });

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("channels/200/messages/300/reactions/") && r.ToString().EndsWith("/@me")),
			HttpMethod.Delete, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}
