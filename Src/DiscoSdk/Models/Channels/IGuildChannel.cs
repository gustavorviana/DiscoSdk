using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

public interface IGuildChannel : IGuildChannelBase, IPositionableChannel, IInviteContainer
{
    IRestAction<IMember[]> GetMembers();
}