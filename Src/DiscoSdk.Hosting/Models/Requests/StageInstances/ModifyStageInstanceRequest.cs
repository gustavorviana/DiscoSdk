using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Models.Requests.StageInstances;

/// <summary>
/// Request body for <c>PATCH /stage-instances/{channel.id}</c>.
/// Reference: https://discord.com/developers/docs/resources/stage-instance#modify-stage-instance
/// </summary>
internal class ModifyStageInstanceRequest
{
	/// <summary>New topic (1-120 chars). Null to leave unchanged.</summary>
	[JsonPropertyName("topic")]
	public string? Topic { get; set; }

	/// <summary>New privacy level. Null to leave unchanged.</summary>
	[JsonPropertyName("privacy_level")]
	public StagePrivacyLevel? PrivacyLevel { get; set; }
}
