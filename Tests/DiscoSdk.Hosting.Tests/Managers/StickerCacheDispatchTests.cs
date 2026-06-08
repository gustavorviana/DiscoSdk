using DiscoSdk.Caching;
using DiscoSdk.Hosting.Caching;
using DiscoSdk.Hosting.Tests.Gateway.Common;
using DiscoSdk.Hosting.Tests.Gateway.Events;
using DiscoSdk.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoSdk.Hosting.Tests.Managers;

/// <summary>
/// Integration tests for the sticker cache wiring. The cache is opt-in — the
/// <see cref="DispatcherTestBase"/> wires its own client with <see cref="StickerCacheFlag.Guild"/>
/// enabled via the test DI container so the dispatcher updates can be observed through
/// the scope exposed by <see cref="IGuild.Stickers"/>.
/// </summary>
public class StickerCacheDispatchTests : DispatcherTestBase
{
	public StickerCacheDispatchTests()
	{
		// Hosts that don't call WithStickerCache start at StickerCacheFlag.None, which makes the
		// manager a no-op. Override the DI registration after the client is built so the manager
		// still observes Guild scope for these tests.
		ReplaceStickerFlags(StickerCacheFlag.Guild);
	}

	private void ReplaceStickerFlags(StickerCacheFlag flags)
	{
		// Replace the singleton in the real services container the client was built with. The
		// dispatched DiscordClient was constructed before this, so reach in via reflection and
		// rebuild the StickerManager so the test sees the new flags.
		var managerField = typeof(DiscordClient).GetProperty(nameof(DiscordClient.StickersInternal),
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
		var manager = (Hosting.Managers.StickerManager)managerField!.GetValue(Client)!;

		var flagsField = typeof(Hosting.Managers.StickerManager).GetField("_flags",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
		flagsField!.SetValue(manager, flags);
	}

	[Fact]
	public async Task GuildStickersUpdate_ReplacesCachedListAsync()
	{
		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildStickersUpdate(guildId: 100,
			(1UL, "alpha"),
			(2UL, "beta")));

		var guild = Client.Guilds.TryGet(new Snowflake(100), out var g) ? g : null;
		Assert.NotNull(guild);

		var stickers = guild!.Stickers.GetCached();
		Assert.Equal(2, stickers.Count);
		Assert.Contains(stickers, s => s.Id == new Snowflake(1) && s.Name == "alpha");
		Assert.Contains(stickers, s => s.Id == new Snowflake(2) && s.Name == "beta");

		await DispatchAsync(DispatchFrames.GuildStickersUpdate(guildId: 100, (3UL, "gamma")));

		stickers = guild.Stickers.GetCached();
		Assert.Single(stickers);
		Assert.Equal("gamma", stickers[0].Name);
	}

	[Fact]
	public async Task GuildStickersUpdate_FlagNone_KeepsCacheEmptyAsync()
	{
		ReplaceStickerFlags(StickerCacheFlag.None);

		await DispatchAsync(DispatchFrames.GuildCreate(id: 100));
		await DispatchAsync(DispatchFrames.GuildStickersUpdate(guildId: 100, (1UL, "alpha")));

		var guild = Client.Guilds.TryGet(new Snowflake(100), out var g) ? g : null;
		Assert.NotNull(guild);
		Assert.Empty(guild!.Stickers.GetCached());
	}
}
