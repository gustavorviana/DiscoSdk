using DiscoSdk.Models.Commands;

namespace DiscoSdk.Commands;

/// <summary>
/// Focused builder for leaf option types that take no children and no constraints:
/// <see cref="DiscoSdk.Models.Enums.SlashCommandOptionType.Boolean"/>,
/// <see cref="DiscoSdk.Models.Enums.SlashCommandOptionType.User"/>,
/// <see cref="DiscoSdk.Models.Enums.SlashCommandOptionType.Role"/>,
/// <see cref="DiscoSdk.Models.Enums.SlashCommandOptionType.Mentionable"/>,
/// <see cref="DiscoSdk.Models.Enums.SlashCommandOptionType.Attachment"/>.
/// Returned by the corresponding entry points on <see cref="SlashCommandBuilder"/>,
/// <see cref="SlashCommandSubCommandBuilder"/>, and any other focused builder via the
/// shared <see cref="SlashCommandFluentNode"/> reset-to-root forwarders.
/// </summary>
public sealed class SlashCommandLeafOptionBuilder : SlashCommandOptionNode
{
    internal SlashCommandOption Option { get; }

    internal SlashCommandLeafOptionBuilder(SlashCommandBuilder root, SlashCommandOption option, ISlashCommandOptionContainer container)
        : base(root, container)
    {
        Option = option;
    }

    /// <summary>Marks the option as required (default <c>true</c>).</summary>
    public SlashCommandLeafOptionBuilder WithRequired(bool required = true)
    {
        Option.Required = required;
        return this;
    }

    /// <summary>Sets localized names for the option.</summary>
    public SlashCommandLeafOptionBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "name");
        Option.NameLocalizations = localizations;
        return this;
    }

    /// <summary>Sets localized descriptions for the option.</summary>
    public SlashCommandLeafOptionBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        SlashCommandBuilder.ValidateLocalizations(localizations, "description");
        Option.DescriptionLocalizations = localizations;
        return this;
    }

    /// <summary>Adds or replaces a single localized name entry.</summary>
    public SlashCommandLeafOptionBuilder AddNameLocalization(string locale, string name)
    {
        SlashCommandBuilder.EnsureLocale(locale);
        Option.NameLocalizations ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Option.NameLocalizations[locale] = name;
        SlashCommandBuilder.ValidateLocalizations(Option.NameLocalizations, "name");
        return this;
    }

    /// <summary>Adds or replaces a single localized description entry.</summary>
    public SlashCommandLeafOptionBuilder AddDescriptionLocalization(string locale, string description)
    {
        SlashCommandBuilder.EnsureLocale(locale);
        Option.DescriptionLocalizations ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Option.DescriptionLocalizations[locale] = description;
        SlashCommandBuilder.ValidateLocalizations(Option.DescriptionLocalizations, "description");
        return this;
    }
}
