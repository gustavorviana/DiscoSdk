using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Messages;

public class MessageWrapperTests : WrapperTestBase
{
	private readonly Snowflake _channelId = new(200);
	private readonly Snowflake _messageId = new(300);
	private readonly Snowflake _guildId = new(100);

	private (MessageWrapper wrapper, ITextBasedChannel channel) NewWrapper(MessageFlags flags = MessageFlags.None)
	{
		var channel = Substitute.For<ITextBasedChannel>();
		channel.Id.Returns(_channelId);
		var msg = new Message
		{
			Id = _messageId,
			ChannelId = _channelId,
			GuildId = _guildId,
			Content = "hello",
			Timestamp = "2024-01-01T00:00:00+00:00",
			Author = new User { UserId = new Snowflake(1), Username = "bot" },
			Flags = flags,
			Mentions = [],
			Reactions = [],
		};
		return (new MessageWrapper(Client, channel, msg, interactionHandle: null), channel);
	}

	[Fact]
	public async Task Delete_DeletesMessageRouteAsync()
	{
		var (wrapper, _) = NewWrapper();

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300"),
			HttpMethod.Delete, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Crosspost_PostsCrosspostRouteAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message { Author = new User { UserId = new Snowflake(1), Username = "bot" } });
		var (wrapper, _) = NewWrapper();

		await wrapper.Crosspost().ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300/crosspost"),
			HttpMethod.Post, Arg.Is<object?>(b => b == null), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddReaction_PutsReactionMeRouteAsync()
	{
		var (wrapper, _) = NewWrapper();

		await wrapper.AddReaction("👍").ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("channels/200/messages/300/reactions/") && r.ToString().EndsWith("/@me")),
			HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetReactions_GetsReactionUsersRouteAsync()
	{
		Http.SendAsync<User[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);
		var (wrapper, _) = NewWrapper();

		await wrapper.GetReactions("👍", limit: 10).ExecuteAsync();

		await Http.Received(1).SendAsync<User[]>(
			Arg.Is<DiscordRoute>(r => r.ToString().Contains("/reactions/") && r.ToString().Contains("limit=10")),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAllReactionsForEmoji_DeletesAllReactionsForEmojiRouteAsync()
	{
		var (wrapper, _) = NewWrapper();

		await wrapper.DeleteAllReactionsForEmoji("👍").ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith("channels/200/messages/300/reactions/") &&
				!r.ToString().EndsWith("/@me")),
			HttpMethod.Delete, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAllReactions_DeletesAllReactionsRouteAsync()
	{
		var (wrapper, _) = NewWrapper();

		await wrapper.DeleteAllReactions().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300/reactions"),
			HttpMethod.Delete, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Pin_PutsPinRouteAsync()
	{
		var (wrapper, _) = NewWrapper();

		await wrapper.Pin().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/pins/300"),
			HttpMethod.Put, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Unpin_DeletesPinRouteAsync()
	{
		var (wrapper, _) = NewWrapper();

		await wrapper.Unpin().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/pins/300"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Reply_PostsChannelMessagesWithReferenceAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message { Author = new User { UserId = new Snowflake(1), Username = "bot" } });
		var (wrapper, _) = NewWrapper();

		await wrapper.Reply("hi back").ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}
