using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Rest.Actions.Messages.Webhooks;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Hosting.Wrappers.Messages;
using DiscoSdk.Models;
using DiscoSdk.Models.Messages;
using DiscoSdk.Rest;
using DiscoSdk.Rest.Actions;
using DiscoSdk.Rest.Actions.Messages.Webhooks;

namespace DiscoSdk.Hosting;

internal sealed class DiscordWebhookClient(WebhookMessageClient client, IWebhookInfo info, WebhookIdentity identity) : IDiscordWebhookClient
{
    public IDiscordRestClient HttpClient => client.Client;

    public string Token => identity.Token;

    public Snowflake Id => info.Id;

    public string Name => info.Name;

    public string? Avatar => info.Avatar;

    public Snowflake? ChannelId => info.ChannelId;

    public string? ApplicationId => info.ApplicationId;

    public Snowflake? GuildId => info.GuildId;

    public IRestAction DeleteMessage(Snowflake messageId, Snowflake? threadId = null)
    {
        return RestAction.Create(async cancellationToken =>
        {
            await client.DeleteAsync(identity, messageId, threadId, cancellationToken);
        });
    }

    public IWebhookEditMessageRestAction EditMessage(Snowflake messageId)
    {
        return new WebhookEditMessageRestAction(identity, client, messageId);
    }

    public IRestAction<IWebhookMessage?> GetMessageById(Snowflake messageId, Snowflake? threadId = null)
    {
        return RestAction<IWebhookMessage?>.Create(async cancellationToken =>
        {
            var message = await client.GetAsync(identity, messageId, threadId, cancellationToken);
            if (message is null)
                return null;

            return new WebhookMessageWrapper(identity, client, message);
        });
    }

    public IWebhookSendMessageRestAction Send(Snowflake? threadId = null)
    {
        return new WebhookCreateMessageRestAction(identity, client);
    }
}