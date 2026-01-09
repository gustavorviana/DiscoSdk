using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

public interface IThreadContainer : IGuildChannel
{
    int DefaultThreadSlowmode { get; }

    ICreateIThreadChannelAction CreateThreadChannel(string name, DiscordId messageId, bool isPrivate);

    IThreadChannelPaginationAction GetThreadChannels();
}