namespace DiscoSdk.Models.Activities;

/// <summary>
/// Read-only view of <see cref="ActivityEmoji"/> — the emoji attached to a custom-status
/// activity.
/// </summary>
public interface IActivityEmoji
{
    /// <summary>The emoji name. For Unicode emoji this is the literal character.</summary>
    string Name { get; }

    /// <summary>The emoji id when it is a custom server emoji, otherwise <c>null</c>.</summary>
    Snowflake? Id { get; }

    /// <summary>
    /// Whether the emoji animates. Defaults to <c>false</c> when Discord omits the field
    /// (which is also the default for Unicode emoji that never animate).
    /// </summary>
    bool Animated { get; }
}
