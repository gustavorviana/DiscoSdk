using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Tests.Wrappers.Common;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;
using System.Net.Http.Headers;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

/// <summary>
/// Tests the fluent builder for editing application-command permissions. Focus is on:
/// (1) the accumulator builds the right wire payload, and (2) <c>ExecuteAsync</c> refuses to send
/// without a bearer token.
/// </summary>
public class EditApplicationCommandPermissionsActionTests : WrapperTestBase
{
    private readonly Snowflake _appId = new(100);
    private readonly Snowflake _guildId = new(200);
    private readonly Snowflake _commandId = new(300);

    public EditApplicationCommandPermissionsActionTests()
    {
        Client.ApplicationId = _appId;
    }

    private EditApplicationCommandPermissionsAction NewAction() => new(Client, _guildId, _commandId);

    [Fact]
    public async Task ExecuteAsync_RefusesWithoutBearerTokenAsync()
    {
        var action = NewAction().AllowRole(new Snowflake(1));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());
        Assert.Contains("WithBearerToken", ex.Message);
    }

    [Fact]
    public async Task AllowRole_PutsRoleTypeAndPermissionTrueAsync()
    {
        Http.SendAsync<ApplicationCommandPermissions>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommandPermissions());

        await NewAction()
            .WithBearerToken("tok")
            .AllowRole(new Snowflake(42))
            .ExecuteAsync();

        await Http.Received(1).SendAsync<ApplicationCommandPermissions>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands/{_commandId}/permissions"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Is<AuthenticationHeaderValue?>(h => h != null && h.Scheme == "Bearer" && h.Parameter == "tok"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DenyUser_AddsUserTypeWithPermissionFalseAsync()
    {
        Http.SendAsync<ApplicationCommandPermissions>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommandPermissions());

        await NewAction()
            .WithBearerToken("tok")
            .DenyUser(new Snowflake(7))
            .ExecuteAsync();

        await Http.Received(1).SendAsync<ApplicationCommandPermissions>(
            Arg.Any<DiscordRoute>(),
            HttpMethod.Put,
            Arg.Is<object?>(b => RequestHasExactlyOnePermission(b, ApplicationCommandPermissionType.User, new Snowflake(7), false)),
            Arg.Any<AuthenticationHeaderValue?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Clear_EmptiesAccumulatorBeforeSendingAsync()
    {
        Http.SendAsync<ApplicationCommandPermissions>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<AuthenticationHeaderValue?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommandPermissions());

        await NewAction()
            .WithBearerToken("tok")
            .AllowRole(new Snowflake(1))
            .AllowUser(new Snowflake(2))
            .Clear()
            .ExecuteAsync();

        await Http.Received(1).SendAsync<ApplicationCommandPermissions>(
            Arg.Any<DiscordRoute>(),
            HttpMethod.Put,
            Arg.Is<object?>(b => RequestPermissionCount(b) == 0),
            Arg.Any<AuthenticationHeaderValue?>(),
            Arg.Any<CancellationToken>());
    }

    private static int RequestPermissionCount(object? body)
    {
        var req = body as DiscoSdk.Hosting.Models.Requests.Commands.EditApplicationCommandPermissionsRequest;
        return req?.Permissions.Length ?? -1;
    }

    private static bool RequestHasExactlyOnePermission(object? body, ApplicationCommandPermissionType type, Snowflake id, bool allowed)
    {
        var req = body as DiscoSdk.Hosting.Models.Requests.Commands.EditApplicationCommandPermissionsRequest;
        if (req is null || req.Permissions.Length != 1) return false;
        var p = req.Permissions[0];
        return p.Type == type && p.Id == id && p.Permission == allowed;
    }
}
