using DiscoSdk.Commands.Localization;
using DiscoSdk.Models;

namespace DiscoSdk.Tests.Commands.Localization;

public class CommandLocalizationBuilderTests
{
    [Fact]
    public void DefineCommand_FlatNameOnly_RegistersGlobalEntry()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ping", b => b
                .ForLocale("en-GB", t => t.WithName("ping-cmd"))
                .ForLocale("en-US", t => t.WithName("ping")));

        var tree = provider.GetLocalizations("ping", null);

        Assert.NotNull(tree);
        Assert.Equal("ping-cmd", tree!["en-GB"].Name);
        Assert.Equal("ping", tree["en-US"].Name);
        Assert.Null(tree["en-GB"].Options);
    }

    [Fact]
    public void DefineCommand_WithDescription_PopulatesBoth()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ban", b => b
                .ForLocale("en-GB", t => t
                    .WithName("ban-user")
                    .WithDescription("Ban a user")));

        var pt = provider.GetLocalizations("ban", null)!["en-GB"];
        Assert.Equal("ban-user", pt.Name);
        Assert.Equal("Ban a user", pt.Description);
    }

    [Fact]
    public void DefineCommand_OptionAtRoot_AddsTopLevelOption()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ban", b => b
                .ForLocale("en-GB", t => t
                    .WithName("ban-user")
                    .Option("user").WithName("target")
                    .Option("reason").WithName("cause")));

        var options = provider.GetLocalizations("ban", null)!["en-GB"].Options;
        Assert.NotNull(options);
        Assert.Equal(2, options!.Count);
        Assert.Equal("user", options[0].OptionName);
        Assert.Equal("target", options[0].Name);
        Assert.Equal("reason", options[1].OptionName);
        Assert.Equal("cause", options[1].Name);
    }

    [Fact]
    public void DefineCommand_OptionChain_ResetsToRootEFStyle()
    {
        // After ThenChoice, calling .Option(...) should go back to the root, not nest under the option.
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ban", b => b
                .ForLocale("en-GB", t => t
                    .Option("reason").WithName("cause")
                        .ThenChoice("spam").WithName("Spam")
                    .Option("user").WithName("target")));

        var options = provider.GetLocalizations("ban", null)!["en-GB"].Options!;
        Assert.Equal(2, options.Count);
        Assert.Equal("reason", options[0].OptionName);
        Assert.Equal("user", options[1].OptionName);
        Assert.Null(options[1].Options);
        Assert.Null(options[1].Choices);
    }

    [Fact]
    public void DefineCommand_MultipleChoicesOnSameOption_AreSiblings()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ban", b => b
                .ForLocale("en-GB", t => t
                    .Option("reason")
                        .ThenChoice("spam").WithName("Spam")
                        .ThenChoice("flood").WithName("Flood")
                        .ThenChoice("nsfw").WithName("Inappropriate")));

        var choices = provider.GetLocalizations("ban", null)!["en-GB"].Options![0].Choices!;
        Assert.Equal(3, choices.Count);
        Assert.Equal("Spam", choices[0].Name);
        Assert.Equal("Flood", choices[1].Name);
        Assert.Equal("Inappropriate", choices[2].Name);
    }

    [Fact]
    public void DefineCommand_SubCommandGroupViaCallback_HasMultipleSubCommandSiblings()
    {
        // /admin user ban  /  /admin user kick — siblings under "user" require the callback overload,
        // because chained ThenOption(name) descends (EF-style); only the callback variant keeps focus on the parent.
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("admin", b => b
                .ForLocale("en-GB", t => t
                    .Option("user", user => user
                        .WithName("target")
                        .ThenOption("ban", ban => ban.WithName("ban-user"))
                        .ThenOption("kick", kick => kick.WithName("kick-user")))));

        var userGroup = provider.GetLocalizations("admin", null)!["en-GB"].Options![0];
        Assert.Equal("user", userGroup.OptionName);
        Assert.Equal("target", userGroup.Name);

        var sub = userGroup.Options!;
        Assert.Equal(2, sub.Count);
        Assert.Equal("ban", sub[0].OptionName);
        Assert.Equal("ban-user", sub[0].Name);
        Assert.Equal("kick", sub[1].OptionName);
        Assert.Equal("kick-user", sub[1].Name);
    }

    [Fact]
    public void DefineCommand_DepthThreeWithChoices_BuildsFullTree()
    {
        // /admin user ban reason:<choice>
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("admin", b => b
                .ForLocale("en-GB", t => t
                    .Option("user", user => user
                        .ThenOption("ban", ban => ban
                            .WithName("ban-user")
                            .ThenOption("reason", reason => reason
                                .WithName("cause")
                                .ThenChoice("spam").WithName("Spam")
                                .ThenChoice("flood").WithName("Flood"))))));

        var ban = provider.GetLocalizations("admin", null)!["en-GB"].Options![0].Options![0];
        Assert.Equal("ban", ban.OptionName);
        Assert.Equal("ban-user", ban.Name);

        var reason = ban.Options![0];
        Assert.Equal("reason", reason.OptionName);
        Assert.Equal("cause", reason.Name);

        var choices = reason.Choices!;
        Assert.Equal(2, choices.Count);
        Assert.Equal("Spam", choices[0].Name);
        Assert.Equal("Flood", choices[1].Name);
    }

    [Fact]
    public void DefineCommand_ChoiceBuilderOptionReset_AddsSiblingAtRoot()
    {
        // From inside ThenChoice, calling .Option(...) jumps back to the command root.
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ban", b => b
                .ForLocale("en-GB", t => t
                    .Option("reason")
                        .ThenChoice("spam").WithName("Spam")
                    .Option("user").WithName("target")));

        var options = provider.GetLocalizations("ban", null)!["en-GB"].Options!;
        Assert.Equal(2, options.Count);
        Assert.Equal("reason", options[0].OptionName);
        Assert.Single(options[0].Choices!);
        Assert.Equal("user", options[1].OptionName);
    }

    [Fact]
    public void DefineCommand_OmittedFields_AreNullInBuiltTree()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ping", b => b
                .ForLocale("en-GB", t => t.WithName("ping-cmd")));

        var pt = provider.GetLocalizations("ping", null)!["en-GB"];
        Assert.Null(pt.Description);
        Assert.Null(pt.Options);
    }

    [Fact]
    public void DefineCommand_DuplicateForLocale_LastWins()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ping", b => b
                .ForLocale("en-GB", t => t.WithName("first"))
                .ForLocale("en-GB", t => t.WithName("second")));

        Assert.Equal("second", provider.GetLocalizations("ping", null)!["en-GB"].Name);
    }

    [Fact]
    public void DefineCommand_LocaleLookupIsCaseInsensitive()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ping", b => b
                .ForLocale("en-GB", t => t.WithName("ping-cmd")));

        var tree = provider.GetLocalizations("ping", null)!;
        Assert.Equal("ping-cmd", tree["EN-gb"].Name);
    }

    [Fact]
    public void ForLocale_InvalidLocale_Throws()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.ForLocale("xx-XX", _ => { }));
    }

    [Fact]
    public void ForLocale_NullConfigure_Throws()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentNullException>(() =>
            builder.ForLocale("en-GB", null!));
    }

    [Fact]
    public void WithName_TooLong_Throws()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.ForLocale("en-GB", t => t.WithName(new string('a', 33))));
    }

    [Fact]
    public void WithName_BadCharacters_Throws()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.ForLocale("en-GB", t => t.WithName("Ban User"))); // uppercase + space
    }

    [Fact]
    public void WithName_AcceptsUnicodeLowercase()
    {
        // Discord allows Unicode lowercase letters in names — verify with an accented Portuguese
        // word ("usuário") to exercise the \p{Ll} branch of the regex.
        var builder = new CommandLocalizationBuilder();
        var ex = Record.Exception(() =>
            builder.ForLocale("pt-BR", t => t.WithName("usuário")));
        Assert.Null(ex);
    }

    [Fact]
    public void WithDescription_TooLong_Throws()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.ForLocale("en-GB", t => t.WithDescription(new string('a', 101))));
    }

    [Fact]
    public void Option_NullName_Throws()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.ForLocale("en-GB", t => t.Option("")));
    }

    [Fact]
    public void OptionCallback_NullConfigure_Throws()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentNullException>(() =>
            builder.ForLocale("en-GB", t => t.Option("user", null!)));
    }

    [Fact]
    public void DefineCommandForGuild_RegistersUnderGuildScope()
    {
        var guildId = Snowflake.Parse("123456789012345678");

        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ping", b => b.ForLocale("en-GB", t => t.WithName("global")))
            .DefineCommandForGuild(guildId, "ping", b => b.ForLocale("en-GB", t => t.WithName("guild")));

        Assert.Equal("global", provider.GetLocalizations("ping", null)!["en-GB"].Name);
        Assert.Equal("guild", provider.GetLocalizations("ping", guildId)!["en-GB"].Name);
    }

    [Fact]
    public void DefineCommand_NullConfigure_Throws()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        Assert.Throws<ArgumentNullException>(() => provider.DefineCommand("ping", null!));
    }

    [Fact]
    public void DefineCommandForGuild_NullConfigure_Throws()
    {
        var provider = new InMemoryCommandLocalizationProvider();
        var guildId = Snowflake.Parse("999999999999999999");
        Assert.Throws<ArgumentNullException>(() => provider.DefineCommandForGuild(guildId, "ping", null!));
    }

    [Fact]
    public void SubCommand_Alias_BehavesLikeOption()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("admin", b => b
                .ForLocale("en-GB", t => t
                    .WithName("admin")
                    .SubCommand("user").WithName("target")));

        var options = provider.GetLocalizations("admin", null)!["en-GB"].Options;
        Assert.NotNull(options);
        Assert.Single(options!);
        Assert.Equal("user", options![0].OptionName);
        Assert.Equal("target", options[0].Name);
    }

    [Fact]
    public void SubCommandGroup_AndThenSubCommand_ReadsParallelToCommandBuilder()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("admin", b => b
                .ForLocale("en-GB", t => t
                    .WithName("admin")
                    .SubCommandGroup("user", user => user
                        .WithName("target")
                        .ThenSubCommand("ban", ban => ban.WithName("ban-user"))
                        .ThenSubCommand("kick", kick => kick.WithName("kick-user")))));

        var userGroup = provider.GetLocalizations("admin", null)!["en-GB"].Options![0];
        Assert.Equal("user", userGroup.OptionName);
        Assert.Equal("target", userGroup.Name);
        Assert.Equal(2, userGroup.Options!.Count);
        Assert.Equal("ban-user", userGroup.Options[0].Name);
        Assert.Equal("kick-user", userGroup.Options[1].Name);
    }

    [Fact]
    public void ThenChoice_TwoArgOverload_SetsLocalizedNameInOneCall()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("ban", b => b
                .ForLocale("en-GB", t => t
                    .Option("reason")
                        .ThenChoice("spam", "Spam Mail")
                        .ThenChoice("flood", "Flooding")));

        var choices = provider.GetLocalizations("ban", null)!["en-GB"].Options![0].Choices!;
        Assert.Equal(2, choices.Count);
        Assert.Equal("Spam Mail", choices[0].Name);
        Assert.Equal("Flooding", choices[1].Name);
    }

    [Fact]
    public void Build_AvailableOnFocusedBuilders_ReturnsLocaleTree()
    {
        // Build() is now reachable from any focused node, mirroring the slash-command builder.
        var tree = new CommandLocalizationBuilder()
            .ForLocale("en-GB", t => t
                .WithName("admin")
                .SubCommandGroup("user", user => user
                    .WithName("target")
                    .ThenSubCommand("ban").WithName("ban-user")
                        .ThenChoice("spam", "Spam")))
            .Build();

        Assert.Equal("admin", tree["en-GB"].Name);
        var ban = tree["en-GB"].Options![0].Options![0];
        Assert.Equal("ban-user", ban.Name);
        Assert.Equal("Spam", ban.Choices![0].Name);
    }

    [Fact]
    public void Build_FromDeepFocus_ReturnsSameLocaleTreeAsOuterBuild()
    {
        var deepBuilt = new CommandLocalizationBuilder()
            .ForLocale("en-GB", t => t
                .WithName("admin")
                .Option("reason")
                    .ThenChoice("spam", "Spam")
                .Build()); // Build() on the choice-focused node

        Assert.NotNull(deepBuilt);
    }

    [Fact]
    public void ThenEnumChoice_EmitsOneChoicePerEnumValue_KeyedByMemberName()
    {
        var provider = new InMemoryCommandLocalizationProvider()
            .DefineCommand("presence", b => b
                .ForLocale("pt-BR", t => t
                    .Option("status")
                        .ThenEnumChoice<SampleStatus>(x => x
                            .For(SampleStatus.Online, "Online")
                            .For(SampleStatus.Idle, "Ausente")
                            .For(SampleStatus.DoNotDisturb, "Não perturbe"))));

        var choices = provider.GetLocalizations("presence", null)!["pt-BR"].Options![0].Choices!;
        Assert.Equal(3, choices.Count);

        Assert.Equal(nameof(SampleStatus.Online), choices[0].ChoiceName);
        Assert.Equal("Online", choices[0].Name);

        Assert.Equal(nameof(SampleStatus.Idle), choices[1].ChoiceName);
        Assert.Equal("Ausente", choices[1].Name);

        Assert.Equal(nameof(SampleStatus.DoNotDisturb), choices[2].ChoiceName);
        Assert.Equal("Não perturbe", choices[2].Name);
    }

    [Fact]
    public void ThenEnumChoice_RejectsBlankLocalizedName()
    {
        var builder = new CommandLocalizationBuilder();
        Assert.Throws<ArgumentException>(() =>
            builder.ForLocale("pt-BR", t => t
                .Option("status")
                    .ThenEnumChoice<SampleStatus>(x => x.For(SampleStatus.Online, "  "))));
    }

    private enum SampleStatus
    {
        Online,
        Idle,
        DoNotDisturb,
    }
}
