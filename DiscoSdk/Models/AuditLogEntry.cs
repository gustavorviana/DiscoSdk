using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents an entry in a Discord guild's audit log.
/// </summary>
public class AuditLogEntry
{
	/// <summary>
	/// Gets or sets the ID of the entry.
	/// </summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>
	/// Gets or sets the ID of the affected entity (webhook, user, role, etc.).
	/// </summary>
	[JsonPropertyName("target_id")]
	public string? TargetId { get; set; }

	/// <summary>
	/// Gets or sets the ID of the user who made the changes.
	/// </summary>
	[JsonPropertyName("user_id")]
	public Snowflake? UserId { get; set; }

	/// <summary>
	/// Gets or sets the type of action that occurred.
	/// </summary>
	[JsonPropertyName("action_type")]
	public AuditLogActionType ActionType { get; set; }

	/// <summary>
	/// Gets or sets the changes made to the target_id.
	/// </summary>
	[JsonPropertyName("changes")]
	public AuditLogChange[]? Changes { get; set; }

	/// <summary>
	/// Gets or sets additional info for certain action types.
	/// </summary>
	[JsonPropertyName("options")]
	public AuditLogOptions? Options { get; set; }

	/// <summary>
	/// Gets or sets the reason for the change.
	/// </summary>
	[JsonPropertyName("reason")]
	public string? Reason { get; set; }
}

