using DiscoSdk.Hosting.Events;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IInteraction"/> for a <see cref="Interaction"/> instance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InteractionWrapper"/> class.
/// </remarks>
/// <param name="interaction">The interaction instance to wrap.</param>
/// <param name="client">The Discord client for performing operations.</param>
/// <param name="handle">The interaction handle for responding to the interaction.</param>
internal class InteractionWrapper(Interaction interaction,
    DiscordClient client,
    InteractionHandle handle,
    ITextBasedChannel? channel,
    IMember? member) : IInteraction
{
    private readonly Interaction _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
    private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly InteractionHandle _handle = handle ?? throw new ArgumentNullException(nameof(handle));

    public DiscordId Id => _interaction.Id;
    public DiscordId ApplicationId => _interaction.ApplicationId;
    public InteractionType Type => _interaction.Type;
    public IInteractionData? Data { get; } = interaction.Data is not null && channel is not null ? new InteractionDataWrapper(interaction.Data, channel, client) : null;
    public IGuild? Guild => channel is IGuildChannel gChannel ? gChannel.Guild : null;
    public ITextBasedChannel? Channel => channel;
    public IMember? Member => member;
    public IUser User { get; } = interaction.User is not null ? new UserWrapper(interaction.User) : throw new InvalidOperationException("Interaction user is null.");
    public string Token => _interaction.Token;
    public int Version => _interaction.Version;
    public IMessage? Message { get; } = interaction.Message is not null ? new MessageWrapper(channel!, interaction.Message, client, null) : null;
    public string? Locale => _interaction.Locale;
    public string? GuildLocale => _interaction.GuildLocale;

    public IRestAction Defer(bool ephemeral = false)
    {
        return new RestAction(async cancellationToken =>
        {
            if (_handle.IsDeferred)
                return;

            _handle.IsDeferred = true;
            await _client.InteractionClient.DeferAsync(_handle, ephemeral, cancellationToken);
        });
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        if (!_interaction.ChannelId.HasValue)
            throw new InvalidOperationException("Cannot reply to interaction without a channel ID.");

        return new SendMessageRestAction(_client, _handle, channel!, content);
    }

    public IRestAction ReplyModal(ModalData modalData)
    {
        ArgumentNullException.ThrowIfNull(modalData);

        if (_handle.IsDeferred)
            throw new InvalidOperationException("Cannot respond with modal after deferring the interaction.");

        return new RestAction(cancellationToken => _client.InteractionClient.RespondWithModalAsync(_handle, modalData, cancellationToken));
    }

    public IEditMessageRestAction Edit()
    {
        if (!_interaction.ChannelId.HasValue)
            throw new InvalidOperationException("Cannot edit interaction response without a channel ID.");

        return new EditMessageRestAction(_client, channel!, default, _handle);
    }

    public ISendMessageRestAction FollowUp(string? content = null)
    {
        if (!_interaction.ChannelId.HasValue)
            throw new InvalidOperationException("Cannot send follow-up to interaction without a channel ID.");

        return new SendMessageRestAction(_client, _handle, channel!, content);
    }

    public IRestAction Delete()
    {
        return new RestAction(cancellationToken => _client.InteractionClient.DeleteOriginalResponseAsync(_handle, cancellationToken));
    }
}