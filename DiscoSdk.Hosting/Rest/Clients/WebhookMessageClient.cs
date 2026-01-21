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
        var path = $"webhooks/{identity.Id}/{identity.Token}";
        return client.SendAsync<WebhookInfo>(path, HttpMethod.Get, cancellationToken);
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

        var path = $"webhooks/{identity.Id}/{identity.Token}";
        path = AppendQuery(path, wait, threadId);

        if (files == null || files.Count == 0)
            return client.SendAsync<Message?>(path, HttpMethod.Post, request, cancellationToken);

        return client.SendMultipartAsync<Message?>(path, HttpMethod.Post, request, files, cancellationToken);
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

        var path = $"webhooks/{identity.Id}/{identity.Token}/messages/{messageId}";
        path = AppendThreadQueryIfPresent(path, threadId);

        if (files == null || files.Count == 0)
            return client.SendAsync<Message>(path, HttpMethod.Patch, request, cancellationToken);

        return client.SendMultipartAsync<Message>(path, HttpMethod.Patch, request, files, cancellationToken);

    }

    public Task DeleteAsync(
        WebhookIdentity identity,
        Snowflake messageId,
        Snowflake? threadId = null,
        CancellationToken cancellationToken = default)
    {
        if (messageId == default)
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));

        var path = $"webhooks/{identity.Id}/{identity.Token}/messages/{messageId}";
        path = AppendThreadQueryIfPresent(path, threadId);

        return client.SendAsync(path, HttpMethod.Delete, body: null, cancellationToken);
    }

    public async Task<Message> GetOriginalResponseAsync(
        WebhookIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var path = $"webhooks/{identity.Id}/{identity.Token}/messages/@original";
        return await client.SendAsync<Message>(path, HttpMethod.Get, null, cancellationToken);
    }

    public async Task<Message> EditOriginalResponseAsync(
        WebhookIdentity identity,
        MessageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var path = $"webhooks/{identity.Id}/{identity.Token}/messages/@original";
        return await client.SendAsync<Message>(path, HttpMethod.Patch, request, cancellationToken);
    }

    public async Task DeleteOriginalResponseAsync(
        WebhookIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var path = $"webhooks/{identity.Id}/{identity.Token}/messages/@original";
        await client.SendAsync(path, HttpMethod.Delete, cancellationToken);
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
