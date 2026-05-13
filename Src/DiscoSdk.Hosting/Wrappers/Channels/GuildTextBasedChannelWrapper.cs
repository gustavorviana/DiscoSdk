using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Actions.Messages;
using DiscoSdk.Hosting.Utils;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Shared base for every guild channel that carries a text chat: text and announcement channels,
/// threads, and (via Text-in-Voice) voice and stage channels. It layers the guild-channel surface
/// (<see cref="IGuildChannel"/>: guild, permissions, members, invites) on top of
/// <see cref="TextBasedChannelWrapper"/> and implements the <see cref="IGuildMessageChannel"/> contract.
/// </summary>
internal abstract class GuildTextBasedChannelWrapper(DiscordClient client, Channel channel, IGuild guild)
    : TextBasedChannelWrapper(client, channel), IGuildMessageChannel
{
    public IGuild Guild => guild;

    public bool Nsfw => _channel.Nsfw == true;

    public int Position => _channel.Position ?? 0;

    public DiscordPermission GetPermission(IMember member)
    {
        return new ChannelPermissionCalculator(guild, GetPermissionContainer()).GetPermission(member);
    }

    public bool CanTalk(IMember member)
    {
        var permission = GetPermission(member);
        var isThread = Type is ChannelType.PrivateThread or ChannelType.PublicThread or ChannelType.AnnouncementThread;
        return permission.HasFlag(isThread ? DiscordPermission.SendMessagesInThreads : DiscordPermission.SendMessages);
    }

    public IRestAction<IMember[]> GetMembers()
    {
        return RestAction<IMember[]>.Create(async cancellationToken =>
        {
            var guildMembers = await Guild.GetMembers().ExecuteAsync(cancellationToken);

            return [.. guildMembers.Where(x => GetPermission(x).HasFlag(DiscordPermission.ViewChannel))];
        });
    }

    public ICreateInviteAction CreateInvite()
    {
        return new CreateInviteAction(_client, this);
    }

    public IRestAction<IReadOnlyList<IInvite>> RetrieveInvites()
    {
        return RestAction<IReadOnlyList<IInvite>>.Create(async cancellationToken =>
        {
            var invites = await _client.InviteClient.GetChannelInvitesAsync(_channel.Id, cancellationToken);
            return invites.Select(invite => new InviteWrapper(invite, this, _client)).Cast<IInvite>().ToList().AsReadOnly();
        });
    }

    public Task DeleteAllReactionsAsync(Snowflake messageId, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.DeleteAllReactionsAsync(_channel.Id, messageId, cancellationToken);
    }

    public Task RemoveReactionAsync(Snowflake messageId, Emoji emoji, CancellationToken cancellationToken = default)
    {
        return _client.MessageClient.RemoveReactionAsync(_channel.Id, messageId, emoji.ToString(), cancellationToken);
    }

    public ISendMessageRestAction SendStickers(IEnumerable<Snowflake> stickers)
    {
        return new SendMessageRestAction(_client, null, this, null).SetStickers(stickers);
    }
}
