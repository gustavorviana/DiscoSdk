using DiscoSdk.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DiscoSdk.Hosting.Gateway.Events;

/// <summary>
/// Per-process, per-handler-interface gate that emits the "registered without required intent"
/// warning at most once. Discord silently never delivers the event when the bot didn't opt into
/// the matching intent, so a handler that looks correctly registered can sit dead forever; the
/// gate logs the misconfig the first time the host registers such a handler.
/// </summary>
internal static class EventHandlerIntentWarnTracker
{
    private static readonly ConcurrentDictionary<Type, byte> Warned = new();

    /// <summary>
    /// Checks <paramref name="handlerInterfaceType"/> for <see cref="RequiresIntentAttribute"/>
    /// declarations and logs once if any required group is not satisfied by <paramref name="intents"/>.
    /// Multiple attributes are AND-joined; bits within a single attribute are OR-joined.
    /// </summary>
    public static void CheckAndWarn(ILogger logger, Type handlerInterfaceType, DiscordIntent intents)
    {
        var attrs = (RequiresIntentAttribute[])handlerInterfaceType
            .GetCustomAttributes(typeof(RequiresIntentAttribute), inherit: true);
        if (attrs.Length == 0)
            return;

        DiscordIntent missingGroups = DiscordIntent.None;
        foreach (var attr in attrs)
        {
            if ((intents & attr.AnyOf) == DiscordIntent.None)
                missingGroups |= attr.AnyOf;
        }

        if (missingGroups == DiscordIntent.None)
            return;

        if (!Warned.TryAdd(handlerInterfaceType, 1))
            return;

        logger.LogWarning(
            "Handler registered for {Handler} but the client is missing every intent in {MissingGroups} — Discord will not deliver this event and the handler will sit dormant. Add the intent on DiscordClientBuilder.WithIntents and (if privileged) enable it in the Developer Portal. This warning logs once per handler type per process.",
            handlerInterfaceType.Name,
            missingGroups);
    }

    internal static void ResetForTests() => Warned.Clear();
}
