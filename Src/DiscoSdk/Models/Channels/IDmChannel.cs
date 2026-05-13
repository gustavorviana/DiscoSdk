namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord direct-message channel (a one-to-one DM). Group DMs are represented by
/// <see cref="IGroupDmChannel"/>.
/// </summary>
public interface IDmChannel : ITextBasedChannel
{
}
