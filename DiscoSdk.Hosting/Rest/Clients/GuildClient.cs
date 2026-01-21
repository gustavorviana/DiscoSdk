using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using System.Text.Json;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Client for Discord guild operations (get members, channels, roles, etc.).
/// </summary>
/// <param name="client">The REST client base to use for requests.</param>
internal class GuildClient(IDiscordRestClient client)
{
    /// <summary>
    /// Gets a list of members in the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="limit">Maximum number of members to return (1-1000, default 1).</param>
    /// <param name="after">Get members after this user ID.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An array of guild members.</returns>
    public Task<GuildMember[]> GetMembersAsync(Snowflake guildId, int? limit = null, Snowflake? after = null, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (limit.HasValue && (limit.Value < 1 || limit.Value > 1000))
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 1000.");

        var queryParams = new List<string>();

        if (limit.HasValue)
            queryParams.Add($"limit={limit.Value}");

        if (after.HasValue)
            queryParams.Add($"after={after.Value}");

        var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
        var path = $"guilds/{guildId}/members{query}";
        return client.SendAsync<GuildMember[]>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Gets a specific member in the specified guild by user ID.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The guild member, or null if the user is not a member of the guild.</returns>
    public async Task<GuildMember?> GetMemberAsync(Snowflake guildId, Snowflake userId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var path = $"guilds/{guildId}/members/{userId}";
        try
        {
            return await client.SendAsync<GuildMember>(path, HttpMethod.Get, null, cancellationToken);
        }
        catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a specific ban in the specified guild by user ID.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="userId">The ID of the banned user.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The ban information, or null if the user is not banned from the guild.</returns>
    public async Task<Ban?> GetBanAsync(Snowflake guildId, Snowflake userId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var path = $"guilds/{guildId}/bans/{userId}";
        try
        {
            return await client.SendAsync<Ban>(path, HttpMethod.Get, null, cancellationToken);
        }
        catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a list of channels in the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An array of channels.</returns>
    public Task<Channel[]> GetChannelsAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}/channels";
        return client.SendAsync<Channel[]>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Gets a list of roles in the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An array of roles.</returns>
    public Task<Role[]> GetRolesAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}/roles";
        return client.SendAsync<Role[]>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Gets a guild by its ID.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The guild, or null if not found.</returns>
    public async Task<Guild?> GetAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}";
        try
        {
            return await client.SendAsync<Guild>(path, HttpMethod.Get, null, cancellationToken);
        }
        catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Requests to speak in a stage channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the stage channel.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RequestToSpeakAsync(Snowflake guildId, Snowflake channelId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        var path = $"guilds/{guildId}/voice-states/@me";
        var request = new { channel_id = channelId.ToString(), request_to_speak_timestamp = DateTimeOffset.UtcNow.ToString("o") };
        return client.SendAsync<object>(path, HttpMethod.Patch, request, cancellationToken);
    }

    /// <summary>
    /// Cancels the request to speak in a stage channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the stage channel.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task CancelRequestToSpeakAsync(Snowflake guildId, Snowflake channelId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (channelId == default)
            throw new ArgumentException("Channel ID cannot be null or empty.", nameof(channelId));

        var path = $"guilds/{guildId}/voice-states/@me";
        var request = new { channel_id = channelId.ToString(), request_to_speak_timestamp = (string?)null };
        return client.SendAsync<object>(path, HttpMethod.Patch, request, cancellationToken);
    }

    /// <summary>
    /// Creates a channel in the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="request">The channel creation request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The created channel.</returns>
    public Task<Channel> CreateChannelAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(request);

        var path = $"guilds/{guildId}/channels";
        return client.SendAsync<Channel>(path, HttpMethod.Post, request, cancellationToken);
    }

    /// <summary>
    /// Bans a member from the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="userId">The ID of the user to ban.</param>
    /// <param name="request">The ban request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task BanMemberAsync(Snowflake guildId, Snowflake userId, object request, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        ArgumentNullException.ThrowIfNull(request);

        var path = $"guilds/{guildId}/bans/{userId}";
        return client.SendAsync(path, HttpMethod.Put, request, cancellationToken);
    }

    /// <summary>
    /// Unbans a user from the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="userId">The ID of the user to unban.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UnbanMemberAsync(Snowflake guildId, Snowflake userId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var path = $"guilds/{guildId}/bans/{userId}";
        return client.SendAsync(path, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Kicks a member from the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="userId">The ID of the user to kick.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task KickMemberAsync(Snowflake guildId, Snowflake userId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (userId == default)
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var path = $"guilds/{guildId}/members/{userId}";
        return client.SendAsync(path, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Creates an emoji in the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="request">The emoji creation request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The created emoji.</returns>
    public Task<Emoji> CreateEmojiAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(request);

        var path = $"guilds/{guildId}/emojis";
        return client.SendAsync<Emoji>(path, HttpMethod.Post, request, cancellationToken);
    }

    /// <summary>
    /// Edits an emoji in the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji to edit.</param>
    /// <param name="request">The emoji edit request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The edited emoji.</returns>
    public Task<Emoji> EditEmojiAsync(Snowflake guildId, Snowflake emojiId, object request, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (emojiId == default)
            throw new ArgumentException("Emoji ID cannot be null or empty.", nameof(emojiId));

        ArgumentNullException.ThrowIfNull(request);

        var path = $"guilds/{guildId}/emojis/{emojiId}";
        return client.SendAsync<Emoji>(path, HttpMethod.Patch, request, cancellationToken);
    }

    /// <summary>
    /// Deletes an emoji from the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="emojiId">The ID of the emoji to delete.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteEmojiAsync(Snowflake guildId, Snowflake emojiId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (emojiId == default)
            throw new ArgumentException("Emoji ID cannot be null or empty.", nameof(emojiId));

        var path = $"guilds/{guildId}/emojis/{emojiId}";
        return client.SendAsync(path, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Edits a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="request">The guild edit request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The edited guild.</returns>
    public Task<Guild> EditAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(request);

        var path = $"guilds/{guildId}";
        return client.SendAsync<Guild>(path, HttpMethod.Patch, request, cancellationToken);
    }

    /// <summary>
    /// Deletes a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}";
        return client.SendAsync(path, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Leaves a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task LeaveAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"users/@me/guilds/{guildId}";
        return client.SendAsync(path, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Gets audit logs from the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="limit">Maximum number of entries to return (1-100, default 50).</param>
    /// <param name="before">Get entries before this entry ID.</param>
    /// <param name="userId">Filter entries by user ID.</param>
    /// <param name="actionType">Filter entries by action type.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The audit log response.</returns>
    public async Task<AuditLogEntry[]> GetAuditLogsAsync(
        Snowflake guildId,
        int? limit = null,
        Snowflake? before = null,
        Snowflake? userId = null,
        AuditLogActionType? actionType = null,
        CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 100.");

        var queryParams = new List<string>();

        if (limit.HasValue)
            queryParams.Add($"limit={limit.Value}");

        if (before.HasValue)
            queryParams.Add($"before={before.Value}");

        if (userId.HasValue)
            queryParams.Add($"user_id={userId.Value}");

        if (actionType.HasValue)
            queryParams.Add($"action_type={(int)actionType.Value}");

        var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : string.Empty;
        var path = $"guilds/{guildId}/audit-logs{query}";
        var auditLog = await client.SendAsync<AuditLog>(path, HttpMethod.Get, null, cancellationToken);
        return auditLog.AuditLogEntries;
    }

    public async Task<VanityUrl?> GetVanityUrlAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        return await client.SendAsync<VanityUrl?>($"guilds/{guildId}/vanity-url", HttpMethod.Get, cancellationToken);
    }

    /// <summary>
    /// Gets all invites for the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An array of invites for the guild.</returns>
    public Task<Invite[]> GetInvitesAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}/invites";
        return client.SendAsync<Invite[]>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Gets the prune count for the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="days">The number of days to count inactive members (1-30).</param>
    /// <param name="includeRoles">The role IDs to include in the prune count.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The prune count response.</returns>
    public async Task<int> GetPruneCountAsync(Snowflake guildId, int days, Snowflake[]? includeRoles = null, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (days < 1 || days > 30)
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be between 1 and 30.");

        var queryParams = new List<string> { $"days={days}" };

        if (includeRoles != null && includeRoles.Length > 0)
            queryParams.Add($"include_roles={string.Join(",", includeRoles.Select(r => r.ToString()))}");

        var query = $"?{string.Join("&", queryParams)}";
        var path = $"guilds/{guildId}/prune{query}";
        var response = await client.SendAsync<Dictionary<string, object>>(path, HttpMethod.Get, null, cancellationToken);
        
        if (response.TryGetValue("pruned", out var prunedValue))
        {
            if (prunedValue is int pruned)
                return pruned;
            if (prunedValue is long prunedLong)
                return (int)prunedLong;
            if (prunedValue is JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                return jsonElement.GetInt32();
        }
        
        return 0;
    }

    /// <summary>
    /// Begins a prune operation on the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="days">The number of days to prune inactive members (1-30).</param>
    /// <param name="includeRoles">The role IDs to include in the prune.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The number of members pruned.</returns>
    public async Task<int> BeginPruneAsync(Snowflake guildId, int days, Snowflake[]? includeRoles = null, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        if (days < 1 || days > 30)
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be between 1 and 30.");

        var request = new Dictionary<string, object?> { ["days"] = days };

        if (includeRoles != null && includeRoles.Length > 0)
            request["include_roles"] = includeRoles.Select(r => r.ToString()).ToArray();

        var path = $"guilds/{guildId}/prune";
        var response = await client.SendAsync<Dictionary<string, object>>(path, HttpMethod.Post, request, cancellationToken);
        
        if (response.TryGetValue("pruned", out var prunedValue))
        {
            if (prunedValue is int pruned)
                return pruned;
            if (prunedValue is long prunedLong)
                return (int)prunedLong;
            if (prunedValue is JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                return jsonElement.GetInt32();
        }
        
        return 0;
    }

    /// <summary>
    /// Gets voice regions available for the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An array of voice regions.</returns>
    public Task<VoiceRegion[]> GetVoiceRegionsAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}/regions";
        return client.SendAsync<VoiceRegion[]>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Gets the preview of the specified guild (for public guilds).
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The guild preview.</returns>
    public Task<GuildPreview> GetPreviewAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}/preview";
        return client.SendAsync<GuildPreview>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Gets the widget of the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The guild widget.</returns>
    public Task<GuildWidget> GetWidgetAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}/widget.json";
        return client.SendAsync<GuildWidget>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Edits the widget of the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="request">The widget edit request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The updated guild widget.</returns>
    public Task<GuildWidget> EditWidgetAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(request);

        var path = $"guilds/{guildId}/widget";
        return client.SendAsync<GuildWidget>(path, HttpMethod.Patch, request, cancellationToken);
    }

    /// <summary>
    /// Gets the welcome screen of the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The welcome screen.</returns>
    public Task<WelcomeScreen> GetWelcomeScreenAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var path = $"guilds/{guildId}/welcome-screen";
        return client.SendAsync<WelcomeScreen>(path, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Edits the welcome screen of the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="request">The welcome screen edit request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The updated welcome screen.</returns>
    public Task<WelcomeScreen> EditWelcomeScreenAsync(Snowflake guildId, object request, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        ArgumentNullException.ThrowIfNull(request);

        var path = $"guilds/{guildId}/welcome-screen";
        return client.SendAsync<WelcomeScreen>(path, HttpMethod.Patch, request, cancellationToken);
    }
}