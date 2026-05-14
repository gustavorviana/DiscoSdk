using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

/// <summary>
/// Tests for the three command-permission methods on <see cref="ApplicationCommandClient"/>.
/// </summary>
public class ApplicationCommandPermissionsClientTests
{
    private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
    private readonly ApplicationCommandClient _client;
    private readonly Snowflake _appId = new(100);
    private readonly Snowflake _guildId = new(200);
    private readonly Snowflake _commandId = new(300);

    public ApplicationCommandPermissionsClientTests()
    {
        _http.JsonOptions.Returns(new JsonSerializerOptions());
        _client = new ApplicationCommandClient(_http);
    }

    [Fact]
    public async Task GetGuildCommandsPermissionsAsync_GetsCorrectRouteAsync()
    {
        _http.SendAsync<ApplicationCommandPermissions[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _client.GetGuildCommandsPermissionsAsync(_appId, _guildId);

        await _http.Received(1).SendAsync<ApplicationCommandPermissions[]>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands/permissions"),
            HttpMethod.Get,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCommandPermissionsAsync_GetsCorrectRouteAsync()
    {
        _http.SendAsync<ApplicationCommandPermissions>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommandPermissions());

        await _client.GetCommandPermissionsAsync(_appId, _guildId, _commandId);

        await _http.Received(1).SendAsync<ApplicationCommandPermissions>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands/{_commandId}/permissions"),
            HttpMethod.Get,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditCommandPermissionsAsync_PutsWithBearerAuthAsync()
    {
        _http.SendAsync<ApplicationCommandPermissions>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommandPermissions());

        var perms = new[]
        {
            new ApplicationCommandPermission { Id = new Snowflake(1), Type = ApplicationCommandPermissionType.Role, Permission = true },
        };

        await _client.EditCommandPermissionsAsync(_appId, _guildId, _commandId, perms, "user-bearer-token");

        await _http.Received(1).SendAsync<ApplicationCommandPermissions>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands/{_commandId}/permissions"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Bearer" && h.Parameter == "user-bearer-token"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void EditCommandPermissionsAsync_RejectsEmptyBearerToken()
    {
        var perms = Array.Empty<ApplicationCommandPermission>();
        Assert.ThrowsAsync<ArgumentException>(() =>
            _client.EditCommandPermissionsAsync(_appId, _guildId, _commandId, perms, ""));
    }
}
