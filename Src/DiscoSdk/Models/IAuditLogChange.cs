namespace DiscoSdk.Models;

/// <summary>
/// One property change inside an <see cref="IAuditLogEntry"/>.
/// </summary>
public interface IAuditLogChange
{
	/// <summary>The Discord-defined name of the changed key (e.g. <c>"name"</c>, <c>"permissions"</c>).</summary>
	string Key { get; }

	/// <summary>The post-change value (untyped — shape depends on <see cref="Key"/>).</summary>
	object? NewValue { get; }

	/// <summary>The pre-change value (untyped — shape depends on <see cref="Key"/>).</summary>
	object? OldValue { get; }
}
