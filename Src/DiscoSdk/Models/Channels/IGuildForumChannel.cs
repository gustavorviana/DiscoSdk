using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a forum channel in a Discord guild.
/// </summary>
public interface IGuildForumChannel : IGuildChannel, IThreadBasedChannel
{
	/// <summary>
	/// Gets the default layout view used to display posts in this forum channel.
	/// </summary>
	ForumLayout DefaultLayout { get; }

	/// <summary>
	/// Gets a manager to edit this forum channel.
	/// </summary>
	/// <returns>A manager that can be configured and executed to edit the channel.</returns>
	/// <remarks>
	/// The manager is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IForumChannelManager GetManager();
}

