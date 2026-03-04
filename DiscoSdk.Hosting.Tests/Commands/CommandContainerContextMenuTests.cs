using DiscoSdk.Commands;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Tests.Commands;

public class CommandContainerContextMenuTests
{
    [Fact]
    public void AddGlobal_UserCommandBuilder_AddsCommand()
    {
        var container = new CommandContainer();

        container.AddGlobal((UserCommandBuilder b) => b.WithName("report_user"));

        var command = Assert.Single(container.GlobalCommands);
        Assert.Equal("report_user", command.Name);
        Assert.Equal(ApplicationCommandType.User, command.Type);
    }

    [Fact]
    public void AddGlobal_MessageCommandBuilder_AddsCommand()
    {
        var container = new CommandContainer();

        container.AddGlobal((MessageCommandBuilder b) => b.WithName("bookmark_message"));

        var command = Assert.Single(container.GlobalCommands);
        Assert.Equal("bookmark_message", command.Name);
        Assert.Equal(ApplicationCommandType.Message, command.Type);
    }

    [Fact]
    public void AddGuild_UserCommandBuilder_AddsCommand()
    {
        var container = new CommandContainer();
        var guildId = new Snowflake(12345);

        container.AddGuild(guildId, (UserCommandBuilder b) => b.WithName("ban_user"));

        Assert.True(container.GuildCommands.ContainsKey(guildId));
        var command = Assert.Single(container.GuildCommands[guildId]);
        Assert.Equal("ban_user", command.Name);
        Assert.Equal(ApplicationCommandType.User, command.Type);
    }

    [Fact]
    public void AddGuild_MessageCommandBuilder_AddsCommand()
    {
        var container = new CommandContainer();
        var guildId = new Snowflake(12345);

        container.AddGuild(guildId, (MessageCommandBuilder b) => b.WithName("pin_message"));

        Assert.True(container.GuildCommands.ContainsKey(guildId));
        var command = Assert.Single(container.GuildCommands[guildId]);
        Assert.Equal("pin_message", command.Name);
        Assert.Equal(ApplicationCommandType.Message, command.Type);
    }

    [Fact]
    public void AddGlobal_UserCommandBuilder_NullConfigure_ThrowsArgumentNullException()
    {
        var container = new CommandContainer();

        Assert.Throws<ArgumentNullException>(() =>
            container.AddGlobal((Func<UserCommandBuilder, UserCommandBuilder>)null!));
    }

    [Fact]
    public void AddGlobal_MessageCommandBuilder_NullConfigure_ThrowsArgumentNullException()
    {
        var container = new CommandContainer();

        Assert.Throws<ArgumentNullException>(() =>
            container.AddGlobal((Func<MessageCommandBuilder, MessageCommandBuilder>)null!));
    }

    [Fact]
    public void AddGuild_UserCommandBuilder_DefaultGuildId_ThrowsArgumentException()
    {
        var container = new CommandContainer();

        Assert.Throws<ArgumentException>(() =>
            container.AddGuild(default, (UserCommandBuilder b) => b.WithName("Test")));
    }

    [Fact]
    public void AddGuild_MessageCommandBuilder_DefaultGuildId_ThrowsArgumentException()
    {
        var container = new CommandContainer();

        Assert.Throws<ArgumentException>(() =>
            container.AddGuild(default, (MessageCommandBuilder b) => b.WithName("Test")));
    }

    [Fact]
    public void AddGlobal_DuplicateUserCommand_ThrowsInvalidOperationException()
    {
        var container = new CommandContainer();
        container.AddGlobal((UserCommandBuilder b) => b.WithName("report_user"));

        Assert.Throws<InvalidOperationException>(() =>
            container.AddGlobal((UserCommandBuilder b) => b.WithName("report_user")));
    }

    [Fact]
    public void AddGlobal_MixedBuilders_AllRegistered()
    {
        var container = new CommandContainer();

        container
            .AddGlobal((SlashCommandBuilder b) => b.WithName("ping").WithDescription("Pong"))
            .AddGlobal((UserCommandBuilder b) => b.WithName("report_user"))
            .AddGlobal((MessageCommandBuilder b) => b.WithName("bookmark"));

        Assert.Equal(3, container.GlobalCommands.Count);
    }

    [Fact]
    public void AddGlobal_UserCommandBuilder_ReturnsContainerForChaining()
    {
        var container = new CommandContainer();

        var result = container.AddGlobal((UserCommandBuilder b) => b.WithName("Test"));

        Assert.Same(container, result);
    }

    [Fact]
    public void AddGlobal_MessageCommandBuilder_ReturnsContainerForChaining()
    {
        var container = new CommandContainer();

        var result = container.AddGlobal((MessageCommandBuilder b) => b.WithName("Test"));

        Assert.Same(container, result);
    }

    [Fact]
    public void AddGuild_UserCommandBuilder_ReturnsContainerForChaining()
    {
        var container = new CommandContainer();

        var result = container.AddGuild(new Snowflake(1), (UserCommandBuilder b) => b.WithName("Test"));

        Assert.Same(container, result);
    }

    [Fact]
    public void AddGuild_MessageCommandBuilder_ReturnsContainerForChaining()
    {
        var container = new CommandContainer();

        var result = container.AddGuild(new Snowflake(1), (MessageCommandBuilder b) => b.WithName("Test"));

        Assert.Same(container, result);
    }
}
