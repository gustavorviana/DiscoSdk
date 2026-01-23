using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

public interface IThreadContainer : IGuildChannel
{
    int DefaultThreadSlowmode { get; }

    ICreateIThreadChannelAction CreateThreadChannel(string name, Snowflake messageId, bool isPrivate);

    IThreadChannelPaginationAction GetThreadChannels();
}