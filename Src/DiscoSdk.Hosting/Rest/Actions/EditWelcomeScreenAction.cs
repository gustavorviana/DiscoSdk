using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditWelcomeScreenAction"/> for editing a guild welcome screen.
/// </summary>
internal class EditWelcomeScreenAction : RestAction<WelcomeScreen>, IEditWelcomeScreenAction
{
	private readonly DiscordClient _client;
	private readonly IGuild _guild;
	private bool? _enabled;
	private string? _description;
	private readonly List<Dictionary<string, object?>> _channels = new();
	private bool _enabledSet;
	private bool _descriptionSet;
	private bool _channelsCleared;

	/// <summary>
	/// Initializes a new instance of the <see cref="EditWelcomeScreenAction"/> class.
	/// </summary>
	/// <param name="client">The Discord client.</param>
	/// <param name="guild">The guild to edit the welcome screen for.</param>
	public EditWelcomeScreenAction(DiscordClient client, IGuild guild)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guild = guild ?? throw new ArgumentNullException(nameof(guild));
	}

	/// <inheritdoc />
	public IEditWelcomeScreenAction SetEnabled(bool enabled)
	{
		_enabled = enabled;
		_enabledSet = true;
		return this;
	}

	/// <inheritdoc />
	public IEditWelcomeScreenAction SetDescription(string? description)
	{
		_description = description;
		_descriptionSet = true;
		return this;
	}

	/// <inheritdoc />
	public IEditWelcomeScreenAction ClearChannels()
	{
		_channels.Clear();
		_channelsCleared = true;
		return this;
	}

	/// <inheritdoc />
	public IEditWelcomeScreenAction AddChannel(
		Snowflake channelId,
		string? description = null,
		Snowflake? emojiId = null,
		string? emojiName = null)
	{
		var channel = new Dictionary<string, object?>
		{
			["channel_id"] = channelId.ToString()
		};

		if (description != null)
			channel["description"] = description;

		if (emojiId.HasValue)
			channel["emoji_id"] = emojiId.Value.ToString();

		if (emojiName != null)
			channel["emoji_name"] = emojiName;

		_channels.Add(channel);
		return this;
	}

	/// <inheritdoc />
	public override async Task<WelcomeScreen> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new Dictionary<string, object?>();

		if (_enabledSet)
			request["enabled"] = _enabled;

		if (_descriptionSet)
			request["description"] = _description;

		if (_channelsCleared || _channels.Count > 0)
			request["welcome_channels"] = _channels.ToArray();

		return await _client.GuildClient.EditWelcomeScreenAsync(_guild.Id, request, cancellationToken);
	}
}

