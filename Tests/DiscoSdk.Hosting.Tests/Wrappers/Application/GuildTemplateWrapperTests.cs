using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Wrappers.Application;

public class GuildTemplateWrapperTests : WrapperTestBase
{
	private static GuildTemplate Model() => new()
	{
		Code = "abc",
		Creator = new User { UserId = new Snowflake(1) },
		SourceGuildId = new Snowflake(100),
		SerializedSourceGuild = new Guild(),
	};

	[Fact]
	public async Task CreateGuild_PostsToTemplateCodeRouteAsync()
	{
		Http.SendAsync<Guild>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Guild());
		var wrapper = new GuildTemplateWrapper(Client, Model());

		await wrapper.CreateGuild("NewGuild").ExecuteAsync();

		await Http.Received(1).SendAsync<Guild>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/templates/abc"),
			HttpMethod.Post,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "name", "NewGuild")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Sync_PutsTemplateCodeOnSourceGuildRouteAsync()
	{
		Http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(Model());
		var wrapper = new GuildTemplateWrapper(Client, Model());

		await wrapper.Sync().ExecuteAsync();

		await Http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/templates/abc"),
			HttpMethod.Put,
			Arg.Is<object?>(b => b == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Modify_PatchesTemplateAsync()
	{
		Http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(Model());
		var wrapper = new GuildTemplateWrapper(Client, Model());

		await wrapper.Modify(name: "newName").ExecuteAsync();

		await Http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/templates/abc"),
			HttpMethod.Patch,
			Arg.Is<object?>(b => BodyPropertyEquals(b, "name", "newName")),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Delete_DeletesTemplateAsync()
	{
		Http.SendAsync<GuildTemplate>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(Model());
		var wrapper = new GuildTemplateWrapper(Client, Model());

		await wrapper.Delete().ExecuteAsync();

		await Http.Received(1).SendAsync<GuildTemplate>(
			Arg.Is<DiscordRoute>(r => r.ToString() == "guilds/100/templates/abc"),
			HttpMethod.Delete,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}
}
