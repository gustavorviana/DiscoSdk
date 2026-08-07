using DiscoSdk.Models;

namespace DiscoSdk.Hosting.Contexts.Models;

internal class InteractionHandle(Snowflake id, string token) : WebhookIdentity(id, token)
{
    public bool IsDeferred { get; set; }
    public bool Responded { get; set; }

    /// <summary>
    /// Set by a handler / command dispatched with <c>[FireAndForget(SkipNextExecutions = true)]</c>.
    /// The dispatcher stops iterating the current chain after the handler returns. Reset to
    /// <c>false</c> at the start of each <c>HandleAllAsync</c> chain so the skip is scoped to one
    /// chain only.
    /// </summary>
    public bool SkipNextExecutions { get; set; }

    public Snowflake GetDeferredId(Snowflake? appId)
    {
        return IsDeferred ? appId!.Value : Id;
    }

    public WebhookIdentity WithAppId(Snowflake? appId)
    {
        return new WebhookIdentity(appId!.Value, Token);
    }
}