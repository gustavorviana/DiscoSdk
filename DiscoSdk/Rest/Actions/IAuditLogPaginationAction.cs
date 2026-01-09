using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving audit logs from a guild.
/// </summary>
public interface IAuditLogPaginationAction : IPaginationAction<AuditLogEntry, IAuditLogPaginationAction>
{
	/// <summary>
	/// Gets audit log entries before this entry ID.
	/// </summary>
	/// <param name="entryId">The entry ID to get audit logs before.</param>
	/// <returns>The current <see cref="IAuditLogPaginationAction"/> instance.</returns>
	IAuditLogPaginationAction Before(DiscordId entryId);

	/// <summary>
	/// Filters audit logs by the user ID who made the changes.
	/// </summary>
	/// <param name="userId">The user ID to filter by, or null to remove the filter.</param>
	/// <returns>The current <see cref="IAuditLogPaginationAction"/> instance.</returns>
	IAuditLogPaginationAction SetUserId(DiscordId? userId);

	/// <summary>
	/// Filters audit logs by the action type.
	/// </summary>
	/// <param name="actionType">The action type to filter by, or null to remove the filter.</param>
	/// <returns>The current <see cref="IAuditLogPaginationAction"/> instance.</returns>
	IAuditLogPaginationAction SetActionType(AuditLogActionType? actionType);
}

