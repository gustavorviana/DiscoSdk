using DiscoSdk.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Tests.Commands;

public class UserCommandBuilderTests
{
    [Fact]
    public void Build_SetsTypeToUser()
    {
        var command = new UserCommandBuilder()
            .WithName("report_user")
            .Build();

        Assert.Equal(ApplicationCommandType.User, command.Type);
    }

    [Fact]
    public void Build_SetsDescriptionToEmptyString()
    {
        var command = new UserCommandBuilder()
            .WithName("report_user")
            .Build();

        Assert.Equal(string.Empty, command.Description);
    }

    [Fact]
    public void Build_PreservesName()
    {
        var command = new UserCommandBuilder()
            .WithName("ReportUser")
            .Build();

        Assert.Equal("ReportUser", command.Name);
    }

    [Fact]
    public void Build_AllowsSpacesAndMixedCase()
    {
        var command = new UserCommandBuilder()
            .WithName("GetUserInfo")
            .Build();

        Assert.Equal("GetUserInfo", command.Name);
    }

    [Fact]
    public void Build_ThrowsWhenNameNotSet()
    {
        var builder = new UserCommandBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void WithName_ThrowsOnNullOrWhitespace()
    {
        var builder = new UserCommandBuilder();
        Assert.Throws<ArgumentException>(() => builder.WithName(""));
        Assert.Throws<ArgumentException>(() => builder.WithName("   "));
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithName_ThrowsWhenTooLong()
    {
        var builder = new UserCommandBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithName(new string('A', 33)));
    }

    [Fact]
    public void WithName_Accepts32Characters()
    {
        var command = new UserCommandBuilder()
            .WithName(new string('A', 32))
            .Build();

        Assert.Equal(32, command.Name.Length);
    }

    [Fact]
    public void Build_SetsAllOptionalProperties()
    {
        var localizations = new Dictionary<string, string> { ["pt-BR"] = "ReportarUsuario" };

        var command = new UserCommandBuilder()
            .WithName("report_user")
            .WithNameLocalizations(localizations)
            .WithDefaultMemberPermissions("8")
            .WithDmPermission(false)
            .WithNsfw(true)
            .Build();

        Assert.Equal("report_user", command.Name);
        Assert.Equal(ApplicationCommandType.User, command.Type);
        Assert.Equal(string.Empty, command.Description);
        Assert.Equal(localizations, command.NameLocalizations);
        Assert.Equal("8", command.DefaultMemberPermissions);
        Assert.False(command.DmPermission);
        Assert.True(command.Nsfw);
    }

    [Fact]
    public void WithNameLocalizations_ThrowsOnInvalidLocale()
    {
        var builder = new UserCommandBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.WithNameLocalizations(new Dictionary<string, string> { ["invalid-locale"] = "test" }));
    }

    [Fact]
    public void WithNameLocalizations_ThrowsOnEmptyValue()
    {
        var builder = new UserCommandBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.WithNameLocalizations(new Dictionary<string, string> { ["en-US"] = "" }));
    }

    [Fact]
    public void WithNameLocalizations_ThrowsOnTooLongValue()
    {
        var builder = new UserCommandBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.WithNameLocalizations(new Dictionary<string, string> { ["en-US"] = new string('A', 33) }));
    }

    [Fact]
    public void Build_HasNoDescriptionMethod()
    {
        // Verify at compile time that there is no WithDescription method
        var methods = typeof(UserCommandBuilder).GetMethods()
            .Select(m => m.Name)
            .ToArray();

        Assert.DoesNotContain("WithDescription", methods);
    }
}

public class MessageCommandBuilderTests
{
    [Fact]
    public void Build_SetsTypeToMessage()
    {
        var command = new MessageCommandBuilder()
            .WithName("translate_message")
            .Build();

        Assert.Equal(ApplicationCommandType.Message, command.Type);
    }

    [Fact]
    public void Build_SetsDescriptionToEmptyString()
    {
        var command = new MessageCommandBuilder()
            .WithName("translate_message")
            .Build();

        Assert.Equal(string.Empty, command.Description);
    }

    [Fact]
    public void Build_PreservesName()
    {
        var command = new MessageCommandBuilder()
            .WithName("TranslateMessage")
            .Build();

        Assert.Equal("TranslateMessage", command.Name);
    }

    [Fact]
    public void Build_AllowsSpacesAndMixedCase()
    {
        var command = new MessageCommandBuilder()
            .WithName("PinThisMessage")
            .Build();

        Assert.Equal("PinThisMessage", command.Name);
    }

    [Fact]
    public void Build_ThrowsWhenNameNotSet()
    {
        var builder = new MessageCommandBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void WithName_ThrowsOnNullOrWhitespace()
    {
        var builder = new MessageCommandBuilder();
        Assert.Throws<ArgumentException>(() => builder.WithName(""));
        Assert.Throws<ArgumentException>(() => builder.WithName("   "));
        Assert.Throws<ArgumentException>(() => builder.WithName(null!));
    }

    [Fact]
    public void WithName_ThrowsWhenTooLong()
    {
        var builder = new MessageCommandBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithName(new string('A', 33)));
    }

    [Fact]
    public void WithName_Accepts32Characters()
    {
        var command = new MessageCommandBuilder()
            .WithName(new string('A', 32))
            .Build();

        Assert.Equal(32, command.Name.Length);
    }

    [Fact]
    public void Build_SetsAllOptionalProperties()
    {
        var localizations = new Dictionary<string, string> { ["pt-BR"] = "TraduzirMensagem" };

        var command = new MessageCommandBuilder()
            .WithName("translate_message")
            .WithNameLocalizations(localizations)
            .WithDefaultMemberPermissions("8")
            .WithDmPermission(false)
            .WithNsfw(true)
            .Build();

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
        var builder = new MessageCommandBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.WithNameLocalizations(new Dictionary<string, string> { ["invalid-locale"] = "test" }));
    }

    [Fact]
    public void Build_HasNoDescriptionMethod()
    {
        var methods = typeof(MessageCommandBuilder).GetMethods()
            .Select(m => m.Name)
            .ToArray();

        Assert.DoesNotContain("WithDescription", methods);
    }
}
