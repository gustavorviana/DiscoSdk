using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord announcement thread channel.
/// </summary>
public interface IAnnouncementThreadChannel : IGuildChannel, ITextBasedChannel, IThreadLikeChannel
{
    /// <summary>
    /// Creates a builder for editing this thread.
    /// </summary>
    /// <returns>An <see cref="IEditThreadChannelRestAction{IAnnouncementThreadChannel}"/> instance for editing the thread.</returns>
    IEditAnnouncementThreadChannelRestAction Edit();
}

