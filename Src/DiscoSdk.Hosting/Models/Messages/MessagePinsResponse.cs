namespace DiscoSdk.Models.Messages;

/// <summary>
/// Response of <c>GET /channels/{channel.id}/messages/pins</c>.
/// </summary>
internal class MessagePinsResponse
{
    /// <summary>
    /// Gets or sets the pins in this page, newest first.
    /// </summary>
    public MessagePin[] Items { get; set; } = [];

    /// <summary>
    /// Gets or sets whether more pins exist before the last item of this page.
    /// </summary>
    public bool HasMore { get; set; }
}

/// <summary>
/// Discord's message pin object.
/// </summary>
internal class MessagePin
{
    /// <summary>
    /// Gets or sets when the message was pinned.
    /// </summary>
    public DateTimeOffset PinnedAt { get; set; }

    /// <summary>
    /// Gets or sets the pinned message.
    /// </summary>
    public Message Message { get; set; } = default!;
}
