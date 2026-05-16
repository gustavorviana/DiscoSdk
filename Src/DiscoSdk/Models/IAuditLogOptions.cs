namespace DiscoSdk.Models;

/// <summary>
/// Action-type-specific contextual fields that ride along with an <see cref="IAuditLogEntry"/>.
/// Only a subset of these is populated for any given entry.
/// </summary>
public interface IAuditLogOptions
{
	/// <summary>Days threshold used by a prune action.</summary>
	string? DeleteMemberDays { get; }

	/// <summary>Number of members removed by a prune action.</summary>
	string? MembersRemoved { get; }

	/// <summary>Channel id of the affected entity (e.g. messages bulk-deleted in this channel).</summary>
	Snowflake? ChannelId { get; }

	/// <summary>Affected entity count.</summary>
	string? Count { get; }

	/// <summary>Id of the overwritten entity for channel-permission changes.</summary>
	Snowflake? Id { get; }

	/// <summary>Type of the overwritten entity — <c>"member"</c> or <c>"role"</c>.</summary>
	string? Type { get; }

	/// <summary>Name of the role when <see cref="Type"/> is <c>"role"</c>.</summary>
	string? RoleName { get; }
}
