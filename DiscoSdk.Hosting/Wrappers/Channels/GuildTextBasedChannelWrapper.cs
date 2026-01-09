using DiscoSdk.Hosting.Utils;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Wrappers.Channels;

/// <summary>
/// Base class for guild text-based channel wrappers that combines TextBasedChannelWrapper and GuildChannelWrapperBase functionality.
/// </summary>
internal abstract class GuildTextBasedChannelWrapper(Channel channel, IGuild guild, DiscordClient client)
    : TextBasedChannelWrapper(channel, client), IGuildChannelBase
{
    public IGuild Guild => guild;

    public bool Nsfw => channel.Nsfw == true;

    public DiscordPermission GetPermission(IMember member)
    {
        return new ChannelPermissionCalculator(guild, GetPermissionContainer()).GetPermission(member);
    }
}