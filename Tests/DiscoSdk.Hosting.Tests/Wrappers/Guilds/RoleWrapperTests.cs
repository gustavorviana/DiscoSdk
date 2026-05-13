using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Guilds;

public class RoleWrapperTests : WrapperTestBase
{
	private readonly IGuild _guild;
	private readonly Snowflake _roleId = new(42);

	public RoleWrapperTests()
	{
		_guild = Substitute.For<IGuild>();
		_guild.Id.Returns(new Snowflake(100));
	}

	private Role Model() => new() { Id = _roleId, Name = "Admin" };

	[Fact]
	public async Task Edit_PatchesRoleAsync()
	{
		Http.SendAsync<Role>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(Model());
		var wrapper = new RoleWrapper(Client, Model(), _guild);

		await wrapper.Edit().SetName("new").SetHoist(true).ExecuteAsync();

		await Http.Received(1).SendAsync<Role>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/roles/42"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyContains(b, "name", "new") && BodyContains(b, "hoist", true)),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesRoleAsync()
	{
		var wrapper = new RoleWrapper(Client, Model(), _guild);

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/roles/42"),
			HttpMethod.Delete, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyPosition_PatchesGuildRolesRouteAsync()
	{
		Http.SendAsync<Role[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);
		var wrapper = new RoleWrapper(Client, Model(), _guild);

		await wrapper.ModifyPosition(7).ExecuteAsync();

		await Http.Received(1).SendAsync<Role[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/roles"),
			HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
	}
}
