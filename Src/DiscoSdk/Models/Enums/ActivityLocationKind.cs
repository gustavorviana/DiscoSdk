using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Enums;

/// <summary>
/// Kind of channel an <see cref="ActivityInstance"/> is running in. Maps to the
/// <c>location.kind</c> field of the activity-instance payload.
/// </summary>
/// <remarks>
/// Source: <see href="https://discord.com/developers/docs/resources/application#activity-location-kind-enum"/>.
/// </remarks>
[JsonConverter(typeof(ActivityLocationKindConverter))]
public enum ActivityLocationKind
{
	/// <summary>Guild channel (Discord wire value: <c>"gc"</c>).</summary>
	GuildChannel,

	/// <summary>Private channel — DM or group DM (Discord wire value: <c>"pc"</c>).</summary>
	PrivateChannel,
}
