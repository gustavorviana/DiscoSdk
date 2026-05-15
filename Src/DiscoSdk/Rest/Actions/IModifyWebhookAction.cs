using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Modifies an existing webhook. Each setter on the builder maps to a single Discord field;
/// only the fields you set are sent.
/// </summary>
public interface IModifyWebhookAction : IRestAction<IWebhook>
{
    /// <summary>Sets the default name (1–80 chars).</summary>
    IModifyWebhookAction SetName(string name);

    /// <summary>Sets the default avatar as a base64-encoded data URI.</summary>
    IModifyWebhookAction SetAvatar(string avatarDataUri);

    /// <summary>Removes the webhook's avatar.</summary>
    IModifyWebhookAction ClearAvatar();

    /// <summary>Moves the webhook to a different channel.</summary>
    IModifyWebhookAction MoveToChannel(Snowflake channelId);
}
