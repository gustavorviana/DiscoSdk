using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a text channel in a Discord guild.
/// </summary>
public interface IGuildTextChannel : IGuildChannel, IGuildMessageChannel
{
	/// <summary>
	/// Gets a manager to edit this text channel.
	/// </summary>
	/// <returns>A manager that can be configured and executed to edit the channel.</returns>
	/// <remarks>
	/// The manager is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	ITextChannelManager GetManager();
}
