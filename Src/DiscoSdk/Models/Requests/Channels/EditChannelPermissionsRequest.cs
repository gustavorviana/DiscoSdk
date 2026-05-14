using DiscoSdk.Models.Enums;
using DiscoSdk.Models.JsonConverters;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests.Channels;

/// <summary>
/// Request body for <c>PUT /channels/{channel.id}/permissions/{overwrite.id}</c>.
/// Reference: https://discord.com/developers/docs/resources/channel#edit-channel-permissions
/// </summary>
internal class EditChannelPermissionsRequest
{
	/// <summary>Permission bit set to allow (defaults to 0 — nothing additionally allowed).</summary>
	[JsonPropertyName("allow")]
	[JsonConverter(typeof(DiscordPermissionConverter))]
	public DiscordPermission Allow { get; set; }

	/// <summary>Permission bit set to deny (defaults to 0 — nothing additionally denied).</summary>
	[JsonPropertyName("deny")]
	[JsonConverter(typeof(DiscordPermissionConverter))]
	public DiscordPermission Deny { get; set; }

	/// <summary>0 for role, 1 for member — disambiguates the overwrite kind.</summary>
	[JsonPropertyName("type")]
	public PermissionOverwriteType Type { get; set; }
}
