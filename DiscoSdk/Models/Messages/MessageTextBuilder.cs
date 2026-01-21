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
    public MessageTextBuilder AppendLine(string text)
    {
        _content.AppendLine(text);
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