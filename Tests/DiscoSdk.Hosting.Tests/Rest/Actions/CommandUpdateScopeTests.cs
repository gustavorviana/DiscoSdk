using DiscoSdk.Commands;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class CommandUpdateScopeTests
{
    private readonly IDiscordRestClient _rest = Substitute.For<IDiscordRestClient>();
    private readonly Snowflake _appId = new(1234567890ul);
    private readonly Snowflake _guildId = new(9999999999ul);

    private CommandUpdateScope GlobalScope(bool overwrite) =>
        new(new ApplicationCommandClient(_rest), _appId, guildId: null, overwrite, null, null, null, null);

    private CommandUpdateScope GuildScope(bool overwrite) =>
        new(new ApplicationCommandClient(_rest), _appId, _guildId, overwrite, null, null, null, null);

    // ── Overwrite mode ──

    [Fact]
    public async Task ApplyAsync_OverwriteGlobal_PutsBulkToGlobalRoute()
    {
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var scope = GlobalScope(overwrite: true);
        scope.AddSlash(b => b.WithName("ping").WithDescription("Ping"));
        scope.AddSlash(b => b.WithName("ban").WithDescription("Ban"));

        await scope.ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_OverwriteGuild_PutsBulkToGuildRoute()
    {
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var scope = GuildScope(overwrite: true);
        scope.AddSlash(b => b.WithName("admin").WithDescription("Admin"));

        await scope.ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_OverwriteEmpty_StillSendsPutToWipe()
    {
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        await GlobalScope(overwrite: true).ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Any<DiscordRoute>(), HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    // ── Append mode ──

    [Fact]
    public async Task ApplyAsync_AppendGlobal_GetsExistingThenPostsNewCommands()
    {
        // Existing on Discord: just "ping" (with the exact same body the builder produces).
        // We add "ping" (no-op) + "ban" (POST).
        var pingId = new Snowflake(100);
        var existing = new List<ApplicationCommand>
        {
            // Options=[] (not null) to match what SlashCommandBuilder.Build emits.
            new() { Id = pingId, Type = ApplicationCommandType.ChatInput, Name = "ping", Description = "Ping", Options = Array.Empty<SlashCommandOption>() },
        };
        _rest.SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
            HttpMethod.Get,
            Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(existing);
        _rest.SendAsync<ApplicationCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommand());

        var scope = GlobalScope(overwrite: false);
        scope.AddSlash(b => b.WithType(ApplicationCommandType.ChatInput).WithName("ping").WithDescription("Ping")); // matches → no-op
        scope.AddSlash(b => b.WithType(ApplicationCommandType.ChatInput).WithName("ban").WithDescription("Ban"));   // new → POST

        await scope.ApplyAsync();

        // 1× GET (read existing) + 1× POST (only the new one)
        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Any<DiscordRoute>(), HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>());
        await _rest.Received(1).SendAsync<ApplicationCommand>(
            Arg.Any<DiscordRoute>(), HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
        await _rest.DidNotReceive().SendAsync<ApplicationCommand>(
            Arg.Any<DiscordRoute>(), HttpMethod.Patch, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_AppendGlobal_PatchesCommandThatChanged()
    {
        var pingId = new Snowflake(100);
        var existing = new List<ApplicationCommand>
        {
            new() { Id = pingId, Type = ApplicationCommandType.ChatInput, Name = "ping", Description = "old description" },
        };
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(existing);
        _rest.SendAsync<ApplicationCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommand());

        var scope = GlobalScope(overwrite: false);
        scope.AddSlash(b => b.WithType(ApplicationCommandType.ChatInput).WithName("ping").WithDescription("new description"));

        await scope.ApplyAsync();

        await _rest.Received(1).SendAsync<ApplicationCommand>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands/{pingId}"),
            HttpMethod.Patch,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
        await _rest.DidNotReceive().SendAsync<ApplicationCommand>(
            Arg.Any<DiscordRoute>(), HttpMethod.Post, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_AppendNeverDeletes()
    {
        // Existing: "ping" + "old-cmd". We queue only "new-cmd". Append must NOT delete old-cmd.
        var existing = new List<ApplicationCommand>
        {
            new() { Id = new Snowflake(1), Name = "ping", Description = "Ping" },
            new() { Id = new Snowflake(2), Name = "old-cmd", Description = "Old" },
        };
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), HttpMethod.Get, Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(existing);
        _rest.SendAsync<ApplicationCommand>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new ApplicationCommand());

        var scope = GlobalScope(overwrite: false);
        scope.AddSlash(b => b.WithName("new-cmd").WithDescription("New"));

        await scope.ApplyAsync();

        // Only the new POST — no DELETE on the legacy commands.
        await _rest.DidNotReceive().SendAsync(Arg.Any<DiscordRoute>(), HttpMethod.Delete, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyAsync_AppendEmpty_NoHttpCalls()
    {
        var scope = GlobalScope(overwrite: false); // empty + append

        await scope.ApplyAsync();

        await _rest.DidNotReceive().SendAsync<List<ApplicationCommand>>(
            Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    // ── Validation ──

    [Fact]
    public void Add_DuplicateCommandName_Throws()
    {
        var scope = GlobalScope(overwrite: true);
        scope.AddSlash(b => b.WithName("ping").WithDescription("Ping"));

        Assert.Throws<InvalidOperationException>(() =>
            scope.AddSlash(b => b.WithName("ping").WithDescription("Another ping")));
    }

    [Fact]
    public void Add_ExceedingMaxUserContextCommands_Throws()
    {
        var scope = GlobalScope(overwrite: true);
        for (var i = 0; i < 15; i++)
        {
            var name = $"user_cmd_{i}";
            scope.AddContextMenu(ContextMenuType.User, b => b.WithName(name));
        }

        Assert.Throws<InvalidOperationException>(() =>
            scope.AddContextMenu(ContextMenuType.User, b => b.WithName("user_cmd_overflow")));
    }

    [Fact]
    public void Add_ExceedingMaxMessageContextCommands_Throws()
    {
        var scope = GlobalScope(overwrite: true);
        for (var i = 0; i < 15; i++)
        {
            var name = $"msg_cmd_{i}";
            scope.AddContextMenu(ContextMenuType.Message, b => b.WithName(name));
        }

        Assert.Throws<InvalidOperationException>(() =>
            scope.AddContextMenu(ContextMenuType.Message, b => b.WithName("msg_cmd_overflow")));
    }

    [Fact]
    public void Add_NullCommand_Throws()
    {
        var scope = GlobalScope(overwrite: true);
        Assert.Throws<ArgumentNullException>(() => scope.Add((ApplicationCommand)null!));
    }

    [Fact]
    public void Add_NullConfigure_Throws()
    {
        var scope = GlobalScope(overwrite: true);
        Assert.Throws<ArgumentNullException>(() => scope.AddSlash((Func<SlashCommandBuilder, SlashCommandBuilder>)null!));
    }

    [Fact]
    public async Task ApplyAsync_ReturnsBuilderForFluentChain()
    {
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var scope = GlobalScope(overwrite: true);
        // Chained Add calls should all return the same scope for fluent usage.
        await scope
            .AddSlash(b => b.WithName("a").WithDescription("a"))
            .AddSlash(b => b.WithName("b").WithDescription("b"))
            .AddSlash(b => b.WithName("c").WithDescription("c"))
            .ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Any<DiscordRoute>(), HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }
}
