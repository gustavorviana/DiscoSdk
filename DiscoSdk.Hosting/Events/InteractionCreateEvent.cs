using DiscoSdk.Events;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Represents the event data for when an interaction is created (e.g., slash command).
/// </summary>
internal class InteractionCreateEvent(DiscordClient client, InteractionHandle handle, IInteraction interaction) : IInteractionCreateEvent
{
    public IInteraction Interaction => interaction;


    public async Task DeferAsync(bool ephemeral = true, CancellationToken cancellationToken = default)
    {
        if (handle.IsDeferred)
            return;

        handle.IsDeferred = true;

        await client.InteractionClient.DeferAsync(handle, ephemeral, cancellationToken);
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        return Interaction.Reply(content);
    }

    public IRestAction ReplyModal(ModalData modalData)
    {
        return Interaction.ReplyModal(modalData);
    }
}

