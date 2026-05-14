namespace DiscoSdk.Models.OAuth2;

/// <summary>
/// String constants for Discord's OAuth2 scopes. Strings, not <c>enum</c>, because Discord adds
/// new scopes over time and the literal value goes on the wire verbatim. Reference:
/// https://discord.com/developers/docs/topics/oauth2#shared-resources-oauth2-scopes
/// </summary>
public static class OAuth2Scope
{
    /// <summary>Read user info excluding email — <c>users/@me</c>.</summary>
    public const string Identify = "identify";

    /// <summary>Adds the email to <c>identify</c>.</summary>
    public const string Email = "email";

    /// <summary>Lists the user's guilds — <c>users/@me/guilds</c>.</summary>
    public const string Guilds = "guilds";

    /// <summary>Adds the user to a guild — <c>guilds/{guild.id}/members/{user.id}</c>.</summary>
    public const string GuildsJoin = "guilds.join";

    /// <summary>Reads the user's member list in a guild.</summary>
    public const string GuildsMembersRead = "guilds.members.read";

    /// <summary>Lists the user's third-party account connections.</summary>
    public const string Connections = "connections";

    /// <summary>Adds the bot to a guild (used in install URLs).</summary>
    public const string Bot = "bot";

    /// <summary>Use slash commands as the user.</summary>
    public const string ApplicationsCommands = "applications.commands";

    /// <summary>Update slash commands as the user.</summary>
    public const string ApplicationsCommandsUpdate = "applications.commands.update";

    /// <summary>Update per-guild command permissions (required for the <c>PUT</c> endpoint).</summary>
    public const string ApplicationsCommandsPermissionsUpdate = "applications.commands.permissions.update";

    /// <summary>Read entitlements for the application as the user.</summary>
    public const string ApplicationsEntitlements = "applications.entitlements";

    /// <summary>Build read access for the application.</summary>
    public const string ApplicationsBuildsRead = "applications.builds.read";

    /// <summary>Upload builds for the application.</summary>
    public const string ApplicationsBuildsUpload = "applications.builds.upload";

    /// <summary>Update the application's store.</summary>
    public const string ApplicationsStoreUpdate = "applications.store.update";

    /// <summary>Adds a user to a group DM — paired with <c>access_token</c> in the add-recipient call.</summary>
    public const string GdmJoin = "gdm.join";

    /// <summary>Read DM channels.</summary>
    public const string DmChannelsRead = "dm_channels.read";

    /// <summary>Read messages (special partner-only scope).</summary>
    public const string MessagesRead = "messages.read";

    /// <summary>Write linked-roles metadata for the user.</summary>
    public const string RoleConnectionsWrite = "role_connections.write";

    /// <summary>Read user activities.</summary>
    public const string ActivitiesRead = "activities.read";

    /// <summary>Write user activities (rich presence).</summary>
    public const string ActivitiesWrite = "activities.write";

    /// <summary>Generate an incoming webhook URL inside a channel.</summary>
    public const string WebhookIncoming = "webhook.incoming";

    /// <summary>Voice access (whitelisted apps only).</summary>
    public const string Voice = "voice";

    /// <summary>RPC — whitelisted only.</summary>
    public const string Rpc = "rpc";

    /// <summary>RPC — read notifications.</summary>
    public const string RpcNotificationsRead = "rpc.notifications.read";

    /// <summary>RPC — read voice state.</summary>
    public const string RpcVoiceRead = "rpc.voice.read";

    /// <summary>RPC — write voice state.</summary>
    public const string RpcVoiceWrite = "rpc.voice.write";

    /// <summary>Read relationships (whitelisted only).</summary>
    public const string RelationshipsRead = "relationships.read";
}
