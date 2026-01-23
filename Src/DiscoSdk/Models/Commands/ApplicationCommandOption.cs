using DiscoSdk.Models.Enums;
using DiscoSdk.Utils;
using System.Linq;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Represents an option (parameter) for an application command.
/// </summary>
public class ApplicationCommandOption : IEquatable<ApplicationCommandOption?>
{
    /// <summary>
    /// Gets or sets the type of option.
    /// </summary>
    [JsonPropertyName("type")]
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
    public ApplicationCommandOptionChoice[]? Choices { get; set; }

    /// <summary>
    /// Gets or sets the nested options (for subcommands and subcommand groups).
    /// </summary>
    [JsonPropertyName("options")]
    public ApplicationCommandOption[]? Options { get; set; }

    /// <summary>
    /// Gets or sets the channel types to include (only for CHANNEL type).
    /// </summary>
    [JsonPropertyName("channel_types")]
    public ChannelType[]? ChannelTypes { get; set; }

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

    public override bool Equals(object? obj)
    {
        return Equals(obj as ApplicationCommandOption);
    }

    public bool Equals(ApplicationCommandOption? other)
    {
        return other is not null &&
               Type == other.Type &&
               Name == other.Name &&
               CollectionUtils.DictionaryEquals(NameLocalizations, other.NameLocalizations) &&
               Description == other.Description &&
               CollectionUtils.DictionaryEquals(DescriptionLocalizations, other.DescriptionLocalizations) &&
               ValueUtils.UnsafeBoolComparer(Required, other.Required) &&
               CollectionUtils.SequenceEquals(Choices, other.Choices) &&
               CollectionUtils.SequenceEquals(Options, other.Options) &&
               CollectionUtils.SequenceEquals(ChannelTypes, other.ChannelTypes) &&
               ValueUtils.ValueEquals(MinValue, other.MinValue) &&
               ValueUtils.ValueEquals(MaxValue, other.MaxValue) &&
               MinLength == other.MinLength &&
               MaxLength == other.MaxLength &&
               Autocomplete == other.Autocomplete;
    }


    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Type);
        hash.Add(Name);
        hash.Add(NameLocalizations);
        hash.Add(Description);
        hash.Add(DescriptionLocalizations);
        hash.Add(Required);
        hash.Add(Choices);
        hash.Add(Options);
        hash.Add(ChannelTypes);
        hash.Add(MinValue);
        hash.Add(MaxValue);
        hash.Add(MinLength);
        hash.Add(MaxLength);
        hash.Add(Autocomplete);
        return hash.ToHashCode();
    }

    public static bool operator ==(ApplicationCommandOption? left, ApplicationCommandOption? right)
    {
        return EqualityComparer<ApplicationCommandOption>.Default.Equals(left, right);
    }

    public static bool operator !=(ApplicationCommandOption? left, ApplicationCommandOption? right)
    {
        return !(left == right);
    }
}
