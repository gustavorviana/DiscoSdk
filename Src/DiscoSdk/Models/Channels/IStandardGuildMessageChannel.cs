namespace DiscoSdk.Models.Channels;

/// <summary>
/// A positionable guild message channel — text and announcement channels. Excludes threads
/// (which have no position) and voice/stage channels (whose primary purpose is audio).
/// </summary>
public interface IStandardGuildMessageChannel : IStandardGuildChannel, IGuildMessageChannel
{
}

