using System.Text.RegularExpressions;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Commands;

/// <summary>
/// Fluent builder for <see cref="ApplicationCommand"/>.
/// </summary>
public class SlashCommandBuilder()
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithName(string name)
    {
        ValidateCommandName(name);
        _name = name.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Sets the command description (1-100 characters).
    /// </summary>
    /// <param name="description">The description shown in the Discord client.</param>
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithDescription(string description)
    {
        ValidateCommandDescription(description);
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the command type.
    /// </summary>
    /// <param name="type">The <see cref="ApplicationCommandType"/> for this command.</param>
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithType(ApplicationCommandType type)
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        ValidateLocalizations(localizations, "name");
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        ValidateLocalizations(localizations, "description");
        _descriptionLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Sets the default member permissions required to use the command.
    /// </summary>
    /// <param name="permissions">
    /// A permissions bitfield represented as a string, matching Discord's API format.
    /// </param>
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithDefaultMemberPermissions(string permissions)
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithDmPermission(bool dmPermission)
    {
        _dmPermission = dmPermission;
        return this;
    }

    /// <summary>
    /// Sets whether the command is age-restricted (NSFW).
    /// </summary>
    /// <param name="nsfw"><c>true</c> if the command is NSFW; otherwise, <c>false</c>.</param>
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithNsfw(bool nsfw)
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Adds an option to the command.
    /// </summary>
    /// <param name="option">The option instance to add.</param>
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    internal SlashCommandBuilder AddOption(ApplicationCommandOption option)
    {
        if (_options.Count >= 25)
            throw new InvalidOperationException("A command can have a maximum of 25 options. Discord API limit.");

        ValidateOption(option);
        _options.Add(option);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.SubCommand"/> option.
    /// </summary>
    /// <param name="name">The subcommand name.</param>
    /// <param name="description">The subcommand description.</param>
    /// <param name="options">Nested options (parameters) for this subcommand.</param>
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddSubCommandOption(
        string name,
        string description,
        params ApplicationCommandOption[] options)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        ValidateNestedOptions(options, ApplicationCommandOptionType.SubCommand);

        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
            Description = description,
            Type = ApplicationCommandOptionType.SubCommand,
            Options = options.Length > 0 ? options : null,
        });
    }

    /// <summary>
    /// Adds a <see cref="ApplicationCommandOptionType.SubCommandGroup"/> option.
    /// </summary>
    /// <param name="name">The subcommand group name.</param>
    /// <param name="description">The subcommand group description.</param>
    /// <param name="options">Subcommands that belong to this group.</param>
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddSubCommandGroupOption(
        string name,
        string description,
        params ApplicationCommandOption[] options)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        ValidateNestedOptions(options, ApplicationCommandOptionType.SubCommandGroup);

        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
            Description = description,
            Type = ApplicationCommandOptionType.SubCommandGroup,
            Options = options.Length > 0 ? options : null,
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddStringOption(
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
        ValidateChoices(choices, ApplicationCommandOptionType.String);
        ValidateAutocompleteAndChoices(autocomplete, choices);

        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
            Description = description,
            Type = ApplicationCommandOptionType.String,
            Required = required,
            Choices = choices.Length > 0 ? choices : null,
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddIntegerOption(
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
        ValidateNumericBounds(minValue, maxValue, ApplicationCommandOptionType.Integer);
        ValidateChoices(choices, ApplicationCommandOptionType.Integer);
        ValidateAutocompleteAndChoices(autocomplete, choices);

        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
            Description = description,
            Type = ApplicationCommandOptionType.Integer,
            Required = required,
            Choices = choices.Length > 0 ? choices : null,
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddBooleanOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddUserOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddChannelOption(
        string name,
        string description,
        bool required = false,
        params ChannelType[] channelTypes)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddRoleOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddMentionableOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddNumberOption(
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
        ValidateNumericBounds(minValue, maxValue, ApplicationCommandOptionType.Number);
        ValidateChoices(choices, ApplicationCommandOptionType.Number);
        ValidateAutocompleteAndChoices(autocomplete, choices);

        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
            Description = description,
            Type = ApplicationCommandOptionType.Number,
            Required = required,
            Choices = choices.Length > 0 ? choices : null,
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
    /// <returns>The current <see cref="SlashCommandBuilder"/> instance.</returns>
    public SlashCommandBuilder AddAttachmentOption(
        string name,
        string description,
        bool required = false)
    {
        ValidateOptionName(name);
        ValidateOptionDescription(description);
        return AddOption(new ApplicationCommandOption
        {
            Name = name.ToLowerInvariant(),
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
            throw new ArgumentException("Command name cannot be null, empty, or contain only whitespace.", nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(name), $"Command name must be between 1 and 32 characters. Current length: {trimmedName.Length}.");

        if (!Regex.IsMatch(trimmedName, @"^[a-z0-9-_]+$"))
            throw new ArgumentException("Command name can only contain lowercase letters (a-z), numbers (0-9), hyphens (-), or underscores (_). Spaces and special characters are not allowed.", nameof(name));
    }

    private static void ValidateCommandDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Command description cannot be null, empty, or contain only whitespace.", nameof(description));

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(description), $"Command description must be between 1 and 100 characters. Current length: {trimmedDescription.Length}.");
    }

    private static void ValidateOptionName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Option name cannot be null, empty, or contain only whitespace.", nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(name), $"Option name must be between 1 and 32 characters. Current length: {trimmedName.Length}.");

        if (!Regex.IsMatch(trimmedName, @"^[a-z0-9-_]+$"))
            throw new ArgumentException("Option name can only contain lowercase letters (a-z), numbers (0-9), hyphens (-), or underscores (_). Spaces and special characters are not allowed.", nameof(name));
    }

    private static void ValidateOptionDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Option description cannot be null, empty, or contain only whitespace.", nameof(description));

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(description), $"Option description must be between 1 and 100 characters. Current length: {trimmedDescription.Length}.");
    }

    private static void ValidateStringLengthBounds(int? minLength, int? maxLength)
    {
        if (minLength.HasValue && minLength.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "String minimum length cannot be negative.");

        if (maxLength.HasValue && maxLength.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "String maximum length cannot be negative.");

        if (minLength.HasValue && minLength.Value > 6000)
            throw new ArgumentOutOfRangeException(nameof(minLength), $"String minimum length cannot be greater than 6000 characters (Discord API limit). Provided value: {minLength.Value}.");

        if (maxLength.HasValue && maxLength.Value > 6000)
            throw new ArgumentOutOfRangeException(nameof(maxLength), $"String maximum length cannot be greater than 6000 characters (Discord API limit). Provided value: {maxLength.Value}.");

        if (minLength.HasValue && maxLength.HasValue && minLength.Value > maxLength.Value)
            throw new ArgumentException($"Minimum length ({minLength.Value}) cannot be greater than maximum length ({maxLength.Value}).");
    }

    private static void ValidateNumericBounds(object? minValue, object? maxValue, ApplicationCommandOptionType optionType)
    {
        const long minInteger = -9007199254740991L; // -2^53 + 1
        const long maxInteger = 9007199254740991L;  // 2^53 - 1
        const double minNumber = -9007199254740991.0; // -2^53 + 1
        const double maxNumber = 9007199254740991.0;  // 2^53 - 1

        if (optionType == ApplicationCommandOptionType.Integer)
        {
            if (minValue is long minLong)
            {
                if (minLong < minInteger || minLong > maxInteger)
                    throw new ArgumentOutOfRangeException(nameof(minValue), $"Integer minimum value must be between {minInteger} and {maxInteger} (Discord API limit). Provided value: {minLong}.");
            }
            else if (minValue is int minInt)
            {
                if (minInt < minInteger || minInt > maxInteger)
                    throw new ArgumentOutOfRangeException(nameof(minValue), $"Integer minimum value must be between {minInteger} and {maxInteger} (Discord API limit). Provided value: {minInt}.");
            }

            if (maxValue is long maxLong)
            {
                if (maxLong < minInteger || maxLong > maxInteger)
                    throw new ArgumentOutOfRangeException(nameof(maxValue), $"Integer maximum value must be between {minInteger} and {maxInteger} (Discord API limit). Provided value: {maxLong}.");
            }
            else if (maxValue is int maxInt)
            {
                if (maxInt < minInteger || maxInt > maxInteger)
                    throw new ArgumentOutOfRangeException(nameof(maxValue), $"Integer maximum value must be between {minInteger} and {maxInteger} (Discord API limit). Provided value: {maxInt}.");
            }
        }
        else if (optionType == ApplicationCommandOptionType.Number)
        {
            if (minValue is double minDouble)
            {
                if (minDouble < minNumber || minDouble > maxNumber)
                    throw new ArgumentOutOfRangeException(nameof(minValue), $"Number minimum value must be between {minNumber} and {maxNumber} (Discord API limit). Provided value: {minDouble}.");
            }
            else if (minValue is decimal minDecimal)
            {
                var minDecimalDouble = (double)minDecimal;
                if (minDecimalDouble < minNumber || minDecimalDouble > maxNumber)
                    throw new ArgumentOutOfRangeException(nameof(minValue), $"Number minimum value must be between {minNumber} and {maxNumber} (Discord API limit). Provided value: {minDecimal}.");
            }
            else if (minValue is float minFloat)
            {
                if (minFloat < minNumber || minFloat > maxNumber)
                    throw new ArgumentOutOfRangeException(nameof(minValue), $"Number minimum value must be between {minNumber} and {maxNumber} (Discord API limit). Provided value: {minFloat}.");
            }

            if (maxValue is double maxDouble)
            {
                if (maxDouble < minNumber || maxDouble > maxNumber)
                    throw new ArgumentOutOfRangeException(nameof(maxValue), $"Number maximum value must be between {minNumber} and {maxNumber} (Discord API limit). Provided value: {maxDouble}.");
            }
            else if (maxValue is decimal maxDecimal)
            {
                var maxDecimalDouble = (double)maxDecimal;
                if (maxDecimalDouble < minNumber || maxDecimalDouble > maxNumber)
                    throw new ArgumentOutOfRangeException(nameof(maxValue), $"Number maximum value must be between {minNumber} and {maxNumber} (Discord API limit). Provided value: {maxDecimal}.");
            }
            else if (maxValue is float maxFloat)
            {
                if (maxFloat < minNumber || maxFloat > maxNumber)
                    throw new ArgumentOutOfRangeException(nameof(maxValue), $"Number maximum value must be between {minNumber} and {maxNumber} (Discord API limit). Provided value: {maxFloat}.");
            }
        }

        if (minValue is IComparable min && maxValue is IComparable max)
        {
            if (min.GetType() == max.GetType() && min.CompareTo(max) > 0)
                throw new ArgumentException($"Minimum value ({minValue}) cannot be greater than maximum value ({maxValue}).");
        }
    }

    private static void ValidateChoices(ApplicationCommandOptionChoice[] choices, ApplicationCommandOptionType optionType)
    {
        if (choices.Length == 0)
            return;

        if (choices.Length > 25)
            throw new ArgumentException($"An option can have a maximum of 25 choices. Provided count: {choices.Length}.", nameof(choices));

        if (optionType != ApplicationCommandOptionType.String &&
            optionType != ApplicationCommandOptionType.Integer &&
            optionType != ApplicationCommandOptionType.Number)
        {
            throw new ArgumentException($"Choices are only allowed for String, Integer, or Number option types. Provided type: {optionType}.", nameof(choices));
        }

        for (int i = 0; i < choices.Length; i++)
        {
            var choice = choices[i];
            if (choice == null)
                throw new ArgumentException($"Choice at index {i} cannot be null.", nameof(choices));

            if (string.IsNullOrWhiteSpace(choice.Name))
                throw new ArgumentException($"Choice name at index {i} cannot be null, empty, or contain only whitespace.", nameof(choices));

            var trimmedChoiceName = choice.Name.Trim();
            if (trimmedChoiceName.Length is < 1 or > 100)
                throw new ArgumentOutOfRangeException(nameof(choices), $"Choice name at index {i} must be between 1 and 100 characters. Current length: {trimmedChoiceName.Length}.");

            if (choice.Value == null)
                throw new ArgumentException($"Choice value at index {i} cannot be null.", nameof(choices));

            // Validate value type based on option type
            if (optionType == ApplicationCommandOptionType.String && choice.Value is not string)
                throw new ArgumentException($"Choice value at index {i} must be a string for String option type. Provided type: {choice.Value.GetType().Name}.", nameof(choices));

            if (optionType == ApplicationCommandOptionType.Integer && choice.Value is not int and not long)
                throw new ArgumentException($"Choice value at index {i} must be an integer (int or long) for Integer option type. Provided type: {choice.Value.GetType().Name}.", nameof(choices));

            if (optionType == ApplicationCommandOptionType.Number && choice.Value is not double and not decimal and not float)
                throw new ArgumentException($"Choice value at index {i} must be a number (double, decimal, or float) for Number option type. Provided type: {choice.Value.GetType().Name}.", nameof(choices));
        }
    }

    private static void ValidateAutocompleteAndChoices(bool? autocomplete, ApplicationCommandOptionChoice[] choices)
    {
        if (autocomplete == true && choices.Length > 0)
            throw new ArgumentException("Cannot use autocomplete and choices at the same time. Choose one option.", nameof(autocomplete));
    }

    private static void ValidateNestedOptions(ApplicationCommandOption[] options, ApplicationCommandOptionType parentType)
    {
        if (options.Length == 0)
            return;

        if (options.Length > 25)
            throw new ArgumentException($"A nested option can have a maximum of 25 sub-options. Provided count: {options.Length}.", nameof(options));

        foreach (var option in options)
        {
            if (option == null)
                throw new ArgumentException("A nested option cannot be null.", nameof(options));

            ValidateOption(option);

            if (parentType == ApplicationCommandOptionType.SubCommandGroup)
            {
                if (option.Type != ApplicationCommandOptionType.SubCommand)
                    throw new ArgumentException($"SubCommandGroups can only contain SubCommands. Provided type: {option.Type}.", nameof(options));
            }
        }
    }

    private static void ValidateOption(ApplicationCommandOption option)
    {
        if (option == null)
            throw new ArgumentNullException(nameof(option), "Option cannot be null.");

        if (string.IsNullOrWhiteSpace(option.Name))
            throw new ArgumentException("Option name cannot be null, empty, or contain only whitespace.", nameof(option));

        if (string.IsNullOrWhiteSpace(option.Description))
            throw new ArgumentException("Option description cannot be null, empty, or contain only whitespace.", nameof(option));

        if (option.Choices != null && option.Choices.Length > 0)
        {
            ValidateChoices(option.Choices, option.Type);
        }

        if (option.Options != null && option.Options.Length > 0)
        {
            ValidateNestedOptions(option.Options, option.Type);
        }
    }

    private static void ValidateLocalizations(Dictionary<string, string>? localizations, string fieldName)
    {
        if (localizations == null)
            return;

        // Locales válidos do Discord
        var validLocales = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id", "da", "de", "en-GB", "en-US", "es-ES", "fr", "hr", "it", "lt", "hu", "nl", "no", "pl", "pt-BR", "ro", "fi", "sv-SE", "vi", "tr", "cs", "el", "bg", "ru", "uk", "hi", "th", "zh-CN", "ja", "zh-TW", "ko"
        };

        foreach (var (locale, value) in localizations)
        {
            if (string.IsNullOrWhiteSpace(locale))
                throw new ArgumentException($"Locale code cannot be null, empty, or contain only whitespace in {fieldName} localizations dictionary.", nameof(localizations));

            if (!validLocales.Contains(locale))
                throw new ArgumentException($"Locale code '{locale}' is not a valid Discord locale. Valid locales: {string.Join(", ", validLocales)}.", nameof(localizations));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Localization value for locale '{locale}' cannot be null, empty, or contain only whitespace in {fieldName} localizations dictionary.", nameof(localizations));

            if (fieldName == "name")
            {
                var trimmedValue = value.Trim();
                if (trimmedValue.Length is < 1 or > 32)
                    throw new ArgumentOutOfRangeException(nameof(localizations), $"Localized name value for locale '{locale}' must be between 1 and 32 characters. Current length: {trimmedValue.Length}.");

                if (!Regex.IsMatch(trimmedValue, @"^[a-z0-9-_]+$"))
                    throw new ArgumentException($"Localized name value for locale '{locale}' can only contain lowercase letters (a-z), numbers (0-9), hyphens (-), or underscores (_).", nameof(localizations));
            }
            else if (fieldName == "description")
            {
                var trimmedValue = value.Trim();
                if (trimmedValue.Length is < 1 or > 100)
                    throw new ArgumentOutOfRangeException(nameof(localizations), $"Localized description value for locale '{locale}' must be between 1 and 100 characters. Current length: {trimmedValue.Length}.");
            }
        }
    }
}