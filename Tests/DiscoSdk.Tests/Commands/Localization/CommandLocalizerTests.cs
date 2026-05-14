using DiscoSdk.Commands;
using DiscoSdk.Commands.Localization;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DiscoSdk.Tests.Commands.Localization;

public class CommandLocalizerTests
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
            ["pt-BR"] = new CommandLocalization { Name = "ping", Description = "Responde com pong." },
            ["es-ES"] = new CommandLocalization { Name = "ping", Description = "Responde con pong." },
        });

        CommandLocalizer.Apply(command, provider);

        Assert.Equal("ping", command.NameLocalizations!["pt-BR"]);
        Assert.Equal("ping", command.NameLocalizations!["es-ES"]);
        Assert.Equal("Responde com pong.", command.DescriptionLocalizations!["pt-BR"]);
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
            ["pt-BR"] = new CommandLocalization
            {
                Name = "banir",
                Description = "Bane um usuário",
                Options =
                [
                    new() { OptionName = "user", Name = "usuario", Description = "Usuário a banir" },
                    new() { OptionName = "reason", Name = "motivo", Description = "Motivo",
                            Choices =
                            [
                                new() { ChoiceName = "spam", Name = "spam" },
                                new() { ChoiceName = "raid", Name = "ataque" },
                            ] },
                ],
            },
        });

        CommandLocalizer.Apply(command, provider);

        Assert.Equal("banir", command.NameLocalizations!["pt-BR"]);

        var userOpt = command.Options!.Single(o => o.Name == "user");
        Assert.Equal("usuario", userOpt.NameLocalizations!["pt-BR"]);
        Assert.Equal("Usuário a banir", userOpt.DescriptionLocalizations!["pt-BR"]);

        var reasonOpt = command.Options!.Single(o => o.Name == "reason");
        Assert.Equal("motivo", reasonOpt.NameLocalizations!["pt-BR"]);
        var spam = reasonOpt.Choices!.Single(c => c.Name == "spam");
        var raid = reasonOpt.Choices!.Single(c => c.Name == "raid");
        Assert.Equal("spam", spam.NameLocalizations!["pt-BR"]);
        Assert.Equal("ataque", raid.NameLocalizations!["pt-BR"]);
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
            ["pt-BR"] = new CommandLocalization
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
                                Name = "definir",
                                Options =
                                [
                                    new() { OptionName = "theme", Name = "tema", Description = "Tema da interface" },
                                ],
                            },
                        ],
                    },
                ],
            },
        });

        CommandLocalizer.Apply(command, provider);

        var ui = command.Options!.Single(o => o.Name == "ui");
        Assert.Equal("interface", ui.NameLocalizations!["pt-BR"]);

        var set = ui.Options!.Single(o => o.Name == "set");
        Assert.Equal("definir", set.NameLocalizations!["pt-BR"]);

        var theme = set.Options!.Single(o => o.Name == "theme");
        Assert.Equal("tema", theme.NameLocalizations!["pt-BR"]);
        Assert.Equal("Tema da interface", theme.DescriptionLocalizations!["pt-BR"]);
    }

    [Fact]
    public void Apply_PreservesManualLocalizationsWhenProviderReturnsSameLocale()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping")
            .WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput)
            .AddNameLocalization("pt-BR", "manual-pt")
            .Build();

        var provider = ProviderFor("ping", new Dictionary<string, CommandLocalization>
        {
            ["pt-BR"] = new CommandLocalization { Name = "provider-pt" },
            ["es-ES"] = new CommandLocalization { Name = "provider-es" },
        });

        CommandLocalizer.Apply(command, provider);

        Assert.Equal("manual-pt", command.NameLocalizations!["pt-BR"]);
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
            ["pt-BR"] = new CommandLocalization { Name = null, Description = "   " },
            ["es-ES"] = new CommandLocalization { Name = "ping", Description = null },
        });

        CommandLocalizer.Apply(command, provider);

        Assert.False(command.NameLocalizations?.ContainsKey("pt-BR") ?? false);
        Assert.False(command.DescriptionLocalizations?.ContainsKey("pt-BR") ?? false);
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

        var ex = Assert.Throws<InvalidOperationException>(() => CommandLocalizer.Apply(command, provider));
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
            ["pt-BR"] = new CommandLocalization
            {
                Options =
                [
                    new() { OptionName = "ephemeral", Name = "efemero" },
                    new() { OptionName = "doesnotexist", Name = "nope" },
                ],
            },
        });

        var logger = Substitute.For<ILogger>();

        CommandLocalizer.Apply(command, provider, logger: logger);

        Assert.Equal("efemero", command.Options!.Single(o => o.Name == "ephemeral").NameLocalizations!["pt-BR"]);

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
            ["pt-BR"] = new CommandLocalization
            {
                Options =
                [
                    new()
                    {
                        OptionName = "reason",
                        Choices =
                        [
                            new() { ChoiceName = "spam", Name = "spam" },
                            new() { ChoiceName = "ghost", Name = "fantasma" },
                        ],
                    },
                ],
            },
        });

        var logger = Substitute.For<ILogger>();
        CommandLocalizer.Apply(command, provider, logger: logger);

        var reason = command.Options!.Single(o => o.Name == "reason");
        Assert.Equal("spam", reason.Choices!.Single(c => c.Name == "spam").NameLocalizations!["pt-BR"]);

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

        CommandLocalizer.Apply(command, provider);

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

        CommandLocalizer.Apply(command, provider);

        Assert.Null(command.NameLocalizations);
        Assert.Null(command.DescriptionLocalizations);
    }

    [Fact]
    public void Apply_NullProvider_Throws()
    {
        var command = new SlashCommandBuilder()
            .WithName("ping").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        Assert.Throws<ArgumentNullException>(() => CommandLocalizer.Apply(command, null!));
    }

    [Fact]
    public void Apply_NullCommand_Throws()
    {
        var provider = Substitute.For<ICommandLocalizationProvider>();
        Assert.Throws<ArgumentNullException>(() => CommandLocalizer.Apply(null!, provider));
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

        CommandLocalizer.Apply(command, provider);

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

        CommandLocalizer.Apply(command, provider, guildId);

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
            ["pt-BR"] = new CommandLocalization { Name = "banir-A" },
        });
        provider.GetLocalizations("ban", guildB).Returns(new Dictionary<string, CommandLocalization>
        {
            ["pt-BR"] = new CommandLocalization { Name = "banir-B" },
        });

        SlashCommand MakeBan() => new SlashCommandBuilder()
            .WithName("ban").WithDescription("desc")
            .WithType(ApplicationCommandType.ChatInput).Build();

        var cmdA = MakeBan();
        var cmdB = MakeBan();

        CommandLocalizer.Apply(cmdA, provider, guildA);
        CommandLocalizer.Apply(cmdB, provider, guildB);

        Assert.Equal("banir-A", cmdA.NameLocalizations!["pt-BR"]);
        Assert.Equal("banir-B", cmdB.NameLocalizations!["pt-BR"]);
    }
}
