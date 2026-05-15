using DiscoSdk.Commands;
using DiscoSdk.Commands.Localization;
using DiscoSdk.Hosting.Commands.Localization;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands.Localization;

public class ContextCommandLocalizerTests
{
    private static IContextCommandLocalizationProvider ProviderFor(
        string commandName,
        IReadOnlyDictionary<string, CommandLocalization>? value,
        Snowflake? guildId = null)
    {
        var provider = Substitute.For<IContextCommandLocalizationProvider>();
        provider.GetLocalizations(commandName, guildId).Returns(value);
        return provider;
    }

    private static ApplicationCommand BuildUserCommand(string name) =>
        new ContextMenuBuilder().WithName(name).Build(ContextMenuType.User);

    private static ApplicationCommand BuildMessageCommand(string name) =>
        new ContextMenuBuilder().WithName(name).Build(ContextMenuType.Message);

    [Fact]
    public void Apply_UserCommand_PopulatesNameLocalizationsAcrossLocales()
    {
        var command = BuildUserCommand("BanUser");

        var provider = ProviderFor("BanUser", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = "Block" },
            ["es-ES"] = new CommandLocalization { Name = "Bloquear" },
        });

        new ContextCommandLocalizer(provider).Apply(command);

        Assert.Equal("Block", command.NameLocalizations!["en-GB"]);
        Assert.Equal("Bloquear", command.NameLocalizations!["es-ES"]);
    }

    [Fact]
    public void Apply_MessageCommand_PopulatesNameLocalizations()
    {
        var command = BuildMessageCommand("ReportMessage");

        var provider = ProviderFor("ReportMessage", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = "Report" },
        });

        new ContextCommandLocalizer(provider).Apply(command);

        Assert.Equal("Report", command.NameLocalizations!["en-GB"]);
    }

    [Fact]
    public void Apply_PreservesManualLocalizationsWhenProviderReturnsSameLocale()
    {
        var command = new ContextMenuBuilder()
            .WithName("BanUser")
            .WithNameLocalizations(new Dictionary<string, string> { ["en-GB"] = "ManualBlock" })
            .Build(ContextMenuType.User);

        var provider = ProviderFor("BanUser", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = "ProviderBlock" },
            ["es-ES"] = new CommandLocalization { Name = "ProviderBloquear" },
        });

        new ContextCommandLocalizer(provider).Apply(command);

        Assert.Equal("ManualBlock", command.NameLocalizations!["en-GB"]);
        Assert.Equal("ProviderBloquear", command.NameLocalizations!["es-ES"]);
    }

    [Fact]
    public void Apply_DescriptionAndOptionsInTree_AreIgnoredAndLogged()
    {
        var command = BuildUserCommand("BanUser");

        var provider = ProviderFor("BanUser", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization
            {
                Name = "Block",
                Description = "should be ignored",
                Options = [new OptionLocalization { OptionName = "x" }],
            },
        });

        var logger = Substitute.For<ILogger>();
        new ContextCommandLocalizer(provider, logger).Apply(command);

        Assert.Equal("Block", command.NameLocalizations!["en-GB"]);
        // Context commands have no DescriptionLocalizations field semantically; we just
        // verify the warning was emitted.
        logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("BanUser")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void Apply_SkipsNullAndWhitespaceNames()
    {
        var command = BuildUserCommand("BanUser");

        var provider = ProviderFor("BanUser", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = null },
            ["es-ES"] = new CommandLocalization { Name = "   " },
            ["fr"] = new CommandLocalization { Name = "Bloquer" },
        });

        new ContextCommandLocalizer(provider).Apply(command);

        Assert.False(command.NameLocalizations?.ContainsKey("en-GB") ?? false);
        Assert.False(command.NameLocalizations?.ContainsKey("es-ES") ?? false);
        Assert.Equal("Bloquer", command.NameLocalizations!["fr"]);
    }

    [Fact]
    public void Apply_ThrowsOnUnknownLocale()
    {
        var command = BuildUserCommand("BanUser");

        var provider = ProviderFor("BanUser", new Dictionary<string, CommandLocalization>
        {
            ["xx-YY"] = new CommandLocalization { Name = "noop" },
        });

        var ex = Assert.Throws<InvalidOperationException>(() => new ContextCommandLocalizer(provider).Apply(command));
        Assert.Contains("xx-YY", ex.Message);
        Assert.Contains("BanUser", ex.Message);
    }

    [Fact]
    public void Apply_ProviderReturnsNull_NoOp()
    {
        var command = BuildUserCommand("BanUser");
        var provider = ProviderFor("BanUser", null);

        new ContextCommandLocalizer(provider).Apply(command);

        Assert.Null(command.NameLocalizations);
    }

    [Fact]
    public void Apply_ProviderReturnsEmpty_NoOp()
    {
        var command = BuildUserCommand("BanUser");
        var provider = ProviderFor("BanUser", new Dictionary<string, CommandLocalization>());

        new ContextCommandLocalizer(provider).Apply(command);

        Assert.Null(command.NameLocalizations);
    }

    [Fact]
    public void Constructor_NullProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ContextCommandLocalizer(null!));
    }

    [Fact]
    public void Apply_NullCommand_Throws()
    {
        var provider = Substitute.For<IContextCommandLocalizationProvider>();
        var localizer = new ContextCommandLocalizer(provider);
        Assert.Throws<ArgumentNullException>(() => localizer.Apply(null!));
    }

    [Fact]
    public void Apply_GuildCommand_PassesGuildIdToProvider()
    {
        var guildId = Snowflake.Parse("773618860875579422");
        var command = BuildUserCommand("BanUser");

        var provider = Substitute.For<IContextCommandLocalizationProvider>();
        provider
            .GetLocalizations(Arg.Any<string>(), Arg.Any<Snowflake?>())
            .Returns((IReadOnlyDictionary<string, CommandLocalization>?)null);

        new ContextCommandLocalizer(provider).Apply(command, guildId);

        provider.Received(1).GetLocalizations("BanUser", guildId);
    }

    [Fact]
    public void Apply_GlobalCommand_PassesNullGuildIdToProvider()
    {
        var command = BuildUserCommand("BanUser");
        var provider = Substitute.For<IContextCommandLocalizationProvider>();
        provider.GetLocalizations(Arg.Any<string>(), Arg.Any<Snowflake?>())
            .Returns((IReadOnlyDictionary<string, CommandLocalization>?)null);

        new ContextCommandLocalizer(provider).Apply(command);

        provider.Received(1).GetLocalizations("BanUser", null);
    }
}
