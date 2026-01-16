namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord direct message channel.
/// </summary>
public interface IDmChannel : ITextBasedChannel
{
    /// <summary>
    /// Gets the ID of the creator of the DM.
    /// </summary>
    Snowflake? OwnerId { get; }
}