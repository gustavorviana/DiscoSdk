using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Managers;

public class ForumChannelManagerWrapperTests : WrapperTestBase
{
	[Fact]
	public async Task ExecuteAsync_PatchesChannelWithForumSettersAsync()
	{
		Http.SendAsync<Channel>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Channel());
		var mgr = new ForumChannelManagerWrapper(new Snowflake(200), Client.ChannelClient);

		mgr.SetTopic("forum-topic");
		mgr.SetDefaultAutoArchiveDuration(ThreadAutoArchiveDuration.OneWeek);
		mgr.SetDefaultSortOrder(SortOrderType.LatestActivity);
		mgr.SetDefaultLayout(ForumLayoutType.GalleryView);
		mgr.SetAvailableTags(["urgent", "bug"]);
		await ((IRestAction)mgr).ExecuteAsync();

		await Http.Received(1).SendAsync<Channel>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "channels/200"),
			HttpMethod.Patch,
			Arg.Is<object?>(body =>
				BodyContains(body, "topic", "forum-topic") &&
				BodyContains(body, "default_auto_archive_duration", (int)ThreadAutoArchiveDuration.OneWeek) &&
				BodyContains(body, "default_sort_order", (int)SortOrderType.LatestActivity) &&
				BodyContains(body, "default_forum_layout", (int)ForumLayoutType.GalleryView) &&
				BodyHasKey(body, "available_tags")),
			Arg.Any<CancellationToken>());
	}
}
