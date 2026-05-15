using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Guild-only entry point to the command registration system. Opens a scope bound to a
/// specific guild and a mode. Resolvable from DI at any point in the bot's lifetime —
/// not tied to the startup registration window — so handlers like <c>GuildCreate</c> can
/// register new commands for a guild on the fly.
/// </summary>
public interface IGuildCommandUpdateFactory
{
    /// <summary>
    /// Opens a registration scope for <paramref name="guildId"/>. The returned scope already
    /// knows its target; the caller only chooses what to put in via <c>Add(...)</c> and then
    /// commits with <c>ApplyAsync()</c>.
    /// </summary>
    /// <param name="guildId">The guild to register commands for.</param>
    /// <param name="overwrite">
    /// <c>true</c> = bulk PUT (Discord replaces every command in the guild scope).
    /// <c>false</c> = append/upsert (reads existing first; POST new, PATCH changed, no-op equal; never deletes).
    /// </param>
    ICommandUpdateScope OpenForGuild(Snowflake guildId, bool overwrite);
}
