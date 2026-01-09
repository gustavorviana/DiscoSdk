using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Messages;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord channel operations (get, edit, delete, threads, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
/// <param name="messageClient">The message client for message-related operations.</param>
internal class ChannelClient(IDiscordRestClientBase client, MessageClient messageClient)
{
	/// <summary>
	/// Gets a channel by its ID.
	/// </summary>
	/// <param name="channelId">The ID of the channel to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The channel, or null if not found.</returns>
	public Task<Channel> GetAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}";
		return client.SendJsonAsync<Channel>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Edits a channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to edit.</param>
	/// <param name="request">The channel edit request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The edited channel.</returns>
	public Task<Channel> EditAsync(DiscordId channelId, object request, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"channels/{channelId}";
		return client.SendJsonAsync<Channel>(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Deletes a channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The deleted channel.</returns>
	public Task<Channel> DeleteAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}";
		return client.SendJsonAsync<Channel>(path, HttpMethod.Delete, null, cancellationToken);
	}

	/// <summary>
	/// Gets messages from the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to get messages from.</param>
	/// <param name="limit">Maximum number of messages to return (1-100, default 50).</param>
	/// <param name="around">Get messages around this message ID.</param>
	/// <param name="before">Get messages before this message ID.</param>
	/// <param name="after">Get messages after this message ID.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of messages.</returns>
	public Task<Message[]> GetMessagesAsync(DiscordId channelId, int? limit = null, DiscordId? around = null, DiscordId? before = null, DiscordId? after = null, CancellationToken cancellationToken = default)
	{
		return messageClient.GetMessagesAsync(channelId, limit, around, before, after, cancellationToken);
	}

	/// <summary>
	/// Gets a message by its ID from the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The message, or null if not found.</returns>
	public Task<Message> GetMessageAsync(DiscordId channelId, DiscordId messageId, CancellationToken cancellationToken = default)
	{
		return messageClient.GetAsync(channelId, messageId, cancellationToken);
	}

	/// <summary>
	/// Deletes a message from the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to delete.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task DeleteMessageAsync(DiscordId channelId, DiscordId messageId, CancellationToken cancellationToken = default)
	{
		return messageClient.DeleteAsync(channelId, messageId, cancellationToken);
	}

	/// <summary>
	/// Deletes multiple messages from the specified channel in a single request.
	/// </summary>
	/// <param name="channelId">The ID of the channel to delete messages from.</param>
	/// <param name="messageIds">The IDs of the messages to delete (2-100 messages).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task BulkDeleteMessagesAsync(DiscordId channelId, DiscordId[] messageIds, CancellationToken cancellationToken = default)
	{
		return messageClient.BulkDeleteMessagesAsync(channelId, messageIds, cancellationToken);
	}

	/// <summary>
	/// Follows an announcement channel to send messages to a target channel.
	/// </summary>
	/// <param name="channelId">The ID of the announcement channel to follow.</param>
	/// <param name="targetChannelId">The ID of the target channel to follow to.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The followed channel information.</returns>
	public Task<FollowedChannel> FollowAsync(DiscordId channelId, DiscordId targetChannelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (targetChannelId == default)
			throw new ArgumentException("Target channel ID cannot be null or empty.", nameof(targetChannelId));

		var path = $"channels/{channelId}/followers";
		var request = new { webhook_channel_id = targetChannelId.ToString() };
		return client.SendJsonAsync<FollowedChannel>(path, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Triggers the typing indicator in the specified channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to trigger typing in.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task TriggerTypingAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		return messageClient.TriggerTypingAsync(channelId, cancellationToken);
	}

	/// <summary>
	/// Adds the current user to a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task JoinThreadAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}/thread-members/@me";
		return client.SendNoContentAsync(path, HttpMethod.Put, cancellationToken);
	}

	/// <summary>
	/// Removes the current user from a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task LeaveThreadAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}/thread-members/@me";
		return client.SendNoContentAsync(path, HttpMethod.Delete, cancellationToken);
	}

	/// <summary>
	/// Adds a member to a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread.</param>
	/// <param name="userId">The ID of the user to add.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task AddThreadMemberAsync(DiscordId channelId, DiscordId userId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var path = $"channels/{channelId}/thread-members/{userId}";
		return client.SendNoContentAsync(path, HttpMethod.Put, cancellationToken);
	}

	/// <summary>
	/// Removes a member from a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread.</param>
	/// <param name="userId">The ID of the user to remove.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task RemoveThreadMemberAsync(DiscordId channelId, DiscordId userId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var path = $"channels/{channelId}/thread-members/{userId}";
		return client.SendNoContentAsync(path, HttpMethod.Delete, cancellationToken);
	}

	/// <summary>
	/// Gets a thread member by their user ID.
	/// </summary>
	/// <param name="channelId">The ID of the thread.</param>
	/// <param name="userId">The ID of the user to get the thread member for.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The thread member, or null if not found.</returns>
	public Task<ThreadMember?> GetThreadMemberAsync(DiscordId channelId, DiscordId userId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (userId == default)
			throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

		var path = $"channels/{channelId}/thread-members/{userId}";
		return client.SendJsonAsync<ThreadMember>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Gets a list of thread members in a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread.</param>
	/// <param name="limit">Maximum number of thread members to return (1-100, default 25).</param>
	/// <param name="after">Get thread members after this user ID.</param>
	/// <param name="withMember">Whether to include guild member data.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An array of thread members.</returns>
	public Task<ThreadMember[]> GetThreadMembersAsync(DiscordId channelId, int? limit = null, DiscordId? after = null, bool? withMember = null, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		var queryParams = new List<string>();

		if (limit.HasValue)
			queryParams.Add($"limit={limit.Value}");

		if (after.HasValue)
			queryParams.Add($"after={after.Value}");

		if (withMember.HasValue)
			queryParams.Add($"with_member={withMember.Value.ToString().ToLowerInvariant()}");

		var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
		var path = $"channels/{channelId}/thread-members{query}";
		return client.SendJsonAsync<ThreadMember[]>(path, HttpMethod.Get, null, cancellationToken);
	}

	/// <summary>
	/// Archives a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread to archive.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task ArchiveThreadAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}";
		var request = new { archived = true };
		return client.SendJsonAsync(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Unarchives a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread to unarchive.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task UnarchiveThreadAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}";
		var request = new { archived = false };
		return client.SendJsonAsync(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Locks a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread to lock.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task LockThreadAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}";
		var request = new { locked = true };
		return client.SendJsonAsync(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Unlocks a thread.
	/// </summary>
	/// <param name="channelId">The ID of the thread to unlock.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task UnlockThreadAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}";
		var request = new { locked = false };
		return client.SendJsonAsync(path, HttpMethod.Patch, request, cancellationToken);
	}

	/// <summary>
	/// Gets the active threads in a channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to get active threads from.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An object containing active threads.</returns>
	public async Task<Channel[]> GetActiveThreadsAsync(DiscordId channelId, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		var path = $"channels/{channelId}/threads/active";
		var response = await client.SendJsonAsync<ThreadsResponse>(path, HttpMethod.Get, null, cancellationToken);
		return response.Threads;
	}

	/// <summary>
	/// Gets the public archived threads in a channel.
	/// </summary>
	/// <param name="channelId">The ID of the channel to get archived threads from.</param>
	/// <param name="before">Get threads before this timestamp.</param>
	/// <param name="limit">Maximum number of threads to return (1-100, default 50).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An object containing archived threads.</returns>
	public async Task<Channel[]> GetPublicArchivedThreadsAsync(DiscordId channelId, string? before = null, int? limit = null, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		var queryParams = new List<string>();

		if (before != null)
			queryParams.Add($"before={Uri.EscapeDataString(before)}");

		if (limit.HasValue)
			queryParams.Add($"limit={limit.Value}");

		var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
		var path = $"channels/{channelId}/threads/archived/public{query}";
		var response = await client.SendJsonAsync<ThreadsResponse>(path, HttpMethod.Get, null, cancellationToken);
		return response.Threads;
	}

	/// <summary>
	/// Gets the private archived threads in a channel (requires MANAGE_THREADS permission).
	/// </summary>
	/// <param name="channelId">The ID of the channel to get archived threads from.</param>
	/// <param name="before">Get threads before this timestamp.</param>
	/// <param name="limit">Maximum number of threads to return (1-100, default 50).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>An object containing archived threads.</returns>
	public async Task<Channel[]> GetPrivateArchivedThreadsAsync(DiscordId channelId, string? before = null, int? limit = null, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

		var queryParams = new List<string>();

		if (before != null)
			queryParams.Add($"before={Uri.EscapeDataString(before)}");

		if (limit.HasValue)
			queryParams.Add($"limit={limit.Value}");

		var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
		var path = $"channels/{channelId}/threads/archived/private{query}";
		var response = await client.SendJsonAsync<ThreadsResponse>(path, HttpMethod.Get, null, cancellationToken);
		return response.Threads;
	}

	/// <summary>
	/// Creates a forum post (thread) in a forum channel.
	/// </summary>
	/// <param name="channelId">The ID of the forum channel.</param>
	/// <param name="request">The forum post creation request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The created forum post response containing the message.</returns>
	public Task<ForumPost> CreateForumPostAsync(DiscordId channelId, object request, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"channels/{channelId}/threads";
		return client.SendJsonAsync<ForumPost>(path, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Creates a thread from a message.
	/// </summary>
	/// <param name="channelId">The ID of the channel containing the message.</param>
	/// <param name="messageId">The ID of the message to create the thread from.</param>
	/// <param name="request">The thread creation request.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The created thread channel.</returns>
	public Task<Channel> CreateThreadFromMessageAsync(DiscordId channelId, DiscordId messageId, object request, CancellationToken cancellationToken = default)
	{
		if (channelId == default)
			throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

		if (messageId == default)
			throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

		ArgumentNullException.ThrowIfNull(request);

		var path = $"channels/{channelId}/messages/{messageId}/threads";
		return client.SendJsonAsync<Channel>(path, HttpMethod.Post, request, cancellationToken);
	}

	/// <summary>
	/// Response model for thread list endpoints.
	/// </summary>
	private class ThreadsResponse
	{
		public Channel[] Threads { get; set; } = [];
		public ThreadMember[] Members { get; set; } = [];
	}
}