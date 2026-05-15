using DiscoSdk.Commands;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Rest.Actions;

public class CommandUpdateScopeAddFromCatalogTests
{
    private readonly IDiscordRestClient _rest = Substitute.For<IDiscordRestClient>();
    private readonly Snowflake _appId = new(1234567890ul);
    private readonly Snowflake _guildId = new(9999999999ul);

    private CommandUpdateScope GuildScope(ICommandRegistry? registry, bool overwrite) =>
        new(new ApplicationCommandClient(_rest), _appId, _guildId, overwrite, null, null, registry, null);

    private CommandUpdateScope GlobalScope(ICommandRegistry? registry, bool overwrite) =>
        new(new ApplicationCommandClient(_rest), _appId, guildId: null, overwrite, null, null, registry, null);

    private static ApplicationCommand Slash(string name) => new()
    {
        Name = name,
        Description = name,
        Type = ApplicationCommandType.ChatInput,
    };

    private static ICommandRegistry RegistryWithOnDemand(string name, ApplicationCommandType type, ApplicationCommand command)
    {
        var registry = Substitute.For<ICommandRegistry>();
        registry.IsOnDemand(name, type).Returns(true);
        registry.TryGet(name, type, out Arg.Any<ApplicationCommand>())
            .Returns(call =>
            {
                call[2] = command;
                return true;
            });
        registry.Get(name, type).Returns(command);
        return registry;
    }

    private static ICommandRegistry RegistryWithPlain(string name, ApplicationCommandType type, ApplicationCommand command)
    {
        var registry = Substitute.For<ICommandRegistry>();
        registry.IsOnDemand(name, type).Returns(false);
        registry.TryGet(name, type, out Arg.Any<ApplicationCommand>())
            .Returns(call =>
            {
                call[2] = command;
                return true;
            });
        registry.Get(name, type).Returns(command);
        return registry;
    }

    private static ICommandRegistry RegistryWithNothing()
    {
        var registry = Substitute.For<ICommandRegistry>();
        registry.IsOnDemand(Arg.Any<string>(), Arg.Any<ApplicationCommandType>()).Returns(false);
        registry.TryGet(Arg.Any<string>(), Arg.Any<ApplicationCommandType>(), out Arg.Any<ApplicationCommand>())
            .Returns(false);
        return registry;
    }

    [Fact]
    public async Task AddFromCatalog_OnDemandCommand_QueuesAndAppliesItAsync()
    {
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var registry = RegistryWithOnDemand("premium", ApplicationCommandType.ChatInput, Slash("premium"));

        var scope = GuildScope(registry, overwrite: true);
        scope.AddFromCatalog("premium", ApplicationCommandType.ChatInput);

        await scope.ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/guilds/{_guildId}/commands"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void AddFromCatalog_NonOnDemandCommandInRegistry_ThrowsInvalidOperation()
    {
        var registry = RegistryWithPlain("regular", ApplicationCommandType.ChatInput, Slash("regular"));

        var scope = GuildScope(registry, overwrite: false);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            scope.AddFromCatalog("regular", ApplicationCommandType.ChatInput));

        Assert.Contains("[OnDemand]", ex.Message);
        Assert.Contains("regular", ex.Message);
    }

    [Fact]
    public void AddFromCatalog_NameNotInRegistry_ThrowsKeyNotFound()
    {
        var registry = RegistryWithNothing();

        var scope = GuildScope(registry, overwrite: false);

        Assert.Throws<KeyNotFoundException>(() =>
            scope.AddFromCatalog("nope", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public void AddFromCatalog_WrongTypeForExistingName_ThrowsKeyNotFound()
    {
        // Same name as slash on-demand but lookup is for User context — must not resolve cross-type.
        var registry = Substitute.For<ICommandRegistry>();
        registry.IsOnDemand("ping", ApplicationCommandType.ChatInput).Returns(true);
        registry.IsOnDemand("ping", ApplicationCommandType.User).Returns(false);
        registry.TryGet("ping", ApplicationCommandType.User, out Arg.Any<ApplicationCommand>())
            .Returns(false);

        var scope = GuildScope(registry, overwrite: false);

        Assert.Throws<KeyNotFoundException>(() =>
            scope.AddFromCatalog("ping", ApplicationCommandType.User));
    }

    [Fact]
    public void AddFromCatalog_NullRegistry_ThrowsInvalidOperation()
    {
        var scope = GuildScope(registry: null, overwrite: false);

        Assert.Throws<InvalidOperationException>(() =>
            scope.AddFromCatalog("anything", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public void AddFromCatalog_BlankName_ThrowsArgumentException()
    {
        var registry = RegistryWithNothing();
        var scope = GuildScope(registry, overwrite: false);

        Assert.Throws<ArgumentException>(() =>
            scope.AddFromCatalog("   ", ApplicationCommandType.ChatInput));
    }

    [Fact]
    public async Task AddFromCatalog_GlobalScope_AlsoWorksWhenCommandIsOnDemandAsync()
    {
        // OnDemand só suprime *auto*-registration. Quem pega manualmente pode mandar pro global.
        _rest.SendAsync<List<ApplicationCommand>>(Arg.Any<DiscordRoute>(), Arg.Any<HttpMethod>(), Arg.Any<object?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ApplicationCommand>());

        var registry = RegistryWithOnDemand("premium", ApplicationCommandType.ChatInput, Slash("premium"));

        var scope = GlobalScope(registry, overwrite: true);
        scope.AddFromCatalog("premium", ApplicationCommandType.ChatInput);

        await scope.ApplyAsync();

        await _rest.Received(1).SendAsync<List<ApplicationCommand>>(
            Arg.Is<DiscordRoute>(r => r.ToString() == $"applications/{_appId}/commands"),
            HttpMethod.Put,
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void AddFromCatalog_ReturnsScopeForFluentChaining()
    {
        var registry = Substitute.For<ICommandRegistry>();
        registry.IsOnDemand("a", ApplicationCommandType.ChatInput).Returns(true);
        registry.IsOnDemand("b", ApplicationCommandType.ChatInput).Returns(true);
        registry.Get("a", ApplicationCommandType.ChatInput).Returns(Slash("a"));
        registry.Get("b", ApplicationCommandType.ChatInput).Returns(Slash("b"));

        var scope = GuildScope(registry, overwrite: true);
        var result = scope
            .AddFromCatalog("a", ApplicationCommandType.ChatInput)
            .AddFromCatalog("b", ApplicationCommandType.ChatInput);

        Assert.Same(scope, result);
    }
}
