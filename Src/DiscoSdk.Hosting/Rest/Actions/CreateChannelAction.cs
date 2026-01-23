using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="ICreateChannelAction"/> for creating Discord channels.
/// </summary>
internal class CreateChannelAction : RestAction<IGuildChannel>, ICreateChannelAction
{
	private readonly DiscordClient _client;
	private readonly IGuild _guild;
	private string? _name;
	private ChannelType? _type;
	private string? _topic;
	private bool? _nsfw;
	private int? _bitrate;
	private int? _userLimit;
	private int? _rateLimitPerUser;
	private Snowflake? _parentId;
	private int? _position;
	private PermissionOverwrite[]? _permissionOverwrites;
	private string? _rtcRegion;
	private VideoQualityMode? _videoQualityMode;
	private int? _defaultAutoArchiveDuration;
	private string? _defaultReactionEmoji;
	private int? _defaultThreadRateLimitPerUser;
	private SortOrderType? _defaultSortOrder;
	private ForumLayoutType? _defaultForumLayout;
	private ForumTag[]? _availableTags;
	private ChannelFlags? _flags;

	public CreateChannelAction(DiscordClient client, IGuild guild, string name, ChannelType type)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guild = guild ?? throw new ArgumentNullException(nameof(guild));
		_name = name ?? throw new ArgumentNullException(nameof(name));
		_type = type;
	}

	public ICreateChannelAction SetName(string name)
	{
		_name = name ?? throw new ArgumentNullException(nameof(name));
		return this;
	}

	public ICreateChannelAction SetType(ChannelType type)
	{
		_type = type;
		return this;
	}

	public ICreateChannelAction SetTopic(string? topic)
	{
		_topic = topic;
		return this;
	}

	public ICreateChannelAction SetNsfw(bool? nsfw = true)
	{
		_nsfw = nsfw;
		return this;
	}

	public ICreateChannelAction SetBitrate(int? bitrate)
	{
		_bitrate = bitrate;
		return this;
	}

	public ICreateChannelAction SetUserLimit(int? userLimit)
	{
		_userLimit = userLimit;
		return this;
	}

	public ICreateChannelAction SetRateLimitPerUser(int? rateLimit)
	{
		_rateLimitPerUser = rateLimit;
		return this;
	}

	public ICreateChannelAction SetParentId(Snowflake? parentId)
	{
		_parentId = parentId;
		return this;
	}

	public ICreateChannelAction SetPosition(int? position)
	{
		_position = position;
		return this;
	}

	public ICreateChannelAction SetPermissionOverwrites(PermissionOverwrite[]? overwrites)
	{
		_permissionOverwrites = overwrites;
		return this;
	}

	public ICreateChannelAction SetRtcRegion(string? region)
	{
		_rtcRegion = region;
		return this;
	}

	public ICreateChannelAction SetVideoQualityMode(VideoQualityMode? mode)
	{
		_videoQualityMode = mode;
		return this;
	}

	public ICreateChannelAction SetDefaultAutoArchiveDuration(int? duration)
	{
		_defaultAutoArchiveDuration = duration;
		return this;
	}

	public ICreateChannelAction SetDefaultReactionEmoji(string? emoji)
	{
		_defaultReactionEmoji = emoji;
		return this;
	}

	public ICreateChannelAction SetDefaultThreadRateLimitPerUser(int? rateLimit)
	{
		_defaultThreadRateLimitPerUser = rateLimit;
		return this;
	}

	public ICreateChannelAction SetDefaultSortOrder(SortOrderType? sortOrder)
	{
		_defaultSortOrder = sortOrder;
		return this;
	}

	public ICreateChannelAction SetDefaultForumLayout(ForumLayoutType? layout)
	{
		_defaultForumLayout = layout;
		return this;
	}

	public ICreateChannelAction SetAvailableTags(ForumTag[]? tags)
	{
		_availableTags = tags;
		return this;
	}

	public ICreateChannelAction SetFlags(ChannelFlags? flags)
	{
		_flags = flags;
		return this;
	}

	public override async Task<IGuildChannel> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_name))
			throw new InvalidOperationException("Channel name is required.");

		if (!_type.HasValue)
			throw new InvalidOperationException("Channel type is required.");

		var request = new Dictionary<string, object?>
		{
			["name"] = _name,
			["type"] = (int)_type.Value
		};

		if (_topic != null)
			request["topic"] = _topic;

		if (_nsfw.HasValue)
			request["nsfw"] = _nsfw.Value;

		if (_bitrate.HasValue)
			request["bitrate"] = _bitrate.Value;

		if (_userLimit.HasValue)
			request["user_limit"] = _userLimit.Value;

		if (_rateLimitPerUser.HasValue)
			request["rate_limit_per_user"] = _rateLimitPerUser.Value;

		if (_parentId.HasValue)
			request["parent_id"] = _parentId.Value.ToString();

		if (_position.HasValue)
			request["position"] = _position.Value;

		if (_permissionOverwrites != null)
			request["permission_overwrites"] = _permissionOverwrites;

		if (_rtcRegion != null)
			request["rtc_region"] = _rtcRegion;

		if (_videoQualityMode.HasValue)
			request["video_quality_mode"] = (int)_videoQualityMode.Value;

		if (_defaultAutoArchiveDuration.HasValue)
			request["default_auto_archive_duration"] = _defaultAutoArchiveDuration.Value;

		if (_defaultReactionEmoji != null)
			request["default_reaction_emoji"] = _defaultReactionEmoji;

		if (_defaultThreadRateLimitPerUser.HasValue)
			request["default_thread_rate_limit_per_user"] = _defaultThreadRateLimitPerUser.Value;

		if (_defaultSortOrder.HasValue)
			request["default_sort_order"] = (int)_defaultSortOrder.Value;

		if (_defaultForumLayout.HasValue)
			request["default_forum_layout"] = (int)_defaultForumLayout.Value;

		if (_availableTags != null)
			request["available_tags"] = _availableTags;

		if (_flags.HasValue)
			request["flags"] = (int)_flags.Value;

		var channel = await _client.GuildClient.CreateChannelAsync(_guild.Id, request, cancellationToken);

		var guildChannel = ChannelWrapper.ToGuildChannel(channel, _guild, _client);
		return (IGuildChannel)guildChannel;
	}
}

