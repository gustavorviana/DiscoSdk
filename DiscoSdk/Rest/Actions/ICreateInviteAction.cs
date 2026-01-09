using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a REST action for creating a Discord invite.
/// </summary>
public interface ICreateInviteAction : IRestAction<IInvite>
{
	/// <summary>
	/// Sets a check function that must return true for the invite to be created.
	/// </summary>
	/// <param name="check">The check function to execute before creating the invite.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetCheck(Func<bool> check);

	/// <summary>
	/// Sets the maximum age of the invite in seconds.
	/// </summary>
	/// <param name="seconds">The maximum age in seconds. Set to 0 or null for no expiration.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetMaxAge(int? seconds);

	/// <summary>
	/// Sets the maximum age of the invite using a time span.
	/// </summary>
	/// <param name="timeout">The timeout value.</param>
	/// <param name="unit">The time unit (e.g., TimeSpan.FromHours(24)).</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetMaxAge(long? timeout, TimeSpan unit);

	/// <summary>
	/// Sets the maximum number of times the invite can be used.
	/// </summary>
	/// <param name="maxUses">The maximum number of uses. Set to 0 or null for unlimited uses.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetMaxUses(int? maxUses);

	/// <summary>
	/// Sets whether the invite grants temporary membership.
	/// When true, users will be kicked when they log out unless they receive a role.
	/// </summary>
	/// <param name="temporary">Whether the invite should be temporary.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetTemporary(bool? temporary);

	/// <summary>
	/// Sets whether a unique invite code should be generated.
	/// When false, may return an existing invite with similar parameters.
	/// </summary>
	/// <param name="unique">Whether the invite should be unique.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetUnique(bool? unique);

	/// <summary>
	/// Sets the target embedded application for this invite.
	/// This automatically sets the target_type to EMBEDDED_APPLICATION.
	/// </summary>
	/// <param name="applicationId">The ID of the embedded application.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetTargetApplication(ulong applicationId);

	/// <summary>
	/// Sets the target embedded application for this invite.
	/// This automatically sets the target_type to EMBEDDED_APPLICATION.
	/// </summary>
	/// <param name="applicationId">The ID of the embedded application as a string.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetTargetApplication(string applicationId);

	/// <summary>
	/// Sets the target user for a stream invite.
	/// This automatically sets the target_type to STREAM.
	/// </summary>
	/// <param name="userId">The ID of the user whose stream to target.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetTargetStream(ulong userId);

	/// <summary>
	/// Sets the target user for a stream invite.
	/// This automatically sets the target_type to STREAM.
	/// </summary>
	/// <param name="userId">The ID of the user whose stream to target as a string.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetTargetStream(string userId);

	/// <summary>
	/// Sets the target user for a stream invite.
	/// This automatically sets the target_type to STREAM.
	/// </summary>
	/// <param name="user">The user whose stream to target.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetTargetStream(IUser user);

	/// <summary>
	/// Sets the target user for a stream invite.
	/// This automatically sets the target_type to STREAM.
	/// </summary>
	/// <param name="member">The member whose stream to target.</param>
	/// <returns>The current <see cref="ICreateInviteAction"/> instance.</returns>
	ICreateInviteAction SetTargetStream(IMember member);
}