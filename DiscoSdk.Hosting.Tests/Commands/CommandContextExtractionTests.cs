using DiscoSdk.Hosting.Contexts;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Tests.Commands;

public class CommandContextExtractionTests
{
    [Fact]
    public void ExtractSubcommandInfo_FlatCommand_ReturnsNullSubcommandAndGroup()
    {
        // Arrange — flat command: /ping query:"hello"
        var options = new[]
        {
            new InteractionOption { Name = "query", Type = SlashCommandOptionType.String, Value = "hello" }
        };

        // Act
        CommandContext.ExtractSubcommandInfo(options, out var group, out var subcommand, out var leafOptions);

        // Assert
        Assert.Null(group);
        Assert.Null(subcommand);
        Assert.Same(options, leafOptions);
    }

    [Fact]
    public void ExtractSubcommandInfo_NullOptions_ReturnsNulls()
    {
        CommandContext.ExtractSubcommandInfo(null, out var group, out var subcommand, out var leafOptions);

        Assert.Null(group);
        Assert.Null(subcommand);
        Assert.Null(leafOptions);
    }

    [Fact]
    public void ExtractSubcommandInfo_EmptyOptions_ReturnsNulls()
    {
        var options = Array.Empty<InteractionOption>();

        CommandContext.ExtractSubcommandInfo(options, out var group, out var subcommand, out var leafOptions);

        Assert.Null(group);
        Assert.Null(subcommand);
        Assert.Same(options, leafOptions);
    }

    [Fact]
    public void ExtractSubcommandInfo_OneLevelSubcommand_ExtractsSubcommandAndLeafOptions()
    {
        // Arrange — /settings reset confirm:true
        var leafOption = new InteractionOption { Name = "confirm", Type = SlashCommandOptionType.Boolean, Value = true };
        var options = new[]
        {
            new InteractionOption
            {
                Name = "reset",
                Type = SlashCommandOptionType.SubCommand,
                Options = [leafOption]
            }
        };

        // Act
        CommandContext.ExtractSubcommandInfo(options, out var group, out var subcommand, out var leafOptions);

        // Assert
        Assert.Null(group);
        Assert.Equal("reset", subcommand);
        Assert.NotNull(leafOptions);
        Assert.Single(leafOptions);
        Assert.Equal("confirm", leafOptions![0].Name);
    }

    [Fact]
    public void ExtractSubcommandInfo_TwoLevelSubcommand_ExtractsGroupSubcommandAndLeafOptions()
    {
        // Arrange — /config notifications enable channel:#general
        var leafOption = new InteractionOption { Name = "channel", Type = SlashCommandOptionType.Channel, Value = "123" };
        var options = new[]
        {
            new InteractionOption
            {
                Name = "notifications",
                Type = SlashCommandOptionType.SubCommandGroup,
                Options =
                [
                    new InteractionOption
                    {
                        Name = "enable",
                        Type = SlashCommandOptionType.SubCommand,
                        Options = [leafOption]
                    }
                ]
            }
        };

        // Act
        CommandContext.ExtractSubcommandInfo(options, out var group, out var subcommand, out var leafOptions);

        // Assert
        Assert.Equal("notifications", group);
        Assert.Equal("enable", subcommand);
        Assert.NotNull(leafOptions);
        Assert.Single(leafOptions);
        Assert.Equal("channel", leafOptions![0].Name);
    }

    [Fact]
    public void ExtractSubcommandInfo_SubcommandWithNoOptions_ReturnsNullLeafOptions()
    {
        // Arrange — /settings reset (no parameters)
        var options = new[]
        {
            new InteractionOption
            {
                Name = "reset",
                Type = SlashCommandOptionType.SubCommand,
                Options = null
            }
        };

        // Act
        CommandContext.ExtractSubcommandInfo(options, out var group, out var subcommand, out var leafOptions);

        // Assert
        Assert.Null(group);
        Assert.Equal("reset", subcommand);
        Assert.Null(leafOptions);
    }
}
