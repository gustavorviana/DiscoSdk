using DiscoSdk.Models.Builders;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Builders;

/// <summary>
/// Fluent builder for <see cref="ApplicationCommand"/>.
/// </summary>
public class ApplicationCommandBuilder() : IApplicationCommandBuilder
{
    private ApplicationCommandType? _type;
    private string? _name;
    private Dictionary<string, string>? _nameLocalizations;
    private string? _description;
    private Dictionary<string, string>? _descriptionLocalizations;
    private readonly List<ApplicationCommandOption> _options = [];
    private string? _defaultMemberPermissions;
    private bool? _dmPermission;
    private bool? _nsfw;
    private string? _version;

    /// <summary>
    /// Sets the command name (1-32 characters, lowercase).
    /// </summary>
    /// <param name="name">The command name as shown in the Discord client.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithName(string name)
    {
        ValidateCommandName(name);
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the command description (1-100 characters).
    /// </summary>
    /// <param name="description">The description shown in the Discord client.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithDescription(string description)
    {
        ValidateCommandDescription(description);
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the command type.
    /// </summary>
    /// <param name="type">The <see cref="ApplicationCommandType"/> for this command.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithType(ApplicationCommandType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets localized names for the command.
    /// </summary>
    /// <param name="localizations">
    /// A dictionary mapping locale codes (for example, <c>\"pt-BR\"</c>, <c>\"en-US\"</c>)
    /// to the localized command name.
    /// </param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        _nameLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Sets localized descriptions for the command.
    /// </summary>
    /// <param name="localizations">
    /// A dictionary mapping locale codes (for example, <c>\"pt-BR\"</c>, <c>\"en-US\"</c>)
    /// to the localized command description.
    /// </param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        _descriptionLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Sets the default member permissions required to use the command.
    /// </summary>
    /// <param name="permissions">
    /// A permissions bitfield represented as a string, matching Discord's API format.
    /// </param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithDefaultMemberPermissions(string permissions)
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
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithDmPermission(bool dmPermission)
    {
        _dmPermission = dmPermission;
        return this;
    }

    /// <summary>
    /// Sets whether the command is age-restricted (NSFW).
    /// </summary>
    /// <param name="nsfw"><c>true</c> if the command is NSFW; otherwise, <c>false</c>.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithNsfw(bool nsfw)
    {
        _nsfw = nsfw;
        return this;
    }

    /// <summary>
    /// Sets the version identifier for the command.
    /// </summary>
    /// <param name="version">
    /// The version string as provided by Discord. Normally set by the API response.
    /// </param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Adds an option to the command.
    /// </summary>
    /// <param name="option">The option instance to add.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    private IApplicationCommandBuilder AddOption(ApplicationCommandOption option)
    {
        _options.Add(option);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.SubCommand"/> option.
    /// </summary>
    /// <param name="name">The subcommand name.</param>
    /// <param name="description">The subcommand description.</param>
    /// <param name="options">Nested options (parameters) for this subcommand.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddSubCommandOption(
        string name,
        string description,
        params ApplicationCommandOption[] options)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.SubCommand,
            Options = options,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.SubCommandGroup"/> option.
    /// </summary>
    /// <param name="name">The subcommand group name.</param>
    /// <param name="description">The subcommand group description.</param>
    /// <param name="options">Subcommands that belong to this group.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddSubCommandGroupOption(
        string name,
        string description,
        params ApplicationCommandOption[] options)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.SubCommandGroup,
            Options = options,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.String"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <param name="minLength">Minimum length for the string value.</param>
    /// <param name="maxLength">Maximum length for the string value.</param>
    /// <param name="autocomplete">Whether autocomplete is enabled for this option.</param>
    /// <param name="choices">Static choices available for this option.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddStringOption(
        string name,
        string description,
        bool required = false,
        int? minLength = null,
        int? maxLength = null,
        bool? autocomplete = null,
        params ApplicationCommandOptionChoice[] choices)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        ValidateStringLengthBounds(minLength, maxLength);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.String,
            Required = required,
            Choices = choices,
            MinLength = minLength,
            MaxLength = maxLength,
            Autocomplete = autocomplete,
        });
    }

    /// <summary>
    /// Adds an <see cref="ApplicationCommandOptionType.Integer"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <param name="minValue">Minimum numeric value.</param>
    /// <param name="maxValue">Maximum numeric value.</param>
    /// <param name="autocomplete">Whether autocomplete is enabled for this option.</param>
    /// <param name="choices">Static choices available for this option.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddIntegerOption(
        string name,
        string description,
        bool required = false,
        object? minValue = null,
        object? maxValue = null,
        bool? autocomplete = null,
        params ApplicationCommandOptionChoice[] choices)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        ValidateNumericBounds(minValue, maxValue);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.Integer,
            Required = required,
            Choices = choices,
            MinValue = minValue,
            MaxValue = maxValue,
            Autocomplete = autocomplete,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.Boolean"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddBooleanOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.Boolean,
            Required = required,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.User"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddUserOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.User,
            Required = required,

        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.Channel"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <param name="channelTypes">Allowed channel types for this option.</param>
    /// <returns>The current <see cref="ApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddChannelOption(
        string name,
        string description,
        bool required = false,
        params ChannelType[] channelTypes)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.Channel,
            Required = required,
            ChannelTypes = channelTypes,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.Role"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddRoleOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.Role,
            Required = required,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.Mentionable"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddMentionableOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.Mentionable,
            Required = required,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.Number"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <param name="minValue">Minimum numeric value.</param>
    /// <param name="maxValue">Maximum numeric value.</param>
    /// <param name="autocomplete">Whether autocomplete is enabled for this option.</param>
    /// <param name="choices">Static choices available for this option.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddNumberOption(
        string name,
        string description,
        bool required = false,
        object? minValue = null,
        object? maxValue = null,
        bool? autocomplete = null,
        params ApplicationCommandOptionChoice[] choices)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        ValidateNumericBounds(minValue, maxValue);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.Number,
            Required = required,
            Choices = choices,
            MinValue = minValue,
            MaxValue = maxValue,
            Autocomplete = autocomplete,
        });
    }

    /// <summary>
    /// Adds an <see cref="ApplicationCommandOptionType.Attachment"/> option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <param name="required">Whether the option must be provided by the user.</param>
    /// <returns>The current <see cref="IApplicationCommandBuilder"/> instance.</returns>
    public IApplicationCommandBuilder AddAttachmentOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name,
            Description = description,
            Type = ApplicationCommandOptionType.Attachment,
            Required = required,
        });
    }

    /// <summary>
    /// Builds the configured <see cref="ApplicationCommand"/> instance.
    /// </summary>
    /// <returns>The configured <see cref="ApplicationCommand"/>.</returns>
    public ApplicationCommand Build()
    {
        return new ApplicationCommand
        {
            Type = _type,
            Name = _name ?? throw new InvalidOperationException("Command name is required."),
            NameLocalizations = _nameLocalizations,
            Description = _description ?? throw new InvalidOperationException("Command description is required."),
            DescriptionLocalizations = _descriptionLocalizations,
            Options = _options.Count > 0 ? [.. _options] : null,
            DefaultMemberPermissions = _defaultMemberPermissions,
            DmPermission = _dmPermission,
            Nsfw = _nsfw,
            Version = _version
        };
    }

    private static void ValidateCommandName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null or empty.", nameof(name));

        if (name.Length is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(name), "Command name must be between 1 and 32 characters.");
    }

    private static void ValidateCommandDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Command description cannot be null or empty.", nameof(description));

        if (description.Length is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(description), "Command description must be between 1 and 100 characters.");
    }

    private static void ValidateOptionName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Option name cannot be null or empty.", nameof(name));

        if (name.Length is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(name), "Option name must be between 1 and 32 characters.");
    }

    private static void ValidateOptionDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Option description cannot be null or empty.", nameof(description));

        if (description.Length is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(description), "Option description must be between 1 and 100 characters.");
    }

    private static void ValidateStringLengthBounds(int? minLength, int? maxLength)
    {
        if (minLength is < 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "minLength cannot be negative.");

        if (maxLength is < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength cannot be negative.");

        if (minLength.HasValue && maxLength.HasValue && minLength.Value > maxLength.Value)
            throw new ArgumentException("minLength cannot be greater than maxLength.");
    }

    private static void ValidateNumericBounds(object? minValue, object? maxValue)
    {
        if (minValue is IComparable min && maxValue is IComparable max)
        {
            if (min.GetType() == max.GetType() && min.CompareTo(max) > 0)
                throw new ArgumentException("minValue cannot be greater than maxValue.");
        }
    }
}