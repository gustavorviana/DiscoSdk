using DiscoSdk.Models;
using DiscoSdk.Models.Channels;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;

namespace DiscoSdk.Hosting.Rest.Actions;

/// <summary>
/// Implementation of <see cref="IOverrideIPermissionAction"/> for upserting permission overrides.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OverridePermissionAction"/> class.
/// </remarks>
/// <param name="client">The Discord client.</param>
/// <param name="channelId">The ID of the channel.</param>
/// <param name="holderId">The ID of the permission holder (user or role).</param>
internal class OverridePermissionAction(DiscordClient client, Snowflake channelId, Snowflake holderId) : RestAction<PermissionOverride>, IOverrideIPermissionAction
{
	private readonly DiscordClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private DiscordPermission? _allow;
	private DiscordPermission? _deny;

    /// <inheritdoc />
    public IOverrideIPermissionAction SetAllow(DiscordPermission permissions)
	{
		_allow = permissions;
		return this;
	}

	/// <inheritdoc />
	public IOverrideIPermissionAction SetDeny(DiscordPermission permissions)
	{
		_deny = permissions;
		return this;
	}

	/// <inheritdoc />
	public override async Task<PermissionOverride> ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var request = new
		{
			id = holderId.ToString(),
			type = 0, // Will need to determine if this is a role (0) or member (1)
			allow = _allow?.ToString() ?? "0",
			deny = _deny?.ToString() ?? "0"
		};

		var channel = await _client.ChannelClient.EditAsync(channelId, request, cancellationToken);
		
		// Find the permission override in the updated channel
		var overwrite = channel.PermissionOverwrites?.FirstOrDefault(po => po.Id == holderId);
        return overwrite == null
            ? throw new InvalidOperationException("Permission override was not found after update.")
            : new PermissionOverride(overwrite);
    }
}