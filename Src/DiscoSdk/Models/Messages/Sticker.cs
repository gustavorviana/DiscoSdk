using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a sticker.
/// </summary>
public class Sticker : IWithSnowflake
{
    /// <summary>
    /// Gets or sets the ID of the sticker.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; set; } = default!;

    public DateTimeOffset CreatedAt => Id.CreatedAt;

    /// <summary>
    /// Gets or sets the ID of the pack the sticker is from.
    /// </summary>
    [JsonPropertyName("pack_id")]
    public Snowflake? PackId { get; set; }

    /// <summary>
    /// Gets or sets the name of the sticker.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the description of the sticker.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the type of sticker format.
    /// </summary>
    [JsonPropertyName("format_type")]
    public StickerFormatType FormatType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this guild sticker can be used.
    /// </summary>
    [JsonPropertyName("available")]
    public bool? Available { get; set; }

    /// <summary>
    /// Gets or sets the ID of the guild that owns the sticker.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the user that uploaded the guild sticker.
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the standard sticker's sort order within its pack.
    /// </summary>
    [JsonPropertyName("sort_value")]
    public int? SortValue { get; set; }

    /// <summary>
    /// Gets or sets the tags for the sticker.
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }
}
