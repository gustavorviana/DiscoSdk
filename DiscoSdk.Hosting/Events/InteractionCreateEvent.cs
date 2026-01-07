using DiscoSdk.Events;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Represents the event data for when an interaction is created (e.g., slash command).
/// </summary>
internal class InteractionCreateEvent(DiscordClient client, InteractionHandle handle, Interaction interaction) : IInteractionCreateEvent
{
    public Interaction Interaction => interaction;


    public async Task DeferAsync(bool ephemeral = true, CancellationToken cancellationToken = default)
    {
        if (handle.IsDeferred)
            return;

        handle.IsDeferred = true;

        await client.InteractionClient.DeferAsync(handle, ephemeral, cancellationToken);
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        return new SendMessageRestAction(client, handle, Interaction.ChannelId!, content);
    }

    public async Task ReplyModal(ModalData modalData, CancellationToken cancellationToken = default)
    {
        if (handle.IsDeferred)
            throw new InvalidOperationException("Cannot respond with modal after deferring the interaction.");

        await client.InteractionClient.RespondWithModalAsync(handle, modalData, cancellationToken);
    }
}

