using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Adds a user to a guild using an OAuth2 access token granted with the <c>guilds.join</c> scope.
/// Configure optional knobs (nickname, roles, mute, deaf) before calling
/// <see cref="IRestAction{T}.ExecuteAsync"/>. The result is the newly-created member, or
/// <c>null</c> if the user was already in the guild (Discord returns 204).
/// </summary>
public interface IAddMemberAction : IRestAction<IMember?>
{
    /// <summary>Sets the initial nickname for the member. Requires <c>MANAGE_NICKNAMES</c>.</summary>
    IAddMemberAction SetNickname(string? nick);

    /// <summary>Sets the initial roles to assign to the member. Requires <c>MANAGE_ROLES</c>.</summary>
    IAddMemberAction SetRoles(IEnumerable<Snowflake>? roles);

    /// <summary>Sets whether the member is server-muted in voice channels. Requires <c>MUTE_MEMBERS</c>.</summary>
    IAddMemberAction SetMuted(bool? muted);

    /// <summary>Sets whether the member is server-deafened in voice channels. Requires <c>DEAFEN_MEMBERS</c>.</summary>
    IAddMemberAction SetDeafened(bool? deafened);
}
