using Microsoft.Extensions.Logging;

namespace DiscoSdk.Hosting.Wrappers.Messages;


/// <summary>
/// Per-process, per-field gate that emits the "MessageContent intent missing" warning at most
/// once. Uses <see cref="Interlocked.CompareExchange(ref int, int, int)"/> on a 0/1 flag so the
/// first read on a hot path wins without locking the rest.
/// </summary>
internal static class MessageContentWarnTracker
{
    private static int _content;
    private static int _embeds;
    private static int _attachments;
    private static int _components;
    private static int _poll;

    public static void WarnOnce(ILogger logger, MessageContentField field)
    {
        if (!TryClaimSlot(field))
            return;

        logger.LogWarning(
            "Accessed Message.{Field} without the MessageContent intent — the value will be empty for non-exempt messages (exemptions: bot is author, bot is mentioned, DM/GroupDm channel). Enable DiscordIntent.MessageContent on DiscordClientBuilder and in the Developer Portal. This warning logs once per field per process.",
            field);
    }

    internal static void ResetForTests()
    {
        Interlocked.Exchange(ref _content, 0);
        Interlocked.Exchange(ref _embeds, 0);
        Interlocked.Exchange(ref _attachments, 0);
        Interlocked.Exchange(ref _components, 0);
        Interlocked.Exchange(ref _poll, 0);
    }

    private static bool TryClaimSlot(MessageContentField field)
    {
        return field switch
        {
            MessageContentField.Content => Interlocked.CompareExchange(ref _content, 1, 0) == 0,
            MessageContentField.Embeds => Interlocked.CompareExchange(ref _embeds, 1, 0) == 0,
            MessageContentField.Attachments => Interlocked.CompareExchange(ref _attachments, 1, 0) == 0,
            MessageContentField.Components => Interlocked.CompareExchange(ref _components, 1, 0) == 0,
            MessageContentField.Poll => Interlocked.CompareExchange(ref _poll, 1, 0) == 0,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };
    }
}