using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway.Payloads;

internal sealed class PresenceUpdatePayload
{
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	[JsonPropertyName("activities")]
	public ActivityPayload[]? Activities { get; set; }

	[JsonPropertyName("afk")]
	public bool? Afk { get; set; }

	[JsonPropertyName("since")]
	public long? Since { get; set; }
}

