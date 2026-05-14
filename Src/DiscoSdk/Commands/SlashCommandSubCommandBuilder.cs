using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for a <see cref="SlashCommandOptionType.SubCommand"/>. Adds leaf-type options
/// (parameters) via the <c>ThenXxxOption</c> family. EF-style: <c>ThenStringOption(name)</c> descends
/// into the new option (focus shifts to it, but subsequent <c>ThenXxxOption</c> calls on the leaf
/// also add siblings inside this sub-command — leaves can't have children, so "Then" on them means
/// "sibling at the parent scope"); the callback overload keeps focus on the sub-command.
/// </summary>
public sealed class SlashCommandSubCommandBuilder : SlashCommandFluentNode, ISlashCommandOptionContainer
{
    internal SlashCommandOption Option { get; }

    internal SlashCommandSubCommandBuilder(SlashCommandBuilder root, SlashCommandOption option)
        : base(root)
    {
        Option = option;
    }

    void ISlashCommandOptionContainer.AddOption(SlashCommandOption option) => AddChild(option);

    /// <summary>Sets localized names for the sub-command.</summary>
    public SlashCommandSubCommandBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "name");
        Option.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Sets localized descriptions for the sub-command.</summary>
    public SlashCommandSubCommandBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "description");
        Option.DescriptionLocalizations = localizations;
        return this;
    }

    /// <summary>Adds a <see cref="SlashCommandOptionType.String"/> option to this sub-command.</summary>
    public SlashCommandStringOptionBuilder ThenStringOption(string name, string description, bool required = false)
    {
        var option = SlashCommandOptionNode.CreateLeafOption(name, description, SlashCommandOptionType.String, required);
        AddChild(option);
        return new SlashCommandStringOptionBuilder(Root, option, this);
    }

    /// <summary>Callback overload of <see cref="ThenStringOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenStringOption(string name, string description, Action<SlashCommandStringOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenStringOption(name, description));
        return this;
    }

    /// <summary>Adds a <see cref="SlashCommandOptionType.Integer"/> option to this sub-command.</summary>
    public SlashCommandIntegerOptionBuilder ThenIntegerOption(string name, string description, bool required = false)
    {
        var option = SlashCommandOptionNode.CreateLeafOption(name, description, SlashCommandOptionType.Integer, required);
        AddChild(option);
        return new SlashCommandIntegerOptionBuilder(Root, option, this);
    }

    /// <summary>Callback overload of <see cref="ThenIntegerOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenIntegerOption(string name, string description, Action<SlashCommandIntegerOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenIntegerOption(name, description));
        return this;
    }

    /// <summary>Adds a <see cref="SlashCommandOptionType.Number"/> option to this sub-command.</summary>
    public SlashCommandNumberOptionBuilder ThenNumberOption(string name, string description, bool required = false)
    {
        var option = SlashCommandOptionNode.CreateLeafOption(name, description, SlashCommandOptionType.Number, required);
        AddChild(option);
        return new SlashCommandNumberOptionBuilder(Root, option, this);
    }

    /// <summary>Callback overload of <see cref="ThenNumberOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenNumberOption(string name, string description, Action<SlashCommandNumberOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenNumberOption(name, description));
        return this;
    }

    /// <summary>Adds a <see cref="SlashCommandOptionType.Boolean"/> option to this sub-command.</summary>
    public SlashCommandLeafOptionBuilder ThenBooleanOption(string name, string description, bool required = false)
        => AddLeaf(name, description, SlashCommandOptionType.Boolean, required);

    /// <summary>Callback overload of <see cref="ThenBooleanOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenBooleanOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenBooleanOption(name, description), configure);

    /// <summary>Adds a <see cref="SlashCommandOptionType.User"/> option to this sub-command.</summary>
    public SlashCommandLeafOptionBuilder ThenUserOption(string name, string description, bool required = false)
        => AddLeaf(name, description, SlashCommandOptionType.User, required);

    /// <summary>Callback overload of <see cref="ThenUserOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenUserOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenUserOption(name, description), configure);

    /// <summary>Adds a <see cref="SlashCommandOptionType.Role"/> option to this sub-command.</summary>
    public SlashCommandLeafOptionBuilder ThenRoleOption(string name, string description, bool required = false)
        => AddLeaf(name, description, SlashCommandOptionType.Role, required);

    /// <summary>Callback overload of <see cref="ThenRoleOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenRoleOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenRoleOption(name, description), configure);

    /// <summary>Adds a <see cref="SlashCommandOptionType.Channel"/> option to this sub-command.</summary>
    public SlashCommandChannelOptionBuilder ThenChannelOption(string name, string description, bool required = false)
    {
        var option = SlashCommandOptionNode.CreateLeafOption(name, description, SlashCommandOptionType.Channel, required);
        AddChild(option);
        return new SlashCommandChannelOptionBuilder(Root, option, this);
    }

    /// <summary>Callback overload of <see cref="ThenChannelOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenChannelOption(string name, string description, Action<SlashCommandChannelOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(ThenChannelOption(name, description));
        return this;
    }

    /// <summary>Adds a <see cref="SlashCommandOptionType.Mentionable"/> option to this sub-command.</summary>
    public SlashCommandLeafOptionBuilder ThenMentionableOption(string name, string description, bool required = false)
        => AddLeaf(name, description, SlashCommandOptionType.Mentionable, required);

    /// <summary>Callback overload of <see cref="ThenMentionableOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenMentionableOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenMentionableOption(name, description), configure);

    /// <summary>Adds a <see cref="SlashCommandOptionType.Attachment"/> option to this sub-command.</summary>
    public SlashCommandLeafOptionBuilder ThenAttachmentOption(string name, string description, bool required = false)
        => AddLeaf(name, description, SlashCommandOptionType.Attachment, required);

    /// <summary>Callback overload of <see cref="ThenAttachmentOption(string, string, bool)"/>.</summary>
    public SlashCommandSubCommandBuilder ThenAttachmentOption(string name, string description, Action<SlashCommandLeafOptionBuilder> configure)
        => RunLeafCallback(ThenAttachmentOption(name, description), configure);

    private SlashCommandLeafOptionBuilder AddLeaf(string name, string description, SlashCommandOptionType type, bool required)
    {
        var option = SlashCommandOptionNode.CreateLeafOption(name, description, type, required);
        AddChild(option);
        return new SlashCommandLeafOptionBuilder(Root, option, this);
    }

    private SlashCommandSubCommandBuilder RunLeafCallback(SlashCommandLeafOptionBuilder leaf, Action<SlashCommandLeafOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(leaf);
        return this;
    }

    private void AddChild(SlashCommandOption option)
    {
        var current = Option.Options;
        var next = current is null
            ? [option]
            : new SlashCommandOption[current.Length + 1];
        if (current is not null)
        {
            Array.Copy(current, next, current.Length);
            next[^1] = option;
        }
        if (next.Length > 25)
            throw new InvalidOperationException("A sub-command can have a maximum of 25 options. Discord API limit.");

        Option.Options = next;
    }
}
