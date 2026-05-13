using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// A selectable option within a guild onboarding <see cref="OnboardingPrompt"/>; choosing it opts the
/// member into the listed channels and roles.
/// </summary>
public class OnboardingPromptOption
{
	/// <summary>The ID of the option.</summary>
	[JsonPropertyName("id")]
	public Snowflake Id { get; set; } = default!;

	/// <summary>IDs of channels a member is opted into when selecting this option.</summary>
	[JsonPropertyName("channel_ids")]
	public Snowflake[] ChannelIds { get; set; } = [];

	/// <summary>IDs of roles assigned to a member when selecting this option.</summary>
	[JsonPropertyName("role_ids")]
	public Snowflake[] RoleIds { get; set; } = [];

	/// <summary>The emoji of the option (when populated by Discord as an object).</summary>
	[JsonPropertyName("emoji")]
	public Emoji? Emoji { get; set; }

	/// <summary>The emoji ID of the option (used when setting the emoji via the update endpoint).</summary>
	[JsonPropertyName("emoji_id")]
	public Snowflake? EmojiId { get; set; }

	/// <summary>The emoji name of the option (used when setting the emoji via the update endpoint).</summary>
	[JsonPropertyName("emoji_name")]
	public string? EmojiName { get; set; }

	/// <summary>Whether the emoji is animated (used when setting the emoji via the update endpoint).</summary>
	[JsonPropertyName("emoji_animated")]
	public bool? EmojiAnimated { get; set; }

	/// <summary>The title of the option.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = default!;

	/// <summary>The description of the option.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}
