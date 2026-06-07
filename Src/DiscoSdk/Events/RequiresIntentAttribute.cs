namespace DiscoSdk.Events;

/// <summary>
/// Declares that a handler marker interface needs at least one of the listed gateway intents
/// to receive events. Stacking the attribute multiple times means ALL groups must be satisfied
/// (AND across attributes, OR within a single attribute).
/// </summary>
/// <remarks>
/// The intent set is checked at handler-registration time by the dispatcher: if none of the
/// required bits are set on the client, a one-shot Warning log surfaces the gap before the bot
/// silently fails to receive the event Discord won't deliver. The attribute is purely advisory —
/// the handler still registers and is callable in tests; the runtime gate is Discord itself.
/// </remarks>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public sealed class RequiresIntentAttribute : Attribute
{
    /// <summary>
    /// Bitwise-OR'd intents; at least one bit must be set on the client for the handler to be reachable.
    /// </summary>
    public DiscordIntent AnyOf { get; }

    public RequiresIntentAttribute(DiscordIntent anyOf)
    {
        AnyOf = anyOf;
    }
}
