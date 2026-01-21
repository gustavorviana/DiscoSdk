using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for responding to Discord interactions (slash commands, buttons, etc.).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InteractionClient"/> class.
/// </remarks>
internal class InteractionClient(DiscordClient discordClient)
{
    private IDiscordRestClient Client => discordClient.HttpClient;

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

    public Task FollowUpAsync(InteractionHandle interaction,
        ExecuteWebhookRequest request,
        IReadOnlyList<MessageFile>? files = null,
        CancellationToken cancellationToken = default)
    {
        var id = interaction.GetDeferredId(discordClient.ApplicationId);
        var path = $"webhooks/{id}/{interaction.Token}";

        if (files == null || files.Count == 0)
            return Client.SendAsync<object>(path, HttpMethod.Post, request, cancellationToken);

        return Client.SendMultipartAsync<Message>(path, HttpMethod.Patch, request, files, cancellationToken);
    }
    public async Task RespondWithModalAsync(InteractionHandle interaction, ModalData modalData, CancellationToken cancellationToken = default)
    {
        await SendCallbackAsync(interaction, modalData, InteractionCallbackType.Modal, cancellationToken);
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
        object? data,
        InteractionCallbackType type,
        CancellationToken cancellationToken)
    {
        var request = new InteractionCallbackRequest
        {
            Type = type,
            Data = data
        };

        var path = $"interactions/{interaction.Id}/{interaction.Token}/callback";
        await Client.SendAsync(path, HttpMethod.Post, request, cancellationToken);
    }

    public enum AcknowledgeType
    {
        ModalSubmit,
        Ephemeral,
        NonEphemeral
    }
}