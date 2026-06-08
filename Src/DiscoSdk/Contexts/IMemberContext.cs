using DiscoSdk.Contexts.Guilds;
using DiscoSdk.Models;

namespace DiscoSdk.Contexts;

public interface IMemberContext : IContext, IGuildContext
{
    /// <summary>
    /// The full member when Discord shipped the member object inline with the event, otherwise
    /// <c>null</c>. Reactions and a few other events only carry the user id, in which case
    /// callers can still rely on <see cref="MemberId"/> to identify the actor.
    /// </summary>
    IMember? Member { get; }

    /// <summary>
    /// Identifies the member behind the event even when <see cref="Member"/> is <c>null</c>.
    /// This is the user id Discord delivered on the payload — always present, regardless of
    /// whether the full member object was inlined.
    /// </summary>
    Snowflake MemberId { get; }
}