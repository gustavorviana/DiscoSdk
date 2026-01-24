using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using DiscoSdk.Models.Messages.Pools;
using DiscoSdk.Rest.Actions.Messages;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Requests.Messages;

/// <summary>
/// Represents a request to create a message in Discord.
/// Contains all fields supported by the Discord API for message creation.
/// </summary>
internal class MessageCreateRequest
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
    /// Gets or sets the poll configuration.
    /// </summary>
    [JsonPropertyName("poll")]
    public Poll? Poll { get; set; }

    /// <summary>
    /// Gets or sets the attachment metadata.
    /// This describes files sent via multipart upload.
    /// </summary>
    [JsonPropertyName("attachments")]
	public MessageAttachmentMetadata[]? Attachments { get; set; }

	/// <summary>
	/// Gets or sets the message flags.
	/// </summary>
	[JsonPropertyName("flags")]
	public MessageFlags? Flags { get; set; }
}