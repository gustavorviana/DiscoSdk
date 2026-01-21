using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Hosting.Rest.Actions.Messages;

/// <summary>
/// Implementation of <see cref="IEditMessageRestAction"/> for editing messages in Discord.
/// </summary>
internal class EditMessageRestAction : MessageBuilderAction<IEditMessageRestAction, IMessage>, IEditMessageRestAction
{
    private readonly DiscordClient _client;
    private readonly ITextBasedChannel _channel;
    private readonly Snowflake _messageId;
    private readonly InteractionHandle? _interactionHandle;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMessageRestAction"/> class.
    /// </summary>
    /// <param name="client">The Discord client.</param>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to edit.</param>
    public EditMessageRestAction(DiscordClient client, ITextBasedChannel channel, Snowflake messageId, InteractionHandle? interactionHandle)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionHandle = interactionHandle;
        _messageId = messageId;
        _channel = channel;

    }

    /// <inheritdoc />
    public override async Task<IMessage> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_content) && _embeds.Count == 0)
            throw new InvalidOperationException("Message must have either content or at least one embed.");

        var request = BuildEditRequest();

        Message message;
        if (_interactionHandle == null)
            message = await _client.MessageClient.EditAsync(_channel.Id, _messageId, request, _attachments, cancellationToken);
        else
            message = await new WebhookMessageClient(_client.HttpClient).EditOriginalResponseAsync(_interactionHandle.WithAppId(_client.ApplicationId), request, cancellationToken);

        return new MessageWrapper(_client, _channel, message, _interactionHandle);
    }
}