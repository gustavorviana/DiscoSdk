using DiscoSdk.Models.Enums;

namespace DiscoSdk.Models.Activities;

/// <summary>
/// Represents an activity payload for updating the bot's presence (Gateway presence update).
/// Use this object to define the activity sent when calling <see cref="Rest.Actions.IUpdatePresenceAction.SetActivity(ActivityUpdate)"/>.
/// </summary>
public class ActivityUpdate
{
	/// <summary>
	/// Gets or sets the activity name (e.g. "Playing x", "Watching y").
	/// </summary>
	public string Name { get; set; } = default!;

	/// <summary>
	/// Gets or sets the activity type.
	/// </summary>
	public ActivityType Type { get; set; }

	/// <summary>
	/// Gets or sets the stream URL when type is <see cref="ActivityType.Streaming"/>.
	/// </summary>
	public string? Url { get; set; }

	/// <summary>
	/// Gets or sets the details line (rich presence).
	/// </summary>
	public string? Details { get; set; }

	/// <summary>
	/// Gets or sets the state line (rich presence).
	/// </summary>
	public string? State { get; set; }

	/// <summary>
	/// Gets or sets the timestamps (start/end) for the activity.
	/// </summary>
	public ActivityTimestamps? Timestamps { get; set; }

	/// <summary>
	/// Gets or sets the emoji for custom status.
	/// </summary>
	public ActivityEmoji? Emoji { get; set; }

	/// <summary>
	/// Gets or sets the party info (id, size).
	/// </summary>
	public ActivityParty? Party { get; set; }

	/// <summary>
	/// Gets or sets the assets (large/small image and text).
	/// </summary>
	public ActivityAssets? Assets { get; set; }

	/// <summary>
	/// Gets or sets the secrets for join/spectate/match.
	/// </summary>
	public ActivitySecrets? Secrets { get; set; }

	/// <summary>
	/// Gets or sets whether the activity is an instanced game session.
	/// </summary>
	public bool? Instance { get; set; }

	/// <summary>
	/// Gets or sets the activity flags.
	/// </summary>
	public int? Flags { get; set; }

	/// <summary>
	/// Gets or sets the buttons (max 2: label + url).
	/// </summary>
	public ActivityButton[]? Buttons { get; set; }
}
