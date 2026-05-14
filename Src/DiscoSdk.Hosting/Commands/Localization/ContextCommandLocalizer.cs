using DiscoSdk.Commands.Localization;
using DiscoSdk.Models;
using DiscoSdk.Models.Commands;
using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Commands.Localization;

/// <summary>
/// Applies an <see cref="IContextCommandLocalizationProvider"/> to a context menu command
/// (User / Message) before it is sent to Discord. Context commands only carry
/// <c>name_localizations</c> on the wire — no description, no options — so this localizer
/// only touches <see cref="SlashCommand.NameLocalizations"/>.
/// </summary>
/// <remarks>
/// Internal pipeline detail. Invoked by <c>CommandUpdateAction</c> for commands whose
/// <see cref="SlashCommand.Type"/> is <c>User</c> or <c>Message</c>; slash commands go
/// through <see cref="SlashCommandLocalizer"/> instead.
/// </remarks>
internal sealed class ContextCommandLocalizer
{
    private readonly IContextCommandLocalizationProvider _provider;
    private readonly ILogger? _logger;

    public ContextCommandLocalizer(IContextCommandLocalizationProvider provider, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Applies translations from the configured provider to <paramref name="command"/>.
    /// Manual localizations already present on the command are preserved.
    /// </summary>
    public void Apply(SlashCommand command, Snowflake? guildId = null)
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
                    $"Locale '{locale}' returned by {nameof(IContextCommandLocalizationProvider)} for command '{command.Name}' is not a supported Discord locale.");

            command.NameLocalizations = SetIfMissing(command.NameLocalizations, locale, tree.Name);

            if (!string.IsNullOrEmpty(tree.Description) || (tree.Options is { Count: > 0 }))
            {
                _logger?.LogWarning(
                    "Context translation for command '{Command}' (locale '{Locale}') contains description/options — ignored. Context commands only support name_localizations.",
                    command.Name, locale);
            }
        }
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
