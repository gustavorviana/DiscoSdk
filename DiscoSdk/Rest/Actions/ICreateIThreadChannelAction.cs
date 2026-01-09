using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for creating a thread channel.
/// </summary>
public interface ICreateIThreadChannelAction : IRestAction<IGuildThreadChannel>
{
	/// <summary>
	/// Sets the applied tags for the thread (forum/media channels only).
	/// </summary>
	/// <param name="tagIds">The IDs of the tags to apply.</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetAppliedTags(params DiscordId[] tagIds);

	/// <summary>
	/// Sets the auto-archive duration for the thread.
	/// </summary>
	/// <param name="duration">The auto-archive duration in minutes (60, 1440, 4320, or 10080).</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetAutoArchiveDuration(int duration);

	/// <summary>
	/// Sets the rate limit per user for the thread.
	/// </summary>
	/// <param name="rateLimit">The rate limit in seconds.</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetRateLimitPerUser(int rateLimit);

	/// <summary>
	/// Sets whether non-moderators can add other non-moderators to a private thread.
	/// Only applicable for private threads created from messages.
	/// </summary>
	/// <param name="invitable">Whether the thread should be invitable.</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetInvitable(bool invitable);

	/// <summary>
	/// Sets the content of the initial message for forum posts.
	/// </summary>
	/// <param name="content">The message content (max 2000 characters).</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetMessageContent(string? content);

	/// <summary>
	/// Adds embeds to the initial message for forum posts.
	/// </summary>
	/// <param name="embeds">The embeds to add (max 10 total).</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction AddMessageEmbeds(params Embed[] embeds);

	/// <summary>
	/// Sets the components for the initial message for forum posts.
	/// </summary>
	/// <param name="components">The message components (max 5 rows).</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetMessageComponents(params MessageComponent[] components);

	/// <summary>
	/// Sets the allowed mentions configuration for the initial message in forum posts.
	/// </summary>
	/// <param name="parse">An array of allowed mention types to parse from the content. Valid values: "roles", "users", "everyone".</param>
	/// <param name="users">An array of user IDs to mention (max 100).</param>
	/// <param name="roles">An array of role IDs to mention (max 100).</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetMessageAllowedMentions(string[]? parse = null, string[]? users = null, string[]? roles = null);

	/// <summary>
	/// Sets the sticker IDs for the initial message in forum posts.
	/// </summary>
	/// <param name="stickerIds">The IDs of stickers to send with the message (max 3).</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetMessageStickers(params DiscordId[] stickerIds);

	/// <summary>
	/// Sets the message flags for the initial message in forum posts.
	/// </summary>
	/// <param name="flags">The message flags.</param>
	/// <returns>The current <see cref="ICreateIThreadChannelAction"/> instance.</returns>
	ICreateIThreadChannelAction SetMessageFlags(MessageFlags? flags);
}