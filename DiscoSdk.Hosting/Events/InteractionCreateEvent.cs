using DiscoSdk.Events;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Represents the event data for when an interaction is created (e.g., slash command).
/// </summary>
internal class InteractionCreateEvent(IInteraction interaction, IDiscordClient client) : IInteractionCreateEvent
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