using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers;

/// <summary>
/// Verifies the public actions on <see cref="StageInstanceWrapper"/> hit the right Discord REST
/// routes — no Discord round-trip, the <see cref="WrapperTestBase.Http"/> mock captures the calls.
/// </summary>
public class StageInstanceWrapperTests : WrapperTestBase
{
	private readonly Snowflake _channelId = new(200);
	private readonly Snowflake _guildId = new(100);

	private StageInstanceWrapper NewWrapper(string topic = "Town Hall") =>
		new(Client, new StageInstance
		{
			Id = new Snowflake(500),
			GuildId = _guildId,
			ChannelId = _channelId,
			Topic = topic,
			PrivacyLevel = StagePrivacyLevel.GuildOnly,
		});

	[Fact]
	public async Task Modify_PatchesStageInstanceByChannelIdAsync()
	{
		Http.SendAsync<StageInstance>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new StageInstance { Id = new Snowflake(500), Topic = "New" });

		var stage = NewWrapper();
		await stage.Modify(topic: "New").ExecuteAsync();

		await Http.Received(1).SendAsync<StageInstance>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"stage-instances/{_channelId}"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "Topic", "New")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesStageInstanceByChannelIdAsync()
	{
		var stage = NewWrapper();
		await stage.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"stage-instances/{_channelId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}
}
