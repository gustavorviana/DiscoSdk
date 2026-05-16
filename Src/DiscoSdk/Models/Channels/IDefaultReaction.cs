namespace DiscoSdk.Models.Channels;

/// <summary>
/// Read-only view of a forum/media channel's default reaction emoji (the one applied by Discord
/// when no other reaction is set on a thread).
/// </summary>
public interface IDefaultReaction
{
	/// <summary>Custom emoji id, when the default reaction is a guild custom emoji.</summary>
	Snowflake? EmojiId { get; }

	/// <summary>Unicode character of the emoji, when the default reaction is a Unicode emoji.</summary>
	string? EmojiName { get; }
}
