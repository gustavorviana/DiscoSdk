using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class RoleClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly RoleClient _client;
	private readonly Snowflake _guildId = new(111);
	private readonly Snowflake _roleId = new(222);

	public RoleClientTests()
	{
		_client = new RoleClient(_http);
	}

	[Fact]
	public async Task CreateAsync_PostsToGuildRolesAsync()
	{
		_http.SendAsync<Role>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Role());

		var req = new { name = "Admin" };
		await _client.CreateAsync(_guildId, req);

		await _http.Received(1).SendAsync<Role>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/roles"),
			HttpMethod.Post,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditAsync_PatchesGuildRoleByIdAsync()
	{
		_http.SendAsync<Role>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new Role());

		var req = new { name = "X" };
		await _client.EditAsync(_guildId, _roleId, req);

		await _http.Received(1).SendAsync<Role>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/roles/{_roleId}"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteAsync_DeletesGuildRoleByIdAsync()
	{
		await _client.DeleteAsync(_guildId, _roleId);

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/roles/{_roleId}"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModifyPositionsAsync_PatchesGuildRolesRouteAsync()
	{
		_http.SendAsync<Role[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns([]);

		var req = new[] { new { id = "1", position = 1 } };
		await _client.ModifyPositionsAsync(_guildId, req);

		await _http.Received(1).SendAsync<Role[]>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/roles"),
			HttpMethod.Patch,
			req,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAsync_DefaultGuildId_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _client.CreateAsync(default, new { }));
	}

	[Fact]
	public async Task CreateAsync_NullRequest_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() => _client.CreateAsync(_guildId, null!));
	}
}
