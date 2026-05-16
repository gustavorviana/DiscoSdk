namespace DiscoSdk.Models;

/// <summary>
/// Read-only view of a guild's welcome screen — the panel shown to new members on Community-enabled
/// guilds.
/// </summary>
public interface IWelcomeScreen
{
	/// <summary>Server description shown above the channels list, or <c>null</c> if unset.</summary>
	string? Description { get; }

	/// <summary>Up-to-5 highlighted channels with description and optional emoji.</summary>
	IReadOnlyList<IWelcomeScreenChannel>? WelcomeChannels { get; }
}
