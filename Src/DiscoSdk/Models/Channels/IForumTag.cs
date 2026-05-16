namespace DiscoSdk.Models.Channels;

/// <summary>
/// Read-only view of a forum/media-channel tag that posts can be classified with.
/// </summary>
public interface IForumTag
{
	/// <summary>Tag id, when it exists (newly-constructed tags for create-channel have <c>null</c>).</summary>
	Snowflake? Id { get; }

	/// <summary>Tag display name (max 20 chars).</summary>
	string Name { get; }

	/// <summary>Whether applying/removing this tag requires the <c>MANAGE_THREADS</c> permission.</summary>
	bool Moderated { get; }

	/// <summary>Custom emoji id when the tag is decorated with a guild custom emoji.</summary>
	Snowflake? EmojiId { get; }

	/// <summary>Unicode character of the emoji when the tag is decorated with a Unicode emoji.</summary>
	string? EmojiName { get; }
}
