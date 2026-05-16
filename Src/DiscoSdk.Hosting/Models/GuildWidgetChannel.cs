using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IGuildWidgetChannel"/>
internal class GuildWidgetChannel : IGuildWidgetChannel
{
	[JsonPropertyName("id")]
	public Snowflake Id { get; init; } = default!;

	[JsonPropertyName("name")]
	public string Name { get; init; } = default!;

	[JsonPropertyName("position")]
	public int Position { get; init; }
}
