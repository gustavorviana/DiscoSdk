namespace DiscoSdk.Models;

/// <summary>
/// Extension methods for <see cref="Emoji"/>.
/// </summary>
public static class EmojiExtensions
{
	/// <summary>
	/// Gets the reaction code for this emoji.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	/// <returns>The reaction code (name:id for custom emojis, or name for unicode emojis).</returns>
	public static string GetAsReactionCode(this Emoji emoji)
	{
		if (emoji.Id.HasValue)
			return $"{emoji.Name}:{emoji.Id.Value}";

		return emoji.Name ?? string.Empty;
	}
}

