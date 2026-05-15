namespace DiscoSdk.Exceptions;

/// <summary>
/// Central guard for intent-gated operations. Call <see cref="Require(DiscordIntent, DiscordIntent, string)"/>
/// before sending a request to Discord (or accessing data that depends on an event whose
/// intent isn't enabled) so the caller receives a clear <see cref="MissingIntentException"/>
/// instead of an opaque Discord error or silently-empty data.
/// </summary>
public static class IntentGuard
{
    /// <summary>
    /// Throws <see cref="MissingIntentException"/> if <paramref name="required"/> is not part
    /// of <paramref name="configured"/>.
    /// </summary>
    /// <param name="configured">The intents the client was built with.</param>
    /// <param name="required">The intent the operation needs.</param>
    /// <param name="operation">Human-readable verb phrase that completes <c>"Cannot ..."</c> in the error message.</param>
    public static void Require(DiscordIntent configured, DiscordIntent required, string operation)
    {
        if (!configured.HasFlag(required))
            throw new MissingIntentException(required, operation);
    }

    /// <summary>
    /// Convenience overload that reads <see cref="IDiscordClient.Intents"/> from <paramref name="client"/>.
    /// </summary>
    public static void Require(IDiscordClient client, DiscordIntent required, string operation)
    {
        ArgumentNullException.ThrowIfNull(client);
        Require(client.Intents, required, operation);
    }
}
