using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for the Discord Sticker REST surface — global sticker / pack lookups and guild-owned
/// sticker CRUD.
/// </summary>
internal class StickerClient(IDiscordRestClient client)
{
	// ---- Global ----

	/// <summary>Gets any sticker by id (built-in pack or guild).</summary>
	public Task<Sticker> GetStickerAsync(Snowflake stickerId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("stickers/{sticker_id}", stickerId);
		return client.SendAsync<Sticker>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Lists Discord's built-in Nitro sticker packs.</summary>
	public Task<StickerPackList> ListStickerPacksAsync(CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("sticker-packs");
		return client.SendAsync<StickerPackList>(route, HttpMethod.Get, null, cancellationToken);
	}

	// ---- Guild stickers ----

	/// <summary>Lists every sticker owned by a guild.</summary>
	public Task<Sticker[]> ListGuildStickersAsync(Snowflake guildId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("guilds/{guild_id}/stickers", guildId);
		return client.SendAsync<Sticker[]>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>Gets a single guild-owned sticker.</summary>
	public Task<Sticker> GetGuildStickerAsync(Snowflake guildId, Snowflake stickerId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("guilds/{guild_id}/stickers/{sticker_id}", guildId, stickerId);
		return client.SendAsync<Sticker>(route, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Creates a guild sticker. Discord expects multipart/form-data with separate <c>name</c>,
	/// <c>description</c>, <c>tags</c>, and <c>file</c> form parts — each as its own field, not
	/// wrapped in <c>payload_json</c>.
	/// </summary>
	public Task<Sticker> CreateGuildStickerAsync(
		Snowflake guildId,
		string name,
		string? description,
		string tags,
		MessageFile file,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(tags);
		ArgumentNullException.ThrowIfNull(file);

		var fields = new Dictionary<string, string>
		{
			["name"] = name,
			["tags"] = tags,
		};
		if (!string.IsNullOrWhiteSpace(description))
			fields["description"] = description;

		var route = new DiscordRoute("guilds/{guild_id}/stickers", guildId);
		return client.SendFormDataAsync<Sticker>(route, HttpMethod.Post, fields, "file", file, cancellationToken);
	}

	/// <summary>Modifies a guild sticker's metadata (name, description, tags).</summary>
	public Task<Sticker> ModifyGuildStickerAsync(Snowflake guildId, Snowflake stickerId, object request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var route = new DiscordRoute("guilds/{guild_id}/stickers/{sticker_id}", guildId, stickerId);
		return client.SendAsync<Sticker>(route, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>Deletes a guild sticker.</summary>
	public Task DeleteGuildStickerAsync(Snowflake guildId, Snowflake stickerId, CancellationToken cancellationToken = default)
	{
		var route = new DiscordRoute("guilds/{guild_id}/stickers/{sticker_id}", guildId, stickerId);
		return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
	}
}
