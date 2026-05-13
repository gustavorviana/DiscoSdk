using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Messages;

public class WebhookMessageWrapperTests : WrapperTestBase
{
	private readonly WebhookIdentity _identity = new(new Snowflake(900), "tok");
	private readonly WebhookMessageClient _msgClient;

	public WebhookMessageWrapperTests()
	{
		_msgClient = new WebhookMessageClient(Http);
	}

	private WebhookMessageWrapper NewWrapper() => new(_identity, _msgClient, new Message
	{
		Id = new Snowflake(300),
		ChannelId = new Snowflake(200),
		Timestamp = "2024-01-01T00:00:00+00:00",
		Author = new User { UserId = new Snowflake(1), Username = "u" },
		Mentions = [],
	});

	[Fact]
	public async Task Delete_DeletesWebhookMessageAsync()
	{
		var wrapper = NewWrapper();

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/900/tok/messages/300"),
			HttpMethod.Delete, Arg.Is<object?>(b => b == null), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Edit_PatchesWebhookMessageAsync()
	{
		Http.SendAsync<Message>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Message { Author = new User { UserId = new Snowflake(1), Username = "u" } });
		var wrapper = NewWrapper();

		await wrapper.Edit().SetContent("new").ExecuteAsync();

		await Http.Received(1).SendAsync<Message>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/900/tok/messages/300"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}
