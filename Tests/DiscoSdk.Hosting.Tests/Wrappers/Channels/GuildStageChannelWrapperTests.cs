using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Channels;

public class GuildStageChannelWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public GuildStageChannelWrapperTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private GuildStageChannelWrapper NewWrapper()
		=> new(Client, new Channel { Id = new Snowflake(200), Type = ChannelType.GuildStageVoice }, _guild);

	[Fact]
	public async Task RequestToSpeak_PatchesMeVoiceStateRouteAsync()
	{
		var wrapper = NewWrapper();

		await wrapper.RequestToSpeak().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/voice-states/@me"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "channel_id", "200")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelRequestToSpeak_PatchesMeVoiceStateRouteWithNullTimestampAsync()
	{
		var wrapper = NewWrapper();

		await wrapper.CancelRequestToSpeak().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/voice-states/@me"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}
