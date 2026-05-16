using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers;

/// <summary>
/// Verifies the Edit / Delete paths of <see cref="ApplicationEmojiWrapper"/> route through the
/// ApplicationClient (not the GuildClient) and hit the right Discord routes.
/// </summary>
public class ApplicationEmojiWrapperTests : WrapperTestBase
{
	private readonly Snowflake _appId = new(900);
	private readonly Snowflake _emojiId = new(901);

	public ApplicationEmojiWrapperTests()
	{
		Client.ApplicationId = _appId;
	}

	private ApplicationEmojiWrapper NewWrapper(string name = "smile") =>
		new(Client, new InternalEmoji { Id = _emojiId, Name = name });

	[Fact]
	public async Task Edit_PatchesApplicationEmojiAsync()
	{
		Http.SendAsync<InternalEmoji>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new InternalEmoji { Id = _emojiId, Name = "renamed" });

		await NewWrapper().Edit().SetName("renamed").ExecuteAsync();

		await Http.Received(1).SendAsync<InternalEmoji>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/emojis/{_emojiId}"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "Name", "renamed")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Edit_SetRolesIsNoopOnApplicationEmojiAsync()
	{
		Http.SendAsync<InternalEmoji>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new InternalEmoji { Id = _emojiId, Name = "x" });

		// SetRoles should be silently dropped — only name reaches the wire.
		await NewWrapper().Edit().SetName("x").SetRoles(new Snowflake(123)).ExecuteAsync();

		await Http.Received(1).SendAsync<InternalEmoji>(
			Arg.Any<DiscordRoute>(),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "Name", "x")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesApplicationEmojiAsync()
	{
		await NewWrapper().Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/emojis/{_emojiId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public void Guild_IsAlwaysNull()
	{
		Assert.Null(NewWrapper().Guild);
	}
}
