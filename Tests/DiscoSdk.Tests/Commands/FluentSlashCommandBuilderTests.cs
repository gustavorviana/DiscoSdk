using DiscoSdk.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Tests.Commands;

public class FluentSlashCommandBuilderTests
{
    private static SlashCommandBuilder BaseBuilder() =>
        new SlashCommandBuilder().WithName("test").WithDescription("Test command");

    [Fact]
    public void StringOption_FocusedBuilder_AddsOption()
    {
        var cmd = BaseBuilder()
            .StringOption("name", "Your name", required: true)
            .Build();

        var opt = Assert.Single(cmd.Options);
        Assert.Equal("name", opt.Name);
        Assert.Equal(SlashCommandOptionType.String, opt.Type);
        Assert.True(opt.Required);
    }

    [Fact]
    public void StringOption_ChainedThenChoice_AppendsAllAsSiblings()
    {
        var cmd = BaseBuilder()
            .StringOption("reason", "Reason")
                .ThenChoice("Spam", "spam")
                .ThenChoice("Flood", "flood")
                .ThenChoice("NSFW", "nsfw")
            .Build();

        var opt = Assert.Single(cmd.Options);
        Assert.NotNull(opt.Choices);
        Assert.Equal(3, opt.Choices!.Length);
        Assert.Equal("Spam", opt.Choices[0].Name);
        Assert.Equal("spam", opt.Choices[0].Value);
        Assert.Equal("Flood", opt.Choices[1].Name);
        Assert.Equal("NSFW", opt.Choices[2].Name);
    }

    [Fact]
    public void StringOption_AfterChoice_OptionResetsToRoot()
    {
        // .StringOption(...) reachable from a ChoiceBuilder via SlashCommandFluentNode
        // forwarder resets to the command root — EF-style.
        var cmd = BaseBuilder()
            .StringOption("reason", "Reason")
                .ThenChoice("Spam", "spam")
            .StringOption("user", "Username")
            .Build();

        Assert.Equal(2, cmd.Options.Length);
        Assert.Equal("reason", cmd.Options[0].Name);
        Assert.Single(cmd.Options[0].Choices!);
        Assert.Equal("user", cmd.Options[1].Name);
        Assert.Null(cmd.Options[1].Choices);
    }

    [Fact]
    public void StringOption_WithMinMaxLength_AppliesConstraints()
    {
        var cmd = BaseBuilder()
            .StringOption("note", "A note")
                .WithMinLength(5)
                .WithMaxLength(100)
            .Build();

        var opt = Assert.Single(cmd.Options);
        Assert.Equal(5, opt.MinLength);
        Assert.Equal(100, opt.MaxLength);
    }

    [Fact]
    public void IntegerOption_ThenChoice_StoresLongValue()
    {
        var cmd = BaseBuilder()
            .IntegerOption("count", "Count")
                .ThenChoice("One", 1L)
                .ThenChoice("Two", 2L)
            .Build();

        var opt = Assert.Single(cmd.Options);
        Assert.Equal(SlashCommandOptionType.Integer, opt.Type);
        Assert.Equal(2, opt.Choices!.Length);
        Assert.Equal(1L, opt.Choices[0].Value);
    }

    [Fact]
    public void NumberOption_ThenChoice_StoresDoubleValue()
    {
        var cmd = BaseBuilder()
            .NumberOption("ratio", "Ratio")
                .ThenChoice("Half", 0.5)
                .ThenChoice("Full", 1.0)
            .Build();

        var opt = Assert.Single(cmd.Options);
        Assert.Equal(SlashCommandOptionType.Number, opt.Type);
        Assert.Equal(0.5, opt.Choices![0].Value);
    }

    [Fact]
    public void ChannelOption_WithChannelTypes_AppliesFilter()
    {
        var cmd = BaseBuilder()
            .ChannelOption("ch", "Channel")
                .WithChannelTypes(ChannelType.GuildText, ChannelType.GuildVoice)
            .Build();

        var opt = Assert.Single(cmd.Options);
        Assert.Equal(SlashCommandOptionType.Channel, opt.Type);
        Assert.Equal(new[] { ChannelType.GuildText, ChannelType.GuildVoice }, opt.ChannelTypes);
    }

    [Fact]
    public void LeafOptions_BooleanUserRoleMentionableAttachment_AllAddTypedOptions()
    {
        var cmd = BaseBuilder()
            .BooleanOption("flag", "A flag")
            .UserOption("user", "A user")
            .RoleOption("role", "A role")
            .MentionableOption("anyone", "Anyone")
            .AttachmentOption("file", "A file")
            .Build();

        Assert.Equal(5, cmd.Options.Length);
        Assert.Equal(SlashCommandOptionType.Boolean, cmd.Options[0].Type);
        Assert.Equal(SlashCommandOptionType.User, cmd.Options[1].Type);
        Assert.Equal(SlashCommandOptionType.Role, cmd.Options[2].Type);
        Assert.Equal(SlashCommandOptionType.Mentionable, cmd.Options[3].Type);
        Assert.Equal(SlashCommandOptionType.Attachment, cmd.Options[4].Type);
    }

    [Fact]
    public void SubCommand_WithThenOptions_PopulatesChildren()
    {
        var cmd = BaseBuilder()
            .SubCommand("ban", "Ban a user", sc => sc
                .ThenUserOption("target", "User to ban", required: true)
                .ThenStringOption("reason", "Reason", required: false))
            .Build();

        var sc = Assert.Single(cmd.Options);
        Assert.Equal(SlashCommandOptionType.SubCommand, sc.Type);
        Assert.Equal(2, sc.Options!.Length);
        Assert.Equal("target", sc.Options[0].Name);
        Assert.Equal(SlashCommandOptionType.User, sc.Options[0].Type);
        Assert.True(sc.Options[0].Required);
        Assert.Equal("reason", sc.Options[1].Name);
        Assert.Equal(SlashCommandOptionType.String, sc.Options[1].Type);
    }

    [Fact]
    public void SubCommand_WithChoicesInsideThenStringOption_ComposesFullTree()
    {
        var cmd = BaseBuilder()
            .SubCommand("ban", "Ban a user", sc => sc
                .ThenUserOption("target", "User", required: true)
                .ThenStringOption("reason", "Reason", reason => reason
                    .ThenChoice("Spam", "spam")
                    .ThenChoice("Flood", "flood")))
            .Build();

        var reason = cmd.Options[0].Options![1];
        Assert.Equal("reason", reason.Name);
        Assert.Equal(2, reason.Choices!.Length);
        Assert.Equal("spam", reason.Choices[0].Value);
    }

    [Fact]
    public void SubCommandGroup_WithSiblingSubCommands_ComposesDepthTwoTree()
    {
        // /admin user ban   /admin user kick
        var cmd = new SlashCommandBuilder()
            .WithName("admin")
            .WithDescription("Admin commands")
            .SubCommandGroup("user", "User management", g => g
                .ThenSubCommand("ban", "Ban a user", ban => ban
                    .ThenUserOption("target", "User", required: true))
                .ThenSubCommand("kick", "Kick a user", kick => kick
                    .ThenUserOption("target", "User", required: true)))
            .Build();

        var group = Assert.Single(cmd.Options);
        Assert.Equal(SlashCommandOptionType.SubCommandGroup, group.Type);
        Assert.Equal(2, group.Options!.Length);

        var ban = group.Options[0];
        Assert.Equal("ban", ban.Name);
        Assert.Equal(SlashCommandOptionType.SubCommand, ban.Type);
        Assert.Equal("target", ban.Options![0].Name);

        var kick = group.Options[1];
        Assert.Equal("kick", kick.Name);
    }

    [Fact]
    public void SubCommandGroup_DepthThreeWithChoices_BuildsFullTree()
    {
        var cmd = new SlashCommandBuilder()
            .WithName("admin")
            .WithDescription("Admin commands")
            .SubCommandGroup("user", "User management", g => g
                .ThenSubCommand("ban", "Ban a user", ban => ban
                    .ThenUserOption("target", "User", required: true)
                    .ThenStringOption("reason", "Reason", r => r
                        .ThenChoice("Spam", "spam")
                        .ThenChoice("Flood", "flood"))))
            .Build();

        var reason = cmd.Options[0].Options![0].Options![1];
        Assert.Equal("reason", reason.Name);
        Assert.Equal(2, reason.Choices!.Length);
    }

    [Fact]
    public void Autocomplete_AfterChoice_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var b = BaseBuilder();
            var opt = b.StringOption("x", "x");
            opt.ThenChoice("a", "a");
            opt.WithAutocomplete();
        });
    }

    [Fact]
    public void Choice_AfterAutocomplete_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            var b = BaseBuilder();
            var opt = b.StringOption("x", "x").WithAutocomplete();
            opt.ThenChoice("a", "a");
        });
    }

    [Fact]
    public void Build_FromAnyFocusedBuilder_ReturnsRootBuiltCommand()
    {
        // .Build() is available on every focused node, delegates to the root.
        var fromChoiceFocus = BaseBuilder()
            .StringOption("reason", "Reason")
                .ThenChoice("Spam", "spam")
                .Build();

        Assert.Equal("test", fromChoiceFocus.Name);
        Assert.Single(fromChoiceFocus.Options);
    }

    [Fact]
    public void Choice_WithNameLocalizations_AttachesToFocusedChoice()
    {
        var cmd = BaseBuilder()
            .StringOption("reason", "Reason")
                .ThenChoice("Spam", "spam")
                    .WithNameLocalizations(new Dictionary<string, string> { ["en-GB"] = "Spam Mail" })
                .ThenChoice("Flood", "flood")
                    .AddNameLocalization("en-GB", "Flooding")
            .Build();

        var choices = cmd.Options[0].Choices!;
        Assert.Equal("Spam Mail", choices[0].NameLocalizations!["en-GB"]);
        Assert.Equal("Flooding", choices[1].NameLocalizations!["en-GB"]);
    }

    [Fact]
    public void SubCommand_FluentChaining_StaysOnFocusedSubCommand()
    {
        // Inside the SubCommand callback, ThenXxxOption(name, configure) keeps focus
        // on the sub-command — multiple siblings can be added in one chain.
        var cmd = BaseBuilder()
            .SubCommand("setup", "Setup", sc => sc
                .ThenStringOption("name", "Name")
                .ThenIntegerOption("age", "Age")
                .ThenBooleanOption("public", "Visibility"))
            .Build();

        var setup = Assert.Single(cmd.Options);
        Assert.Equal(3, setup.Options!.Length);
        Assert.Equal(SlashCommandOptionType.String, setup.Options[0].Type);
        Assert.Equal(SlashCommandOptionType.Integer, setup.Options[1].Type);
        Assert.Equal(SlashCommandOptionType.Boolean, setup.Options[2].Type);
    }

    [Fact]
    public void Validation_InvalidOptionName_ThrowsAtAddTime()
    {
        Assert.Throws<ArgumentException>(() =>
            BaseBuilder().StringOption("Bad Name With Spaces", "desc"));
    }

    [Fact]
    public void Validation_InvalidOptionDescription_ThrowsAtAddTime()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BaseBuilder().StringOption("ok", new string('x', 101)));
    }

    [Fact]
    public void Validation_TooManyChoices_ThrowsBeforeAppending()
    {
        var opt = BaseBuilder().StringOption("x", "x");
        for (var i = 0; i < 25; i++)
            opt.ThenChoice($"name{i}", $"value{i}");

        Assert.Throws<ArgumentException>(() => opt.ThenChoice("overflow", "overflow"));
    }

    [Fact]
    public void Validation_NullCallback_OnSubCommand_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            BaseBuilder().SubCommand("foo", "desc", null!));
    }

    [Fact]
    public void Validation_NullCallback_OnThenStringOption_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            BaseBuilder().SubCommand("foo", "desc")
                .ThenStringOption("x", "desc", (Action<SlashCommandStringOptionBuilder>)null!));
    }

    [Fact]
    public void RetroCompat_AddStringOption_StillWorksAlongsideFluent()
    {
        var cmd = BaseBuilder()
            .AddStringOption("legacy", "Legacy option", required: true)
            .StringOption("modern", "Modern option")
            .Build();

        Assert.Equal(2, cmd.Options.Length);
        Assert.Equal("legacy", cmd.Options[0].Name);
        Assert.Equal("modern", cmd.Options[1].Name);
    }
}
