using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Other;

public class WebhookWrapperTests : WrapperTestBase
{
	private static Webhook Model(Snowflake? id = null) => new() { Id = id ?? new Snowflake(42) };

	[Fact]
	public async Task Modify_PatchesWebhookWithBodyAsync()
	{
		Http.SendAsync<Webhook>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(Model());
		var wrapper = new WebhookWrapper(Client, Model());

		await wrapper.Modify().SetName("new").SetAvatar("data:img").ExecuteAsync();

		await Http.Received(1).SendAsync<Webhook>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/42"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyContains(b, "name", "new") && BodyContains(b, "avatar", "data:img")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesWebhookAsync()
	{
		var wrapper = new WebhookWrapper(Client, Model());

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/42"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
