using DiscoSdk.Models.Messages;

namespace DiscoSdk.Rest.Actions.Messages.Webhooks;

public interface IWebhookEditMessageRestAction
    : IMessageBuilderAction<IWebhookEditMessageRestAction, IWebhookMessage>, 
    IWebhookWithThread<IWebhookEditMessageRestAction>;