using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Read-only thread-specific metadata returned alongside a thread channel.
/// </summary>
public interface IThreadMetadata
{
	/// <summary>Whether the thread is archived.</summary>
	bool Archived { get; }

	/// <summary>
	/// Auto-archive duration in minutes — the thread stops appearing in the channel list after
	/// this many minutes of inactivity. Discord accepts 60, 1440, 4320 or 10080.
	/// </summary>
	ThreadAutoArchiveDuration? AutoArchiveDuration { get; }

	/// <summary>Timestamp of the last archive-status change. Used for "recent activity" sorting.</summary>
	DateTimeOffset? ArchiveTimestamp { get; }

	/// <summary>Whether the thread is locked — only <c>MANAGE_THREADS</c> can unarchive it.</summary>
	bool Locked { get; }

	/// <summary>For private threads, whether non-moderators can add other non-moderators.</summary>
	bool? Invitable { get; }

	/// <summary>Creation timestamp — only populated for threads created after 2022-01-09.</summary>
	DateTimeOffset? CreateTimestamp { get; }
}
