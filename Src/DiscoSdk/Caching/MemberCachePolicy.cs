namespace DiscoSdk.Caching;

/// <summary>
/// Preset member cache policies suitable for the most common scenarios.
/// </summary>
/// <remarks>
/// For advanced compositions (combining criteria, custom predicates, role filters), use
/// <see cref="MemberCachePolicyBuilder"/> instead and pass the resulting
/// <see cref="IMemberCachePolicy"/> to the client builder.
/// </remarks>
public enum MemberCachePolicy
{
    /// <summary>No members are cached. All lookups go through REST.</summary>
    None,

    /// <summary>Only the guild owner is cached.</summary>
    Owner,

    /// <summary>Only members currently in a voice channel are cached.</summary>
    Voice,

    /// <summary>Only members whose presence is anything other than offline are cached.</summary>
    Online,

    /// <summary>Every member observed by the gateway is cached.</summary>
    All
}
