using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper that implements <see cref="IReaction"/> for a <see cref="Reaction"/> instance.
/// </summary>
internal class ReactionWrapper : IReaction
{
    private readonly DiscordClient _client;
    private readonly Reaction _reaction;
    private readonly IMessage _message;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactionWrapper"/> class.
    /// </summary>
    /// <param name="reaction">The reaction instance to wrap.</param>
    /// <param name="client">The Discord client for accessing intents.</param>
    public ReactionWrapper(Reaction reaction, IMessage message, DiscordClient client)
    {
        _reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Gets the number of times this emoji has been used to react.
    /// </summary>
    public int Count => _reaction.Count;

    /// <summary>
    /// Gets a value indicating whether the current user reacted using this emoji.
    /// </summary>
    public bool Me => _reaction.Me;

    /// <summary>
    /// Gets the emoji information.
    /// </summary>
    public Emoji Emoji => _reaction.Emoji;

    /// <inheritdoc />
    public IRestAction Delete()
    {
        ValidateReactionIntent("remove reactions from");

        if (_message.Flags.HasFlag(MessageFlags.Ephemeral))
            throw EphemeralMessageException.Operation("remove reactions from");

        return RestAction.Create(cancellationToken =>
        {
            if (Me)
                return _client.MessageClient.RemoveReactionAsync(_message.Channel.Id, _message.Id, Emoji.ToString()!, cancellationToken);

            return _client.MessageClient.RemoveUserReactionAsync(_message.Channel.Id, _message.Id, Emoji.ToString()!, _message.Author.Id, cancellationToken);
        });
    }

    /// <summary>
    /// Asserts that the appropriate reaction intent (<see cref="DiscordIntent.GuildMessageReactions"/>
    /// or <see cref="DiscordIntent.DirectMessageReactions"/>) is enabled for this message's scope.
    /// </summary>
    private void ValidateReactionIntent(string operation)
    {
        var required = _message.Guild?.Id != null
            ? DiscordIntent.GuildMessageReactions
            : DiscordIntent.DirectMessageReactions;

        IntentGuard.Require(_client, required, operation);
    }
}