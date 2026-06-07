using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Tests.Models;

/// <summary>
/// Covers the fluent builders for Discord onboarding prompts and options. Validates required
/// fields, length caps, and that Build() produces a wire-shape POCO carrying every set value.
/// </summary>
public class OnboardingPromptBuilderTests
{
    [Fact]
    public void OptionBuilder_RequiresTitleBeforeBuild()
    {
        var b = new OnboardingPromptOptionBuilder();
        Assert.Throws<InvalidOperationException>(() => b.Build());
    }

    [Fact]
    public void OptionBuilder_TitleLengthValidated()
    {
        var b = new OnboardingPromptOptionBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => b.SetTitle(new string('x', 51)));
    }

    [Fact]
    public void OptionBuilder_CarriesEveryFieldThroughBuild()
    {
        var option = new OnboardingPromptOptionBuilder()
            .SetId(new Snowflake(42))
            .SetTitle("Devs")
            .SetDescription("Backend, infra, tooling")
            .AddChannel(new Snowflake(900))
            .AddChannel(new Snowflake(901))
            .AddRole(new Snowflake(800))
            .SetUnicodeEmoji("🛠️")
            .Build();

        Assert.Equal(new Snowflake(42), option.Id);
        Assert.Equal("Devs", option.Title);
        Assert.Equal("Backend, infra, tooling", option.Description);
        Assert.Equal([new Snowflake(900), new Snowflake(901)], option.ChannelIds);
        Assert.Equal([new Snowflake(800)], option.RoleIds);
        Assert.Equal("🛠️", option.EmojiName);
        Assert.Null(option.EmojiId);
    }

    [Fact]
    public void OptionBuilder_CustomEmojiClearsUnicode()
    {
        var option = new OnboardingPromptOptionBuilder()
            .SetTitle("X")
            .SetUnicodeEmoji("🎉")
            .SetCustomEmoji(new Snowflake(123), animated: true)
            .Build();

        Assert.Null(option.EmojiName);
        Assert.Equal(new Snowflake(123), option.EmojiId);
        Assert.True(option.EmojiAnimated);
    }

    [Fact]
    public void PromptBuilder_RequiresTitleAndOptionsBeforeBuild()
    {
        Assert.Throws<InvalidOperationException>(() => new OnboardingPromptBuilder().Build());
        Assert.Throws<InvalidOperationException>(() => new OnboardingPromptBuilder().SetTitle("X").Build());
    }

    [Fact]
    public void PromptBuilder_OptionsCap50()
    {
        var b = new OnboardingPromptBuilder().SetTitle("X");
        for (var i = 0; i < 50; i++)
            b.AddOption(new OnboardingPromptOptionBuilder().SetTitle("o"));
        Assert.Throws<InvalidOperationException>(() =>
            b.AddOption(new OnboardingPromptOptionBuilder().SetTitle("overflow")));
    }

    [Fact]
    public void PromptBuilder_AddOptionInlineCallbackBuilds()
    {
        var prompt = new OnboardingPromptBuilder()
            .SetTitle("X")
            .AddOption(o => o
                .SetTitle("Devs")
                .AddRole(new Snowflake(800))
                .SetUnicodeEmoji("🛠️"))
            .Build();

        var opt = Assert.Single(prompt.Options);
        Assert.Equal("Devs", opt.Title);
        Assert.Equal([new Snowflake(800)], opt.RoleIds);
        Assert.Equal("🛠️", opt.EmojiName);
    }

    [Fact]
    public void PromptBuilder_AddOptionNullCallback_Throws()
    {
        var b = new OnboardingPromptBuilder().SetTitle("X");
        Assert.Throws<ArgumentNullException>(() => b.AddOption((Action<OnboardingPromptOptionBuilder>)null!));
    }

    [Fact]
    public void PromptBuilder_CarriesEveryFieldThroughBuild()
    {
        var prompt = new OnboardingPromptBuilder()
            .SetId(new Snowflake(50))
            .SetTitle("Pick a tribe")
            .SetType(OnboardingPromptType.Dropdown)
            .SetSingleSelect(true)
            .SetRequired(true)
            .SetInOnboarding(false)
            .AddOption(new OnboardingPromptOptionBuilder().SetTitle("Devs"))
            .Build();

        Assert.Equal(new Snowflake(50), prompt.Id);
        Assert.Equal("Pick a tribe", prompt.Title);
        Assert.Equal(OnboardingPromptType.Dropdown, prompt.Type);
        Assert.True(prompt.SingleSelect);
        Assert.True(prompt.Required);
        Assert.False(prompt.InOnboarding);
        Assert.Single(prompt.Options);
    }
}
