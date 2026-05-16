using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IGuildWidget"/>
internal class GuildWidget : IGuildWidget
{
	[JsonPropertyName("id")]
	public Snowflake Id { get; init; } = default!;

	[JsonPropertyName("name")]
	public string Name { get; init; } = default!;

	[JsonPropertyName("instant_invite")]
	public string? InstantInvite { get; init; }

	[JsonPropertyName("channels")]
	public GuildWidgetChannel[]? Channels { get; init; }

	[JsonPropertyName("members")]
	public GuildWidgetMember[]? Members { get; init; }

	[JsonPropertyName("presence_count")]
	public int PresenceCount { get; init; }

	IReadOnlyList<IGuildWidgetChannel>? IGuildWidget.Channels => Channels;
	IReadOnlyList<IGuildWidgetMember>? IGuildWidget.Members => Members;
}
