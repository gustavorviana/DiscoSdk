using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving reaction users.
/// </summary>
public interface IReactionPaginationAction : IPaginationAction<User, IReactionPaginationAction>
{
    IReactionPaginationAction After(DiscordId userId);
}