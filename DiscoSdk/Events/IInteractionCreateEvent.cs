using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Events;

/// <summary>
/// Represents the event data for when an interaction is created (e.g., slash command).
/// </summary>
public interface IInteractionCreateEvent
{
    /// <summary>
    /// Gets or sets the interaction that was created.
    /// </summary>
    IInteraction Interaction { get; }

    IRestAction Defer(bool ephemeral = true);
    ISendMessageRestAction Reply(string? content = null);
    IReplyModalRestAction ReplyModal();
}