using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord guild integration (subscriber-sync from Twitch / YouTube / Discord bots, etc).
/// </summary>
public interface IIntegration : IWithSnowflake
{
	/// <summary>The display name of the integration.</summary>
	string Name { get; }

	/// <summary>The integration type — e.g. <c>twitch</c>, <c>youtube</c>, <c>discord</c>.</summary>
	string Type { get; }

	/// <summary>Whether the integration is currently enabled.</summary>
	bool Enabled { get; }

	/// <summary>Whether the integration is currently syncing.</summary>
	bool? Syncing { get; }

	/// <summary>The role that an integration subscriber is granted.</summary>
	Snowflake? RoleId { get; }

	/// <summary>Whether emoticons should be synced for this integration.</summary>
	bool? EnableEmoticons { get; }

	/// <summary>Behavior applied when an integration subscription expires.</summary>
	IntegrationExpireBehavior? ExpireBehavior { get; }

	/// <summary>The grace period (in days) before the expire behavior kicks in.</summary>
	int? ExpireGracePeriod { get; }

	/// <summary>The Discord user that owns this integration, if applicable.</summary>
	IUser? User { get; }

	/// <summary>The external account tied to this integration.</summary>
	IIntegrationAccount Account { get; }

	/// <summary>When the integration was last synced.</summary>
	DateTimeOffset? SyncedAt { get; }

	/// <summary>How many subscribers the integration has.</summary>
	int? SubscriberCount { get; }

	/// <summary>Whether the integration has been revoked.</summary>
	bool? Revoked { get; }

	/// <summary>The scopes the integration was authorized for.</summary>
	IReadOnlyList<string>? Scopes { get; }

	/// <summary>
	/// Gets a REST action that deletes this guild integration (revokes it and removes the linked role).
	/// </summary>
	IRestAction Delete();
}
