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

    /// <summary>Modifies the current user. Pass <c>null</c> for any field to leave it unchanged.</summary>
    /// <param name="username">If set, the new username.</param>
    /// <param name="avatar">If set, the new avatar as a base64-encoded data URI, or empty string to remove.</param>
    /// <param name="banner">If set, the new banner as a base64-encoded data URI, or empty string to remove.</param>
    IRestAction<IUser> Modify(string? username = null, string? avatar = null, string? banner = null);

    /// <summary>Lists the guilds the bot is a member of. Paginate with <paramref name="before"/> / <paramref name="after"/>.</summary>
    IRestAction<IReadOnlyList<IGuild>> GetGuilds(int? limit = null, Snowflake? before = null, Snowflake? after = null, bool? withCounts = null);

    /// <summary>Returns the bot's member object in a specific guild.</summary>
    IRestAction<IMember> GetGuildMember(Snowflake guildId);

    /// <summary>Lists the bot's third-party account connections.</summary>
    IRestAction<IReadOnlyList<IConnection>> GetConnections();

    /// <summary>Retrieves the bot's role-connection data for the given application (linked roles).</summary>
    IRestAction<IApplicationRoleConnection> GetApplicationRoleConnection(Snowflake applicationId);

    /// <summary>Updates the bot's role-connection data for the given application (linked roles).</summary>
    IRestAction<IApplicationRoleConnection> UpdateApplicationRoleConnection(Snowflake applicationId, ApplicationRoleConnection record);
}
