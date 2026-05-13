using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class GuildNewsChannelWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public GuildNewsChannelWrapperTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private GuildNewsChannelWrapper NewWrapper()
		=> new(Client, new Channel { Id = new Snowflake(200), Type = ChannelType.GuildAnnouncement }, _guild);

	[Fact]
	public async Task CrosspostMessage_PostsCrosspostRouteAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message { Author = new DiscoSdk.Models.Users.User { UserId = new Snowflake(1), Username = "u" } });

		await NewWrapper().CrosspostMessage(new Snowflake(300)).ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300/crosspost"),
			HttpMethod.Post, Arg.Is<object?>(b => b == null), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Follow_PostsFollowersRouteAsync()
	{
		Http.SendAsync<FollowedChannel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new FollowedChannel());

		await NewWrapper().Follow(new Snowflake(999)).ExecuteAsync();

		await Http.Received(1).SendAsync<FollowedChannel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/followers"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "webhook_channel_id", "999")),
			Arg.Any<CancellationToken>());
	}
}
