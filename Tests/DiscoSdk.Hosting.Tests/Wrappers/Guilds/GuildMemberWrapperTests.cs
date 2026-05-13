using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Guilds;

public class GuildMemberWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;

	public GuildMemberWrapperTests()
	{
		// Real GuildWrapper so the member's Ban/Kick path can route through it.
		_guild = new GuildWrapper(new Guild { Id = new Snowflake(100), Name = "g" }, Client);
	}

	private GuildMember NewModel() => new()
	{
		User = new User { UserId = new Snowflake(42), Username = "u" },
		JoinedAt = "2024-01-01T00:00:00+00:00",
		Roles = [],
		Flags = GuildMemberFlags.None,
	};

	[Fact]
	public async Task BanAsync_HitsGuildBanRouteAsync()
	{
		var wrapper = new GuildMemberWrapper(Client, NewModel(), _guild);

		await wrapper.BanAsync(deletionTimeframe: 3);

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/bans/42"),
			HttpMethod.Put,
			Arg.Is<object?>(b => BodyContains(b, "delete_message_days", 3)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task KickAsync_HitsGuildMemberDeleteRouteAsync()
	{
		var wrapper = new GuildMemberWrapper(Client, NewModel(), _guild);

		await wrapper.KickAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/members/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}
}
