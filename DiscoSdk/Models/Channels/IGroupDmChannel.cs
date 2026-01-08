namespace DiscoSdk.Models.Channels;

/// <summary>
/// Represents a Discord group direct message channel.
/// </summary>
public interface IGroupDmChannel : IDmChannel
{
	/// <summary>
	/// Gets the icon hash of the group DM.
	/// </summary>
	string? Icon { get; }

	/// <summary>
	/// Gets the application ID of the group DM creator if it is bot-created.
	/// </summary>
	DiscordId? ApplicationId { get; }
}

