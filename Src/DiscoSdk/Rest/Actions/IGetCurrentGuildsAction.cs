using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Lists the guilds the current user (bot) is a member of. Use the setters to paginate or to
/// include approximate member/presence counts in the response.
/// </summary>
public interface IGetCurrentGuildsAction : IRestAction<IReadOnlyList<IGuild>>
{
    /// <summary>Caps the page size (1-200). Discord defaults to 200.</summary>
    IGetCurrentGuildsAction SetLimit(int limit);

    /// <summary>Returns guilds with ids less than this value.</summary>
    IGetCurrentGuildsAction Before(Snowflake guildId);

    /// <summary>Returns guilds with ids greater than this value.</summary>
    IGetCurrentGuildsAction After(Snowflake guildId);

    /// <summary>Asks Discord to populate approximate member/presence counts on each entry.</summary>
    IGetCurrentGuildsAction WithCounts();
}
