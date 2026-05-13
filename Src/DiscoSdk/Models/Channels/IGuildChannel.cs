using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// A channel that belongs to a guild and can hold member-level permissions and invites.
/// This is the broadest "real" guild channel contract — it intentionally does <b>not</b> require a
/// <see cref="IPositionableChannel.Position"/>, because threads are guild channels yet have no slot
/// in the channel list. Channels that do appear in the list implement <see cref="IStandardGuildChannel"/>.
/// </summary>
public interface IGuildChannel : IGuildChannelBase, IInviteContainer
{
    /// <summary>
    /// Gets the members that can view this channel.
    /// </summary>
    IRestAction<IMember[]> GetMembers();
}
