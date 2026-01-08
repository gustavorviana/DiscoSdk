using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord public thread channel.
/// </summary>
public interface IPublicThreadChannel : IGuildChannel, ITextBasedChannel, IThreadLikeChannel
{
	/// <summary>
	/// Creates a builder for editing this thread.
	/// </summary>
	/// <returns>An <see cref="IEditPublicThreadChannelRestAction"/> instance for editing the thread.</returns>
	IEditPublicThreadChannelRestAction Edit();
}

