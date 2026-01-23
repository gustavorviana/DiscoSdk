using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a change in an audit log entry.
/// </summary>
public class AuditLogChange
{
	/// <summary>
	/// Gets or sets the name of the changed entity.
	/// </summary>
	[JsonPropertyName("key")]
	public string Key { get; set; } = default!;

	/// <summary>
	/// Gets or sets the new value of the key.
	/// </summary>
	[JsonPropertyName("new_value")]
	public object? NewValue { get; set; }

	/// <summary>
	/// Gets or sets the old value of the key.
	/// </summary>
	[JsonPropertyName("old_value")]
	public object? OldValue { get; set; }
}

