using DiscoSdk.Exceptions;
using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest;
using System.Text;
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
        var route = new DiscordRoute($"guilds/{{guild_id}}/members{query}", guildId);
        return client.SendAsync<GuildMember[]>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/members/{user_id}", guildId, userId);
        try
        {
            return await client.SendAsync<GuildMember>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/bans/{user_id}", guildId, userId);
        try
        {
            return await client.SendAsync<Ban>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/channels", guildId);
        return client.SendAsync<Channel[]>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/roles", guildId);
        return client.SendAsync<Role[]>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}", guildId);
        try
        {
            return await client.SendAsync<Guild>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/voice-states/@me", guildId);
        var request = new { channel_id = channelId.ToString(), request_to_speak_timestamp = DateTimeOffset.UtcNow.ToString("o") };
        return client.SendAsync(route, HttpMethod.Patch, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/voice-states/@me", guildId);
        var request = new { channel_id = channelId.ToString(), request_to_speak_timestamp = (string?)null };
        return client.SendAsync(route, HttpMethod.Patch, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/channels", guildId);
        return client.SendAsync<Channel>(route, HttpMethod.Post, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/bans/{user_id}", guildId, userId);
        return client.SendAsync(route, HttpMethod.Put, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/bans/{user_id}", guildId, userId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/members/{user_id}", guildId, userId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/emojis", guildId);
        return client.SendAsync<Emoji>(route, HttpMethod.Post, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/emojis/{emoji_id}", guildId, emojiId);
        return client.SendAsync<Emoji>(route, HttpMethod.Patch, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/emojis/{emoji_id}", guildId, emojiId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}", guildId);
        return client.SendAsync<Guild>(route, HttpMethod.Patch, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}", guildId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
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

        var route = new DiscordRoute("users/@me/guilds/{guild_id}", guildId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
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
        var route = new DiscordRoute($"guilds/{{guild_id}}/audit-logs{query}", guildId);
        var auditLog = await client.SendAsync<AuditLog>(route, HttpMethod.Get, null, cancellationToken);
        return auditLog.AuditLogEntries;
    }

    public async Task<VanityUrl?> GetVanityUrlAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        if (guildId == default)
            throw new ArgumentException("Guild ID cannot be null or empty.", nameof(guildId));

        var route = new DiscordRoute("guilds/{guild_id}/vanity-url", guildId);
        return await client.SendAsync<VanityUrl?>(route, HttpMethod.Get, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/invites", guildId);
        return client.SendAsync<Invite[]>(route, HttpMethod.Get, null, cancellationToken);
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
        var route = new DiscordRoute($"guilds/{{guild_id}}/prune{query}", guildId);
        var response = await client.SendAsync<Dictionary<string, object>>(route, HttpMethod.Get, null, cancellationToken);
        
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

        var route = new DiscordRoute("guilds/{guild_id}/prune", guildId);
        var response = await client.SendAsync<Dictionary<string, object>>(route, HttpMethod.Post, request, cancellationToken);
        
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

        var route = new DiscordRoute("guilds/{guild_id}/regions", guildId);
        return client.SendAsync<VoiceRegion[]>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/preview", guildId);
        return client.SendAsync<GuildPreview>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/widget.json", guildId);
        return client.SendAsync<GuildWidget>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/widget", guildId);
        return client.SendAsync<GuildWidget>(route, HttpMethod.Patch, request, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/welcome-screen", guildId);
        return client.SendAsync<WelcomeScreen>(route, HttpMethod.Get, null, cancellationToken);
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

        var route = new DiscordRoute("guilds/{guild_id}/welcome-screen", guildId);
        return client.SendAsync<WelcomeScreen>(route, HttpMethod.Patch, request, cancellationToken);
    }

    /// <summary>
    /// Modifies the positions of a set of channel objects in the guild. <paramref name="positions"/> is
    /// a list of <c>{ id, position, lock_permissions, parent_id }</c>-shaped objects.
    /// </summary>
    public Task ModifyChannelPositionsAsync(Snowflake guildId, IEnumerable<object> positions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(positions);
        var route = new DiscordRoute("guilds/{guild_id}/channels", guildId);
        return client.SendAsync(route, HttpMethod.Patch, positions, cancellationToken);
    }

    /// <summary>
    /// Lists bans in a guild with pagination support.
    /// </summary>
    public Task<Ban[]> GetBansAsync(Snowflake guildId, int? limit = null, Snowflake? before = null, Snowflake? after = null, CancellationToken cancellationToken = default)
    {
        if (limit.HasValue && (limit.Value < 1 || limit.Value > 1000))
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 1000.");

        var sb = new StringBuilder("guilds/{guild_id}/bans");
        var hasQuery = false;
        void Append(string k, string v) { sb.Append(hasQuery ? '&' : '?').Append(k).Append('=').Append(v); hasQuery = true; }
        if (limit.HasValue) Append("limit", limit.Value.ToString());
        if (before.HasValue) Append("before", before.Value.ToString());
        if (after.HasValue) Append("after", after.Value.ToString());

        var route = new DiscordRoute(sb.ToString(), guildId);
        return client.SendAsync<Ban[]>(route, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Bulk-bans up to 200 users from a guild in a single call. Returns the user IDs that were banned
    /// and those that failed (via the response object's <c>banned_users</c> / <c>failed_users</c> fields).
    /// </summary>
    public Task<JsonElement> BulkBanAsync(Snowflake guildId, IEnumerable<Snowflake> userIds, int? deleteMessageSeconds = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        var route = new DiscordRoute("guilds/{guild_id}/bulk-ban", guildId);
        var body = deleteMessageSeconds.HasValue
            ? (object)new { user_ids = userIds.Select(u => u.ToString()).ToArray(), delete_message_seconds = deleteMessageSeconds.Value }
            : new { user_ids = userIds.Select(u => u.ToString()).ToArray() };
        return client.SendAsync<JsonElement>(route, HttpMethod.Post, body, cancellationToken);
    }

    /// <summary>
    /// Adds a user to a guild using an OAuth2 access token granted with the <c>guilds.join</c> scope.
    /// Returns the resulting guild member, or null if the user was already a member.
    /// </summary>
    public async Task<GuildMember?> AddMemberAsync(Snowflake guildId, Snowflake userId, string accessToken, string? nick = null, IEnumerable<Snowflake>? roles = null, bool? mute = null, bool? deaf = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

        var body = new Dictionary<string, object?> { ["access_token"] = accessToken };
        if (nick is not null) body["nick"] = nick;
        if (roles is not null) body["roles"] = roles.Select(r => r.ToString()).ToArray();
        if (mute.HasValue) body["mute"] = mute.Value;
        if (deaf.HasValue) body["deaf"] = deaf.Value;

        var route = new DiscordRoute("guilds/{guild_id}/members/{user_id}", guildId, userId);
        try
        {
            return await client.SendAsync<GuildMember>(route, HttpMethod.Put, body, cancellationToken);
        }
        catch (DiscordApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return null;
        }
    }

    /// <summary>
    /// Modifies the bot's own nickname in a guild.
    /// </summary>
    public Task<GuildMember> ModifyCurrentMemberAsync(Snowflake guildId, string? nick, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/members/@me", guildId);
        return client.SendAsync<GuildMember>(route, HttpMethod.Patch, new { nick }, cancellationToken);
    }

    /// <summary>
    /// Modifies attributes of a guild member. The body should be a partial shape with any subset of
    /// <c>nick</c>, <c>roles</c>, <c>mute</c>, <c>deaf</c>, <c>channel_id</c>, <c>communication_disabled_until</c>, <c>flags</c>.
    /// </summary>
    public Task<GuildMember> ModifyMemberAsync(Snowflake guildId, Snowflake userId, object request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var route = new DiscordRoute("guilds/{guild_id}/members/{user_id}", guildId, userId);
        return client.SendAsync<GuildMember>(route, HttpMethod.Patch, request, cancellationToken);
    }

    /// <summary>
    /// Adds a role to a guild member.
    /// </summary>
    public Task AddMemberRoleAsync(Snowflake guildId, Snowflake userId, Snowflake roleId, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/members/{user_id}/roles/{role_id}", guildId, userId, roleId);
        return client.SendAsync(route, HttpMethod.Put, body: null, cancellationToken);
    }

    /// <summary>
    /// Removes a role from a guild member.
    /// </summary>
    public Task RemoveMemberRoleAsync(Snowflake guildId, Snowflake userId, Snowflake roleId, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/members/{user_id}/roles/{role_id}", guildId, userId, roleId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Modifies the required MFA level for the guild. Caller must be the guild owner.
    /// </summary>
    public Task ModifyMfaLevelAsync(Snowflake guildId, MfaLevel level, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/mfa", guildId);
        return client.SendAsync(route, HttpMethod.Post, new { level = (int)level }, cancellationToken);
    }

    /// <summary>
    /// Lists the integrations attached to the guild (Twitch / YouTube subs, Discord bots, etc.).
    /// </summary>
    public Task<Integration[]> ListIntegrationsAsync(Snowflake guildId, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/integrations", guildId);
        return client.SendAsync<Integration[]>(route, HttpMethod.Get, null, cancellationToken);
    }

    /// <summary>
    /// Deletes a guild integration and removes the linked role.
    /// </summary>
    public Task DeleteIntegrationAsync(Snowflake guildId, Snowflake integrationId, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("guilds/{guild_id}/integrations/{integration_id}", guildId, integrationId);
        return client.SendAsync(route, HttpMethod.Delete, cancellationToken);
    }

    /// <summary>
    /// Modifies the guild's incident actions — temporarily disables invites and/or DMs until the
    /// specified timestamps. Pass <c>null</c> to clear the suspension.
    /// </summary>
    public Task<IncidentsData> ModifyIncidentActionsAsync(Snowflake guildId, DateTimeOffset? invitesDisabledUntil, DateTimeOffset? dmsDisabledUntil, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            invites_disabled_until = invitesDisabledUntil?.ToString("o"),
            dms_disabled_until = dmsDisabledUntil?.ToString("o")
        };
        var route = new DiscordRoute("guilds/{guild_id}/incident-actions", guildId);
        return client.SendAsync<IncidentsData>(route, HttpMethod.Put, body, cancellationToken);
    }

    /// <summary>
    /// Searches the guild's member list by username/nickname prefix. Returns up to <paramref name="limit"/>
    /// members (1–1000, default 1).
    /// </summary>
    public Task<GuildMember[]> SearchMembersAsync(Snowflake guildId, string query, int? limit = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        if (limit.HasValue && (limit.Value < 1 || limit.Value > 1000))
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 1000.");

        var sb = new StringBuilder("guilds/{guild_id}/members/search?query=").Append(Uri.EscapeDataString(query));
        if (limit.HasValue) sb.Append("&limit=").Append(limit.Value);

        var route = new DiscordRoute(sb.ToString(), guildId);
        return client.SendAsync<GuildMember[]>(route, HttpMethod.Get, null, cancellationToken);
    }
}