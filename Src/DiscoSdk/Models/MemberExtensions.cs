namespace DiscoSdk.Models;

/// <summary>
/// Extension methods for <see cref="IMember"/>.
/// </summary>
public static class MemberExtensions
{
	/// <summary>
	/// Gets the avatar URL of this member using the guild avatar URL template.
	/// </summary>
	/// <param name="member">The member to get the avatar URL for.</param>
	/// <returns>The avatar URL, or null if the member doesn't have a guild avatar.</returns>
	public static string? GetAvatarUrl(this IMember member)
	{
		var avatarId = member.AvatarId;
		if (avatarId == null)
			return null;

		var extension = avatarId.StartsWith("a_") ? "gif" : "png";
		// Replace %s placeholders with actual values in order
		var url = IMember.UrlAvatarUrl;
		url = url.Replace("%s", member.Guild.Id.ToString(), StringComparison.Ordinal);
		url = url.Replace("%s", member.Id.ToString(), StringComparison.Ordinal);
		url = url.Replace("%s", avatarId, StringComparison.Ordinal);
		url = url.Replace("%s", extension, StringComparison.Ordinal);
		return url;
	}
}

