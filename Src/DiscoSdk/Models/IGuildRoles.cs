using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Per-guild role surface — every operation that targets <c>/guilds/:id/roles</c>.
/// </summary>
public interface IGuildRoles
{
    /// <summary>Builds a deferred REST action that creates a new role in this guild.</summary>
    IRoleAction Create();

    /// <summary>
    /// Builds a deferred REST action that lists every role in this guild.
    /// </summary>
    IRestAction<IReadOnlyList<IRole>> GetAll();

    /// <summary>
    /// Builds a deferred REST action that retrieves a single role by id. Uses Discord's dedicated
    /// <c>GET /guilds/{guild.id}/roles/{role.id}</c> endpoint (introduced 2024) — does not list
    /// and filter client-side. Returns <c>null</c> when the role does not exist.
    /// </summary>
    IRestAction<IRole?> Get(Snowflake roleId);

    /// <summary>
    /// Builds a deferred REST action that bulk-reorders roles in this guild
    /// (<c>PATCH /guilds/{guild.id}/roles</c>). Call <c>Move(roleId, position)</c> for each role
    /// to move; omitted roles keep their position.
    /// </summary>
    IModifyRolePositionsAction ModifyPositions();

    /// <summary>
    /// Synchronous snapshot of every role currently held in the cache for this guild. Reads come
    /// straight from the in-memory store fed by gateway events — no I/O.
    /// </summary>
    IReadOnlyList<IRole> GetCached();

    /// <summary>Synchronous count of roles currently in the cache for this guild.</summary>
    int GetCachedCount();
}
