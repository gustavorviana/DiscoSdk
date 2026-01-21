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
}