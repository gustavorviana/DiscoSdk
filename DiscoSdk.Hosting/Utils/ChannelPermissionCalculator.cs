using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;

namespace DiscoSdk.Hosting.Utils;

internal class ChannelPermissionCalculator
{
    private readonly DiscordPermission _everyonePermissions;
    private readonly PermissionOverwrite[] _channelOverwrite;
    private readonly IRole[] _guildRoles;

    public ChannelPermissionCalculator(IGuild guild, IPermissionContainer permissionContainer)
    {
        _channelOverwrite = permissionContainer.PermissionOverwrites ?? [];
        _guildRoles = guild.Roles ?? [];

        var everyoneRole = _guildRoles.FirstOrDefault(r => r.Id == guild.Id);
        _everyonePermissions = everyoneRole?.Permissions ?? DiscordPermission.None;
    }

    public DiscordPermission GetPermission(IMember member)
    {
        if (member.IsOwner)
            return DiscordPermission.Administrator;

        var permissions = _everyonePermissions;

        foreach (var role in member.Roles)
            permissions |= role.Permissions;

        if (permissions.HasFlag(DiscordPermission.Administrator))
            return DiscordPermission.Administrator;

        return ApplyMemberSpecificOverwrites(ApplyChannelOverwrites(permissions, member), member);
    }

    private DiscordPermission ApplyChannelOverwrites(DiscordPermission permissions, IMember member)
    {
        // Apply role overwrites (in order of role position, highest first)
        var roleOverwrites = _channelOverwrite
            .Where(ow => ow.Type == PermissionOverwriteType.Role)
            .OrderByDescending(ow =>
            {
                // Find role in guild roles array
                var role = _guildRoles?.FirstOrDefault(r => r.Id == ow.Id);
                return role?.Position ?? 0;
            });

        foreach (var overwrite in roleOverwrites)
        {
            // Only apply if member has this role
            if (member.Roles.Any(r => r.Id == overwrite.Id))
            {
                if (ulong.TryParse(overwrite.Allow, out var allow))
                    permissions |= (DiscordPermission)allow;
                if (ulong.TryParse(overwrite.Deny, out var deny))
                    permissions &= ~(DiscordPermission)deny;
            }
        }

        return permissions;
    }

    private DiscordPermission ApplyMemberSpecificOverwrites(DiscordPermission permissions, IMember member)
    {
        var memberOverwrite = _channelOverwrite.FirstOrDefault(ow => ow.Type == PermissionOverwriteType.Member && ow.Id == member.Id);

        if (memberOverwrite != null)
        {
            if (ulong.TryParse(memberOverwrite.Allow, out var allow))
                permissions |= (DiscordPermission)allow;
            if (ulong.TryParse(memberOverwrite.Deny, out var deny))
                permissions &= ~(DiscordPermission)deny;
        }

        return permissions;
    }
}