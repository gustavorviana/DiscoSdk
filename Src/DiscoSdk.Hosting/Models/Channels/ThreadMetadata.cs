using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Channels;

/// <inheritdoc cref="IThreadMetadata"/>
internal class ThreadMetadata : IThreadMetadata
{
	[JsonPropertyName("archived")]
	public bool Archived { get; init; }

	[JsonPropertyName("auto_archive_duration")]
	public ThreadAutoArchiveDuration? AutoArchiveDuration { get; init; }

	[JsonPropertyName("archive_timestamp")]
	public DateTimeOffset? ArchiveTimestamp { get; init; }

	[JsonPropertyName("locked")]
	public bool Locked { get; init; }

	[JsonPropertyName("invitable")]
	public bool? Invitable { get; init; }

	[JsonPropertyName("create_timestamp")]
	public DateTimeOffset? CreateTimestamp { get; init; }
}
