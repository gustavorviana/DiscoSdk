using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models;
using DiscoSdk.Models.Requests.Stickers;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

internal sealed class ModifyGuildStickerAction(DiscordClient client, Snowflake guildId, Snowflake stickerId)
	: RestAction<ISticker>, IModifyGuildStickerAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly Snowflake _guildId = guildId;
	private readonly Snowflake _stickerId = stickerId;

	private string? _name;
	private string? _description;
	private string? _tags;

	/// <inheritdoc />
	public IModifyGuildStickerAction SetName(string name) { _name = name; return this; }
	/// <inheritdoc />
	public IModifyGuildStickerAction SetDescription(string description) { _description = description; return this; }
	/// <inheritdoc />
	public IModifyGuildStickerAction SetTags(string tags) { _tags = tags; return this; }

	/// <inheritdoc />
	public override async Task<ISticker> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new ModifyGuildStickerRequest
		{
			Name = _name,
			Description = _description,
			Tags = _tags,
		};

		var model = await _client.StickerClient.ModifyGuildStickerAsync(_guildId, _stickerId, request, cancellationToken);
		return new StickerWrapper(_client, model);
	}
}
