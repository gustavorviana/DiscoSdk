using DiscoSdk.Models.Enums;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <inheritdoc cref="IGuildWidgetMember"/>
internal class GuildWidgetMember : IGuildWidgetMember
{
	[JsonPropertyName("id")]
	public Snowflake? Id { get; init; }

	[JsonPropertyName("username")]
	public string Username { get; init; } = default!;

	[JsonPropertyName("discriminator")]
	public string Discriminator { get; init; } = default!;

	[JsonPropertyName("avatar")]
	public string? Avatar { get; init; }

	[JsonPropertyName("status")]
	public OnlineStatus Status { get; init; }

	[JsonPropertyName("avatar_url")]
	public string? AvatarUrl { get; init; }
}
