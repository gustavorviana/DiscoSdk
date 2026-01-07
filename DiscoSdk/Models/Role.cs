using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord role.
/// </summary>
public class Role
{
    /// <summary>
    /// Gets or sets the role's unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public DiscordId Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the role's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the role's color.
    /// </summary>
    [JsonPropertyName("color")]
    public Color Color { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role is hoisted.
    /// </summary>
    [JsonPropertyName("hoist")]
    public bool Hoist { get; set; }

    /// <summary>
    /// Gets or sets the role's icon hash.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the role's unicode emoji.
    /// </summary>
    [JsonPropertyName("unicode_emoji")]
    public string? UnicodeEmoji { get; set; }

    /// <summary>
    /// Gets or sets the role's position.
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the role's permissions as a string (bitfield).
    /// </summary>
    [JsonPropertyName("permissions")]
    [JsonConverter(typeof(SafeStringConverter))]
    public string Permissions { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the role is managed by an integration.
    /// </summary>
    [JsonPropertyName("managed")]
    public bool Managed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role is mentionable.
    /// </summary>
    [JsonPropertyName("mentionable")]
    public bool Mentionable { get; set; }

    /// <summary>
    /// Gets or sets the tags for the role.
    /// </summary>
    [JsonPropertyName("tags")]
    public RoleTags? Tags { get; set; }

    /// <summary>
    /// Gets or sets the role flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public RoleFlags? Flags { get; set; }
}