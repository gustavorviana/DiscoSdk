using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Commands;

/// <summary>
/// Represents a choice for an application command option.
/// </summary>
public class SlashCommandOptionChoice : IEquatable<SlashCommandOptionChoice?>
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as SlashCommandOptionChoice);
    }

    public bool Equals(SlashCommandOptionChoice? other)
    {
        return other is not null &&
               Name == other.Name &&
               EqualityComparer<Dictionary<string, string>?>.Default.Equals(NameLocalizations, other.NameLocalizations) &&
               EqualityComparer<object>.Default.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, NameLocalizations, Value);
    }

    public static bool operator ==(SlashCommandOptionChoice? left, SlashCommandOptionChoice? right)
    {
        return EqualityComparer<SlashCommandOptionChoice>.Default.Equals(left, right);
    }

    public static bool operator !=(SlashCommandOptionChoice? left, SlashCommandOptionChoice? right)
    {
        return !(left == right);
    }
}