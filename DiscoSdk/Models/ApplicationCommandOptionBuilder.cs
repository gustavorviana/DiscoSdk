namespace DiscoSdk.Models;

/// <summary>
/// Builder for creating <see cref="ApplicationCommandOption"/> instances with validation to prevent conflicts.
/// </summary>
public class ApplicationCommandOptionBuilder
{
    private ApplicationCommandOptionType _type;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private Dictionary<string, string>? _nameLocalizations;
    private Dictionary<string, string>? _descriptionLocalizations;
    private bool? _required;
    private List<ApplicationCommandOptionChoice>? _choices;
    private List<ApplicationCommandOption>? _options;
    private List<int>? _channelTypes;
    private object? _minValue;
    private object? _maxValue;
    private int? _minLength;
    private int? _maxLength;
    private bool? _autocomplete;

    /// <summary>
    /// Sets the type of the option.
    /// </summary>
    /// <param name="type">The option type.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithType(ApplicationCommandOptionType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the name of the option (1-32 characters, lowercase).
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of the option (1-100 characters).
    /// </summary>
    /// <param name="description">The option description.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the localization dictionary for the name field.
    /// </summary>
    /// <param name="localizations">Dictionary mapping locale codes to localized names.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithNameLocalizations(Dictionary<string, string> localizations)
    {
        _nameLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Sets the localization dictionary for the description field.
    /// </summary>
    /// <param name="localizations">Dictionary mapping locale codes to localized descriptions.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithDescriptionLocalizations(Dictionary<string, string> localizations)
    {
        _descriptionLocalizations = localizations;
        return this;
    }

    /// <summary>
    /// Sets whether the option is required.
    /// Note: This is ignored for SubCommand and SubCommandGroup types.
    /// </summary>
    /// <param name="required">Whether the option is required.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithRequired(bool required)
    {
        _required = required;
        return this;
    }

    /// <summary>
    /// Sets the choices for the option.
    /// Only valid for STRING, INTEGER, and NUMBER types.
    /// Cannot be used together with autocomplete.
    /// </summary>
    /// <param name="choices">List of choices for the option.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithChoices(params ApplicationCommandOptionChoice[] choices)
    {
        _choices = [.. choices];
        _autocomplete = null; // Clear autocomplete if choices are set
        return this;
    }

    /// <summary>
    /// Adds a single choice to the option.
    /// Only valid for STRING, INTEGER, and NUMBER types.
    /// Cannot be used together with autocomplete.
    /// </summary>
    /// <param name="name">The name of the choice (1-100 characters).</param>
    /// <param name="value">The value of the choice (string, integer, or double).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder AddChoice(string name, object value)
    {
        _choices ??= [];
        _choices.Add(new ApplicationCommandOptionChoice
        {
            Name = name,
            Value = value
        });
        _autocomplete = null; // Clear autocomplete if choices are added
        return this;
    }

    /// <summary>
    /// Sets the nested options (for subcommands and subcommand groups).
    /// Only valid for SubCommand and SubCommandGroup types.
    /// </summary>
    /// <param name="options">List of nested options.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithOptions(params ApplicationCommandOption[] options)
    {
        _options = [.. options];
        return this;
    }

    /// <summary>
    /// Adds a nested option using a builder.
    /// Only valid for SubCommand and SubCommandGroup types.
    /// </summary>
    /// <param name="builder">The builder for the nested option.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder AddOption(ApplicationCommandOptionBuilder builder)
    {
        _options ??= [];
        _options.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Adds a nested option directly.
    /// Only valid for SubCommand and SubCommandGroup types.
    /// </summary>
    /// <param name="option">The nested option to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder AddOption(ApplicationCommandOption option)
    {
        _options ??= [];
        _options.Add(option);
        return this;
    }

    /// <summary>
    /// Sets the channel types to include.
    /// Only valid for CHANNEL type.
    /// </summary>
    /// <param name="channelTypes">List of channel type IDs to include.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithChannelTypes(List<int> channelTypes)
    {
        _channelTypes = channelTypes;
        return this;
    }

    /// <summary>
    /// Adds a channel type to include.
    /// Only valid for CHANNEL type.
    /// </summary>
    /// <param name="channelType">The channel type ID to include.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder AddChannelType(int channelType)
    {
        _channelTypes ??= [];
        _channelTypes.Add(channelType);
        return this;
    }

    /// <summary>
    /// Sets the minimum value (for INTEGER and NUMBER types).
    /// </summary>
    /// <param name="minValue">The minimum value (int for INTEGER, double for NUMBER).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithMinValue(object minValue)
    {
        _minValue = minValue;
        return this;
    }

    /// <summary>
    /// Sets the maximum value (for INTEGER and NUMBER types).
    /// </summary>
    /// <param name="maxValue">The maximum value (int for INTEGER, double for NUMBER).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithMaxValue(object maxValue)
    {
        _maxValue = maxValue;
        return this;
    }

    /// <summary>
    /// Sets the minimum length (for STRING type).
    /// </summary>
    /// <param name="minLength">The minimum length (0-6000).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithMinLength(int minLength)
    {
        _minLength = minLength;
        return this;
    }

    /// <summary>
    /// Sets the maximum length (for STRING type).
    /// </summary>
    /// <param name="maxLength">The maximum length (1-6000).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithMaxLength(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }

    /// <summary>
    /// Sets whether autocomplete is enabled.
    /// Only valid for STRING, INTEGER, and NUMBER types.
    /// Cannot be used together with choices.
    /// </summary>
    /// <param name="autocomplete">Whether autocomplete is enabled.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApplicationCommandOptionBuilder WithAutocomplete(bool autocomplete)
    {
        _autocomplete = autocomplete;
        _choices = null; // Clear choices if autocomplete is set
        return this;
    }

    /// <summary>
    /// Builds the <see cref="ApplicationCommandOption"/> instance with validation.
    /// </summary>
    /// <returns>The built option instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the option configuration is invalid.</exception>
    public ApplicationCommandOption Build()
    {
        Validate();

        return new ApplicationCommandOption
        {
            Type = _type,
            Name = _name,
            Description = _description,
            NameLocalizations = _nameLocalizations,
            DescriptionLocalizations = _descriptionLocalizations,
            Required = _required,
            Choices = _choices,
            Options = _options,
            ChannelTypes = _channelTypes,
            MinValue = _minValue,
            MaxValue = _maxValue,
            MinLength = _minLength,
            MaxLength = _maxLength,
            Autocomplete = _autocomplete
        };
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Option name is required.");

        if (_name.Length > 32)
            throw new InvalidOperationException("Option name must be 1-32 characters.");

        if (string.IsNullOrWhiteSpace(_description))
            throw new InvalidOperationException("Option description is required.");

        if (_description.Length > 100)
            throw new InvalidOperationException("Option description must be 1-100 characters.");

        // Validate choices
        if (_choices != null && _choices.Count > 0)
        {
            if (_type != ApplicationCommandOptionType.String &&
                _type != ApplicationCommandOptionType.Integer &&
                _type != ApplicationCommandOptionType.Number)
            {
                throw new InvalidOperationException($"Choices can only be used with STRING, INTEGER, or NUMBER types, not {_type}.");
            }

            if (_autocomplete == true)
            {
                throw new InvalidOperationException("Choices and autocomplete cannot be used together.");
            }

            if (_choices.Count > 25)
            {
                throw new InvalidOperationException("Choices cannot exceed 25 items.");
            }
        }

        // Validate autocomplete
        if (_autocomplete == true)
        {
            if (_type != ApplicationCommandOptionType.String &&
                _type != ApplicationCommandOptionType.Integer &&
                _type != ApplicationCommandOptionType.Number)
            {
                throw new InvalidOperationException($"Autocomplete can only be used with STRING, INTEGER, or NUMBER types, not {_type}.");
            }
        }

        // Validate nested options
        if (_options != null && _options.Count > 0)
        {
            if (_type != ApplicationCommandOptionType.SubCommand &&
                _type != ApplicationCommandOptionType.SubCommandGroup)
            {
                throw new InvalidOperationException($"Nested options can only be used with SubCommand or SubCommandGroup types, not {_type}.");
            }
        }

        // Validate channel types
        if (_channelTypes != null && _channelTypes.Count > 0)
        {
            if (_type != ApplicationCommandOptionType.Channel)
            {
                throw new InvalidOperationException($"Channel types can only be used with CHANNEL type, not {_type}.");
            }
        }

        // Validate min/max value
        if (_minValue != null || _maxValue != null)
        {
            if (_type != ApplicationCommandOptionType.Integer && _type != ApplicationCommandOptionType.Number)
            {
                throw new InvalidOperationException($"Min/Max value can only be used with INTEGER or NUMBER types, not {_type}.");
            }
        }

        // Validate min/max length
        if (_minLength.HasValue || _maxLength.HasValue)
        {
            if (_type != ApplicationCommandOptionType.String)
            {
                throw new InvalidOperationException($"Min/Max length can only be used with STRING type, not {_type}.");
            }

            if (_minLength.HasValue && (_minLength < 0 || _minLength > 6000))
            {
                throw new InvalidOperationException("Min length must be between 0 and 6000.");
            }

            if (_maxLength.HasValue && (_maxLength < 1 || _maxLength > 6000))
            {
                throw new InvalidOperationException("Max length must be between 1 and 6000.");
            }

            if (_minLength.HasValue && _maxLength.HasValue && _minLength > _maxLength)
            {
                throw new InvalidOperationException("Min length cannot be greater than max length.");
            }
        }

        // Validate required (not applicable to subcommands)
        if (_required.HasValue && (_type == ApplicationCommandOptionType.SubCommand || _type == ApplicationCommandOptionType.SubCommandGroup))
        {
            // Just clear it, don't throw - it's ignored by Discord anyway
            _required = null;
        }
    }

    /// <summary>
    /// Creates a new builder instance.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static ApplicationCommandOptionBuilder Create()
    {
        return new ApplicationCommandOptionBuilder();
    }

    /// <summary>
    /// Creates a builder for a String option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a String option.</returns>
    public static ApplicationCommandOptionBuilder String(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.String)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for an Integer option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for an Integer option.</returns>
    public static ApplicationCommandOptionBuilder Integer(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Integer)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a Number (double) option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a Number option.</returns>
    public static ApplicationCommandOptionBuilder Number(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Number)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a Boolean option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a Boolean option.</returns>
    public static ApplicationCommandOptionBuilder Boolean(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Boolean)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a User option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a User option.</returns>
    public static ApplicationCommandOptionBuilder User(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.User)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a Channel option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a Channel option.</returns>
    public static ApplicationCommandOptionBuilder Channel(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Channel)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a Role option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a Role option.</returns>
    public static ApplicationCommandOptionBuilder Role(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Role)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a Mentionable (user or role) option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a Mentionable option.</returns>
    public static ApplicationCommandOptionBuilder Mentionable(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Mentionable)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for an Attachment option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for an Attachment option.</returns>
    public static ApplicationCommandOptionBuilder Attachment(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.Attachment)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a SubCommand option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a SubCommand option.</returns>
    public static ApplicationCommandOptionBuilder SubCommand(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(name)
            .WithDescription(description);
    }

    /// <summary>
    /// Creates a builder for a SubCommandGroup option.
    /// </summary>
    /// <param name="name">The option name.</param>
    /// <param name="description">The option description.</param>
    /// <returns>A builder instance configured for a SubCommandGroup option.</returns>
    public static ApplicationCommandOptionBuilder SubCommandGroup(string name, string description)
    {
        return new ApplicationCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommandGroup)
            .WithName(name)
            .WithDescription(description);
    }
}

