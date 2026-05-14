namespace DiscoSdk.Commands.Localization;

/// <summary>
/// Fluent builder for the per-locale translation tree of a single application command. Used by
/// <see cref="InMemoryCommandLocalizationProvider.DefineCommand"/> /
/// <see cref="InMemoryCommandLocalizationProvider.DefineCommandForGuild"/>.
/// Validates locale strings against <see cref="DiscordLocales"/> at <c>ForLocale</c> time;
/// builds the final <c>(locale → CommandLocalization)</c> dictionary on <see cref="Build"/>.
/// </summary>
public sealed class CommandLocalizationBuilder
{
    private readonly Dictionary<string, LocaleTranslationBuilder> _byLocale = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds (or replaces) the translation tree for <paramref name="locale"/>. Locale must be a
    /// valid Discord locale code (e.g. <c>"pt-BR"</c>, <c>"en-US"</c>) — see <see cref="DiscordLocales"/>.
    /// </summary>
    public CommandLocalizationBuilder ForLocale(string locale, Action<LocaleTranslationBuilder> configure)
    {
        if (string.IsNullOrWhiteSpace(locale))
            throw new ArgumentException("Locale cannot be null or empty.", nameof(locale));
        if (!DiscordLocales.Has(locale))
            throw new ArgumentException($"'{locale}' is not a valid Discord locale. See DiscordLocales.GetAll().", nameof(locale));
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new LocaleTranslationBuilder();
        configure(builder);
        _byLocale[locale] = builder;
        return this;
    }

    /// <summary>Produces the final <c>locale → tree</c> dictionary the provider stores.</summary>
    internal IReadOnlyDictionary<string, CommandLocalization> Build()
        => _byLocale.ToDictionary(kv => kv.Key, kv => kv.Value.Build(), StringComparer.OrdinalIgnoreCase);
}
