using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a guild widget.
/// </summary>
public interface IEditGuildWidgetAction : IRestAction<GuildWidget>
{
	/// <summary>
	/// Sets whether the widget is enabled.
	/// </summary>
	/// <param name="enabled">True to enable the widget, false to disable it.</param>
	/// <returns>The current <see cref="IEditGuildWidgetAction"/> instance.</returns>
	IEditGuildWidgetAction SetEnabled(bool enabled);

	/// <summary>
	/// Sets the channel ID for the widget invite.
	/// </summary>
	/// <param name="channelId">The channel ID, or null to remove the channel.</param>
	/// <returns>The current <see cref="IEditGuildWidgetAction"/> instance.</returns>
	IEditGuildWidgetAction SetChannelId(Snowflake? channelId);
}

