using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a stage channel in a Discord guild. Stage channels also carry an embedded text chat
/// ("Text-in-Voice"), hence the <see cref="IGuildMessageChannel"/> surface in addition to the audio one.
/// </summary>
public interface IGuildStageChannel : IGuildMessageChannel, IVideoChannel
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
	IRestAction RequestToSpeak();

	/// <summary>
	/// Cancels the request to speak in this stage channel.
	/// </summary>
	IRestAction CancelRequestToSpeak();

	/// <summary>
	/// Fetches the live stage instance currently attached to this channel.
	/// </summary>
	IRestAction<IStageInstance> GetStageInstance();

	/// <summary>
	/// Opens a new stage instance on this channel. Returns a fluent builder — chain
	/// <c>SetPrivacyLevel</c> / <c>SetSendStartNotification</c> / <c>SetGuildScheduledEvent</c> and
	/// finish with <c>ExecuteAsync</c>.
	/// </summary>
	/// <param name="topic">Topic shown to listeners (1-120 chars).</param>
	ICreateStageInstanceAction CreateStageInstance(string topic);
}
