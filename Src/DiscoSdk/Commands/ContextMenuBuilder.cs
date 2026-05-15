using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Fluent builder for context menu commands (User and Message). The variant is chosen at
/// <see cref="Build(ContextMenuType)"/> time — the builder shape is identical for both, so
/// one type covers both Discord <c>ApplicationCommandType.User</c> and
/// <c>ApplicationCommandType.Message</c>.
/// </summary>
public class ContextMenuBuilder()
{
    private string? _name;
    private Dictionary<string, string>? _nameLocalizations;
    private string? _defaultMemberPermissions;
    private bool? _dmPermission;
    private bool? _nsfw;

    /// <summary>
    /// Sets the command name (1-32 characters). Mixed case and spaces are allowed, matching
    /// Discord's rules for USER and MESSAGE commands.
    /// </summary>
    /// <param name="name">The command name as shown in the Discord context menu.</param>
    /// <returns>The current <see cref="ContextMenuBuilder"/> instance.</returns>
    public ContextMenuBuilder WithName(string name)
    {
        ValidateContextMenuName(name);
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets localized names for the command.
    /// </summary>
    /// <param name="localizations">
    /// A dictionary mapping locale codes (for example, <c>"pt-BR"</c>, <c>"en-US"</c>)
    /// to the localized command name.
    /// </param>
    /// <returns>The current <see cref="ContextMenuBuilder"/> instance.</returns>
    public ContextMenuBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        ValidateNameLocalizations(localizations);
        _nameLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Sets the default member permissions required to use the command.
    /// </summary>
    /// <param name="permissions">
    /// A permissions bitfield represented as a string, matching Discord's API format.
    /// </param>
    /// <returns>The current <see cref="ContextMenuBuilder"/> instance.</returns>
    public ContextMenuBuilder WithDefaultMemberPermissions(string permissions)
    {
        _defaultMemberPermissions = permissions;
        return this;
    }

    /// <summary>
    /// Sets whether the command can be used in direct messages.
    /// </summary>
    /// <param name="dmPermission">
    /// <c>true</c> to allow the command in DMs; <c>false</c> to restrict it to guilds.
    /// </param>
    /// <returns>The current <see cref="ContextMenuBuilder"/> instance.</returns>
    public ContextMenuBuilder WithDmPermission(bool dmPermission)
    {
        _dmPermission = dmPermission;
        return this;
    }

    /// <summary>
    /// Sets whether the command is age-restricted (NSFW).
    /// </summary>
    /// <param name="nsfw"><c>true</c> if the command is NSFW; otherwise, <c>false</c>.</param>
    /// <returns>The current <see cref="ContextMenuBuilder"/> instance.</returns>
    public ContextMenuBuilder WithNsfw(bool nsfw)
    {
        _nsfw = nsfw;
        return this;
    }

    /// <summary>
    /// Builds the configured <see cref="ApplicationCommand"/> with the chosen
    /// <paramref name="type"/> (<see cref="ApplicationCommandType.User"/> or
    /// <see cref="ApplicationCommandType.Message"/>).
    /// </summary>
    public ApplicationCommand Build(ContextMenuType type)
    {
        return new ApplicationCommand
        {
            Type = type switch
            {
                ContextMenuType.User => ApplicationCommandType.User,
                ContextMenuType.Message => ApplicationCommandType.Message,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown ContextMenuType."),
            },
            Name = _name ?? throw new InvalidOperationException("Command name is required."),
            NameLocalizations = _nameLocalizations,
            Description = string.Empty,
            DefaultMemberPermissions = _defaultMemberPermissions,
            DmPermission = _dmPermission,
            Nsfw = _nsfw,
        };
    }

    internal static void ValidateContextMenuName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null, empty, or contain only whitespace.", nameof(name));

        if (name.Length is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(name), $"Command name must be between 1 and 32 characters. Current length: {name.Length}.");
    }

    private static void ValidateNameLocalizations(Dictionary<string, string>? localizations)
    {
        if (localizations == null)
            return;

        foreach (var (locale, value) in localizations)
        {
            if (string.IsNullOrWhiteSpace(locale))
                throw new ArgumentException("Locale cannot be null or empty.", nameof(localizations));

            if (!DiscordLocales.Has(locale))
                throw new ArgumentException(
                    $"Locale '{locale}' is not supported by Discord.",
                    nameof(localizations));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    $"Localization value for locale '{locale}' cannot be null, empty, or contain only whitespace.",
                    nameof(localizations));

            if (value.Length is < 1 or > 32)
                throw new ArgumentOutOfRangeException(nameof(localizations),
                    $"Localized name value for locale '{locale}' must be between 1 and 32 characters. Current length: {value.Length}.");
        }
    }
}
