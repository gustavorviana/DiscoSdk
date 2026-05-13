using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class CreateGuildStickerAction(DiscordClient client, Snowflake guildId, string name, string tags, MessageFile file)
	: RestAction<ISticker>, ICreateGuildStickerAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Snowflake _guildId = guildId;
	private readonly string _name = !string.IsNullOrWhiteSpace(name)
		? name
		: throw new ArgumentException("Name cannot be null or empty.", nameof(name));

	private string _tags = !string.IsNullOrWhiteSpace(tags)
		? tags
		: throw new ArgumentException("Tags cannot be null or empty.", nameof(tags));
	private MessageFile _file = file ?? throw new ArgumentNullException(nameof(file));
	private string? _description;

	/// <inheritdoc />
	public ICreateGuildStickerAction SetDescription(string description) { _description = description; return this; }
	/// <inheritdoc />
	public ICreateGuildStickerAction SetTags(string tags)
	{
		_tags = !string.IsNullOrWhiteSpace(tags)
			? tags
			: throw new ArgumentException("Tags cannot be null or empty.", nameof(tags));
		return this;
	}
	/// <inheritdoc />
	public ICreateGuildStickerAction SetFile(MessageFile file)
	{
		_file = file ?? throw new ArgumentNullException(nameof(file));
		return this;
	}

	/// <inheritdoc />
	public override async Task<ISticker> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var model = await _client.StickerClient.CreateGuildStickerAsync(
			_guildId, _name, _description, _tags, _file, cancellationToken);
		return new StickerWrapper(_client, model);
	}
}
