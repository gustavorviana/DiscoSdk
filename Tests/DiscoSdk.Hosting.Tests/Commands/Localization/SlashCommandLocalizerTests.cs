using DiscoSdk.Commands;
using DiscoSdk.Commands.Localization;
using DiscoSdk.Hosting.Commands.Localization;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DiscoSdk.Hosting.Tests.Commands.Localization;

public class SlashCommandLocalizerTests
{
    private static ICommandLocalizationProvider ProviderFor(
        string commandName,
        IReadOnlyDictionary<string, CommandLocalization>? value,
        Snowflake? guildId = null)
    {
        var provider = Substitute.For<ICommandLocalizationProvider>();
        provider.GetLocalizations(commandName, guildId).Returns(value);
        return provider;
    }

    [Fact]
    public void Apply_PopulatesCommandNameAndDescriptionForEachLocale()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping")
            .WithDescription("Replies with pong.")
            .WithType(ApplicationCommandType.ChatInput)
            .Build();

        var provider = ProviderFor("ping", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = "ping", Description = "Replies with pong (UK)." },
            ["es-ES"] = new CommandLocalization { Name = "ping", Description = "Responde con pong." },
        });

        new SlashCommandLocalizer(provider).Apply(command);

        Assert.Equal("ping", command.NameLocalizations!["en-GB"]);
        Assert.Equal("ping", command.NameLocalizations!["es-ES"]);
        Assert.Equal("Replies with pong (UK).", command.DescriptionLocalizations!["en-GB"]);
        Assert.Equal("Responde con pong.", command.DescriptionLocalizations!["es-ES"]);
    }

    [Fact]
    public void Apply_LocalizesEachOptionAndChoice()
    {
        var command = new SlashCommandBuilder()
            .WithName("ban")
            .WithDescription("Ban a user.")
            .WithType(ApplicationCommandType.ChatInput)
            .AddUserOption("user", "User to ban", required: true)
            .AddStringOption(
                name: "reason",
                description: "Reason",
                required: false,
                minLength: null, maxLength: null, autocomplete: null,
                new SlashCommandOptionChoice { Name = "spam", Value = "spam" },
                new SlashCommandOptionChoice { Name = "raid", Value = "raid" })
            .Build();

        var provider = ProviderFor("ban", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization
            {
                Name = "ban-user",
                Description = "Ban a user",
                Options =
                [
                    new() { OptionName = "user", Name = "target", Description = "User to ban" },
                    new() { OptionName = "reason", Name = "cause", Description = "Cause",
                            Choices =
                            [
                                new() { ChoiceName = "spam", Name = "Spam" },
                                new() { ChoiceName = "raid", Name = "Raid attack" },
                            ] },
                ],
            },
        });

        new SlashCommandLocalizer(provider).Apply(command);

        Assert.Equal("ban-user", command.NameLocalizations!["en-GB"]);

        var userOpt = command.Options!.Single(o => o.Name == "user");
        Assert.Equal("target", userOpt.NameLocalizations!["en-GB"]);
        Assert.Equal("User to ban", userOpt.DescriptionLocalizations!["en-GB"]);

        var reasonOpt = command.Options!.Single(o => o.Name == "reason");
        Assert.Equal("cause", reasonOpt.NameLocalizations!["en-GB"]);
        var spam = reasonOpt.Choices!.Single(c => c.Name == "spam");
        var raid = reasonOpt.Choices!.Single(c => c.Name == "raid");
        Assert.Equal("Spam", spam.NameLocalizations!["en-GB"]);
        Assert.Equal("Raid attack", raid.NameLocalizations!["en-GB"]);
    }

    [Fact]
    public void Apply_HandlesNestedSubCommandAndSubGroupOptions()
    {
        var themeOption = new SlashCommandOption
        {
            Type = SlashCommandOptionType.String,
            Name = "theme",
            Description = "UI theme",
        };
        var setSub = new SlashCommandOption
        {
            Type = SlashCommandOptionType.SubCommand,
            Name = "set",
            Description = "Set a value",
            Options = [themeOption],
        };
        var command = new SlashCommandBuilder()
            .WithName("config")
            .WithDescription("Server config")
            .WithType(ApplicationCommandType.ChatInput)
            .AddSubCommandGroupOption("ui", "UI options", setSub)
            .Build();

        var provider = ProviderFor("config", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization
            {
                Options =
                [
                    new()
                    {
                        OptionName = "ui",
                        Name = "interface",
                        Options =
                        [
                            new()
                            {
                                OptionName = "set",
                                Name = "apply",
                                Options =
                                [
                                    new() { OptionName = "theme", Name = "skin", Description = "Interface theme" },
                                ],
                            },
                        ],
                    },
                ],
            },
        });

        new SlashCommandLocalizer(provider).Apply(command);

        var ui = command.Options!.Single(o => o.Name == "ui");
        Assert.Equal("interface", ui.NameLocalizations!["en-GB"]);

        var set = ui.Options!.Single(o => o.Name == "set");
        Assert.Equal("apply", set.NameLocalizations!["en-GB"]);

        var theme = set.Options!.Single(o => o.Name == "theme");
        Assert.Equal("skin", theme.NameLocalizations!["en-GB"]);
        Assert.Equal("Interface theme", theme.DescriptionLocalizations!["en-GB"]);
    }

    [Fact]
    public void Apply_PreservesManualLocalizationsWhenProviderReturnsSameLocale()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping")
            .WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput)
            .AddNameLocalization("en-GB", "manual-uk")
            .Build();

        var provider = ProviderFor("ping", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = "provider-uk" },
            ["es-ES"] = new CommandLocalization { Name = "provider-es" },
        });

        new SlashCommandLocalizer(provider).Apply(command);

        Assert.Equal("manual-uk", command.NameLocalizations!["en-GB"]);
        Assert.Equal("provider-es", command.NameLocalizations!["es-ES"]);
    }

    [Fact]
    public void Apply_SkipsNullAndWhitespaceValuesFromProvider()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping")
            .WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput)
            .Build();

        var provider = ProviderFor("ping", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = null, Description = "   " },
            ["es-ES"] = new CommandLocalization { Name = "ping", Description = null },
        });

        new SlashCommandLocalizer(provider).Apply(command);

        Assert.False(command.NameLocalizations?.ContainsKey("en-GB") ?? false);
        Assert.False(command.DescriptionLocalizations?.ContainsKey("en-GB") ?? false);
        Assert.Equal("ping", command.NameLocalizations!["es-ES"]);
        Assert.False(command.DescriptionLocalizations?.ContainsKey("es-ES") ?? false);
    }

    [Fact]
    public void Apply_ThrowsWhenProviderReturnsUnknownLocale()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        var provider = ProviderFor("ping", new Dictionary<string, CommandLocalization>
        {
            ["xx-YY"] = new CommandLocalization { Name = "noop" },
        });

        var ex = Assert.Throws<InvalidOperationException>(() => new SlashCommandLocalizer(provider).Apply(command));
        Assert.Contains("xx-YY", ex.Message);
        Assert.Contains("ping", ex.Message);
    }

    [Fact]
    public void Apply_UnknownOption_LogsWarningAndContinues()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput)
            .AddBooleanOption("ephemeral", "Ephemeral reply")
            .Build();

        var provider = ProviderFor("ping", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization
            {
                Options =
                [
                    new() { OptionName = "ephemeral", Name = "transient" },
                    new() { OptionName = "doesnotexist", Name = "nope" },
                ],
            },
        });

        var logger = Substitute.For<ILogger>();

        new SlashCommandLocalizer(provider, logger).Apply(command);

        Assert.Equal("transient", command.Options!.Single(o => o.Name == "ephemeral").NameLocalizations!["en-GB"]);

        logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("doesnotexist") && o.ToString()!.Contains("ping")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void Apply_UnknownChoice_LogsWarningAndContinues()
    {
        var command = new SlashCommandBuilder()
            .WithName("ban").WithDescription("Ban")
            .WithType(ApplicationCommandType.ChatInput)
            .AddStringOption(
                "reason", "Reason", required: false,
                minLength: null, maxLength: null, autocomplete: null,
                new SlashCommandOptionChoice { Name = "spam", Value = "spam" })
            .Build();

        var provider = ProviderFor("ban", new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization
            {
                Options =
                [
                    new()
                    {
                        OptionName = "reason",
                        Choices =
                        [
                            new() { ChoiceName = "spam", Name = "Spam" },
                            new() { ChoiceName = "ghost", Name = "Ghost" },
                        ],
                    },
                ],
            },
        });

        var logger = Substitute.For<ILogger>();
        new SlashCommandLocalizer(provider, logger).Apply(command);

        var reason = command.Options!.Single(o => o.Name == "reason");
        Assert.Equal("Spam", reason.Choices!.Single(c => c.Name == "spam").NameLocalizations!["en-GB"]);

        logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("ghost") && o.ToString()!.Contains("ban")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void Apply_ProviderReturnsNull_NoOp()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        var provider = ProviderFor("ping", null);

        new SlashCommandLocalizer(provider).Apply(command);

        Assert.Null(command.NameLocalizations);
        Assert.Null(command.DescriptionLocalizations);
    }

    [Fact]
    public void Apply_ProviderReturnsEmptyDict_NoOp()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        var provider = ProviderFor("ping", new Dictionary<string, CommandLocalization>());

        new SlashCommandLocalizer(provider).Apply(command);

        Assert.Null(command.NameLocalizations);
        Assert.Null(command.DescriptionLocalizations);
    }

    [Fact]
    public void Constructor_NullProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SlashCommandLocalizer(null!));
    }

    [Fact]
    public void Apply_NullCommand_Throws()
    {
        var provider = Substitute.For<ICommandLocalizationProvider>();
        var localizer = new SlashCommandLocalizer(provider);
        Assert.Throws<ArgumentNullException>(() => localizer.Apply(null!));
    }

    [Fact]
    public void Apply_GlobalCommand_PassesNullGuildIdToProvider()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        var provider = Substitute.For<ICommandLocalizationProvider>();
        provider
            .GetLocalizations(Arg.Any<string>(), Arg.Any<Snowflake?>())
            .Returns((IReadOnlyDictionary<string, CommandLocalization>?)null);

        new SlashCommandLocalizer(provider).Apply(command);

        provider.Received(1).GetLocalizations("ping", null);
    }

    [Fact]
    public void Apply_GuildCommand_PassesGuildIdToProvider()
    {
        var guildId = Snowflake.Parse("773618860875579422");
        var command = new SlashCommandBuilder()
            .WithName("ban").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        var provider = Substitute.For<ICommandLocalizationProvider>();
        provider
            .GetLocalizations(Arg.Any<string>(), Arg.Any<Snowflake?>())
            .Returns((IReadOnlyDictionary<string, CommandLocalization>?)null);

        new SlashCommandLocalizer(provider).Apply(command, guildId);

        provider.Received(1).GetLocalizations("ban", guildId);
    }

    [Fact]
    public void Apply_DifferentGuilds_GetDifferentTranslations()
    {
        var guildA = Snowflake.Parse("111111111111111111");
        var guildB = Snowflake.Parse("222222222222222222");

        var provider = Substitute.For<ICommandLocalizationProvider>();
        provider.GetLocalizations("ban", guildA).Returns(new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = "ban-A" },
        });
        provider.GetLocalizations("ban", guildB).Returns(new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = "ban-B" },
        });

        ApplicationCommand MakeBan() => new SlashCommandBuilder()
            .WithName("ban").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        var cmdA = MakeBan();
        var cmdB = MakeBan();

        var localizer = new SlashCommandLocalizer(provider);
        localizer.Apply(cmdA, guildA);
        localizer.Apply(cmdB, guildB);

        Assert.Equal("ban-A", cmdA.NameLocalizations!["en-GB"]);
        Assert.Equal("ban-B", cmdB.NameLocalizations!["en-GB"]);
    }
}
