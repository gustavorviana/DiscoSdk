using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Hosting.Models.Requests.StageInstances;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class CreateStageInstanceAction(DiscordClient client, Snowflake channelId, string topic)
	: RestAction<IStageInstance>, ICreateStageInstanceAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Snowflake _channelId = channelId;
	private readonly string _topic = !string.IsNullOrWhiteSpace(topic)
		? topic
		: throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

	private StagePrivacyLevel? _privacyLevel;
	private bool? _sendStartNotification;
	private Snowflake? _guildScheduledEventId;

	/// <inheritdoc />
	public ICreateStageInstanceAction SetPrivacyLevel(StagePrivacyLevel privacyLevel)
	{
		_privacyLevel = privacyLevel;
		return this;
	}

	/// <inheritdoc />
	public ICreateStageInstanceAction SetSendStartNotification(bool sendStartNotification)
	{
		_sendStartNotification = sendStartNotification;
		return this;
	}

	/// <inheritdoc />
	public ICreateStageInstanceAction SetGuildScheduledEvent(Snowflake guildScheduledEventId)
	{
		_guildScheduledEventId = guildScheduledEventId;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IStageInstance> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new CreateStageInstanceRequest
		{
			ChannelId = _channelId.ToString(),
			Topic = _topic,
			PrivacyLevel = _privacyLevel,
			SendStartNotification = _sendStartNotification,
			GuildScheduledEventId = _guildScheduledEventId?.ToString(),
		};

		var model = await _client.StageInstanceClient.CreateAsync(request, cancellationToken);
		return new StageInstanceWrapper(_client, model);
	}
}
