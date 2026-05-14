using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Intermediate base for focused option builders (leaves, string, integer, number, channel).
/// Adds a shared <c>AddSibling</c> appender that points to the collection where this option's
/// siblings live (root command options, or a parent sub-command's options) and re-exposes the
/// <c>ThenXxxOption</c> family — calling them adds a sibling at the same scope as the focused
/// option and shifts focus to the new option.
/// <para>
/// Distinct from <see cref="SlashCommandFluentNode"/>'s <c>StringOption</c>/<c>SubCommand</c>/etc.
/// forwarders, which always reset to the command root (EF <c>.Include</c> after <c>.ThenInclude</c>).
/// </para>
/// </summary>
public abstract class SlashCommandOptionNode : SlashCommandFluentNode
{
    internal ISlashCommandOptionContainer Container { get; }

    private protected SlashCommandOptionNode(SlashCommandBuilder root, ISlashCommandOptionContainer container)
        : base(root)
    {
        Container = container;
    }

    /// <summary>Adds a sibling string option at the same scope and returns it focused.</summary>
    public SlashCommandStringOptionBuilder ThenStringOption(string name, string description, bool required = false)
        => AddTypedSibling<SlashCommandStringOptionBuilder>(name, description, SlashCommandOptionType.String, required,
            (root, opt) => new SlashCommandStringOptionBuilder(root, opt, Container));

    /// <summary>Callback overload of <see cref="ThenStringOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenStringOption(string name, string description, Action<SlashCommandStringOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenStringOption(name, description));
        return this;
    }

    /// <summary>Adds a sibling integer option at the same scope and returns it focused.</summary>
    public SlashCommandIntegerOptionBuilder ThenIntegerOption(string name, string description, bool required = false)
        => AddTypedSibling<SlashCommandIntegerOptionBuilder>(name, description, SlashCommandOptionType.Integer, required,
            (root, opt) => new SlashCommandIntegerOptionBuilder(root, opt, Container));

    /// <summary>Callback overload of <see cref="ThenIntegerOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenIntegerOption(string name, string description, Action<SlashCommandIntegerOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenIntegerOption(name, description));
        return this;
    }

    /// <summary>Adds a sibling number option at the same scope and returns it focused.</summary>
    public SlashCommandNumberOptionBuilder ThenNumberOption(string name, string description, bool required = false)
        => AddTypedSibling<SlashCommandNumberOptionBuilder>(name, description, SlashCommandOptionType.Number, required,
            (root, opt) => new SlashCommandNumberOptionBuilder(root, opt, Container));

    /// <summary>Callback overload of <see cref="ThenNumberOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenNumberOption(string name, string description, Action<SlashCommandNumberOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenNumberOption(name, description));
        return this;
    }

    /// <summary>Adds a sibling boolean option at the same scope and returns it focused.</summary>
    public SlashCommandLeafOptionBuilder ThenBooleanOption(string name, string description, bool required = false)
        => AddLeafSibling(name, description, SlashCommandOptionType.Boolean, required);

    /// <summary>Callback overload of <see cref="ThenBooleanOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenBooleanOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenBooleanOption(name, description), configure);

    /// <summary>Adds a sibling user option at the same scope and returns it focused.</summary>
    public SlashCommandLeafOptionBuilder ThenUserOption(string name, string description, bool required = false)
        => AddLeafSibling(name, description, SlashCommandOptionType.User, required);

    /// <summary>Callback overload of <see cref="ThenUserOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenUserOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenUserOption(name, description), configure);

    /// <summary>Adds a sibling role option at the same scope and returns it focused.</summary>
    public SlashCommandLeafOptionBuilder ThenRoleOption(string name, string description, bool required = false)
        => AddLeafSibling(name, description, SlashCommandOptionType.Role, required);

    /// <summary>Callback overload of <see cref="ThenRoleOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenRoleOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenRoleOption(name, description), configure);

    /// <summary>Adds a sibling channel option at the same scope and returns it focused.</summary>
    public SlashCommandChannelOptionBuilder ThenChannelOption(string name, string description, bool required = false)
        => AddTypedSibling<SlashCommandChannelOptionBuilder>(name, description, SlashCommandOptionType.Channel, required,
            (root, opt) => new SlashCommandChannelOptionBuilder(root, opt, Container));

    /// <summary>Callback overload of <see cref="ThenChannelOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenChannelOption(string name, string description, Action<SlashCommandChannelOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenChannelOption(name, description));
        return this;
    }

    /// <summary>Adds a sibling mentionable option at the same scope and returns it focused.</summary>
    public SlashCommandLeafOptionBuilder ThenMentionableOption(string name, string description, bool required = false)
        => AddLeafSibling(name, description, SlashCommandOptionType.Mentionable, required);

    /// <summary>Callback overload of <see cref="ThenMentionableOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenMentionableOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenMentionableOption(name, description), configure);

    /// <summary>Adds a sibling attachment option at the same scope and returns it focused.</summary>
    public SlashCommandLeafOptionBuilder ThenAttachmentOption(string name, string description, bool required = false)
        => AddLeafSibling(name, description, SlashCommandOptionType.Attachment, required);

    /// <summary>Callback overload of <see cref="ThenAttachmentOption(string, string, bool)"/>.</summary>
    public SlashCommandOptionNode ThenAttachmentOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenAttachmentOption(name, description), configure);

    private SlashCommandLeafOptionBuilder AddLeafSibling(string name, string description, SlashCommandOptionType type, bool required)
    {
        var option = CreateLeafOption(name, description, type, required);
        Container.AddOption(option);
        return new SlashCommandLeafOptionBuilder(Root, option, Container);
    }

    private SlashCommandOptionNode RunLeafCallback(SlashCommandLeafOptionBuilder leaf, Action<SlashCommandLeafOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(leaf);
        return this;
    }

    private TBuilder AddTypedSibling<TBuilder>(
        string name,
        string description,
        SlashCommandOptionType type,
        bool required,
        Func<SlashCommandBuilder, SlashCommandOption, TBuilder> factory)
    {
        var option = CreateLeafOption(name, description, type, required);
        Container.AddOption(option);
        return factory(Root, option);
    }

    internal static SlashCommandOption CreateLeafOption(string name, string description, SlashCommandOptionType type, bool required)
    {
        SlashCommandBuilder.ValidateOptionName(name);
        SlashCommandBuilder.ValidateOptionDescription(description);

        return new SlashCommandOption
        {
            Name = name.ToLowerInvariant(),
            Description = description,
            Type = type,
            Required = required,
        };
    }
}
