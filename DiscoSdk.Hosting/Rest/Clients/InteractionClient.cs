using DiscoSdk.Hosting.Events;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for responding to Discord interactions (slash commands, buttons, etc.).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InteractionClient"/> class.
/// </remarks>
/// <param name="client">The REST client base to use for requests.</param>
internal class InteractionClient(IDiscordRestClientBase client)
{
    public async Task DeferAsync(InteractionHandle interaction, bool ephemeral = false, CancellationToken cancellationToken = default)
    {
        var data = ephemeral ? new InteractionCallbackData
        {
            Flags = MessageFlags.Ephemeral,
        } : null;

        await SendCallbackAsync(interaction,
            data,
            InteractionCallbackType.DeferredChannelMessageWithSource,
            cancellationToken);
    }

    public async Task RespondAsync(InteractionHandle interaction, InteractionCallbackData data, CancellationToken cancellationToken = default)
    {
        await SendCallbackAsync(interaction, data, InteractionCallbackType.ChannelMessageWithSource, cancellationToken);
    }

    public async Task FollowUpAsync(InteractionHandle interaction, FollowUpMessageRequest request, CancellationToken ct = default)
    {
        var path = $"webhooks/{interaction.Id}/{interaction.Token}";
        await client.SendJsonAsync<object>(path, HttpMethod.Post, request, ct);
    }

    public async Task RespondWithModalAsync(InteractionHandle interaction, ModalData modalData, CancellationToken cancellationToken = default)
    {
        var data = new InteractionCallbackData
        {
            CustomId = modalData.CustomId,
            Title = modalData.Title,
            Components = modalData.Components
        };

        await SendCallbackAsync(interaction, data, InteractionCallbackType.Modal, cancellationToken);
    }

    public async Task AcknowledgeAsync(InteractionHandle interaction, AcknowledgeType type, CancellationToken cancellationToken = default)
    {
        var data = type == AcknowledgeType.ModalSubmit ? null : new InteractionCallbackData
        {
            Flags = type == AcknowledgeType.Ephemeral ? MessageFlags.Ephemeral : MessageFlags.None
        };

        var callbackType = type == AcknowledgeType.ModalSubmit
            ? InteractionCallbackType.DeferredUpdateMessage
            : InteractionCallbackType.DeferredChannelMessageWithSource;

        await SendCallbackAsync(interaction, data, callbackType, cancellationToken);
    }

    public async Task SendCallbackAsync(InteractionHandle interaction,
        InteractionCallbackData? data,
        InteractionCallbackType type,
        CancellationToken cancellationToken)
    {
        var request = new InteractionCallbackRequest
        {
            Type = type,
            Data = data
        };

        var path = $"interactions/{interaction.Id}/{interaction.Token}/callback";
        await client.SendJsonAsync<object>(path, HttpMethod.Post, request, cancellationToken);
    }

    public enum AcknowledgeType
    {
        ModalSubmit,
        Ephemeral,
        NonEphemeral
    }
}