namespace DiscoSdk.Models.Enums;

/// <summary>
/// Represents the type of activity in a Discord presence.
/// </summary>
public enum ActivityType
{
	/// <summary>
	/// Playing a game.
	/// </summary>
	Playing = 0,

	/// <summary>
	/// Streaming.
	/// </summary>
	Streaming = 1,

	/// <summary>
	/// Listening to music.
	/// </summary>
	Listening = 2,

	/// <summary>
	/// Watching.
	/// </summary>
	Watching = 3,

	/// <summary>
	/// Custom status.
	/// </summary>
	Custom = 4,

	/// <summary>
	/// Competing in a game.
	/// </summary>
	Competing = 5
}