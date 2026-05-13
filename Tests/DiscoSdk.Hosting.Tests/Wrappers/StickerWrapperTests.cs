using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers;

/// <summary>
/// Verifies the public actions on <see cref="StickerWrapper"/> hit the right Discord REST routes.
/// </summary>
public class StickerWrapperTests : WrapperTestBase
{
	private readonly Snowflake _guildId = new(100);
	private readonly Snowflake _stickerId = new(1200);

	private StickerWrapper NewGuildSticker() =>
		new(Client, new Sticker
		{
			Id = _stickerId,
			GuildId = _guildId,
			Name = "fancy",
			Type = StickerType.Guild,
			FormatType = StickerFormatType.Png,
		});

	private StickerWrapper NewStandardSticker() =>
		new(Client, new Sticker
		{
			Id = _stickerId,
			Name = "wave",
			Type = StickerType.Standard,
			FormatType = StickerFormatType.Png,
		});

	[Fact]
	public async Task Modify_PatchesGuildStickerAsync()
	{
		Http.SendAsync<Sticker>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Sticker { Id = _stickerId, GuildId = _guildId, Name = "renamed" });

		await NewGuildSticker().Modify().SetName("renamed").ExecuteAsync();

		await Http.Received(1).SendAsync<Sticker>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/stickers/{_stickerId}"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "Name", "renamed")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesGuildStickerAsync()
	{
		await NewGuildSticker().Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/stickers/{_stickerId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public void Modify_OnStandardSticker_Throws()
	{
		Assert.Throws<InvalidOperationException>(() => NewStandardSticker().Modify());
	}

	[Fact]
	public void Delete_OnStandardSticker_Throws()
	{
		Assert.Throws<InvalidOperationException>(() => NewStandardSticker().Delete());
	}
}
