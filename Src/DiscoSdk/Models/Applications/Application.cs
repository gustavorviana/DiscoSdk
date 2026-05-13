using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Users;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// Represents a Discord application (the entity a bot belongs to). A partial form arrives in the
/// gateway <c>READY</c> payload; the full object is returned by the Get Current Application endpoint.
/// </summary>
public class Application
{
	/// <summary>The ID of the application.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>The name of the application.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The icon hash of the application.</summary>
	[JsonPropertyName("icon")]
	public string? Icon { get; set; }

	/// <summary>The description of the application.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = default!;

	/// <summary>A list of RPC origin URLs, if RPC is enabled.</summary>
	[JsonPropertyName("rpc_origins")]
	public string[]? RpcOrigins { get; set; }

	/// <summary>Whether anyone can add the app's bot to guilds, or only the app owner.</summary>
	[JsonPropertyName("bot_public")]
	public bool BotPublic { get; set; }

	/// <summary>Whether the app's bot requires completion of the full OAuth2 code-grant flow to join a guild.</summary>
	[JsonPropertyName("bot_require_code_grant")]
	public bool BotRequireCodeGrant { get; set; }

	/// <summary>A partial user object for the bot user associated with the app.</summary>
	[JsonPropertyName("bot")]
	public User? Bot { get; set; }

	/// <summary>The URL of the app's Terms of Service.</summary>
	[JsonPropertyName("terms_of_service_url")]
	public string? TermsOfServiceUrl { get; set; }

	/// <summary>The URL of the app's Privacy Policy.</summary>
	[JsonPropertyName("privacy_policy_url")]
	public string? PrivacyPolicyUrl { get; set; }

	/// <summary>A partial user object for the owner of the app.</summary>
	[JsonPropertyName("owner")]
	public User? Owner { get; set; }

	/// <summary>The hex-encoded key for verification in interactions and the GameSDK's GetTicket.</summary>
	[JsonPropertyName("verify_key")]
	public string VerifyKey { get; set; } = default!;

	/// <summary>If the app belongs to a team, the team that owns it.</summary>
	[JsonPropertyName("team")]
	public Team? Team { get; set; }

	/// <summary>The guild associated with the app (e.g. its support server).</summary>
	[JsonPropertyName("guild_id")]
	public Snowflake? GuildId { get; set; }

	/// <summary>A partial object of the associated guild.</summary>
	[JsonPropertyName("guild")]
	public Guild? Guild { get; set; }

	/// <summary>If the app is a game sold on Discord, the ID of the "Game SKU" that is created, if any.</summary>
	[JsonPropertyName("primary_sku_id")]
	public Snowflake? PrimarySkuId { get; set; }

	/// <summary>If the app is a game sold on Discord, the URL slug that links to the store page.</summary>
	[JsonPropertyName("slug")]
	public string? Slug { get; set; }

	/// <summary>The default rich-presence invite cover image hash.</summary>
	[JsonPropertyName("cover_image")]
	public string? CoverImage { get; set; }

	/// <summary>The public flags of the app.</summary>
	[JsonPropertyName("flags")]
	public ApplicationFlags? Flags { get; set; }

	/// <summary>An approximate count of guilds the app has been added to.</summary>
	[JsonPropertyName("approximate_guild_count")]
	public int? ApproximateGuildCount { get; set; }

	/// <summary>An approximate count of users that have installed the app (authorized with the <c>application.commands</c> scope).</summary>
	[JsonPropertyName("approximate_user_install_count")]
	public int? ApproximateUserInstallCount { get; set; }

	/// <summary>An array of redirect URIs for the app.</summary>
	[JsonPropertyName("redirect_uris")]
	public string[]? RedirectUris { get; set; }

	/// <summary>The interactions endpoint URL for the app.</summary>
	[JsonPropertyName("interactions_endpoint_url")]
	public string? InteractionsEndpointUrl { get; set; }

	/// <summary>The role connection verification URL for the app.</summary>
	[JsonPropertyName("role_connections_verification_url")]
	public string? RoleConnectionsVerificationUrl { get; set; }

	/// <summary>The event webhooks URL for the app to receive webhook events.</summary>
	[JsonPropertyName("event_webhooks_url")]
	public string? EventWebhooksUrl { get; set; }

	/// <summary>Whether event webhooks are enabled, disabled, or disabled by Discord (1 = disabled, 2 = enabled, 3 = disabled by Discord).</summary>
	[JsonPropertyName("event_webhooks_status")]
	public int? EventWebhooksStatus { get; set; }

	/// <summary>The list of webhook event types the app subscribes to.</summary>
	[JsonPropertyName("event_webhooks_types")]
	public string[]? EventWebhooksTypes { get; set; }

	/// <summary>Up to 5 tags describing the content and functionality of the app.</summary>
	[JsonPropertyName("tags")]
	public string[]? Tags { get; set; }

	/// <summary>Settings for the app's default in-app authorization link, if enabled.</summary>
	[JsonPropertyName("install_params")]
	public ApplicationInstallParams? InstallParams { get; set; }

	/// <summary>The default scopes and permissions for each supported installation context, keyed by integration type ("0" = guild, "1" = user).</summary>
	[JsonPropertyName("integration_types_config")]
	public Dictionary<string, ApplicationIntegrationTypeConfiguration>? IntegrationTypesConfig { get; set; }

	/// <summary>The default custom authorization URL for the app, if enabled.</summary>
	[JsonPropertyName("custom_install_url")]
	public string? CustomInstallUrl { get; set; }
}
