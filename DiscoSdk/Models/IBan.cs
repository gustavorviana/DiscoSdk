namespace DiscoSdk.Models;

/// <summary>
/// Represents a banned user in a Discord guild.
/// </summary>
public interface IBan
{
	/// <summary>
	/// Gets the reason for the ban, if any.
	/// </summary>
	string? Reason { get; }

	/// <summary>
	/// Gets the user who was banned.
	/// </summary>
	IUser User { get; }
}

