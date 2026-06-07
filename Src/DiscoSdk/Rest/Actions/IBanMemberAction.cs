namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for banning a member from a Discord guild. The reason supplied via
/// <see cref="IRestActionWithReason{TSelf}.WithReason"/> is sent on the <c>X-Audit-Log-Reason</c>
/// header and appears verbatim against the audit-log entry Discord creates for the ban.
/// </summary>
public interface IBanMemberAction : IRestAction, IRestActionWithReason<IBanMemberAction>
{
	/// <summary>
	/// Sets the number of days to delete messages for (0-7).
	/// </summary>
	/// <param name="days">The number of days to delete messages for.</param>
	/// <returns>The current <see cref="IBanMemberAction"/> instance.</returns>
	IBanMemberAction SetDeleteMessageDays(int days);
}
