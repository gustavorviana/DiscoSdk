using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Requests.Messages;
using DiscoSdk.Rest;

namespace DiscoSdk.Hosting.Rest.Clients;

/// <summary>
/// Provides operations for Discord webhooks.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="WebhookMessageClient"/>.
/// </remarks>
/// <param name="client">REST client used to send requests.</param>
internal sealed class WebhookMessageClient(IDiscordRestClient client)
{
    public IDiscordRestClient Client => client;
    public Task<WebhookInfo> GetInfoAsync(WebhookIdentity identity, CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}", identity.Id, identity.Token);
        return client.SendAsync<WebhookInfo>(route, HttpMethod.Get, cancellationToken);
    }

    public Task<Message?> ExecuteAsync(
        WebhookIdentity identity,
        ExecuteWebhookRequest request,
        IReadOnlyList<MessageFile>? files = null,
        bool wait = true,
        Snowflake? threadId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}", identity.Id, identity.Token);
        var routeString = AppendQuery(route.ToString(), wait, threadId);
        var routeWithQuery = new DiscordRoute(routeString);

        if (files == null || files.Count == 0)
            return client.SendAsync<Message?>(routeWithQuery, HttpMethod.Post, request, cancellationToken);

        return client.SendMultipartAsync<Message?>(routeWithQuery, HttpMethod.Post, request, files, cancellationToken);
    }

    public Task<Message> EditAsync(
        WebhookIdentity identity,
        Snowflake messageId,
        MessageEditRequest request,
        IReadOnlyList<MessageFile>? files = null,
        Snowflake? threadId = null,
        CancellationToken cancellationToken = default)
    {
        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        ArgumentNullException.ThrowIfNull(request);

        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}/messages/{message_id}", identity.Id, identity.Token, messageId);
        var routeString = AppendThreadQueryIfPresent(route.ToString(), threadId);
        var routeWithQuery = new DiscordRoute(routeString);

        if (files == null || files.Count == 0)
            return client.SendAsync<Message>(routeWithQuery, HttpMethod.Patch, request, cancellationToken);

        return client.SendMultipartAsync<Message>(routeWithQuery, HttpMethod.Patch, request, files, cancellationToken);

    }

    public Task DeleteAsync(
        WebhookIdentity identity,
        Snowflake messageId,
        Snowflake? threadId = null,
        CancellationToken cancellationToken = default)
    {
        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}/messages/{message_id}", identity.Id, identity.Token, messageId);
        var routeString = AppendThreadQueryIfPresent(route.ToString(), threadId);
        var routeWithQuery = new DiscordRoute(routeString);

        return client.SendAsync(routeWithQuery, HttpMethod.Delete, body: null, cancellationToken);
    }

    public async Task<Message> GetOriginalResponseAsync(
        WebhookIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}/messages/@original", identity.Id, identity.Token);
        return await client.SendAsync<Message>(route, HttpMethod.Get, null, cancellationToken);
    }

    public async Task<Message> EditOriginalResponseAsync(
        WebhookIdentity identity,
        MessageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}/messages/@original", identity.Id, identity.Token);
        return await client.SendAsync<Message>(route, HttpMethod.Patch, request, cancellationToken);
    }

    public async Task DeleteOriginalResponseAsync(
        WebhookIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}/messages/@original", identity.Id, identity.Token);
        await client.SendAsync(route, HttpMethod.Delete, cancellationToken);
    }

    private static string AppendQuery(string path, bool wait, Snowflake? threadId)
    {
        var hasQuery = false;

        path = AppendQueryParam(path, "wait", wait ? "true" : "false", ref hasQuery);

        if (threadId.HasValue && threadId.Value != default)
            path = AppendQueryParam(path, "thread_id", threadId.Value.ToString(), ref hasQuery);

        return path;
    }

    private static string AppendThreadQueryIfPresent(string path, Snowflake? threadId)
    {
        if (!threadId.HasValue || threadId.Value == default)
            return path;

        var hasQuery = false;
        return AppendQueryParam(path, "thread_id", threadId.Value.ToString(), ref hasQuery);
    }

    private static string AppendQueryParam(string path, string name, string value, ref bool hasQuery)
    {
        path += hasQuery ? "&" : "?";
        path += name;
        path += "=";
        path += value;
        hasQuery = true;
        return path;
    }
}
