using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Mentions;
using DiscoSdk.Rest.Actions.Messages;
using System.Text;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Builds a message text while tracking semantic mentions (users, roles, everyone),
/// automatically generating a safe <see cref="AllowedMentions"/> payload.
///
/// This builder allows you to write mentions that appear visually in the message
/// while precisely controlling which ones will actually notify (ping).
/// </summary>
public sealed class MessageTextBuilder : MentionBuilderBase<MessageTextBuilder>
{
    /// <summary>
    /// Accumulates the raw textual content of the message.
    /// </summary>
    private readonly StringBuilder _content = new();

    /// <summary>
    /// Appends raw text to the message content.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>The current <see cref="MessageTextBuilder"/> instance.</returns>
    public MessageTextBuilder Append(string text)
    {
        _content.Append(text);
        return this;
    }

    /// <summary>
    /// Appends raw text followed by a line break.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>The current <see cref="MessageTextBuilder"/> instance.</returns>
    public MessageTextBuilder AppendLine(string? text = null)
    {
        _content.AppendLine(text ?? String.Empty);
        return this;
    }

    /// <summary>
    /// Appends the textual representation of a <see cref="Mention"/> into the message
    /// content and registers it for later generation of <see cref="AllowedMentions"/>.
    /// </summary>
    /// <param name="mention">The mention to append.</param>
    /// <returns>The current <see cref="MessageTextBuilder"/> instance.</returns>
    public override MessageTextBuilder AppendMention(Mention mention)
    {
        _content.Append(mention.ToString());
        return base.AppendMention(mention);
    }

    /// <summary>
    /// Appends a Discord timestamp mention for the current UTC time.
    /// </summary>
    /// <param name="format">The timestamp format. Defaults to <see cref="TimestampFormat.ShortDateTime"/>.</param>
    /// <returns>The current <see cref="MessageTextBuilder"/> instance.</returns>
    public MessageTextBuilder AppendTimestamp(TimestampFormat format = TimestampFormat.ShortDateTime)
    {
        return AppendTimestamp(DateTimeOffset.UtcNow, format);
    }

    /// <summary>
    /// Appends a Discord timestamp mention for a specific date and time.
    /// </summary>
    /// <param name="timestamp">The timestamp to display. Will be converted to UTC.</param>
    /// <param name="format">The timestamp format. Defaults to <see cref="TimestampFormat.ShortDateTime"/>.</param>
    /// <returns>The current <see cref="MessageTextBuilder"/> instance.</returns>
    public MessageTextBuilder AppendTimestamp(DateTimeOffset timestamp, TimestampFormat format = TimestampFormat.ShortDateTime)
    {
        var unixTimestamp = timestamp.ToUniversalTime().ToUnixTimeSeconds();
        var formatChar = (char)format;
        _content.Append($"<t:{unixTimestamp}:{formatChar}>");
        return this;
    }

    /// <summary>
    /// Appends a Discord timestamp mention from a DateTime (converted to UTC).
    /// </summary>
    /// <param name="timestamp">The timestamp to display. Will be converted to UTC.</param>
    /// <param name="format">The timestamp format. Defaults to <see cref="TimestampFormat.ShortDateTime"/>.</param>
    /// <returns>The current <see cref="MessageTextBuilder"/> instance.</returns>
    public MessageTextBuilder AppendTimestamp(DateTime timestamp, TimestampFormat format = TimestampFormat.ShortDateTime)
    {
        return AppendTimestamp(new DateTimeOffset(timestamp).ToUniversalTime(), format);
    }

    /// <summary>
    /// Returns the built message content.
    /// </summary>
    public override string ToString()
    {
        return _content.ToString();
    }

    public static implicit operator MentionBuilder(MessageTextBuilder builder)
    {
        var targetBuilder = new MentionBuilder();
        builder.CopyTo(targetBuilder);
        return targetBuilder;
    }
}