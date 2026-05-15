using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Resolves an invite by its code. The result is <c>null</c> when Discord returns 404. Use the
/// setters to ask Discord for richer metadata (member/online counts, expiration timestamp, scheduled
/// event embed).
/// </summary>
public interface IGetInviteAction : IRestAction<IInvite?>
{
    /// <summary>Asks Discord to include approximate member / online counts on the invite.</summary>
    IGetInviteAction WithCounts();

    /// <summary>Asks Discord to include the invite's expiration timestamp.</summary>
    IGetInviteAction WithExpiration();

    /// <summary>Embeds the specified scheduled event on the resolved invite (for event invites).</summary>
    IGetInviteAction WithScheduledEvent(Snowflake guildScheduledEventId);
}
