using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a guild welcome screen.
/// </summary>
public interface IEditWelcomeScreenAction : IRestAction<WelcomeScreen>
{
    /// <summary>
    /// Enables or disables the welcome screen.
    /// </summary>
    /// <param name="enabled">Whether the welcome screen should be enabled.</param>
    /// <returns>The current <see cref="IEditWelcomeScreenAction"/> instance.</returns>
    IEditWelcomeScreenAction SetEnabled(bool enabled);

    /// <summary>
    /// Sets the welcome screen description.
    /// </summary>
    /// <param name="description">The description to display.</param>
    /// <returns>The current <see cref="IEditWelcomeScreenAction"/> instance.</returns>
    IEditWelcomeScreenAction SetDescription(string? description);

    /// <summary>
    /// Clears all configured welcome channels.
    /// </summary>
    /// <returns>The current <see cref="IEditWelcomeScreenAction"/> instance.</returns>
    IEditWelcomeScreenAction ClearChannels();

    /// <summary>
    /// Adds or replaces a welcome channel entry.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="description">The channel description.</param>
    /// <param name="emojiId">Optional emoji id.</param>
    /// <param name="emojiName">Optional emoji name.</param>
    /// <returns>The current <see cref="IEditWelcomeScreenAction"/> instance.</returns>
    IEditWelcomeScreenAction AddChannel(
        Snowflake channelId,
        string? description = null,
        Snowflake? emojiId = null,
        string? emojiName = null);
}