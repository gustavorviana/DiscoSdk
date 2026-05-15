using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord webhook attached to a guild channel, plus the operations available on it.
/// </summary>
public interface IWebhook : IWithSnowflake
{
	/// <summary>The webhook type.</summary>
	WebhookType Type { get; }

	/// <summary>The guild ID this webhook lives in, if any.</summary>
	Snowflake? GuildId { get; }

	/// <summary>The channel ID this webhook is bound to, if any.</summary>
	Snowflake? ChannelId { get; }

	/// <summary>The user that created the webhook, if available.</summary>
	IUser? User { get; }

	/// <summary>The default display name of the webhook.</summary>
	string? Name { get; }

	/// <summary>The avatar hash of the webhook.</summary>
	string? Avatar { get; }

	/// <summary>The secure token (only present for Incoming webhooks the bot can see in full).</summary>
	string? Token { get; }

	/// <summary>The ID of the app this webhook belongs to, if it is an Application webhook.</summary>
	Snowflake? ApplicationId { get; }

	/// <summary>The URL used for executing the webhook (only returned by the OAuth2 webhook endpoint).</summary>
	string? Url { get; }

	/// <summary>
	/// Builds a REST action that modifies this webhook. Configure name / avatar / target channel
	/// on the returned builder before calling <see cref="IRestAction{T}.ExecuteAsync"/>.
	/// </summary>
	IModifyWebhookAction Modify();

	/// <summary>
	/// Gets a REST action that deletes this webhook.
	/// </summary>
	IRestAction Delete();
}
