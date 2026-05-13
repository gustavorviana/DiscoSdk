using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Requests.ScheduledEvents;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class CreateScheduledEventAction(
	DiscordClient client,
	Snowflake guildId,
	string name,
	DateTimeOffset scheduledStartTime,
	ScheduledEventEntityType entityType) : RestAction<IGuildScheduledEvent>, ICreateScheduledEventAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Snowflake _guildId = guildId;
	private readonly string _name = !string.IsNullOrWhiteSpace(name)
		? name
		: throw new ArgumentException("Name cannot be null or empty.", nameof(name));
	private readonly DateTimeOffset _scheduledStartTime = scheduledStartTime;
	private readonly ScheduledEventEntityType _entityType = entityType;

	private ScheduledEventPrivacyLevel _privacyLevel = ScheduledEventPrivacyLevel.GuildOnly;
	private Snowflake? _channelId;
	private string? _description;
	private DateTimeOffset? _scheduledEndTime;
	private ScheduledEventEntityMetadata? _entityMetadata;
	private string? _image;

	/// <inheritdoc />
	public ICreateScheduledEventAction SetPrivacyLevel(ScheduledEventPrivacyLevel privacyLevel)
	{
		_privacyLevel = privacyLevel;
		return this;
	}

	/// <inheritdoc />
	public ICreateScheduledEventAction SetChannel(Snowflake channelId)
	{
		_channelId = channelId;
		return this;
	}

	/// <inheritdoc />
	public ICreateScheduledEventAction SetDescription(string description)
	{
		_description = description;
		return this;
	}

	/// <inheritdoc />
	public ICreateScheduledEventAction SetScheduledEndTime(DateTimeOffset scheduledEndTime)
	{
		_scheduledEndTime = scheduledEndTime;
		return this;
	}

	/// <inheritdoc />
	public ICreateScheduledEventAction SetLocation(string location)
	{
		_entityMetadata = new ScheduledEventEntityMetadata { Location = location };
		return this;
	}

	/// <inheritdoc />
	public ICreateScheduledEventAction SetImage(string imageDataUri)
	{
		_image = imageDataUri;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IGuildScheduledEvent> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new CreateScheduledEventRequest
		{
			Name = _name,
			ScheduledStartTime = _scheduledStartTime,
			EntityType = _entityType,
			PrivacyLevel = _privacyLevel,
			ChannelId = _channelId?.ToString(),
			Description = _description,
			ScheduledEndTime = _scheduledEndTime,
			EntityMetadata = _entityMetadata,
			Image = _image,
		};

		var model = await _client.GuildScheduledEventClient.CreateAsync(_guildId, request, cancellationToken);
		return new GuildScheduledEventWrapper(_client, model);
	}
}
