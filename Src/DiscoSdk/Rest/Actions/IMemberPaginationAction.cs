using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Represents a pagination action for retrieving guild members.
/// </summary>
/// <remarks>
/// Executing this action (via <c>ExecuteAsync</c>) requires the privileged
/// <see cref="DiscordIntent.GuildMembers"/> intent — Discord's <c>List Guild Members</c>
/// endpoint enforces it. Without the intent, the SDK throws
/// <see cref="DiscoSdk.Exceptions.MissingIntentException"/> before sending the request.
/// </remarks>
public interface IMemberPaginationAction : IPaginationAction<IMember, IMemberPaginationAction>
{
	/// <summary>
	/// Gets members after this user ID.
	/// </summary>
	/// <param name="userId">The user ID to get members after.</param>
	/// <returns>The current <see cref="IMemberPaginationAction"/> instance.</returns>
	IMemberPaginationAction After(Snowflake userId);
}
