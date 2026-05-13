using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving guild members.
/// </summary>
public interface IMemberPaginationAction : IPaginationAction<IMember, IMemberPaginationAction>
{
	/// <summary>
	/// Gets members after this user ID.
	/// </summary>
	/// <param name="userId">The user ID to get members after.</param>
	/// <returns>The current <see cref="IMemberPaginationAction"/> instance.</returns>
	IMemberPaginationAction After(Snowflake userId);
}
