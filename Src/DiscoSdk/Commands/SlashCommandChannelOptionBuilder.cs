using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for a <see cref="SlashCommandOptionType.Channel"/> option. Adds channel-specific
/// <c>WithChannelTypes</c> filter.
/// </summary>
public sealed class SlashCommandChannelOptionBuilder : SlashCommandOptionNode
{
    internal SlashCommandOption Option { get; }

    internal SlashCommandChannelOptionBuilder(SlashCommandBuilder root, SlashCommandOption option, ISlashCommandOptionContainer container)
        : base(root, container)
    {
        Option = option;
    }

    /// <summary>Marks the option as required (default <c>true</c>).</summary>
    public SlashCommandChannelOptionBuilder WithRequired(bool required = true)
    {
        Option.Required = required;
        return this;
    }

    /// <summary>Restricts the channel picker to the supplied channel types.</summary>
    public SlashCommandChannelOptionBuilder WithChannelTypes(params ChannelType[] channelTypes)
    {
        Option.ChannelTypes = channelTypes is { Length: > 0 } ? channelTypes : null;
        return this;
    }

    /// <summary>Sets localized names for the option.</summary>
    public SlashCommandChannelOptionBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "name");
        Option.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Sets localized descriptions for the option.</summary>
    public SlashCommandChannelOptionBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "description");
        Option.DescriptionLocalizations = localizations;
        return this;
    }
}
