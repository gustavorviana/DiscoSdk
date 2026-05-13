namespace DiscoSdk.Models.Enums;

/// <summary>
/// The Discord webhook type.
/// </summary>
public enum WebhookType
{
	/// <summary>An incoming webhook that posts messages to a channel via a token.</summary>
	Incoming = 1,

	/// <summary>A Channel-Follower webhook used internally for cross-posting announcement-channel messages.</summary>
	ChannelFollower = 2,

	/// <summary>An Application webhook used for interaction-response follow-ups.</summary>
	Application = 3
}
