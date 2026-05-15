using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Modules
{
    /// <summary>
    /// Implemented by modules that want to register application commands during the
    /// <c>CommandsUpdateWindowOpened</c> phase. The runtime calls
    /// <see cref="OnCommandsUpdateWindowOpenedAsync"/> on every registered module, then fires
    /// the public event, and finally commits every scope opened in the session in a single pass.
    /// Modules and the user event handler share the same session — opening the same target
    /// returns the same accumulator.
    /// </summary>
    public interface ICommandsUpdateWindowModule : IDiscoModule
    {
        /// <summary>
        /// Queues commands for this module via the supplied <paramref name="session"/>. The
        /// session does not expose <c>ApplyAsync</c>; the SDK commits every scope after every
        /// participant has run.
        /// </summary>
        Task OnCommandsUpdateWindowOpenedAsync(IDiscordClient discordClient, ICommandUpdateSession session);
    }
}
