using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models;

/// <summary>
/// Fluent builder for one <see cref="OnboardingPrompt"/>. Existing prompts round-trip their
/// <see cref="OnboardingPrompt.Id"/>; fresh prompts leave it at <c>default</c> and Discord
/// assigns one on PUT. Each prompt must end up with at least one option built via
/// <see cref="AddOption"/>.
/// </summary>
public class OnboardingPromptBuilder
{
    private Snowflake _id;
    private OnboardingPromptType _type = OnboardingPromptType.MultipleChoice;
    private string _title = string.Empty;
    private bool _singleSelect;
    private bool _required;
    private bool _inOnboarding = true;
    private readonly List<OnboardingPromptOption> _options = [];

    /// <summary>Round-trips an existing prompt's id so Discord can resolve it during the PUT replace.</summary>
    public OnboardingPromptBuilder SetId(Snowflake id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the prompt type (multiple-choice or dropdown).</summary>
    public OnboardingPromptBuilder SetType(OnboardingPromptType type)
    {
        _type = type;
        return this;
    }

    /// <summary>Sets the prompt title (1-100 chars).</summary>
    public OnboardingPromptBuilder SetTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (title.Length is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(title), "Prompt title must be 1-100 characters.");
        _title = title;
        return this;
    }

    /// <summary>Sets whether members can only select one option for the prompt.</summary>
    public OnboardingPromptBuilder SetSingleSelect(bool singleSelect)
    {
        _singleSelect = singleSelect;
        return this;
    }

    /// <summary>Sets whether the prompt is required before a member completes the onboarding flow.</summary>
    public OnboardingPromptBuilder SetRequired(bool required)
    {
        _required = required;
        return this;
    }

    /// <summary>
    /// Sets whether the prompt appears in the onboarding flow. When false, it only appears in
    /// Channels &amp; Roles for existing members.
    /// </summary>
    public OnboardingPromptBuilder SetInOnboarding(bool inOnboarding)
    {
        _inOnboarding = inOnboarding;
        return this;
    }

    /// <summary>Appends a prebuilt option to the prompt. Maximum 50 options per prompt.</summary>
    public OnboardingPromptBuilder AddOption(OnboardingPromptOption option)
    {
        ArgumentNullException.ThrowIfNull(option);
        if (_options.Count >= 50)
            throw new InvalidOperationException("Prompt cannot have more than 50 options.");
        _options.Add(option);
        return this;
    }

    /// <summary>Appends an option via its builder. Maximum 50 options per prompt.</summary>
    public OnboardingPromptBuilder AddOption(OnboardingPromptOptionBuilder option)
    {
        ArgumentNullException.ThrowIfNull(option);
        return AddOption(option.Build());
    }

    /// <summary>
    /// Appends an option configured inline. The callback receives a fresh
    /// <see cref="OnboardingPromptOptionBuilder"/>; <c>Build</c> is called automatically.
    /// </summary>
    public OnboardingPromptBuilder AddOption(Action<OnboardingPromptOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var b = new OnboardingPromptOptionBuilder();
        configure(b);
        return AddOption(b);
    }

    /// <summary>Materializes the wire-shape prompt.</summary>
    public OnboardingPrompt Build()
    {
        if (string.IsNullOrWhiteSpace(_title))
            throw new InvalidOperationException("Prompt requires SetTitle before Build.");
        if (_options.Count == 0)
            throw new InvalidOperationException("Prompt requires at least one AddOption before Build.");

        return new OnboardingPrompt
        {
            Id = _id,
            Type = _type,
            Options = [.. _options],
            Title = _title,
            SingleSelect = _singleSelect,
            Required = _required,
            InOnboarding = _inOnboarding,
        };
    }
}
