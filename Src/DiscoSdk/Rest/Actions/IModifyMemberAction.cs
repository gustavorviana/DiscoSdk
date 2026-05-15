using DiscoSdk.Models;

namespace DiscoSdk.Rest.Actions;

/// <summary>
/// Modifies a guild member. Every field is optional — only the knobs you set are sent to Discord.
/// Discord requires different permissions per field (e.g. <c>MANAGE_NICKNAMES</c>,
/// <c>MANAGE_ROLES</c>, <c>MUTE_MEMBERS</c>, <c>MODERATE_MEMBERS</c>); calling the corresponding
/// setter on a token without that permission will surface as a 403 at execute time.
/// </summary>
public interface IModifyMemberAction : IRestAction<IMember>
{
    /// <summary>Sets a new nickname. Pass <c>null</c> to clear it.</summary>
    IModifyMemberAction SetNickname(string? nick);

    /// <summary>Replaces the member's full role list (additive operations live on <c>AddRole</c>/<c>RemoveRole</c>).</summary>
    IModifyMemberAction SetRoles(IEnumerable<Snowflake> roles);

    /// <summary>Sets the voice-mute state. Member must be connected to a voice channel.</summary>
    IModifyMemberAction SetMuted(bool muted);

    /// <summary>Sets the voice-deafen state. Member must be connected to a voice channel.</summary>
    IModifyMemberAction SetDeafened(bool deafened);

    /// <summary>Moves the member to a specific voice channel. Pass <c>null</c> to disconnect.</summary>
    IModifyMemberAction MoveToVoiceChannel(Snowflake? channelId);

    /// <summary>Applies a timeout that auto-expires at the supplied timestamp (max 28 days out).</summary>
    IModifyMemberAction Timeout(DateTimeOffset until);

    /// <summary>Clears any active timeout on the member.</summary>
    IModifyMemberAction ClearTimeout();

    /// <summary>Sets the raw member-flags bitfield. Most callers should prefer the typed setters that wrap individual flags.</summary>
    IModifyMemberAction SetFlags(int flags);
}
