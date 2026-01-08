using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord private thread channel.
/// </summary>
public interface IPrivateThreadChannel : IGuildChannel, ITextBasedChannel, IThreadLikeChannel
{
	/// <summary>
	/// Creates a builder for editing this thread.
	/// </summary>
	/// <returns>An <see cref="IEditPrivateThreadChannelRestAction"/> instance for editing the thread.</returns>
	IEditPrivateThreadChannelRestAction Edit();
}

