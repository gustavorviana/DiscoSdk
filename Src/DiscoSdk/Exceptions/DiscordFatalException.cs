namespace DiscoSdk.Exceptions;

/// <summary>
/// Thrown out of <see cref="IDiscordClient.WaitShutdownAsync(System.Threading.CancellationToken)"/>
/// (and the timeout overload) and <see cref="IDiscordClient.WaitReadyAsync(System.Threading.CancellationToken)"/>
/// when the client terminated because of an unrecoverable error — a shard hit an exception the
/// recovery path can't handle (non-transport failure, internal bug, or a future close-code-fatal
/// once Stage 2 wires those).
/// </summary>
/// <remarks>
/// The original cause is preserved as <see cref="System.Exception.InnerException"/>. Catch this
/// in the bot entry point to make a clean exit (or to escalate to the process supervisor) instead
/// of silently returning from <c>WaitShutdownAsync</c> as if shutdown was graceful.
/// </remarks>
public sealed class DiscordFatalException : DiscoException
{
    public DiscordFatalException(string message, Exception inner) : base(message, inner) { }
}
