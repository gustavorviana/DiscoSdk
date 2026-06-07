namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Mix-in implemented by every mutating REST action that supports the Discord audit-log reason
/// header. <typeparamref name="TSelf"/> uses CRTP so chained calls preserve the concrete builder
/// type and the rest of its fluent surface remains visible.
/// </summary>
/// <remarks>
/// <para>
/// Calling <see cref="WithReason"/> stores the reason on the builder; it is only emitted on the
/// <c>X-Audit-Log-Reason</c> request header when the terminal <c>ExecuteAsync</c> fires. Replaying
/// the same builder with a different reason between executes is supported — the most recently set
/// value wins.
/// </para>
/// <para>
/// Validation (non-empty, ≤ 512 chars) lives in <see cref="AuditLogReason"/>. Invalid input
/// throws <see cref="System.ArgumentException"/> at setter time so the call site, not the network,
/// flags the bad reason.
/// </para>
/// </remarks>
public interface IRestActionWithReason<out TSelf>
{
    /// <summary>
    /// Attaches a moderation reason to be sent as <c>X-Audit-Log-Reason</c> on the next
    /// <c>ExecuteAsync</c>.
    /// </summary>
    /// <param name="reason">Free-form text, 1–512 chars. Discord stores it verbatim against the audit-log entry.</param>
    /// <returns>The same builder instance — chain further setters or call <c>ExecuteAsync</c>.</returns>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="reason"/> is null, empty, whitespace, or exceeds <see cref="AuditLogReason.MaxLength"/>.
    /// </exception>
    TSelf WithReason(string reason);
}

/// <summary>
/// Audit-loggable counterpart of <see cref="IRestAction"/>. Used for bare mutating endpoints that
/// have no other configuration knobs (kick, unban, role add / remove, etc.) — the only thing you
/// can configure on them is the reason.
/// </summary>
public interface IReasonedRestAction : IRestAction, IRestActionWithReason<IReasonedRestAction>
{
}

/// <summary>
/// Audit-loggable counterpart of <see cref="IRestAction{T}"/>. Used for bare mutating endpoints
/// that return a payload — same shape as <see cref="IReasonedRestAction"/>, just with a result.
/// </summary>
public interface IReasonedRestAction<T> : IRestAction<T>, IRestActionWithReason<IReasonedRestAction<T>>
{
}
