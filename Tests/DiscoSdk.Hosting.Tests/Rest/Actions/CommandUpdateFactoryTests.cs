using DiscoSdk;
using DiscoSdk.Commands;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class CommandUpdateFactoryTests
{
    private readonly IDiscordRestClient _rest = Substitute.For<IDiscordRestClient>();
    private readonly Snowflake _appId = new(1234567890ul);
    private readonly IDiscordClient _discord;

    public CommandUpdateFactoryTests()
    {
        _discord = Substitute.For<IDiscordClient>();
        _discord.ApplicationId.Returns(_appId);
    }

    [Fact]
    public async Task OpenForGlobal_ReturnsScopeThatTargetsGlobalRoute()
    {
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var factory = new CommandUpdateFactory(_discord, _rest);
        await factory.OpenForGlobal(overwrite: true)
            .AddSlash(b => b.WithName("ping").WithDescription("Ping"))
            .ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OpenForGuild_ReturnsScopeThatTargetsGuildRoute()
    {
        var guildId = new Snowflake(5555);
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var factory = new CommandUpdateFactory(_discord, _rest);
        await factory.OpenForGuild(guildId, overwrite: true)
            .AddSlash(b => b.WithName("admin").WithDescription("Admin"))
            .ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{guildId}/commands"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MultipleScopes_AreIndependent()
    {
        var guildId = new Snowflake(5555);
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var factory = new CommandUpdateFactory(_discord, _rest);

        await factory.OpenForGlobal(overwrite: true)
            .AddSlash(b => b.WithName("ping").WithDescription("a"))
            .ApplyAsync();
        await factory.OpenForGlobal(overwrite: true)
            .AddSlash(b => b.WithName("ping").WithDescription("b"))
            .ApplyAsync();
        await factory.OpenForGuild(guildId, overwrite: true)
            .AddSlash(b => b.WithName("admin").WithDescription("c"))
            .ApplyAsync();

        await _rest.Received(2).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
            HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{guildId}/commands"),
            HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Factory_ImplementsBothInterfaces()
    {
        var factory = new CommandUpdateFactory(_discord, _rest);
        Assert.IsAssignableFrom<ICommandUpdateFactory>(factory);
        Assert.IsAssignableFrom<IGuildCommandUpdateFactory>(factory);
    }

    [Fact]
    public async Task GuildCommandUpdateFactory_InterfaceAlone_OpensGuildScopes()
    {
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        IGuildCommandUpdateFactory guildOnly = new CommandUpdateFactory(_discord, _rest);
        var guildId = new Snowflake(7777);

        await guildOnly.OpenForGuild(guildId, overwrite: true)
            .AddSlash(b => b.WithName("welcome").WithDescription("Welcome"))
            .ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{guildId}/commands"),
            HttpMethod.Put, Arg.Any<object?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_NullDiscord_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandUpdateFactory(null!, _rest));
    }

    [Fact]
    public void Constructor_NullRest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandUpdateFactory(_discord, null!));
    }

    [Fact]
    public void OpenForGlobal_BeforeReady_ThrowsBecauseApplicationIdIsNull()
    {
        // Simulates the pre-READY state — IDiscordClient is registered but ApplicationId hasn't
        // been populated yet. Opening a scope must surface this clearly instead of NRE-ing later.
        var preReady = Substitute.For<IDiscordClient>();
        preReady.ApplicationId.Returns((Snowflake?)null);
        var factory = new CommandUpdateFactory(preReady, _rest);

        Assert.Throws<InvalidOperationException>(() => factory.OpenForGlobal(overwrite: true));
    }

    [Fact]
    public void OpenForGuild_BeforeReady_ThrowsBecauseApplicationIdIsNull()
    {
        var preReady = Substitute.For<IDiscordClient>();
        preReady.ApplicationId.Returns((Snowflake?)null);
        var factory = new CommandUpdateFactory(preReady, _rest);

        Assert.Throws<InvalidOperationException>(() => factory.OpenForGuild(new Snowflake(1), overwrite: true));
    }
}
