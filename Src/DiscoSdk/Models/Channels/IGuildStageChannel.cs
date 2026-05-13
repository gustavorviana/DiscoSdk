using DiscoSdk.Models.Enums;
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

	/// <summary>
	/// Fetches the live stage instance currently attached to this channel.
	/// </summary>
	IRestAction<IStageInstance> GetStageInstance();

	/// <summary>
	/// Opens a new stage instance on this channel.
	/// </summary>
	/// <param name="topic">Topic shown to listeners (1-120 chars).</param>
	/// <param name="privacyLevel">Defaults to <see cref="StagePrivacyLevel.GuildOnly"/> when null.</param>
	/// <param name="sendStartNotification">Whether to ping <c>@everyone</c> on start (requires permission).</param>
	/// <param name="guildScheduledEventId">If non-null, links the stage instance to an existing scheduled event.</param>
	IRestAction<IStageInstance> CreateStageInstance(
		string topic,
		StagePrivacyLevel? privacyLevel = null,
		bool? sendStartNotification = null,
		Snowflake? guildScheduledEventId = null);
}
