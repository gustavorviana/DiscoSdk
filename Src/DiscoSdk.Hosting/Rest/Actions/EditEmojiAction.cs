using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IEditEmojiAction"/> for editing Discord emojis.
/// </summary>
internal class EditEmojiAction : RestAction<IEmoji>, IEditEmojiAction
{
	private readonly DiscordClient _client;
	private readonly IGuild _guild;
	private readonly Snowflake _emojiId;
	private string? _name;
	private Snowflake[]? _roles;

	public EditEmojiAction(DiscordClient client, IGuild guild, Snowflake emojiId)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guild = guild ?? throw new ArgumentNullException(nameof(guild));
		_emojiId = emojiId;
	}

	public IEditEmojiAction SetName(string name)
	{
		_name = name ?? throw new ArgumentNullException(nameof(name));
		return this;
	}

	public IEditEmojiAction SetRoles(params Snowflake[] roleIds)
	{
		_roles = roleIds;
		return this;
	}

	public override async Task<IEmoji> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new Dictionary<string, object?>();

		if (_name != null)
			request["name"] = _name;

		if (_roles != null)
			request["roles"] = _roles.Select(r => r.ToString()).ToArray();

		var emoji = await _client.GuildClient.EditEmojiAsync(_guild.Id, _emojiId, request, cancellationToken);

		return new EmojiWrapper(_client, emoji, _guild);
	}
}

