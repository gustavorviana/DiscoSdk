using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for banning a member from a Discord guild.
/// </summary>
public interface IBanMemberAction : IRestAction
{
	/// <summary>
	/// Sets the number of days to delete messages for (0-7).
	/// </summary>
	/// <param name="days">The number of days to delete messages for.</param>
	/// <returns>The current <see cref="IBanMemberAction"/> instance.</returns>
	IBanMemberAction SetDeleteMessageDays(int days);

	/// <summary>
	/// Sets the reason for the ban.
	/// </summary>
	/// <param name="reason">The reason for the ban.</param>
	/// <returns>The current <see cref="IBanMemberAction"/> instance.</returns>
	IBanMemberAction SetReason(string? reason);
}

