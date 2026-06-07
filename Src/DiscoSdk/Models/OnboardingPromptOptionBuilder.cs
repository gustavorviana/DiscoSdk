namespace DiscoSdk.Models;

/// <summary>
/// Fluent builder for one <see cref="OnboardingPromptOption"/>. Choosing this option opts the
/// member into the listed channels and/or roles. Existing options round-trip their
/// <see cref="OnboardingPromptOption.Id"/>; freshly built options leave the id at <c>default</c>
/// and Discord assigns one on PUT.
/// </summary>
public class OnboardingPromptOptionBuilder
{
    private Snowflake _id;
    private readonly List<Snowflake> _channelIds = [];
    private readonly List<Snowflake> _roleIds = [];
    private string _title = string.Empty;
    private string? _description;
    private Snowflake? _emojiId;
    private string? _emojiName;
    private bool? _emojiAnimated;

    /// <summary>
    /// Round-trips an existing option's id so Discord can resolve it during the PUT replace.
    /// New options leave this unset.
    /// </summary>
    public OnboardingPromptOptionBuilder SetId(Snowflake id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the option title (1-50 chars).</summary>
    public OnboardingPromptOptionBuilder SetTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (title.Length is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(title), "Option title must be 1-50 characters.");
        _title = title;
        return this;
    }

    /// <summary>Sets the option description (0-100 chars). Pass <c>null</c> to clear.</summary>
    public OnboardingPromptOptionBuilder SetDescription(string? description)
    {
        if (description is { Length: > 100 })
            throw new ArgumentOutOfRangeException(nameof(description), "Option description cannot exceed 100 characters.");
        _description = description;
        return this;
    }

    /// <summary>Adds a channel the member is opted into when selecting this option.</summary>
    public OnboardingPromptOptionBuilder AddChannel(Snowflake channelId)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));
        _channelIds.Add(channelId);
        return this;
    }

    /// <summary>Adds a role assigned to the member when selecting this option.</summary>
    public OnboardingPromptOptionBuilder AddRole(Snowflake roleId)
    {
        if (roleId == default)
            throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));
        _roleIds.Add(roleId);
        return this;
    }

    /// <summary>Sets the option's emoji using a custom emoji id (and optional animated flag).</summary>
    public OnboardingPromptOptionBuilder SetCustomEmoji(Snowflake emojiId, bool animated = false)
    {
        if (emojiId == default)
            throw new ArgumentException("Emoji ID cannot be null or empty.", nameof(emojiId));
        _emojiId = emojiId;
        _emojiName = null;
        _emojiAnimated = animated;
        return this;
    }

    /// <summary>Sets the option's emoji using a unicode emoji literal (e.g. "🎉").</summary>
    public OnboardingPromptOptionBuilder SetUnicodeEmoji(string emojiCodepoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emojiCodepoint);
        _emojiName = emojiCodepoint;
        _emojiId = null;
        _emojiAnimated = null;
        return this;
    }

    /// <summary>Materializes the wire-shape option.</summary>
    public OnboardingPromptOption Build()
    {
        if (string.IsNullOrWhiteSpace(_title))
            throw new InvalidOperationException("Option requires SetTitle before Build.");

        return new OnboardingPromptOption
        {
            Id = _id,
            ChannelIds = [.. _channelIds],
            RoleIds = [.. _roleIds],
            Title = _title,
            Description = _description,
            EmojiId = _emojiId,
            EmojiName = _emojiName,
            EmojiAnimated = _emojiAnimated,
        };
    }
}
