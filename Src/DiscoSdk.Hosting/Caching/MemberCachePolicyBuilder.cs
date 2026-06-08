using DiscoSdk.Caching;
using DiscoSdk.Hosting.Caching.Policies;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Caching;

/// <summary>
/// Composes member cache policies declaratively. Each <c>Include*</c> call adds a criterion; the
/// final decision is combined using the <see cref="PolicyMode"/> supplied at construction
/// (<see cref="PolicyMode.All"/> by default — the safer, more restrictive option).
/// </summary>
/// <remarks>
/// Nested groups are built either by passing a pre-built <see cref="IMemberCachePolicy"/> /
/// <see cref="MemberCachePolicyBuilder"/> to <see cref="Include(IMemberCachePolicy)"/> /
/// <see cref="Include(MemberCachePolicyBuilder)"/>, or by configuring an inline subgroup with
/// <see cref="IncludeAny"/> / <see cref="IncludeAll"/>. The builder is single-use: calling
/// <see cref="Build"/> finalizes it and any further mutation throws.
/// </remarks>
public sealed class MemberCachePolicyBuilder
{
    private readonly List<IMemberCachePolicy> _criteria = [];
    private readonly PolicyMode _mode;
    private bool _built;

    /// <summary>
    /// Creates a new builder. The supplied <paramref name="mode"/> controls how criteria added to
    /// this builder are combined (default <see cref="PolicyMode.All"/>).
    /// </summary>
    /// <param name="mode">How to combine added criteria.</param>
    public MemberCachePolicyBuilder(PolicyMode mode = PolicyMode.All)
    {
        _mode = mode;
    }

    /// <summary>Adds the guild owner as a matching criterion.</summary>
    public MemberCachePolicyBuilder IncludeOwner() => Add(OwnerPolicy.Instance);

    /// <summary>Adds members currently in a voice channel as a matching criterion.</summary>
    public MemberCachePolicyBuilder IncludeVoice() => Add(VoicePolicy.Instance);

    /// <summary>Adds members whose presence is not offline as a matching criterion.</summary>
    public MemberCachePolicyBuilder IncludeOnline() => Add(OnlinePolicy.Instance);

    /// <summary>Adds members currently boosting the guild as a matching criterion.</summary>
    public MemberCachePolicyBuilder IncludeBoosters() => Add(BoosterPolicy.Instance);

    /// <summary>Adds members still pending membership screening as a matching criterion.</summary>
    public MemberCachePolicyBuilder IncludePending() => Add(PendingPolicy.Instance);

    /// <summary>
    /// Adds members holding at least one of the supplied roles as a matching criterion.
    /// </summary>
    /// <param name="roleIds">The role IDs to consider.</param>
    public MemberCachePolicyBuilder IncludeRoles(params Snowflake[] roleIds)
    {
        ArgumentNullException.ThrowIfNull(roleIds);
        return Add(new RolesPolicy(roleIds));
    }

    /// <summary>
    /// Adds members satisfying the supplied predicate as a matching criterion.
    /// </summary>
    /// <param name="predicate">The user predicate to evaluate.</param>
    public MemberCachePolicyBuilder IncludeWhere(Func<IMember, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return Add(new PredicatePolicy(predicate));
    }

    /// <summary>Includes an existing policy as a nested criterion.</summary>
    /// <param name="policy">The pre-built policy to include.</param>
    public MemberCachePolicyBuilder Include(IMemberCachePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        return Add(policy);
    }

    /// <summary>Includes a builder as a nested criterion. The builder is finalized internally.</summary>
    /// <param name="builder">The nested builder to include.</param>
    public MemberCachePolicyBuilder Include(MemberCachePolicyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return Include(builder.Build());
    }

    /// <summary>
    /// Configures an inline subgroup combined with <see cref="PolicyMode.Any"/> and includes it as
    /// a nested criterion of this builder.
    /// </summary>
    /// <param name="configure">Configuration action for the subgroup.</param>
    public MemberCachePolicyBuilder IncludeAny(Action<MemberCachePolicyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var sub = new MemberCachePolicyBuilder(PolicyMode.Any);
        configure(sub);
        return Include(sub.Build());
    }

    /// <summary>
    /// Configures an inline subgroup combined with <see cref="PolicyMode.All"/> and includes it as
    /// a nested criterion of this builder.
    /// </summary>
    /// <param name="configure">Configuration action for the subgroup.</param>
    public MemberCachePolicyBuilder IncludeAll(Action<MemberCachePolicyBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var sub = new MemberCachePolicyBuilder(PolicyMode.All);
        configure(sub);
        return Include(sub.Build());
    }

    /// <summary>Finalizes the builder and returns the composed policy.</summary>
    public IMemberCachePolicy Build()
    {
        if (_built)
            throw new InvalidOperationException("This builder has already been built.");

        _built = true;
        return new GroupPolicy(_criteria, _mode);
    }

    private MemberCachePolicyBuilder Add(IMemberCachePolicy policy)
    {
        if (_built)
            throw new InvalidOperationException("Cannot mutate a builder after Build has been called.");
        _criteria.Add(policy);
        return this;
    }
}
