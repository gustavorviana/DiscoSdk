using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest;
using NSubstitute;
using System.Reflection;

namespace DiscoSdk.Hosting.Tests.Wrappers.Other;

public class InteractionWrapperTests : WrapperTestBase
{
	private readonly Snowflake _appId = new(900);

	public InteractionWrapperTests()
	{
		// ApplicationId is normally set on Ready — set it here via reflection so the wrapper's
		// Delete route (which uses ApplicationId) resolves cleanly.
		typeof(DiscordClient).GetProperty(nameof(DiscordClient.ApplicationId))!.SetValue(Client, _appId);
	}

	private InteractionWrapper NewWrapper(out InteractionHandle handle)
	{
		var interaction = new Interaction
		{
			Id = new Snowflake(50),
			ApplicationId = _appId,
			Type = InteractionType.ApplicationCommand,
			Token = "tok",
			ChannelId = new Snowflake(200),
		};
		handle = new InteractionHandle(interaction.Id, interaction.Token);
		var channel = Substitute.For<ITextBasedChannel>();
		channel.Id.Returns(new Snowflake(200));
		return new InteractionWrapper(interaction, Client, handle, channel, member: null);
	}

	[Fact]
	public async Task Defer_PostsInteractionCallbackRouteAsync()
	{
		var wrapper = NewWrapper(out _);

		await wrapper.Defer().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "interactions/50/tok/callback"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Defer_TwiceOnSameHandle_DoesNotRetransmitAsync()
	{
		var wrapper = NewWrapper(out var handle);

		await wrapper.Defer().ExecuteAsync();
		// Second call should be a no-op since IsDeferred latches true.
		await wrapper.Defer().ExecuteAsync();

		Assert.True(handle.IsDeferred);
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "interactions/50/tok/callback"),
			HttpMethod.Post,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesOriginalResponseAsync()
	{
		var wrapper = NewWrapper(out _);

		await wrapper.Delete().ExecuteAsync();

		// Delete uses WebhookIdentity(appId, token) → DELETE /webhooks/{app_id}/{token}/messages/@original
		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "webhooks/900/tok/messages/@original"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
