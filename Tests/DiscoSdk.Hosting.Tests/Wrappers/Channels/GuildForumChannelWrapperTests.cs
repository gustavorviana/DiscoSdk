using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class GuildForumChannelWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public GuildForumChannelWrapperTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private GuildForumChannelWrapper NewWrapper()
		=> new(Client, new Channel { Id = new Snowflake(200), Type = ChannelType.GuildForum }, _guild);

	[Fact]
	public async Task StartPost_PostsForumPostRouteAsync()
	{
		Http.SendAsync<ForumPost>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new ForumPost { Channel = new Channel { Id = new Snowflake(999), Type = ChannelType.PublicThread, GuildId = new Snowflake(100) } });

		await NewWrapper().StartPost("topic").SetMessageContent("hello").ExecuteAsync();

		await Http.Received(1).SendAsync<ForumPost>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/threads"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateThreadChannel_FromMessage_PostsMessageThreadsRouteAsync()
	{
		Http.SendAsync<ForumPost>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new ForumPost());
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel { Id = new Snowflake(999), Type = ChannelType.PublicThread });

		await NewWrapper().CreateThreadChannel("thread", new Snowflake(300), isPrivate: false).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300/threads"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}
