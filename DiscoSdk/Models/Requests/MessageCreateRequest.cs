using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests;

/// <summary>
/// Represents a request to create a message in Discord.
/// Contains all fields supported by the Discord API for message creation.
/// </summary>
public class MessageCreateRequest
{
	/// <summary>
	/// Gets or sets the message content (max 2000 characters).
	/// </summary>
	[JsonPropertyName("content")]
	public string? Content { get; set; }

	/// <summary>
	/// Gets or sets a nonce that can be used for message verification.
	/// Can be an integer or string.
	/// </summary>
	[JsonPropertyName("nonce")]
	public object? Nonce { get; set; }

	/// <summary>
	/// Gets or sets whether the message should be sent as TTS (text-to-speech).
	/// </summary>
	[JsonPropertyName("tts")]
	public bool? Tts { get; set; }

	/// <summary>
	/// Gets or sets the embeds to include in the message (max 10).
	/// </summary>
	[JsonPropertyName("embeds")]
	public Embed[]? Embeds { get; set; }

	/// <summary>
	/// Gets or sets the allowed mentions configuration.
	/// </summary>
	[JsonPropertyName("allowed_mentions")]
	public AllowedMentions? AllowedMentions { get; set; }

	/// <summary>
	/// Gets or sets the message reference (for replying to a message).
	/// </summary>
	[JsonPropertyName("message_reference")]
	public MessageReference? MessageReference { get; set; }

	/// <summary>
	/// Gets or sets the message components (buttons, select menus, etc.) (max 5 rows).
	/// </summary>
	[JsonPropertyName("components")]
	public MessageComponent[]? Components { get; set; }

	/// <summary>
	/// Gets or sets the IDs of stickers to send with the message (max 3).
	/// </summary>
	[JsonPropertyName("sticker_ids")]
	public string[]? StickerIds { get; set; }

	/// <summary>
	/// Gets or sets the message flags.
	/// </summary>
	[JsonPropertyName("flags")]
	public MessageFlags? Flags { get; set; }
}

/// <summary>
/// Represents allowed mentions configuration for a message.
/// </summary>
public class AllowedMentions
{
	/// <summary>
	/// Gets or sets an array of allowed mention types to parse from the content.
	/// Valid values: "roles", "users", "everyone"
	/// </summary>
	[JsonPropertyName("parse")]
	public string[]? Parse { get; set; }

	/// <summary>
	/// Gets or sets an array of role IDs to mention (max 100).
	/// </summary>
	[JsonPropertyName("roles")]
	public string[]? Roles { get; set; }

	/// <summary>
	/// Gets or sets an array of user IDs to mention (max 100).
	/// </summary>
	[JsonPropertyName("users")]
	public string[]? Users { get; set; }

	/// <summary>
	/// Gets or sets whether to mention the user who replied.
	/// </summary>
	[JsonPropertyName("replied_user")]
	public bool? RepliedUser { get; set; }
}

