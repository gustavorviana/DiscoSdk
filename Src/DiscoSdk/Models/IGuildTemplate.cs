using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Models;

/// <summary>
/// A Discord guild template (a snapshot of a guild's settings), with the operations that can be
/// performed on it.
/// </summary>
public interface IGuildTemplate
{
	/// <summary>The template code (unique ID).</summary>
	string Code { get; }

	/// <summary>The name of the template.</summary>
	string Name { get; }

	/// <summary>The description of the template.</summary>
	string? Description { get; }

	/// <summary>Number of times this template has been used.</summary>
	int UsageCount { get; }

	/// <summary>The ID of the user who created the template.</summary>
	Snowflake CreatorId { get; }

	/// <summary>The user who created the template.</summary>
	IUser Creator { get; }

	/// <summary>When this template was created (ISO 8601).</summary>
	string CreatedAt { get; }

	/// <summary>When this template was last synced to the source guild (ISO 8601).</summary>
	string UpdatedAt { get; }

	/// <summary>The ID of the guild this template is based on.</summary>
	Snowflake SourceGuildId { get; }

	/// <summary>A partial guild snapshot of what this template produces.</summary>
	IGuild SerializedSourceGuild { get; }

	/// <summary>Whether the template has unsynced changes.</summary>
	bool? IsDirty { get; }

	/// <summary>Creates a REST action that creates a new guild from this template.</summary>
	/// <param name="name">The name of the new guild.</param>
	/// <param name="icon">Optional base64 128x128 image data URI for the guild icon.</param>
	IRestAction<IGuild> CreateGuild(string name, string? icon = null);

	/// <summary>Creates a REST action that syncs this template to its source guild's current state.</summary>
	IRestAction<IGuildTemplate> Sync();

	/// <summary>Creates a REST action that updates this template's name and/or description.</summary>
	IRestAction<IGuildTemplate> Modify(string? name = null, string? description = null);

	/// <summary>Creates a REST action that deletes this template.</summary>
	IRestAction<IGuildTemplate> Delete();
}
