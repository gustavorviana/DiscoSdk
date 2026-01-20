using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Contexts;

internal class InteractionContextWrapper(DiscordClient client, InteractionWrapper interaction) : ContextWrapper(client), IInteractionContext
{
    public InteractionWrapper Interaction => interaction;

    IInteraction IInteractionContext.Interaction => Interaction;

    public IRestAction Defer(bool ephemeral = true)
    {
        return interaction.Defer(ephemeral);
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        return Interaction.Reply(content);
    }

    public IReplyModalRestAction ReplyModal()
    {
        return Interaction.ReplyModal();
    }
}