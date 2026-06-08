namespace DiscoSdk.Caching;

/// <summary>
/// Controls how multiple inclusion criteria inside a <see cref="MemberCachePolicyBuilder"/>
/// are combined into a single decision.
/// </summary>
public enum PolicyMode
{
    /// <summary>
    /// The member is cached only when <em>every</em> configured criterion is satisfied (logical AND).
    /// This is the default mode and the most restrictive (fail-safe) option.
    /// </summary>
    All,

    /// <summary>
    /// The member is cached when <em>any</em> configured criterion is satisfied (logical OR).
    /// </summary>
    Any
}
