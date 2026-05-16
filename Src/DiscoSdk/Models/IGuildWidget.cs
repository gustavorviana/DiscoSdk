namespace DiscoSdk.Models;

/// <summary>
/// Read-only view of a guild widget (the embed snapshot used by the <c>/widget.json</c> endpoint).
/// </summary>
public interface IGuildWidget
{
	/// <summary>The guild's id.</summary>
	Snowflake Id { get; }

	/// <summary>The guild's name.</summary>
	string Name { get; }

	/// <summary>The instant invite URL for the configured widget invite channel, or <c>null</c>.</summary>
	string? InstantInvite { get; }

	/// <summary>Voice and stage channels accessible by @everyone, as exposed by the widget.</summary>
	IReadOnlyList<IGuildWidgetChannel>? Channels { get; }

	/// <summary>Online members exposed by the widget (capped by Discord — not a full list).</summary>
	IReadOnlyList<IGuildWidgetMember>? Members { get; }

	/// <summary>Number of online members in the guild at the snapshot time.</summary>
	int PresenceCount { get; }
}
