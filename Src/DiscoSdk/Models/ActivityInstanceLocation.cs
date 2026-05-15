using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Location where an <see cref="ActivityInstance"/> is running. Returned as part of the
/// <c>activity_instance.location</c> object on
/// <c>GET /applications/{application_id}/activity-instances/{instance_id}</c>.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/application#activity-location-object"/>.
/// </remarks>
public sealed class ActivityInstanceLocation
{
	/// <summary>Unique identifier for the location.</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;

	/// <summary>
	/// Kind of channel: <see cref="ActivityLocationKind.GuildChannel"/> (Discord wire <c>"gc"</c>)
	/// or <see cref="ActivityLocationKind.PrivateChannel"/> (Discord wire <c>"pc"</c>).
	/// </summary>
	[JsonPropertyName("kind")]
	public ActivityLocationKind Kind { get; set; }

	/// <summary>Channel the activity is running in.</summary>
	[JsonPropertyName("channel_id")]
	public Snowflake ChannelId { get; set; }

	/// <summary>
	/// Guild the activity is running in. <c>null</c> for private-channel locations
	/// (<see cref="ActivityLocationKind.PrivateChannel"/>).
	/// </summary>
	[JsonPropertyName("guild_id")]
	public Snowflake? GuildId { get; set; }
}
