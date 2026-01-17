using System.Text.Json.Serialization;

namespace DiscoSdk.Hosting.Gateway.Payloads;

internal sealed class ActivityPayload
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	[JsonPropertyName("type")]
	public int Type { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("created_at")]
	public long? CreatedAt { get; set; }

	[JsonPropertyName("timestamps")]
	public ActivityTimestampsPayload? Timestamps { get; set; }

	[JsonPropertyName("application_id")]
	public string? ApplicationId { get; set; }

	[JsonPropertyName("details")]
	public string? Details { get; set; }

	[JsonPropertyName("state")]
	public string? State { get; set; }

	[JsonPropertyName("emoji")]
	public ActivityEmojiPayload? Emoji { get; set; }

	[JsonPropertyName("party")]
	public ActivityPartyPayload? Party { get; set; }

	[JsonPropertyName("assets")]
	public ActivityAssetsPayload? Assets { get; set; }

	[JsonPropertyName("secrets")]
	public ActivitySecretsPayload? Secrets { get; set; }

	[JsonPropertyName("instance")]
	public bool? Instance { get; set; }

	[JsonPropertyName("flags")]
	public int? Flags { get; set; }

	[JsonPropertyName("buttons")]
	public ActivityButtonPayload[]? Buttons { get; set; }
}

internal sealed class ActivityTimestampsPayload
{
	[JsonPropertyName("start")]
	public long? Start { get; set; }

	[JsonPropertyName("end")]
	public long? End { get; set; }
}

internal sealed class ActivityEmojiPayload
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("animated")]
	public bool? Animated { get; set; }
}

internal sealed class ActivityPartyPayload
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("size")]
	public int[]? Size { get; set; }
}

internal sealed class ActivityAssetsPayload
{
	[JsonPropertyName("large_image")]
	public string? LargeImage { get; set; }

	[JsonPropertyName("large_text")]
	public string? LargeText { get; set; }

	[JsonPropertyName("small_image")]
	public string? SmallImage { get; set; }

	[JsonPropertyName("small_text")]
	public string? SmallText { get; set; }
}

internal sealed class ActivitySecretsPayload
{
	[JsonPropertyName("join")]
	public string? Join { get; set; }

	[JsonPropertyName("spectate")]
	public string? Spectate { get; set; }

	[JsonPropertyName("match")]
	public string? Match { get; set; }
}

internal sealed class ActivityButtonPayload
{
	[JsonPropertyName("label")]
	public string Label { get; set; } = default!;

	[JsonPropertyName("url")]
	public string Url { get; set; } = default!;
}

