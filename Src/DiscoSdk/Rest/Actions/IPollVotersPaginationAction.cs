using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving poll voters.
/// </summary>
public interface IPollVotersPaginationAction : IPaginationAction<IUser, IPollVotersPaginationAction>
{
    IPollVotersPaginationAction After(Snowflake userId);
}