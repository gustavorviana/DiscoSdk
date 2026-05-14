using DiscoSdk.Commands.Localization;
using DiscoSdk.Models;

namespace DiscoSdk.Tests.Commands.Localization;

public class InMemoryCommandLocalizationProviderTests
{
    private static IReadOnlyDictionary<string, CommandLocalization> Tree(string localizedName)
        => new Dictionary<string, CommandLocalization>
        {
            ["en-GB"] = new CommandLocalization { Name = localizedName },
        };

    [Fact]
    public void DefaultConstructor_IsNonStrict()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        Assert.False(provider.Strict);
    }

    [Fact]
    public void StrictConstructor_SetsStrict()
    {
        var provider = new InMemoryCommandLocalizationProvider(strict: true);
        Assert.True(provider.Strict);
    }

    [Fact]
    public void GetLocalizations_NoGuildId_ReadsGlobalTable()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        provider.RegisterCommand("search", Tree("find"));

        var result = provider.GetLocalizations("search", null);

        Assert.NotNull(result);
        Assert.Equal("find", result!["en-GB"].Name);
    }

    [Fact]
    public void GetLocalizations_NoGuildId_UnknownCommand_ReturnsNull()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        Assert.Null(provider.GetLocalizations("nope", null));
    }

    [Fact]
    public void GetLocalizations_WithGuildId_ReadsPerGuildEntry()
    {
        var guildId = Snowflake.Parse("111111111111111111");
        var provider = new InMemoryCommandLocalizationProvider();
        provider.RegisterCommand("search", Tree("find-global"));
        provider.RegisterCommand(guildId, "search", Tree("find-guild"));

        var result = provider.GetLocalizations("search", guildId);

        Assert.Equal("find-guild", result!["en-GB"].Name);
    }

    [Fact]
    public void GetLocalizations_GuildHasNoEntry_NonStrict_FallsBackToGlobal()
    {
        var unknownGuild = Snowflake.Parse("222222222222222222");
        var provider = new InMemoryCommandLocalizationProvider(strict: false);
        provider.RegisterCommand("search", Tree("find-global"));

        var result = provider.GetLocalizations("search", unknownGuild);

        Assert.Equal("find-global", result!["en-GB"].Name);
    }

    [Fact]
    public void GetLocalizations_GuildExistsButCommandMissing_NonStrict_FallsBackToGlobal()
    {
        var guildId = Snowflake.Parse("333333333333333333");
        var provider = new InMemoryCommandLocalizationProvider(strict: false);
        provider.RegisterCommand("search", Tree("find-global"));
        provider.RegisterCommand(guildId, "other", Tree("other-guild"));

        var result = provider.GetLocalizations("search", guildId);

        Assert.Equal("find-global", result!["en-GB"].Name);
    }

    [Fact]
    public void GetLocalizations_GuildHasNoEntry_Strict_ReturnsNull()
    {
        var unknownGuild = Snowflake.Parse("444444444444444444");
        var provider = new InMemoryCommandLocalizationProvider(strict: true);
        provider.RegisterCommand("search", Tree("find-global"));

        Assert.Null(provider.GetLocalizations("search", unknownGuild));
    }

    [Fact]
    public void GetLocalizations_GuildExistsButCommandMissing_Strict_ReturnsNull()
    {
        var guildId = Snowflake.Parse("555555555555555555");
        var provider = new InMemoryCommandLocalizationProvider(strict: true);
        provider.RegisterCommand("search", Tree("find-global"));
        provider.RegisterCommand(guildId, "other", Tree("other-guild"));

        Assert.Null(provider.GetLocalizations("search", guildId));
    }

    [Fact]
    public void RegisterCommand_DuplicateName_Overwrites()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        provider.RegisterCommand("search", Tree("first"));
        provider.RegisterCommand("search", Tree("second"));

        Assert.Equal("second", provider.GetLocalizations("search", null)!["en-GB"].Name);
    }

    [Fact]
    public void RegisterCommand_PerGuild_DuplicateOverwrites()
    {
        var guildId = Snowflake.Parse("777777777777777777");
        var provider = new InMemoryCommandLocalizationProvider();
        provider.RegisterCommand(guildId, "search", Tree("from-first"));
        provider.RegisterCommand(guildId, "search", Tree("from-second"));

        Assert.Equal("from-second", provider.GetLocalizations("search", guildId)!["en-GB"].Name);
    }

    [Fact]
    public void RegisterCommand_NullName_Throws()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        Assert.Throws<ArgumentException>(() => provider.RegisterCommand(null!, Tree("x")));
    }

    [Fact]
    public void RegisterCommand_EmptyName_Throws()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        Assert.Throws<ArgumentException>(() => provider.RegisterCommand("", Tree("x")));
    }

    [Fact]
    public void RegisterCommand_NullTranslations_Throws()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        Assert.Throws<ArgumentNullException>(() => provider.RegisterCommand("search", null!));
    }

    [Fact]
    public void RegisterCommand_FluentChain()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .RegisterCommand("ping", Tree("ping-cmd"))
            .RegisterCommand("ban", Tree("ban-user"));

        Assert.Equal("ping-cmd", provider.GetLocalizations("ping", null)!["en-GB"].Name);
        Assert.Equal("ban-user", provider.GetLocalizations("ban", null)!["en-GB"].Name);
    }
}
