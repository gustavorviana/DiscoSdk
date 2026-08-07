namespace DiscoSdk;

/// <summary>
/// Opt-in: dispatch this handler / command without awaiting it. The next event on the shard runs
/// immediately, ordering is no longer guaranteed, and exceptions are logged and swallowed.
/// Apply on a class or a method. See the <c>Event Dispatch</c> wiki page for trade-offs and
/// when this is safe.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class FireAndForgetAttribute : Attribute
{
    /// <summary>
    /// When <c>true</c>, the dispatcher breaks the current handler chain after invoking this
    /// handler — any handler that comes after in the same chain is skipped. Default is <c>false</c>.
    /// Scoped to the current chain only.
    /// </summary>
    public bool SkipNextExecutions { get; set; }
}
