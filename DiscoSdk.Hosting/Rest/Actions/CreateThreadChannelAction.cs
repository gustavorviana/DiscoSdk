using DiscoSdk.Hosting.Wrappers.Channels;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Requests;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="ICreateIThreadChannelAction"/> for creating thread channels.
/// </summary>
internal class CreateThreadChannelAction : RestAction<IGuildThreadChannel>, ICreateIThreadChannelAction
{
	private readonly DiscordClient _client;
	private readonly IGuildChannel _channel;
	private readonly string _name;
	private readonly Snowflake? _messageId;
	private int? _autoArchiveDuration;
	private int? _rateLimitPerUser;
	private Snowflake[]? _appliedTags;
	private bool? _invitable;
	private string? _messageContent;
	private readonly List<Embed> _messageEmbeds = [];
	private List<MessageComponent>? _messageComponents;
	private AllowedMentions? _messageAllowedMentions;
	private List<Snowflake>? _messageStickerIds;
	private MessageFlags? _messageFlags;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateThreadChannelAction"/> class for creating a thread from a message.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="channel">The channel to create the thread in.</param>
	/// <param name="name">The name of the thread.</param>
	/// <param name="messageId">The ID of the message to create the thread from.</param>
	/// <param name="isPrivate">Whether the thread should be private. This parameter is currently not used by the Discord API directly.</param>
	public CreateThreadChannelAction(DiscordClient client, IGuildChannel channel, string name, Snowflake messageId, bool isPrivate)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_channel = channel;
		_name = name ?? throw new ArgumentNullException(nameof(name));
		_messageId = messageId;
		// Note: isPrivate parameter is kept for API compatibility but the thread type is determined by Discord based on channel permissions
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateThreadChannelAction"/> class for creating a standalone thread.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="channel">The channel to create the thread in.</param>
	/// <param name="name">The name of the thread.</param>
	public CreateThreadChannelAction(DiscordClient client, IGuildChannel channel, string name)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
        _channel = channel;
		_name = name ?? throw new ArgumentNullException(nameof(name));
	}

	/// <summary>
	/// Sets the auto-archive duration for the thread.
	/// </summary>
	/// <param name="duration">The auto-archive duration in minutes (60, 1440, 4320, or 10080).</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	public ICreateIThreadChannelAction SetAutoArchiveDuration(int duration)
	{
		_autoArchiveDuration = duration;
		return this;
	}

	/// <summary>
	/// Sets the rate limit per user for the thread.
	/// </summary>
	/// <param name="rateLimit">The rate limit in seconds.</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	public ICreateIThreadChannelAction SetRateLimitPerUser(int rateLimit)
	{
		_rateLimitPerUser = rateLimit;
		return this;
	}

	/// <summary>
	/// Sets the applied tags for the thread (forum/media channels only).
	/// </summary>
	/// <param name="tagIds">The IDs of the tags to apply.</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	public ICreateIThreadChannelAction SetAppliedTags(params Snowflake[] tagIds)
	{
		_appliedTags = tagIds;
		return this;
	}

	/// <inheritdoc />
	public ICreateIThreadChannelAction SetInvitable(bool invitable)
	{
		_invitable = invitable;
		return this;
	}

	/// <inheritdoc />
	public ICreateIThreadChannelAction SetMessageContent(string? content)
	{
		if (content != null && content.Length > 2000)
			throw new ArgumentException("Message content cannot exceed 2000 characters.", nameof(content));

		_messageContent = content;
		return this;
	}

	/// <inheritdoc />
	public ICreateIThreadChannelAction AddMessageEmbeds(params Embed[] embeds)
	{
		ArgumentNullException.ThrowIfNull(embeds);

		foreach (var embed in embeds)
		{
			if (embed == null)
				continue;

			if (_messageEmbeds.Count >= 10)
				throw new InvalidOperationException("Message cannot have more than 10 embeds.");

			_messageEmbeds.Add(embed);
		}

		return this;
	}

	/// <inheritdoc />
	public ICreateIThreadChannelAction SetMessageComponents(params MessageComponent[] components)
	{
		if (components == null || components.Length == 0)
		{
			_messageComponents = null;
			return this;
		}

		if (components.Length > 5)
			throw new ArgumentException("Message cannot have more than 5 component rows.", nameof(components));

		_messageComponents = components.Select(c =>
		{
			if (c.Type == ComponentType.ActionRow)
				return c;

			// Wrap non-ActionRow components in an ActionRow
			return new MessageComponent
			{
				Type = ComponentType.ActionRow,
				Components = [c]
			};
		}).ToList();

		return this;
	}

	/// <inheritdoc />
	public ICreateIThreadChannelAction SetMessageAllowedMentions(string[]? parse = null, string[]? users = null, string[]? roles = null)
	{
		if (parse == null && users == null && roles == null)
		{
			_messageAllowedMentions = null;
			return this;
		}

		_messageAllowedMentions = new AllowedMentions
		{
			Parse = parse,
			Users = users,
			Roles = roles
		};

		return this;
	}

	/// <inheritdoc />
	public ICreateIThreadChannelAction SetMessageStickers(params Snowflake[] stickerIds)
	{
		if (stickerIds == null || stickerIds.Length == 0)
		{
			_messageStickerIds = null;
			return this;
		}

		if (stickerIds.Length > 3)
			throw new ArgumentException("Message cannot have more than 3 stickers.", nameof(stickerIds));

		_messageStickerIds = stickerIds.ToList();
		return this;
	}

	/// <inheritdoc />
	public ICreateIThreadChannelAction SetMessageFlags(MessageFlags? flags)
	{
		_messageFlags = flags;
		return this;
	}

	/// <inheritdoc />
	public override async Task<IGuildThreadChannel> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		Channel threadChannel;

		if (_messageId.HasValue)
		{
			// Create thread from message
			var request = new
			{
				name = _name,
				auto_archive_duration = _autoArchiveDuration,
				rate_limit_per_user = _rateLimitPerUser,
				invitable = _invitable
			};

			threadChannel = await _client.ChannelClient.CreateThreadFromMessageAsync(_channel.Id, _messageId.Value, request, cancellationToken);
		}
		else
		{
			// Create standalone thread (forum/media channel)
			// Forum posts require a message object
			if (string.IsNullOrWhiteSpace(_messageContent) && _messageEmbeds.Count == 0)
				throw new InvalidOperationException("Forum post must have either message content or at least one embed.");

			var messageRequest = new MessageCreateRequest
			{
				Content = _messageContent,
				Embeds = [.. _messageEmbeds],
				Components = _messageComponents?.ToArray(),
				AllowedMentions = _messageAllowedMentions,
				StickerIds = _messageStickerIds?.Select(id => id.ToString()).ToArray(),
				Flags = _messageFlags
			};

			var request = new
			{
				name = _name,
				auto_archive_duration = _autoArchiveDuration,
				rate_limit_per_user = _rateLimitPerUser,
				applied_tags = _appliedTags?.Select(id => id.ToString()).ToArray(),
				message = messageRequest
			};

			var forumPost = await _client.ChannelClient.CreateForumPostAsync(_channel.Id, request, cancellationToken);
			threadChannel = forumPost.Channel;
		}

		return new GuildThreadChannelWrapper(threadChannel, _channel.Guild, _client);
	}
}