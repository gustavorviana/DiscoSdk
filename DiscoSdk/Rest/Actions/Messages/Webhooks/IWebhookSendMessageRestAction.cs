using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions.Messages.Webhooks;

public interface IWebhookSendMessageRestAction : ICreateMessageBuilderBaseAction<IWebhookSendMessageRestAction, IWebhookMessage?>,
    IWebhookWithThread<IWebhookSendMessageRestAction>
{
    IWebhookSendMessageRestAction Wait(bool wait = true);

    IWebhookSendMessageRestAction SetUsername(string? username);

    IWebhookSendMessageRestAction SetAvatarUrl(string? avatarUrl);

    IWebhookSendMessageRestAction SetThreadName(string? threadName);

    IWebhookSendMessageRestAction SetAppliedTags(params ulong[] tagIds);
}