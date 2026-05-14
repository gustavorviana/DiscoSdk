using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Requests.Channels;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class GroupDmChannelWrapperTests : WrapperTestBase
{
	private readonly Snowflake _channelId = new(42);
	private readonly Snowflake _userId = new(99);

	private GroupDmChannelWrapper NewWrapper()
		=> new(Client, new Channel { Id = _channelId });

	[Fact]
	public async Task AddRecipient_PutsRecipientsRouteWithTypedBodyAsync()
	{
		await NewWrapper().AddRecipient(_userId, "tok-abc", nick: "buddy").ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/recipients/{_userId}"),
			HttpMethod.Put,
			Arg.Is<object?>(b =>
				(b as GroupDmAddRecipientRequest)!.AccessToken == "tok-abc" &&
				(b as GroupDmAddRecipientRequest)!.Nick == "buddy"),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AddRecipient_OmitsNickWhenNullAsync()
	{
		await NewWrapper().AddRecipient(_userId, "tok-abc").ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Any<DiscordRoute>(),
			HttpMethod.Put,
			Arg.Is<object?>(b => (b as GroupDmAddRecipientRequest)!.Nick == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public void AddRecipient_RejectsEmptyAccessToken()
	{
		Assert.ThrowsAny<ArgumentException>(() => NewWrapper().AddRecipient(_userId, ""));
	}

	[Fact]
	public async Task RemoveRecipient_DeletesRecipientsRouteAsync()
	{
		await NewWrapper().RemoveRecipient(_userId).ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"channels/{_channelId}/recipients/{_userId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
