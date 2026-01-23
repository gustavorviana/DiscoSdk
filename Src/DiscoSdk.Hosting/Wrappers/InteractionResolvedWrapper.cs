using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Interactions;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Hosting.Wrappers;

internal record InteractionResolvedWrapper(IReadOnlyCollection<IUser> Users,
    IReadOnlyCollection<IMember> Members,
    IReadOnlyCollection<IRole> Roles,
    IReadOnlyCollection<IChannel> Channels,
    IReadOnlyCollection<IMessage> Messages
) : IInteractionResolved;