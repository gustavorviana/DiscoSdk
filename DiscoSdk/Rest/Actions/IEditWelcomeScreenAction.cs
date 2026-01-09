using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a guild welcome screen.
/// </summary>
public interface IEditWelcomeScreenAction : IRestAction<WelcomeScreen>
{
	/// <summary>
	/// Sets whether the welcome screen is enabled.
	/// </summary>
	/// <param name="enabled">True to enable the welcome screen, false to disable it.</param>
	/// <returns>The current <see cref="IEditWelcomeScreenAction"/> instance.</returns>
	IEditWelcomeScreenAction SetEnabled(bool enabled);

	/// <summary>
	/// Sets the description of the welcome screen.
	/// </summary>
	/// <param name="description">The description, or null to remove it.</param>
	/// <returns>The current <see cref="IEditWelcomeScreenAction"/> instance.</returns>
	IEditWelcomeScreenAction SetDescription(string? description);

	/// <summary>
	/// Sets the welcome channels.
	/// </summary>
	/// <param name="channels">The welcome channels, or null to remove them.</param>
	/// <returns>The current <see cref="IEditWelcomeScreenAction"/> instance.</returns>
	IEditWelcomeScreenAction SetWelcomeChannels(WelcomeScreenChannel[]? channels);
}

