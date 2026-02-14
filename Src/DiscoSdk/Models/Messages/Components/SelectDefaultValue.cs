using DiscoSdk.Models;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages.Components;

/// <summary>
/// Default value for user/role/channel/mentionable select components.
/// </summary>
public class SelectDefaultValue
{
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; }

	[JsonPropertyName("type")]
	public string Type { get; set; } = default!; // "user" | "role" | "channel"
}
