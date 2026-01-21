using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class InteractionHandle(Snowflake id, string token) : WebhookIdentity(id, token)
{
    public bool IsDeferred { get; set; }
    public bool Responded { get; set; }

    public Snowflake GetDeferredId(Snowflake? appId)
    {
        return IsDeferred ? appId!.Value : Id;
    }

    public WebhookIdentity WithAppId(Snowflake? appId)
    {
        return new WebhookIdentity(appId!.Value, Token);
    }
}