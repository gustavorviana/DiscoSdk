using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Requests;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord message operations (create, get, edit, delete, reactions, etc.).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageClient"/> class.
/// </remarks>
/// <param name="client">The REST client base to use for requests.</param>
internal class MessageClient(IDiscordRestClientBase client)
{
	/// <summary>
	/// Creates a new message in the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to send the message to.</param>
	/// <param name="request">The message creation request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The created message.</returns>
	public Task<Message> CreateAsync(DiscordId channelId, MessageCreateRequest request, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"channels/{channelId}/messages";
		return client.SendJsonAsync<Message>(path, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Gets a message by its ID from the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The message, or null if not found.</returns>
	public Task<Message> GetAsync(DiscordId channelId, DiscordId messageId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		var path = $"channels/{channelId}/messages/{messageId}";
		return client.SendJsonAsync<Message>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Edits an existing message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to edit.</param>
	/// <param name="request">The message edit request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The edited message.</returns>
	public Task<Message> EditAsync(DiscordId channelId, DiscordId messageId, MessageEditRequest request, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"channels/{channelId}/messages/{messageId}";
		return client.SendJsonAsync<Message>(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Deletes a message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task DeleteAsync(DiscordId channelId, DiscordId messageId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		var path = $"channels/{channelId}/messages/{messageId}";
		return client.SendJsonAsync(path, HttpMethod.Delete, null, cancellationToken);
	}

	/// <summary>
	/// Crossposts a message in a news channel.
	/// </summary>
	/// <param name="channelId">The ID of the news channel containing the message.</param>
	/// <param name="messageId">The ID of the message to crosspost.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The crossposted message.</returns>
	public Task<Message> CrosspostAsync(DiscordId channelId, DiscordId messageId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		var path = $"channels/{channelId}/messages/{messageId}/crosspost";
		return client.SendJsonAsync<Message>(path, HttpMethod.Post, null, cancellationToken);
	}

	/// <summary>
	/// Adds a reaction to a message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to react to.</param>
	/// <param name="emoji">The emoji to react with (URL-encoded if custom emoji).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task AddReactionAsync(DiscordId channelId, DiscordId messageId, string emoji, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		if (string.IsNullOrWhiteSpace(emoji))
			throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

		var encodedEmoji = Uri.EscapeDataString(emoji);
		var path = $"channels/{channelId}/messages/{messageId}/reactions/{encodedEmoji}/@me";
		return client.SendJsonAsync(path, HttpMethod.Put, null, cancellationToken);
	}

	/// <summary>
	/// Removes a reaction from a message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to remove the reaction from.</param>
	/// <param name="emoji">The emoji to remove (URL-encoded if custom emoji).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task RemoveReactionAsync(DiscordId channelId, DiscordId messageId, string emoji, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		if (string.IsNullOrWhiteSpace(emoji))
			throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

		var encodedEmoji = Uri.EscapeDataString(emoji);
		var path = $"channels/{channelId}/messages/{messageId}/reactions/{encodedEmoji}/@me";
		return client.SendJsonAsync(path, HttpMethod.Delete, null, cancellationToken);
	}

	/// <summary>
	/// Removes a reaction from a message for a specific user.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to remove the reaction from.</param>
	/// <param name="emoji">The emoji to remove (URL-encoded if custom emoji).</param>
	/// <param name="userId">The ID of the user whose reaction to remove. Use "@me" for the current user.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task RemoveUserReactionAsync(DiscordId channelId, DiscordId messageId, string emoji, string userId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		if (string.IsNullOrWhiteSpace(emoji))
			throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

		if (string.IsNullOrWhiteSpace(userId))
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var encodedEmoji = Uri.EscapeDataString(emoji);
		var path = $"channels/{channelId}/messages/{messageId}/reactions/{encodedEmoji}/{userId}";
		return client.SendJsonAsync(path, HttpMethod.Delete, null, cancellationToken);
	}

	/// <summary>
	/// Gets all users who reacted with a specific emoji to a message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to get reactions for (URL-encoded if custom emoji).</param>
	/// <param name="after">Get users after this user ID.</param>
	/// <param name="limit">Maximum number of users to return (1-100, default 25).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of users who reacted.</returns>
	public Task<User[]> GetReactionsAsync(DiscordId channelId, DiscordId messageId, string emoji, string? after = null, int? limit = null, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		if (string.IsNullOrWhiteSpace(emoji))
			throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

		if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		var encodedEmoji = Uri.EscapeDataString(emoji);
		var queryParams = new List<string>();

		if (after != null)
			queryParams.Add($"after={Uri.EscapeDataString(after)}");

		if (limit.HasValue)
			queryParams.Add($"limit={limit.Value}");

		var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
		var path = $"channels/{channelId}/messages/{messageId}/reactions/{encodedEmoji}{query}";
		return client.SendJsonAsync<User[]>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Deletes all reactions of a specific emoji from a message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task DeleteAllReactionsForEmojiAsync(DiscordId channelId, DiscordId messageId, string emoji, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		if (string.IsNullOrWhiteSpace(emoji))
			throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

		var encodedEmoji = Uri.EscapeDataString(emoji);
		var path = $"channels/{channelId}/messages/{messageId}/reactions/{encodedEmoji}";
		return client.SendJsonAsync(path, HttpMethod.Delete, null, cancellationToken);
	}

	/// <summary>
	/// Deletes all reactions from a message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task DeleteAllReactionsAsync(DiscordId channelId, DiscordId messageId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		var path = $"channels/{channelId}/messages/{messageId}/reactions";
		return client.SendJsonAsync(path, HttpMethod.Delete, null, cancellationToken);
	}
}

