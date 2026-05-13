using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// Wrapper over the raw <see cref="Sticker"/> POCO. Standard pack stickers expose read-only data;
/// guild stickers also expose Modify / Delete.
/// </summary>
internal sealed class StickerWrapper(DiscordClient client, Sticker model) : ISticker
{
	/// <inheritdoc />
	public Snowflake Id => model.Id;
	/// <inheritdoc />
	public DateTimeOffset CreatedAt => model.Id.CreatedAt;
	/// <inheritdoc />
	public string Name => model.Name;
	/// <inheritdoc />
	public string? Description => model.Description;
	/// <inheritdoc />
	public string? Tags => model.Tags;
	/// <inheritdoc />
	public StickerType Type => model.Type;
	/// <inheritdoc />
	public StickerFormatType FormatType => model.FormatType;
	/// <inheritdoc />
	public bool? Available => model.Available;
	/// <inheritdoc />
	public Snowflake? GuildId => model.GuildId;
	/// <inheritdoc />
	public Snowflake? PackId => model.PackId;
	/// <inheritdoc />
	public int? SortValue => model.SortValue;
	/// <inheritdoc />
	public IUser? User => model.User is null ? null : new UserWrapper(client, model.User);

	/// <inheritdoc />
	public IModifyGuildStickerAction Modify()
	{
		if (model.GuildId is null || model.GuildId.Value.Empty)
			throw new InvalidOperationException("Only guild stickers can be modified. Standard pack stickers are read-only.");

		return new ModifyGuildStickerAction(client, model.GuildId.Value, model.Id);
	}

	/// <inheritdoc />
	public IRestAction Delete()
	{
		if (model.GuildId is null || model.GuildId.Value.Empty)
			throw new InvalidOperationException("Only guild stickers can be deleted. Standard pack stickers are read-only.");

		var guildId = model.GuildId.Value;
		var stickerId = model.Id;
		return RestAction.Create(ct => client.StickerClient.DeleteGuildStickerAsync(guildId, stickerId, ct));
	}
}
