using DiscoSdk.Models.Messages.Embeds;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Fluent builder for creating Discord embeds.
/// Provides methods to configure embed properties such as title, description, color, fields, images, and more.
/// </summary>
public class EmbedBuilder
{
    private readonly List<EmbedField> _fields = [];
    private readonly string? _title;
    private readonly string? _url;
    private string? _type;
    private string? _description;
    private string? _timestamp;
    private Color? _color;
    private EmbedFooter? _footer;
    private EmbedImage? _image;
    private EmbedThumbnail? _thumbnail;
    private EmbedVideo? _video;
    private EmbedProvider? _provider;
    private EmbedAuthor? _author;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbedBuilder"/> class with a title and optional URL.
    /// </summary>
    /// <param name="title">The title of the embed. Must be between 1 and 256 characters.</param>
    /// <param name="url">Optional URL to link the title to. Must be a valid HTTP/HTTPS URL if provided.</param>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or exceeds 256 characters.</exception>
    /// <exception cref="ArgumentException">Thrown when url is provided but is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder(string title, string? url = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null, empty, or contain only whitespace.", nameof(title));
        if (title.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(title), $"Title cannot exceed 256 characters. Current length: {title.Length}.");

        if (!string.IsNullOrWhiteSpace(url) && !IsValidUrl(url))
            throw new ArgumentException("URL must be a valid HTTP or HTTPS URL.", nameof(url));

        _title = title;
        _url = url;
    }

    /// <summary>
    /// Sets the description of the embed.
    /// </summary>
    /// <param name="description">The description text. Must be between 1 and 4096 characters.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when description is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when description exceeds 4096 characters.</exception>
    public EmbedBuilder SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null, empty, or contain only whitespace.", nameof(description));
        if (description.Length > 4096)
            throw new ArgumentOutOfRangeException(nameof(description), $"Description cannot exceed 4096 characters. Current length: {description.Length}.");

        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the color of the embed sidebar.
    /// </summary>
    /// <param name="color">The color to set. Can be null to remove the color.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    public EmbedBuilder SetColor(Color color)
    {
        _color = color;
        return this;
    }

    /// <summary>
    /// Sets the author of the embed with only a name.
    /// </summary>
    /// <param name="name">The author name. Must be between 1 and 256 characters.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when name exceeds 256 characters.</exception>
    public EmbedBuilder SetAuthor(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Author name cannot be null, empty, or contain only whitespace.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(name), $"Author name cannot exceed 256 characters. Current length: {name.Length}.");

        _author = new EmbedAuthor
        {
            Name = name
        };
        return this;
    }

    /// <summary>
    /// Sets the author of the embed with a name and URL.
    /// </summary>
    /// <param name="name">The author name. Must be between 1 and 256 characters.</param>
    /// <param name="url">The URL to link the author name to. Must be a valid HTTP/HTTPS URL.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when name exceeds 256 characters.</exception>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder SetAuthor(string name, string url)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Author name cannot be null, empty, or contain only whitespace.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(name), $"Author name cannot exceed 256 characters. Current length: {name.Length}.");
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null, empty, or contain only whitespace.", nameof(url));
        if (!IsValidUrl(url))
            throw new ArgumentException("URL must be a valid HTTP or HTTPS URL.", nameof(url));

        _author = new EmbedAuthor
        {
            Name = name,
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Sets the author of the embed with a name, URL, and icon.
    /// </summary>
    /// <param name="name">The author name. Must be between 1 and 256 characters.</param>
    /// <param name="url">The URL to link the author name to. Must be a valid HTTP/HTTPS URL.</param>
    /// <param name="iconUrl">The URL of the author icon. Must be a valid HTTP/HTTPS URL.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when name exceeds 256 characters.</exception>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    /// <exception cref="ArgumentException">Thrown when iconUrl is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder SetAuthor(string name, string url, string iconUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Author name cannot be null, empty, or contain only whitespace.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(name), $"Author name cannot exceed 256 characters. Current length: {name.Length}.");
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null, empty, or contain only whitespace.", nameof(url));
        if (!IsValidUrl(url))
            throw new ArgumentException("URL must be a valid HTTP or HTTPS URL.", nameof(url));
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentException("Icon URL cannot be null, empty, or contain only whitespace.", nameof(iconUrl));
        if (!IsValidUrl(iconUrl))
            throw new ArgumentException("Icon URL must be a valid HTTP or HTTPS URL.", nameof(iconUrl));

        _author = new EmbedAuthor
        {
            Name = name,
            Url = url,
            IconUrl = iconUrl
        };
        return this;
    }

    /// <summary>
    /// Sets the thumbnail image of the embed.
    /// </summary>
    /// <param name="url">The URL of the thumbnail image. Must be a valid HTTP/HTTPS URL.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder SetThumbnail(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Thumbnail URL cannot be null, empty, or contain only whitespace.", nameof(url));
        if (!IsValidUrl(url))
            throw new ArgumentException("Thumbnail URL must be a valid HTTP or HTTPS URL.", nameof(url));

        _thumbnail = new EmbedThumbnail
        {
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Adds a field to the embed. An embed can have a maximum of 25 fields.
    /// </summary>
    /// <param name="name">The field name. Must be between 1 and 256 characters.</param>
    /// <param name="value">The field value. Must be between 1 and 1024 characters.</param>
    /// <param name="inline">Whether the field should be displayed inline. Defaults to false.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when name exceeds 256 characters.</exception>
    /// <exception cref="ArgumentException">Thrown when value is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value exceeds 1024 characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add more than 25 fields.</exception>
    public EmbedBuilder AddField(string name, string value, bool inline = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Field name cannot be null, empty, or contain only whitespace.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(name), $"Field name cannot exceed 256 characters. Current length: {name.Length}.");
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Field value cannot be null, empty, or contain only whitespace.", nameof(value));
        if (value.Length > 1024)
            throw new ArgumentOutOfRangeException(nameof(value), $"Field value cannot exceed 1024 characters. Current length: {value.Length}.");

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
    /// Sets the main image of the embed.
    /// </summary>
    /// <param name="url">The URL of the image. Must be a valid HTTP/HTTPS URL.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder SetImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be null, empty, or contain only whitespace.", nameof(url));
        if (!IsValidUrl(url))
            throw new ArgumentException("Image URL must be a valid HTTP or HTTPS URL.", nameof(url));

        _image = new EmbedImage
        {
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Sets the footer of the embed with only text.
    /// </summary>
    /// <param name="text">The footer text. Must be between 1 and 2048 characters.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when text is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when text exceeds 2048 characters.</exception>
    public EmbedBuilder SetFooter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Footer text cannot be null, empty, or contain only whitespace.", nameof(text));
        if (text.Length > 2048)
            throw new ArgumentOutOfRangeException(nameof(text), $"Footer text cannot exceed 2048 characters. Current length: {text.Length}.");

        _footer = new EmbedFooter
        {
            Text = text
        };
        return this;
    }

    /// <summary>
    /// Sets the footer of the embed with text and an icon.
    /// </summary>
    /// <param name="text">The footer text. Must be between 1 and 2048 characters.</param>
    /// <param name="iconUrl">The URL of the footer icon. Must be a valid HTTP/HTTPS URL.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when text is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when text exceeds 2048 characters.</exception>
    /// <exception cref="ArgumentException">Thrown when iconUrl is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder SetFooter(string text, string iconUrl)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Footer text cannot be null, empty, or contain only whitespace.", nameof(text));
        if (text.Length > 2048)
            throw new ArgumentOutOfRangeException(nameof(text), $"Footer text cannot exceed 2048 characters. Current length: {text.Length}.");
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentException("Icon URL cannot be null, empty, or contain only whitespace.", nameof(iconUrl));
        if (!IsValidUrl(iconUrl))
            throw new ArgumentException("Icon URL must be a valid HTTP or HTTPS URL.", nameof(iconUrl));

        _footer = new EmbedFooter
        {
            Text = text,
            IconUrl = iconUrl
        };
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the embed to the current UTC time.
    /// </summary>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    public EmbedBuilder SetTimestamp()
    {
        _timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the embed to a specific date and time.
    /// </summary>
    /// <param name="timestamp">The timestamp to set. Will be converted to ISO 8601 format.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    public EmbedBuilder SetTimestamp(DateTimeOffset timestamp)
    {
        _timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return this;
    }

    /// <summary>
    /// Sets the timestamp of the embed from a DateTime (converted to UTC).
    /// </summary>
    /// <param name="timestamp">The timestamp to set. Will be converted to UTC and formatted as ISO 8601.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    public EmbedBuilder SetTimestamp(DateTime timestamp)
    {
        _timestamp = new DateTimeOffset(timestamp).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return this;
    }

    /// <summary>
    /// Sets the type of the embed. Typically "rich" for rich embeds.
    /// </summary>
    /// <param name="type">The embed type. Usually "rich" for user-created embeds.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when type is null, empty, or contains only whitespace.</exception>
    public EmbedBuilder SetType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be null, empty, or contain only whitespace.", nameof(type));

        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the video information for the embed.
    /// </summary>
    /// <param name="url">The URL of the video. Must be a valid HTTP/HTTPS URL.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder SetVideo(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Video URL cannot be null, empty, or contain only whitespace.", nameof(url));
        if (!IsValidUrl(url))
            throw new ArgumentException("Video URL must be a valid HTTP or HTTPS URL.", nameof(url));

        _video = new EmbedVideo
        {
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Sets the video information for the embed with dimensions.
    /// </summary>
    /// <param name="url">The URL of the video. Must be a valid HTTP/HTTPS URL.</param>
    /// <param name="width">The width of the video in pixels.</param>
    /// <param name="height">The height of the video in pixels.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are negative.</exception>
    public EmbedBuilder SetVideo(string url, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Video URL cannot be null, empty, or contain only whitespace.", nameof(url));
        if (!IsValidUrl(url))
            throw new ArgumentException("Video URL must be a valid HTTP or HTTPS URL.", nameof(url));
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height cannot be negative.");

        _video = new EmbedVideo
        {
            Url = url,
            Width = width,
            Height = height
        };
        return this;
    }

    /// <summary>
    /// Sets the provider information for the embed.
    /// </summary>
    /// <param name="name">The provider name. Must be between 1 and 256 characters.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when name exceeds 256 characters.</exception>
    public EmbedBuilder SetProvider(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be null, empty, or contain only whitespace.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(name), $"Provider name cannot exceed 256 characters. Current length: {name.Length}.");

        _provider = new EmbedProvider
        {
            Name = name
        };
        return this;
    }

    /// <summary>
    /// Sets the provider information for the embed with a name and URL.
    /// </summary>
    /// <param name="name">The provider name. Must be between 1 and 256 characters.</param>
    /// <param name="url">The provider URL. Must be a valid HTTP/HTTPS URL.</param>
    /// <returns>The current <see cref="EmbedBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or contains only whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when name exceeds 256 characters.</exception>
    /// <exception cref="ArgumentException">Thrown when url is null, empty, or is not a valid HTTP/HTTPS URL.</exception>
    public EmbedBuilder SetProvider(string name, string url)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be null, empty, or contain only whitespace.", nameof(name));
        if (name.Length > 256)
            throw new ArgumentOutOfRangeException(nameof(name), $"Provider name cannot exceed 256 characters. Current length: {name.Length}.");
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Provider URL cannot be null, empty, or contain only whitespace.", nameof(url));
        if (!IsValidUrl(url))
            throw new ArgumentException("Provider URL must be a valid HTTP or HTTPS URL.", nameof(url));

        _provider = new EmbedProvider
        {
            Name = name,
            Url = url
        };
        return this;
    }

    /// <summary>
    /// Builds and validates the embed according to Discord API constraints.
    /// </summary>
    /// <returns>The built <see cref="Embed"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the total embed length exceeds 6000 characters.</exception>
    public Embed Build()
    {
        // Validate total embed length (Discord API limit: 6000 characters)
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
            throw new InvalidOperationException($"Total embed length (title + description + fields + footer + author) cannot exceed 6000 characters. Current length: {totalLength}.");

        return new Embed
        {
            Author = _author,
            Color = _color,
            Description = _description,
            Fields = [.._fields!],
            Footer = _footer,
            Image = _image,
            Thumbnail = _thumbnail,
            Video = _video,
            Provider = _provider,
            Timestamp = _timestamp,
            Title = _title,
            Type = _type,
            Url = _url
        };
    }

    /// <summary>
    /// Validates if a string is a well-formed HTTP or HTTPS URL.
    /// </summary>
    /// <param name="url">The URL string to validate.</param>
    /// <returns>True if the URL is valid and uses HTTP or HTTPS scheme; otherwise, false.</returns>
    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}

