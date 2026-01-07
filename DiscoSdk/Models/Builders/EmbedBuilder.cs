using DiscoSdk.Models.Messages.Embeds;

namespace DiscoSdk.Models.Builders;

/// <summary>
/// Fluent builder for creating Discord embeds.
/// </summary>
public class EmbedBuilder
{
    private readonly List<EmbedField> _fields = [];
    public readonly string? _title;
    public readonly string? _url;
    public string? _type;
    public string? _description;
    public string? _timestamp;
    public Color? _color;
    public EmbedFooter? _footer;
    public EmbedImage? _image;
    public EmbedThumbnail? _thumbnail;
    public EmbedVideo? _video;
    public EmbedProvider? _provider;
    public EmbedAuthor? _author;

    public EmbedBuilder(string title, string? url = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        if (title.Length > 256)
            throw new ArgumentException("Title cannot exceed 256 characters.", nameof(title));

        _title = title;
        _url = url;
    }

    /// <summary>
    /// Sets the description of the embed.
    /// </summary>
    /// <param name="description">The description text (max 4096 characters).</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
        if (description.Length > 4096)
            throw new ArgumentException("Description cannot exceed 4096 characters.", nameof(description));

        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the color of the embed.
    /// </summary>
    /// <param name="color">The color to set.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetColor(Color color)
    {
        _color = color;
        return this;
    }

    /// <summary>
    /// Sets the author of the embed.
    /// </summary>
    /// <param name="name">The author name (max 256 characters).</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetAuthor(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Author name cannot be null or empty.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentException("Author name cannot exceed 256 characters.", nameof(name));

        _author = new EmbedAuthor
        {
            Name = name
        };
        return this;
    }

    /// <summary>
    /// Sets the author of the embed with a URL.
    /// </summary>
    /// <param name="name">The author name (max 256 characters).</param>
    /// <param name="url">The URL to link the author name to.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetAuthor(string name, string url)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Author name cannot be null or empty.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentException("Author name cannot exceed 256 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));

        _author = new EmbedAuthor
        {
            Name = name,
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Sets the author of the embed with a URL and icon.
    /// </summary>
    /// <param name="name">The author name (max 256 characters).</param>
    /// <param name="url">The URL to link the author name to.</param>
    /// <param name="iconUrl">The URL of the author icon.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetAuthor(string name, string url, string iconUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Author name cannot be null or empty.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentException("Author name cannot exceed 256 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentException("Icon URL cannot be null or empty.", nameof(iconUrl));

        _author = new EmbedAuthor
        {
            Name = name,
            Url = url,
            IconUrl = iconUrl
        };
        return this;
    }

    /// <summary>
    /// Sets the thumbnail of the embed.
    /// </summary>
    /// <param name="url">The URL of the thumbnail image.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetThumbnail(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Thumbnail URL cannot be null or empty.", nameof(url));

        _thumbnail = new EmbedThumbnail
        {
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Adds a field to the embed.
    /// </summary>
    /// <param name="name">The field name (max 256 characters).</param>
    /// <param name="value">The field value (max 1024 characters).</param>
    /// <param name="inline">Whether the field should be displayed inline. Defaults to false.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder AddField(string name, string value, bool inline = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name cannot be null or empty.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentException("Field name cannot exceed 256 characters.", nameof(name));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Field value cannot be null or empty.", nameof(value));
        if (value.Length > 1024)
            throw new ArgumentException("Field value cannot exceed 1024 characters.", nameof(value));

        if (_fields.Count >= 25)
            throw new InvalidOperationException("Embed cannot have more than 25 fields.");

        _fields.Add(new EmbedField
        {
            Name = name,
            Value = value,
            Inline = inline
        });

        return this;
    }

    /// <summary>
    /// Sets the image of the embed.
    /// </summary>
    /// <param name="url">The URL of the image.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be null or empty.", nameof(url));

        _image = new EmbedImage
        {
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Sets the footer of the embed.
    /// </summary>
    /// <param name="text">The footer text (max 2048 characters).</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetFooter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Footer text cannot be null or empty.", nameof(text));
        if (text.Length > 2048)
            throw new ArgumentException("Footer text cannot exceed 2048 characters.", nameof(text));

        _footer = new EmbedFooter
        {
            Text = text
        };
        return this;
    }

    /// <summary>
    /// Sets the footer of the embed with an icon.
    /// </summary>
    /// <param name="text">The footer text (max 2048 characters).</param>
    /// <param name="iconUrl">The URL of the footer icon.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetFooter(string text, string iconUrl)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Footer text cannot be null or empty.", nameof(text));
        if (text.Length > 2048)
            throw new ArgumentException("Footer text cannot exceed 2048 characters.", nameof(text));
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentException("Icon URL cannot be null or empty.", nameof(iconUrl));

        _footer = new EmbedFooter
        {
            Text = text,
            IconUrl = iconUrl
        };
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the embed to the current time.
    /// </summary>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetTimestamp()
    {
        _timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the embed to a specific date and time.
    /// </summary>
    /// <param name="timestamp">The timestamp to set.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetTimestamp(DateTimeOffset timestamp)
    {
        _timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the embed from a DateTime.
    /// </summary>
    /// <param name="timestamp">The timestamp to set (will be converted to UTC).</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance.</returns>
    public EmbedBuilder SetTimestamp(DateTime timestamp)
    {
        _timestamp = new DateTimeOffset(timestamp).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return this;
    }

    /// <summary>
    /// Builds the embed.
    /// </summary>
    /// <returns>The built <see cref="Embed"/> instance.</returns>
    public Embed Build()
    {
        // Validate total embed length
        int totalLength = 0;
        if (!string.IsNullOrEmpty(_title))
            totalLength += _title.Length;

        if (!string.IsNullOrEmpty(_description))
            totalLength += _description.Length;

        if (_fields != null)
        {
            foreach (var field in _fields)
            {
                totalLength += field.Name?.Length ?? 0;
                totalLength += field.Value?.Length ?? 0;
            }
        }

        if (_footer != null && !string.IsNullOrEmpty(_footer.Text))
            totalLength += _footer.Text.Length;

        if (_author != null && !string.IsNullOrEmpty(_author.Name))
            totalLength += _author.Name.Length;

        if (totalLength > 6000)
            throw new InvalidOperationException("Total embed length (title + description + fields + footer + author) cannot exceed 6000 characters.");

        return new Embed
        {
            Author = _author,
            Color = _color,
            Description = _description,
            Fields = [.._fields!],
            Footer = _footer,
            Image = _image,
            Thumbnail = _thumbnail,
            Timestamp = _timestamp,
            Title = _title,
            Url = _url
        };
    }
}

