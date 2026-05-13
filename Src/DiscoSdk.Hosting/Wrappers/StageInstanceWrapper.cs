using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Requests.StageInstances;
using DiscoSdk.Rest;
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
	public IRestAction<IStageInstance> Modify(string? topic = null, StagePrivacyLevel? privacyLevel = null)
	{
		return RestAction<IStageInstance>.Create(async ct =>
		{
			var request = new ModifyStageInstanceRequest
			{
				Topic = topic,
				PrivacyLevel = privacyLevel,
			};

			var updated = await client.StageInstanceClient.ModifyAsync(_model.ChannelId, request, ct);
			_model = updated;
			return (IStageInstance)this;
		});
	}

	/// <inheritdoc />
	public IRestAction Delete()
		=> RestAction.Create(ct => client.StageInstanceClient.DeleteAsync(_model.ChannelId, ct));
}
