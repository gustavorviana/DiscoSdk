using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Requests.ScheduledEvents;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifyScheduledEventAction(DiscordClient client, Snowflake guildId, Snowflake eventId)
	: RestAction<IGuildScheduledEvent>, IModifyScheduledEventAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Snowflake _guildId = guildId;
	private readonly Snowflake _eventId = eventId;

	private string? _name;
	private string? _description;
	private DateTimeOffset? _scheduledStartTime;
	private DateTimeOffset? _scheduledEndTime;
	private ScheduledEventPrivacyLevel? _privacyLevel;
	private ScheduledEventStatus? _status;
	private ScheduledEventEntityType? _entityType;
	private Snowflake? _channelId;
	private ScheduledEventEntityMetadata? _entityMetadata;
	private string? _image;

	/// <inheritdoc />
	public IModifyScheduledEventAction SetName(string name) { _name = name; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetDescription(string description) { _description = description; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetScheduledStartTime(DateTimeOffset scheduledStartTime) { _scheduledStartTime = scheduledStartTime; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetScheduledEndTime(DateTimeOffset scheduledEndTime) { _scheduledEndTime = scheduledEndTime; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetPrivacyLevel(ScheduledEventPrivacyLevel privacyLevel) { _privacyLevel = privacyLevel; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetStatus(ScheduledEventStatus status) { _status = status; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetEntityType(ScheduledEventEntityType entityType) { _entityType = entityType; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetChannel(Snowflake channelId) { _channelId = channelId; return this; }
	/// <inheritdoc />
	public IModifyScheduledEventAction SetLocation(string location)
	{
		_entityMetadata = new ScheduledEventEntityMetadata { Location = location };
		return this;
	}
	/// <inheritdoc />
	public IModifyScheduledEventAction SetImage(string imageDataUri) { _image = imageDataUri; return this; }

	/// <inheritdoc />
	public override async Task<IGuildScheduledEvent> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new ModifyScheduledEventRequest
		{
			Name = _name,
			Description = _description,
			ScheduledStartTime = _scheduledStartTime,
			ScheduledEndTime = _scheduledEndTime,
			PrivacyLevel = _privacyLevel,
			Status = _status,
			EntityType = _entityType,
			ChannelId = _channelId?.ToString(),
			EntityMetadata = _entityMetadata,
			Image = _image,
		};

		var model = await _client.GuildScheduledEventClient.ModifyAsync(_guildId, _eventId, request, cancellationToken);
		return new GuildScheduledEventWrapper(_client, model);
	}
}
