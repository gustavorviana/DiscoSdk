using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents an activity (presence) as received from Discord. Use for reading only.
/// For updating the bot's presence, use <see cref="ActivityUpdate"/>.
/// </summary>
public class Activity
{
    /// <summary>
    /// Gets or sets the activity's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    [JsonPropertyName("type")]
    public ActivityType Type { get; set; }

    /// <summary>
    /// Gets or sets the stream URL, if the activity is streaming.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the activity was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public long? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamps for the activity.
    /// </summary>
    [JsonPropertyName("timestamps")]
    public ActivityTimestamps? Timestamps { get; set; }

    /// <summary>
    /// Gets or sets the application ID for the activity.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Snowflake? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the details of the activity.
    /// </summary>
    [JsonPropertyName("details")]
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the state of the activity.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the emoji used for custom status.
    /// </summary>
    [JsonPropertyName("emoji")]
    public ActivityEmoji? Emoji { get; set; }

    /// <summary>
    /// Gets or sets the party information.
    /// </summary>
    [JsonPropertyName("party")]
    public ActivityParty? Party { get; set; }

    /// <summary>
    /// Gets or sets the assets for the activity.
    /// </summary>
    [JsonPropertyName("assets")]
    public ActivityAssets? Assets { get; set; }

    /// <summary>
    /// Gets or sets the secrets for joining and spectating.
    /// </summary>
    [JsonPropertyName("secrets")]
    public ActivitySecrets? Secrets { get; set; }

    /// <summary>
    /// Gets or sets whether the activity is an instanced game session.
    /// </summary>
    [JsonPropertyName("instance")]
    public bool? Instance { get; set; }

    /// <summary>
    /// Gets or sets the flags for the activity.
    /// </summary>
    [JsonPropertyName("flags")]
    public int? Flags { get; set; }

    /// <summary>
    /// Gets or sets the button labels for the activity (max 2). Discord returns this as an array of strings.
    /// </summary>
    [JsonPropertyName("buttons")]
    public string[]? Buttons { get; set; }
}
