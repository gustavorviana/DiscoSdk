using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers.Managers;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Wrapper that implements <see cref="IGuildTextChannel"/> for a <see cref="Channel"/> instance.
/// </summary>
internal class GuildTextChannelWrapper : GuildTextBasedChannelWrapper, IGuildTextChannel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuildTextChannelWrapper"/> class.
    /// </summary>
    /// <param name="channel">The channel instance to wrap.</param>
    /// <param name="guild">The guild this channel belongs to.</param>
    /// <param name="client">The Discord client for performing operations.</param>
    public GuildTextChannelWrapper(Channel channel, IGuild guild, DiscordClient client)
        : base(channel, guild, client)
    {
    }

    /// <inheritdoc />
    public bool CanTalk(IMember member)
    {
        var permission = GetPermission(member);
        var isThread = Type is ChannelType.PrivateThread or ChannelType.PublicThread or ChannelType.AnnouncementThread;
        return permission.HasFlag(isThread ? DiscordPermission.SendMessagesInThreads : DiscordPermission.SendMessages);
    }

    /// <inheritdoc />
    public Task DeleteAllReactionsAsync(DiscordId messageId, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.DeleteAllReactionsAsync(_channel.Id, messageId, cancellationToken);
    }

    public ITextChannelManager GetManager()
    {
        return new TextChannelManagerWrapper(Id, _client.ChannelClient);
    }

    /// <inheritdoc />
    public Task RemoveReactionAsync(DiscordId messageId, Emoji emoji, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.RemoveReactionAsync(_channel.Id, messageId, emoji.ToString(), cancellationToken);
    }

    /// <inheritdoc />
    public ISendMessageRestAction SendStickers(IEnumerable<DiscordId> stickers)
    {
        return new SendMessageRestAction(_client, null, this, null).SetStickers(stickers);
    }
}