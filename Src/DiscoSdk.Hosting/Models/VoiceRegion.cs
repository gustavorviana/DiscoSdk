using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IVoiceRegion"/>
internal class VoiceRegion : IVoiceRegion
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = default!;

	[JsonPropertyName("name")]
	public string Name { get; init; } = default!;

	[JsonPropertyName("optimal")]
	public bool Optimal { get; init; }

	[JsonPropertyName("deprecated")]
	public bool Deprecated { get; init; }

	[JsonPropertyName("custom")]
	public bool Custom { get; init; }
}
