using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

/// <summary>
/// Tests <see cref="TextBasedChannelWrapper"/>'s message / pin / reaction / poll routes. Constructs
/// the wrapper directly (it's the base for both DM and guild text channels).
/// </summary>
public class TextBasedChannelWrapperTests : WrapperTestBase
{
	private TextBasedChannelWrapper NewWrapper()
		=> new(Client, new Channel { Id = new Snowflake(200), Type = ChannelType.GuildText });

	private static Message NewMessage()
		=> new() { Id = new Snowflake(300), Author = new User { UserId = new Snowflake(1), Username = "u" } };

	[Fact]
	public async Task GetMessagesAsync_GetsChannelMessagesAsync()
	{
		Http.SendAsync<Message[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await NewWrapper().GetMessagesAsync(limit: 50).ExecuteAsync();

		await Http.Received(1).SendAsync<Message[]>(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("channels/200/messages") && r.ToString().Contains("limit=50")),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetMessageAsync_GetsMessageByIdAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(NewMessage());

		await NewWrapper().GetMessageAsync(new Snowflake(300)).ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteMessageAsync_DeletesMessageRouteAsync()
	{
		await NewWrapper().DeleteMessageAsync(new Snowflake(300)).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300"),
			HttpMethod.Delete, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BulkDeleteMessagesAsync_PostsBulkDeleteRouteAsync()
	{
		await NewWrapper().BulkDeleteMessagesAsync([new Snowflake(300), new Snowflake(301)]).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/bulk-delete"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task TriggerTypingAsync_PostsTypingRouteAsync()
	{
		await NewWrapper().TriggerTypingAsync().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/typing"),
			HttpMethod.Post, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SendMessage_PostsChannelMessagesAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(NewMessage());

		await NewWrapper().SendMessage("hello").ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages"),
			HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddReactionByIdAsync_PutsReactionMeRouteAsync()
	{
		await NewWrapper().AddReactionByIdAsync(new Snowflake(300), new Emoji { Name = "smile" }).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("channels/200/messages/300/reactions/") && r.ToString().EndsWith("/@me")),
			HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RemoveReactionByIdAsync_DeletesReactionMeRouteAsync()
	{
		await NewWrapper().RemoveReactionByIdAsync(new Snowflake(300), new Emoji { Name = "smile" }).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString().StartsWith("channels/200/messages/300/reactions/") && r.ToString().EndsWith("/@me")),
			HttpMethod.Delete, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task PinMessageByIdAsync_PutsPinRouteAsync()
	{
		await NewWrapper().PinMessageByIdAsync(new Snowflake(300)).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/pins/300"),
			HttpMethod.Put, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UnpinMessageByIdAsync_DeletesPinRouteAsync()
	{
		await NewWrapper().UnpinMessageByIdAsync(new Snowflake(300)).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/pins/300"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RetrievePinnedMessages_GetsPinsRouteAsync()
	{
		Http.SendAsync<Message[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		await NewWrapper().RetrievePinnedMessages().ExecuteAsync();

		await Http.Received(1).SendAsync<Message[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/pins"),
			HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditMessageById_PatchesMessageRouteAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(NewMessage());

		await NewWrapper().EditMessageById(new Snowflake(300)).SetContent("new").ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/messages/300"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EndPollByIdAsync_PostsExpireRouteAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(NewMessage());

		await NewWrapper().EndPollByIdAsync(new Snowflake(300)).ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200/polls/300/expire"),
			HttpMethod.Post, Arg.Is<object?>(b => b == null), Arg.Any<CancellationToken>());
	}
}
