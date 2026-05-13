using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

public class ApplicationCommandClientTests
{
	private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
	private readonly ApplicationCommandClient _client;
	private readonly Snowflake _appId = new(1234567890ul);
	private readonly Snowflake _guildId = new(9999999999ul);

	public ApplicationCommandClientTests()
	{
		_client = new ApplicationCommandClient(_http);
	}

	[Fact]
	public async Task RegisterGlobalCommandsAsync_PutsToApplicationCommandsRouteAsync()
	{
		_http.SendAsync<List<SlashCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new List<SlashCommand>());

		var commands = new List<SlashCommand> { new() { Name = "ping" } };
		await _client.RegisterGlobalCommandsAsync(_appId, commands);

		await _http.Received(1).SendAsync<List<SlashCommand>>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
			HttpMethod.Put,
			commands,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RegisterGuildCommandsAsync_PutsToApplicationGuildCommandsRouteAsync()
	{
		_http.SendAsync<List<SlashCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new List<SlashCommand>());

		await _client.RegisterGuildCommandsAsync(_appId, _guildId, new List<SlashCommand>());

		await _http.Received(1).SendAsync<List<SlashCommand>>(
			Arg.Is<DiscordRoute>(r =>
				r.Template == "applications/{application_id}/guilds/{guild_id}/commands" &&
				r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands"),
			HttpMethod.Put,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetGlobalCommandsAsync_GetsListRouteAsync()
	{
		_http.SendAsync<List<SlashCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new List<SlashCommand>());

		await _client.GetGlobalCommandsAsync(_appId);

		await _http.Received(1).SendAsync<List<SlashCommand>>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteGlobalCommandAsync_DeletesByIdAsync()
	{
		await _client.DeleteGlobalCommandAsync(_appId, "777");

		await _http.Received(1).SendAsync(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands/777"),
			HttpMethod.Delete,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetGlobalCommandAsync_GetsSingleCommandAsync()
	{
		var commandId = new Snowflake(42);
		var expected = new SlashCommand { Id = commandId, Name = "ping" };
		_http.SendAsync<SlashCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(expected);

		var result = await _client.GetGlobalCommandAsync(_appId, commandId);

		Assert.Same(expected, result);
		await _http.Received(1).SendAsync<SlashCommand>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands/{commandId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateGlobalCommandAsync_PostsTheCommandBodyAsync()
	{
		var cmd = new SlashCommand { Name = "echo", Description = "echo back" };
		_http.SendAsync<SlashCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(cmd);

		await _client.CreateGlobalCommandAsync(_appId, cmd);

		await _http.Received(1).SendAsync<SlashCommand>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
			HttpMethod.Post,
			cmd,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditGlobalCommandAsync_PatchesByIdAsync()
	{
		var commandId = new Snowflake(42);
		_http.SendAsync<SlashCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new SlashCommand { Name = "x" });

		await _client.EditGlobalCommandAsync(_appId, commandId, new { name = "x" });

		await _http.Received(1).SendAsync<SlashCommand>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands/{commandId}"),
			HttpMethod.Patch,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetGuildCommandAsync_GetsSingleGuildCommandAsync()
	{
		var commandId = new Snowflake(42);
		_http.SendAsync<SlashCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new SlashCommand());

		await _client.GetGuildCommandAsync(_appId, _guildId, commandId);

		await _http.Received(1).SendAsync<SlashCommand>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands/{commandId}"),
			HttpMethod.Get,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateGuildCommandAsync_PostsToGuildScopeAsync()
	{
		var cmd = new SlashCommand { Name = "guild-cmd" };
		_http.SendAsync<SlashCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(cmd);

		await _client.CreateGuildCommandAsync(_appId, _guildId, cmd);

		await _http.Received(1).SendAsync<SlashCommand>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands"),
			HttpMethod.Post,
			cmd,
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditGuildCommandAsync_PatchesGuildScopeAsync()
	{
		var commandId = new Snowflake(42);
		_http.SendAsync<SlashCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
			.Returns(new SlashCommand());

		await _client.EditGuildCommandAsync(_appId, _guildId, commandId, new { name = "x" });

		await _http.Received(1).SendAsync<SlashCommand>(
			Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands/{commandId}"),
			HttpMethod.Patch,
			Arg.Any<object?>(),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateGlobalCommandAsync_NullCommand_ThrowsAsync()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() => _client.CreateGlobalCommandAsync(_appId, null!));
	}
}
