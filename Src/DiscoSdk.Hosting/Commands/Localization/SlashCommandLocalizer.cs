using DiscoSdk.Commands.Localization;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Commands.Localization;

/// <summary>
/// Applies an <see cref="ICommandLocalizationProvider"/> to a slash command before it is sent
/// to Discord. Reads the command's translation tree and merges each locale's tree into the
/// command's <c>NameLocalizations</c> / <c>DescriptionLocalizations</c> dictionaries
/// (including recursive options and choices).
/// </summary>
/// <remarks>
/// Internal pipeline detail. Invoked by <c>CommandUpdateAction</c> just before commands are
/// pushed to Discord — manual localizations already set via builder methods (e.g.
/// <c>SlashCommandBuilder.AddNameLocalization(...)</c>) are preserved.
/// </remarks>
internal sealed class SlashCommandLocalizer
{
    private readonly ICommandLocalizationProvider _provider;
    private readonly ILogger? _logger;

    public SlashCommandLocalizer(ICommandLocalizationProvider provider, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Applies translations from the configured provider to <paramref name="command"/>.
    /// Manual localizations already present on the command are preserved.
    /// </summary>
    public void Apply(ApplicationCommand command, Snowflake? guildId = null)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrEmpty(command.Name))
            return;

        var byLocale = _provider.GetLocalizations(command.Name, guildId);
        if (byLocale is null || byLocale.Count == 0)
            return;

        foreach (var (locale, tree) in byLocale)
        {
            if (string.IsNullOrWhiteSpace(locale) || tree is null)
                continue;

            if (!DiscordLocales.Has(locale))
                throw new InvalidOperationException(
                    $"Locale '{locale}' returned by {nameof(ICommandLocalizationProvider)} for command '{command.Name}' is not a supported Discord locale.");

            ApplyCommand(command, locale, tree);
        }
    }

    private void ApplyCommand(ApplicationCommand command, string locale, CommandLocalization tree)
    {
        command.NameLocalizations = SetIfMissing(command.NameLocalizations, locale, tree.Name);
        command.DescriptionLocalizations = SetIfMissing(command.DescriptionLocalizations, locale, tree.Description);

        if (tree.Options is null || tree.Options.Count == 0 || command.Options is null || command.Options.Length == 0)
            return;

        var commandOptionsByName = BuildIndex(command.Options, o => o.Name);

        foreach (var optionLoc in tree.Options)
        {
            if (optionLoc is null || string.IsNullOrEmpty(optionLoc.OptionName))
                continue;

            if (!commandOptionsByName.TryGetValue(optionLoc.OptionName, out var commandOption))
            {
                _logger?.LogWarning(
                    "Localization provider returned translation for option '{Option}' on command '{Command}' but no such option exists.",
                    optionLoc.OptionName, command.Name);
                continue;
            }

            ApplyOption(commandOption, locale, optionLoc, command.Name, optionLoc.OptionName);
        }
    }

    private void ApplyOption(
        SlashCommandOption commandOption,
        string locale,
        OptionLocalization tree,
        string commandName,
        string optionPath)
    {
        commandOption.NameLocalizations = SetIfMissing(commandOption.NameLocalizations, locale, tree.Name);
        commandOption.DescriptionLocalizations = SetIfMissing(commandOption.DescriptionLocalizations, locale, tree.Description);

        if (tree.Options is { Count: > 0 } nestedTree && commandOption.Options is { Length: > 0 } nestedCmd)
        {
            var nestedByName = BuildIndex(nestedCmd, o => o.Name);
            foreach (var nested in nestedTree)
            {
                if (nested is null || string.IsNullOrEmpty(nested.OptionName))
                    continue;

                if (!nestedByName.TryGetValue(nested.OptionName, out var nestedCommandOption))
                {
                    _logger?.LogWarning(
                        "Localization provider returned translation for option '{Path}' on command '{Command}' but no such option exists.",
                        $"{optionPath}.{nested.OptionName}", commandName);
                    continue;
                }

                ApplyOption(nestedCommandOption, locale, nested, commandName, $"{optionPath}.{nested.OptionName}");
            }
        }

        if (tree.Choices is { Count: > 0 } choiceTree && commandOption.Choices is { Length: > 0 } choiceCmd)
        {
            var choicesByName = BuildIndex(choiceCmd, c => c.Name);
            foreach (var choiceLoc in choiceTree)
            {
                if (choiceLoc is null || string.IsNullOrEmpty(choiceLoc.ChoiceName))
                    continue;

                if (!choicesByName.TryGetValue(choiceLoc.ChoiceName, out var commandChoice))
                {
                    _logger?.LogWarning(
                        "Localization provider returned translation for choice '{Choice}' on option '{Path}' of command '{Command}' but no such choice exists.",
                        choiceLoc.ChoiceName, optionPath, commandName);
                    continue;
                }

                commandChoice.NameLocalizations = SetIfMissing(commandChoice.NameLocalizations, locale, choiceLoc.Name);
            }
        }
    }

    private static Dictionary<string, T> BuildIndex<T>(IEnumerable<T> source, Func<T, string> keySelector)
    {
        var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!string.IsNullOrEmpty(key))
                dict[key] = item;
        }
        return dict;
    }

    private static Dictionary<string, string>? SetIfMissing(Dictionary<string, string>? current, string locale, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return current;

        current ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!current.ContainsKey(locale))
            current[locale] = value.Trim();

        return current;
    }
}
