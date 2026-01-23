using DiscoSdk.Models.Activities;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using System.Collections.Immutable;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a member of a Discord guild.
/// </summary>
public interface IMember : IPermissionHolder
{
	/// <summary>
	/// The base URL template for guild member avatars.
	/// </summary>
	const string UrlAvatarUrl = "https://cdn.discordapp.com/guilds/%s/users/%s/avatars/%s.%s";

	/// <summary>
	/// The maximum timeout length in days.
	/// </summary>
	const int MaxTimeOutLength = 28;

	/// <summary>
	/// The maximum nickname length.
	/// </summary>
	const int MaxNicknameLength = 32;

	/// <summary>
	/// Gets the user this member represents.
	/// </summary>
	IUser User { get; }

	/// <summary>
	/// Gets the guild this member belongs to.
	/// </summary>
	IGuild Guild { get; }

	/// <summary>
	/// Gets the date and time when this member joined the guild.
	/// </summary>
	DateTimeOffset? TimeJoined { get; }

	/// <summary>
	/// Gets the date and time when this member started boosting the guild.
	/// </summary>
	DateTimeOffset? TimeBoosted { get; }

	/// <summary>
	/// Gets a value indicating whether this member is currently boosting the guild.
	/// </summary>
	bool IsBoosting => TimeBoosted != null;

	/// <summary>
	/// Gets the date and time when this member's timeout will end.
	/// </summary>
	DateTimeOffset? TimeOutEnd { get; }

	/// <summary>
	/// Gets a value indicating whether this member is currently timed out.
	/// </summary>
	bool IsTimedOut => TimeOutEnd != null && TimeOutEnd > DateTimeOffset.UtcNow;

	/// <summary>
	/// Gets the voice state of this member, if they are in a voice channel.
	/// </summary>
	IGuildVoiceState? VoiceState { get; }

    /// <summary>
    /// Gets the activities of this member.
    /// </summary>
    Activity[] Activities { get; }

	/// <summary>
	/// Gets the online status of this member.
	/// </summary>
	OnlineStatus OnlineStatus { get; }

	/// <summary>
	/// Gets the online status of this member for the specified client type.
	/// </summary>
	/// <param name="clientType">The client type to get the status for.</param>
	/// <returns>The online status for the specified client type.</returns>
	OnlineStatus GetOnlineStatus(ClientType clientType);

	/// <summary>
	/// Gets the active client types of this member.
	/// </summary>
	ImmutableHashSet<ClientType> ActiveClients { get; }

	/// <summary>
	/// Gets the nickname of this member, or null if they don't have one.
	/// </summary>
	string? Nickname { get; }

	/// <summary>
	/// Gets the effective name of this member (nickname if available, otherwise username).
	/// </summary>
	string EffectiveName { get; }

	/// <summary>
	/// Gets the avatar ID of this member, or null if they don't have a guild avatar.
	/// </summary>
	string? AvatarId { get; }

	/// <summary>
	/// Gets the avatar URL of this member, or null if they don't have a guild avatar.
	/// </summary>
	string? AvatarUrl { get; }

	/// <summary>
	/// Gets the effective avatar URL of this member (guild avatar if available, otherwise user avatar).
	/// </summary>
	string EffectiveAvatarUrl => AvatarUrl ?? User.EffectiveAvatarUrl;

	/// <summary>
	/// Gets the roles of this member, sorted by position.
	/// </summary>
	ImmutableList<IRole> Roles { get; }

	/// <summary>
	/// Gets the roles of this member, unsorted.
	/// </summary>
	ImmutableHashSet<IRole> UnsortedRoles { get; }

	/// <summary>
	/// Gets the colors of this member based on their highest role.
	/// </summary>
	RoleColors Colors { get; }

	/// <summary>
	/// Gets the primary color of this member.
	/// </summary>
	/// <remarks>This property is deprecated. Use <see cref="Colors"/>.Primary instead.</remarks>
	[Obsolete("Use Colors.Primary instead.")]
	Color Color => Colors.Primary;

	/// <summary>
	/// Gets the raw primary color value of this member.
	/// </summary>
	/// <remarks>This property is deprecated. Use <see cref="Colors"/>.PrimaryRaw instead.</remarks>
	[Obsolete("Use Colors.PrimaryRaw instead.")]
	int ColorRaw => Colors.PrimaryRaw;

	/// <summary>
	/// Gets the raw flags value of this member.
	/// </summary>
	int FlagsRaw { get; }

	/// <summary>
	/// Gets the flags of this member.
	/// </summary>
	ImmutableHashSet<MemberFlag> Flags => MemberFlagExtensions.FromRaw(FlagsRaw);

	/// <summary>
	/// Determines whether this member can interact with the specified member.
	/// </summary>
	/// <param name="member">The member to check interaction with.</param>
	/// <returns>True if this member can interact with the specified member, false otherwise.</returns>
	bool CanInteract(IMember member);

	/// <summary>
	/// Determines whether this member can interact with the specified role.
	/// </summary>
	/// <param name="role">The role to check interaction with.</param>
	/// <returns>True if this member can interact with the specified role, false otherwise.</returns>
	bool CanInteract(IRole role);

	/// <summary>
	/// Determines whether this member can interact with the specified emoji.
	/// </summary>
	/// <param name="emoji">The emoji to check interaction with.</param>
	/// <returns>True if this member can interact with the specified emoji, false otherwise.</returns>
	bool CanInteract(Emoji emoji);

	/// <summary>
	/// Gets a value indicating whether this member is the owner of the guild.
	/// </summary>
	bool IsOwner { get; }

	/// <summary>
	/// Gets a value indicating whether this member is pending (has not passed membership screening).
	/// </summary>
	bool IsPending { get; }

	/// <summary>
	/// Gets the default channel for this member, if available.
	/// </summary>
	IChannel? DefaultChannel { get; }

	/// <summary>
	/// Bans this member from the guild.
	/// </summary>
	/// <param name="deletionTimeframe">The number of days of messages to delete (0-7).</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task BanAsync(int deletionTimeframe, CancellationToken cancellationToken = default);

	/// <summary>
	/// Kicks this member from the guild.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task KickAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Times out this member for the specified duration.
	/// </summary>
	/// <param name="duration">The duration of the timeout.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task TimeoutForAsync(TimeSpan duration, CancellationToken cancellationToken = default);

	/// <summary>
	/// Times out this member until the specified date and time.
	/// </summary>
	/// <param name="until">The date and time when the timeout should end.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task TimeoutUntilAsync(DateTimeOffset until, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the timeout from this member.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task RemoveTimeoutAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Mutes or unmutes this member in voice channels.
	/// </summary>
	/// <param name="mute">True to mute, false to unmute.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task MuteAsync(bool mute, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deafens or undeafens this member in voice channels.
	/// </summary>
	/// <param name="deafen">True to deafen, false to undeafen.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task DeafenAsync(bool deafen, CancellationToken cancellationToken = default);

	/// <summary>
	/// Modifies the nickname of this member.
	/// </summary>
	/// <param name="nickname">The new nickname, or null to remove the nickname.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task ModifyNicknameAsync(string? nickname, CancellationToken cancellationToken = default);

	/// <summary>
	/// Modifies the flags of this member.
	/// </summary>
	/// <param name="newFlags">The new flags to set.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task ModifyFlagsAsync(IEnumerable<MemberFlag> newFlags, CancellationToken cancellationToken = default);
}