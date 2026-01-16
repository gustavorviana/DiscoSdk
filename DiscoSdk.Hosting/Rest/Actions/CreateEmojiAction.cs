using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="ICreateEmojiAction"/> for creating Discord emojis.
/// </summary>
internal class CreateEmojiAction : RestAction<IEmoji>, ICreateEmojiAction
{
	private readonly DiscordClient _client;
	private readonly Snowflake _guildId;
	private readonly IGuild _guild;
	private string? _name;
	private DiscordImage? _image;
	private Snowflake[]? _roles;

	public CreateEmojiAction(DiscordClient client, IGuild guild, string name, DiscordImage image)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		_guild = guild ?? throw new ArgumentNullException(nameof(guild));
		_guildId = guild.Id;
		_name = name ?? throw new ArgumentNullException(nameof(name));
		_image = image ?? throw new ArgumentNullException(nameof(image));
	}

    public ICreateEmojiAction SetName(string name)
	{
		_name = name ?? throw new ArgumentNullException(nameof(name));
		return this;
	}

	public ICreateEmojiAction SetImage(DiscordImage image)
	{
		_image = image ?? throw new ArgumentNullException(nameof(image));
		return this;
	}

	public ICreateEmojiAction SetRoles(params Snowflake[] roleIds)
	{
		_roles = roleIds;
		return this;
	}

	public override async Task<IEmoji> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_name))
			throw new InvalidOperationException("Emoji name is required.");

		if (_image == null)
			throw new InvalidOperationException("Emoji image is required.");

		var base64 = _image.ToBase64();
		var imageDataUri = $"data:{_image.ImageType};base64,{base64}";

		var request = new Dictionary<string, object?>
		{
			["name"] = _name,
			["image"] = imageDataUri
		};

		if (_roles != null && _roles.Length > 0)
			request["roles"] = _roles.Select(r => r.ToString()).ToArray();

		var emoji = await _client.GuildClient.CreateEmojiAsync(_guildId, request, cancellationToken);
		return new EmojiWrapper(emoji, _guild, _client);
	}
}