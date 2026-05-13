namespace DiscoSdk.Models.AutoModeration;

/// <summary>
/// Read-only view of the metadata attached to an <see cref="IAutoModerationAction"/>.
/// </summary>
public interface IAutoModerationActionMetadata
{
	/// <summary>Channel to which matched content should be logged (SEND_ALERT_MESSAGE).</summary>
	Snowflake? ChannelId { get; }

	/// <summary>Timeout duration in seconds (TIMEOUT, max 2419200 — 4 weeks).</summary>
	int? DurationSeconds { get; }

	/// <summary>Additional explanation shown to the member when their message is blocked (BLOCK_MESSAGE, max 150 chars).</summary>
	string? CustomMessage { get; }
}
