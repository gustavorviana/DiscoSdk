using DiscoSdk.Models.Enums;
using DiscoSdk.Utils;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Represents a Discord application command (slash command).
/// </summary>
public class ApplicationCommand : IEquatable<ApplicationCommand?>
{
    /// <summary>
    /// Gets or sets the unique ID of the command.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake? Id { get; set; }

    /// <summary>
    /// Gets or sets the type of command.
    /// </summary>
    [JsonPropertyName("type")]
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
    public ApplicationCommandOption[]? Options { get; set; }

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

    public override bool Equals(object? obj)
    {
        return Equals(obj as ApplicationCommand);
    }

    public bool Equals(ApplicationCommand? other)
    {
        return other is not null &&
               Type == other.Type &&
               Name == other.Name &&
               CollectionUtils.DictionaryEquals(NameLocalizations, other.NameLocalizations) &&
               Description == other.Description &&
               CollectionUtils.DictionaryEquals(DescriptionLocalizations, other.DescriptionLocalizations) &&
               CollectionUtils.SequenceEquals(Options, other.Options) &&
               DefaultMemberPermissions == other.DefaultMemberPermissions &&
               ValueUtils.UnsafeBoolComparer(DmPermission, other.DmPermission) &&
               ValueUtils.UnsafeBoolComparer(Nsfw, other.Nsfw);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(Type);
        hash.Add(Name);
        hash.Add(NameLocalizations);
        hash.Add(Description);
        hash.Add(DescriptionLocalizations);
        hash.Add(Options);
        hash.Add(DefaultMemberPermissions);
        hash.Add(DmPermission);
        hash.Add(Nsfw);
        return hash.ToHashCode();
    }

    public static bool operator ==(ApplicationCommand? left, ApplicationCommand? right)
    {
        return EqualityComparer<ApplicationCommand>.Default.Equals(left, right);
    }

    public static bool operator !=(ApplicationCommand? left, ApplicationCommand? right)
    {
        return !(left == right);
    }
}
