using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving reaction users.
/// </summary>
public interface IReactionPaginationAction : IPaginationAction<IUser, IReactionPaginationAction>
{
    IReactionPaginationAction After(Snowflake userId);
}