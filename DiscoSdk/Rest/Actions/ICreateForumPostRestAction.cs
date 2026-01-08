using DiscoSdk.Models;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for creating a forum post in Discord.
/// </summary>
public interface ICreateForumPostRestAction : IRestAction<IMessage>
{
	/// <summary>
	/// Sets the name of the forum post (thread).
	/// </summary>
	/// <param name="name">The name of the post (max 100 characters).</param>
	/// <returns>The current <see cref="ICreateForumPostRestAction"/> instance.</returns>
	ICreateForumPostRestAction SetName(string name);

	/// <summary>
	/// Sets the content of the initial message in the post.
	/// </summary>
	/// <param name="content">The message content (max 2000 characters).</param>
	/// <returns>The current <see cref="ICreateForumPostRestAction"/> instance.</returns>
	ICreateForumPostRestAction SetContent(string? content);

	/// <summary>
	/// Sets the auto-archive duration for the thread.
	/// </summary>
	/// <param name="duration">The duration in minutes (60, 1440, 4320, 10080).</param>
	/// <returns>The current <see cref="ICreateForumPostRestAction"/> instance.</returns>
	ICreateForumPostRestAction SetAutoArchiveDuration(int? duration);

	/// <summary>
	/// Sets the rate limit per user for the thread.
	/// </summary>
	/// <param name="rateLimitPerUser">The rate limit in seconds.</param>
	/// <returns>The current <see cref="ICreateForumPostRestAction"/> instance.</returns>
	ICreateForumPostRestAction SetRateLimitPerUser(int? rateLimitPerUser);

	/// <summary>
	/// Adds embeds to the initial message.
	/// </summary>
	/// <param name="embeds">The embeds to add.</param>
	/// <returns>The current <see cref="ICreateForumPostRestAction"/> instance.</returns>
	ICreateForumPostRestAction AddEmbeds(params DiscoSdk.Models.Messages.Embeds.Embed[] embeds);

	/// <summary>
	/// Sets the applied tags for the forum post.
	/// </summary>
	/// <param name="tagIds">The IDs of the tags to apply.</param>
	/// <returns>The current <see cref="ICreateForumPostRestAction"/> instance.</returns>
	ICreateForumPostRestAction SetAppliedTags(params DiscordId[] tagIds);
}

