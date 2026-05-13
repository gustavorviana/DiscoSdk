using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord guild integration record (Twitch sub, YouTube sub, Discord bot, etc).
/// </summary>
public class Integration
{
	/// <summary>The ID of the integration.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; }

	/// <summary>The name of the integration.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The integration type (e.g. <c>twitch</c>, <c>youtube</c>, <c>discord</c>).</summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = default!;

	/// <summary>Whether the integration is enabled.</summary>
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	/// <summary>Whether the integration is syncing (not present for Discord bot integrations).</summary>
	[JsonPropertyName("syncing")]
	public bool? Syncing { get; set; }

	/// <summary>The role that an integration subscriber gets, if linked.</summary>
	[JsonPropertyName("role_id")]
	public Snowflake? RoleId { get; set; }

	/// <summary>Whether emoticons should be synced for this integration.</summary>
	[JsonPropertyName("enable_emoticons")]
	public bool? EnableEmoticons { get; set; }

	/// <summary>Behavior applied when an integration subscription expires.</summary>
	[JsonPropertyName("expire_behavior")]
	public IntegrationExpireBehavior? ExpireBehavior { get; set; }

	/// <summary>The grace period (in days) before the expire behavior kicks in.</summary>
	[JsonPropertyName("expire_grace_period")]
	public int? ExpireGracePeriod { get; set; }

	/// <summary>The Discord user that owns this integration, if applicable.</summary>
	[JsonPropertyName("user")]
	public User? User { get; set; }

	/// <summary>The external account tied to this integration.</summary>
	[JsonPropertyName("account")]
	public IntegrationAccount Account { get; set; } = default!;

	/// <summary>When the integration was last synced.</summary>
	[JsonPropertyName("synced_at")]
	public DateTimeOffset? SyncedAt { get; set; }

	/// <summary>How many subscribers the integration has.</summary>
	[JsonPropertyName("subscriber_count")]
	public int? SubscriberCount { get; set; }

	/// <summary>Whether the integration has been revoked.</summary>
	[JsonPropertyName("revoked")]
	public bool? Revoked { get; set; }

	/// <summary>The bot/OAuth2 application backing this integration, if any.</summary>
	[JsonPropertyName("application")]
	public IntegrationApplication? Application { get; set; }

	/// <summary>The scopes the integration has been authorized for.</summary>
	[JsonPropertyName("scopes")]
	public string[]? Scopes { get; set; }
}
