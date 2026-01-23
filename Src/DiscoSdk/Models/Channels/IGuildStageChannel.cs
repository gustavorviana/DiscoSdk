using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a stage channel in a Discord guild.
/// </summary>
public interface IGuildStageChannel : IGuildChannel, IVideoChannel
{
	/// <summary>
	/// Gets a manager to edit this stage channel.
	/// </summary>
	/// <returns>A manager that can be configured and executed to edit the channel.</returns>
	/// <remarks>
	/// The manager is not executed immediately. Call <see cref="IRestAction{T}.ExecuteAsync"/> to execute it.
	/// </remarks>
	IStageChannelManager GetManager();

	/// <summary>
	/// Requests to speak in this stage channel.
	/// </summary>
	/// <returns>A REST action that can be executed to request to speak.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction RequestToSpeak();

	/// <summary>
	/// Cancels the request to speak in this stage channel.
	/// </summary>
	/// <returns>A REST action that can be executed to cancel the request to speak.</returns>
	/// <remarks>
	/// The action is not executed immediately. Call <see cref="IRestAction.ExecuteAsync"/> to execute it.
	/// </remarks>
	IRestAction CancelRequestToSpeak();
}

