using DiscoSdk.Models;
using DiscoSdk.Models.Users;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving reaction users.
/// </summary>
public interface IReactionPaginationAction : IPaginationAction<User, IReactionPaginationAction>
{
    IReactionPaginationAction After(Snowflake userId);
}