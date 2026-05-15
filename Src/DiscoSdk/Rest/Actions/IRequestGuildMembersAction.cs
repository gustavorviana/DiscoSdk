using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Fluent builder for the <c>Request Guild Members</c> gateway command. Configure with
/// <c>Set*</c> methods, then pick a terminal operation: <see cref="GetAsync"/> to buffer
/// everything into a list, or <see cref="StreamAsync"/> to consume members one by one as they
/// arrive.
/// </summary>
/// <remarks>
/// Either <see cref="SetQuery"/> or <see cref="SetUserIds"/> must be supplied — or neither, in
/// which case the SDK sends an empty <c>query</c> to fetch every member (requires the
/// privileged <see cref="DiscordIntent.GuildMembers"/> intent). Setting
/// <see cref="SetPresences(bool)"/> additionally requires <see cref="DiscordIntent.GuildPresences"/>.
/// Both checks happen at terminal time and throw
/// <see cref="DiscoSdk.Exceptions.MissingIntentException"/> before anything is sent to Discord.
/// </remarks>
public interface IRequestGuildMembersAction
{
	/// <summary>Username prefix filter (max 32 chars). Mutually exclusive with <see cref="SetUserIds"/>.</summary>
	IRequestGuildMembersAction SetQuery(string query);

	/// <summary>
	/// Caps the number of returned members at <paramref name="limit"/>. Sent on the wire as
	/// <c>limit=N</c>, so Discord stops sending chunks once N is reached.
	/// </summary>
	IRequestGuildMembersAction SetLimit(int limit);

	/// <summary>Includes presence data on each chunk.</summary>
	/// <remarks>
	/// Setting this to <c>true</c> requires the privileged <see cref="DiscordIntent.GuildPresences"/>
	/// intent — the terminal call (<see cref="GetAsync"/> / <see cref="StreamAsync"/>) throws
	/// <see cref="DiscoSdk.Exceptions.MissingIntentException"/> otherwise.
	/// </remarks>
	IRequestGuildMembersAction SetPresences(bool presences);

	/// <summary>
	/// Requests specific user ids (max 100). Clears any query set previously.
	/// </summary>
	IRequestGuildMembersAction SetUserIds(params Snowflake[] userIds);

	/// <summary>
	/// Buffers every chunk into a single list and returns it once Discord finishes streaming.
	/// Convenient when you genuinely need the full set — avoid for very large guilds where memory
	/// matters; prefer <see cref="StreamAsync"/> instead.
	/// </summary>
	/// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
	/// Thrown when the configuration requires an intent that isn't enabled on the client.
	/// See the remarks on <see cref="IRequestGuildMembersAction"/> for the rules.
	/// </exception>
	Task<IReadOnlyList<IMember>> GetAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Yields members one by one as the SDK receives chunks from Discord. The caller drives the
	/// pace via <c>await foreach</c>; breaking early or cancelling the token aborts the pending
	/// request immediately.
	/// </summary>
	/// <exception cref="DiscoSdk.Exceptions.MissingIntentException">
	/// Thrown when the configuration requires an intent that isn't enabled on the client.
	/// See the remarks on <see cref="IRequestGuildMembersAction"/> for the rules.
	/// </exception>
	IAsyncEnumerable<IMember> StreamAsync(CancellationToken cancellationToken = default);
}
