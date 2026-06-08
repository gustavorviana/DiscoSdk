using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>
/// Combines multiple inner policies using the configured <see cref="PolicyMode"/>. Empty groups
/// evaluate to <c>true</c> when the mode is <see cref="PolicyMode.All"/> (vacuous truth) and to
/// <c>false</c> when the mode is <see cref="PolicyMode.Any"/>.
/// </summary>
internal sealed class GroupPolicy : IMemberCachePolicy
{
    private readonly IMemberCachePolicy[] _children;
    private readonly PolicyMode _mode;

    public GroupPolicy(IReadOnlyList<IMemberCachePolicy> children, PolicyMode mode)
    {
        ArgumentNullException.ThrowIfNull(children);
        _children = [.. children];
        _mode = mode;
    }

    public bool ShouldCache(IMember member)
    {
        if (_mode == PolicyMode.All)
        {
            foreach (var child in _children)
            {
                if (!child.ShouldCache(member))
                    return false;
            }
            return true;
        }

        foreach (var child in _children)
        {
            if (child.ShouldCache(member))
                return true;
        }
        return false;
    }
}
