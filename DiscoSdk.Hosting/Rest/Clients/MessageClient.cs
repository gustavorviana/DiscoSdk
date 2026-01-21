using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Requests.Messages;
using DiscoSdk.Models.Users;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord message operations (create, get, edit, delete, reactions, etc.).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageClient"/> class.
/// </remarks>
/// <param name="client">The REST client base to use for requests.</param>
internal class MessageClient(IDiscordRestClient client)
{
    /// <summary>
    /// Creates a new message in the specified channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to send the message to.</param>
    /// <param name="request">The message creation request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The created message.</returns>
    public Task<Message> CreateAsync(Snowflake channelId, MessageCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        ArgumentNullException.ThrowIfNull(request);

        var route = new DiscordRoute("channels/{channel_id}/messages", channelId);
        return client.SendAsync<Message>(route, HttpMethod.Post, request, cancellationToken);
    }

    /// <summary>
    /// Gets a message by its ID from the specified channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The message, or null if not found.</returns>
    public Task<Message> GetAsync(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("channels/{channel_id}/messages/{message_id}", channelId, messageId);
        return client.SendAsync<Message>(route, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Edits an existing message.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to edit.</param>
    /// <param name="request">The message edit request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The edited message.</returns>
    public Task<Message> EditAsync(Snowflake channelId,
        Snowflake messageId,
        MessageEditRequest request,
        IReadOnlyList<MessageFile>? files = null,
        CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        ArgumentNullException.ThrowIfNull(request);

        var route = new DiscordRoute("channels/{channel_id}/messages/{message_id}", channelId, messageId);
        if (files == null || files.Count == 0)
            return client.SendAsync<Message>(route, HttpMethod.Patch, request, cancellationToken);

        return client.SendMultipartAsync<Message>(route, HttpMethod.Patch, request, files, cancellationToken);
    }

    /// <summary>
    /// Deletes a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to delete.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAsync(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("channels/{channel_id}/messages/{message_id}", channelId, messageId);
        return client.SendAsync(route, HttpMethod.Delete, null, cancellationToken);
    }

    /// <summary>
    /// Crossposts a message in a news channel.
    /// </summary>
    /// <param name="channelId">The ID of the news channel containing the message.</param>
    /// <param name="messageId">The ID of the message to crosspost.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The crossposted message.</returns>
    public Task<Message> CrosspostAsync(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("channels/{channel_id}/messages/{message_id}/crosspost", channelId, messageId);
        return client.SendAsync<Message>(route, HttpMethod.Post, null, cancellationToken);
    }

    /// <summary>
    /// Adds a reaction to a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to react to.</param>
    /// <param name="emoji">The emoji to react with (URL-encoded if custom emoji).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddReactionAsync(Snowflake channelId, Snowflake messageId, string emoji, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        if (string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

        var encodedEmoji = Uri.EscapeDataString(emoji);
        var route = new DiscordRoute($"channels/{{channel_id}}/messages/{{message_id}}/reactions/{encodedEmoji}/@me", channelId, messageId);
        return client.SendAsync(route, HttpMethod.Put, null, cancellationToken);
    }

    /// <summary>
    /// Removes a reaction from a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to remove the reaction from.</param>
    /// <param name="emoji">The emoji to remove (URL-encoded if custom emoji).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveReactionAsync(Snowflake channelId, Snowflake messageId, string emoji, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        if (string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

        var encodedEmoji = Uri.EscapeDataString(emoji);
        var route = new DiscordRoute($"channels/{{channel_id}}/messages/{{message_id}}/reactions/{encodedEmoji}/@me", channelId, messageId);
        return client.SendAsync(route, HttpMethod.Delete, null, cancellationToken);
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
    public Task RemoveUserReactionAsync(Snowflake channelId, Snowflake messageId, string emoji, Snowflake userId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        if (string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

        if (userId.Empty)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var encodedEmoji = Uri.EscapeDataString(emoji);
        var route = new DiscordRoute($"channels/{{channel_id}}/messages/{{message_id}}/reactions/{encodedEmoji}/{{user_id}}", channelId, messageId, userId);
        return client.SendAsync(route, HttpMethod.Delete, null, cancellationToken);
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
    public Task<User[]> GetReactionsAsync(Snowflake channelId, Snowflake messageId, string emoji, string? after = null, int? limit = null, CancellationToken cancellationToken = default)
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
        var route = new DiscordRoute($"channels/{{channel_id}}/messages/{{message_id}}/reactions/{encodedEmoji}{query}", channelId, messageId);
        return client.SendAsync<User[]>(route, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Deletes all reactions of a specific emoji from a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="emoji">The emoji to remove all reactions for (URL-encoded if custom emoji).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAllReactionsForEmojiAsync(Snowflake channelId, Snowflake messageId, string emoji, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        if (string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Emoji cannot be null or empty.", nameof(emoji));

        var encodedEmoji = Uri.EscapeDataString(emoji);
        var route = new DiscordRoute($"channels/{{channel_id}}/messages/{{message_id}}/reactions/{encodedEmoji}", channelId, messageId);
        return client.SendAsync(route, HttpMethod.Delete, null, cancellationToken);
    }

    /// <summary>
    /// Deletes all reactions from a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAllReactionsAsync(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("channels/{channel_id}/messages/{message_id}/reactions", channelId, messageId);
        return client.SendAsync(route, HttpMethod.Delete, null, cancellationToken);
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
    public Task<Message[]> GetMessagesAsync(Snowflake channelId, int? limit = null, Snowflake? around = null, Snowflake? before = null, Snowflake? after = null, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

        var queryParams = new List<string>();

        if (limit.HasValue)
            queryParams.Add($"limit={limit.Value}");

        if (around.HasValue)
            queryParams.Add($"around={around.Value}");

        if (before.HasValue)
            queryParams.Add($"before={before.Value}");

        if (after.HasValue)
            queryParams.Add($"after={after.Value}");

        var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
        var route = new DiscordRoute($"channels/{{channel_id}}/messages{query}", channelId);
        return client.SendAsync<Message[]>(route, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Deletes multiple messages from the specified channel in a single request.
    /// </summary>
    /// <param name="channelId">The ID of the channel to delete messages from.</param>
    /// <param name="messageIds">The IDs of the messages to delete (2-100 messages).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task BulkDeleteMessagesAsync(Snowflake channelId, Snowflake[] messageIds, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageIds == null || messageIds.Length == 0)
            throw new ArgumentException("Message IDs cannot be null or empty.", nameof(messageIds));

        if (messageIds.Length < 2 || messageIds.Length > 100)
            throw new ArgumentOutOfRangeException(nameof(messageIds), "Must delete between 2 and 100 messages.");

        var route = new DiscordRoute("channels/{channel_id}/messages/bulk-delete", channelId);
        var request = new { messages = messageIds.Select(id => id.ToString()).ToArray() };
        return client.SendAsync(route, HttpMethod.Post, request, cancellationToken);
    }

    /// <summary>
    /// Triggers the typing indicator in the specified channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to trigger typing in.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task TriggerTypingAsync(Snowflake channelId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        var route = new DiscordRoute("channels/{channel_id}/typing", channelId);
        return client.SendAsync(route, HttpMethod.Post, cancellationToken);
    }

    /// <summary>
    /// Pins a message in the specified channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to pin.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task PinAsync(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("channels/{channel_id}/pins/{message_id}", channelId, messageId);
        return client.SendAsync(route, HttpMethod.Put, cancellationToken);
    }

    /// <summary>
    /// Unpins a message from the specified channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the message.</param>
    /// <param name="messageId">The ID of the message to unpin.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UnpinAsync(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("channels/{channel_id}/pins/{message_id}", channelId, messageId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Gets all pinned messages in the specified channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to get pinned messages from.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An array of pinned messages.</returns>
    public Task<Message[]> GetPinnedMessagesAsync(Snowflake channelId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        var route = new DiscordRoute("channels/{channel_id}/pins", channelId);
        return client.SendAsync<Message[]>(route, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Ends a poll by its message ID.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the poll message.</param>
    /// <param name="messageId">The ID of the message containing the poll.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The updated message with the poll ended.</returns>
    public Task<Message> EndPollAsync(Snowflake channelId, Snowflake messageId, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("channels/{channel_id}/polls/{message_id}/expire", channelId, messageId);
        return client.SendAsync<Message>(route, HttpMethod.Post, null, cancellationToken);
    }

    /// <summary>
    /// Gets poll voters by message ID and answer ID.
    /// </summary>
    /// <param name="channelId">The ID of the channel containing the poll message.</param>
    /// <param name="messageId">The ID of the message containing the poll.</param>
    /// <param name="answerId">The ID of the poll answer.</param>
    /// <param name="after">Get voters after this user ID.</param>
    /// <param name="limit">Maximum number of voters to return (1-100, default 25).</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An array of users who voted for the specified answer.</returns>
    public Task<User[]> GetPollVotersAsync(Snowflake channelId, Snowflake messageId, ulong answerId, string? after = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

        var queryParams = new List<string>();

        if (after != null)
            queryParams.Add($"after={Uri.EscapeDataString(after)}");

        if (limit.HasValue)
            queryParams.Add($"limit={limit.Value}");

        var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
        var route = new DiscordRoute($"channels/{{channel_id}}/polls/{{message_id}}/answers/{{answer_id}}{query}", channelId, messageId, answerId);
        return client.SendAsync<User[]>(route, HttpMethod.Get, null, cancellationToken);
    }
}

