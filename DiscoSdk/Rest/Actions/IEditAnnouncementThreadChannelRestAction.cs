using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord announcement thread channel.
/// </summary>
public interface IEditAnnouncementThreadChannelRestAction : IRestAction<IAnnouncementThreadChannel>
{
	/// <summary>
	/// Sets the name of the thread.
	/// </summary>
	/// <param name="name">The new name of the thread (1-100 characters).</param>
	/// <returns>The current <see cref="IEditAnnouncementThreadChannelRestAction"/> instance.</returns>
	IEditAnnouncementThreadChannelRestAction SetName(string? name);

	/// <summary>
	/// Sets whether the thread is archived.
	/// </summary>
	/// <param name="archived">Whether the thread should be archived.</param>
	/// <returns>The current <see cref="IEditAnnouncementThreadChannelRestAction"/> instance.</returns>
	IEditAnnouncementThreadChannelRestAction SetArchived(bool? archived);

	/// <summary>
	/// Sets the auto-archive duration for the thread.
	/// </summary>
	/// <param name="duration">The duration in minutes (60, 1440, 4320, 10080).</param>
	/// <returns>The current <see cref="IEditAnnouncementThreadChannelRestAction"/> instance.</returns>
	IEditAnnouncementThreadChannelRestAction SetAutoArchiveDuration(int? duration);

	/// <summary>
	/// Sets whether the thread is locked.
	/// </summary>
	/// <param name="locked">Whether the thread should be locked.</param>
	/// <returns>The current <see cref="IEditAnnouncementThreadChannelRestAction"/> instance.</returns>
	IEditAnnouncementThreadChannelRestAction SetLocked(bool? locked);

	/// <summary>
	/// Sets whether non-moderators can add other non-moderators to the thread.
	/// </summary>
	/// <param name="invitable">Whether the thread should be invitable.</param>
	/// <returns>The current <see cref="IEditAnnouncementThreadChannelRestAction"/> instance.</returns>
	IEditAnnouncementThreadChannelRestAction SetInvitable(bool? invitable);

	/// <summary>
	/// Sets the rate limit per user for the thread.
	/// </summary>
	/// <param name="rateLimitPerUser">The rate limit in seconds (0-21600).</param>
	/// <returns>The current <see cref="IEditAnnouncementThreadChannelRestAction"/> instance.</returns>
	IEditAnnouncementThreadChannelRestAction SetRateLimitPerUser(int? rateLimitPerUser);

	/// <summary>
	/// Sets the channel flags.
	/// </summary>
	/// <param name="flags">The channel flags.</param>
	/// <returns>The current <see cref="IEditAnnouncementThreadChannelRestAction"/> instance.</returns>
	IEditAnnouncementThreadChannelRestAction SetFlags(ChannelFlags? flags);
}

