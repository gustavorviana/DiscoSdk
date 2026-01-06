using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents the type of an application command.
/// </summary>
public enum ApplicationCommandType
{
    /// <summary>
    /// Slash command; a text-based command that shows up in a pop-up.
    /// </summary>
    ChatInput = 1,

    /// <summary>
    /// A UI-based command that shows up when you right-click or long-press on a user.
    /// </summary>
    User = 2,

    /// <summary>
    /// A UI-based command that shows up when you right-click or long-press on a message.
    /// </summary>
    Message = 3
}

/// <summary>
/// Represents the type of an application command option.
/// </summary>
public enum ApplicationCommandOptionType
{
    /// <summary>
    /// A subcommand.
    /// </summary>
    SubCommand = 1,

    /// <summary>
    /// A subcommand group.
    /// </summary>
    SubCommandGroup = 2,

    /// <summary>
    /// A string option.
    /// </summary>
    String = 3,

    /// <summary>
    /// An integer option.
    /// </summary>
    Integer = 4,

    /// <summary>
    /// A boolean option.
    /// </summary>
    Boolean = 5,

    /// <summary>
    /// A user option.
    /// </summary>
    User = 6,

    /// <summary>
    /// A channel option.
    /// </summary>
    Channel = 7,

    /// <summary>
    /// A role option.
    /// </summary>
    Role = 8,

    /// <summary>
    /// A mentionable option (user or role).
    /// </summary>
    Mentionable = 9,

    /// <summary>
    /// A number option (double).
    /// </summary>
    Number = 10,

    /// <summary>
    /// An attachment option.
    /// </summary>
    Attachment = 11
}

/// <summary>
/// Represents a Discord application command (slash command).
/// </summary>
public class ApplicationCommand
{
    /// <summary>
    /// Gets or sets the unique ID of the command.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the type of command.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(ApplicationCommandTypeNullableConverter))]
    public ApplicationCommandType? Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the command (1-32 characters, lowercase).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the name field.
    /// </summary>
    [JsonPropertyName("name_localizations")]
    public Dictionary<string, string>? NameLocalizations { get; set; }

    /// <summary>
    /// Gets or sets the description of the command (1-100 characters).
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the description field.
    /// </summary>
    [JsonPropertyName("description_localizations")]
    public Dictionary<string, string>? DescriptionLocalizations { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the command.
    /// </summary>
    [JsonPropertyName("options")]
    public List<ApplicationCommandOption>? Options { get; set; }

    /// <summary>
    /// Gets or sets the default member permissions required to use the command.
    /// </summary>
    [JsonPropertyName("default_member_permissions")]
    public string? DefaultMemberPermissions { get; set; }

    /// <summary>
    /// Gets or sets whether the command is available in DMs.
    /// </summary>
    [JsonPropertyName("dm_permission")]
    public bool? DmPermission { get; set; }

    /// <summary>
    /// Gets or sets whether the command is age-restricted.
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool? Nsfw { get; set; }

    /// <summary>
    /// Gets or sets the version of the command.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

/// <summary>
/// Represents an option (parameter) for an application command.
/// </summary>
public class ApplicationCommandOption
{
    /// <summary>
    /// Gets or sets the type of option.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(ApplicationCommandOptionTypeConverter))]
    public ApplicationCommandOptionType Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the option (1-32 characters, lowercase).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the name field.
    /// </summary>
    [JsonPropertyName("name_localizations")]
    public Dictionary<string, string>? NameLocalizations { get; set; }

    /// <summary>
    /// Gets or sets the description of the option (1-100 characters).
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the description field.
    /// </summary>
    [JsonPropertyName("description_localizations")]
    public Dictionary<string, string>? DescriptionLocalizations { get; set; }

    /// <summary>
    /// Gets or sets whether the option is required.
    /// </summary>
    [JsonPropertyName("required")]
    public bool? Required { get; set; }

    /// <summary>
    /// Gets or sets the choices for the option (only for STRING, INTEGER, and NUMBER types).
    /// </summary>
    [JsonPropertyName("choices")]
    public List<ApplicationCommandOptionChoice>? Choices { get; set; }

    /// <summary>
    /// Gets or sets the nested options (for subcommands and subcommand groups).
    /// </summary>
    [JsonPropertyName("options")]
    public List<ApplicationCommandOption>? Options { get; set; }

    /// <summary>
    /// Gets or sets the channel types to include (only for CHANNEL type).
    /// </summary>
    [JsonPropertyName("channel_types")]
    public List<int>? ChannelTypes { get; set; }

    /// <summary>
    /// Gets or sets the minimum value (for INTEGER and NUMBER types).
    /// </summary>
    [JsonPropertyName("min_value")]
    public object? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value (for INTEGER and NUMBER types).
    /// </summary>
    [JsonPropertyName("max_value")]
    public object? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the minimum length (for STRING type).
    /// </summary>
    [JsonPropertyName("min_length")]
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum length (for STRING type).
    /// </summary>
    [JsonPropertyName("max_length")]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets whether autocomplete is enabled.
    /// </summary>
    [JsonPropertyName("autocomplete")]
    public bool? Autocomplete { get; set; }
}

/// <summary>
/// Represents a choice for an application command option.
/// </summary>
public class ApplicationCommandOptionChoice
{
    /// <summary>
    /// Gets or sets the name of the choice (1-100 characters).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the localization dictionary for the name field.
    /// </summary>
    [JsonPropertyName("name_localizations")]
    public Dictionary<string, string>? NameLocalizations { get; set; }

    /// <summary>
    /// Gets or sets the value of the choice (string, integer, or double).
    /// </summary>
    [JsonPropertyName("value")]
    public object Value { get; set; } = default!;
}

