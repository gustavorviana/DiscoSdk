using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using System.Collections.Generic;

namespace DiscoSdk.Models.Applications;

/// <summary>
/// A Discord application (the entity a bot belongs to), with the operations that can be performed on it.
/// </summary>
public interface IApplication
{
	/// <summary>The ID of the application.</summary>
	Snowflake Id { get; }

	/// <summary>The name of the application.</summary>
	string Name { get; }

	/// <summary>The icon hash of the application.</summary>
	string? Icon { get; }

	/// <summary>The description of the application.</summary>
	string Description { get; }

	/// <summary>A list of RPC origin URLs, if RPC is enabled.</summary>
	IReadOnlyList<string>? RpcOrigins { get; }

	/// <summary>Whether anyone can add the app's bot to guilds, or only the app owner.</summary>
	bool BotPublic { get; }

	/// <summary>Whether the app's bot requires completion of the full OAuth2 code-grant flow to join a guild.</summary>
	bool BotRequireCodeGrant { get; }

	/// <summary>The bot user associated with the app.</summary>
	IUser? Bot { get; }

	/// <summary>The URL of the app's Terms of Service.</summary>
	string? TermsOfServiceUrl { get; }

	/// <summary>The URL of the app's Privacy Policy.</summary>
	string? PrivacyPolicyUrl { get; }

	/// <summary>The owner of the app.</summary>
	IUser? Owner { get; }

	/// <summary>The hex-encoded key for verification in interactions and the GameSDK's GetTicket.</summary>
	string VerifyKey { get; }

	/// <summary>If the app belongs to a team, the team that owns it.</summary>
	ITeam? Team { get; }

	/// <summary>The ID of the guild associated with the app, if any.</summary>
	Snowflake? GuildId { get; }

	/// <summary>If the app is a game sold on Discord, the ID of the "Game SKU" that is created, if any.</summary>
	Snowflake? PrimarySkuId { get; }

	/// <summary>If the app is a game sold on Discord, the URL slug that links to the store page.</summary>
	string? Slug { get; }

	/// <summary>The default rich-presence invite cover image hash.</summary>
	string? CoverImage { get; }

	/// <summary>The public flags of the app.</summary>
	ApplicationFlags? Flags { get; }

	/// <summary>An approximate count of guilds the app has been added to.</summary>
	int? ApproximateGuildCount { get; }

	/// <summary>An approximate count of users that have installed the app.</summary>
	int? ApproximateUserInstallCount { get; }

	/// <summary>An array of redirect URIs for the app.</summary>
	IReadOnlyList<string>? RedirectUris { get; }

	/// <summary>The interactions endpoint URL for the app.</summary>
	string? InteractionsEndpointUrl { get; }

	/// <summary>The role connection verification URL for the app.</summary>
	string? RoleConnectionsVerificationUrl { get; }

	/// <summary>The event webhooks URL for the app to receive webhook events.</summary>
	string? EventWebhooksUrl { get; }

	/// <summary>Up to 5 tags describing the content and functionality of the app.</summary>
	IReadOnlyList<string>? Tags { get; }

	/// <summary>Settings for the app's default in-app authorization link, if enabled.</summary>
	IApplicationInstallParams? InstallParams { get; }

	/// <summary>The default scopes and permissions for each supported installation context.</summary>
	IReadOnlyDictionary<string, IApplicationIntegrationTypeConfiguration>? IntegrationTypesConfig { get; }

	/// <summary>The default custom authorization URL for the app, if enabled.</summary>
	string? CustomInstallUrl { get; }

	/// <summary>Creates a REST action that edits this application's properties.</summary>
	IEditApplicationAction Edit();
}
