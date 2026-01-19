using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Contexts;

internal class InteractionContext(IInteraction interaction, IDiscordClient client) : IInteractionContext
{
    public IInteraction Interaction => interaction;

    public IDiscordClient Client => client;

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
