using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Other;

public class ReactionWrapperTests : WrapperTestBase
{
	private (IMessage message, ITextBasedChannel channel) NewDmMessage()
	{
		var channel = Substitute.For<ITextBasedChannel>();
		channel.Id.Returns(new Snowflake(200));
		var author = Substitute.For<IUser>();
		author.Id.Returns(new Snowflake(50));
		var message = Substitute.For<IMessage>();
		message.Id.Returns(new Snowflake(300));
		message.Channel.Returns(channel);
		message.Guild.Returns((IGuild?)null);
		message.Author.Returns(author);
		message.Flags.Returns(MessageFlags.None);
		return (message, channel);
	}

	[Fact]
	public async Task Delete_OwnReaction_RemovesOwnReactionAsync()
	{
		var (message, _) = NewDmMessage();
		var emoji = new Emoji { Name = "smile" };
		var wrapper = new ReactionWrapper(new Reaction { Count = 1, Me = true, Emoji = emoji }, message, Client);

		await wrapper.Delete().ExecuteAsync();

		// Own reaction → DELETE /channels/{cid}/messages/{mid}/reactions/{emoji}/@me
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith("channels/200/messages/300/reactions/") &&
				r.ToString().EndsWith("/@me")),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_OtherReaction_RemovesAuthorReactionAsync()
	{
		var (message, _) = NewDmMessage();
		var emoji = new Emoji { Name = "smile" };
		var wrapper = new ReactionWrapper(new Reaction { Count = 1, Me = false, Emoji = emoji }, message, Client);

		await wrapper.Delete().ExecuteAsync();

		// Not me → DELETE /channels/{cid}/messages/{mid}/reactions/{emoji}/{author_id}
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r =>
				r.ToString().StartsWith("channels/200/messages/300/reactions/") &&
				r.ToString().EndsWith("/50")),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}
