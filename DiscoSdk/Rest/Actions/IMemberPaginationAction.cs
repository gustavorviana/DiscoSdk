using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving guild members.
/// </summary>
public interface IMemberPaginationAction : IPaginationAction<IMember, IMemberPaginationAction>
{
	// TODO: Add pagination methods (after, limit, etc.)
}

