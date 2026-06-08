using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild prune surface — every operation that targets <c>/guilds/:id/prune*</c>.
/// </summary>
public interface IGuildPrune
{
    /// <summary>Builds a deferred REST action that returns how many members would be pruned.</summary>
    /// <param name="days">Days of inactivity (1-30).</param>
    /// <param name="includeRoles">Role ids to include in the count.</param>
    IRestAction<int> Count(int days, params Snowflake[] includeRoles);

    /// <summary>Builds a deferred REST action that begins a prune operation.</summary>
    /// <param name="days">Days of inactivity (1-30).</param>
    /// <param name="includeRoles">Role ids to include in the prune.</param>
    IReasonedRestAction<int> Begin(int days, params Snowflake[] includeRoles);
}
