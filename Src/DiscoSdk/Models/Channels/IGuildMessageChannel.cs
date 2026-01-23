namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a message channel in a Discord guild.
/// This interface groups channels that support text-based messaging.
/// </summary>
public interface IGuildMessageChannel : IGuildChannel, ITextBasedChannel
{

}

