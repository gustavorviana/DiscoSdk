using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents the full Discord webhook object returned by the channel/guild webhook endpoints.
/// </summary>
public class Webhook
{
	/// <summary>The ID of the webhook.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; }

	/// <summary>The type of the webhook.</summary>
	[JsonPropertyName("type")]
	public WebhookType Type { get; set; }

	/// <summary>The guild ID this webhook lives in, if any (null for application webhooks).</summary>
	[JsonPropertyName("guild_id")]
	public Snowflake? GuildId { get; set; }

	/// <summary>The channel ID this webhook is bound to, if any.</summary>
	[JsonPropertyName("channel_id")]
	public Snowflake? ChannelId { get; set; }

	/// <summary>The user that created the webhook (not present when fetched via webhook token).</summary>
	[JsonPropertyName("user")]
	public User? User { get; set; }

	/// <summary>The default display name of the webhook.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The default avatar hash of the webhook.</summary>
	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	/// <summary>The secure token of the webhook (only present for Incoming webhooks).</summary>
	[JsonPropertyName("token")]
	public string? Token { get; set; }

	/// <summary>The ID of the app this webhook belongs to, if it is an Application webhook.</summary>
	[JsonPropertyName("application_id")]
	public Snowflake? ApplicationId { get; set; }

	/// <summary>The URL used for executing the webhook (only returned by the OAuth2 webhook endpoint).</summary>
	[JsonPropertyName("url")]
	public string? Url { get; set; }
}
