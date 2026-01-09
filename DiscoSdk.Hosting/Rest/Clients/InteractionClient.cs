using DiscoSdk.Hosting.Events;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Requests;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for responding to Discord interactions (slash commands, buttons, etc.).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InteractionClient"/> class.
/// </remarks>
internal class InteractionClient(DiscordClient discordClient)
{
    private IDiscordRestClientBase Client => discordClient._client;

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
        var id = interaction.IsDeferred ? discordClient.ApplicationId : interaction.Id.ToString();
        var path = $"webhooks/{id}/{interaction.Token}";
        await Client.SendJsonAsync<object>(path, HttpMethod.Post, request, ct);
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
		await Client.SendJsonAsync<object>(path, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Gets the original interaction response message.
	/// </summary>
	/// <param name="interaction">The interaction handle containing the interaction ID and token.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The original message.</returns>
	public async Task<Message> GetOriginalResponseAsync(InteractionHandle interaction, CancellationToken cancellationToken = default)
	{
		var path = $"webhooks/{discordClient.ApplicationId}/{interaction.Token}/messages/@original";
		return await Client.SendJsonAsync<Message>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Edits the original interaction response message.
	/// </summary>
	/// <param name="interaction">The interaction handle containing the interaction ID and token.</param>
	/// <param name="request">The message edit request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The edited message.</returns>
	public async Task<Message> EditOriginalResponseAsync(InteractionHandle interaction, MessageEditRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var path = $"webhooks/{discordClient.ApplicationId}/{interaction.Token}/messages/@original";
		return await Client.SendJsonAsync<Message>(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Deletes the original interaction response message.
	/// </summary>
	/// <param name="interaction">The interaction handle containing the interaction ID and token.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task DeleteOriginalResponseAsync(InteractionHandle interaction, CancellationToken cancellationToken = default)
	{
		var path = $"webhooks/{discordClient.ApplicationId}/{interaction.Token}/messages/@original";
		await Client.SendNoContentAsync(path, HttpMethod.Delete, cancellationToken);
	}

	public enum AcknowledgeType
	{
		ModalSubmit,
		Ephemeral,
		NonEphemeral
	}
}