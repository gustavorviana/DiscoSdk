namespace DiscoSdk.Models;

/// <summary>
/// One channel entry inside an <see cref="IWelcomeScreen"/>.
/// </summary>
public interface IWelcomeScreenChannel
{
	/// <summary>Channel id featured in the welcome screen.</summary>
	Snowflake ChannelId { get; }

	/// <summary>Description shown next to the channel.</summary>
	string Description { get; }

	/// <summary>Emoji id when the emoji is a custom guild emoji, otherwise <c>null</c>.</summary>
	string? EmojiId { get; }

	/// <summary>Emoji name (custom alias) or the Unicode character for built-in emojis, or <c>null</c>.</summary>
	string? EmojiName { get; }
}
