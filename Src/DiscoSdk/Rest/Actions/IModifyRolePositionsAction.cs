using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Bulk role-reorder builder backing <c>PATCH /guilds/{guild.id}/roles</c>. Each <c>Move</c>
/// call queues one (roleId, newPosition) tuple; roles you never <c>Move</c> keep their current
/// position. Discord rebalances neighbours automatically. The endpoint accepts an audit-log
/// reason via the <see cref="IRestActionWithReason{TSelf}"/> contract.
/// </summary>
public interface IModifyRolePositionsAction
    : IRestAction<IReadOnlyList<IRole>>, IRestActionWithReason<IModifyRolePositionsAction>
{
    /// <summary>
    /// Queues a role to be moved to <paramref name="position"/>. <paramref name="position"/>
    /// may be <c>null</c> to clear an existing pending position, and <paramref name="lockPermissions"/>
    /// pins channel permission overwrites to match the new position when true. Calling
    /// <c>Move</c> twice for the same role keeps the last value.
    /// </summary>
    IModifyRolePositionsAction Move(Snowflake roleId, int? position, bool? lockPermissions = null);
}
