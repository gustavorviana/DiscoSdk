using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Contexts.Interactions;

public interface IInteractionContext : IContext
{
    IInteraction Interaction { get; }

    IRestAction Defer(bool ephemeral = true);
    ISendMessageRestAction Reply(string? content = null);
    IReplyModalRestAction ReplyModal();

    /// <summary>
    /// Responds to this interaction by launching the bot's embedded Discord Activity in the
    /// invoking user's voice channel (callback type 12 <c>LAUNCH_ACTIVITY</c>).
    /// </summary>
    /// <remarks>
    /// Only works for applications flagged as <c>EMBEDDED</c> in the Discord Developer Portal.
    /// The invoking user must be connected to a voice channel; Discord opens the activity there.
    /// </remarks>
    ILaunchActivityRestAction LaunchActivity();
}