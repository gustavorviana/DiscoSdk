using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;


/// <summary>
/// Represents a permission overwrite for a channel.
/// </summary>
public class PermissionOverwrite
{
    /// <summary>
    /// Gets or sets the role or user ID.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of overwrite (0 for role, 1 for member).
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PermissionOverwriteType Type { get; set; }

    /// <summary>
    /// Gets or sets the permission bit set.
    /// </summary>
    [JsonPropertyName("allow")]
    [JsonConverter(typeof(DiscordPermissionConverter))]
    public DiscordPermission Allow { get; set; } = default!;

    /// <summary>
    /// Gets or sets the permission bit set.
    /// </summary>
    [JsonPropertyName("deny")]
    [JsonConverter(typeof(DiscordPermissionConverter))]
    public DiscordPermission Deny { get; set; } = default!;
}