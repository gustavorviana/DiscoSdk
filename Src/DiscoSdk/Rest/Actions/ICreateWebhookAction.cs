using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Creates a new webhook on a guild channel. The seed parameter is the channel; the default
/// display name is set via <see cref="SetName"/> (mandatory — Discord requires 1–80 chars).
/// </summary>
public interface ICreateWebhookAction : IRestAction<IWebhook>
{
    /// <summary>Sets the default webhook name (1–80 chars). Required by Discord.</summary>
    ICreateWebhookAction SetName(string name);

    /// <summary>Sets the default avatar as a base64-encoded data URI.</summary>
    ICreateWebhookAction SetAvatar(string avatarDataUri);
}
