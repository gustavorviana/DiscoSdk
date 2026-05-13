namespace DiscoSdk.Models.Channels;

/// <summary>
/// A guild channel that carries a text chat: standard text and announcement channels, threads,
/// and (via Text-in-Voice) voice and stage channels. Combines the guild-channel surface
/// (<see cref="IGuildChannel"/>) with the text surface (<see cref="IGuildTextChannelBase"/>).
/// </summary>
public interface IGuildMessageChannel : IGuildChannel, IGuildTextChannelBase
{
}

