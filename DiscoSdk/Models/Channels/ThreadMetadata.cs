using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents thread metadata.
/// </summary>
public class ThreadMetadata
{
    /// <summary>
    /// Gets or sets a value indicating whether the thread is archived.
    /// </summary>
    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    /// <summary>
    /// Gets or sets the duration in minutes to automatically archive the thread after recent activity.
    /// </summary>
    [JsonPropertyName("auto_archive_duration")]
    public int AutoArchiveDuration { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the thread's archive status was last changed.
    /// </summary>
    [JsonPropertyName("archive_timestamp")]
    public string ArchiveTimestamp { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the thread is locked.
    /// </summary>
    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether non-moderators can add other non-moderators to a thread.
    /// </summary>
    [JsonPropertyName("invitable")]
    public bool? Invitable { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the thread was created.
    /// </summary>
    [JsonPropertyName("create_timestamp")]
    public string? CreateTimestamp { get; set; }
}
