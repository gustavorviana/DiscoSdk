namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord group direct-message channel (a DM with three or more participants).
/// </summary>
public interface IGroupDmChannel : IDmChannel
{
	/// <summary>
	/// Gets the ID of the user who owns (created) this group DM.
	/// </summary>
	Snowflake OwnerId { get; }
}
