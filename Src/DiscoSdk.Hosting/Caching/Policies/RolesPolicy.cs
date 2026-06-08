using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>Caches members that hold at least one of the configured role IDs.</summary>
internal sealed class RolesPolicy : IMemberCachePolicy
{
    private readonly HashSet<Snowflake> _roleIds;

    public RolesPolicy(IEnumerable<Snowflake> roleIds)
    {
        ArgumentNullException.ThrowIfNull(roleIds);
        _roleIds = [.. roleIds];
    }

    public bool ShouldCache(IMember member)
    {
        if (_roleIds.Count == 0)
            return false;

        foreach (var role in member.UnsortedRoles)
        {
            if (_roleIds.Contains(role.Id))
                return true;
        }

        return false;
    }
}
