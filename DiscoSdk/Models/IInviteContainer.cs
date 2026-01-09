using DiscoSdk.Models.Channels;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

public interface IInviteContainer : IGuildChannelBase
{
    ICreateInviteAction CreateInvite();

    IRestAction<IReadOnlyList<IInvite>> RetrieveInvites();
}