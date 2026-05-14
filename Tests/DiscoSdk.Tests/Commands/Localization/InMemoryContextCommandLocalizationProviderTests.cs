using DiscoSdk.Commands.Localization;
using DiscoSdk.Models;

namespace DiscoSdk.Tests.Commands.Localization;

public class InMemoryContextCommandLocalizationProviderTests
{
    private static IReadOnlyDictionary<string, CommandLocalization> Tree(string localizedName)
        => new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = localizedName },
        };

    [Fact]
    public void DefaultConstructor_IsNonStrict()
    {
        var provider = new InMemoryContextCommandLocalizationProvider();
        Assert.False(provider.Strict);
    }

    [Fact]
    public void GetLocalizations_NoGuildId_ReadsGlobalTable()
    {
        var provider = new InMemoryContextCommandLocalizationProvider();
        provider.RegisterCommand("Ban User", Tree("Block User"));

        var result = provider.GetLocalizations("Ban User", null);

        Assert.NotNull(result);
        Assert.Equal("Block User", result!["en-GB"].Name);
    }

    [Fact]
    public void GetLocalizations_NoGuildId_UnknownCommand_ReturnsNull()
    {
        var provider = new InMemoryContextCommandLocalizationProvider();
        Assert.Null(provider.GetLocalizations("nope", null));
    }

    [Fact]
    public void RegisterCommand_FlatNamesByLocale_WrapsIntoCommandLocalization()
    {
        var provider = new InMemoryContextCommandLocalizationProvider();
        provider.RegisterCommand("Report Message", new Dictionary<string, string>
        {
            ["en-GB"] = "Report",
            ["es-ES"] = "Denunciar",
        });

        var result = provider.GetLocalizations("Report Message", null)!;
        Assert.Equal("Report", result["en-GB"].Name);
        Assert.Equal("Denunciar", result["es-ES"].Name);
    }

    [Fact]
    public void GetLocalizations_WithGuildId_ReadsPerGuildEntry()
    {
        var guildId = Snowflake.Parse("111111111111111111");
        var provider = new InMemoryContextCommandLocalizationProvider();
        provider.RegisterCommand("Ban User", Tree("Block User"));
        provider.RegisterCommand(guildId, "Ban User", Tree("Punish Member"));

        var result = provider.GetLocalizations("Ban User", guildId);

        Assert.Equal("Punish Member", result!["en-GB"].Name);
    }

    [Fact]
    public void GetLocalizations_GuildHasNoEntry_NonStrict_FallsBackToGlobal()
    {
        var unknownGuild = Snowflake.Parse("222222222222222222");
        var provider = new InMemoryContextCommandLocalizationProvider(strict: false);
        provider.RegisterCommand("Ban User", Tree("Block User"));

        var result = provider.GetLocalizations("Ban User", unknownGuild);

        Assert.Equal("Block User", result!["en-GB"].Name);
    }

    [Fact]
    public void GetLocalizations_GuildHasNoEntry_Strict_ReturnsNull()
    {
        var unknownGuild = Snowflake.Parse("333333333333333333");
        var provider = new InMemoryContextCommandLocalizationProvider(strict: true);
        provider.RegisterCommand("Ban User", Tree("Block User"));

        Assert.Null(provider.GetLocalizations("Ban User", unknownGuild));
    }

    [Fact]
    public void RegisterCommand_PerGuild_FlatNamesByLocale_Works()
    {
        var guildId = Snowflake.Parse("444444444444444444");
        var provider = new InMemoryContextCommandLocalizationProvider();
        provider.RegisterCommand(guildId, "Report", new Dictionary<string, string>
        {
            ["en-GB"] = "Report (guild)",
        });

        var result = provider.GetLocalizations("Report", guildId)!;
        Assert.Equal("Report (guild)", result["en-GB"].Name);
    }

    [Fact]
    public void RegisterCommand_DuplicateName_Overwrites()
    {
        var provider = new InMemoryContextCommandLocalizationProvider();
        provider.RegisterCommand("Ban User", Tree("first"));
        provider.RegisterCommand("Ban User", Tree("second"));

        Assert.Equal("second", provider.GetLocalizations("Ban User", null)!["en-GB"].Name);
    }

    [Fact]
    public void RegisterCommand_NullName_Throws()
    {
        var provider = new InMemoryContextCommandLocalizationProvider();
        Assert.Throws<ArgumentException>(() => provider.RegisterCommand(null!, Tree("x")));
    }

    [Fact]
    public void RegisterCommand_NullTranslations_Throws()
    {
        var provider = new InMemoryContextCommandLocalizationProvider();
        Assert.Throws<ArgumentNullException>(() =>
            provider.RegisterCommand("x", (IReadOnlyDictionary<string, CommandLocalization>)null!));
    }

    [Fact]
    public void RegisterCommand_FluentChain()
    {
        var provider = new InMemoryContextCommandLocalizationProvider()
            .RegisterCommand("Ban User", Tree("Block"))
            .RegisterCommand("Report Message", Tree("Report"));

        Assert.Equal("Block", provider.GetLocalizations("Ban User", null)!["en-GB"].Name);
        Assert.Equal("Report", provider.GetLocalizations("Report Message", null)!["en-GB"].Name);
    }
}
