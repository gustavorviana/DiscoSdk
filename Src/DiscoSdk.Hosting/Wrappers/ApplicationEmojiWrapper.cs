using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Models;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Wrappers;

/// <summary>
/// <see cref="IEmoji"/> implementation for an application-owned emoji. <see cref="Guild"/> is
/// always <c>null</c>; <see cref="Edit"/> and <see cref="Delete"/> route through
/// <see cref="ApplicationClient"/> instead of <c>GuildClient</c>.
/// </summary>
internal sealed class ApplicationEmojiWrapper(DiscordClient client, InternalEmoji emoji) : IEmoji
{
	/// <inheritdoc />
	public Snowflake Id => emoji.Id ?? throw new InvalidOperationException("Emoji must have an ID.");

	/// <inheritdoc />
	public DateTimeOffset CreatedAt => emoji.Id?.CreatedAt ?? throw new InvalidOperationException("Emoji must have an ID.");

	/// <inheritdoc />
	public string? Name => emoji.Name;

	/// <inheritdoc />
	public string[] Roles => emoji.Roles;

	/// <inheritdoc />
	public IUser? User => emoji.User is null ? null : new UserWrapper(client, emoji.User);

	/// <inheritdoc />
	public bool RequireColons => emoji.RequireColons;

	/// <inheritdoc />
	public bool IsManaged => emoji.Managed;

	/// <inheritdoc />
	public bool IsAnimated => emoji.Animated;

	/// <inheritdoc />
	public bool Available => emoji.Available;

	/// <summary>Application emojis are global — there is no owning guild.</summary>
	public IGuild? Guild => null;

	/// <inheritdoc />
	public IEditEmojiAction Edit()
		=> new EditApplicationEmojiAction(client, Id);

	/// <inheritdoc />
	/// <remarks>
	/// Returns <see cref="IReasonedRestAction"/> to satisfy <see cref="IEmoji"/>'s
	/// <see cref="IReasonedDeletable"/> contract, but Discord does NOT record application-emoji
	/// deletes in any audit log — the reason set via <c>WithReason</c> is silently dropped
	/// (application emojis are global to the bot, not bound to a guild).
	/// </remarks>
	public IReasonedRestAction Delete()
	{
		var emojiId = Id;
		return new ReasonedRestAction((_, ct) =>
			client.ApplicationClient.DeleteApplicationEmojiAsync(client.RequireApplicationId(), emojiId, ct));
	}

	IRestAction IDeletable.Delete() => Delete();
}
