using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Models;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Hosting.Models.Requests.Messages;
using DiscoSdk.Rest;
using System.Text;

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
    public Task<Message> GetAsync(
        WebhookIdentity identity,
        Snowflake messageId,
        Snowflake? threadId = null,
        CancellationToken cancellationToken = default)
    {
        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var route = new DiscordRoute(
            "webhooks/{webhook_id}/{webhook_token}/messages/{message_id}",
            identity.Id,
            identity.Token,
            messageId
        );

        var routeString = AppendThreadQueryIfPresent(route.ToString(), threadId);
        var routeWithQuery = new DiscordRoute(routeString);

        return client.SendAsync<Message>(routeWithQuery, HttpMethod.Get, body: null, cancellationToken);
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

    public Task<Message> EditOriginalResponseAsync(
        WebhookIdentity identity,
        MessageEditRequest request,
        IReadOnlyList<MessageFile>? files = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var route = new DiscordRoute("webhooks/{webhook_id}/{webhook_token}/messages/@original", identity.Id, identity.Token);
        if (files == null || files.Count == 0)
            return client.SendAsync<Message>(route, HttpMethod.Patch, request, cancellationToken);

        return client.SendMultipartAsync<Message>(route, HttpMethod.Patch, request, files, cancellationToken);
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
        var sb = new StringBuilder(path.Length + 32).Append(path);
        var hasQuery = false;

        AppendQueryParam(sb, "wait", wait ? "true" : "false", ref hasQuery);

        if (threadId.HasValue && threadId.Value != default)
            AppendQueryParam(sb, "thread_id", threadId.Value.ToString(), ref hasQuery);

        return sb.ToString();
    }

    private static string AppendThreadQueryIfPresent(string path, Snowflake? threadId)
    {
        if (!threadId.HasValue || threadId.Value == default)
            return path;

        var sb = new StringBuilder(path.Length + 32).Append(path);
        var hasQuery = false;
        AppendQueryParam(sb, "thread_id", threadId.Value.ToString(), ref hasQuery);
        return sb.ToString();
    }

    private static void AppendQueryParam(StringBuilder sb, string name, string value, ref bool hasQuery)
    {
        sb.Append(hasQuery ? '&' : '?');
        sb.Append(name);
        sb.Append('=');
        sb.Append(value);
        hasQuery = true;
    }
}
