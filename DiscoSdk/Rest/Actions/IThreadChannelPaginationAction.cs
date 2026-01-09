using DiscoSdk.Models.Channels;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving thread channels.
/// </summary>
public interface IThreadChannelPaginationAction : IPaginationAction<IGuildThreadChannel, IThreadChannelPaginationAction>
{
	/// <summary>
	/// Gets threads before the specified timestamp (ISO 8601 format).
	/// Only applies to archived threads.
	/// </summary>
	/// <param name="timestamp">The timestamp to get threads before.</param>
	/// <returns>The current <see cref="IThreadChannelPaginationAction"/> instance.</returns>
	IThreadChannelPaginationAction Before(string timestamp);
}

