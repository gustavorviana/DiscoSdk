using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Contexts.Interactions;

public interface IInteractionContext : IContext
{
    IInteraction Interaction { get; }

    IRestAction Defer(bool ephemeral = true);
    ISendMessageRestAction Reply(string? content = null);
    IReplyModalRestAction ReplyModal();
}