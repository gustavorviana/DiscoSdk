using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Actions.Messages;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages;

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
    internal InteractionHandle Handle => handle;

    public Snowflake Id => _interaction.Id;
    public Snowflake ApplicationId => _interaction.ApplicationId;
    public InteractionType Type => _interaction.Type;
    public IInteractionData? Data { get; } = interaction.Data is not null && channel is not null ? new InteractionDataWrapper(client, interaction.Data, channel) : null;
    public IGuild? Guild => channel is IGuildChannel gChannel ? gChannel.Guild : null;
    public ITextBasedChannel Channel => channel;
    public IMember? Member => member;
    public IUser? User { get; } = interaction.User is not null ? new UserWrapper(client, interaction.User) : null;
    public string Token => _interaction.Token;
    public int Version => _interaction.Version;
    public IMessage? Message { get; } = interaction.Message is not null ? new MessageWrapper(client, channel!, interaction.Message, null) : null;
    public string? Locale => _interaction.Locale;
    public string? GuildLocale => _interaction.GuildLocale;

    public IRestAction Defer(bool ephemeral = false)
    {
        return RestAction.Create(async cancellationToken =>
        {
            if (handle.IsDeferred)
                return;

            handle.IsDeferred = true;
            await _client.InteractionClient.DeferAsync(handle, ephemeral, cancellationToken);
        });
    }

    public ISendMessageRestAction Reply(string? content = null)
    {
        if (!_interaction.ChannelId.HasValue)
            throw new InvalidOperationException("Cannot reply to interaction without a channel ID.");

        return new SendMessageRestAction(_client, handle, channel!, content);
    }

    public IReplyModalRestAction ReplyModal()
    {
        return new ReplyModalRestAction(_client, handle);
    }

    public IEditMessageRestAction Edit()
    {
        if (!_interaction.ChannelId.HasValue)
            throw new InvalidOperationException("Cannot edit interaction response without a channel ID.");

        return new EditMessageRestAction(_client, channel!, default, handle);
    }

    public IRestAction Delete()
    {
        return RestAction.Create(cancellationToken => new WebhookMessageClient(client.HttpClient).
        DeleteOriginalResponseAsync(Handle.WithAppId(_client.ApplicationId),
            cancellationToken));
    }
}