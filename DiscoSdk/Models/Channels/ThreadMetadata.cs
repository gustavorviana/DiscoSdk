using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents thread-specific metadata for a thread channel.
/// </summary>
public class ThreadMetadata
{
	/// <summary>
	/// Gets or sets whether the thread is archived.
	/// </summary>
	[JsonPropertyName("archived")]
	public bool Archived { get; set; }

	/// <summary>
	/// Gets or sets the auto-archive duration in minutes.
	/// The thread will stop showing in the channel list after this duration of inactivity.
	/// Can be set to: 60, 1440, 4320, 10080.
	/// </summary>
	[JsonPropertyName("auto_archive_duration")]
	public ThreadAutoArchiveDuration? AutoArchiveDuration { get; set; }

	/// <summary>
	/// Gets or sets the timestamp when the thread's archive status was last changed.
	/// Used for calculating recent activity.
	/// </summary>
	[JsonPropertyName("archive_timestamp")]
	public DateTimeOffset? ArchiveTimestamp { get; set; }

	/// <summary>
	/// Gets or sets whether the thread is locked.
	/// When a thread is locked, only users with MANAGE_THREADS can unarchive it.
	/// </summary>
	[JsonPropertyName("locked")]
	public bool Locked { get; set; }

	/// <summary>
	/// Gets or sets whether non-moderators can add other non-moderators to a thread.
	/// Only available on private threads.
	/// </summary>
	[JsonPropertyName("invitable")]
	public bool? Invitable { get; set; }

	/// <summary>
	/// Gets or sets the timestamp when the thread was created.
	/// Only populated for threads created after 2022-01-09.
	/// </summary>
	[JsonPropertyName("create_timestamp")]
	public DateTimeOffset? CreateTimestamp { get; set; }
}

