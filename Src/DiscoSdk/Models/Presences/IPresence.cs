using DiscoSdk.Models.Activities;

namespace DiscoSdk.Models.Presences;

/// <summary>
/// Read-only view of a Discord presence — the activity / online-status of a user as broadcast
/// over the gateway (<c>PRESENCE_UPDATE</c> and the bundled snapshot inside <c>GUILD_CREATE</c>).
/// </summary>
/// <remarks>
/// Per Discord's docs the <c>user</c> object inside a presence is partial — only the
/// <c>id</c> is guaranteed — so this interface surfaces it as <see cref="UserId"/> rather than
/// a full user object. Resolve the full user via <see cref="IDiscordClient.GetUser"/> if needed.
/// </remarks>
public interface IPresence
{
	/// <summary>The user this presence applies to. Only the id is guaranteed by Discord.</summary>
	Snowflake UserId { get; }

	/// <summary>Overall presence status — <c>"online"</c>, <c>"idle"</c>, <c>"dnd"</c>, or <c>"offline"</c>.</summary>
	string? Status { get; }

	/// <summary>Gateway-side timestamp when this presence was processed (millis since epoch).</summary>
	long ProcessedAtTimestamp { get; }

	/// <summary>The user's primary activity (typically the game they're playing), or <c>null</c>.</summary>
	Activity? Game { get; }

	/// <summary>Per-platform status (desktop / mobile / web), or <c>null</c> if absent.</summary>
	IClientStatus? ClientStatus { get; }

	/// <summary>All current activities for the user.</summary>
	Activity[] Activities { get; }
}
