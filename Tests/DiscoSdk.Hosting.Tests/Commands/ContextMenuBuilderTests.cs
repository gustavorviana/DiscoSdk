using DiscoSdk.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Tests.Commands;

public class ContextMenuBuilderTests
{
    [Fact]
    public void Build_User_SetsTypeToUser()
    {
        var command = new ContextMenuBuilder()
            .WithName("report_user")
            .Build(ContextMenuType.User);

        Assert.Equal(ApplicationCommandType.User, command.Type);
    }

    [Fact]
    public void Build_Message_SetsTypeToMessage()
    {
        var command = new ContextMenuBuilder()
            .WithName("translate_message")
            .Build(ContextMenuType.Message);

        Assert.Equal(ApplicationCommandType.Message, command.Type);
    }

    [Fact]
    public void Build_SetsDescriptionToEmptyString()
    {
        var command = new ContextMenuBuilder()
            .WithName("report_user")
            .Build(ContextMenuType.User);

        Assert.Equal(string.Empty, command.Description);
    }

    [Fact]
    public void Build_PreservesName()
    {
        var command = new ContextMenuBuilder()
            .WithName("ReportUser")
            .Build(ContextMenuType.User);

        Assert.Equal("ReportUser", command.Name);
    }

    [Fact]
    public void Build_AllowsMixedCase()
    {
        var command = new ContextMenuBuilder()
            .WithName("GetUserInfo")
            .Build(ContextMenuType.User);

        Assert.Equal("GetUserInfo", command.Name);
    }

    [Fact]
    public void Build_ThrowsWhenNameNotSet()
    {
        var builder = new ContextMenuBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build(ContextMenuType.User));
    }

    [Fact]
    public void WithName_ThrowsOnNullOrWhitespace()
    {
        var builder = new ContextMenuBuilder();
        Assert.Throws<ArgumentException>(() => builder.WithName(""));
        Assert.Throws<ArgumentException>(() => builder.WithName("   "));
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithName_ThrowsWhenTooLong()
    {
        var builder = new ContextMenuBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithName(new string('A', 33)));
    }

    [Fact]
    public void WithName_Accepts32Characters()
    {
        var command = new ContextMenuBuilder()
            .WithName(new string('A', 32))
            .Build(ContextMenuType.User);

        Assert.Equal(32, command.Name.Length);
    }

    [Fact]
    public void Build_User_SetsAllOptionalProperties()
    {
        var localizations = new Dictionary<string, string> { ["pt-BR"] = "ReportarUsuario" };

        var command = new ContextMenuBuilder()
            .WithName("report_user")
            .WithNameLocalizations(localizations)
            .WithDefaultMemberPermissions("8")
            .WithDmPermission(false)
            .WithNsfw(true)
            .Build(ContextMenuType.User);

        Assert.Equal("report_user", command.Name);
        Assert.Equal(ApplicationCommandType.User, command.Type);
        Assert.Equal(string.Empty, command.Description);
        Assert.Equal(localizations, command.NameLocalizations);
        Assert.Equal("8", command.DefaultMemberPermissions);
        Assert.False(command.DmPermission);
        Assert.True(command.Nsfw);
    }

    [Fact]
    public void Build_Message_SetsAllOptionalProperties()
    {
        var localizations = new Dictionary<string, string> { ["pt-BR"] = "TraduzirMensagem" };

        var command = new ContextMenuBuilder()
            .WithName("translate_message")
            .WithNameLocalizations(localizations)
            .WithDefaultMemberPermissions("8")
            .WithDmPermission(false)
            .WithNsfw(true)
            .Build(ContextMenuType.Message);

        Assert.Equal("translate_message", command.Name);
        Assert.Equal(ApplicationCommandType.Message, command.Type);
        Assert.Equal(string.Empty, command.Description);
        Assert.Equal(localizations, command.NameLocalizations);
        Assert.Equal("8", command.DefaultMemberPermissions);
        Assert.False(command.DmPermission);
        Assert.True(command.Nsfw);
    }

    [Fact]
    public void WithNameLocalizations_ThrowsOnInvalidLocale()
    {
        var builder = new ContextMenuBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.WithNameLocalizations(new Dictionary<string, string> { ["invalid-locale"] = "test" }));
    }

    [Fact]
    public void WithNameLocalizations_ThrowsOnEmptyValue()
    {
        var builder = new ContextMenuBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.WithNameLocalizations(new Dictionary<string, string> { ["en-US"] = "" }));
    }

    [Fact]
    public void WithNameLocalizations_ThrowsOnTooLongValue()
    {
        var builder = new ContextMenuBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.WithNameLocalizations(new Dictionary<string, string> { ["en-US"] = new string('A', 33) }));
    }

    [Fact]
    public void Build_HasNoDescriptionMethod()
    {
        // Verify at compile time that there is no WithDescription method
        var methods = typeof(ContextMenuBuilder).GetMethods()
            .Select(m => m.Name)
            .ToArray();

        Assert.DoesNotContain("WithDescription", methods);
    }

    [Fact]
    public void Build_ThrowsOnUnknownContextMenuType()
    {
        var builder = new ContextMenuBuilder().WithName("test");
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Build((ContextMenuType)999));
    }
}
