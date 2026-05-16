using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IAuditLogChange"/>
internal class AuditLogChange : IAuditLogChange
{
	[JsonPropertyName("key")]
	public string Key { get; init; } = default!;

	[JsonPropertyName("new_value")]
	public object? NewValue { get; init; }

	[JsonPropertyName("old_value")]
	public object? OldValue { get; init; }
}
