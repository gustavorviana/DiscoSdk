using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord message.
/// </summary>
public class Message
{
    /// <summary>
    /// Gets or sets the ID of the message.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the channel the message was sent in.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild the message was sent in.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

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
    public List<User> Mentions { get; set; } = [];

    /// <summary>
    /// Gets or sets the roles specifically mentioned in this message.
    /// </summary>
    [JsonPropertyName("mention_roles")]
    public List<string> MentionRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets the channels specifically mentioned in this message.
    /// </summary>
    [JsonPropertyName("mention_channels")]
    public List<ChannelMention>? MentionChannels { get; set; }

    /// <summary>
    /// Gets or sets any attached files.
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<Attachment> Attachments { get; set; } = [];

    /// <summary>
    /// Gets or sets any embedded content.
    /// </summary>
    [JsonPropertyName("embeds")]
    public List<Embed> Embeds { get; set; } = [];

    /// <summary>
    /// Gets or sets the reactions to the message.
    /// </summary>
    [JsonPropertyName("reactions")]
    public List<Reaction>? Reactions { get; set; }

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
    public string? WebhookId { get; set; }

    /// <summary>
    /// Gets or sets the type of message.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

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
    public int? Flags { get; set; }

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
    public List<MessageComponent>? Components { get; set; }

    /// <summary>
    /// Gets or sets the stickers sent with the message.
    /// </summary>
    [JsonPropertyName("sticker_items")]
    public List<StickerItem>? StickerItems { get; set; }

    /// <summary>
    /// Gets or sets the stickers sent with the message.
    /// </summary>
    [JsonPropertyName("stickers")]
    public List<Sticker>? Stickers { get; set; }

    /// <summary>
    /// Gets or sets the position of the message in the thread.
    /// </summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }
}

/// <summary>
/// Represents a channel mention in a message.
/// </summary>
public class ChannelMention
{
    /// <summary>
    /// Gets or sets the ID of the channel.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the guild containing the channel.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of channel.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the channel.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}

/// <summary>
/// Represents an attachment in a message.
/// </summary>
public class Attachment
{
    /// <summary>
    /// Gets or sets the attachment ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the file attached.
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = default!;

    /// <summary>
    /// Gets or sets the description for the file.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the attachment's media type.
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the source URL of the file.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>
    /// Gets or sets a proxied URL of the file.
    /// </summary>
    [JsonPropertyName("proxy_url")]
    public string ProxyUrl { get; set; } = default!;

    /// <summary>
    /// Gets or sets the height of the file (if image).
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the file (if image).
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this attachment is ephemeral.
    /// </summary>
    [JsonPropertyName("ephemeral")]
    public bool? Ephemeral { get; set; }

    /// <summary>
    /// Gets or sets the duration of the audio file (if audio).
    /// </summary>
    [JsonPropertyName("duration_secs")]
    public double? DurationSecs { get; set; }

    /// <summary>
    /// Gets or sets the base64 encoded bytearray representing a sampled waveform (if audio).
    /// </summary>
    [JsonPropertyName("waveform")]
    public string? Waveform { get; set; }
}

/// <summary>
/// Represents an embed in a message.
/// </summary>
public class Embed
{
    /// <summary>
    /// Gets or sets the title of the embed.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of embed.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the description of the embed.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL of the embed.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the embed content.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the color code of the embed.
    /// </summary>
    [JsonPropertyName("color")]
    public int? Color { get; set; }

    /// <summary>
    /// Gets or sets the footer information.
    /// </summary>
    [JsonPropertyName("footer")]
    public EmbedFooter? Footer { get; set; }

    /// <summary>
    /// Gets or sets the image information.
    /// </summary>
    [JsonPropertyName("image")]
    public EmbedImage? Image { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail information.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public EmbedThumbnail? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the video information.
    /// </summary>
    [JsonPropertyName("video")]
    public EmbedVideo? Video { get; set; }

    /// <summary>
    /// Gets or sets the provider information.
    /// </summary>
    [JsonPropertyName("provider")]
    public EmbedProvider? Provider { get; set; }

    /// <summary>
    /// Gets or sets the author information.
    /// </summary>
    [JsonPropertyName("author")]
    public EmbedAuthor? Author { get; set; }

    /// <summary>
    /// Gets or sets the fields of the embed.
    /// </summary>
    [JsonPropertyName("fields")]
    public List<EmbedField>? Fields { get; set; }
}

/// <summary>
/// Represents an embed footer.
/// </summary>
public class EmbedFooter
{
    /// <summary>
    /// Gets or sets the footer text.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = default!;

    /// <summary>
    /// Gets or sets the URL of the footer icon.
    /// </summary>
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets a proxied URL of the footer icon.
    /// </summary>
    [JsonPropertyName("proxy_icon_url")]
    public string? ProxyIconUrl { get; set; }
}

/// <summary>
/// Represents an embed image.
/// </summary>
public class EmbedImage
{
    /// <summary>
    /// Gets or sets the source URL of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>
    /// Gets or sets a proxied URL of the image.
    /// </summary>
    [JsonPropertyName("proxy_url")]
    public string? ProxyUrl { get; set; }

    /// <summary>
    /// Gets or sets the height of the image.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the image.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

/// <summary>
/// Represents an embed thumbnail.
/// </summary>
public class EmbedThumbnail
{
    /// <summary>
    /// Gets or sets the source URL of the thumbnail.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    /// <summary>
    /// Gets or sets a proxied URL of the thumbnail.
    /// </summary>
    [JsonPropertyName("proxy_url")]
    public string? ProxyUrl { get; set; }

    /// <summary>
    /// Gets or sets the height of the thumbnail.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the thumbnail.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

/// <summary>
/// Represents an embed video.
/// </summary>
public class EmbedVideo
{
    /// <summary>
    /// Gets or sets the source URL of the video.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a proxied URL of the video.
    /// </summary>
    [JsonPropertyName("proxy_url")]
    public string? ProxyUrl { get; set; }

    /// <summary>
    /// Gets or sets the height of the video.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the width of the video.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

/// <summary>
/// Represents an embed provider.
/// </summary>
public class EmbedProvider
{
    /// <summary>
    /// Gets or sets the name of the provider.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the URL of the provider.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Represents an embed author.
/// </summary>
public class EmbedAuthor
{
    /// <summary>
    /// Gets or sets the name of the author.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the URL of the author.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the URL of the author icon.
    /// </summary>
    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets a proxied URL of the author icon.
    /// </summary>
    [JsonPropertyName("proxy_icon_url")]
    public string? ProxyIconUrl { get; set; }
}

/// <summary>
/// Represents an embed field.
/// </summary>
public class EmbedField
{
    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the field.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the field should be displayed inline.
    /// </summary>
    [JsonPropertyName("inline")]
    public bool? Inline { get; set; }
}

/// <summary>
/// Represents a reaction to a message.
/// </summary>
public class Reaction
{
    /// <summary>
    /// Gets or sets the number of times this emoji has been used to react.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user reacted using this emoji.
    /// </summary>
    [JsonPropertyName("me")]
    public bool Me { get; set; }

    /// <summary>
    /// Gets or sets the emoji information.
    /// </summary>
    [JsonPropertyName("emoji")]
    public Emoji Emoji { get; set; } = default!;
}

/// <summary>
/// Represents message activity.
/// </summary>
public class MessageActivity
{
    /// <summary>
    /// Gets or sets the type of message activity.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// Gets or sets the party ID from a Rich Presence event.
    /// </summary>
    [JsonPropertyName("party_id")]
    public string? PartyId { get; set; }
}

/// <summary>
/// Represents a message application.
/// </summary>
public class MessageApplication
{
    /// <summary>
    /// Gets or sets the ID of the application.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the icon hash of the application.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the ID of the embed's image asset.
    /// </summary>
    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; set; }
}

/// <summary>
/// Represents a message reference.
/// </summary>
public class MessageReference
{
    /// <summary>
    /// Gets or sets the ID of the originating message.
    /// </summary>
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the originating message's channel.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the originating message's guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message reference may not exist.
    /// </summary>
    [JsonPropertyName("fail_if_not_exists")]
    public bool? FailIfNotExists { get; set; }
}

/// <summary>
/// Represents a message interaction.
/// </summary>
public class MessageInteraction
{
    /// <summary>
    /// Gets or sets the ID of the interaction.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of interaction.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the application command.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the user who invoked the interaction.
    /// </summary>
    [JsonPropertyName("user")]
    public User User { get; set; } = default!;

    /// <summary>
    /// Gets or sets the member who invoked the interaction in the guild.
    /// </summary>
    [JsonPropertyName("member")]
    public GuildMember? Member { get; set; }
}

/// <summary>
/// Represents a message component.
/// </summary>
public class MessageComponent
{
    /// <summary>
    /// Gets or sets the type of component.
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// Gets or sets the style of the component.
    /// </summary>
    [JsonPropertyName("style")]
    public int? Style { get; set; }

    /// <summary>
    /// Gets or sets the label of the component.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the emoji of the component.
    /// </summary>
    [JsonPropertyName("emoji")]
    public Emoji? Emoji { get; set; }

    /// <summary>
    /// Gets or sets the custom ID of the component.
    /// </summary>
    [JsonPropertyName("custom_id")]
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets or sets the URL of the component.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is disabled.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    /// Gets or sets the components of the component.
    /// </summary>
    [JsonPropertyName("components")]
    public List<MessageComponent>? Components { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text for select components.
    /// </summary>
    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of items that must be chosen.
    /// </summary>
    [JsonPropertyName("min_values")]
    public int? MinValues { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items that can be chosen.
    /// </summary>
    [JsonPropertyName("max_values")]
    public int? MaxValues { get; set; }

    /// <summary>
    /// Gets or sets the options for select components.
    /// </summary>
    [JsonPropertyName("options")]
    public List<SelectOption>? Options { get; set; }
}

/// <summary>
/// Represents a select option.
/// </summary>
public class SelectOption
{
    /// <summary>
    /// Gets or sets the user-facing name of the option.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dev-defined value of the option.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the description of the option.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the emoji of the option.
    /// </summary>
    [JsonPropertyName("emoji")]
    public Emoji? Emoji { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this option should be selected by default.
    /// </summary>
    [JsonPropertyName("default")]
    public bool? Default { get; set; }
}

/// <summary>
/// Represents a sticker item.
/// </summary>
public class StickerItem
{
    /// <summary>
    /// Gets or sets the ID of the sticker.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the name of the sticker.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of sticker format.
    /// </summary>
    [JsonPropertyName("format_type")]
    public int FormatType { get; set; }
}

/// <summary>
/// Represents a sticker.
/// </summary>
public class Sticker
{
    /// <summary>
    /// Gets or sets the ID of the sticker.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the pack the sticker is from.
    /// </summary>
    [JsonPropertyName("pack_id")]
    public string? PackId { get; set; }

    /// <summary>
    /// Gets or sets the name of the sticker.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the description of the sticker.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the type of sticker format.
    /// </summary>
    [JsonPropertyName("format_type")]
    public int FormatType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this guild sticker can be used.
    /// </summary>
    [JsonPropertyName("available")]
    public bool? Available { get; set; }

    /// <summary>
    /// Gets or sets the ID of the guild that owns the sticker.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    /// <summary>
    /// Gets or sets the user that uploaded the guild sticker.
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the standard sticker's sort order within its pack.
    /// </summary>
    [JsonPropertyName("sort_value")]
    public int? SortValue { get; set; }

    /// <summary>
    /// Gets or sets the tags for the sticker.
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }
}

