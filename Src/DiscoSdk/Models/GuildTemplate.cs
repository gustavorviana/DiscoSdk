using DiscoSdk.Models.Users;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a guild template — a snapshot of a guild's settings that can be used to create new guilds.
/// </summary>
public class GuildTemplate
{
	/// <summary>The template code (unique ID).</summary>
	[JsonPropertyName("code")]
	public string Code { get; set; } = default!;

	/// <summary>The name of the template.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = default!;

	/// <summary>The description of the template.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>Number of times this template has been used.</summary>
	[JsonPropertyName("usage_count")]
	public int UsageCount { get; set; }

	/// <summary>The ID of the user who created the template.</summary>
	[JsonPropertyName("creator_id")]
	public Snowflake CreatorId { get; set; } = default!;

	/// <summary>The user who created the template.</summary>
	[JsonPropertyName("creator")]
	public User Creator { get; set; } = default!;

	/// <summary>When this template was created (ISO 8601).</summary>
	[JsonPropertyName("created_at")]
	public string CreatedAt { get; set; } = default!;

	/// <summary>When this template was last synced to the source guild (ISO 8601).</summary>
	[JsonPropertyName("updated_at")]
	public string UpdatedAt { get; set; } = default!;

	/// <summary>The ID of the guild this template is based on.</summary>
	[JsonPropertyName("source_guild_id")]
	public Snowflake SourceGuildId { get; set; } = default!;

	/// <summary>A partial guild snapshot of what this template produces.</summary>
	[JsonPropertyName("serialized_source_guild")]
	public Guild SerializedSourceGuild { get; set; } = default!;

	/// <summary>Whether the template has unsynced changes.</summary>
	[JsonPropertyName("is_dirty")]
	public bool? IsDirty { get; set; }
}
