using DiscoSdk.Events;
using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Events;

/// <summary>
/// Represents the event data for when an interaction is created (e.g., slash command).
/// </summary>
public class InteractionCreateEvent(DiscordClient client, Interaction interaction) : IInteractionCreateEvent
{
    private bool _deferred = false;
    public Interaction Interaction => interaction;

    public async Task DeferAsync(bool ephemeral = true, CancellationToken cancellationToken = default)
    {
        if (_deferred)
            return;

        _deferred = true;

        await client.InteractionClient.DeferAsync(Interaction.Id, Interaction.Token, ephemeral, cancellationToken);
    }

    public async Task RespondAsync(string content, bool ephemeral = true, CancellationToken cancellationToken = default)
    {
        if (_deferred)
            await client.InteractionClient.FollowUpAsync(interaction.ApplicationId, interaction.Token, content, ephemeral, cancellationToken);
        else
            await client.InteractionClient.RespondAsync(interaction.Id, interaction.Token, content, ephemeral, cancellationToken);
    }
}

