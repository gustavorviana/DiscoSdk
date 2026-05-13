using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

public interface IThreadContainer : IGuildChannel
{
    /// <summary>
    /// Gets the default rate limit per user (slowmode), in seconds, applied to threads created in this channel.
    /// </summary>
    int DefaultThreadSlowmode { get; }

    ICreateIThreadChannelAction CreateThreadChannel(string name, Snowflake messageId, bool isPrivate);

    /// <summary>
    /// Begins a new post (a thread with an initial message) in this forum or media channel. Configure
    /// the message content, embeds, applied tags, etc. on the returned action before executing it.
    /// </summary>
    /// <param name="name">The post (thread) name.</param>
    ICreateIThreadChannelAction StartPost(string name);

    IThreadChannelPaginationAction GetThreadChannels();
}