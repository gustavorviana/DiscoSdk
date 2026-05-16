using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models;

/// <summary>
/// A member entry inside an <see cref="IGuildWidget"/> — anonymised by Discord and bounded by a
/// hard cap, so this does <strong>not</strong> represent the full member list.
/// </summary>
public interface IGuildWidgetMember
{
	/// <summary>The user id, when Discord chooses to disclose it.</summary>
	Snowflake? Id { get; }

	/// <summary>The user's username.</summary>
	string Username { get; }

	/// <summary>The user's discriminator (legacy tag).</summary>
	string Discriminator { get; }

	/// <summary>The user's avatar hash, or <c>null</c>.</summary>
	string? Avatar { get; }

	/// <summary>Reported online status.</summary>
	OnlineStatus Status { get; }

	/// <summary>Direct URL to the rendered avatar image, when provided by the widget endpoint.</summary>
	string? AvatarUrl { get; }
}
