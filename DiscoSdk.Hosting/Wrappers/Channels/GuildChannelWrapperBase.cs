using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Utils;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Base class for guild channel wrappers that provides common implementations for IGuildChannelBase, IPermissionContainer, etc.
/// </summary>
internal abstract class GuildChannelWrapperBase(Channel channel, IGuild guild, DiscordClient client) 
    : ChannelWrapper(channel, client), IGuildChannel
{
    public IGuild Guild => guild;
    public bool Nsfw => _channel.Nsfw ?? false;
    public int Position => _channel.Position ?? 0;

    public IRestAction<IMember[]> GetMembers()
    {
        return RestAction<IMember[]>.Create(async cancellationToken =>
        {
            var guildMembers = await Guild.GetMembers().ExecuteAsync(cancellationToken);
            var permissionContainer = GetPermissionContainer();
            
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

    public DiscordPermission GetPermission(IMember member)
    {
        return new ChannelPermissionCalculator(guild, GetPermissionContainer()).GetPermission(member);
    }
}