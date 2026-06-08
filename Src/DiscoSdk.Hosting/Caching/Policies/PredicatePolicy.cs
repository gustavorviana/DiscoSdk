using DiscoSdk.Caching;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching.Policies;

/// <summary>Delegates the decision to a user-supplied predicate.</summary>
internal sealed class PredicatePolicy : IMemberCachePolicy
{
    private readonly Func<IMember, bool> _predicate;

    public PredicatePolicy(Func<IMember, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _predicate = predicate;
    }

    public bool ShouldCache(IMember member) => _predicate(member);
}
