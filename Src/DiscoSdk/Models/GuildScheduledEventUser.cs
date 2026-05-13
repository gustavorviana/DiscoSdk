using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Raw model returned by <c>GET /guilds/{guild.id}/scheduled-events/{event.id}/users</c>.
/// Internal — consumers use <see cref="IGuildScheduledEventUser"/>.
/// </summary>
internal class GuildScheduledEventUser
{
	/// <summary>The scheduled event this subscription is for.</summary>
	[JsonPropertyName("guild_scheduled_event_id")]
	public Snowflake GuildScheduledEventId { get; set; } = default!;

	/// <summary>The user that subscribed.</summary>
	[JsonPropertyName("user")]
	public User User { get; set; } = default!;

	/// <summary>The user's guild member object (only when <c>with_member=true</c>).</summary>
	[JsonPropertyName("member")]
	public GuildMember? Member { get; set; }
}
