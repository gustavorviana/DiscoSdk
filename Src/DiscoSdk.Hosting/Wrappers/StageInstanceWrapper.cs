using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper around the raw <see cref="StageInstance"/> POCO. Exposes the read surface plus
/// <c>Modify</c> / <c>Delete</c> actions that route through <see cref="DiscordClient.StageInstanceClient"/>.
/// </summary>
internal sealed class StageInstanceWrapper(DiscordClient client, StageInstance model) : IStageInstance
{
	private StageInstance _model = model;

	/// <inheritdoc />
	public Snowflake Id => _model.Id;

	/// <inheritdoc />
	public Snowflake GuildId => _model.GuildId;

	/// <inheritdoc />
	public Snowflake ChannelId => _model.ChannelId;

	/// <inheritdoc />
	public string Topic => _model.Topic;

	/// <inheritdoc />
	public StagePrivacyLevel PrivacyLevel => _model.PrivacyLevel;

	/// <inheritdoc />
	public Snowflake? GuildScheduledEventId => _model.GuildScheduledEventId;

	/// <inheritdoc />
	public IModifyStageInstanceAction Modify()
		=> new ModifyStageInstanceAction(client, _model.ChannelId);

	/// <inheritdoc />
	public IReasonedRestAction Delete()
	{
		var channelId = _model.ChannelId;
		return new ReasonedRestAction((reason, ct) => client.StageInstanceClient.DeleteAsync(channelId, reason, ct));
	}

	IRestAction IDeletable.Delete() => Delete();
}
