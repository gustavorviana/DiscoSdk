using DiscoSdk.Models;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for editing a Discord guild.
/// </summary>
public interface IEditGuildRestAction : IRestAction<IGuild>
{
	/// <summary>
	/// Sets the guild's name.
	/// </summary>
	/// <param name="name">The guild's name.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetName(string name);

	/// <summary>
	/// Sets the verification level required for the guild.
	/// </summary>
	/// <param name="level">The verification level, or null to not change it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetVerificationLevel(VerificationLevel? level);

	/// <summary>
	/// Sets the default message notification level.
	/// </summary>
	/// <param name="level">The default message notification level, or null to not change it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetDefaultMessageNotifications(DefaultMessageNotificationLevel? level);

	/// <summary>
	/// Sets the explicit content filter level.
	/// </summary>
	/// <param name="level">The explicit content filter level, or null to not change it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetExplicitContentFilter(ExplicitContentFilterLevel? level);

	/// <summary>
	/// Sets the AFK channel ID.
	/// </summary>
	/// <param name="channelId">The AFK channel ID, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetAfkChannelId(DiscordId? channelId);

	/// <summary>
	/// Sets the AFK timeout in seconds.
	/// </summary>
	/// <param name="timeout">The AFK timeout in seconds, or null to not change it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetAfkTimeout(int? timeout);

	/// <summary>
	/// Sets the guild's icon from image data.
	/// </summary>
	/// <param name="icon">The icon image data, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetIcon(byte[]? icon);

	/// <summary>
	/// Sets the guild's icon from a hash.
	/// </summary>
	/// <param name="iconHash">The icon hash, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetIcon(string? iconHash);

	/// <summary>
	/// Sets the guild's splash from image data.
	/// </summary>
	/// <param name="splash">The splash image data, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetSplash(byte[]? splash);

	/// <summary>
	/// Sets the guild's splash from a hash.
	/// </summary>
	/// <param name="splashHash">The splash hash, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetSplash(string? splashHash);

	/// <summary>
	/// Sets the guild's discovery splash from image data.
	/// </summary>
	/// <param name="discoverySplash">The discovery splash image data, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetDiscoverySplash(byte[]? discoverySplash);

	/// <summary>
	/// Sets the guild's discovery splash from a hash.
	/// </summary>
	/// <param name="discoverySplashHash">The discovery splash hash, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetDiscoverySplash(string? discoverySplashHash);

	/// <summary>
	/// Sets the guild's banner from image data.
	/// </summary>
	/// <param name="banner">The banner image data, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetBanner(byte[]? banner);

	/// <summary>
	/// Sets the guild's banner from a hash.
	/// </summary>
	/// <param name="bannerHash">The banner hash, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetBanner(string? bannerHash);

	/// <summary>
	/// Sets the system channel ID.
	/// </summary>
	/// <param name="channelId">The system channel ID, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetSystemChannelId(DiscordId? channelId);

	/// <summary>
	/// Sets the system channel flags.
	/// </summary>
	/// <param name="flags">The system channel flags, or null to not change them.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetSystemChannelFlags(SystemChannelFlags? flags);

	/// <summary>
	/// Sets the rules channel ID.
	/// </summary>
	/// <param name="channelId">The rules channel ID, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetRulesChannelId(DiscordId? channelId);

	/// <summary>
	/// Sets the public updates channel ID.
	/// </summary>
	/// <param name="channelId">The public updates channel ID, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetPublicUpdatesChannelId(DiscordId? channelId);

	/// <summary>
	/// Sets the preferred locale.
	/// </summary>
	/// <param name="locale">The preferred locale, or null to not change it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetPreferredLocale(string? locale);

	/// <summary>
	/// Sets the guild's description.
	/// </summary>
	/// <param name="description">The guild's description, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetDescription(string? description);

	/// <summary>
	/// Sets whether the premium progress bar is enabled.
	/// </summary>
	/// <param name="enabled">True to enable the premium progress bar, false to disable it, or null to not change it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetPremiumProgressBarEnabled(bool? enabled);

	/// <summary>
	/// Sets the safety alerts channel ID.
	/// </summary>
	/// <param name="channelId">The safety alerts channel ID, or null to remove it.</param>
	/// <returns>The current <see cref="IEditGuildRestAction"/> instance.</returns>
	IEditGuildRestAction SetSafetyAlertsChannelId(DiscordId? channelId);
}