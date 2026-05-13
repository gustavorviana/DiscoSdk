namespace DiscoSdk.Models.Channels;

/// <summary>
/// A guild channel that occupies a position in the channel list — i.e. every guild channel
/// except threads (text, announcement, voice, stage, forum, media and categories).
/// </summary>
public interface IStandardGuildChannel : IGuildChannel, IPositionableChannel
{
}
