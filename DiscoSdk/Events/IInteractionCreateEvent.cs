using DiscoSdk.Models;

namespace DiscoSdk.Events
{
    /// <summary>
    /// Represents the event data for when an interaction is created (e.g., slash command).
    /// </summary>
    public interface IInteractionCreateEvent
    {
        /// <summary>
        /// Gets or sets the interaction that was created.
        /// </summary>
        Interaction Interaction { get; }

        Task DeferAsync(bool ephemeral = true, CancellationToken cancellationToken = default);
        Task RespondAsync(string content, bool ephemeral = true, CancellationToken cancellationToken = default);
    }
}
