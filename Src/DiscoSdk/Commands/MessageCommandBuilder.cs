using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Fluent builder for message context menu commands (<see cref="ApplicationCommandType.Message"/>).
/// </summary>
public class MessageCommandBuilder()
{
    private string? _name;
    private Dictionary<string, string>? _nameLocalizations;
    private string? _defaultMemberPermissions;
    private bool? _dmPermission;
    private bool? _nsfw;

    /// <summary>
    /// Sets the command name (1-32 characters, mixed case, no spaces).
    /// </summary>
    /// <param name="name">The command name as shown in the Discord context menu.</param>
    /// <returns>The current <see cref="MessageCommandBuilder"/> instance.</returns>
    public MessageCommandBuilder WithName(string name)
    {
        UserCommandBuilder.ValidateContextMenuName(name);
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
    /// <returns>The current <see cref="MessageCommandBuilder"/> instance.</returns>
    public MessageCommandBuilder WithNameLocalizations(Dictionary<string, string> localizations)
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
    /// <returns>The current <see cref="MessageCommandBuilder"/> instance.</returns>
    public MessageCommandBuilder WithDefaultMemberPermissions(string permissions)
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
    /// <returns>The current <see cref="MessageCommandBuilder"/> instance.</returns>
    public MessageCommandBuilder WithDmPermission(bool dmPermission)
    {
        _dmPermission = dmPermission;
        return this;
    }

    /// <summary>
    /// Sets whether the command is age-restricted (NSFW).
    /// </summary>
    /// <param name="nsfw"><c>true</c> if the command is NSFW; otherwise, <c>false</c>.</param>
    /// <returns>The current <see cref="MessageCommandBuilder"/> instance.</returns>
    public MessageCommandBuilder WithNsfw(bool nsfw)
    {
        _nsfw = nsfw;
        return this;
    }

    /// <summary>
    /// Builds the configured <see cref="SlashCommand"/> with <see cref="ApplicationCommandType.Message"/>.
    /// </summary>
    /// <returns>The configured <see cref="SlashCommand"/>.</returns>
    public SlashCommand Build()
    {
        return new SlashCommand
        {
            Type = ApplicationCommandType.Message,
            Name = _name ?? throw new InvalidOperationException("Command name is required."),
            NameLocalizations = _nameLocalizations,
            Description = string.Empty,
            DefaultMemberPermissions = _defaultMemberPermissions,
            DmPermission = _dmPermission,
            Nsfw = _nsfw,
        };
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

            foreach (var ch in value)
            {
                if (char.IsWhiteSpace(ch))
                    throw new ArgumentException(
                        $"Localized name value for locale '{locale}' cannot contain whitespace characters.",
                        nameof(localizations));
            }
        }
    }
}
