using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;
using DiscoSdk.Models.Messages.Embeds;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models.Messages;


/// <summary>
/// Represents a Discord message.
/// </summary>
public class Message
{
    /// <summary>
    /// Gets or sets the ID of the message.
    /// </summary>
    [JsonPropertyName("id")]
    public DiscordId Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the channel the message was sent in.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public DiscordId ChannelId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild the message was sent in.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public DiscordId? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the author of the message.
    /// </summary>
    [JsonPropertyName("author")]
    public User Author { get; set; } = default!;

    /// <summary>
    /// Gets or sets the member properties for this message's author.
    /// </summary>
    [JsonPropertyName("member")]
    public GuildMember? Member { get; set; }

    /// <summary>
    /// Gets or sets the contents of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    /// <summary>
    /// Gets or sets when this message was sent.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = default!;

    /// <summary>
    /// Gets or sets when this message was edited.
    /// </summary>
    [JsonPropertyName("edited_timestamp")]
    public string? EditedTimestamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this was a TTS message.
    /// </summary>
    [JsonPropertyName("tts")]
    public bool Tts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this message mentions everyone.
    /// </summary>
    [JsonPropertyName("mention_everyone")]
    public bool MentionEveryone { get; set; }

    /// <summary>
    /// Gets or sets the users specifically mentioned in the message.
    /// </summary>
    [JsonPropertyName("mentions")]
    public User[] Mentions { get; set; } = [];

    /// <summary>
    /// Gets or sets the roles specifically mentioned in this message.
    /// </summary>
    [JsonPropertyName("mention_roles")]
    public List<string> MentionRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets the channels specifically mentioned in this message.
    /// </summary>
    [JsonPropertyName("mention_channels")]
    public ChannelMention[]? MentionChannels { get; set; }

    /// <summary>
    /// Gets or sets any attached files.
    /// </summary>
    [JsonPropertyName("attachments")]
    public Attachment[] Attachments { get; set; } = [];

    /// <summary>
    /// Gets or sets any embedded content.
    /// </summary>
    [JsonPropertyName("embeds")]
    public Embed[] Embeds { get; set; } = [];

    /// <summary>
    /// Gets or sets the reactions to the message.
    /// </summary>
    [JsonPropertyName("reactions")]
    public Reaction[]? Reactions { get; set; }

    /// <summary>
    /// Gets or sets the nonce used for verifying a message was sent.
    /// </summary>
    [JsonPropertyName("nonce")]
    public object? Nonce { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this message is pinned.
    /// </summary>
    [JsonPropertyName("pinned")]
    public bool Pinned { get; set; }

    /// <summary>
    /// Gets or sets the ID of the webhook that sent this message.
    /// </summary>
    [JsonPropertyName("webhook_id")]
    public DiscordId? WebhookId { get; set; }

    /// <summary>
    /// Gets or sets the type of message.
    /// </summary>
    [JsonPropertyName("type")]
    public MessageType Type { get; set; }

    /// <summary>
    /// Gets or sets the sent with Premium subscription message activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public MessageActivity? Activity { get; set; }

    /// <summary>
    /// Gets or sets the application associated with the message.
    /// </summary>
    [JsonPropertyName("application")]
    public MessageApplication? Application { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application that created the message.
    /// </summary>
    [JsonPropertyName("application_id")]
    public string? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the data showing the source of a crosspost, channel follow add, pin, or reply message.
    /// </summary>
    [JsonPropertyName("message_reference")]
    public MessageReference? MessageReference { get; set; }

    /// <summary>
    /// Gets or sets the message flags.
    /// </summary>
    [JsonPropertyName("flags")]
    public MessageFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the message associated with the message_reference.
    /// </summary>
    [JsonPropertyName("referenced_message")]
    public Message? ReferencedMessage { get; set; }

    /// <summary>
    /// Gets or sets the interaction that created this message.
    /// </summary>
    [JsonPropertyName("interaction")]
    public MessageInteraction? Interaction { get; set; }

    /// <summary>
    /// Gets or sets the thread that was started from this message.
    /// </summary>
    [JsonPropertyName("thread")]
    public Channel? Thread { get; set; }

    /// <summary>
    /// Gets or sets the components of the message.
    /// </summary>
    [JsonPropertyName("components")]
    public MessageComponent[]? Components { get; set; }

    /// <summary>
    /// Gets or sets the stickers sent with the message.
    /// </summary>
    [JsonPropertyName("sticker_items")]
    public StickerItem[]? StickerItems { get; set; }

    /// <summary>
    /// Gets or sets the stickers sent with the message.
    /// </summary>
    [JsonPropertyName("stickers")]
    public Sticker[]? Stickers { get; set; }

    /// <summary>
    /// Gets or sets the position of the message in the thread.
    /// </summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }
}

