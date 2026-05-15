using DiscoSdk.Models;
using DiscoSdk.Models.Applications;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk;

/// <summary>
/// Public surface for endpoints scoped to the current bot user (the <c>@me</c> namespace in
/// Discord's REST API). Exposed via <see cref="IDiscordClient.Me"/>.
/// </summary>
public interface IMe
{
    /// <summary>Returns the current user (the bot).</summary>
    IRestAction<IUser> Get();

    /// <summary>Builds a REST action to modify the current user. Configure fields on the returned builder.</summary>
    IModifyMeAction Modify();

    /// <summary>
    /// Builds a REST action that lists the guilds the bot is a member of. Configure paging / count
    /// flags on the returned builder before calling <see cref="IRestAction{T}.ExecuteAsync"/>.
    /// </summary>
    IGetCurrentGuildsAction GetGuilds();

    /// <summary>Returns the bot's member object in a specific guild.</summary>
    IRestAction<IMember> GetGuildMember(Snowflake guildId);

    /// <summary>Lists the bot's third-party account connections.</summary>
    IRestAction<IReadOnlyList<IConnection>> GetConnections();

    /// <summary>Retrieves the bot's role-connection data for the given application (linked roles).</summary>
    IRestAction<IApplicationRoleConnection> GetApplicationRoleConnection(Snowflake applicationId);

    /// <summary>Updates the bot's role-connection data for the given application (linked roles).</summary>
    IRestAction<IApplicationRoleConnection> UpdateApplicationRoleConnection(Snowflake applicationId, ApplicationRoleConnection record);
}
