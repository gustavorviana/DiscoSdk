using DiscoSdk.Hosting;
using DiscoSdk.Hosting.Builders;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using NSubstitute;
using System.Collections.Generic;
using System.Text.Json;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

/// <summary>
/// Verifies that <see cref="ModifyRolePositionsAction"/> serializes Move calls into the
/// Discord <c>PATCH /guilds/{guild.id}/roles</c> body shape (array of <c>{id, position?, lock_permissions?}</c>),
/// propagates the audit-log reason header, and refuses to fire without at least one Move.
/// </summary>
public class ModifyRolePositionsActionTests
{
    private readonly IDiscordRestClient _http = Substitute.For<IDiscordRestClient>();
    private readonly Snowflake _guildId = new(111);

    private DiscordClient NewClient()
    {
        _http.JsonOptions.Returns(new JsonSerializerOptions());
        return DiscordClientBuilder.Create("test-token")
            .WithIntents(DiscoSdk.DiscordIntent.Guilds)
            .WithRestClient(_http)
            .Build();
    }

    private IGuild StubGuild()
    {
        var g = Substitute.For<IGuild>();
        g.Id.Returns(_guildId);
        return g;
    }

    [Fact]
    public async Task ExecuteAsync_NoMoves_ThrowsAsync()
    {
        var action = new ModifyRolePositionsAction(NewClient(), StubGuild());
        await Assert.ThrowsAsync<InvalidOperationException>(() => action.ExecuteAsync());
    }

    private static bool TwoMovesPayload(object? body)
    {
        if (body is not IEnumerable<Dictionary<string, object?>> list)
            return false;
        var items = list.ToList();
        if (items.Count != 2) return false;
        var ten = items.SingleOrDefault(d => (string)d["id"]! == "10");
        var twenty = items.SingleOrDefault(d => (string)d["id"]! == "20");
        return ten != null
            && twenty != null
            && (int)ten["position"]! == 3
            && !ten.ContainsKey("lock_permissions")
            && !twenty.ContainsKey("position")
            && (bool)twenty["lock_permissions"]! == true;
    }

    private static bool SingleMovePosition9(object? body)
    {
        if (body is not IEnumerable<Dictionary<string, object?>> list) return false;
        var items = list.ToList();
        return items.Count == 1 && (int)items[0]["position"]! == 9;
    }

    [Fact]
    public async Task ExecuteAsync_SerializesMovesAndHitsRolesEndpointAsync()
    {
        _http.SendAsync<Role[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var action = new ModifyRolePositionsAction(NewClient(), StubGuild());
        action
            .Move(new Snowflake(10), position: 3)
            .Move(new Snowflake(20), position: null, lockPermissions: true);

        await action.ExecuteAsync();

        await _http.Received(1).SendAsync<Role[]>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/roles"),
            HttpMethod.Patch,
            Arg.Is<object?>(body => TwoMovesPayload(body)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithReason_RoutesThroughReasonedAsync()
    {
        _http.SendWithReasonAsync<Role[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var action = new ModifyRolePositionsAction(NewClient(), StubGuild())
            .Move(new Snowflake(10), 1)
            .WithReason("Server restructure");

        await action.ExecuteAsync();

        await _http.Received(1).SendWithReasonAsync<Role[]>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"guilds/{_guildId}/roles"),
            HttpMethod.Patch,
            Arg.Any<object?>(),
            "Server restructure",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Move_DefaultRoleId_Throws()
    {
        var action = new ModifyRolePositionsAction(NewClient(), StubGuild());
        Assert.Throws<ArgumentException>(() => action.Move(default, 1));
    }

    [Fact]
    public async Task Move_SameRoleTwice_KeepsLastValueAsync()
    {
        _http.SendAsync<Role[]>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var action = new ModifyRolePositionsAction(NewClient(), StubGuild());
        action.Move(new Snowflake(10), 5).Move(new Snowflake(10), 9);

        await action.ExecuteAsync();

        await _http.Received(1).SendAsync<Role[]>(
            Arg.Any<DiscordRoute>(),
            HttpMethod.Patch,
            Arg.Is<object?>(body => SingleMovePosition9(body)),
            Arg.Any<CancellationToken>());
    }
}
